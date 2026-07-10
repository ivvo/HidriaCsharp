using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using Basler.Pylon;

namespace BaslerCam
{
    /// <summary>
    /// Represents Basler mono or color camera
    /// </summary>
    public class BaslerCamera : IBaslerCamera, IBaslerCamContinuousGrab, IDisposable
    {
        #region Events
        /// <summary>
        /// Event triggers when the image is ready after calling the OneShot method.
        /// </summary>
        public event EventHandler<ImageReadyEventArgs> OneShotImageReadyEvent;

        /// <summary>
        /// Event triggers when the multiple images are ready after calling MultipleShot method
        /// </summary>
        public event EventHandler<ImagesReadyEventArgs> MultipleShotImagesReadyEvent;

        /// <summary>
        /// Event triggers when image is ready after calling ContinuousShot method.
        /// </summary>
        public event EventHandler<ImageReadyEventArgs> ContinuousShotImageReadyEvent;

        /// <summary>
        /// Event triggers if there is camera grab error.
        /// </summary>
        public event EventHandler<EventArgs> CameraGrabErrorEvent;

        /// <summary>
        /// Event triggers when camera disconnects.
        /// </summary>
        public event EventHandler<EventArgs> CameraDisconnectedEvent;
        #endregion

        #region Private and protected fields
        protected Camera Cam;
        protected PixelDataConverter Converter;
        protected bool DisposedValue;
        private object LockObj;
        private bool OneShotActive;
        private bool MultipleShotActive;
        private bool ContinuousShotActive;
        private bool GrabImageContinuousShot;
        private int NumOfImagesToGrab;
        private int NumOfImagesAlreadyGrabbed;
        private List<Bitmap> MultipleGrabImages;
        #endregion

        /// <summary>
        /// Initializes new instance of the Basler camera.
        /// </summary>
        public BaslerCamera()
        {
            Cam = null;
            MultipleGrabImages = null;
            Converter = new PixelDataConverter();
            LockObj = new object();
            OneShotActive = false;
            MultipleShotActive = false;
            ContinuousShotActive = false;
            GrabImageContinuousShot = false;
            DisposedValue = false;
            NumOfImagesToGrab = 0;
            NumOfImagesAlreadyGrabbed = 0;
        }

        #region Private and protected methods
        /// <summary>
        /// Clears grabbed images
        /// </summary>
        private void ClearGrabbedImages()
        {
            // Dispose all grabbed images(if there are any) during the Multiple shot.
            if (MultipleGrabImages != null)
                foreach (Bitmap GrabbedImage in MultipleGrabImages)
                    GrabbedImage.Dispose();
        }

        /// <summary>
        /// Disposes the object
        /// </summary>
        /// <param name="disposing">Disposing</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!DisposedValue)
            {
                if (disposing)
                {
                    if(Cam != null)
                    {
                        try
                        {
                            // Close and dispose the camera object
                            Cam.Close();
                            Cam.Dispose();

                            // Clear images from the list
                            ClearGrabbedImages();
                        }
                        finally
                        {
                            Cam = null;
                        }
                    }
                }

                DisposedValue = true;
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Opens the camera.
        /// </summary>
        /// <param name="serialNumber">Camera serial number</param>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void CameraOpen(string serialNumber)
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if the connection to the camera has already been made
            if (Cam != null)
                throw new InvalidOperationException("Connection to the camera needs to be closed before opening the new one!");

            try
            {
                Cam = new Camera(serialNumber);

                // Open the connection to the camera
                Cam.Open();

                // Set camera timeout to 1000ms. This is used to detect when the camera is disconnected
                Cam.Parameters[PLTransportLayer.HeartbeatTimeout].TrySetValue(1000, IntegerValueCorrection.Nearest);

                // Register the events of the image provider
                Cam.ConnectionLost += OnConnectionLost;
                Cam.StreamGrabber.ImageGrabbed += OnImageGrabbed;
                Cam.StreamGrabber.GrabStopped += OnGrabStopped;
            }
            catch (Exception ex)
            {
                throw new CameraException("Camera cannot be opened!", ex);
            }
        }

        /// <summary>
        /// Closes the camera.
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        public void CameraClose()
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Close the camera
            if (Cam != null)
            {
                // Unregister the events
                Cam.ConnectionLost -= OnConnectionLost;
                Cam.StreamGrabber.ImageGrabbed -= OnImageGrabbed;
                Cam.StreamGrabber.GrabStopped -= OnGrabStopped;

                try
                {
                    Cam.Close();
                    Cam.Dispose();
                }
                finally
                {
                    Cam = null;
                }
            }

