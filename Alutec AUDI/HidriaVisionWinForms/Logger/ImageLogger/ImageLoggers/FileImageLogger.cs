using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace Logger.ImageLogger
{
    public class FileImageLogger : ImageLoggerBase
    {
        #region Constants
        private const string FileExtention = ".bmp";
        #endregion

        #region Private fields
        private string Station;
        private string FullPath;
        private int TypeNum;
        private int ProgramNum;
        #endregion

        /// <summary>
        /// Constructs the object of type FileImageLogger.
        /// </summary>
        /// <param name="folderPath">Folder path.</param>
        /// <param name="station">Station name.</param>
        /// <param name="typeNum">Type number.</param>
        /// <param name="programNum">Program number.</param>
        /// <param name="errorHandlingCallback">Callback for errors.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="System.Security.SecurityException"></exception> 
        /// <exception cref="NotSupportedException"></exception> 
        /// <exception cref="PathTooLongException"></exception> 
        public FileImageLogger(string folderPath, string station, int typeNum, int programNum, Action<Task> errorHandlingCallback) : base(errorHandlingCallback)
        {
            // Check if folder path is null or empty
            if (string.IsNullOrEmpty(folderPath))
                throw new ArgumentException("Folder path is null or empty.");

            // Check if station name is null or empty
            if (string.IsNullOrEmpty(station))
                throw new ArgumentException("Station name is null or empty.");

            // Check if handler is null
            if (errorHandlingCallback == null)
                throw new ArgumentException("Error handling callback is null.");

            Station = station;
            TypeNum = typeNum;
            ProgramNum = programNum;

            // Construct full path
            FullPath = string.Format(@"{0}\{1}\Type{2:D3}\P{3:D3}", Path.GetFullPath(folderPath), Station, TypeNum, ProgramNum);
        }

        #region Public methods
        /// <summary>
        /// Prepares the image logger.
        /// </summary>
        /// <exception cref="IOException"></exception>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public override void Prepare()
        {
            if (!IsPrepared)
            {
                // Create the directories
                Directory.CreateDirectory(Path.Combine(FullPath, "OK"));
                Directory.CreateDirectory(Path.Combine(FullPath, "NOK"));

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
            List<FileInfo> ImagesToDelete;
            string ChoosenDirectory;
            string ConstructedFileName;

            // Consume data
            while (!ImageLogEntries.IsCompleted)
            {
                // Check if operation has been canceled
                if (TokenSource.Token.IsCancellationRequested)
                    break;

                if (ImageLogEntries.TryTake(out ImageLogEntry LogEntry, 50))
                {
                    // Choose the image directory
                    if (LogEntry.StatusOk)
                        ChoosenDirectory = "OK";
                    else
                        ChoosenDirectory = "NOK";

                    // Get images to delete. Delete them if any
                    ImagesToDelete = new DirectoryInfo(Path.Combine(FullPath, ChoosenDirectory)).EnumerateFiles()
                                                                                                .OrderByDescending(x => x.CreationTime)
                                                                                                .Skip(MaxNumberOfImages -1)
                                                                                                .ToList();

                    ImagesToDelete.ForEach(x => x.Delete());

                    // Add new image to the directory and fire the event
                    ConstructedFileName = $"{LogEntry.TimeOfOccurance.ToString("yyyyMMdd")}_{LogEntry.TimeOfOccurance.ToString("HHmmss")}_{LogEntry.TimeOfOccurance.ToString("fff")}_I{LogEntry.ImageID.ToString("00")}{FileExtention}";
                    LogEntry.Img?.Save(Path.Combine(FullPath, ChoosenDirectory, ConstructedFileName));

                    OnImageLogAddedEvent(LogEntry);
                }
            }
        }
        #endregion
    }
}
