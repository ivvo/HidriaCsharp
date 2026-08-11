using System;
using System.Drawing;

namespace Logger.ImageLogger
{
    /// <summary>
    /// This structure represents an image log entry.
    /// </summary>
    public struct ImageLogEntry
    {
        #region Public fields
        public readonly Bitmap Img;
        public readonly DateTime TimeOfOccurance;
        public readonly int ImageID;
        public readonly bool StatusOk;
        /// <summary>
        /// Optional custom base name for the saved file (e.g. DMC code).
        /// When null or empty, the logger falls back to the timestamp-based name.
        /// </summary>
        public readonly string CustomName;
        #endregion

        public ImageLogEntry(Bitmap img, DateTime timeOfOccurance, int imageID, bool statusOk)
            : this(img, timeOfOccurance, imageID, statusOk, null)
        {
        }

        public ImageLogEntry(Bitmap img, DateTime timeOfOccurance, int imageID, bool statusOk, string customName)
        {
            Img = img;
            TimeOfOccurance = timeOfOccurance;
            ImageID = imageID;
            StatusOk = statusOk;
            CustomName = customName;
        }
    }
}
