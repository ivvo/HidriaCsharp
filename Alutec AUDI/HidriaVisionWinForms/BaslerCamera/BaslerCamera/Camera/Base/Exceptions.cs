using System;
using System.Runtime.Serialization;

namespace BaslerCam
{
    /// <summary>
    /// Custom camera exception.
    /// </summary>
    [Serializable]
    public class CameraException : Exception
    {
        /// <summary>
        /// Just create the exception.
        /// </summary>
        public CameraException()
        {
        }

        /// <summary>
        /// Initializes new instance with the message.
        /// </summary>
        /// <param name="message">Exception message</param>
        public CameraException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes new instance with the message and the inner cause.
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="inner">Inner exception</param>
        public CameraException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes new instance that holds the serialized data about exception being thrown.
        /// </summary>
        /// <param name="info">Holds the serialized object data about the exception being thrown</param>
        /// <param name="context">Contains contextual information about the source or destination</param>
        protected CameraException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
