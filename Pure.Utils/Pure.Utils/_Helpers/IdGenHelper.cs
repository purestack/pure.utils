using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Pure.Utils
{
    public class IdGenerateManager:Singleton<IdGenerateManager>
    {
        /// <summary>
        /// 随机数生成器
        /// </summary>
        private  readonly RandomNumberGenerator Rng = RandomNumberGenerator.Create();
        public  string CreateDefaulGuidId() {
            return Guid.NewGuid().ToString();
        }
        private   Snowflake _snowflake;
        /// <summary>
        /// 雪花算法ID
        /// </summary>
        private   Snowflake _Snowflake {
            get
            {
                if (_snowflake == null)
                {
                    _snowflake = new Snowflake(1, 1);
                }
                return _snowflake;
            }
        }

        private  TimestampId _timestampId;

        private  TimestampId _TimestampId
        {
            get
            {
                if (_timestampId == null)
                {
                    _timestampId = TimestampId.GetInstance();
                }
                return _timestampId;
            }
        }
       
        public  long CreateSnowflakeId()
        {
            return _Snowflake.NextId();
        }
        public  string CreateObjectId()
        {
            return ObjectId.GenerateNewStringId();
        }
        public  string CreateMongoId()
        {
            return MongoId.GenerateNewId().ToString();
        }
        public  string CreateTimestampId()
        {
            return _TimestampId.GetId();
        }
        /// <summary>
        /// 创建有序的 Guid
        /// </summary>
        /// <param name="databaseType">数据库类型</param>
        /// <returns></returns>
        public  Guid CreateSequentialGuid(SequentialGuidDatabaseType databaseType)
        {
            switch (databaseType)
            {
                case SequentialGuidDatabaseType.SqlServer:
                    return CreateSequentialGuid(SequentialGuidType.SequentialAtEnd);
                case SequentialGuidDatabaseType.Oracle:
                    return CreateSequentialGuid(SequentialGuidType.SequentialAsBinary);
                case SequentialGuidDatabaseType.MySql:
                    return CreateSequentialGuid(SequentialGuidType.SequentialAsString);
                case SequentialGuidDatabaseType.PostgreSql:
                    return CreateSequentialGuid(SequentialGuidType.SequentialAsString);
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// 创建有序的 Guid
        /// </summary>
        /// <param name="guidType">有序 Guid 类型</param>
        /// <returns></returns>
        public  Guid CreateSequentialGuid(SequentialGuidType guidType)
        {
            // We start with 16 bytes of cryptographically strong random data.
            var randomBytes = new byte[10];
            lock (Rng)
            {
                Rng.GetBytes(randomBytes);
            }

            // An alternate method: use a normally-created GUID to get our initial
            // random data:
            // byte[] randomBytes = Guid.NewGuid().ToByteArray();
            // This is faster than using RNGCryptoServiceProvider, but I don't
            // recommend it because the .NET Framework makes no guarantee of the
            // randomness of GUID data, and future versions (or different
            // implementations like Mono) might use a different method.

            // Now we have the random basis for our GUID.  Next, we need to
            // create the six-byte block which will be our timestamp.

            // We start with the number of milliseconds that have elapsed since
            // DateTime.MinValue.  This will form the timestamp.  There's no use
            // being more specific than milliseconds, since DateTime.Now has
            // limited resolution.

            // Using millisecond resolution for our 48-bit timestamp gives us
            // about 5900 years before the timestamp overflows and cycles.
            // Hopefully this should be sufficient for most purposes. :)
            long timestamp = DateTime.UtcNow.Ticks / 10000L;

            // Then get the bytes
            byte[] timestampBytes = BitConverter.GetBytes(timestamp);

            // Since we're converting from an Int64, we have to reverse on
            // little-endian systems.
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(timestampBytes);
            }

            byte[] guidBytes = new byte[16];

            switch (guidType)
            {
                case SequentialGuidType.SequentialAsString:
                case SequentialGuidType.SequentialAsBinary:

                    // For string and byte-array version, we copy the timestamp first, followed
                    // by the random data.
                    Buffer.BlockCopy(timestampBytes, 2, guidBytes, 0, 6);
                    Buffer.BlockCopy(randomBytes, 0, guidBytes, 6, 10);

                    // If formatting as a string, we have to compensate for the fact
                    // that .NET regards the Data1 and Data2 block as an Int32 and an Int16,
                    // respectively.  That means that it switches the order on little-endian
                    // systems.  So again, we have to reverse.
                    if (guidType == SequentialGuidType.SequentialAsString && BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(guidBytes, 0, 4);
                        Array.Reverse(guidBytes, 4, 2);
                    }

                    break;

                case SequentialGuidType.SequentialAtEnd:

                    // For sequential-at-the-end versions, we copy the random data first,
                    // followed by the timestamp.
                    Buffer.BlockCopy(randomBytes, 0, guidBytes, 0, 10);
                    Buffer.BlockCopy(timestampBytes, 2, guidBytes, 10, 6);
                    break;
            }

            return new Guid(guidBytes);
        }
    }


    /// <summary>
    /// 有序Guid数据库类型
    /// </summary>
    public enum SequentialGuidDatabaseType
    {
        /// <summary>
        /// SqlServer
        /// </summary>
        SqlServer,
        /// <summary>
        /// Oracle
        /// </summary>
        Oracle,
        /// <summary>
        /// MySql
        /// </summary>
        MySql,
        /// <summary>
        /// PostgreSql
        /// </summary>
        PostgreSql
    }

    /// <summary>
    /// 有序Guid类型
    /// </summary>
    public enum SequentialGuidType
    {
        /// <summary>
        /// 生成的GUID 按照字符串顺序排列
        /// </summary>
        SequentialAsString,
        /// <summary>
        /// 生成的GUID 按照二进制的顺序排列
        /// </summary>
        SequentialAsBinary,
        /// <summary>
        /// 生成的GUID 像SQL Server, 按照末尾部分排列
        /// </summary>
        SequentialAtEnd
    }
    /// <summary>
    /// 时间戳ID，借鉴雪花算法，生成唯一时间戳ID
    /// 参考文章：http://www.cnblogs.com/rjf1979/p/6282855.html
    /// </summary>
    public class TimestampId
    {
        private long _lastTimestamp;
        private long _sequence;//计数从零开始
        private readonly DateTime? _initialDateTime;
        private static TimestampId _timestampId;
        private const int MAX_END_NUMBER = 9999;

        private TimestampId(DateTime? initialDateTime)
        {
            _initialDateTime = initialDateTime;
        }

        /// <summary>
        /// 获取单个实例对象
        /// </summary>
        /// <param name="initialDateTime">初始化时间，与当前时间做一个相差取时间戳</param>
        /// <returns></returns>
        public static TimestampId GetInstance(DateTime? initialDateTime = null)
        {
            if (!initialDateTime.HasValue || initialDateTime == null)
            {
                Interlocked.CompareExchange(ref _timestampId, new TimestampId(initialDateTime), null);
            }
            return _timestampId;
        }

        /// <summary>
        /// 初始化时间，作用时间戳的相差
        /// </summary>
        protected DateTime InitialDateTime
        {
            get
            {
                if (_initialDateTime == null || _initialDateTime.Value == DateTime.MinValue)
                {
                    return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                }
                return _initialDateTime.Value;
            }
        }

        /// <summary>
        /// 获取唯一时间戳ID
        /// </summary>
        /// <returns></returns>
        public string GetId()
        {
            long temp;
            var timestamp = GetUniqueTimeStamp(_lastTimestamp, out temp);
            return $"{timestamp}{Fill(temp)}";
        }

        /// <summary>
        /// 补位填充
        /// </summary>
        /// <param name="temp">数字</param>
        /// <returns></returns>
        private string Fill(long temp)
        {
            var num = temp.ToString();
            IList<char> chars = new List<char>();
            for (int i = 0; i < MAX_END_NUMBER.ToString().Length - num.Length; i++)
            {
                chars.Add('0');
            }
            return new string(chars.ToArray()) + num;
        }

        /// <summary>
        /// 获取唯一时间戳
        /// </summary>
        /// <param name="lastTimeStamp">最后时间戳</param>
        /// <param name="temp">临时时间戳</param>
        /// <returns></returns>
        public long GetUniqueTimeStamp(long lastTimeStamp, out long temp)
        {
            lock (this)
            {
                temp = 1;
                var timeStamp = GetTimeStamp();
                if (timeStamp == _lastTimestamp)
                {
                    _sequence = _sequence + 1;
                    temp = _sequence;
                    if (temp >= MAX_END_NUMBER)
                    {
                        timeStamp = GetTimeStamp();
                        _lastTimestamp = timeStamp;
                        temp = _sequence = 1;
                    }
                }
                else
                {
                    _sequence = 1;
                    _lastTimestamp = timeStamp;
                }
                return timeStamp;
            }
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        private long GetTimeStamp()
        {
            if (InitialDateTime >= DateTime.Now)
            {
                throw new Exception("初始化时间比当前时间还大，不合理");
            }
            var ts = DateTime.UtcNow - InitialDateTime;
            return (long)ts.TotalMilliseconds;
        }
    }

    /// <summary>
    /// MongoId is a 12-byte Id consists of:
    /// a 4-byte value representing the seconds since the Unix epoch,
    /// a 3-byte machine identifier,
    /// a 2-byte process id, and
    /// a 3-byte counter, starting with a random value.
    /// </summary>
    [Serializable]
    public struct MongoId : IComparable<MongoId>, IEquatable<MongoId>
    {
        private static MongoId __emptyInstance = default(MongoId);
        private static int __staticMachine = (GetMachineHash() + AppDomain.CurrentDomain.Id) & 0x00ffffff;
        private static short __staticPid = GetPid();
        private static int __staticIncrement = (new Random()).Next();

        private int _a;
        private int _b;
        private int _c;

        public MongoId(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (bytes.Length != 12)
            {
                throw new ArgumentException("Byte array must be 12 bytes long", "bytes");
            }

            FromByteArray(bytes, 0, out _a, out _b, out _c);
        }

        public MongoId(byte[] bytes, int offset)
        {
            FromByteArray(bytes, offset, out _a, out _b, out _c);
        }

        public MongoId(DateTime timestamp, int machine, short pid, int increment)
            : this(MongoIdTimer.GetTimestampFromDateTime(timestamp), machine, pid, increment)
        {
        }

        public MongoId(int timestamp, int machine, short pid, int increment)
        {
            if ((machine & 0xff000000) != 0)
            {
                throw new ArgumentOutOfRangeException("machine", "The machine value must be between 0 and 16777215 (it must fit in 3 bytes).");
            }
            if ((increment & 0xff000000) != 0)
            {
                throw new ArgumentOutOfRangeException("increment", "The increment value must be between 0 and 16777215 (it must fit in 3 bytes).");
            }

            _a = timestamp;
            _b = (machine << 8) | (((int)pid >> 8) & 0xff);
            _c = ((int)pid << 24) | increment;
        }

        public MongoId(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            var bytes = MongoIdHexer.ParseHexString(value);
            FromByteArray(bytes, 0, out _a, out _b, out _c);
        }

        public static MongoId Empty
        {
            get { return __emptyInstance; }
        }

        public int Timestamp
        {
            get { return _a; }
        }

        public int Machine
        {
            get { return (_b >> 8) & 0xffffff; }
        }

        public short Pid
        {
            get { return (short)(((_b << 8) & 0xff00) | ((_c >> 24) & 0x00ff)); }
        }

        public int Increment
        {
            get { return _c & 0xffffff; }
        }

        public DateTime CreationTime
        {
            get { return MongoIdTimer.UnixEpoch.AddSeconds(Timestamp); }
        }

        public static bool operator <(MongoId left, MongoId right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(MongoId left, MongoId right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator ==(MongoId left, MongoId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MongoId left, MongoId right)
        {
            return !(left == right);
        }

        public static bool operator >=(MongoId left, MongoId right)
        {
            return left.CompareTo(right) >= 0;
        }

        public static bool operator >(MongoId left, MongoId right)
        {
            return left.CompareTo(right) > 0;
        }

        public static MongoId GenerateNewId()
        {
            return GenerateNewId(MongoIdTimer.GetTimestampFromDateTime(DateTime.UtcNow));
        }

        public static MongoId GenerateNewId(DateTime timestamp)
        {
            return GenerateNewId(MongoIdTimer.GetTimestampFromDateTime(timestamp));
        }

        public static MongoId GenerateNewId(int timestamp)
        {
            int increment = Interlocked.Increment(ref __staticIncrement) & 0x00ffffff; // only use low order 3 bytes
            return new MongoId(timestamp, __staticMachine, __staticPid, increment);
        }

        public static byte[] Pack(int timestamp, int machine, short pid, int increment)
        {
            if ((machine & 0xff000000) != 0)
            {
                throw new ArgumentOutOfRangeException("machine", "The machine value must be between 0 and 16777215 (it must fit in 3 bytes).");
            }
            if ((increment & 0xff000000) != 0)
            {
                throw new ArgumentOutOfRangeException("increment", "The increment value must be between 0 and 16777215 (it must fit in 3 bytes).");
            }

            byte[] bytes = new byte[12];
            bytes[0] = (byte)(timestamp >> 24);
            bytes[1] = (byte)(timestamp >> 16);
            bytes[2] = (byte)(timestamp >> 8);
            bytes[3] = (byte)(timestamp);
            bytes[4] = (byte)(machine >> 16);
            bytes[5] = (byte)(machine >> 8);
            bytes[6] = (byte)(machine);
            bytes[7] = (byte)(pid >> 8);
            bytes[8] = (byte)(pid);
            bytes[9] = (byte)(increment >> 16);
            bytes[10] = (byte)(increment >> 8);
            bytes[11] = (byte)(increment);
            return bytes;
        }

        public static void Unpack(byte[] bytes, out int timestamp, out int machine, out short pid, out int increment)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (bytes.Length != 12)
            {
                throw new ArgumentOutOfRangeException("bytes", "Byte array must be 12 bytes long.");
            }

            timestamp = (bytes[0] << 24) + (bytes[1] << 16) + (bytes[2] << 8) + bytes[3];
            machine = (bytes[4] << 16) + (bytes[5] << 8) + bytes[6];
            pid = (short)((bytes[7] << 8) + bytes[8]);
            increment = (bytes[9] << 16) + (bytes[10] << 8) + bytes[11];
        }

        public static MongoId Parse(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }

            MongoId objectId;
            if (TryParse(s, out objectId))
            {
                return objectId;
            }
            else
            {
                var message = string.Format("'{0}' is not a valid 24 digit hex string.", s);
                throw new FormatException(message);
            }
        }

        public static bool TryParse(string s, out MongoId objectId)
        {
            // don't throw ArgumentNullException if s is null
            if (s != null && s.Length == 24)
            {
                byte[] bytes;
                if (MongoIdHexer.TryParseHexString(s, out bytes))
                {
                    objectId = new MongoId(bytes);
                    return true;
                }
            }

            objectId = default(MongoId);
            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int GetCurrentProcessId()
        {
            return Process.GetCurrentProcess().Id;
        }

        private static int GetMachineHash()
        {
            var hostName = Environment.MachineName; // use instead of Dns.HostName so it will work offline
            return 0x00ffffff & hostName.GetHashCode(); // use first 3 bytes of hash
        }

        private static short GetPid()
        {
            try
            {
                return (short)GetCurrentProcessId(); // use low order two bytes only
            }
            catch (SecurityException)
            {
                return 0;
            }
        }

        private static void FromByteArray(byte[] bytes, int offset, out int a, out int b, out int c)
        {
            a = (bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];
            b = (bytes[offset + 4] << 24) | (bytes[offset + 5] << 16) | (bytes[offset + 6] << 8) | bytes[offset + 7];
            c = (bytes[offset + 8] << 24) | (bytes[offset + 9] << 16) | (bytes[offset + 10] << 8) | bytes[offset + 11];
        }

        public int CompareTo(MongoId other)
        {
            int result = ((uint)_a).CompareTo((uint)other._a);
            if (result != 0) { return result; }
            result = ((uint)_b).CompareTo((uint)other._b);
            if (result != 0) { return result; }
            return ((uint)_c).CompareTo((uint)other._c);
        }

        public bool Equals(MongoId other)
        {
            return
                _a == other._a &&
                _b == other._b &&
                _c == other._c;
        }

        public override bool Equals(object obj)
        {
            if (obj is MongoId)
            {
                return Equals((MongoId)obj);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = 37 * hash + _a.GetHashCode();
            hash = 37 * hash + _b.GetHashCode();
            hash = 37 * hash + _c.GetHashCode();
            return hash;
        }

        public byte[] ToByteArray()
        {
            var bytes = new byte[12];
            ToByteArray(bytes, 0);
            return bytes;
        }

        public void ToByteArray(byte[] destination, int offset)
        {
            if (destination == null)
            {
                throw new ArgumentNullException("destination");
            }
            if (offset + 12 > destination.Length)
            {
                throw new ArgumentException("Not enough room in destination buffer.", "offset");
            }

            destination[offset + 0] = (byte)(_a >> 24);
            destination[offset + 1] = (byte)(_a >> 16);
            destination[offset + 2] = (byte)(_a >> 8);
            destination[offset + 3] = (byte)(_a);
            destination[offset + 4] = (byte)(_b >> 24);
            destination[offset + 5] = (byte)(_b >> 16);
            destination[offset + 6] = (byte)(_b >> 8);
            destination[offset + 7] = (byte)(_b);
            destination[offset + 8] = (byte)(_c >> 24);
            destination[offset + 9] = (byte)(_c >> 16);
            destination[offset + 10] = (byte)(_c >> 8);
            destination[offset + 11] = (byte)(_c);
        }

        public override string ToString()
        {
            return MongoIdHexer.ToHexString(ToByteArray());
        }

        internal static class MongoIdTimer
        {
            private static readonly DateTime __unixEpoch;
            private static readonly long __dateTimeMaxValueMillisecondsSinceEpoch;
            private static readonly long __dateTimeMinValueMillisecondsSinceEpoch;

            static MongoIdTimer()
            {
                __unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                __dateTimeMaxValueMillisecondsSinceEpoch = (DateTime.MaxValue - __unixEpoch).Ticks / 10000;
                __dateTimeMinValueMillisecondsSinceEpoch = (DateTime.MinValue - __unixEpoch).Ticks / 10000;
            }

            public static long DateTimeMaxValueMillisecondsSinceEpoch
            {
                get { return __dateTimeMaxValueMillisecondsSinceEpoch; }
            }

            public static long DateTimeMinValueMillisecondsSinceEpoch
            {
                get { return __dateTimeMinValueMillisecondsSinceEpoch; }
            }

            public static DateTime UnixEpoch { get { return __unixEpoch; } }

            public static DateTime ToUniversalTime(DateTime dateTime)
            {
                if (dateTime == DateTime.MinValue)
                {
                    return DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
                }
                else if (dateTime == DateTime.MaxValue)
                {
                    return DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);
                }
                else
                {
                    return dateTime.ToUniversalTime();
                }
            }

            public static int GetTimestampFromDateTime(DateTime timestamp)
            {
                var secondsSinceEpoch = (long)Math.Floor((ToUniversalTime(timestamp) - UnixEpoch).TotalSeconds);
                if (secondsSinceEpoch < int.MinValue || secondsSinceEpoch > int.MaxValue)
                {
                    throw new ArgumentOutOfRangeException("timestamp");
                }
                return (int)secondsSinceEpoch;
            }
        }

        internal static class MongoIdHexer
        {
            public static string ToHexString(byte[] bytes)
            {
                if (bytes == null)
                {
                    throw new ArgumentNullException("bytes");
                }
                var sb = new StringBuilder(bytes.Length * 2);
                foreach (var b in bytes)
                {
                    sb.AppendFormat("{0:x2}", b);
                }
                return sb.ToString();
            }

            public static bool TryParseHexString(string s, out byte[] bytes)
            {
                try
                {
                    bytes = ParseHexString(s);
                }
                catch
                {
                    bytes = null;
                    return false;
                }

                return true;
            }

            public static byte[] ParseHexString(string s)
            {
                if (s == null)
                {
                    throw new ArgumentNullException("s");
                }

                byte[] bytes;
                if ((s.Length & 1) != 0)
                {
                    s = "0" + s; // make length of s even
                }
                bytes = new byte[s.Length / 2];
                for (int i = 0; i < bytes.Length; i++)
                {
                    string hex = s.Substring(2 * i, 2);
                    try
                    {
                        byte b = Convert.ToByte(hex, 16);
                        bytes[i] = b;
                    }
                    catch (FormatException e)
                    {
                        throw new FormatException(
                            string.Format("Invalid hex string {0}. Problem with substring {1} starting at position {2}",
                            s,
                            hex,
                            2 * i),
                            e);
                    }
                }

                return bytes;
            }
        }
    }

    /// <summary>
    /// Id生成器，代码出自：https://github.com/tangxuehua/ecommon/blob/master/src/ECommon/Utilities/ObjectId.cs
    /// </summary>
    public struct ObjectId : IComparable<ObjectId>, IEquatable<ObjectId>
    {
        // private static fields
        private static readonly DateTime __unixEpoch;
        private static readonly long __dateTimeMaxValueMillisecondsSinceEpoch;
        private static readonly long __dateTimeMinValueMillisecondsSinceEpoch;
        private static ObjectId __emptyInstance = default(ObjectId);
        private static int __staticMachine;
        private static short __staticPid;
        private static int __staticIncrement; // high byte will be masked out when generating new ObjectId
        private static uint[] _lookup32 = Enumerable.Range(0, 256).Select(i => {
            string s = i.ToString("x2");
            return ((uint)s[0]) + ((uint)s[1] << 16);
        }).ToArray();

        // we're using 14 bytes instead of 12 to hold the ObjectId in memory but unlike a byte[] there is no additional object on the heap
        // the extra two bytes are not visible to anyone outside of this class and they buy us considerable simplification
        // an additional advantage of this representation is that it will serialize to JSON without any 64 bit overflow problems
        private int _timestamp;
        private int _machine;
        private short _pid;
        private int _increment;

        // static constructor
        static ObjectId()
        {
            __unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            __dateTimeMaxValueMillisecondsSinceEpoch = (DateTime.MaxValue - __unixEpoch).Ticks / 10000;
            __dateTimeMinValueMillisecondsSinceEpoch = (DateTime.MinValue - __unixEpoch).Ticks / 10000;
            __staticMachine = GetMachineHash();
            __staticIncrement = (new System.Random()).Next();
            __staticPid = (short)GetCurrentProcessId();
        }

        // constructors
        /// <summary>
        /// Initializes a new instance of the ObjectId class.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        public ObjectId(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            Unpack(bytes, out _timestamp, out _machine, out _pid, out _increment);
        }

        /// <summary>
        /// Initializes a new instance of the ObjectId class.
        /// </summary>
        /// <param name="timestamp">The timestamp (expressed as a DateTime).</param>
        /// <param name="machine">The machine hash.</param>
        /// <param name="pid">The PID.</param>
        /// <param name="increment">The increment.</param>
        public ObjectId(DateTime timestamp, int machine, short pid, int increment)
            : this(GetTimestampFromDateTime(timestamp), machine, pid, increment)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ObjectId class.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="machine">The machine hash.</param>
        /// <param name="pid">The PID.</param>
        /// <param name="increment">The increment.</param>
        public ObjectId(int timestamp, int machine, short pid, int increment)
        {
            if ((machine & 0xff000000) != 0)
            {
                throw new ArgumentOutOfRangeException("machine", "The machine value must be between 0 and 16777215 (it must fit in 3 bytes).");
            }
            if ((increment & 0xff000000) != 0)
            {
                throw new ArgumentOutOfRangeException("increment", "The increment value must be between 0 and 16777215 (it must fit in 3 bytes).");
            }

            _timestamp = timestamp;
            _machine = machine;
            _pid = pid;
            _increment = increment;
        }

        /// <summary>
        /// Initializes a new instance of the ObjectId class.
        /// </summary>
        /// <param name="value">The value.</param>
        public ObjectId(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            Unpack(ParseHexString(value), out _timestamp, out _machine, out _pid, out _increment);
        }

        // public static properties
        /// <summary>
        /// Gets an instance of ObjectId where the value is empty.
        /// </summary>
        public static ObjectId Empty
        {
            get { return __emptyInstance; }
        }

        // public properties
        /// <summary>
        /// Gets the timestamp.
        /// </summary>
        public int Timestamp
        {
            get { return _timestamp; }
        }

        /// <summary>
        /// Gets the machine.
        /// </summary>
        public int Machine
        {
            get { return _machine; }
        }

        /// <summary>
        /// Gets the PID.
        /// </summary>
        public short Pid
        {
            get { return _pid; }
        }

        /// <summary>
        /// Gets the increment.
        /// </summary>
        public int Increment
        {
            get { return _increment; }
        }

        /// <summary>
        /// Gets the creation time (derived from the timestamp).
        /// </summary>
        public DateTime CreationTime
        {
            get { return __unixEpoch.AddSeconds(_timestamp); }
        }

        // public operators
        /// <summary>
        /// Compares two ObjectIds.
        /// </summary>
        /// <param name="lhs">The first ObjectId.</param>
        /// <param name="rhs">The other ObjectId</param>
        /// <returns>True if the first ObjectId is less than the second ObjectId.</returns>
        public static bool operator <(ObjectId lhs, ObjectId rhs)
        {
            return lhs.CompareTo(rhs) < 0;
        }

        /// <summary>
        /// Compares two ObjectIds.
        /// </summary>
        /// <param name="lhs">The first ObjectId.</param>
        /// <param name="rhs">The other ObjectId</param>
        /// <returns>True if the first ObjectId is less than or equal to the second ObjectId.</returns>
        public static bool operator <=(ObjectId lhs, ObjectId rhs)
        {
            return lhs.CompareTo(rhs) <= 0;
        }

        /// <summary>
        /// Compares two ObjectIds.
        /// </summary>
        /// <param name="lhs">The first ObjectId.</param>
        /// <param name="rhs">The other ObjectId.</param>
        /// <returns>True if the two ObjectIds are equal.</returns>
        public static bool operator ==(ObjectId lhs, ObjectId rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Compares two ObjectIds.
        /// </summary>
        /// <param name="lhs">The first ObjectId.</param>
        /// <param name="rhs">The other ObjectId.</param>
        /// <returns>True if the two ObjectIds are not equal.</returns>
        public static bool operator !=(ObjectId lhs, ObjectId rhs)
        {
            return !(lhs == rhs);
        }

        /// <summary>
        /// Compares two ObjectIds.
        /// </summary>
        /// <param name="lhs">The first ObjectId.</param>
        /// <param name="rhs">The other ObjectId</param>
        /// <returns>True if the first ObjectId is greather than or equal to the second ObjectId.</returns>
        public static bool operator >=(ObjectId lhs, ObjectId rhs)
        {
            return lhs.CompareTo(rhs) >= 0;
        }

        /// <summary>
        /// Compares two ObjectIds.
        /// </summary>
        /// <param name="lhs">The first ObjectId.</param>
        /// <param name="rhs">The other ObjectId</param>
        /// <returns>True if the first ObjectId is greather than the second ObjectId.</returns>
        public static bool operator >(ObjectId lhs, ObjectId rhs)
        {
            return lhs.CompareTo(rhs) > 0;
        }

        // public static methods
        /// <summary>
        /// Generates a new ObjectId with a unique value.
        /// </summary>
        /// <returns>An ObjectId.</returns>
        public static ObjectId GenerateNewId()
        {
            return GenerateNewId(GetTimestampFromDateTime(DateTime.UtcNow));
        }

        /// <summary>
        /// Generates a new ObjectId with a unique value (with the timestamp component based on a given DateTime).
        /// </summary>
        /// <param name="timestamp">The timestamp component (expressed as a DateTime).</param>
        /// <returns>An ObjectId.</returns>
        public static ObjectId GenerateNewId(DateTime timestamp)
        {
            return GenerateNewId(GetTimestampFromDateTime(timestamp));
        }

        /// <summary>
        /// Generates a new ObjectId with a unique value (with the given timestamp).
        /// </summary>
        /// <param name="timestamp">The timestamp component.</param>
        /// <returns>An ObjectId.</returns>
        public static ObjectId GenerateNewId(int timestamp)
        {
            int increment = Interlocked.Increment(ref __staticIncrement) & 0x00ffffff; // only use low order 3 bytes
            return new ObjectId(timestamp, __staticMachine, __staticPid, increment);
        }

        /// <summary>
        /// Generates a new ObjectId string with a unique value.
        /// </summary>
        /// <returns>The string value of the new generated ObjectId.</returns>
        public static string GenerateNewStringId()
        {
            return GenerateNewId().ToString();
        }

        /// <summary>
        /// Packs the components of an ObjectId into a byte array.
        /// </summary>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="machine">The machine hash.</param>
        /// <param name="pid">The PID.</param>
        /// <param name="increment">The increment.</param>
        /// <returns>A byte array.</returns>
        public static byte[] Pack(int timestamp, int machine, short pid, int increment)
        {
            if ((machine & 0xff000000) != 0)
            {
                throw new ArgumentOutOfRangeException("machine", "The machine value must be between 0 and 16777215 (it must fit in 3 bytes).");
            }
            if ((increment & 0xff000000) != 0)
            {
                throw new ArgumentOutOfRangeException("increment", "The increment value must be between 0 and 16777215 (it must fit in 3 bytes).");
            }

            byte[] bytes = new byte[12];
            bytes[0] = (byte)(timestamp >> 24);
            bytes[1] = (byte)(timestamp >> 16);
            bytes[2] = (byte)(timestamp >> 8);
            bytes[3] = (byte)(timestamp);
            bytes[4] = (byte)(machine >> 16);
            bytes[5] = (byte)(machine >> 8);
            bytes[6] = (byte)(machine);
            bytes[7] = (byte)(pid >> 8);
            bytes[8] = (byte)(pid);
            bytes[9] = (byte)(increment >> 16);
            bytes[10] = (byte)(increment >> 8);
            bytes[11] = (byte)(increment);
            return bytes;
        }

        /// <summary>
        /// Parses a string and creates a new ObjectId.
        /// </summary>
        /// <param name="s">The string value.</param>
        /// <returns>A ObjectId.</returns>
        public static ObjectId Parse(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            if (s.Length != 24)
            {
                throw new ArgumentOutOfRangeException("s", "ObjectId string value must be 24 characters.");
            }
            return new ObjectId(ParseHexString(s));
        }

        /// <summary>
        /// Unpacks a byte array into the components of an ObjectId.
        /// </summary>
        /// <param name="bytes">A byte array.</param>
        /// <param name="timestamp">The timestamp.</param>
        /// <param name="machine">The machine hash.</param>
        /// <param name="pid">The PID.</param>
        /// <param name="increment">The increment.</param>
        public static void Unpack(byte[] bytes, out int timestamp, out int machine, out short pid, out int increment)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            if (bytes.Length != 12)
            {
                throw new ArgumentOutOfRangeException("bytes", "Byte array must be 12 bytes long.");
            }
            timestamp = (bytes[0] << 24) + (bytes[1] << 16) + (bytes[2] << 8) + bytes[3];
            machine = (bytes[4] << 16) + (bytes[5] << 8) + bytes[6];
            pid = (short)((bytes[7] << 8) + bytes[8]);
            increment = (bytes[9] << 16) + (bytes[10] << 8) + bytes[11];
        }

        // private static methods
        /// <summary>
        /// Gets the current process id.  This method exists because of how CAS operates on the call stack, checking
        /// for permissions before executing the method.  Hence, if we inlined this call, the calling method would not execute
        /// before throwing an exception requiring the try/catch at an even higher level that we don't necessarily control.
        /// </summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int GetCurrentProcessId()
        {
            return Process.GetCurrentProcess().Id;
        }

        private static int GetMachineHash()
        {
            var hostName = Environment.MachineName; // use instead of Dns.HostName so it will work offline
            var md5 = MD5.Create();
            var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(hostName));
            return (hash[0] << 16) + (hash[1] << 8) + hash[2]; // use first 3 bytes of hash
        }

        private static int GetTimestampFromDateTime(DateTime timestamp)
        {
            return (int)Math.Floor((ToUniversalTime(timestamp) - __unixEpoch).TotalSeconds);
        }

        // public methods
        /// <summary>
        /// Compares this ObjectId to another ObjectId.
        /// </summary>
        /// <param name="other">The other ObjectId.</param>
        /// <returns>A 32-bit signed integer that indicates whether this ObjectId is less than, equal to, or greather than the other.</returns>
        public int CompareTo(ObjectId other)
        {
            int r = _timestamp.CompareTo(other._timestamp);
            if (r != 0) { return r; }
            r = _machine.CompareTo(other._machine);
            if (r != 0) { return r; }
            r = _pid.CompareTo(other._pid);
            if (r != 0) { return r; }
            return _increment.CompareTo(other._increment);
        }

        /// <summary>
        /// Compares this ObjectId to another ObjectId.
        /// </summary>
        /// <param name="rhs">The other ObjectId.</param>
        /// <returns>True if the two ObjectIds are equal.</returns>
        public bool Equals(ObjectId rhs)
        {
            return
                _timestamp == rhs._timestamp &&
                _machine == rhs._machine &&
                _pid == rhs._pid &&
                _increment == rhs._increment;
        }

        /// <summary>
        /// Compares this ObjectId to another object.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True if the other object is an ObjectId and equal to this one.</returns>
        public override bool Equals(object obj)
        {
            if (obj is ObjectId)
            {
                return Equals((ObjectId)obj);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the hash code.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            int hash = 17;
            hash = 37 * hash + _timestamp.GetHashCode();
            hash = 37 * hash + _machine.GetHashCode();
            hash = 37 * hash + _pid.GetHashCode();
            hash = 37 * hash + _increment.GetHashCode();
            return hash;
        }

        /// <summary>
        /// Converts the ObjectId to a byte array.
        /// </summary>
        /// <returns>A byte array.</returns>
        public byte[] ToByteArray()
        {
            return Pack(_timestamp, _machine, _pid, _increment);
        }

        /// <summary>
        /// Returns a string representation of the value.
        /// </summary>
        /// <returns>A string representation of the value.</returns>
        public override string ToString()
        {
            return ToHexString(ToByteArray());
        }

        /// <summary>
        /// Parses a hex string into its equivalent byte array.
        /// </summary>
        /// <param name="s">The hex string to parse.</param>
        /// <returns>The byte equivalent of the hex string.</returns>
        public static byte[] ParseHexString(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }

            if (s.Length % 2 == 1)
            {
                throw new Exception("The binary key cannot have an odd number of digits");
            }

            byte[] arr = new byte[s.Length >> 1];

            for (int i = 0; i < s.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(s[i << 1]) << 4) + (GetHexVal(s[(i << 1) + 1])));
            }

            return arr;
        }
        /// <summary>
        /// Converts a byte array to a hex string.
        /// </summary>
        /// <param name="bytes">The byte array.</param>
        /// <returns>A hex string.</returns>
        public static string ToHexString(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException("bytes");
            }
            var result = new char[bytes.Length * 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                var val = _lookup32[bytes[i]];
                result[2 * i] = (char)val;
                result[2 * i + 1] = (char)(val >> 16);
            }
            return new string(result);
        }
        /// <summary>
        /// Converts a DateTime to number of milliseconds since Unix epoch.
        /// </summary>
        /// <param name="dateTime">A DateTime.</param>
        /// <returns>Number of seconds since Unix epoch.</returns>
        public static long ToMillisecondsSinceEpoch(DateTime dateTime)
        {
            var utcDateTime = ToUniversalTime(dateTime);
            return (utcDateTime - __unixEpoch).Ticks / 10000;
        }
        /// <summary>
        /// Converts a DateTime to UTC (with special handling for MinValue and MaxValue).
        /// </summary>
        /// <param name="dateTime">A DateTime.</param>
        /// <returns>The DateTime in UTC.</returns>
        public static DateTime ToUniversalTime(DateTime dateTime)
        {
            if (dateTime == DateTime.MinValue)
            {
                return DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
            }
            else if (dateTime == DateTime.MaxValue)
            {
                return DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);
            }
            else
            {
                return dateTime.ToUniversalTime();
            }
        }

        private static int GetHexVal(char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            //return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }
    }

    /// <summary>
    /// From: https://github.com/twitter/snowflake
    /// An object that generates IDs.
    /// This is broken into a separate class in case
    /// we ever want to support multiple worker threads
    /// per process
    /// </summary>
    public class Snowflake : Singleton<Snowflake>
    {
        private long workerId;
        private long datacenterId;
        private long sequence = 0L;

        private static long twepoch = 1288834974657L;

        private static long workerIdBits = 5L;
        private static long datacenterIdBits = 5L;
        private static long maxWorkerId = -1L ^ (-1L << (int)workerIdBits);
        private static long maxDatacenterId = -1L ^ (-1L << (int)datacenterIdBits);
        private static long sequenceBits = 12L;

        private long workerIdShift = sequenceBits;
        private long datacenterIdShift = sequenceBits + workerIdBits;
        private long timestampLeftShift = sequenceBits + workerIdBits + datacenterIdBits;
        private long sequenceMask = -1L ^ (-1L << (int)sequenceBits);

        private long lastTimestamp = -1L;
        private static object syncRoot = new object();


        public Snowflake()
        {
            //CreateSnowflake(0L, -1);            
            CreateSnowflake(1L, 1);
        }
        public Snowflake(long machineId)
        {
            CreateSnowflake(machineId, -1);
        }

        public Snowflake(long machineId, long datacenterId)
        {
            CreateSnowflake(machineId, datacenterId);
        }
        public void CreateSnowflake(long workerId, long datacenterId)
        {

            // sanity check for workerId
            if (workerId > maxWorkerId || workerId < 0)
            {
                throw new ArgumentException(string.Format("worker Id can't be greater than %d or less than 0", maxWorkerId));
            }
            if (datacenterId > maxDatacenterId || datacenterId < 0)
            {
                throw new ArgumentException(string.Format("datacenter Id can't be greater than %d or less than 0", maxDatacenterId));
            }
            this.workerId = workerId;
            this.datacenterId = datacenterId;
        }

        public long NextId()
        {
            lock (syncRoot)
            {
                long timestamp = timeGen();

                if (timestamp < lastTimestamp)
                {
                    throw new ApplicationException(string.Format("Clock moved backwards.  Refusing to generate id for %d milliseconds", lastTimestamp - timestamp));
                }

                if (lastTimestamp == timestamp)
                {
                    sequence = (sequence + 1) & sequenceMask;
                    if (sequence == 0)
                    {
                        timestamp = tilNextMillis(lastTimestamp);
                    }
                }
                else
                {
                    sequence = 0L;
                }

                lastTimestamp = timestamp;

                return ((timestamp - twepoch) << (int)timestampLeftShift) | (datacenterId << (int)datacenterIdShift) | (workerId << (int)workerIdShift) | sequence;
            }
        }

        protected long tilNextMillis(long lastTimestamp)
        {
            long timestamp = timeGen();
            while (timestamp <= lastTimestamp)
            {
                timestamp = timeGen();
            }
            return timestamp;
        }

        protected long timeGen()
        {
            return (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }
    }

}
