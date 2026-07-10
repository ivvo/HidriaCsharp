using System;
using System.IO;

namespace ResultsView
{
    /// <summary>
    /// This structure represents image source.
    /// </summary>
    public struct ImageSource
    {
        #region Public fields
        public readonly string ImagePath;
        #endregion

        /// <summary>
        /// Constructs structure of type ImageSource.
        /// </summary>
        /// <param name="imagePath"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="System.Security.SecurityException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="PathTooLongException"></exception>
        public ImageSource(string imagePath)
        {
            ImagePath = Path.GetFullPath(imagePath);
        }
    }
}
