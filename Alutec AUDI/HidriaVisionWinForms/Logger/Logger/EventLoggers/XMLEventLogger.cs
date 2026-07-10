using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Logger
{
    public class XMLEventLogger : EventLoggerBase
    {
        #region Constants
        private const string FileExtention = "xml";
        #endregion

        #region Private fields
        private string FolderPath;
        private int MaxNumOfEntries;
        #endregion

        /// <summary>
        /// Constructs an object of type XMLEventLogger.
        /// </summary>
        /// <param name="folderPath">Folder path.</param>
        /// <param name="maxNumOfEntries">Maximum number of entries in a file.</param>
        /// <param name="errorHandlingCallback">Callback for errors.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="System.Security.SecurityException"></exception> 
        /// <exception cref="NotSupportedException"></exception> 
        /// <exception cref="PathTooLongException"></exception> 
        public XMLEventLogger(string folderPath, int maxNumOfEntries, Action<Task> errorHandlingCallback) : base(errorHandlingCallback)
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
            // Consume log entries
            while (!LogEntries.IsCompleted)
            {
                // Check if operation has been canceled
                if (TokenSource.Token.IsCancellationRequested)
                    break;

                // Try to get log entry from the queue
                if (LogEntries.TryTake(out LogEntry Entry, 50))
                {
                    XDocument Document;
                    XElement LogEntryElement;

                    // Get current date
                    DateTime CurrentDate = DateTime.Now;

                    // Try to get the last log file
                    LogFile? CurrentLogFile = LogFile.GetLastLogFile(FolderPath, FileExtention, CurrentDate);

                    if (CurrentLogFile == null)
                    {
                        // Create new log file
                        CurrentLogFile = new LogFile(CurrentDate, 1, FolderPath, FileExtention);

                        // Create new xml
                        XElement RootElement = new XElement("Log");
                        Document = new XDocument();
                        Document.Add(RootElement);
                    }
                    else
                    {
                        // Load the existing xml file
                        Document = XDocument.Load(CurrentLogFile.Value.FullLogFilePath);

                        // Check if we reached maximum numbers of entries
                        if (Document.Element("Log").Elements("LogEntry").Count() >= MaxNumOfEntries)
                        {
                            // Create new xml
                            XElement RootElement = new XElement("Log");
                            Document = new XDocument();
                            Document.Add(RootElement);

                            // Create new log file
                            CurrentLogFile = new LogFile(CurrentDate, CurrentLogFile.Value.LogIndex + 1, FolderPath, FileExtention);
                        }
                    }

                    // Construct the log entry element
                    LogEntryElement = new XElement("LogEntry", new XElement("TimeOfOccurance", Entry.TimeOfOccurance.ToString("d/M/yyyy HH:mm:ss"))
                                                   , new XElement("EventSeverity", Entry.EventSeverity.ToString())
                                                   , new XElement("CallingMember", Entry.CallingMember)
                                                   , new XElement("Message", Entry.Message));

                    // Append the log entry element to xml file
                    Document.Element("Log").Add(LogEntryElement);

                    // Save the xml file
                    Document.Save(CurrentLogFile.Value.FullLogFilePath);

                    // Fire the event when new log entry is added
                    OnLogEntryAddedEvent(Entry);
                }
            }
        }
        #endregion
    }
}
