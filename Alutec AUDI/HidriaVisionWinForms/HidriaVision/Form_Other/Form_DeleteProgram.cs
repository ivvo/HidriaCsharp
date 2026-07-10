using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HidriaVision
{
    public partial class Form_DeleteType : Form
    {
        #region Public Properties     
        public string TypeToDeleteFullName { private get; set; }
        #endregion

        public Form_DeleteType()
        {
            InitializeComponent();
        }

        #region Private Events
        /// <summary>
        /// Event when form is loading
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form_DeleteProgram_Load(object sender, EventArgs e)
        {
            textBoxTypeToDeleteFullName.Text = TypeToDeleteFullName;
        }

        /// <summary>
        /// Event Cancel button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// Event OK button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
        #endregion
    }
}
