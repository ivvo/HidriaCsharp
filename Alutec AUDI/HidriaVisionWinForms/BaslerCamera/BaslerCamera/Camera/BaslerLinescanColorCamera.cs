using Basler.Pylon;
using System;

namespace BaslerCam
{
    public class BaslerLinescanColorCamera : BaslerLinescanCamera, IBaslerColorCamera
    {
        /// <summary>
        /// Sets the red balance ratio for the color camera.
        /// </summary>
        /// <param name="balanceRatio">Red balance ration in raw format</param>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SetRedBalanceRatio(long balanceRatio)
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if camera is opened before setting red balance ratio
            if (Cam == null || !Cam.IsOpen || !Cam.IsConnected)
                throw new InvalidOperationException("Camera not opened or connected!");

            try
            {
                // Select red gain
                Cam.Parameters[PLCamera.GainSelector].SetValue(PLCamera.GainSelector.Red);

                long Min = Cam.Parameters[PLCamera.GainRaw].GetMinimum();
                long Max = Cam.Parameters[PLCamera.GainRaw].GetMaximum();

                if (balanceRatio < Min)
                    Cam.Parameters[PLCamera.GainRaw].SetValue(Min);
                else if (balanceRatio > Max)
                    Cam.Parameters[PLCamera.GainRaw].SetValue(Max);
                else
                    Cam.Parameters[PLCamera.GainRaw].SetValue(balanceRatio);
            }
            catch (Exception ex)
            {
                throw new CameraException("Cannot set the red balance ratio!", ex);
            }
        }

        /// <summary>
        /// Gets the current red balance ratio of the color camera.
        /// </summary>
        /// <returns>Returns the current red balance ration of the camera in raw format</returns>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public long GetRedBalanceRatio()
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if camera is opened before getting red balance ratio
            if (Cam == null || !Cam.IsOpen || !Cam.IsConnected)
                throw new InvalidOperationException("Camera not opened or connected!");

            try
            {
                // Select red gain
                Cam.Parameters[PLCamera.GainSelector].SetValue(PLCamera.GainSelector.Red);

                return Cam.Parameters[PLCamera.GainRaw].GetValue();
            }
            catch (Exception ex)
            {
                throw new CameraException("Cannot get the red balance ratio!", ex);
            }
        }

        /// <summary>
        /// Sets the green balance ratio for the color camera.
        /// </summary>
        /// <param name="balanceRatio">Green balance ration in raw format</param>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SetGreenBalanceRatio(long balanceRatio)
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if camera is opened before setting green balance ratio
            if (Cam == null || !Cam.IsOpen || !Cam.IsConnected)
                throw new InvalidOperationException("Camera not opened or connected!");

            try
            {
                // Select green gain
                Cam.Parameters[PLCamera.GainSelector].SetValue(PLCamera.GainSelector.Green);

                long Min = Cam.Parameters[PLCamera.GainRaw].GetMinimum();
                long Max = Cam.Parameters[PLCamera.GainRaw].GetMaximum();

                if (balanceRatio < Min)
                    Cam.Parameters[PLCamera.GainRaw].SetValue(Min);
                else if (balanceRatio > Max)
                    Cam.Parameters[PLCamera.GainRaw].SetValue(Max);
                else
                    Cam.Parameters[PLCamera.GainRaw].SetValue(balanceRatio);
            }
            catch (Exception ex)
            {
                throw new CameraException("Cannot set the green balance ratio!", ex);
            }
        }

        /// <summary>
        /// Gets the current green balance ratio of the color camera.
        /// </summary>
        /// <returns>Returns the current green balance ration of the camera in raw format</returns>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public long GetGreenBalanceRatio()
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if camera is opened before getting green balance ratio
            if (Cam == null || !Cam.IsOpen || !Cam.IsConnected)
                throw new InvalidOperationException("Camera not opened or connected!");

            try
            {
                // Select green gain
                Cam.Parameters[PLCamera.GainSelector].SetValue(PLCamera.GainSelector.Green);

                return Cam.Parameters[PLCamera.GainRaw].GetValue();
            }
            catch (Exception ex)
            {
                throw new CameraException("Cannot get the green balance ratio!", ex);
            }
        }

        /// <summary>
        /// Sets the blue balance ratio for the color camera.
        /// </summary>
        /// <param name="balanceRatio">Blue balance ration in raw format</param>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SetBlueBalanceRatio(long balanceRatio)
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if camera is opened before setting blue balance ratio
            if (Cam == null || !Cam.IsOpen || !Cam.IsConnected)
                throw new InvalidOperationException("Camera not opened or connected!");

            try
            {
                // Select blue gain
                Cam.Parameters[PLCamera.GainSelector].SetValue(PLCamera.GainSelector.Blue);

                long Min = Cam.Parameters[PLCamera.GainRaw].GetMinimum();
                long Max = Cam.Parameters[PLCamera.GainRaw].GetMaximum();

                if (balanceRatio < Min)
                    Cam.Parameters[PLCamera.GainRaw].SetValue(Min);
                else if (balanceRatio > Max)
                    Cam.Parameters[PLCamera.GainRaw].SetValue(Max);
                else
                    Cam.Parameters[PLCamera.GainRaw].SetValue(balanceRatio);
            }
            catch (Exception ex)
            {
                throw new CameraException("Cannot set the blue balance ratio!", ex);
            }
        }

        /// <summary>
        /// Gets the current blue balance ratio of the color camera.
        /// </summary>
        /// <returns>Returns the current blue balance ration of the camera in raw format</returns>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public long GetBlueBalanceRatio()
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check if camera is opened before getting blue balance ratio
            if (Cam == null || !Cam.IsOpen || !Cam.IsConnected)
                throw new InvalidOperationException("Camera not opened or connected!");

            try
            {
                // Select blue gain
                Cam.Parameters[PLCamera.GainSelector].SetValue(PLCamera.GainSelector.Blue);

                return Cam.Parameters[PLCamera.GainRaw].GetValue();
            }
            catch (Exception ex)
            {
                throw new CameraException("Cannot get the blue balance ratio!", ex);
            }
        }
    }
}
