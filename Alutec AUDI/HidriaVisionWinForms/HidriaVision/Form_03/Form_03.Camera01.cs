using BaslerCam;
using System;
using System.Drawing;
using System.Windows.Forms;
using Logger;

namespace HidriaVision
{
    public partial class Form_03 : Form
    {
        #region Events
        /// <summary>
        /// Event triggers when camera 01 becomes disconnected.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event arguments.</param>
        private void Camera01_Disconnected(object sender, EventArgs e)
        {
            // Add entry to log
            Logger.AddEntry(LoggingLevel.Error, "Station03: Camera01 disconnected");

            // Camera has been disconnected. Set error flag
            Camera01Error.Value = true;
        }  

        /// <summary>
        /// Event triggers when grab error occurs on camera 01
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Event arguments.</param>
        private void Camera01_GrabError(object sender, EventArgs e)
        {
            // Add entry to log
            Logger.AddEntry(LoggingLevel.Error, "Station03: Grab failed on camera01");

            // Error happened during image grabbing. Set error flag
            Camera01Error.Value = true;
        }

        /// <summary>
        /// Event triggers when camera 01 image is ready.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event arguments.</param>
        private void Camera01_ImagesReady(object sender, ImageReadyEventArgs e)
        {
            //Camera01.StopGrab();
            if (!InEditing.Value)
            {
                // Set the image and set the image ready flag
                Camera01SFXImages.Value = e.GrabbedImage;
                //Camera01SFXImages.Value = new Bitmap(e.GrabbedImage);
                Camera01SFXImagesReady.Value = true;
            }
            else
            {
                LastSFXFirstImage.Value = new Bitmap(e.GrabbedImage);
                //LastSFXSecondImage.Value = new Bitmap(e.GrabbedImages[1]);
                Camera01_AnalyzeImageEditing();
            }                       
        }

        /// <summary>
        /// Here image is analyzed for editing
        /// </summary>
        private void Camera01_AnalyzeImageEditing()
        {
            try
            {
                switch (TriggerSource.Value)
                {
                   
                }
            }
            catch { }
        }
        #endregion
    }
}
