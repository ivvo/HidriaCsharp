using System;

namespace Logger
{
    /// <summary>
    /// Represents custom event arguments for new added log entry event.
    /// </summary>
    public class LogEntryAddedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets log entry.
        /// </summary>
        public LogEntry AddedLogEntry{ get; }

        /// <summary>
        /// Initializes new instance of LogEntryAddedEventArgs.
        /// </summary>
        /// <param name="logEntry">Added log entry.</param>
        public LogEntryAddedEventArgs(LogEntry logEntry)
        {
            AddedLogEntry = logEntry;
        }
    }
}
