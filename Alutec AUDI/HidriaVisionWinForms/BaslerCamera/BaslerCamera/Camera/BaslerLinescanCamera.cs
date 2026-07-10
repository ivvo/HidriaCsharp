using Basler.Pylon;
using System;

namespace BaslerCam
{
    /// <summary>
    /// Represents Basler mono or color linescan camera.
    /// </summary>
    public class BaslerLinescanCamera : BaslerCamera
    {
        /// <summary>
        /// Initializes new instance of the Basler linescan camera.
        /// </summary>
        public BaslerLinescanCamera() : base(){ }

        #region Public methods
        /// <summary>
        /// Sets the framerate of the camera.
        /// </summary>
        /// <param name="frameRate">Framerate of the camera</param>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public override void SetFrameRate(int frameRate)
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if camera is opened before setting the frame rate
            if (Cam == null || !Cam.IsOpen || !Cam.IsConnected)
                throw new InvalidOperationException("Camera not opened or connected!");

            try
            {
                int Min = (int)Cam.Parameters[PLCamera.AcquisitionLineRateAbs].GetMinimum();
                int Max = (int)Cam.Parameters[PLCamera.AcquisitionLineRateAbs].GetMaximum();

                // Check if framerate is within the range and set the value
                if (frameRate < Min)
                    Cam.Parameters[PLCamera.AcquisitionLineRateAbs].SetValue(Min);
                else if (frameRate > Max)
                    Cam.Parameters[PLCamera.AcquisitionLineRateAbs].SetValue(Max);
                else
                    Cam.Parameters[PLCamera.AcquisitionLineRateAbs].SetValue(frameRate);
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
        public override int GetFrameRate()
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if camera is opened before getting the frame rate
            if (Cam == null || !Cam.IsOpen || !Cam.IsConnected)
                throw new InvalidOperationException("Camera not opened or connected!");

            try
            {
                return (int)Cam.Parameters[PLCamera.AcquisitionLineRateAbs].GetValue();
            }
            catch (Exception ex)
            {
                throw new CameraException("Cannot get the framerate!", ex);
            }
        }
        #endregion
    }
}
