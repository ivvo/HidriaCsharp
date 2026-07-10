using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BaslerCam;
using Logger;

namespace HidriaVision.Helpers
{
    public partial class TriggerImage : UserControl
    {
        #region Public Delegates
        public EventHandler<string> LoadImageFromDisc;
        public EventHandler<string> SaveImageToDisc;
        #endregion

        #region Private Fields
        BaslerCamera Camera;
        SharedVar<bool> CameraError;
        //BaslerCamera Camera01;
        //SharedVar<bool> Camera01Error;
        //BaslerCamera Camera02;
        //SharedVar<bool> Camera02Error;
        FileEventLogger Logger;
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
                    BeginInvoke(new Action(() => this.btnTrigger.Text = value));
                }
                else
                {
                    this.btnTrigger.Text = value;
                }
            }
        }
        #endregion

        public TriggerImage(BaslerCamera camera, FileEventLogger logger, SharedVar<bool> cameraError, SharedVar<int> triggerSource)
        {
            InitializeComponent();

            Camera = camera;
            Logger = logger;
            CameraError = cameraError;
            TriggerSource = triggerSource;
        }

        #region Private Events
        /// <summary>
        /// Event when control is loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TriggerOneImage_Load(object sender, EventArgs e)
        {
            numericUpDownExposure.Value = (decimal)CameraExposure;
        }
       
        /// <summary>
        /// Event when trigger is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnTrigger_Click(object sender, EventArgs e)
        {
            if(!CameraError.Value)
            {
                // Set correct image source
                TriggerSource.Value = SetTriggerSource;

                if(TriggerSource.Value == 1)
                {
                    // Lock
                    lock (Camera)
                    {
                        try
                        {
                            Camera.SetExposure((long)(CameraExposure * 1000));
                            Camera.OneShot();
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
                else if (TriggerSource.Value == 2)
                {
                    // Lock
                    lock (Camera)
                    {
                        try
                        {
                            Camera.SetExposureUSB((long)(CameraExposure * 1000));
                            Camera.OneShot();
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
        /// Event when image load is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLoad_Click(object sender, EventArgs e)
        {
            // Set correct image source
            TriggerSource.Value = SetTriggerSource;

            // Construct open file dialog for loading image
            OpenFileDialog loadDialog = new OpenFileDialog();
            loadDialog.RestoreDirectory = true;
            loadDialog.Title = "Load image BMP";
            loadDialog.DefaultExt = "bmp";
            loadDialog.Filter = "BMP images (*.bmp)|*.bmp";
            loadDialog.FilterIndex = 1;
            loadDialog.Multiselect = false;

            if (loadDialog.ShowDialog(this) == DialogResult.OK)
            {
                // Set event for image loaded with its path and name
                LoadImageFromDisc?.Invoke(this, loadDialog.FileName);
            }
        }

        /// <summary>
        /// Event when image save is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, EventArgs e)
        {
            // Set correct image source
            TriggerSource.Value = SetTriggerSource;

            // Construct save file dialog for saving image
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.RestoreDirectory = true;
            saveDialog.Title = "Save image BMP";
            saveDialog.DefaultExt = "bmp";
            saveDialog.Filter = "BMP images (*.bmp)|*.bmp";
            saveDialog.FilterIndex = 1;          

            if (saveDialog.ShowDialog(this) == DialogResult.OK)
            {
                // Set event for image save with its path and name
                SaveImageToDisc?.Invoke(this, saveDialog.FileName);
            }
        }
        #endregion

    }
}
