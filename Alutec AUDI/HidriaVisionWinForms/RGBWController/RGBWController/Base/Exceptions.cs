using System;

namespace LedController
{
    /// <summary>
    /// Custom exception for failed operation on the controller
    /// </summary>
    public class ControllerOperationFailedException : Exception
    {
        // Just creates the exception
        public ControllerOperationFailedException()
        {
        }

        /// <summary>
        /// Initializes new instance with the message.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public ControllerOperationFailedException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initializes new instance with the message and the inner cause.
        /// </summary>
        /// <param name="message">Exception message.</param>
        /// <param name="inner">Inner exception.</param>
        public ControllerOperationFailedException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
