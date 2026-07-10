using System.Windows.Forms;

namespace HidriaVision
{
    public partial class Form_Error : Form
    {
        public Form_Error(string message, string longMessage)
        {
            InitializeComponent();

            // Set message
            textBoxMessage.Text = message;
            textBoxLongMessage.Text = longMessage;
            
            // Enter behaves as if you pressed ok button with mouse
            this.AcceptButton = btnOK;
        }

        #region Private events

        /// <summary>
        /// Event fires when ok button is clicked.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void btnOK_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }
        #endregion
    }
}
