using System;
using System.Collections.Generic;
using System.Drawing;
using Hik.Communication.ScsServices.Service;

namespace BaslerCam.Service
{
    /// <summary>
    /// Represents basler camera service
    /// </summary>
    public class BaslerCameraService : ScsService, IBaslerCameraService
    {
        #region Private fields
        private bool _ImageReady;
        private bool _MultipleGrabImagesReady;
        private bool _CameraDisconnected;
        private bool _CameraGrabError;
        private Bitmap _GrabbedImage;
        private List<Bitmap> _MultipleGrabImages;
        private IBaslerCamera Cam;
        private object LockObj;
        #endregion

        #region Properties
        /// <summary>
        /// Gets the information about the image availability.
        /// </summary>
        public bool ImageReady
        {
            get
            {
                lock(LockObj)
                    return _ImageReady;
            }
        }

        /// <summary>
        /// Gets the information about the availability of the images of the multi grab
        /// </summary>
        public bool MultipleGrabImageReady
        {
            get
            {
                lock (LockObj)
                    return _MultipleGrabImagesReady;
            }
        }

        /// <summary>
        /// Gets the information about if the camera has lost its connection.
        /// </summary>
        public bool CameraDisconnected
        {
            get
            {
                lock(LockObj)
                    return _CameraDisconnected;
            }
        }

        /// <summary>
        /// Gets the information about if the camera grab error occured.
        /// </summary>
        public bool CameraGrabError
        {
            get
            {
                lock(LockObj)
                    return _CameraGrabError;
            }
        }

        /// <summary>
        /// Gets the last grabbed image if available.
        /// </summary>
        public Bitmap GrabbedImage
        {
            get
            {
                lock (LockObj)
                {
                    if (_ImageReady)
                    {
                        _ImageReady = false;
                        return _GrabbedImage;
                    }
                }

                return null;
            }
        }

        /// <summary>
        /// Gets the last images of the multi grab
        /// </summary>
        public List<Bitmap> MultipleGrabImages
        {
            get
            {
                lock (LockObj)
                {
                    if (_MultipleGrabImagesReady)
                    {
                        _MultipleGrabImagesReady = false;
                        return _MultipleGrabImages;
                    }
                }

                return null;
            }
        }
        #endregion

        /// <summary>
        /// Initializes new instance of the camera service
        /// </summary>
        /// <param name="cam"></param>
        public BaslerCameraService(IBaslerCamera cam)
        {
            Cam = cam;
            LockObj = new object();

            // Register the events
            Cam.OneShotImageReadyEvent += OnImageReady;
            Cam.MultipleShotImagesReadyEvent += OnMultipleGrabImagesReady;
            Cam.CameraGrabErrorEvent += OnCameraError;
            Cam.CameraDisconnectedEvent += OnCameraDisconnected;
        }

        #region Public methods
        /// <summary>
        /// This method is used to open the camera.
        /// </summary>
        /// <param name="serialNumber">Camera serial number</param>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void CameraOpen(string serialNumber)
        {
            Cam.CameraOpen(serialNumber);
        }

        /// <summary>
        /// This method is used to close the camera.
        /// </summary>
        public void CameraClose()
        {
            // Close the camera
            Cam.CameraClose();

            // Reset flags
            _ImageReady = false;
            _MultipleGrabImagesReady = false;
            _CameraGrabError = false;
            _CameraDisconnected = false;
        }

        /// <summary>
        /// This method is used to start the one shot image grab.
        /// </summary>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void OneShot()
        {
            Cam.OneShot();
        }

        /// <summary>
        /// This method is used to start the multiple images grab.
        /// </summary>
        /// <param name="numOfImages">Number of images to be grabbed</param>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void MultipleShot(int numOfImages)
        {
            Cam.MultipleShot(numOfImages);
        }

        /// <summary>
        /// This method is used to start continuous shot image grab.
        /// </summary>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void ContinuousShot()
        {
            Cam.ContinuousShot();
        }

        /// <summary>
        /// This method is used to stop the continuous shot image grab.
        /// </summary>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void StopGrab()
        {
            Cam.StopGrab();
        }

        /// <summary>
        /// This method is used to start the grabbing procedure of one image during continuous shot.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void GrabImageDuringContinuousShot()
        {
            Cam.GrabImageDuringContinuousShot();
        }

        /// <summary>
        /// This method is called to set the width of the image.
        /// </summary>
        /// <param name="width">Width of the image in pixels</param>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SetWidth(long width)
        {
            Cam.SetWidth(width);
        }

