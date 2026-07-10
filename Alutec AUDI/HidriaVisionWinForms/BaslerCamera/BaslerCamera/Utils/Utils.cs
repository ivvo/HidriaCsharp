using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using WPFMediaImaging = System.Windows.Media.Imaging;
using WPFMedia = System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using Basler.Pylon;

namespace BaslerCam.Utils
{
    /// <summary>
    /// Static helper class for basler camera
    /// </summary>
    public static class CameraUtils
    {
        /// <summary>
        /// Gets the available cameras
        /// </summary>
        /// <returns>Returns available cameras</returns>
        public static IEnumerable<string> GetAvailableCameras()
        {
            foreach (ICameraInfo CamInfo in CameraFinder.Enumerate())
                yield return CamInfo[CameraInfoKey.SerialNumber];
        }

        /// <summary>
        /// Checks if the camera with the serial number exists
        /// </summary>
        /// <param name="serialNumber">Camera serial number</param>
        /// <returns>Returns the information if the camera exists</returns>
        public static bool IsCameraAvailable(string serialNumber)
        {
            foreach (string SerialNumber in GetAvailableCameras())
                if (SerialNumber == serialNumber)
                    return true;
            return false;
        }
    }

    /// <summary>
    /// Static helper class for bitmap manipulation
    /// </summary>
    public static class ImageUtils
    {
        [DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
        public static extern void CopyMemory(IntPtr Destination, IntPtr Source, int Length);

        /// <summary>
        /// Checks if writeable bitmap image is compatible with the bitmap image
        /// </summary>
        /// <param name="writeableBitmap">Writeable bitmap image</param>
        /// <param name="bitmap">Bitmap image</param>
        /// <returns>Returns the information if the writeable bitmap image is compatible with bitmap image</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static bool IsWriteableBitmapCompatible(WriteableBitmap writeableBitmap, Bitmap bitmap)
        {
            if (writeableBitmap == null || bitmap == null)
                throw new ArgumentNullException("Bitmap or writeablebitmap is null");
            else if (writeableBitmap.Height != bitmap.Height || writeableBitmap.Width != bitmap.Width)
                return false;
            return true;
        }

        /// <summary>
        /// Creates new writeable bitmap
        /// </summary>
        /// <param name="width">Width of the image</param>
        /// <param name="height">Height of the image</param>
        /// <returns></returns>
        public static WriteableBitmap CreateWriteableBitmap(int width, int height)
        {
            // Create a pallet
            BitmapPalette Pallete = new WPFMediaImaging.BitmapPalette(new List<WPFMedia.Color> { WPFMedia.Colors.Red, WPFMedia.Colors.Blue, WPFMedia.Colors.Green });

            // Return new writeable bitmap
            return new WPFMediaImaging.WriteableBitmap(width, height, 96, 96, WPFMedia.PixelFormats.Bgr32, Pallete);
        }

        /// <summary>
        /// Updates existing writeable bitmap
        /// </summary>
        /// <param name="writeableBitmap">Writeable bitmap image</param>
        /// <param name="bitmap">Bitmap image</param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void UpdateWritableBitmap(ref WPFMediaImaging.WriteableBitmap writeableBitmap, Bitmap bitmap)
        {
            // Check if the WriteableBitmap can be updated with bitmap data
            if (!IsWriteableBitmapCompatible(writeableBitmap, bitmap))
            {
                throw new InvalidOperationException("Cannot update incompatible Writeable bitmap!");
            }

            BitmapData BmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

            // Lock writeable bitmap back buffer
            writeableBitmap.Lock();

            // Copy the bitmap's data directly to the on-screen buffers
            CopyMemory(writeableBitmap.BackBuffer, BmpData.Scan0, BmpData.Stride * bitmap.Height);

            // Moves the back buffer to the front.
            writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, bitmap.Width, bitmap.Height));

            // Unlock back buffer
            writeableBitmap.Unlock();

