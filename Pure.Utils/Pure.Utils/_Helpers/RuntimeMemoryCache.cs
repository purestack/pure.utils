using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
#if !NET45
using Microsoft.Extensions.Caching.Memory;
#else
using System.Runtime.Caching;

#endif
namespace Pure.Utils
{

#if !NET45
    /// <summary>
    /// 运行时内存缓存
    /// </summary>
    public class RuntimeMemoryCache 
    {
        TimeSpan DefaultSlidingExpireTime = new TimeSpan(0, 30, 0);
        private readonly string _region;
        private IMemoryCache _cache;
        /// <summary>
        /// 初始化一个<see cref="RuntimeMemoryCache"/>类型的新实例
        /// </summary>
        public RuntimeMemoryCache(string region)
        {
            _region = region;

            this._cache = new MemoryCache(new MemoryCacheOptions());
          

        }

        
       

        #region Implementation of ICache

        /// <summary>
        /// 从缓存中获取数据
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>获取的数据</returns>
        public  object Get(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }
            string cacheKey = GetCacheKey(key);
            object value = _cache.Get(cacheKey);
            if (value == null)
            {
                return null;
            }
            CacheEntry entry = (CacheEntry)value;

            return entry != null ? entry.Value : null;
        }

        /// <summary>
        /// 从缓存中获取强类型数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <returns>获取的强类型数据</returns>
        public  T Get<T>(string key)
        {
            return (T)Get(key);
        }

        /// <summary>
        /// 获取当前缓存对象中的所有数据
        /// </summary>
        /// <returns>所有数据的集合</returns>
        public  IEnumerable<object> GetAll()
        {

            return GetCacheValues();
        }

        /// <summary>
        /// 获取所有缓存键
        /// </summary>
        /// <returns></returns>
        public List<string> GetCacheKeys()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var entries = _cache.GetType().GetField("_entries", flags).GetValue(_cache);
            var cacheItems = entries as IDictionary;
            var keys = new List<string>();
            if (cacheItems == null) return keys;

            string token = _region + ":";
            string key = "";
            foreach (DictionaryEntry cacheItem in cacheItems)
            {
                key = cacheItem.Key.ToString();
                if (key.StartsWith(token))
                {
                    keys.Add(key);

                }
            }


