using System.Windows.Forms;
using UserManagment;
using UserManagment.DataManagment;
using XMLSettings;
using System.Linq;
using System;

namespace HidriaVision
{
    public partial class Form_Settings : Form
    {
        #region Events
        public event EventHandler IsConfirmed;
        #endregion

        #region Private fields
        private XMLSettingsManager MainSettings;
        private XMLSettingsManager Station03Settings;

        private XmlDataManager DataManager;
        private UserData Admin;
        string[] IPAddress;
        private string Station03CameraID;

        private string Language;
        private int LoginTimeout;
        private int NumOfLogEntries;
        private int Rack;
        private int Slot;
        #endregion

        public Form_Settings(XMLSettingsManager mainSettings, XMLSettingsManager station03Settings, XmlDataManager dataManager)
        {
            InitializeComponent();

            MainSettings = mainSettings;
            Station03Settings = station03Settings;
            DataManager = dataManager;

            // PLC settings
            IPAddress = ((string)MainSettings.GetElement("Setting", "PLC", "IP")).Split('.');
            Rack = (int)MainSettings.GetElement("Setting", "PLC", "Rack");
            Slot = (int)MainSettings.GetElement("Setting", "PLC", "Slot");

            numericUpDownIP1.Value = decimal.Parse(IPAddress[0]);
            numericUpDownIP2.Value = decimal.Parse(IPAddress[1]);
            numericUpDownIP3.Value = decimal.Parse(IPAddress[2]);
            numericUpDownIP4.Value = decimal.Parse(IPAddress[3]);
            numericUpDownRack.Value = Rack;
            numericUpDownSlot.Value = Slot;

            // Station03 settings
            Station03CameraID = (string)Station03Settings.GetElement("Camera", "Camera01", "ID");
            textBoxStation03ID.Text = Station03CameraID;

            // Administrator settings
            Admin = DataManager.GetEntries().First();
            LoginTimeout = (int)MainSettings.GetElement("Setting", "Login", "LoginTimeout");

            textBoxOld.Text = Admin.UserCred.Password.Base64Str;
            numericUpDownTimeout.Value = LoginTimeout;

            // logger settings
            NumOfLogEntries = (int)MainSettings.GetElement("Setting", "Logging", "NumOfEntries");

            numericUpDownMaxLogged.Value = NumOfLogEntries;

            // Language settings
            Language = (string)MainSettings.GetElement("Setting", "Language", "SelectedLanguage");

            if (Language == "English")
                comboBoxLanguage.SelectedIndex = 0;
            else if(Language == "Slovenian")
                comboBoxLanguage.SelectedIndex = 1;
            else
                comboBoxLanguage.SelectedIndex = 0;
        }

        #region Private events
        /// <summary>
        /// Ebent fires when ok buton is clicked.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void BtnOk_Click(object sender, EventArgs e)
        {
            // Save PLC settings
            string OldIPAddress = string.Join(".", IPAddress);
            string NewIPAddress = string.Format("{0}.{1}.{2}.{3}", numericUpDownIP1.Value, numericUpDownIP2.Value, numericUpDownIP3.Value, numericUpDownIP4.Value);

            if(NewIPAddress != OldIPAddress)
                MainSettings.SetElement("Setting", "PLC", "IP", NewIPAddress);

            if(Rack != numericUpDownRack.Value)
                MainSettings.SetElement("Setting", "PLC", "Rack", (int)numericUpDownRack.Value);

            if(Slot != numericUpDownSlot.Value)
                MainSettings.SetElement("Setting", "PLC", "Slot", (int)numericUpDownSlot.Value);

            // Save station03 settings
            if (Station03CameraID != textBoxStation03ID.Text)
                Station03Settings.SetElement("Camera", "Camera01", "ID", textBoxStation03ID.Text);
            

            // Save logging settings
            if (NumOfLogEntries != numericUpDownMaxLogged.Value)
                MainSettings.SetElement("Setting", "Logging", "NumOfEntries", (int)numericUpDownMaxLogged.Value);

            // Save language settings
            if(Language != (string)comboBoxLanguage.SelectedItem)
                MainSettings.SetElement("Setting", "Language", "SelectedLanguage", (string)comboBoxLanguage.SelectedItem);

            IsConfirmed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event fires when cancel button is clicked.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void BtnCancel_Click(object sender, System.EventArgs e)
        {
            IsConfirmed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Event fires when button which confirms new credential settings is clicked.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void ButtonAccept_Click(object sender, System.EventArgs e)
        {
            // Check if new password has been entered
            if (textBoxNew.Text.Length > 0 || textBoxRepeat.Text.Length > 0)
            {
                // Check if password matches
                if (textBoxNew.Text == textBoxRepeat.Text)
                {
                    // Change password
                    UserData ModifiedAdmin = new UserData(Admin.Name, Admin.Surname, new UserCredentials(Admin.UserCred.Username, textBoxNew.Text), null, Admin.UserRole);

                    DataManager.ModifyEntry(Admin, ModifiedAdmin);

                    MessageBox.Show("Password has been changed.");
                }
                else
                    MessageBox.Show("Entered password does not match.");
            }

            // Save login timeout
            if (LoginTimeout != numericUpDownTimeout.Value)
                MainSettings.SetElement("Setting", "Login", "LoginTimeout", (int)numericUpDownTimeout.Value);

        }
        #endregion


    }
}
