using System;

namespace Snap7Manager
{
    /// <summary>
    /// Custom snap7 exception
    /// </summary>
    public class Snap7Exception : Exception
    {
        /// <summary>
        /// Just create the exception
        /// </summary>
        public Snap7Exception() { }

        /// <summary>
        /// Initializes new instance with the message.
        /// </summary>
        /// <param name="message">Exception message.</param>
        public Snap7Exception(string message) : base(message) { }
    }
}