        /// <summary>
        /// This method is called to get the width of the image.
        /// </summary>
        /// <returns>Width of the image in pixels</returns>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public long GetWidth()
        {
            return Cam.GetWidth();
        }

        /// <summary>
        /// This method is called to set the height of the image.
        /// </summary>
        /// <param name="height">Height of the image in pixels</param>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SetHeight(long height)
        {
            Cam.SetHeight(height);
        }

        /// <summary>
        /// This method is called to get the height of the image.
        /// </summary>
        /// <returns>Returns the height of the image in pixels</returns>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public long GetHeight()
        {
            return Cam.GetHeight();
        }

        /// <summary>
        /// This method is called to set the offset in X direction.
        /// </summary>
        /// <param name="xOffset">Offset in X direction in pixels</param>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SetXOffset(long xOffset)
        {
            Cam.SetXOffset(xOffset);
        }

        /// <summary>
        /// This method is called to get the offset in X direction.
        /// </summary>
        /// <returns>Returns the offset in X direction in pixels</returns>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public long GetXOffset()
        {
            return Cam.GetXOffset();
        }

        /// <summary>
        /// This method is called to set the offset in Y direction.
        /// </summary>
        /// <param name="yOffset">Offset in Y direction in pixels</param>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SetYOffset(long yOffset)
        {
            Cam.SetYOffset(yOffset);
        }

        /// <summary>
        /// This method is called to get the offset in Y direction.
        /// </summary>
        /// <returns>Returns the offset in Y direction in pixels</returns>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public long GetYOffset()
        {
            return Cam.GetYOffset();
        }

        /// <summary>
        /// This method is called to set the exposure of the camera.
        /// </summary>
        /// <param name="exposure">Exposure of the camera in raw format</param>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SetExposure(long exposure)
        {
            Cam.SetExposure(exposure);
        }

        /// <summary>
        /// This method is called to get the exposure of the camera.
        /// </summary>
        /// <returns>Returns the exposure of the camera in raw format</returns>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public long GetExposure()
        {
            return Cam.GetExposure();
        }

        /// <summary>
        /// This method is called to set the autoexposure of the camera.
        /// </summary>
        /// <param name="enable">Autoexposure enabled or disabled<</param>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SetAutoExposure(bool enable)
        {
            Cam.SetAutoExposure(enable);
        }

        /// <summary>
        /// This method is called to get the camera autoexposure status.
        /// </summary>
        /// <returns>Returns camera autoexposure status</returns>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public bool AutoExposureSet()
        {
            return Cam.AutoExposureSet();
        }

        /// <summary>
        /// This method is called to set the framerate of the camera.
        /// </summary>
        /// <param name="frameRate">Framerate of the camera</param>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SetFrameRate(int frameRate)
        {
            Cam.SetFrameRate(frameRate);
        }

        /// <summary>
        /// This method is called to get the current framerate of the camera.
        /// </summary>
        /// <returns>Returns the current framerate of the camera</returns>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public int GetFrameRate()
        {
            return Cam.GetFrameRate();
        }

        /// <summary>
        /// This method is called to set the userset.
        /// </summary>
        /// <param name="userSet">Userset enum</param>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void ApplyUserSet(ConfigurationSetSelector userSet)
        {
            Cam.ApplyUserSet(userSet);
        }

        /// <summary>
        /// This method is called to get the current camera userset
        /// </summary>
        /// <returns>Current camera userset</returns>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public ConfigurationSetSelector GetAppliedUserSet()
        {
            return Cam.GetAppliedUserSet();
        }
        #endregion

        #region Events
        private void OnImageReady(object sender, ImageReadyEventArgs e)
        {
            lock (LockObj)
            {
                _GrabbedImage = e.GrabbedImage;
                _ImageReady = true;
            }
        }

        private void OnMultipleGrabImagesReady(object sender, ImagesReadyEventArgs e)
        {
            lock (LockObj)
            {
                _MultipleGrabImages = e.GrabbedImages;
                _MultipleGrabImagesReady = true;
            }
        }

        private void OnCameraError(object sender, EventArgs e)
        {
            lock(LockObj)
                _CameraGrabError = true;
        }

        private void OnCameraDisconnected(object sender, EventArgs e)
        {
            lock(LockObj)
                _CameraDisconnected = true;
        }
        #endregion
    }
}
