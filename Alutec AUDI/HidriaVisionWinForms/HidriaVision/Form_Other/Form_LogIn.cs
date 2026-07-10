using System;
using System.Windows.Forms;
using UserManagment;
using UserManagment.LoginManagment;
using XMLSettings;

namespace HidriaVision
{
    public partial class Form_LogIn : Form
    {
        #region private fields
        private LoginManager UserLogin;
        private XMLSettingsManager MainSettings;
        #endregion

        public Form_LogIn(LoginManager userLogin, XMLSettingsManager mainSettings)
        {
            InitializeComponent();

            UserLogin = userLogin;
            MainSettings = mainSettings;

            // Enter behaves as if you pressed ok button with mouse
            this.AcceptButton = btnOK;
        }

        #region Private events
        /// <summary>
        /// Event fires when ok button is clicked.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (UserLogin.Login(new UserCredentials("Admin", textBoxPassword.Text), (int)MainSettings.GetElement("Setting", "Login", "LoginTimeout")))
                    DialogResult = DialogResult.OK;
                else
                    MessageBox.Show("Username or password incorrect.");
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// Event fires when cancel button is clicked.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
        #endregion
    }
}
