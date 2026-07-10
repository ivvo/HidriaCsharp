using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HidriaVision
{
    public partial class Form_Loading : Form
    {
        #region Private fields
        private int _loadingCountCurrent = 0;
        private int _loadingCountMax = 0;
        #endregion

        public Form_Loading()
        {
            InitializeComponent();
            this.TopLevel = true;
            this.TopMost = true;
            // Load number of steps for progressbar from settings
            _loadingCountMax = Properties.Settings.Default.LoadCount;
            loadProgressBar.Maximum = _loadingCountMax;
        }

        #region Methods
        /// <summary>
        /// This method increments progressbar and displays current text
        /// </summary>
        /// <param name="text">Text to display</param>
        public void progressBarUpdate(string text)
        {
            _loadingCountCurrent++;
            this.Invoke(new Action(() => loadProgressBar.Increment(1)));
            this.Invoke(new Action(() => infoLabel.Text = text ));
        }

        /// <summary>
        /// This method gracefully closes loading form
        /// </summary>
        public void closeForm()
        {
            if(_loadingCountCurrent > _loadingCountMax)
            {
                Properties.Settings.Default.LoadCount = _loadingCountCurrent;
                Properties.Settings.Default.Save();
            }          
            this.Invoke(new Action(() => this.Close() ));
        }
        #endregion
    }
}
