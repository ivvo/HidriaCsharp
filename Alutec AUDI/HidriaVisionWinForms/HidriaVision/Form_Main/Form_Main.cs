using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Logger;
using UserManagment.LoginManagment;
using UserManagment.DataManagment;
using System.Threading.Tasks;
using XMLSettings;
using Snap7Manager;
using System.Collections.Generic;
using HidriaVision.Properties;

namespace HidriaVision
{
    public partial class Form_Main : Form
    {
        #region Invoke declarations
        private const int GWL_STYLE = -16;
        private const int WS_DISABLED = 0x08000000;

        // P/Invoke declarations
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int
        wDest, int hDest, IntPtr hdcSource, int xSrc, int ySrc, CopyPixelOperation rop);
        [DllImport("user32.dll")]
        private static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDc);
        [DllImport("gdi32.dll")]
        private static extern IntPtr DeleteDC(IntPtr hDc);
        [DllImport("gdi32.dll")]
        private static extern IntPtr DeleteObject(IntPtr hDc);
        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);
        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();
        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowDC(IntPtr ptr);
        #endregion

        #region Constants
        /// <summary>
        /// Application status flags positions
        /// </summary>
        private readonly byte ApplicationAliveFlagPos = 0;
        #endregion

        #region Private Fields
        // Managers variables
        private FileEventLogger Logger;
        private LoginManager UserLogin;
        private XMLSettingsManager MainSettings;
        private XMLSettingsManager Station03Settings;
        private XmlDataManager DataManager;
        private Snap7CommunicationManager Snap7ComManager;

        // Forms and form variables
        private Form_03 Frm_03;
        private Form_Loading Frm_Loading;
        private Form_Log Frm_Log;
        private Form_Settings Frm_Settings;
        private SharedVar<bool> Station03Error;
        private SharedVar<bool> Station07Error;
        private SharedVar<bool> Station09Error;
        private Form_DummyEditToolBlock dummyEdit;
        private Point mLastPos;
        private Screen[] screens;
        private Screen currentScreen;
        private bool Snap7Error;
        private bool StationsErrorToggle;
        #endregion

        public Form_Main(FileEventLogger logger)
        {
            InitializeComponent();
            DataManager = new XmlDataManager("./Users", "Users.xml");
            MainSettings = new XMLSettingsManager("./Settings/", "Settings.xml");
            Station03Settings = new XMLSettingsManager("./Station03/", "Settings.xml");
            UserLogin = new LoginManager(DataManager);

            // Forms and form variables
            Logger = logger;
            Station03Error = new SharedVar<bool>();
            Station07Error = new SharedVar<bool>();
            Station09Error = new SharedVar<bool>();
            Snap7Error = false;
            Snap7ComManager = null;

            // Initialize data manager
            DataManager.Initialize();

            // Register callback for logger
            Logger.LogEntryAddedEvent += OnLogEntryAdded;
        }

        #region Private methods
        /// <summary>
        /// Enables or disables a form.
        /// </summary>
        /// <param name="enabled"></param>
        public void SetNativeEnabled(bool enabled)
        {
            SetWindowLong(Handle, GWL_STYLE, GetWindowLong(Handle, GWL_STYLE) &
                ~WS_DISABLED | (enabled ? 0 : WS_DISABLED));

            Focus();
        }


        /// <summary>
        /// Show station2 form.
        /// </summary>
        private void Station03Show()
        {
            Frm_03.TopLevel = false;
            splitContainer1.Panel2.Controls.Clear();
            splitContainer1.Panel2.Controls.Add(Frm_03);
            Frm_03.Dock = DockStyle.Fill;

            Frm_Log.Hide();
            Frm_03.Show();
            btnLogView.ForeColor = Color.Gray;
            btnSettingsView.ForeColor = Color.Gray;
            btnStation03.ForeColor = Color.White;
        }

        /// <summary>
        /// Show log form.
        /// </summary>
        private void LogShow()
        {
            Frm_Log.TopLevel = false;
            splitContainer1.Panel2.Controls.Clear();
            splitContainer1.Panel2.Controls.Add(Frm_Log);
            Frm_Log.Dock = DockStyle.Fill;

            Frm_03.Hide();
            Frm_Log.Show();
            btnSettingsView.ForeColor = Color.Gray;
            btnStation03.ForeColor = Color.Gray;
            btnLogView.ForeColor = Color.White;
        }
        #endregion

        #region Private events and callbacks
        /// <summary>
        /// Callback fires when there is a problem with image logger.
        /// </summary>
        /// <param name="t">Task object.</param>
        private void ImageLoggerErrorHandler(Task t)
        {
            using (Form_Error errorFrm = new Form_Error(t.Exception.Message, t.Exception.ToString()))
            {
                errorFrm.ShowDialog();
            }

            // Close forms
            Invoke(new Action(() => Close()));
        }

        //TODO
        /// <summary>
        /// Callback fires when there is a problem with image logger.
        /// </summary>
        /// <param name="t">Task object.</param>
        private void ImageLoggerDebugErrorHandler1(Task t)
        {
            using (Form_Error errorFrm = new Form_Error(t.Exception.Message, t.Exception.ToString()))
            {
                errorFrm.ShowDialog();
            }

            // Close forms
            Invoke(new Action(() => Close()));
        }

        /// <summary>
        /// Callback fires when there is a problem with image logger.
        /// </summary>
        /// <param name="t">Task object.</param>
        private void ImageLoggerDebugErrorHandler2(Task t)
        {
            using (Form_Error errorFrm = new Form_Error(t.Exception.Message, t.Exception.ToString()))
            {
                errorFrm.ShowDialog();
            }

            // Close forms
            Invoke(new Action(() => Close()));
        }

        /// <summary>
        /// Callback fires when there is a problem with image logger.
        /// </summary>
        /// <param name="t">Task object.</param>
        private void ImageLoggerDebugErrorHandler3(Task t)
        {
            using (Form_Error errorFrm = new Form_Error(t.Exception.Message, t.Exception.ToString()))
            {
                errorFrm.ShowDialog();
            }

            // Close forms
            Invoke(new Action(() => Close()));
        }
        /// <summary>
        /// Callback fires when there is a problem with image logger.
        /// </summary>
        /// <param name="t">Task object.</param>
        private void ImageLoggerDebugErrorHandler4(Task t)
        {
            using (Form_Error errorFrm = new Form_Error(t.Exception.Message, t.Exception.ToString()))
            {
                errorFrm.ShowDialog();
            }

            // Close forms
            Invoke(new Action(() => Close()));
        }

        /// <summary>
        /// Event fires when there is a snap7 connection change.
        /// </summary>
        /// <param name="o">Sender.</param>
        /// <param name="e">Event args.</param>
        private void OnSnap7ConnectionStatusChanged(object o, Snap7onnectionStatusChangedEventArgs e)
        {
            // Set snap7 error flag according to the connection status
            if (e.ConnectionStatus == PLCConnectionStatus.Online)
            {
                Logger.AddEntry(LoggingLevel.Info, "Snap7 connection established");
                Snap7Error = false;
            }
            else
            {
                if(e.ConnectionStatus == PLCConnectionStatus.Error)
                    Logger.AddEntry(LoggingLevel.Error, "Snap7 error while reading or writing data");
                else
                    Logger.AddEntry(LoggingLevel.Error, "Snap7 connection cannot be established");

                Snap7Error = true;
            }
        }

        /// <summary>
        /// Event fires when new log entry is added.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void OnLogEntryAdded(object sender, LogEntryAddedEventArgs e)
        {
            Frm_Log.AddEntry(e.AddedLogEntry);
        }

        /// <summary>
        /// Event fires when form loads.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private async void Form_Main_Load(object sender, EventArgs e)
        {
            Hide();
            SetNativeEnabled(false);

            // Show loading form
            Frm_Loading = new Form_Loading();
            Frm_Loading.StartPosition = FormStartPosition.CenterScreen;
            Frm_Loading.TopMost = true;
            Frm_Loading.Show();

            // Find position settings for main window
            screens = Screen.AllScreens;
            bool displayFoundFlag = false;
            int displayCount = screens.Count();

            foreach (Screen cnt in screens)
            {
                if ((cnt.DeviceName == Settings.Default.ScreenName) && displayCount == Settings.Default.ScreenCount) displayFoundFlag = true;
            }

            if (Settings.Default.Maximised && displayFoundFlag == true)
            {
                Location = Settings.Default.Location;
                panelButton.Visible = false;
                WindowState = FormWindowState.Maximized;
                splitContainer1.SplitterDistance = 106;
                panelButton.Visible = true;
            }
            else if (Settings.Default.Minimised && displayFoundFlag == true)
            {
                Location = Settings.Default.Location;
                panelButton.Visible = false;
                WindowState = FormWindowState.Minimized;
                splitContainer1.SplitterDistance = 106;
                panelButton.Visible = true;
            }
            else if (displayFoundFlag == true)
            {
                Location = Settings.Default.Location;
                panelButton.Visible = false;
                WindowState = FormWindowState.Normal;
                splitContainer1.SplitterDistance = 106;
                panelButton.Visible = true;
            }

            Application.DoEvents();
            SetNativeEnabled(false);

            // Create snap7 communication manager and load db sections
            dynamic PLCSettings = MainSettings.GetSegment("Setting", "PLC");
            Snap7ComManager = new Snap7CommunicationManager(PLCSettings.IP, PLCSettings.Rack, PLCSettings.Slot);
            Snap7ComManager.ConnectionStatusChanged += OnSnap7ConnectionStatusChanged;
            LoadSnap7DataSections();

            // Create stations and log forms          
            Frm_03 = new Form_03(this, Station03Error, Logger, UserLogin, Station03Settings, MainSettings, Snap7ComManager, Frm_Loading.progressBarUpdate, ImageLoggerErrorHandler);
            Frm_Log = new Form_Log((int)MainSettings.GetElement("Setting", "Logging", "NumOfEntries"));

            Frm_03.MdiParent = this;
            Frm_Log.MdiParent = this;


            Application.DoEvents();

            // Load dummy toolblock and perform progress bar update
            Frm_Loading.progressBarUpdate("Loading dummy");
            Logger.AddEntry(LoggingLevel.Info, "Loading dummy");
            dummyEdit = new Form_DummyEditToolBlock();

            Application.DoEvents();

            // Initialize stations
           
            await Frm_03.Initialize();

            // Start snap7 manager and perform progress bar update
            Frm_Loading.progressBarUpdate("Starting snap7 communication");
            Logger.AddEntry(LoggingLevel.Info, "Starting snap7 communication");
            Snap7ComManager.Start();

            // Close loading form and set focus to main form
            Frm_Loading.closeForm();
            SetNativeEnabled(true);
            Show();
            Focus();

            // Show station 
            LogShow();
            Station03Show();

            // Start error and application status timers
            errorTimer.Start();
            applicationStatusTimer.Start();
        }

        /// <summary>
        /// Event fires when form is closing.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args.</param>
        private void Form_Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Close forms for stations           
            Frm_03.Close();

            // Unregister callback for logger
            Logger.LogEntryAddedEvent -= OnLogEntryAdded;

            // Close log form
            Frm_Log.Close();

            // Stop snap7 manager
            Snap7ComManager.Stop();

            // Save state of the application window
            currentScreen = Screen.FromControl(this);
            screens = Screen.AllScreens;
            Settings.Default.ScreenCount = screens.Count();
            Settings.Default.ScreenName = currentScreen.DeviceName;

            if (WindowState == FormWindowState.Maximized)
            {
                Settings.Default.Location = RestoreBounds.Location;
                Settings.Default.Maximised = true;
                Settings.Default.Minimised = false;

            }
            else if (WindowState == FormWindowState.Normal)
            {
                Settings.Default.Location = Location;
                Settings.Default.Maximised = false;
                Settings.Default.Minimised = false;
            }
            else
            {
                Settings.Default.Location = RestoreBounds.Location;
                Settings.Default.Maximised = false;
                Settings.Default.Minimised = true;
            }

            Settings.Default.Save();
        }

        /// <summary>
        /// Event fires when station03 button is clicked.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args</param>
        private void BtnStation03_Click(object sender, EventArgs e)
        {
            Station03Show();
        }
 
        /// <summary>
        /// Event fires when log button is clicked.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void logView_Click(object sender, EventArgs e)
        {
            LogShow();
        }

        /// <summary>
        /// Event fires when settings button is clicked.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void BtnSettingsView_Click(object sender, EventArgs e)
        {
            SetNativeEnabled(false);

            using (Form_LogIn LoginForm = new Form_LogIn(UserLogin, MainSettings))
            {
                // Check if user is logged in. If not show login form
                if (UserLogin.CurrentUser != null || LoginForm.ShowDialog() == DialogResult.OK)
                {
                    Frm_Settings = new Form_Settings(MainSettings, Station03Settings, DataManager);
                    Frm_Settings.IsConfirmed += Frm_SettingsIsConfirmed;
                    Frm_Settings.Show(this);
                }
                else
                    SetNativeEnabled(true);
            }
        }

        /// <summary>
        /// Event fires when exit button is clicked.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void btnExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Event fires when minimize button is clicked.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void btnMinimize_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        /// <summary>
        /// Event fires when restore button is clicked.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void btnRestore_Click(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {

                panelButton.Visible = false;
                WindowState = FormWindowState.Maximized;
                splitContainer1.SplitterDistance = 106;
                panelButton.Visible = true;
            }
            else if (WindowState == FormWindowState.Maximized)
            {
                panelButton.Visible = false;
                WindowState = FormWindowState.Normal;
                splitContainer1.SplitterDistance = 106;
                panelButton.Visible = true;
            }
        }

        /// <summary>
        /// Event fires when settings are confirmed.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="res">Event args.</param>
        private void Frm_SettingsIsConfirmed(object sender, EventArgs e)
        {
            Frm_Settings.Close();
            SetNativeEnabled(true);
        }

        /// <summary>
        /// Event fires when header is selected and mouse is moved.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void picBox_Header_MouseMove(object sender, MouseEventArgs e)
        {
            if (WindowState != FormWindowState.Maximized)
            {
                if (e.Button == MouseButtons.Left)
                {
                    Location = new Point(Location.X + e.X - mLastPos.X,
                      Location.Y + e.Y - mLastPos.Y);
                }
                // NOTE: else is intentional!
                else mLastPos = e.Location;
            }
        }

        /// <summary>
        /// Event fires when blank header is selected and mouse is moved.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void picBox_HeaderBlank_MouseMove(object sender, MouseEventArgs e)
        {
            if (WindowState != FormWindowState.Maximized)
            {
                if (e.Button == MouseButtons.Left)
                {
                    Location = new Point(Location.X + e.X - mLastPos.X,
                      Location.Y + e.Y - mLastPos.Y);
                }
                // NOTE: else is intentional!
                else mLastPos = e.Location;
            }
        }

        /// <summary>
        /// Event fires when header is double clicked.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void picBox_Header_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {

                panelButton.Visible = false;
                WindowState = FormWindowState.Maximized;
                splitContainer1.SplitterDistance = 106;
                panelButton.Visible = true;
            }
            else if (WindowState == FormWindowState.Maximized)
            {
                panelButton.Visible = false;
                WindowState = FormWindowState.Normal;
                splitContainer1.SplitterDistance = 106;
                panelButton.Visible = true;
            }
        }

        /// <summary>
        /// Event fires when blank header is double clicked.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void picBox_HeaderBlank_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {

                panelButton.Visible = false;
                WindowState = FormWindowState.Maximized;
                splitContainer1.SplitterDistance = 106;
                panelButton.Visible = true;
            }
            else if (WindowState == FormWindowState.Maximized)
            {
                panelButton.Visible = false;
                WindowState = FormWindowState.Normal;
                splitContainer1.SplitterDistance = 106;
                panelButton.Visible = true;
            }
        }

        /// <summary>
        /// Event fires when header text is double clicked.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void labelForm_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (WindowState == FormWindowState.Normal)
            {

                panelButton.Visible = false;
                WindowState = FormWindowState.Maximized;
                splitContainer1.SplitterDistance = 106;
                panelButton.Visible = true;
            }
            else if (WindowState == FormWindowState.Maximized)
            {
                panelButton.Visible = false;
                WindowState = FormWindowState.Normal;
                splitContainer1.SplitterDistance = 106;
                panelButton.Visible = true;
            }
        }

        /// <summary>
        /// Event fires when header text is selected and mouse is moved.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void labelForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (WindowState != FormWindowState.Maximized)
            {
                if (e.Button == MouseButtons.Left)
                {
                    Location = new Point(Location.X + e.X - mLastPos.X,
                      Location.Y + e.Y - mLastPos.Y);
                }
                // NOTE: else is intentional!
                else mLastPos = e.Location;
            }
        }

        /// <summary>
        /// Event fires when winforms error timer elapses.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void ErrorTimer_Tick(object sender, EventArgs e)
        {
            // Station3
            if (Station03Error.Value)
            {
                // Toggle color if error is active
                if (StationsErrorToggle)
                    btnStation03.ForeColor = Color.Firebrick;
                else
                {
                    //if (Frm_09.Visible)
                    //    btnStation03.ForeColor = Color.White;
                    //else
                    //    btnStation03.ForeColor = Color.Gray;
                }
            }
            else
            {
                // Restore color to its default state
                if (Frm_03.Visible)
                    btnStation03.ForeColor = Color.White;
                else
                    btnStation03.ForeColor = Color.Gray;
            }

            // Snap7
            if (Snap7Error)
            {
                // Toggle color if error is active
                if (StationsErrorToggle)
                    btnLogIn.BackgroundImage = Resources.Offline;
                else
                    btnLogIn.BackgroundImage = Resources.Online;
            }
            else
                btnLogIn.BackgroundImage = Resources.Online;

            // Toggle station error flag
            StationsErrorToggle = !StationsErrorToggle;
        }

        /// <summary>
        /// Event fires when application status timer elapses.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void ApplicationStatusTimer_Tick(object sender, EventArgs e)
        {
            byte ApplicationStatus = 0;

            // Set alive status bit
            HelperMethods.SetStatusBit(ApplicationAliveFlagPos, true, ref ApplicationStatus);

            // Set application status
            Snap7ComManager.WriteValue("Application_Status:Byte", new List<object>() { ApplicationStatus });
        }
        #endregion
    }
}
