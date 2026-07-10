using System;

namespace Logger.DataLogger
{
    /// <summary>
    /// Represents custom event arguments for new added data event.
    /// </summary>
    public class DataAddedEventArgs<T> : EventArgs where T : struct
    {
        /// <summary>
        /// Gets data.
        /// </summary>
        public T Data { get; }

        /// <summary>
        /// Initializes new instance of DataAddedEventArgs.
        /// </summary>
        /// <param name="data">Added data.</param>
        public DataAddedEventArgs(T data)
        {
            Data = data;
        }
    }
}
