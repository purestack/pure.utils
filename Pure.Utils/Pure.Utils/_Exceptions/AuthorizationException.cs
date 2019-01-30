using System;
using System.Runtime.Serialization;

namespace Pure.Utils
{
    /// <summary>
    /// This exception is thrown on an unauthorized request.
    /// </summary>
    [Serializable]
    public class  AuthorizationException : BaseException
    {
        /// <summary>
        /// Creates a new <see cref="MiniceAuthorizationException"/> object.
        /// </summary>
        public AuthorizationException()
        {

        }

        /// <summary>
        /// Creates a new <see cref="MiniceAuthorizationException"/> object.
        /// </summary>
        public AuthorizationException(SerializationInfo serializationInfo, StreamingContext context)
            : base(serializationInfo, context)
        {

        }

        /// <summary>
        /// Creates a new <see cref="MiniceAuthorizationException"/> object.
        /// </summary>
        /// <param name="message">Exception message</param>
        public AuthorizationException(string message)
            : base(message)
        {

        }

        /// <summary>
        /// Creates a new <see cref="MiniceAuthorizationException"/> object.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public AuthorizationException(string message, Exception innerException)
            : base(message, innerException)
        {

        }
    }
}
