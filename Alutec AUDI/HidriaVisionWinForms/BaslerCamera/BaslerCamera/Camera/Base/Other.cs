using System;
using System.Collections.Generic;
using System.Drawing;

namespace BaslerCam
{
    /// <summary>
    /// Represents custom event arguments for camera image grab event
    /// </summary>
    public class ImageReadyEventArgs : EventArgs
    {
        /// <summary>
        /// Gets grabbed image
        /// </summary>
        public Bitmap GrabbedImage { get; }

        /// <summary>
        /// Initializes new instance of ImageReadyEventArgs
        /// </summary>
        /// <param name="image">Grabbed Bitmap image</param>
        public ImageReadyEventArgs(Bitmap image)
        {
            GrabbedImage = image;
        }
    }

    /// <summary>
    /// Represents custom event arguments for multiple images grab event
    /// </summary>
    /// 
    public class ImagesReadyEventArgs : EventArgs
    {
        /// <summary>
        /// Gets grabbed image
        /// </summary>
        public List<Bitmap> GrabbedImages { get; }

        /// <summary>
        /// Initializes new instance of ImageReadyEventArgs
        /// </summary>
        /// <param name="image">Grabbed Bitmap image</param>
        public ImagesReadyEventArgs(List<Bitmap> images)
        {
            GrabbedImages = images;
        }
    }

    public struct OverlayPosition
    {
        /// <summary>
        /// Overlay cross position X
        /// </summary>
        public int CrossPosX;

        /// <summary>
        /// Overlay cross position Y
        /// </summary>
        public int CrossPosY;

        /// <summary>
        /// Initializes new instance of OverlayPosition
        /// </summary>
        /// <param name="crossPosX">Overlay cross position X</param>
        /// <param name="crossPosY">Overlay cross position Y</param>
        public OverlayPosition(int crossPosX, int crossPosY)
        {
            CrossPosX = crossPosX;
            CrossPosY = crossPosY;
        }
    }
}
