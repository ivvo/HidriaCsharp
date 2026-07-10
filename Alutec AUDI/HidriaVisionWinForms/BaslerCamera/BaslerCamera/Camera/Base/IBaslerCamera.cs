using System;

namespace BaslerCam
{
    /// <summary>
    /// This interface defines methods of a camera.
    /// </summary>
    public interface IBaslerCamera
    {
        #region events
        /// <summary>
        /// Event triggers when camera disconnects.
        /// </summary>
        event EventHandler<EventArgs> CameraDisconnectedEvent;

        /// <summary>
        /// Event triggers when image is ready in continuous mode
        /// </summary>
        event EventHandler<ImageReadyEventArgs> ContinuousShotImageReadyEvent;

        /// <summary>
        /// Event triggers if there is camera grab error.
        /// </summary>
        event EventHandler<EventArgs> CameraGrabErrorEvent;

        /// <summary>
        /// Event triggers when images are ready in multiple shot
        /// </summary>
        event EventHandler<ImagesReadyEventArgs> MultipleShotImagesReadyEvent;

        /// <summary>
        /// Event triggers when image is ready in one shot mode
        /// </summary>
        event EventHandler<ImageReadyEventArgs> OneShotImageReadyEvent;
        #endregion

        #region Methods
        /// <summary>
        /// Opens the camera.
        /// </summary>
        /// <param name="serialNumber">Camera serial number</param>
        void CameraOpen(string serialNumber);

        /// <summary>
        /// Closes the camera.
        /// </summary>
        void CameraClose();

        /// <summary>
        /// Starts the one shot image grab.
        /// </summary>
        void OneShot();

        /// <summary>
        /// Starts the multiple images grab.
        /// </summary>
        /// <param name="numOfImages">Number of images to be grabbed</param>
        void MultipleShot(int numOfImages);

        /// <summary>
        /// Starts the continuous shot image grab.
        /// </summary>
        void ContinuousShot();

        /// <summary>
        /// Stops the image grabbing.
        /// </summary>
        void StopGrab();

        /// <summary>
        /// Starts the grabbing procedure of one image during continuous shot.
        /// </summary>
        void GrabImageDuringContinuousShot();

        /// <summary>
        /// Sets width of the image.
        /// </summary>
        /// <param name="width">Width of the image in pixels</param>
        void SetWidth(long width);

        /// <summary>
        /// Gets the with of the image.
        /// </summary>
        /// <returns>Returns width of the image in pixels</returns>
        long GetWidth();

        /// <summary>
        /// Sets height of the image.
        /// </summary>
        /// <param name="height">Height of the image in pixels</param>
        void SetHeight(long height);

        /// <summary>
        /// Gets the height of the image.
        /// </summary>
        /// <returns>Returns the height of the image in pixels</returns>
        long GetHeight();

        /// <summary>
        /// Sets the offset in X direction.
        /// </summary>
        /// <param name="xOffset">Offset in X direction in pixels</param>
        void SetXOffset(long xOffset);

        /// <summary>
        /// Gets the offset in X direction.
        /// </summary>
        /// <returns>Returns the offset in X direction in pixels</returns>
        long GetXOffset();

        /// <summary>
        /// Sets offset in Y direction.
        /// </summary>
        /// <param name="yOffset">Offset in Y direction in pixels</param>
        void SetYOffset(long yOffset);

        /// <summary>
        /// Gets the offset in Y direction.
        /// </summary>
        /// <returns>Returns the offset in Y direction in pixels</returns>
        long GetYOffset();

        /// <summary>
        /// Sets the exposure of the camera.
        /// </summary>
        /// <param name="exposure">Exposure of the camera in raw format</param>
        void SetExposure(long exposure);

        /// <summary>
        /// Gets the the exposure of the camera.
        /// </summary>
        /// <returns>Returns the exposure of the camera in raw format</returns>
        long GetExposure();

        /// <summary>
        /// Sets the autoexposure of the camera.
        /// </summary>
        /// <param name="enable">Autoexposure enabled or disabled</param>
        void SetAutoExposure(bool enable);

        /// <summary>
        /// Gets the camera autoexposure status.
        /// </summary>
        /// <returns>Returns camera autoexposure status</returns>
        bool AutoExposureSet();

        /// <summary>
        /// Sets the framerate of the camera.
        /// </summary>
        /// <param name="frameRate">Framerate of the camera</param>
        void SetFrameRate(int frameRate);

        /// <summary>
        /// Gets the current framerate of the camera.
        /// </summary>
        /// <returns>Returns the current framerate of the camera</returns>
        int GetFrameRate();

        /// <summary>
        /// Sets the userset.
        /// </summary>
        /// <param name="userSet">Userset enum</param>
        void ApplyUserSet(ConfigurationSetSelector userSet);

        /// <summary>
        /// Gets the current camera userset.
        /// </summary>
        /// <returns>Current camera userset</returns>
        ConfigurationSetSelector GetAppliedUserSet();
        #endregion
    }
}