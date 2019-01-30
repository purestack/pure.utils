using System;
using System.Runtime.Serialization;

namespace Pure.Utils
{
    /// <summary>
    /// Base exception type for those are thrown by Minice system for Minice specific exceptions.
    /// </summary>
    [Serializable]
    public class BaseException : Exception
    {
        /// <summary>
        /// Creates a new <see cref="MiniceException"/> object.
        /// </summary>
        public BaseException()
        {

        }

        /// <summary>
        /// Creates a new <see cref="MiniceException"/> object.
        /// </summary>
        public BaseException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {

        }

        /// <summary>
        /// Creates a new <see cref="MiniceException"/> object.
        /// </summary>
        /// <param name="message">Exception message</param>
        public BaseException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Creates a new <see cref="MiniceException"/> object.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public BaseException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
