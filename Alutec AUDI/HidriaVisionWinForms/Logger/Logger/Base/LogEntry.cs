using System;

namespace Logger
{
    /// <summary>
    /// This structure represents a log entry.
    /// </summary>
    public struct LogEntry
    {
        #region Public fields
        public readonly DateTime TimeOfOccurance;
        public readonly LoggingLevel EventSeverity;
        public readonly string CallingMember;
        public readonly string Message;
        #endregion

        public LogEntry(LoggingLevel eventSeverity, string callingMember, string message)
        {
            TimeOfOccurance = DateTime.Now;
            EventSeverity = eventSeverity;
            CallingMember = callingMember;
            Message = message;
        }
    }
}
