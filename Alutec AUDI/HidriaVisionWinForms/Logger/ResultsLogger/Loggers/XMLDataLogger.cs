
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Logger.DataLogger
{
    public class XMLDataLogger<T> : DataLoggerBase<T> where T : struct
    {
        #region Constants
        private const string FileExtention = "xml";
        #endregion

        #region Private fields
        private string Station;
        private string FullPath;
        private int MaxNumOfEntries;
        private int TypeNum;
        private int ProgramNum;
        #endregion

        /// <summary>
        /// Constructs object of type XMLDataLogger.
        /// </summary>
        /// <param name="folderPath">Folder path.</param>
        /// <param name="station">Station name.</param>
        /// <param name="typeNum">Part type number.</param>
        /// <param name="programNum">Program number.</param>
        /// <param name="maxNumOfEntries">Maximum number of entries in a file.</param>
        /// <param name="errorHandlingCallback">Callback for errors.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="System.Security.SecurityException"></exception> 
        /// <exception cref="NotSupportedException"></exception> 
        /// <exception cref="PathTooLongException"></exception> 
        public XMLDataLogger(string folderPath, string station, int typeNum, int programNum, int maxNumOfEntries, Action<Task> errorHandlingCallback) : base(errorHandlingCallback)
        {
            // Check if folder path is null or empty
            if (string.IsNullOrEmpty(folderPath))
                throw new ArgumentException("Folder path is null or empty.");

            // Check if station name is null or empty
            if (string.IsNullOrEmpty(station))
                throw new ArgumentException("Station name is null or empty.");

            // Check if all values are positive
            if (typeNum < 0 || programNum < 0 || maxNumOfEntries < 0)
                throw new ArgumentException("Type number, program number and maximum number of entries must be a positive numbers.");

            // Check if handler is null
            if (errorHandlingCallback == null)
                throw new ArgumentException("Error handling callback is null.");

            Station = station;
            TypeNum = typeNum;
            ProgramNum = programNum;
            MaxNumOfEntries = maxNumOfEntries;

            // Construct full path
            FullPath = string.Format(@"{0}\{1}\Type{2:D3}\P{3:D3}", Path.GetFullPath(folderPath), Station, TypeNum, ProgramNum);
        }

        #region Public methods
        /// <summary>
        /// Prepares data logger.
        /// </summary> 
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public override void Prepare()
        {
            if (!IsPrepared)
            {
                // Create the directories
                Directory.CreateDirectory(FullPath);

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
            // Consume data
            while (!Data.IsCompleted)
            {
                // Check if operation has been canceled
                if (TokenSource.Token.IsCancellationRequested)
                    break;

                // Try to get data from the queue
                if (base.Data.TryTake(out T Data, 50))
                {
                    XDocument Document;
                    XElement LogEntryElement;

                    // Get all the fields from the structure
                    FieldInfo[] StructFields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);

                    // Get current date
                    DateTime CurrentDate = DateTime.Now;

                    // Try to get the last log file
                    LogFile? CurrentLogFile = LogFile.GetLastLogFile(FullPath, FileExtention, CurrentDate);

                    if (CurrentLogFile == null)
                    {
                        // Create new log file
                        CurrentLogFile = new LogFile(CurrentDate, 1, FullPath, FileExtention);

                        // Create new xml
                        XElement RootElement = new XElement("ResultsLog");
                        Document = new XDocument();
                        Document.Add(RootElement);
                    }
                    else
                    {
                        // Load the existing xml file
                        Document = XDocument.Load(CurrentLogFile.Value.FullLogFilePath);

                        // Check if we reached maximum numbers of entries
                        if (Document.Element("ResultsLog").Elements("ResultEntry").Count() >= MaxNumOfEntries)
                        {
                            // Create new xml
                            XElement RootElement = new XElement("ResultsLog");
                            Document = new XDocument();
                            Document.Add(RootElement);

                            // Create new log file
                            CurrentLogFile = new LogFile(CurrentDate, CurrentLogFile.Value.LogIndex + 1, FullPath, FileExtention);
                        }
                    }

                    // Construct the data log entry element
                    LogEntryElement = new XElement("ResultEntry");

                    // Go through all fields and their values and add it to log entry element
                    foreach (FieldInfo structField in StructFields)
                        LogEntryElement.Add(new XElement(structField.Name, string.Format(CultureInfo.InvariantCulture, "{0}", structField.GetValue(Data))));

                    // Append the log entry element to xml file
                    Document.Element("ResultsLog").Add(LogEntryElement);

                    // Save the xml file
                    Document.Save(CurrentLogFile.Value.FullLogFilePath);

                    // Fire the event when new data log entry is added
                    OnDataAddedEvent(Data);
                }
            }
        }
        #endregion
    }
}
