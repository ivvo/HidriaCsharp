using Hik.Communication.ScsServices.Service;
using System.Collections.Generic;
using System.Drawing;

namespace BaslerCam.Service
{
    /// <summary>
    /// This interface defines methods of a camera server.
    /// Defined methods are called by the server.
    /// </summary>
    [ScsService(Version = "1.1.1")]
    public interface IBaslerCameraService
    {
        #region Properties
        /// <summary>
        /// Gets the information about the image availability.
        /// </summary>
        bool ImageReady { get; }

        /// <summary>
        /// Gets the information about the availability of the images of the multi grab
        /// </summary>
        bool MultipleGrabImageReady { get; }

        /// <summary>
        /// Gets the information about if the camera has lost its connection.
        /// </summary>
        bool CameraDisconnected { get; }

        /// <summary>
        /// Gets the information about if the camera grab error occured.
        /// </summary>
        bool CameraGrabError { get; }

        /// <summary>
        /// Gets the last grabbed image if available.
        /// </summary>
        Bitmap GrabbedImage { get;}

        /// <summary>
        /// Gets the last images of the multi grab
        /// </summary>
        List<Bitmap> MultipleGrabImages { get; }
        #endregion

        #region Methods
        /// <summary>
        /// This method is used to open the camera.
        /// </summary>
        /// <param name="serialNumber">Camera serial number</param>
        void CameraOpen(string serialNumber);

        /// <summary>
        /// This method is used to close the camera.
        /// </summary>
        void CameraClose();

        /// <summary>
        /// This method is used to start the one shot image grab.
        /// </summary>
        void OneShot();

        /// <summary>
        /// This method is used to start the multiple images grab.
        /// </summary>
        /// <param name="numOfImages">Number of images to be grabbed</param>
        void MultipleShot(int numOfImages);

        /// <summary>
        /// This method is used to start continuous shot image grab.
        /// </summary>
        void ContinuousShot();

        /// <summary>
        /// This method is used to stop the continuous shot image grab.
        /// </summary>
        void StopGrab();

        /// <summary>
        /// This method is used to start the grabbing procedure of one image during continuous shot.
        /// </summary>
        void GrabImageDuringContinuousShot();

        /// <summary>
        /// This method is called to set the width of the image.
        /// </summary>
        /// <param name="width">Width of the image in pixels</param>
        void SetWidth(long width);

        /// <summary>
        /// This method is called to get the width of the image.
        /// </summary>
        /// <returns>Width of the image in pixels</returns>
        long GetWidth();

        /// <summary>
        /// This method is called to set the height of the image.
        /// </summary>
        /// <param name="height">Height of the image in pixels</param>
        void SetHeight(long height);

        /// <summary>
        /// This method is called to get the height of the image.
        /// </summary>
        /// <returns>Returns the height of the image in pixels</returns>
        long GetHeight();

        /// <summary>
        /// This method is called to set the offset in X direction.
        /// </summary>
        /// <param name="xOffset">Offset in X direction in pixels</param>
        void SetXOffset(long xOffset);

        /// <summary>
        /// This method is called to get the offset in X direction.
        /// </summary>
        /// <returns>Returns the offset in X direction in pixels</returns>
        long GetXOffset();

        /// <summary>
        /// This method is called to set the offset in Y direction.
        /// </summary>
        /// <param name="yOffset">Offset in Y direction in pixels</param>
        void SetYOffset(long yOffset);

        /// <summary>
        /// This method is called to get the offset in Y direction.
        /// </summary>
        /// <returns>Returns the offset in Y direction in pixels</returns>
        long GetYOffset();

        /// <summary>
        /// This method is called to set the exposure of the camera.
        /// </summary>
        /// <param name="exposure">Exposure of the camera in raw format</param>
        void SetExposure(long exposure);

        /// <summary>
        /// This method is called to get the exposure of the camera.
        /// </summary>
        /// <returns>Returns the exposure of the camera in raw format</returns>
        long GetExposure();

        /// <summary>
        /// This method is called to set the autoexposure of the camera.
        /// </summary>
        /// <param name="enable">Autoexposure enabled or disabled<</param>
        void SetAutoExposure(bool enable);

        /// <summary>
        /// This method is called to get the camera autoexposure status.
        /// </summary>
        /// <returns>Returns camera autoexposure status</returns>
        bool AutoExposureSet();

        /// <summary>
        /// This method is called to set the framerate of the camera.
        /// </summary>
        /// <param name="frameRate">Framerate of the camera</param>
        void SetFrameRate(int frameRate);

        /// <summary>
        /// This method is called to get the current framerate of the camera.
        /// </summary>
        /// <returns>Returns the current framerate of the camera</returns>
        int GetFrameRate();

        /// <summary>
        /// This method is called to set the userset.
        /// </summary>
        /// <param name="userSet">Userset enum</param>
        void ApplyUserSet(ConfigurationSetSelector userSet);

        /// <summary>
        /// This method is called to get the current camera userset
        /// </summary>
        /// <returns>Current camera userset</returns>
        ConfigurationSetSelector GetAppliedUserSet();
        #endregion
    }
}
