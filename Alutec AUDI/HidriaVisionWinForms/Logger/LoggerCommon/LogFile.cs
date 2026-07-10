using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

[assembly: InternalsVisibleTo("Logger")]
[assembly: InternalsVisibleTo("ResultsLogger")]
namespace Logger
{
    /// <summary>
    /// This structure represents log file.
    /// </summary>
    internal struct LogFile
    {
        #region Public fields
        public readonly DateTime LogDate;
        public readonly int LogIndex;
        public readonly string FileExtention;
        public readonly string FolderPath;
        #endregion

        public LogFile(DateTime logDate, int logIndex, string filePath, string fileExtention)
        {
            LogDate = logDate;
            LogIndex = logIndex;
            FolderPath = filePath;
            FileExtention = fileExtention;
        }

        #region Properties
        /// <summary>
        /// Gets full log file path with name
        /// </summary>
        public string FullLogFilePath
        {
            get
            {
                return string.Format(@"{0}\{1:yyyy_MM_dd}-{2}.{3}", FolderPath, LogDate, LogIndex, FileExtention);
            }
        }
        #endregion

        #region Static methods
        /// <summary>
        /// Gets the last log file in a folder from the selected date
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static LogFile? GetLastLogFile(string path, string fileExtention, DateTime date)
        {
            Regex Reg = new Regex(@"^([12]\d{3}_(0[1-9]|1[0-2])_(0[1-9]|[12]\d|3[01]))-([1-9][0-9]{0,2}|10000)\.[a-zA-Z0-9]+$");
            string CurrentDate = DateTime.Now.ToString("yyyy_MM_dd");

            // Get log file in a directory
            var FileName = Directory.GetFiles(path)
                        .Select(x => Path.GetFileName(x))
                        .Where(x => Reg.IsMatch(x) && x.StartsWith(CurrentDate) && x.EndsWith(fileExtention))
                        .Select(x => x.Split(new[] { '-', '.' }))
                        .OrderByDescending(x => int.Parse(x[1]))
                        .FirstOrDefault();

            if (FileName?.Length > 0)
                return new LogFile(DateTime.ParseExact(FileName[0], "yyyy_MM_dd", CultureInfo.InvariantCulture), int.Parse(FileName[1]), path, FileName[2]);

            return null;
        }
        #endregion
    }
}
