using System;

namespace BaslerCam
{
    /// <summary>
    /// Interface for class capable of listening for new camera images during continuous shot.
    /// </summary>
    public interface IBaslerCamContinuousGrab
    {
        #region Events
        /// <summary>
        /// Event triggers when image is ready after calling ContinuousShot method.
        /// </summary>
        event EventHandler<ImageReadyEventArgs> ContinuousShotImageReadyEvent;
        #endregion
    }
}
