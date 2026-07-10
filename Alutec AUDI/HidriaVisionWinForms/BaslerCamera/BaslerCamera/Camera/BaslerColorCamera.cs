using Basler.Pylon;
using System;

namespace BaslerCam
{
    public class BaslerColorCamera : BaslerCamera, IBaslerColorCamera
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
                // Select custom balance light source
                Cam.Parameters[PLCamera.LightSourceSelector].SetValue(PLCamera.LightSourceSelector.Custom);

                // Select red balance ration
                Cam.Parameters[PLCamera.BalanceRatioSelector].SetValue(PLCamera.BalanceRatioSelector.Red);

                long Min = Cam.Parameters[PLCamera.BalanceRatioRaw].GetMinimum();
                long Max = Cam.Parameters[PLCamera.BalanceRatioRaw].GetMaximum();

                if (balanceRatio < Min)
                    Cam.Parameters[PLCamera.BalanceRatioRaw].SetValue(Min);
                else if (balanceRatio > Max)
                    Cam.Parameters[PLCamera.BalanceRatioRaw].SetValue(Max);
                else
                    Cam.Parameters[PLCamera.BalanceRatioRaw].SetValue(balanceRatio);
            }
            catch (Exception ex)
            {
                throw new CameraException("Cannot set the red balance", ex);
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
                // Select custom balance light source
                Cam.Parameters[PLCamera.LightSourceSelector].SetValue(PLCamera.LightSourceSelector.Custom);

                // Select red balance ration
                Cam.Parameters[PLCamera.BalanceRatioSelector].SetValue(PLCamera.BalanceRatioSelector.Red);

                return Cam.Parameters[PLCamera.BalanceRatioRaw].GetValue();
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
                // Select custom balance light source
                Cam.Parameters[PLCamera.LightSourceSelector].SetValue(PLCamera.LightSourceSelector.Custom);

                // Select green balance ration
                Cam.Parameters[PLCamera.BalanceRatioSelector].SetValue(PLCamera.BalanceRatioSelector.Green);

                long Min = Cam.Parameters[PLCamera.BalanceRatioRaw].GetMinimum();
                long Max = Cam.Parameters[PLCamera.BalanceRatioRaw].GetMaximum();

                if (balanceRatio < Min)
                    Cam.Parameters[PLCamera.BalanceRatioRaw].SetValue(Min);
                else if (balanceRatio > Max)
                    Cam.Parameters[PLCamera.BalanceRatioRaw].SetValue(Max);
                else
                    Cam.Parameters[PLCamera.BalanceRatioRaw].SetValue(balanceRatio);
            }
            catch (Exception ex)
            {
                throw new CameraException("Cannot set the green balance ratio!", ex);
            }
        }

        /// <summary>
        /// Gets the current green balance ratio of the color camera.
        /// </summary>
        /// <returns>Returns the current green balance ration of the color camera in raw format</returns>
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
                // Select custom balance light source
                Cam.Parameters[PLCamera.LightSourceSelector].SetValue(PLCamera.LightSourceSelector.Custom);

                // Select green balance ration
                Cam.Parameters[PLCamera.BalanceRatioSelector].SetValue(PLCamera.BalanceRatioSelector.Green);

                return Cam.Parameters[PLCamera.BalanceRatioRaw].GetValue();
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
                // Select custom balance light source
                Cam.Parameters[PLCamera.LightSourceSelector].SetValue(PLCamera.LightSourceSelector.Custom);

                // Select blue balance ration
                Cam.Parameters[PLCamera.BalanceRatioSelector].SetValue(PLCamera.BalanceRatioSelector.Blue);

                long Min = Cam.Parameters[PLCamera.BalanceRatioRaw].GetMinimum();
                long Max = Cam.Parameters[PLCamera.BalanceRatioRaw].GetMaximum();

                if (balanceRatio < Min)
                    Cam.Parameters[PLCamera.BalanceRatioRaw].SetValue(Min);
                else if (balanceRatio > Max)
                    Cam.Parameters[PLCamera.BalanceRatioRaw].SetValue(Max);
                else
                    Cam.Parameters[PLCamera.BalanceRatioRaw].SetValue(balanceRatio);
            }
            catch (Exception ex)
            {
                throw new CameraException("Cannot set the blue balance ratio!", ex);
            }
        }

        /// <summary>
        /// Gets the current blue balance ratio of the color camera.
        /// </summary>
        /// <returns>Returns the current blue balance ration of the color camera in raw format</returns>
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
                // Select custom balance light source
                Cam.Parameters[PLCamera.LightSourceSelector].SetValue(PLCamera.LightSourceSelector.Custom);

                // Select blue balance ration
                Cam.Parameters[PLCamera.BalanceRatioSelector].SetValue(PLCamera.BalanceRatioSelector.Blue);

                return Cam.Parameters[PLCamera.BalanceRatioRaw].GetValue();
            }
            catch (Exception ex)
            {
                throw new CameraException("Cannot get the blue balance ratio!", ex);
            }
        }
    }
}
