using System;

namespace BaslerCam.Service
{
    public class BaslerColorCameraService : BaslerCameraService, IBaslerColorCameraService
    {
        #region Private fields
        private IBaslerColorCamera Cam;
        #endregion

        /// <summary>
        /// Initializes new instance of the color camera service
        /// </summary>
        /// <param name="cam"></param>
        public BaslerColorCameraService(IBaslerColorCamera cam) : base(cam)
        {
            Cam = cam;
        }

        /// <summary>
        /// This method is called to set the red balance ratio for the color camera.
        /// </summary>
        /// <param name="balanceRatio">Red balance ration in raw format</param>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SetRedBalanceRatio(long balanceRatio)
        {
            Cam.SetRedBalanceRatio(balanceRatio);
        }

        /// <summary>
        /// This method is called to get the current red balance ratio of the color camera.
        /// </summary>
        /// <returns>Returns the current red balance ration of the color camera in raw format</returns>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public long GetRedBalanceRatio()
        {
            return Cam.GetRedBalanceRatio();
        }

        /// <summary>
        /// This method is called to set the green balance ratio for the color camera.
        /// </summary>
        /// <param name="balanceRatio">Green balance ration in raw format</param>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SetGreenBalanceRatio(long balanceRatio)
        {
            Cam.SetGreenBalanceRatio(balanceRatio);
        }

        /// <summary>
        /// This method is called to get the current green balance ratio of the color camera.
        /// </summary>
        /// <returns>Returns the current green balance ration of the color camera in raw format</returns>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public long GetGreenBalanceRatio()
        {
            return Cam.GetGreenBalanceRatio();
        }

        /// <summary>
        /// This method is called to set the blue balance ratio for the color camera.
        /// </summary>
        /// <param name="balanceRatio">Blue balance ration in raw format</param>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SetBlueBalanceRatio(long balanceRatio)
        {
            Cam.SetBlueBalanceRatio(balanceRatio);
        }

        /// <summary>
        /// This method is called to get the current blue balance ratio of the color camera.
        /// </summary>
        /// <returns>Returns the current blue balance ration of the color camera in raw format</returns>
        /// <exception cref="CameraException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public long GetBlueBalanceRatio()
        {
            return Cam.GetBlueBalanceRatio();
        }
    }
}
