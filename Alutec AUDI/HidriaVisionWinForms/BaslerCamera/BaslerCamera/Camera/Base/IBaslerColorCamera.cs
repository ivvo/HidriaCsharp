namespace BaslerCam
{
    /// <summary>
    /// This interface defines methods of a color camera.
    /// </summary>
    public interface IBaslerColorCamera : IBaslerCamera
    {
        #region Methods
        /// <summary>
        /// Sets the red balance ratio for the color camera.
        /// </summary>
        /// <param name="balanceRatio">Red balance ration in raw format</param>
        void SetRedBalanceRatio(long balanceRatio);

        /// <summary>
        /// Gets the current red balance ratio of the color camera.
        /// </summary>
        /// <returns>Returns the current red balance ration of the camera in raw format</returns>
        long GetRedBalanceRatio();

        /// <summary>
        /// Sets the green balance ratio for the color camera.
        /// </summary>
        /// <param name="balanceRatio">Green balance ration in raw format</param>
        void SetGreenBalanceRatio(long balanceRatio);

        /// <summary>
        /// Gets the current green balance ratio of the color camera.
        /// </summary>
        /// <returns>Returns the current green balance ration of the color camera in raw format</returns>
        long GetGreenBalanceRatio();

        /// <summary>
        /// Sets the blue balance ratio for the color camera.
        /// </summary>
        /// <param name="balanceRatio">Blue balance ration in raw format</param>
        void SetBlueBalanceRatio(long balanceRatio);

        /// <summary>
        /// Gets the current blue balance ratio of the color camera.
        /// </summary>
        /// <returns>Returns the current blue balance ration of the color camera in raw format</returns>
        long GetBlueBalanceRatio();
        #endregion
    }
}