            // Clear grabbed images
            ClearGrabbedImages();

            // Reset the flags
            GrabImageContinuousShot = false;
            ContinuousShotActive = false;
            OneShotActive = false;
            MultipleShotActive = false;
        }

        /// <summary>
        /// Starts the one shot image grab.
        /// </summary>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void OneShot()
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if camera is opened before starting the one frame grabbing
            if (Cam == null || !Cam.IsOpen || !Cam.IsConnected)
                throw new InvalidOperationException("Camera not opened or connected!");

            lock (LockObj)
            {
                // Check if continuous shot is off
                if (!ContinuousShotActive)
                {
                    // Check if multiple shot is off
                    if (!MultipleShotActive)
                    {
                        if (!OneShotActive)
                        {
                            try
                            {
                                // Starts the grabbing of one image.
                                Cam.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.SingleFrame);
                                Cam.StreamGrabber.Start(1, GrabStrategy.LatestImages, GrabLoop.ProvidedByStreamGrabber);

                                OneShotActive = true;
                            }
                            catch (Exception ex)
                            {
                                throw new CameraException("Cannot start one frame grabbing!", ex);
                            }
                        }
                    }
                    else
                        throw new InvalidOperationException("Multiple shot is active!");
                }
                else
                    throw new InvalidOperationException("Continuous shot is active!");
            }
        }

        /// <summary>
        /// Starts the multiple images grab.
        /// </summary>
        /// <param name="numOfImages">Number of images to be grabbed</param>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void MultipleShot(int numOfImages)
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if camera is opened before starting the multiple frame grabbing
            if (Cam == null || !Cam.IsOpen || !Cam.IsConnected)
                throw new InvalidOperationException("Camera not opened or connected!");

            lock (LockObj)
            {
                // Check if one shot is off
                if (!OneShotActive)
                {
                    // Check if continuous shot is off
                    if (!ContinuousShotActive)
                    {
                        if (!MultipleShotActive)
                        {
                            try
                            {
                                // Save the number of images to be grabbed into a local variable. This information is needed inside the image grab camera callback
                                NumOfImagesToGrab = numOfImages;

                                NumOfImagesAlreadyGrabbed = 0;

                                // Create empty list for grabbed images
                                MultipleGrabImages = new List<Bitmap>();

                                // Starts the grabbing of multiple images
                                Cam.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.Continuous);
                                Cam.StreamGrabber.Start(numOfImages, GrabStrategy.OneByOne, GrabLoop.ProvidedByStreamGrabber);
                                //Cam.StreamGrabber.Start(numOfImages, GrabStrategy.LatestImages, GrabLoop.ProvidedByStreamGrabber);

                                MultipleShotActive = true;
                            }
                            catch (Exception ex)
                            {
                                throw new CameraException("Cannot start multiple frame grabbing!", ex);
                            }
                        }
                    }
                    else
                        throw new InvalidOperationException("Continuous shot is active!");
                }
                else
                    throw new InvalidOperationException("One shot is active!");
            }
        }

        /// <summary>
        /// Starts the continuous shot image grab.
        /// </summary>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void ContinuousShot()
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if camera is opened before starting the continuous frame grabbing
            if (Cam == null || !Cam.IsOpen || !Cam.IsConnected)
                throw new InvalidOperationException("Camera not opened or connected!");

            lock (LockObj)
            {
                // Check if one shot is off
                if (!OneShotActive)
                {
                    // Check if multiple shot is off
                    if (!MultipleShotActive)
                    {
                        if (!ContinuousShotActive)
                        {
                            try
                            {
                                // Start the grabbing of images until grabbing is stopped.
                                Cam.Parameters[PLCamera.AcquisitionMode].SetValue(PLCamera.AcquisitionMode.Continuous);
                                Cam.StreamGrabber.Start(GrabStrategy.LatestImages, GrabLoop.ProvidedByStreamGrabber);

                                ContinuousShotActive = true;
                            }
                            catch (Exception ex)
                            {
                                throw new CameraException("Cannot start continuous shot!", ex);
                            }
                        }
                    }
                    else
                        throw new InvalidOperationException("Multiple shot is active!");
                }
                else
                    throw new InvalidOperationException("One shot is active!");
            }
        }

        /// <summary>
        /// Stops the image grabbing.
        /// </summary>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void StopGrab()
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if camera is opened before stopping the frame grabbing
            if (Cam == null || !Cam.IsOpen || !Cam.IsConnected)
                throw new InvalidOperationException("Camera not opened or connected!");

            // Stop the grabbing
            try
            {
                Cam.StreamGrabber.Stop();
            }
            catch (Exception ex)
            {
                throw new CameraException("Cannot stop image grabbing!", ex);
            }
            finally
            {
                // Reset the flags
                GrabImageContinuousShot = false;
                ContinuousShotActive = false;
                OneShotActive = false;
                MultipleShotActive = false;

                // Clear images from the list
                ClearGrabbedImages();
            }
        }

        /// <summary>
        /// Starts the grabbing procedure of one image during continuous shot.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void GrabImageDuringContinuousShot()
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if camera is opened before stopping the continuous frame grabbing
            if (Cam == null || !Cam.IsOpen || !Cam.IsConnected)
                throw new InvalidOperationException("Camera not opened or connected!");

            lock (LockObj)
            {
                if (ContinuousShotActive)
                    GrabImageContinuousShot = true;
                else
                    throw new InvalidOperationException("Continuous shot not active!");
            }
        }

        /// <summary>
        /// Sets width of the image.
        /// </summary>
        /// <param name="width">Width of the image in pixels</param>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SetWidth(long width)
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if camera is opened before setting the image width
            if (Cam == null || !Cam.IsOpen || !Cam.IsConnected)
                throw new InvalidOperationException("Camera not opened or connected!");

            try
            {
                long Min = Cam.Parameters[PLCamera.Width].GetMinimum();
                long Max = Cam.Parameters[PLCamera.Width].GetMaximum();

                // Check if width is within the range and set the value
                if (width < Min)
                    Cam.Parameters[PLCamera.Width].SetValue(Min);
                else if(width > Max)
                    Cam.Parameters[PLCamera.Width].SetValue(Max);
                else
                    Cam.Parameters[PLCamera.Width].SetValue(width);
            }
            catch (Exception ex)
            {
                throw new CameraException("Cannot set image width!", ex);
            }
        }

        /// <summary>
        /// Gets the with of the image.
        /// </summary>
        /// <returns>Returns width of the image in pixels</returns>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public long GetWidth()
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if camera is opened before getting the image width
            if (Cam == null || !Cam.IsOpen || !Cam.IsConnected)
                throw new InvalidOperationException("Camera not opened or connected!");

            try
            {
                return Cam.Parameters[PLCamera.Width].GetValue();
            }
            catch (Exception ex)
            {
                throw new CameraException("Cannot get image width!", ex);
            }
        }

        /// <summary>
        /// Sets height of the image.
        /// </summary>
        /// <param name="height">Height of the image in pixels</param>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SetHeight(long height)
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if camera is opened before setting the image height
            if (Cam == null || !Cam.IsOpen || !Cam.IsConnected)
                throw new InvalidOperationException("Camera not opened or connected!");

            try
            {
                long Min = Cam.Parameters[PLCamera.Height].GetMinimum();
                long Max = Cam.Parameters[PLCamera.Height].GetMaximum();

                // Check if height is within the range and set the value
                if (height < Min)
                    Cam.Parameters[PLCamera.Height].SetValue(Min);
                else if(height > Max)
                    Cam.Parameters[PLCamera.Height].SetValue(Max);
                else
                    Cam.Parameters[PLCamera.Height].SetValue(height);
            }
            catch (Exception ex)
            {
                throw new CameraException("Cannot set image height!", ex);
            }
        }

        /// <summary>
        /// Gets the height of the image.
        /// </summary>
        /// <returns>Returns the height of the image in pixels</returns>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public long GetHeight()
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if camera is opened before getting the image height
            if (Cam == null || !Cam.IsOpen || !Cam.IsConnected)
                throw new InvalidOperationException("Camera not opened or connected!");

            try
            {
                return Cam.Parameters[PLCamera.Height].GetValue();
            }
            catch (Exception ex)
            {
                throw new CameraException("Cannot get image height!", ex);
            }
        }

        /// <summary>
        /// Sets the offset in X direction.
        /// </summary>
        /// <param name="xOffset">Offset in X direction in pixels</param>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SetXOffset(long xOffset)
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if camera is opened before setting the x Offset
            if (Cam == null || !Cam.IsOpen || !Cam.IsConnected)
                throw new InvalidOperationException("Camera not opened or connected!");

            try
            {
                long Min = Cam.Parameters[PLCamera.OffsetX].GetMinimum();
                long Max = Cam.Parameters[PLCamera.OffsetX].GetMaximum();

                // Check if xoffset is within the range and set the value
                if (xOffset < Min)
                    Cam.Parameters[PLCamera.OffsetX].SetValue(Min);
                else if(xOffset > Max)
                    Cam.Parameters[PLCamera.OffsetX].SetValue(Max);
                else
                    Cam.Parameters[PLCamera.OffsetX].SetValue(xOffset);
            }
            catch (Exception ex)
            {
                throw new CameraException("Cannot set x offset!", ex);
            }
        }

        /// <summary>
        /// Gets the offset in X direction.
        /// </summary>
        /// <returns>Returns the offset in X direction in pixels</returns>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public long GetXOffset()
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if camera is opened before getting the x Offset
            if (Cam == null || !Cam.IsOpen || !Cam.IsConnected)
                throw new InvalidOperationException("Camera not opened or connected!");

            try
            {
                return Cam.Parameters[PLCamera.OffsetX].GetValue();
            }
            catch (Exception ex)
            {
                throw new CameraException("Cannot get x offset!", ex);
            }
        }

        /// <summary>
        /// Sets offset in Y direction.
        /// </summary>
        /// <param name="yOffset">Offset in Y direction in pixels</param>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SetYOffset(long yOffset)
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if camera is opened before setting the y offset
            if (Cam == null || !Cam.IsOpen || !Cam.IsConnected)
                throw new InvalidOperationException("Camera not opened or connected!");

            try
            {
                long Min = Cam.Parameters[PLCamera.OffsetY].GetMinimum();
                long Max = Cam.Parameters[PLCamera.OffsetY].GetMaximum();

                // Check if yoffset is within the range and set the value
                if (yOffset < Min )
                    Cam.Parameters[PLCamera.OffsetY].SetValue(Min);
                else if(yOffset > Max)
                    Cam.Parameters[PLCamera.OffsetY].SetValue(Max);
                else
                    Cam.Parameters[PLCamera.OffsetY].SetValue(yOffset);
            }
            catch (Exception ex)
            {
                throw new CameraException("Cannot set y offset", ex);
            }
        }

        /// <summary>
        /// Gets the offset in Y direction.
        /// </summary>
        /// <returns>Returns the offset in Y direction in pixels</returns>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public long GetYOffset()
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if camera is opened before getting the y offset
            if (Cam == null || !Cam.IsOpen || !Cam.IsConnected)
                throw new InvalidOperationException("Camera not opened or connected!");

            try
            {
                return Cam.Parameters[PLCamera.OffsetY].GetValue();
            }
            catch (Exception ex)
            {
                throw new CameraException("Cannot get y offset!", ex);
            }
        }

        /// <summary>
        /// Sets the exposure of the camera.
        /// </summary>
        /// <param name="exposure">Exposure of the camera in raw format</param>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SetExposure(long exposure)
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if camera is opened before setting the exposure
            if (Cam == null || !Cam.IsOpen || !Cam.IsConnected)
                throw new InvalidOperationException("Camera not opened or connected!");

            try
            {/*
                long Min = Cam.Parameters[PLCamera.ExposureTimeRaw].GetMinimum();
                long Max = Cam.Parameters[PLCamera.ExposureTimeRaw].GetMaximum();
               
                


                 Check if exposure is within the range and set the value
                
                Cam.Parameters[PLCamera.ExposureTime].SetValue(exposure);

                // Check if exposure is within the range and set the value
                if (exposure < Min )
                    Cam.Parameters[PLCamera.ExposureTimeRaw].SetValue(Min);
                else if(exposure > Max)
                    Cam.Parameters[PLCamera.ExposureTimeRaw].SetValue(Max);
                else
                    Cam.Parameters[PLCamera.ExposureTimeRaw].SetValue(exposure);
                */


                long Min = (long)Cam.Parameters[PLCamera.ExposureTime].GetMinimum();
                long Max = (long)Cam.Parameters[PLCamera.ExposureTime].GetMaximum();

                // Check if exposure is within the range and set the value
                if (exposure < Min)
                    Cam.Parameters[PLCamera.ExposureTime].SetValue(Min);
                else if (exposure > Max)
                    Cam.Parameters[PLCamera.ExposureTime].SetValue(Max);
                else
                    Cam.Parameters[PLCamera.ExposureTime].SetValue(exposure);
            }
            catch (Exception ex)
            {
                throw new CameraException("Cannot set the exposure!", ex);
            }
        }


        /// <summary>
        /// Sets the exposure of the camera.
        /// </summary>
        /// <param name="exposure">Exposure of the camera in raw format</param>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SetExposureUSB(long exposure)
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if camera is opened before setting the exposure
            if (Cam == null || !Cam.IsOpen || !Cam.IsConnected)
                throw new InvalidOperationException("Camera not opened or connected!");

            try
            {
                long Min = (long)Cam.Parameters[PLCamera.ExposureTime].GetMinimum();
                long Max = (long)Cam.Parameters[PLCamera.ExposureTime].GetMaximum();

                // Check if exposure is within the range and set the value
                if (exposure < Min)
                    Cam.Parameters[PLCamera.ExposureTime].SetValue(Min);
                else if (exposure > Max)
                    Cam.Parameters[PLCamera.ExposureTime].SetValue(Max);
                else
                    Cam.Parameters[PLCamera.ExposureTime].SetValue(exposure);
            }
            catch (Exception ex)
            {
                throw new CameraException("Cannot set the exposure!", ex);
            }
        }

        /// <summary>
        /// Gets the the exposure of the camera.
        /// </summary>
        /// <returns>Returns the exposure of the camera in raw format</returns>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public long GetExposure()
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if camera is opened before getting the exposure
            if (Cam == null || !Cam.IsOpen || !Cam.IsConnected)
                throw new InvalidOperationException("Camera not opened or connected!");

            try
            {
                //return Cam.Parameters[PLCamera.ExposureTimeRaw].GetValue();
                return (long)Cam.Parameters[PLCamera.ExposureTime].GetValue();
            }
            catch (Exception ex)
            {
                throw new CameraException("Cannot get the exposure!", ex);
            }
        }


        /// <summary>
        /// Gets the the exposure of the camera.
        /// </summary>
        /// <returns>Returns the exposure of the camera in raw format</returns>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public long GetExposureUSB()
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if camera is opened before getting the exposure
            if (Cam == null || !Cam.IsOpen || !Cam.IsConnected)
                throw new InvalidOperationException("Camera not opened or connected!");

            try
            {
                return (long)Cam.Parameters[PLCamera.ExposureTime].GetValue();
            }
            catch (Exception ex)
            {
                throw new CameraException("Cannot get the exposure!", ex);
            }
        }

        /// <summary>
        /// Sets the autoexposure of the camera.
        /// </summary>
        /// <param name="enable">Autoexposure enabled or disabled</param>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SetAutoExposure(bool enable)
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if camera is opened before setting the auto exposure
            if (Cam == null || !Cam.IsOpen || !Cam.IsConnected)
                throw new InvalidOperationException("Camera not opened or connected!");

            try
            {
                // Enable or disable autoexposure
                Cam.Parameters[PLCamera.ExposureAuto].SetValue(enable == true ? PLCamera.ExposureAuto.Continuous : PLCamera.ExposureAuto.Off);
            }
            catch (Exception ex)
            {
                throw new CameraException("Cannot set the auto exposure!", ex);
            }
        }

        /// <summary>
        /// Gets the camera autoexposure status.
        /// </summary>
        /// <returns>Returns camera autoexposure status</returns>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public bool AutoExposureSet()
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if camera is opened before getting the auto exposure status
            if (Cam == null || !Cam.IsOpen || !Cam.IsConnected)
                throw new InvalidOperationException("Camera not opened or connected!");

            try
            {
                return Cam.Parameters[PLCamera.ExposureAuto].GetValue() == PLCamera.ExposureAuto.Continuous;
            }
            catch (Exception ex)
            {
                throw new CameraException("Cannot get the auto exposure!", ex);
            }
        }

        /// <summary>
        /// Sets the framerate of the camera.
        /// </summary>
        /// <param name="frameRate">Framerate of the camera</param>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public virtual void SetFrameRate(int frameRate)
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if camera is opened before setting the frame rate
            if (Cam == null || !Cam.IsOpen || !Cam.IsConnected)
                throw new InvalidOperationException("Camera not opened or connected!");

            try
            {
                int Min = (int)Cam.Parameters[PLCamera.AcquisitionFrameRateAbs].GetMinimum();
                int Max = (int)Cam.Parameters[PLCamera.AcquisitionFrameRateAbs].GetMaximum();

                // Enable the framerate option if not enabled
                if (!Cam.Parameters[PLCamera.AcquisitionFrameRateEnable].GetValue())
                    Cam.Parameters[PLCamera.AcquisitionFrameRateEnable].SetValue(true);

                // Check if framerate is within the range and set the value
                if (frameRate < Min )
                    Cam.Parameters[PLCamera.AcquisitionFrameRateAbs].SetValue(Min);
                else if(frameRate > Max)
                    Cam.Parameters[PLCamera.AcquisitionFrameRateAbs].SetValue(Max);
                else
                    Cam.Parameters[PLCamera.AcquisitionFrameRateAbs].SetValue(frameRate);
            }
            catch (Exception ex)
            {
                throw new CameraException("Cannot set the framerate!", ex);
            }
        }

        /// <summary>
        /// Gets the current framerate of the camera.
        /// </summary>
        /// <returns>Returns the current framerate of the camera</returns>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public virtual int GetFrameRate()
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if camera is opened before getting the frame rate
            if (Cam == null || !Cam.IsOpen || !Cam.IsConnected)
                throw new InvalidOperationException("Camera not opened or connected!");

            try
            {
                return (int)Cam.Parameters[PLCamera.AcquisitionFrameRateAbs].GetValue();
            }
            catch (Exception ex)
            {
                throw new CameraException("Cannot get the framerate!", ex);
            }
        }

        /// <summary>
        /// Sets the userset.
        /// </summary>
        /// <param name="userSet">Userset enum</param>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void ApplyUserSet(ConfigurationSetSelector userSet)
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if camera is opened before applying the user set
            if (Cam == null || !Cam.IsOpen || !Cam.IsConnected)
                throw new InvalidOperationException("Camera not opened or connected!");

            try
            {
                switch (userSet)
                {
                    case ConfigurationSetSelector.UserSet1:
                        Cam.Parameters[PLCamera.UserSetSelector].SetValue(PLCamera.UserSetSelector.UserSet1);
                        break;
                    case ConfigurationSetSelector.UserSet2:
                        Cam.Parameters[PLCamera.UserSetSelector].SetValue(PLCamera.UserSetSelector.UserSet2);
                        break;
                    case ConfigurationSetSelector.UserSet3:
                        Cam.Parameters[PLCamera.UserSetSelector].SetValue(PLCamera.UserSetSelector.UserSet3);
                        break;
                }

                // Apply the user set
                Cam.Parameters[PLCamera.UserSetLoad].Execute();
            }
            catch (Exception ex)
            {
                throw new CameraException("Cannot apply the selected userset!", ex);
            }
        }

        /// <summary>
        /// Gets the current camera userset.
        /// </summary>
        /// <returns>Current camera userset</returns>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public ConfigurationSetSelector GetAppliedUserSet()
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if camera is opened before getting the applied user set
            if (Cam == null || !Cam.IsOpen || !Cam.IsConnected)
                throw new InvalidOperationException("Camera not opened or connected!");

            try
            {
                string UserSet = Cam.Parameters[PLCamera.UserSetSelector].GetValue();

                if (UserSet == PLCamera.UserSetSelector.UserSet1)
                    return ConfigurationSetSelector.UserSet1;
                else if (UserSet == PLCamera.UserSetSelector.UserSet2)
                    return ConfigurationSetSelector.UserSet2;
                else if (UserSet == PLCamera.UserSetSelector.UserSet3)
                    return ConfigurationSetSelector.UserSet3;
                else
                    return ConfigurationSetSelector.Unknown;
            }
            catch (Exception ex)
            {
                throw new CameraException("Cannot get the used userset!", ex);
            }
        }

        public void SetCameraOutput(bool SetOutput)
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if camera is opened before setting the x Offset
            if (Cam == null || !Cam.IsOpen || !Cam.IsConnected)
                throw new InvalidOperationException("Camera not opened or connected!");

            try
            {
                Cam.Parameters[PLCamera.UserOutputValue].SetValue(SetOutput);
            }
            catch (Exception ex)
            {
                throw new CameraException("Cannot set x offset!", ex);
            }
        }

        /// <summary>
        /// Disposes the object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        #region Camera events
        private void OnImageGrabbed(object sender, ImageGrabbedEventArgs e)
        {
            try
            {
                using (IGrabResult GrabResult = e.GrabResult)
                {
                    if (GrabResult.GrabSucceeded)
                    {
                        using (Bitmap GrabbedImage = new Bitmap(GrabResult.Width, GrabResult.Height, PixelFormat.Format32bppRgb))
                        {
                            bool FireContinuousShotEvent = false;
                            bool FireOneShotEvent = false;
                            bool FireMultipleShotEvent = false;

                            // Lock the bits of the bitmap.
                            BitmapData bmpData = GrabbedImage.LockBits(new Rectangle(0, 0, GrabbedImage.Width, GrabbedImage.Height), ImageLockMode.ReadWrite, GrabbedImage.PixelFormat);
                            // Place the pointer to the buffer of the bitmap.
                            Converter.OutputPixelFormat = PixelType.BGRA8packed;
                            IntPtr ptrBmp = bmpData.Scan0;
                            Converter.Convert(ptrBmp, bmpData.Stride * GrabbedImage.Height, GrabResult);
                            GrabbedImage.UnlockBits(bmpData);

                            lock (LockObj)
                            {
                                if (MultipleShotActive)
                                {
                                    
                   
                                    // Add image to the list of grabbed images
                                    MultipleGrabImages.Add(GrabbedImage.Clone(new Rectangle(0, 0, GrabbedImage.Width, GrabbedImage.Height), GrabbedImage.PixelFormat));

                                    // Increase the image counter
                                    NumOfImagesAlreadyGrabbed++;

                                    // Set the event flag if multishot is active
                                    if (NumOfImagesAlreadyGrabbed == NumOfImagesToGrab)
                                    {
                                        MultipleShotActive = false;
                                        FireMultipleShotEvent = true;
                                    }
                                        
                                }

                                // Set the event flag if continuous shot is active
                                if (ContinuousShotActive)
                                    FireContinuousShotEvent = true;

                                // Set the event flag if one shot is active or if image grab is requested during continuous shot
                                if (OneShotActive || (ContinuousShotActive && GrabImageContinuousShot))
                                    FireOneShotEvent = true;
                            }

                            // Invoke the event if continuous shot is active
                            if (FireContinuousShotEvent)
                                ContinuousShotImageReadyEvent?.Invoke(this, new ImageReadyEventArgs(GrabbedImage.Clone(new Rectangle(0, 0, GrabbedImage.Width, GrabbedImage.Height), GrabbedImage.PixelFormat)));

                            // Invoke the event if one shot is active or if continuous shot is active and the single image has to be grabbed
                            if (FireOneShotEvent)
                                OneShotImageReadyEvent?.Invoke(this, new ImageReadyEventArgs(GrabbedImage.Clone(new Rectangle(0, 0, GrabbedImage.Width, GrabbedImage.Height), GrabbedImage.PixelFormat)));

                            // Invoke the event if multiple shot is active and the multiple images have to be grabbed
                            if (FireMultipleShotEvent)
                            {
                                MultipleShotImagesReadyEvent?.Invoke(this, new ImagesReadyEventArgs(MultipleGrabImages));
                               
                            }
                                

                            // Reset flags
                            lock (LockObj)
                            {
                                if (FireOneShotEvent)
                                {
                                    GrabImageContinuousShot = false;
                                    OneShotActive = false;
                                }

                                if (FireMultipleShotEvent)
                                    MultipleShotActive = false;
                            }
                        }
                    }
                    else
                    {
                        // Invoke the event when there was a grab error
                        CameraGrabErrorEvent?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
            catch
            {
                // Invoke the event when there was a grab error
                CameraGrabErrorEvent?.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnGrabStopped(object sender, GrabStopEventArgs e)
        {
            if(e.Reason == GrabStopReason.GrabEngineError)
                // Invoke the event when there was a grab error
                CameraGrabErrorEvent?.Invoke(this, EventArgs.Empty);
        }

        private void OnConnectionLost(object sender, EventArgs e)
        {
            // Invoke the event when the camera connection is lost
            CameraDisconnectedEvent?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
}