            return keys;
        }
        public List<object> GetCacheValues()
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var entries = _cache.GetType().GetField("_entries", flags).GetValue(_cache);
            var cacheItems = entries as IDictionary;
            var values = new List<object>();
            if (cacheItems == null) return values;

            string token = _region + ":";
            string key = "";
            foreach (DictionaryEntry cacheItem in cacheItems)
            {
                key = cacheItem.Key.ToString();
                if (key.StartsWith(token))
                {
                    values.Add(cacheItem.Value);

                }
            }

            return values;
        }
        /// <summary>
        /// 获取当前缓存中的所有数据
        /// </summary>
        /// <typeparam name="T">项数据类型</typeparam>
        /// <returns>所有数据的集合</returns>
        public  IEnumerable<T> GetAll<T>()
        {
            return GetAll().Cast<T>();
        }

        /// <summary>
        /// 使用默认配置添加或替换缓存项
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存数据</param>
        public  void Set(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }

            string cacheKey = GetCacheKey(key);
            CacheEntry entry = new CacheEntry(value, DateTime.UtcNow.Add( DefaultSlidingExpireTime));

            _cache.Set(cacheKey, entry,  DefaultSlidingExpireTime);
        }

        /// <summary>
        /// 添加或替换缓存项并设置绝对过期时间
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存数据</param>
        /// <param name="absoluteExpiration">绝对过期时间，过了这个时间点，缓存即过期</param>
        public  void Set(string key, object value, DateTime absoluteExpiration)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }
            string cacheKey = GetCacheKey(key);
            CacheEntry entry = new CacheEntry(value, absoluteExpiration);
            _cache.Set(cacheKey, entry, absoluteExpiration);
        }

        /// <summary>
        /// 添加或替换缓存项并设置相对过期时间
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存数据</param>
        /// <param name="slidingExpiration">滑动过期时间，在此时间内访问缓存，缓存将继续有效</param>
        public  void Set(string key, object value, TimeSpan slidingExpiration)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }
            string cacheKey = GetCacheKey(key);
            CacheEntry entry = new CacheEntry(value, DateTime.UtcNow.Add(slidingExpiration));
            _cache.Set(cacheKey, entry, slidingExpiration);
        }

        /// <summary>
        /// 移除指定键的缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        public  void Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }
            string cacheKey = GetCacheKey(key);
            _cache.Remove(cacheKey);
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        public  void FlushAll()
        {
            string token = _region + ":";
            var keys = GetCacheKeys();
            List<string> cacheKeys = keys.Where(m => m.StartsWith(token)).Select(m => m).ToList();
            foreach (string cacheKey in cacheKeys)
            {
                _cache.Remove(cacheKey);
            }
        }


        private readonly object _lockObject = new object();
        private long UpdateCounter(string key, long value, TimeSpan? expiresIn = null)
        {

            if (expiresIn.HasValue && expiresIn.Value.Ticks < 0)
            {
                Remove(key);
                return -1;
            }
            var keys = GetCacheKeys();

            lock (_lockObject)
            {
                string cacheKey = GetCacheKey(key);

                if (!keys.Contains(cacheKey))
                {
                    if (expiresIn.HasValue)
                        Set(key, value, expiresIn.Value);
                    else
                        Set(key, value);
                    return value;
                }

                var current = Get<long>(key);
                if (expiresIn.HasValue)
                    Set(key, current += value, expiresIn.Value);
                else
                    Set(key, current += value);
                return current;
            }
        }

        public  long Increment(string key, uint amount)
        {
            return UpdateCounter(key, amount);
        }

        public  long Increment(string key, uint amount, DateTime expiresAt)
        {
            return UpdateCounter(key, amount, expiresAt.ToUniversalTime().Subtract(DateTime.UtcNow));
        }

        public  long Increment(string key, uint amount, TimeSpan expiresIn)
        {
            return UpdateCounter(key, amount, expiresIn);
        }

        public  long Decrement(string key, uint amount)
        {
            return UpdateCounter(key, amount * -1);
        }

        public  long Decrement(string key, uint amount, DateTime expiresAt)
        {
            return UpdateCounter(key, amount * -1, expiresAt.ToUniversalTime().Subtract(DateTime.UtcNow));
        }

        public  long Decrement(string key, uint amount, TimeSpan expiresIn)
        {
            return UpdateCounter(key, amount * -1, expiresIn);
        }
        private readonly object _lock = new object();
        private bool TryGetValue(string key, out object entry)
        {
            return TryGet(key, out entry);
        }
        public  bool TryGet<T>(string key, out T value)
        {
            string cacheKey = GetCacheKey(key);

            value = default(T);
            var keys = GetCacheKeys();
            if (!keys.Contains(cacheKey))
                return false;

            try
            {
                value = Get<T>(key);
                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool CacheAdd(string key, object value)
        {
            return CacheAdd(key, value, DateTime.MaxValue);
        }
        private bool CacheAdd(string key, object value, DateTime expiresAt)
        {
            expiresAt = expiresAt.ToUniversalTime();
            if (expiresAt < DateTime.UtcNow)
            {
                Remove(key);
                return false;
            }

            lock (_lock)
            {
                object entry;
                if (TryGetValue(key, out entry))
                    return false;

                Set(key, value, expiresAt);

                return true;
            }
        }

        public  bool Add<T>(string key, T value)
        {
            return CacheAdd(key, value);
        }
        public  bool Add<T>(string key, T value, DateTime expiresAt)
        {
            return CacheAdd(key, value, expiresAt);
        }
        public  bool Add<T>(string key, T value, TimeSpan expiresIn)
        {
            return CacheAdd(key, value, DateTime.UtcNow.Add(expiresIn));
        }

        private bool CacheSet(string key, object value)
        {
            return CacheSet(key, value, DateTime.MaxValue);
        }

        private bool CacheSet(string key, object value, DateTime expiresAt)
        {
            return CacheSet(key, value, expiresAt, null);
        }

        private bool CacheSet(string key, object value, DateTime expiresAt, long? checkLastModified)
        {

            expiresAt = expiresAt.ToUniversalTime();
            if (expiresAt < DateTime.UtcNow)
            {
                Remove(key);
                return false;
            }

            CacheEntry entry;
            if (!TryGet(key, out entry))
            {
                Set(key, value, expiresAt);
                return true;
            }

            if (checkLastModified.HasValue
                && entry.LastModifiedTicks != checkLastModified.Value)
                return false;

            entry.Value = value;
            entry.ExpiresAt = expiresAt;

            return true;
        }

        private bool CacheReplace(string key, object value)
        {
            return CacheReplace(key, value, DateTime.MaxValue);
        }

        private bool CacheReplace(string key, object value, DateTime expiresAt)
        {
            return !CacheSet(key, value, expiresAt);
        }
        public  bool Replace<T>(string key, T value)
        {
            return CacheReplace(key, value);
        }
        public  bool Replace<T>(string key, T value, DateTime expiresAt)
        {
            return CacheReplace(key, value, expiresAt);
        }
        public  bool Replace<T>(string key, T value, TimeSpan expiresIn)
        {
            return CacheReplace(key, value, DateTime.UtcNow.Add(expiresIn));
        }

        public  void RemoveByPrefix(string prefix)
        {
            RemoveByPattern(prefix + "*");
        }

        public  DateTime? GetExpiration(string key)
        {
            CacheEntry value;
            if (!TryGet(key, out value))
                return null;

            if (value.ExpiresAt >= DateTime.UtcNow)
                return value.ExpiresAt;

            Remove(key);
            return null;
        }

        public  void SetExpiration(string key, DateTime expiresAt)
        {
            expiresAt = expiresAt.ToUniversalTime();
            if (expiresAt < DateTime.UtcNow)
            {
                Remove(key);
                return;
            }

            CacheEntry value;
            if (!TryGet(key, out value))
                return;

            value.ExpiresAt = expiresAt;
        }

        public  void SetExpiration(string key, TimeSpan expiresIn)
        {
            SetExpiration(key, DateTime.UtcNow.Add(expiresIn));
        }

        public void RemoveByPattern(string pattern)
        {
            RemoveByRegex(pattern.Replace("*", ".*").Replace("?", ".+"));
        }

        public void RemoveByRegex(string pattern)
        {
            //var regex = new Regex(pattern);
            //var enumerator = _cache.AsEnumerable();
            try
            {

                IList<string> l = SearchCacheRegex(pattern);
                foreach (var s in l)
                {
                    Remove(s);
                }

                //foreach (var current in enumerator)
                //{
                //    if (regex.IsMatch(current.Key))
                //        Remove(current.Key);
                //    else
                //    {
                //        var v = current.Value as CacheEntry;
                //        if (v != null || v.ExpiresAt < DateTime.UtcNow)
                //        {
                //            Remove(current.Key);

                //        }
                //    }

                //}

            }
            catch
            {
                //Logger.Error().Exception(ex).Message("Error trying to remove items from cache with this {0} pattern", pattern).Write();
            }

        }

        /// <summary>
        /// 搜索 匹配到的缓存
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public IList<string> SearchCacheRegex(string pattern)
        {
            var cacheKeys = GetCacheKeys();
            var l = cacheKeys.Where(k => Regex.IsMatch(k, pattern)).ToList();
            return l.AsReadOnly();
        }
        #endregion

        #region 私有方法

        private string GetCacheKey(string key)
        {
            return string.IsNullOrEmpty(_region) ? key : string.Concat(_region, ":", key);
        }

        #endregion
    }

#else

using System.Runtime.Caching;


       /// <summary>
    /// 运行时内存缓存
    /// </summary>
    public class RuntimeMemoryCache : CacheBase
    {
        private readonly string _region;
        private readonly ObjectCache _cache;

        /// <summary>
        /// 初始化一个<see cref="RuntimeMemoryCache"/>类型的新实例
        /// </summary>
        public RuntimeMemoryCache(string region)
        {
            _region = region;
            _cache = MemoryCache.Default;
            _option = new CacheOption();

        }

        CacheOption _option = null;
        public  CacheOption Option
        {
            get
            {
                return _option;
            }
            set
            {
                _option = value;
            }
        }
        /// <summary>
        /// 获取 缓存区域名称，可作为缓存键标识，给缓存管理带来便利
        /// </summary>
        public  string Region
        {
            get { return _region; }
        }

    #region Implementation of ICache

        /// <summary>
        /// 从缓存中获取数据
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <returns>获取的数据</returns>
        public  object Get(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }
            string cacheKey = GetCacheKey(key);
            object value = _cache.Get(cacheKey);
            if (value == null)
            {
                return null;
            }
            CacheEntry entry = (CacheEntry)value;

            return entry != null ? entry.Value : null;
        }

        /// <summary>
        /// 从缓存中获取强类型数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">缓存键</param>
        /// <returns>获取的强类型数据</returns>
        public  T Get<T>(string key)
        {
            return (T)Get(key);
        }

        /// <summary>
        /// 获取当前缓存对象中的所有数据
        /// </summary>
        /// <returns>所有数据的集合</returns>
        public  IEnumerable<object> GetAll()
        {
            string token = string.Concat(_region, ":");
            return _cache.Where(m => m.Key.StartsWith(token)).Select(m => m.Value).Cast<CacheEntry>().Select(m => m.Value);
        }

        /// <summary>
        /// 获取当前缓存中的所有数据
        /// </summary>
        /// <typeparam name="T">项数据类型</typeparam>
        /// <returns>所有数据的集合</returns>
        public  IEnumerable<T> GetAll<T>()
        {
            return GetAll().Cast<T>();
        }

        /// <summary>
        /// 使用默认配置添加或替换缓存项
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存数据</param>
        public  void Set(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }
             
            string cacheKey = GetCacheKey(key);
            CacheEntry entry = new CacheEntry(value, DateTime.UtcNow.Add(Option.DefaultSlidingExpireTime));
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.SlidingExpiration = Option.DefaultSlidingExpireTime;
            _cache.Set(cacheKey, entry, policy);
        }

        /// <summary>
        /// 添加或替换缓存项并设置绝对过期时间
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存数据</param>
        /// <param name="absoluteExpiration">绝对过期时间，过了这个时间点，缓存即过期</param>
        public  void Set(string key, object value, DateTime absoluteExpiration)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }
            string cacheKey = GetCacheKey(key);
            CacheEntry entry = new CacheEntry(value, absoluteExpiration);
            CacheItemPolicy policy = new CacheItemPolicy() { AbsoluteExpiration = absoluteExpiration };
            _cache.Set(cacheKey, entry, policy);
        }

        /// <summary>
        /// 添加或替换缓存项并设置相对过期时间
        /// </summary>
        /// <param name="key">缓存键</param>
        /// <param name="value">缓存数据</param>
        /// <param name="slidingExpiration">滑动过期时间，在此时间内访问缓存，缓存将继续有效</param>
        public  void Set(string key, object value, TimeSpan slidingExpiration)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }
            string cacheKey = GetCacheKey(key);
            CacheEntry entry = new CacheEntry(value, DateTime.UtcNow.Add(slidingExpiration));
            CacheItemPolicy policy = new CacheItemPolicy() { SlidingExpiration = slidingExpiration };
            _cache.Set(cacheKey, entry, policy);
        }

        /// <summary>
        /// 移除指定键的缓存
        /// </summary>
        /// <param name="key">缓存键</param>
        public  void Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("key");
            }
            string cacheKey = GetCacheKey(key);
            _cache.Remove(cacheKey);
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        public  void FlushAll()
        {
            string token = _region + ":";
            List<string> cacheKeys = _cache.Where(m => m.Key.StartsWith(token)).Select(m => m.Key).ToList();
            foreach (string cacheKey in cacheKeys)
            {
                _cache.Remove(cacheKey);
            }
        }


        private readonly object _lockObject = new object();
        private long UpdateCounter(string key, long value, TimeSpan? expiresIn = null)
        {
             
            if (expiresIn.HasValue && expiresIn.Value.Ticks < 0)
            {
                Remove(key);
                return -1;
            }

            lock (_lockObject)
            {
                string cacheKey = GetCacheKey(key);

                if (!_cache.Contains(cacheKey))
                {
                    if (expiresIn.HasValue)
                        Set(key, value, expiresIn.Value);
                    else
                        Set(key, value);
                    return value;
                }

                var current = Get<long>(key);
                if (expiresIn.HasValue)
                    Set(key, current += value, expiresIn.Value);
                else
                    Set(key, current += value);
                return current;
            }
        }

        public  long Increment(string key, uint amount)
        {
            return UpdateCounter(key, amount);
        }

        public  long Increment(string key, uint amount, DateTime expiresAt)
        {
            return UpdateCounter(key, amount, expiresAt.ToUniversalTime().Subtract(DateTime.UtcNow));
        }

        public  long Increment(string key, uint amount, TimeSpan expiresIn)
        {
            return UpdateCounter(key, amount, expiresIn);
        }

        public  long Decrement(string key, uint amount)
        {
            return UpdateCounter(key, amount * -1);
        }

        public  long Decrement(string key, uint amount, DateTime expiresAt)
        {
            return UpdateCounter(key, amount * -1, expiresAt.ToUniversalTime().Subtract(DateTime.UtcNow));
        }

        public  long Decrement(string key, uint amount, TimeSpan expiresIn)
        {
            return UpdateCounter(key, amount * -1, expiresIn);
        }
        private readonly object _lock = new object();
        private bool TryGetValue(string key, out object entry)
        {
            return TryGet(key, out entry);
        }
        public  bool TryGet<T>(string key, out T value)
        {
            string cacheKey = GetCacheKey(key);

            value = default(T);
            if (!_cache.Contains(cacheKey))
                return false;

            try
            {
                value = Get<T>(key);
                return true;
            }
            catch
            {
                return false;
            }
        }
        private bool CacheAdd(string key, object value)
        {
            return CacheAdd(key, value, DateTime.MaxValue);
        }
        private bool CacheAdd(string key, object value, DateTime expiresAt)
        {
            expiresAt = expiresAt.ToUniversalTime();
            if (expiresAt < DateTime.UtcNow)
            {
                Remove(key);
                return false;
            }

            lock (_lock)
            {
                object entry;
                if (TryGetValue(key, out entry))
                    return false;
                 
                Set(key, value, expiresAt);

                return true;
            }
        }

        public  bool Add<T>(string key, T value)
        {
            return CacheAdd(key, value);
        }
        public  bool Add<T>(string key, T value, DateTime expiresAt)
        {
            return CacheAdd(key, value, expiresAt);
        }
        public  bool Add<T>(string key, T value, TimeSpan expiresIn)
        {
            return CacheAdd(key, value, DateTime.UtcNow.Add(expiresIn));
        }

        private bool CacheSet(string key, object value)
        {
            return CacheSet(key, value, DateTime.MaxValue);
        }

        private bool CacheSet(string key, object value, DateTime expiresAt)
        {
            return CacheSet(key, value, expiresAt, null);
        }

        private bool CacheSet(string key, object value, DateTime expiresAt, long? checkLastModified)
        {
            
            expiresAt = expiresAt.ToUniversalTime();
            if (expiresAt < DateTime.UtcNow)
            { 
                Remove(key);
                return false;
            }

            CacheEntry entry;
            if (!TryGet(key, out entry))
            { 
                Set(key, value, expiresAt);
                return true;
            }

            if (checkLastModified.HasValue
                && entry.LastModifiedTicks != checkLastModified.Value)
                return false;

            entry.Value = value;
            entry.ExpiresAt = expiresAt;
            
            return true;
        }

        private    bool CacheReplace(string key, object value)
        {
            return CacheReplace(key, value, DateTime.MaxValue);
        }

        private bool CacheReplace(string key, object value, DateTime expiresAt)
        {
            return !CacheSet(key, value, expiresAt);
        }
        public  bool Replace<T>(string key, T value)
        {
            return CacheReplace(key, value);
        }
        public  bool Replace<T>(string key, T value, DateTime expiresAt)
        {
            return CacheReplace(key, value, expiresAt);
        }
        public  bool Replace<T>(string key, T value, TimeSpan expiresIn)
        {
            return CacheReplace(key, value, DateTime.UtcNow.Add(expiresIn));
        }

        public  void RemoveByPrefix(string prefix)
        {
            RemoveByPattern(prefix + "*");
        }

        public  DateTime? GetExpiration(string key)
        {
            CacheEntry value;
            if (! TryGet(key, out value))
                return null;

            if (value.ExpiresAt >= DateTime.UtcNow)
                return value.ExpiresAt;

             Remove(key);
            return null;
        }

        public  void SetExpiration(string key, DateTime expiresAt)
        {
            expiresAt = expiresAt.ToUniversalTime();
            if (expiresAt < DateTime.UtcNow)
            {
                Remove(key);
                return;
            }

            CacheEntry value;
            if (!TryGet(key, out value))
                return;

            value.ExpiresAt = expiresAt;
        }

        public  void SetExpiration(string key, TimeSpan expiresIn)
        {
            SetExpiration(key, DateTime.UtcNow.Add(expiresIn));
        }

        public void RemoveByPattern(string pattern)
        {
            RemoveByRegex(pattern.Replace("*", ".*").Replace("?", ".+"));
        }

        public void RemoveByRegex(string pattern)
        {
            var regex = new Regex(pattern);
            var enumerator = _cache.AsEnumerable();
            try
            {
                foreach (var current in enumerator)
                {
                    if (regex.IsMatch(current.Key))
                        Remove(current.Key);
                    else
                    {
                        var v = current.Value as CacheEntry;
                        if (v != null || v.ExpiresAt < DateTime.UtcNow)
                        {
                            Remove(current.Key);

                        }
                    }
                   
                }
                
            }
            catch 
            {
                //Logger.Error().Exception(ex).Message("Error trying to remove items from cache with this {0} pattern", pattern).Write();
            }
        }
    #endregion

    #region 私有方法

        private string GetCacheKey(string key)
        {
            return string.IsNullOrEmpty(_region) ? key :  string.Concat(_region, ":", key);
        }

    #endregion
    }

#endif

    public class CacheEntry
    {
        private object _cacheValue;
        private static long _instanceCount = 0;
#if DEBUG
        private long _usageCount = 0;
#endif

        public CacheEntry(object value, DateTime expiresAt)
        {
            Value = value;
            ExpiresAt = expiresAt;
            LastModifiedTicks = DateTime.UtcNow.Ticks;
            InstanceNumber = Interlocked.Increment(ref _instanceCount);
        }

        internal long InstanceNumber { get; private set; }
        internal DateTime ExpiresAt { get; set; }
        internal long LastAccessTicks { get; private set; }
        internal long LastModifiedTicks { get; private set; }
#if DEBUG
        internal long UsageCount { get { return _usageCount; } }
#endif

        internal object Value
        {
            get
            {
                LastAccessTicks = DateTime.UtcNow.Ticks;
#if DEBUG
                Interlocked.Increment(ref _usageCount);
#endif
                return _cacheValue;
            }
            set
            {
                _cacheValue = value;
                LastAccessTicks = DateTime.UtcNow.Ticks;
                LastModifiedTicks = DateTime.UtcNow.Ticks;
            }
        }



    }
}