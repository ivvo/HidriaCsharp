namespace BaslerCam.Service
{
    public interface IBaslerColorCameraService : IBaslerCameraService
    {
        /// <summary>
        /// This method is called to set the red balance ratio for the color camera.
        /// </summary>
        /// <param name="balanceRatio">Red balance ration in raw format</param>
        void SetRedBalanceRatio(long balanceRatio);

        /// <summary>
        /// This method is called to get the current red balance ratio of the color camera.
        /// </summary>
        /// <returns>Returns the current red balance ration of the color camera in raw format</returns>
        long GetRedBalanceRatio();

        /// <summary>
        /// This method is called to set the green balance ratio for the color camera.
        /// </summary>
        /// <param name="balanceRatio">Green balance ration in raw format</param>
        void SetGreenBalanceRatio(long balanceRatio);

        /// <summary>
        /// This method is called to get the current green balance ratio of the color camera.
        /// </summary>
        /// <returns>Returns the current green balance ration of the color camera in raw format</returns>
        long GetGreenBalanceRatio();

        /// <summary>
        /// This method is called to set the blue balance ratio for the color camera.
        /// </summary>
        /// <param name="balanceRatio">Blue balance ration in raw format</param>
        void SetBlueBalanceRatio(long balanceRatio);

        /// <summary>
        /// This method is called to get the current blue balance ratio of the color camera.
        /// </summary>
        /// <returns>Returns the current blue balance ration of the color camera in raw format</returns>
        long GetBlueBalanceRatio();
    }
}