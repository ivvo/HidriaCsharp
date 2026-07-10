using BaslerCam;
using Logger.ImageLogger;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToolblockManager;
using XMLSettings;

namespace HidriaVision
{
    public partial class Form_03 : Form
    {
        #region Private methods


        /// <summary>
        /// Method loads images from disc 
        /// </summary>
        /// <param name="pathName">Full path and name of the image</param>     
        private async void LoadImage1FromDisc(object sender, string pathName)
        {
            int pTo = pathName.LastIndexOf(".") - 4;

            string result = pathName.Substring(0, pTo);
            string Image1 = $"{result}_I01.bmp";
            string Image2 = $"{result}_I02.bmp";

            if (!File.Exists(Image1)) return;
            else  LastSFXFirstImage.Value = new Bitmap(Image1);

            if (!File.Exists(Image2)) return;
            else LastSFXSecondImage.Value = new Bitmap(Image2);

            await Task.Run(() => Camera01_AnalyzeImageEditing());
        }

        /// <summary>
        /// Method saves image to disc 
        /// </summary>
        /// <param name="pathName">Full path and name of the image</param>     
        private void SaveImage1ToDisc(object sender, string pathName)
        {
            if (!Directory.Exists(Path.GetDirectoryName(pathName))) return;

            LastSFXFirstImage.Value.Save(pathName);
        }

        /// <summary>
        /// Method saves image to disc 
        /// </summary>
        /// <param name="pathName">Full path and name of the image</param>     
        private void SaveImage2ToDisc(object sender, string pathName)
        {
            if (!Directory.Exists(Path.GetDirectoryName(pathName))) return;

            LastSFXSecondImage.Value.Save(pathName);
        }


      
        #endregion
    }
}
    