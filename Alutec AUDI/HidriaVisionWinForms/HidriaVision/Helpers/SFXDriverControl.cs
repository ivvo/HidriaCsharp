using System;
using System.Windows.Forms;
using LedController;
using Logger;

namespace HidriaVision.Helpers
{
    public partial class SFXDriverControl : UserControl
    {
        #region Public Properties      
        /// <summary>
        /// Get or set value for intensity
        /// </summary>
        public byte IntensityValue { get; set; } = 0;
        #endregion

        #region Private Fields
        private RGBWController LedController;
        private SharedVar<bool> LedControllerError;
        private FileEventLogger Logger;
        #endregion

        public SFXDriverControl(RGBWController controller, SharedVar<bool> ledControllerError, FileEventLogger logger)
        {
            InitializeComponent();

            LedController = controller;
            LedControllerError = ledControllerError;
            Logger = logger;
        }

        #region Private Events     
        /// <summary>
        /// Fires when control is loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SFXDriverControl_Load(object sender, EventArgs e)
        {
            trackBarIntensity.Value = IntensityValue;
            setIntensity();
        }

        /// <summary>
        /// Event when trackbarChannel1 value has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trackBarIntensity_ValueChanged(object sender, EventArgs e)
        {
            IntensityValue = (byte)trackBarIntensity.Value;
            setIntensity();
        }
        #endregion

        #region Private Methods


        /// <summary>
        /// Method for setting intensity values
        /// </summary>        
        private void setIntensity()
        {
            this.textBoxValueIntensity.Text = IntensityValue.ToString();
            trackBarIntensity.Value = IntensityValue;
        }

        private void textBoxValueIntensity_TextChanged(object sender, EventArgs e)
        {
            try
            {
                IntensityValue = Convert.ToByte(textBoxValueIntensity.Text);
            }
            catch
            {
                IntensityValue = 0;
            }
            setIntensity();
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Method to send channel value to controller and start surface fx.
        /// </summary>
        /// <param name="intensity">Channel 1 intensity.</param>       
        public void changeControllerSFXIntensityAndStart(byte intensity)
        {
            // If there is no error, send new value
            if (!LedControllerError.Value)
            {
                // Lock
                lock (LedController)
                {
                    try
                    {
                        // Set led channels
                        LedController.SetLedChannels(intensity, intensity, intensity, intensity);

                        // Set surface FX
                        LedController.SetLedTriggerMode(LedTriggerMode.External);
                        LedController.SetSurfaceFx(SurfaceFxState.Enabled);
                        LedController.SetSurfaceFxMode(SurfaceFxMode.Trigger);
                        LedController.SetSurfaceFxTriggerDelay(20);
                        LedController.ResetSurfaceFx();

                        // Start surface FX
                        LedController.StartSurfaceFx();
                    }
                    catch (Exception e) when (e is InvalidOperationException || e is TimeoutException || e is ControllerOperationFailedException)
                    {
                        // Add entry to a log
                        Logger.AddEntry(LoggingLevel.Error, $"LedController Error: {e.Message}");
                        LedControllerError.Value = true;

                        // Show message
                        MessageBox.Show($"LedController Error: {e.Message}");
                    }
                }
            }
        }
        #endregion


    }
}