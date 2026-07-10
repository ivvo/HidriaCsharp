using System;
using System.Windows.Forms;
using BaslerCam;
using Logger;

namespace HidriaVision.Helpers
{
    public partial class TriggerImagesSFX : UserControl
    {
        #region Public Delegates
        public EventHandler<string> LoadImagesFromDisc;
        public EventHandler<string> SaveImage1ToDisc;
        public EventHandler<string> SaveImage2ToDisc;
        #endregion

        #region Private Fields
        BaslerCamera Camera;
        SharedVar<bool> CameraError;
        FileEventLogger Logger;
        SFXDriverControl LedDriverControl;
        SharedVar<int> TriggerSource;
        #endregion

        #region Public Properties
        /// <summary>
        /// Set Exposure
        /// </summary>
        public float CameraExposure { get; set; } = 0.0f;

        /// <summary>
        /// Set Trigger source
        /// </summary>
        public int SetTriggerSource { private get; set; } = 0;

        /// <summary>
        /// Set button name
        /// </summary>
        public string ButtonName
        {
            set
            {
                if (this.InvokeRequired)
                {
                    BeginInvoke(new Action(() => this.btnTriggerSFX.Text = value));
                }
                else
                {
                    this.btnTriggerSFX.Text = value;
                }
            }
        }
        #endregion

        public TriggerImagesSFX(BaslerCamera camera, FileEventLogger logger, SharedVar<bool> cameraError, SFXDriverControl ledDriverControl, SharedVar<int> triggerSource)
        {
            InitializeComponent();

            Camera = camera;
            Logger = logger;
            CameraError = cameraError;
            LedDriverControl = ledDriverControl;
            TriggerSource = triggerSource;
        }

        #region Private Events
        /// <summary>
        /// Event when control is loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TriggerImagesSFX_Load(object sender, EventArgs e)
        {
            numericUpDownExposure.Value = (decimal)CameraExposure;
        }

        /// <summary>
        /// Event when trigger is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTriggerSFX_Click(object sender, EventArgs e)
        {
            if (!CameraError.Value)
            {
                // Set correct image source
                TriggerSource.Value = SetTriggerSource;

                // Set surface fx and start it
                LedDriverControl.changeControllerSFXIntensityAndStart(LedDriverControl.IntensityValue);

                // Lock
                lock (Camera)
                {
                    try
                    {
                        if (CameraExposure * 1000 != Camera.GetExposure())
                            Camera.SetExposure((long)(CameraExposure * 1000));
                        Camera.StopGrab();
                        Camera.MultipleShot(2);
                    }
                    catch (Exception ex) when (ex is CameraException || ex is InvalidOperationException)
                    {
                        // Add entry to a log
                        Logger.AddEntry(LoggingLevel.Error, $"Camera error: {ex.Message}");
                        CameraError.Value = true;

                        // Show message
                        MessageBox.Show($"Camera error: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Event when exposure value is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpDownExposure_ValueChanged(object sender, EventArgs e)
        {
            CameraExposure = (float)numericUpDownExposure.Value;
        }

        /// <summary>
        /// Event when image 1 load is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLoad1_Click(object sender, EventArgs e)
        {
            // Set correct image source
            TriggerSource.Value = SetTriggerSource;

            // Construct open file dialog for loading image
            OpenFileDialog loadDialog = new OpenFileDialog();         
            loadDialog.RestoreDirectory = true;
            loadDialog.Title = "Load image 1 BMP";
            loadDialog.DefaultExt = "bmp";
            loadDialog.Filter = "BMP images (*.bmp)|*.bmp";
            loadDialog.FilterIndex = 1;
            loadDialog.Multiselect = false;

            if (loadDialog.ShowDialog(this) == DialogResult.OK)
            {
                // Set event for image loaded with its path and name
                LoadImagesFromDisc?.Invoke(this, loadDialog.FileName);
            }
        }

        /// <summary>
        /// Event when image 1 save is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave1_Click(object sender, EventArgs e)
        {
            // Set correct image source
            TriggerSource.Value = SetTriggerSource;

            // Construct save file dialog for saving image
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.RestoreDirectory = true;
            saveDialog.Title = "Save image 1 BMP";
            saveDialog.DefaultExt = "bmp";
            saveDialog.Filter = "BMP images (*.bmp)|*.bmp";
            saveDialog.FilterIndex = 1;

            if (saveDialog.ShowDialog(this) == DialogResult.OK)
            {
                // Set event for image save with its path and name
                SaveImage1ToDisc?.Invoke(this, saveDialog.FileName);
            }
        }

        /// <summary>
        /// Event when image 2 save is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave2_Click(object sender, EventArgs e)
        {
            // Set correct image source
            TriggerSource.Value = SetTriggerSource;

            // Construct save file dialog for saving image
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.RestoreDirectory = true;
            saveDialog.Title = "Save image 2 BMP";
            saveDialog.DefaultExt = "bmp";
            saveDialog.Filter = "BMP images (*.bmp)|*.bmp";
            saveDialog.FilterIndex = 1;

            if (saveDialog.ShowDialog(this) == DialogResult.OK)
            {
                // Set event for image save with its path and name
                SaveImage2ToDisc?.Invoke(this, saveDialog.FileName);
            }
        }
        #endregion
    }
}
