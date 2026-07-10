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
        #endregion

        public ImageLogEntry(Bitmap img, DateTime timeOfOccurance, int imageID, bool statusOk)
        {
            Img = img;
            TimeOfOccurance = timeOfOccurance;
            ImageID = imageID;
            StatusOk = statusOk;
        }
    }
}