            bitmap.UnlockBits(BmpData);
        }

        /// <summary>
        /// Adds overlay to the bitmap image
        /// </summary>
        /// <param name="overlayPos">Position of the overlay</param>
        /// <param name="FPS">Frames per second</param>
        /// <param name="imageOrientation">Image orientation</param>
        /// <param name="bitmap">Bitmap image</param>
        public static void AddOverlay(OverlayPosition overlayPos, double FPS, ImageOrientation imageOrientation, ref Bitmap bitmap)
        {
            using (Font Font = new Font("Arial", 24.0f, System.Drawing.FontStyle.Regular))
            using (Pen RedPen = new Pen(Color.Red))
            using (Graphics Overlay = Graphics.FromImage(bitmap))
            {
                // Set pen width
                RedPen.Width = 4;

                // Change the origin of the coordinate system
                Overlay.TranslateTransform((float)bitmap.Width / 2, (float)bitmap.Height / 2);
                // Rotate 
                Overlay.RotateTransform(360 - (int)imageOrientation);
                // Move the origin to the original position
                if (imageOrientation == ImageOrientation.Orientation_0 || imageOrientation == ImageOrientation.Orientation_180)
                    Overlay.TranslateTransform(-(float)bitmap.Width / 2, -(float)bitmap.Height / 2);
                else
                    Overlay.TranslateTransform(-(float)bitmap.Height / 2, -(float)bitmap.Width / 2);

                // Draw strings
                Overlay.DrawString("FPS: " + FPS.ToString("0.00"), Font, RedPen.Brush, 10, 30);

                // Draw cross
                if (imageOrientation == ImageOrientation.Orientation_0)
                {
                    Overlay.DrawLine(RedPen, overlayPos.CrossPosX, 0, overlayPos.CrossPosX, bitmap.Height);
                    Overlay.DrawLine(RedPen, 0, overlayPos.CrossPosY, bitmap.Width, overlayPos.CrossPosY);
                    Overlay.DrawEllipse(RedPen, overlayPos.CrossPosX - 10, overlayPos.CrossPosY - 10, 20, 20);
                }
                else if (imageOrientation == ImageOrientation.Orientation_90)
                {
                    Overlay.DrawLine(RedPen, 0, overlayPos.CrossPosX, bitmap.Height, overlayPos.CrossPosX);
                    Overlay.DrawLine(RedPen, bitmap.Height - overlayPos.CrossPosY, 0, bitmap.Height - overlayPos.CrossPosY, bitmap.Width);
                    Overlay.DrawEllipse(RedPen, bitmap.Height - overlayPos.CrossPosY - 10, overlayPos.CrossPosX - 10, 20, 20);
                }
                else if (imageOrientation == ImageOrientation.Orientation_180)
                {
                    Overlay.DrawLine(RedPen, bitmap.Width - overlayPos.CrossPosX, 0, bitmap.Width - overlayPos.CrossPosX, bitmap.Height);
                    Overlay.DrawLine(RedPen, 0, bitmap.Height - overlayPos.CrossPosY, bitmap.Width, bitmap.Height - overlayPos.CrossPosY);
                    Overlay.DrawEllipse(RedPen, bitmap.Width - overlayPos.CrossPosX - 10, bitmap.Height - overlayPos.CrossPosY - 10, 20, 20);
                }
                else
                {
                    Overlay.DrawLine(RedPen, 0, bitmap.Width - overlayPos.CrossPosX, bitmap.Height, bitmap.Width - overlayPos.CrossPosX);
                    Overlay.DrawLine(RedPen, overlayPos.CrossPosY, 0, overlayPos.CrossPosY, bitmap.Width);
                    Overlay.DrawEllipse(RedPen, overlayPos.CrossPosY - 10, bitmap.Width - overlayPos.CrossPosX - 10, 20, 20);
                }
            }
        }
    }
}
