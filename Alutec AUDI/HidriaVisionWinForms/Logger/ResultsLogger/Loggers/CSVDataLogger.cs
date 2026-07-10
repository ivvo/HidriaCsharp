using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Logger.DataLogger
{
    public class CSVDataLogger<T> : DataLoggerBase<T> where T : struct
    {
        #region Constants
        private const string FileExtention = "csv";
        #endregion

        #region Private fields
        private string Station;
        private string FullPath;
        private int MaxNumOfEntries;
        private int TypeNum;
        private int ProgramNum;
        #endregion

        /// <summary>
        /// Constructs object of type CSVDataLogger.
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
        public CSVDataLogger(string folderPath, string station, int typeNum, int programNum, int maxNumOfEntries, Action<Task> errorHandlingCallback) : base(errorHandlingCallback)
        {
            // Check if folder path is null or empty
            if(string.IsNullOrEmpty(folderPath))
                throw new ArgumentException("Folder path is null or empty.");

            // Check if station name is null or empty
            if(string.IsNullOrEmpty(station))
                throw new ArgumentException("Station name is null or empty.");

            // Check if all values are positive
            if(typeNum < 0 || programNum < 0 || maxNumOfEntries < 0)
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
            int NumOfEntries = 0;

            // Consume data
            while (!Data.IsCompleted)
            {
                // Check if operation has been canceled
                if (TokenSource.Token.IsCancellationRequested)
                    break;

                // Try to get data from the queue
                if (base.Data.TryTake(out T Data, 50))
                {
                    StringBuilder ResultLogEntry = new StringBuilder();

                    // Get all the fields from the structure
                    FieldInfo[] StructFields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);

                    // Get current date
                    DateTime CurrentDate = DateTime.Now;

                    // Try to get the last data log file
                    LogFile? CurrentLogFile = LogFile.GetLastLogFile(FullPath, FileExtention, CurrentDate);

                    // If there is no data log file present, create a new one. If there is data log file present, count all lines inside of it
                    if (CurrentLogFile == null)
                    {
                        StringBuilder CSVHeader = new StringBuilder();

                        CurrentLogFile = new LogFile(CurrentDate, 1, FullPath, FileExtention);
                        File.AppendAllText(CurrentLogFile.Value.FullLogFilePath, GenerateCSVHeader() + Environment.NewLine);
                        NumOfEntries = 1;
                    }
                    else
                        NumOfEntries = File.ReadAllLines(CurrentLogFile.Value.FullLogFilePath).Length - 1;

                    // Check if we reached maximum number of entries
                    if (NumOfEntries >= MaxNumOfEntries)
                    {
                        // Create new data log file and reset counter
                        CurrentLogFile = new LogFile(CurrentDate, CurrentLogFile.Value.LogIndex + 1, FullPath, FileExtention);
                        File.AppendAllText(CurrentLogFile.Value.FullLogFilePath, GenerateCSVHeader() + Environment.NewLine);
                        NumOfEntries = 1;
                    }

                    // Go through all fields and their values and write them to data log file
                    for (int i = 0; i < StructFields.Length; i++)
                    {
                        if (i == StructFields.Length - 1)
                            ResultLogEntry.AppendFormat(CultureInfo.InvariantCulture, "{0}", StructFields[i].GetValue(Data));
                        else
                            ResultLogEntry.AppendFormat(CultureInfo.InvariantCulture, "{0};", StructFields[i].GetValue(Data));
                    }

                    // Write log entry into file
                    File.AppendAllText(CurrentLogFile.Value.FullLogFilePath, ResultLogEntry + Environment.NewLine);

                    // Fire the event when new data log entry is added
                    OnDataAddedEvent(Data);
                }
            }
        }

        /// <summary>
        /// Generates CSV header.
        /// </summary>
        /// <returns></returns>
        private string GenerateCSVHeader()
        {
            StringBuilder CSVHeader = new StringBuilder();
            FieldInfo[] StructFields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);

            // Go through all fields and create CSV header
            for (int i = 0; i < StructFields.Length; i++)
            {
                if (i == StructFields.Length - 1)
                    CSVHeader.AppendFormat("{0}", StructFields[i].Name);
                else
                    CSVHeader.AppendFormat("{0};", StructFields[i].Name);
            }

            return CSVHeader.ToString();
        }
        #endregion
    }
}
