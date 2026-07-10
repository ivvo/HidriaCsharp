using System;
using System.IO;
using System.Threading.Tasks;

namespace Logger
{
    public class CSVEventLogger : EventLoggerBase
    {
        #region Constants
        private const string FileExtention = "csv";
        #endregion

        #region Private fields
        private string FolderPath;
        private int MaxNumOfEntries;
        #endregion

        /// <summary>
        /// Constructs object of type CSVEventLogger.
        /// </summary>
        /// <param name="folderPath">Folder path.</param>
        /// <param name="maxNumOfEntries">Maximum number of entries in a file.</param>
        /// <param name="errorHandlingCallback">Callback for errors.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="System.Security.SecurityException"></exception> 
        /// <exception cref="NotSupportedException"></exception> 
        /// <exception cref="PathTooLongException"></exception> 
        public CSVEventLogger(string folderPath, int maxNumOfEntries, Action<Task> errorHandlingCallback) : base(errorHandlingCallback)
        {
            // Check if folder path is null or empty
            if (string.IsNullOrEmpty(folderPath))
                throw new ArgumentException("Folder path is null or empty.");

            // Check if number of entries is positive number
            if (maxNumOfEntries < 0)
                throw new ArgumentException("Maximum number of entries must be a positive numbers.");

            // Check if handler is null
            if (errorHandlingCallback == null)
                throw new ArgumentException("Error handling callback is null.");

            FolderPath = Path.GetFullPath(folderPath);
            MaxNumOfEntries = maxNumOfEntries;
        }

        #region Public methods
        /// <summary>
        /// Prepares the event logger.
        /// </summary>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public override void Prepare()
        {
            if (!IsPrepared)
            {
                // Create directory for logs
                Directory.CreateDirectory(FolderPath);

                IsPrepared = true;
            }
        }
        #endregion

        #region Private and protected methods
        /// <summary>
        /// Represents processing queue method.
        /// </summary>
        protected override void ProcessingQueue()
        {
            int NumOfEntries = 0;

            // Consume log entries
            while (!LogEntries.IsCompleted)
            {
                // Check if operation has been canceled
                if (TokenSource.Token.IsCancellationRequested)
                    break;

                // Try to get log entry from the queue
                if (LogEntries.TryTake(out LogEntry Entry, 50))
                {
                    // Get current date
                    DateTime CurrentDate = DateTime.Now;

                    // Try to get the last log file
                    LogFile? CurrentLogFile = LogFile.GetLastLogFile(FolderPath, FileExtention, CurrentDate);

                    // If there is no log file present, create a new one and append csv header. If there is log file present, count all lines inside of it
                    if (CurrentLogFile == null)
                    {
                        CurrentLogFile = new LogFile(CurrentDate, 1, FolderPath, FileExtention);
                        File.AppendAllText(CurrentLogFile.Value.FullLogFilePath, "TimeOfOccurance;EventSeverity;CallingMember;Message" + Environment.NewLine);
                        NumOfEntries = 1;
                    }
                    else
                        NumOfEntries = File.ReadAllLines(CurrentLogFile.Value.FullLogFilePath).Length - 1;

                    // Check if we reached maximum number of entries. If we reached, create new log file and append csv header
                    if (NumOfEntries >= MaxNumOfEntries)
                    {
                        // Create new log file and reset counter
                        CurrentLogFile = new LogFile(CurrentDate, CurrentLogFile.Value.LogIndex + 1, FolderPath, FileExtention);
                        File.AppendAllText(CurrentLogFile.Value.FullLogFilePath, "TimeOfOccurance;EventSeverity;CallingMember;Message" + Environment.NewLine);
                        NumOfEntries = 1;
                    }

                    // Write log entry into file
                    File.AppendAllText(CurrentLogFile.Value.FullLogFilePath, string.Format("{0:d/M/yyyy HH:mm:ss};{1};{2};{3}", Entry.TimeOfOccurance, Entry.EventSeverity.ToString(), Entry.CallingMember, Entry.Message) + Environment.NewLine);

                    // Fire the event when new log entry is added
                    OnLogEntryAddedEvent(Entry);
                }
            }
        }
        #endregion
    }
}
