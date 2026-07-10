using System;

namespace Logger.ImageLogger
{
    /// <summary>
    /// Represents custom event arguments for new added image event.
    /// </summary>
    public class ImageLogAddedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets image log entry.
        /// </summary>
        public ImageLogEntry ImageEntry { get; }

        /// <summary>
        /// Initializes new instance of ImageLogAddedEventArgs.
        /// </summary>
        /// <param name="imageEntry">Image log entry.</param>
        public ImageLogAddedEventArgs(ImageLogEntry imageEntry)
        {
            ImageEntry = imageEntry;
        }
    }
}
