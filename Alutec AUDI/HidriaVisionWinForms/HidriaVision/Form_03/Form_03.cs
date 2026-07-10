using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Logger;
using XMLSettings;
using BaslerCam;
using ToolblockManager;

using System.Collections.Generic;
using System.Threading;
using UserManagment.LoginManagment;
using LedController;
using Logger.ImageLogger;
using System.Drawing.Imaging;
using Snap7Manager;
using HidriaVision.Helpers;


namespace HidriaVision
{
    public partial class Form_03 : Form
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

        #region Private fields
        // Editing variables
        private SharedVar<bool> InEditing;
        private SharedVar<bool> InEditCalibration;
        private XMLSettingsManager EditingParameters;
        private SharedVar<Bitmap> LastSFXFirstImage;
        private SharedVar<Bitmap> LastSFXSecondImage;
        private SharedVar<int> TriggerSource;

        // Camera 01 variables
        private BaslerCamera Camera01;
        private SharedVar<bool> Camera01SFXImagesReady;
        private SharedVar<bool> Camera01Error;
        private SharedVar<string> Camera01ID;
        private SharedVar<Bitmap> Camera01SFXImages;
        private SharedVar<dynamic> Camera01Exposures;
        private SharedVar<dynamic> Camera01Intensities;
        private byte Camera01NumOfReconnectErrorLogs;

        // Led controller variables
        private RGBWController LedController;
        private SharedVar<bool> LedControllerError;
        private byte LedControllerNumOfReconnectErrorLogs;

        // Motor Variables
        private SharedVar<bool> StepperMotorError;

        // Main operation variables
        private SharedVar<bool> PreviousStationError;
        private SharedVar<bool> PreviousStationReady;
        private SharedVar<bool> StationError;
        private SharedVar<bool> StationReady;
        private SharedVar<bool> ToolblockResultsValid;
        private SharedVar<bool> ToolblockProcessed;
        private SharedVar<byte> ResultRequest;
        private SharedVar<byte> ResultResponse;
        private SharedVar<byte> TriggerCountRequest;
        private SharedVar<byte> TriggerCountResponse;
        private SharedVar<string> DMC_codeRequest;
        private SharedVar<string> DMC_codeResponse;
        private SharedVar<float> FoundAtAngleResponse;
        private SharedVar<bool> MainOperationCycleStart;
        private SharedVar<bool> MainOperationCycleCancel;
        private SharedVar<long> CycleTime;
        private SharedVar<Station03CommonStatusFlags> Status;
        private object MainOperationObjLock;
        private CancellationTokenSource MainOperationCancel;
        private Thread MainOperationThread;

        // Manager variables      
        private SharedVar<XMLSettingsManager> Parameters;
        private XMLSettingsManager StationSettings;
        private XMLSettingsManager MainSettings;
        private LoginManager UserLogin;
        private Snap7CommunicationManager Snap7ComManager;

        // Other variables
        private SharedVar<Station03CommonResults> CommonResults;
        private SharedVar<bool> OrientationOK;
        private SharedVar<bool> PartPresent;
        private SharedVar<byte> CurrentResponse;
        private SharedVar<double> FoundAtAngle;
        private SharedVar<byte> CurrentType;
        private SharedVar<byte> CurrentResultRequest;
        private Action<string> ProgressBarUpdate;
        private Action<Task> ImageLoggerErrorHandler;
        private SharedVar<FileImageLogger> ImageLogger;
        private FileEventLogger Logger;
        private Form_Main MainForm;
        private bool toogleLogView;
        
        #endregion

        #region Additional Forms
        Form_ToolBLockList Frm_ToolBlockList;
        Form_EditToolBlock Frm_EditToolBlock;
        #endregion

        #region Additional Controls
        // Control for SFX
        private SFXDriverControl SFXDriverCtrl;
        private TriggerImagesSFX TriggerOneImageCtrl;
        #endregion

        public Form_03(Form_Main mainForm, SharedVar<bool> stationError, FileEventLogger logger, LoginManager userLogin, XMLSettingsManager stationSettings, XMLSettingsManager mainSettings, Snap7CommunicationManager snap7ComManager, Action<string> progressBarUpdate, Action<Task> imageLoggerErrorHandler)
        {
            InitializeComponent();

            // Editing variables init
            InEditing = new SharedVar<bool>();
            InEditCalibration = new SharedVar<bool>();
            LastSFXFirstImage = new SharedVar<Bitmap>();
            LastSFXSecondImage = new SharedVar<Bitmap>();
            TriggerSource = new SharedVar<int>();

            // Camera 01 variables init
            Camera01 = new BaslerCamera();
            Camera01SFXImagesReady = new SharedVar<bool>();
            Camera01Error = new SharedVar<bool>();
            Camera01ID = new SharedVar<string>();
            Camera01SFXImages = new SharedVar<Bitmap>();
            Camera01Exposures = new SharedVar<dynamic>();
            Camera01Intensities = new SharedVar<dynamic>();
            Camera01NumOfReconnectErrorLogs = 0;

            // Led controller variables
            LedControllerError = new SharedVar<bool>();
            LedControllerNumOfReconnectErrorLogs = 0;
            LedController = null;

            //Motor variables
            StepperMotorError = new SharedVar<bool>();

            // Main operation variables initialization
            PreviousStationError = new SharedVar<bool>();
            PreviousStationReady = new SharedVar<bool>();
            StationError = stationError;
            StationReady = new SharedVar<bool>();
            ToolblockResultsValid = new SharedVar<bool>();
            ToolblockProcessed = new SharedVar<bool>();
            MainOperationCycleStart = new SharedVar<bool>();
            MainOperationCycleCancel = new SharedVar<bool>();
            CycleTime = new SharedVar<long>();
            MainOperationCancel = new CancellationTokenSource();
            MainOperationThread = null;
            ResultRequest = new SharedVar<byte>();
            ResultResponse = new SharedVar<byte>();
            DMC_codeRequest = new SharedVar<string>();
            DMC_codeResponse = new SharedVar<string>();
            TriggerCountRequest = new SharedVar<byte>();
            TriggerCountResponse = new SharedVar<byte>();
            
            FoundAtAngleResponse = new SharedVar<float>();
            Status = new SharedVar<Station03CommonStatusFlags>();
            MainOperationObjLock = new object();

            // Manager variables initialization        
            Parameters = new SharedVar<XMLSettingsManager>();
            StationSettings = stationSettings;
            MainSettings = mainSettings;
            Snap7ComManager = snap7ComManager;

            // Other variables initialization
            CommonResults = new SharedVar<Station03CommonResults>();
            CurrentType = new SharedVar<byte>();
            CurrentResultRequest = new SharedVar<byte>();
            OrientationOK = new SharedVar<bool>();
            PartPresent = new SharedVar<bool>();
            CurrentResponse = new SharedVar<byte>();
            FoundAtAngle = new SharedVar<double>();
            ProgressBarUpdate = progressBarUpdate;
            ImageLoggerErrorHandler = imageLoggerErrorHandler;
            ImageLogger = new SharedVar<FileImageLogger>();
            Logger = logger;
            UserLogin = userLogin;
            MainForm = mainForm;
            toogleLogView = true;
        }

        #region Public Methods
        /// <summary>
        /// Initializes Station03.
        /// </summary>
        /// <returns>Returns Task object.</returns>
        public async Task Initialize()
        {


            // Perform progress bar update
            ProgressBarUpdate("Station03: Initializing XML managers");
            Logger.AddEntry(LoggingLevel.Info, "Station03: Initializing XML managers");

            // Initialize XML managers
            Parameters.Value = new XMLSettingsManager($"./Station03/Types/Type000_Tip1/P000_Default/", "Parameters.xml");

            // Perform progress bar update
            ProgressBarUpdate("Station06: Loading snap7 db sections");
            Logger.AddEntry(LoggingLevel.Info, "Station06: Loading db sections");


            // Load snap7 db sections
            LoadSnap7DbSections();

            //// Perform progress bar update
            //ProgressBarUpdate("Station03: Initializing led driver");
            //Logger.AddEntry(LoggingLevel.Info, "Station03: Initializing led driver");

            //// Initialize led controller
            //try
            //{
            //    LedController = new RGBWController((string)StationSettings.GetElement("SerialConnection", "LedController01", "PortName"));

            //    // Open the connection
            //    await Task.Run(() => LedController.OpenPort(Rs232baudrate.Baudrate_115200));


            //    //lock (LedController)
            //    //{
            //    //    try
            //    //    {
            //    //        byte LedIntensity = (byte)Parameters.Value.GetElement("LedControllerIntensites", "Camera1Intensities", "Intensity");
            //    //        LedController.SetLedChannels(LedIntensity, LedIntensity, LedIntensity, LedIntensity);
            //    //        // Set surface FX
            //    //        LedController.SetLedTriggerMode(LedTriggerMode.External);
            //    //        LedController.SetSurfaceFx(SurfaceFxState.Enabled);
            //    //        LedController.SetSurfaceFxMode(SurfaceFxMode.Trigger);
            //    //        LedController.SetSurfaceFxTriggerDelay(50);
            //    //    }
            //    //    catch (Exception e) when (e is InvalidOperationException || e is TimeoutException || e is ControllerOperationFailedException)
            //    //    {
            //    //        // Add entry to a log
            //    //        Logger.AddEntry(LoggingLevel.Error, $"Station03: {e.Message}");
            //    //        LedControllerError.Value = true;
            //    //    }
            //    //}
            //    // Add log entry
            //    Logger.AddEntry(LoggingLevel.Info, "Station03: Led driver initialized");
            //}
            //catch (InvalidOperationException e)
            //{
            //    // Just Add entry to a log and set error flag
            //    Logger.AddEntry(LoggingLevel.Error, $"Station03: {e.Message}");
            //    LedControllerError.Value = true;
            //}

            // Perform progress bar update
            ProgressBarUpdate("Station06: Initializing camera 01");
            Logger.AddEntry(LoggingLevel.Info, "Station06: Initializing camera 01");

            // Initialize camera 1
            try
            {
                Camera01ID.Value = (string)StationSettings.GetElement("Camera", "Camera01", "ID");
                Camera01Exposures.Value = Parameters.Value.GetSegment("Exposures", "Camera1Exposures");

                // Subscribe to events
                Camera01.CameraDisconnectedEvent += Camera01_Disconnected;
                Camera01.CameraGrabErrorEvent += Camera01_GrabError;
                Camera01.OneShotImageReadyEvent += Camera01_ImagesReady;


                // Open the camera
                await Task.Run(() => Camera01.CameraOpen(Camera01ID.Value));

                // Set initial parameters
                Camera01.ApplyUserSet(ConfigurationSetSelector.UserSet1);
                Camera01.SetExposure((long)(Camera01Exposures.Value.Exposure * 1000));

                // Add log entry
                Logger.AddEntry(LoggingLevel.Info, "Station06: Camera 01 initialized");
            }
            catch (CameraException e)
            {
                // Just Add entry to a log and set error flag
                Logger.AddEntry(LoggingLevel.Error, $"Station06: {e.Message}");
                Camera01Error.Value = true;
            }

            // Perform progress bar update
            ProgressBarUpdate("Station06: Prepare image logger");
            Logger.AddEntry(LoggingLevel.Info, "Station03: Prepare image logger");

            // Create image logger
            //ImageLogger.Value = new FileImageLogger("./Log/ImageLog", "Station06", 0, 0, ImageLoggerErrorHandler);
            ImageLogger.Value = new FileImageLogger(@"C:\Slike", "Station06", 0, 0, ImageLoggerErrorHandler);
            ImageLogger.Value.MaxNumberOfImages = 200 * 2;
            ImageLogger.Value.Prepare();
            ImageLogger.Value.Start();

            // Perform progress bar update
            ProgressBarUpdate("Station06: Initialization sequence completed");
            Logger.AddEntry(LoggingLevel.Info, "Station06: Initialization sequence completed");

            // Start the main operation thread              
            MainOperationThread = new Thread(() => MainOperation(MainOperationCancel.Token));
            MainOperationThread.Start();
        }

        #region Private Methods 
        private void UpdateCommonStatusControl()
        {
            // Update common control
            Form_03_CommonControl1.SetCommonStatus(new Station03CommonStatus()
            {
         
                ResultRequest = ResultRequest.Value,
                ResultResponse = ResultResponse.Value,
                
                AngleResponse = FoundAtAngleResponse.Value,
                CycleTime = CycleTime.Value,
                Status = Status.Value
            });
        }
        #endregion

        /// <summary>
        /// Enables or disables a form.
        /// </summary>
        /// <param name="enabled"></param>
        private void SetNativeEnabled(bool enabled)
        {
            SetWindowLong(Handle, GWL_STYLE, GetWindowLong(Handle, GWL_STYLE) &
                ~WS_DISABLED | (enabled ? 0 : WS_DISABLED));

            MainForm.SetNativeEnabled(enabled);
        }
        #endregion

        #region Private Events
        /// <summary>
        /// Event fires when form resizes.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void Form_03_Resize(object sender, EventArgs e)
        {
            int heightDisplay = splitContainerDisplay.Panel1.Height;
            int widthDisplay = splitContainerDisplay.Panel1.Width;
  
        }

        /// <summary>
        /// Event fires when split container boundary is moved.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void splitContainerDisplay_SplitterMoved(object sender, SplitterEventArgs e)
        {
            int heightDisplay = splitContainerDisplay.Panel1.Height;
            int widthDisplay = splitContainerDisplay.Panel1.Width;
       
        }

        /// <summary>
        /// Event occurs when the form is closing.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event arguments.</param>
        private void Form_03_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Terminate the main operation thread
            if(MainOperationThread != null)
            {
                MainOperationCancel.Cancel();
                MainOperationThread.Join(500);
            }

            // Stop image logger
            ImageLogger.Value.Stop();
            ImageLogger.Value.Dispose();

            // Close the camera
            Camera01.CameraClose();
        }

        /// <summary>
        /// Event fires when display record changes.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void cogRecordsDisplay_RecordChange(object sender, EventArgs e)
        {
            
        }

        /// <summary>
        /// Event fires when display record is double clicked.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void cogRecordsDisplay_DoubleClick(object sender, EventArgs e)
        {
            
        }
     
        /// <summary>
        /// Event fires when button for screenshoot is clicked.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void btnSnapShot_Click(object sender, EventArgs e)
        {
            Size sz = this.Bounds.Size;

            IntPtr hDesk = GetDesktopWindow();
            IntPtr hSrce = GetWindowDC(hDesk);
            IntPtr hDest = CreateCompatibleDC(hSrce);
            IntPtr hBmp = CreateCompatibleBitmap(hSrce, sz.Width, sz.Height);
            IntPtr hOldBmp = SelectObject(hDest, hBmp);

            bool b = BitBlt(hDest, 0, 0, sz.Width, sz.Height, hSrce, this.PointToScreen(System.Drawing.Point.Empty).X, this.PointToScreen(System.Drawing.Point.Empty).Y, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);
            Bitmap bmp = Bitmap.FromHbitmap(hBmp);
            SelectObject(hDest, hOldBmp);
            DeleteObject(hBmp);
            DeleteDC(hDest);
            ReleaseDC(hDesk, hSrce);

            SaveFileDialog saveImgDialog = new SaveFileDialog();
            //saveImgDialog.Filter = Form_03_InfoLanguage[1];
            //saveImgDialog.Title = Form_03_InfoLanguage[2];
            saveImgDialog.RestoreDirectory = true;

            DateTime saveNow = DateTime.Now;

            /*saveImgDialog.FileName = Form_03_InfoLanguage[3] + saveNow.Year.ToString() + "-" + saveNow.Month.ToString("00") + "-" + saveNow.Day.ToString("00") + "_" + saveNow.Hour.ToString("00") + "h" +
                 saveNow.Minute.ToString("00") + "m" + saveNow.Second.ToString("00") + "s";*/

            ImageFormat format = ImageFormat.Jpeg;

            if (saveImgDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string ext = System.IO.Path.GetExtension(saveImgDialog.FileName);
                switch (ext)
                {
                    case ".jpg":
                        format = ImageFormat.Jpeg;
                        break;
                    case ".bmp":
                        format = ImageFormat.Bmp;
                        break;
                    case ".png":
                        format = ImageFormat.Png;
                        break;
                }
                bmp.Save(saveImgDialog.FileName, format);
            }

            bmp.Dispose();
        }

        /// <summary>
        /// Event fires when button for parameters is clicked.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void btnParameters_Click(object sender, EventArgs e)
        {
            using (Form_LogIn LoginForm = new Form_LogIn(UserLogin, MainSettings))
            {
                // Check if user is logged in. If not show login form
                if (UserLogin.CurrentUser != null || LoginForm.ShowDialog() == DialogResult.OK)
                {

                }
            }
        }
    
        /// <summary>
        /// Event fires when button for edit toolblock is clicked.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void btnEdit_Click(object sender, EventArgs e)
        {
            SetNativeEnabled(false);

            using (Form_LogIn LoginForm = new Form_LogIn(UserLogin, MainSettings))
            {
                // Check if user is logged in. If not show login form
                if(UserLogin.CurrentUser != null || LoginForm.ShowDialog() == DialogResult.OK)
                {
                    InEditing.Value = true;

                }
                else
                    SetNativeEnabled(true);
            }
        }  
        
        /// <summary>
        /// Event fires when toolblock is confirmed to be edited.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="res">Event args.</param>
        private async void Frm_ToolBlockListIsConfirmed(object sender, DialogResult res)
        {
            if (res == DialogResult.OK)
            {
                Frm_EditToolBlock = new Form_EditToolBlock(Logger);
                BaslerCamera EditingCamera = Camera01;
                float EditingExposureFirst = 0.0f;
                byte EditingIntensity = 0;



                Frm_EditToolBlock.HeaderName = $"Station03 / {Frm_ToolBlockList.SelectedTypeFullName} / {Frm_ToolBlockList.SelectedToolBlockFullName}";               
                EditingParameters = new XMLSettingsManager($"./Station03/Types/{Frm_ToolBlockList.SelectedTypeFullName}/{Frm_ToolBlockList.SelectedProgramFullName}/", "Parameters.xml");

                InEditCalibration.Value = Frm_ToolBlockList.IsSelectedToolBlockCalibration;
                Frm_ToolBlockList.Hide();

                Frm_EditToolBlock.FormIsConfirmed += Frm_EditToolBlockIsConfirmed;
                Frm_EditToolBlock.EnableToolBlockModule = false;

                // SFX Control
                SFXDriverCtrl = new SFXDriverControl(LedController, LedControllerError, Logger);

                //// Trigger Control for SFX
                TriggerOneImageCtrl = new TriggerImagesSFX(EditingCamera, Logger, Camera01Error, SFXDriverCtrl, TriggerSource);
                TriggerOneImageCtrl.SetTriggerSource = 1;

                try
                { 
                    // Select appropriate toolblock and exposure
                    if (!Frm_ToolBlockList.IsSelectedToolBlockCalibration)
                    {
                        // We are editing toolblock
                        switch (Frm_ToolBlockList.SelectedToolBlockFullName)
                        {
                            default:
                                EditingCamera = Camera01;
                                EditingExposureFirst = (float)EditingParameters.GetElement("Exposures", "Camera1Exposures", "Exposure");
                                EditingIntensity = (byte)EditingParameters.GetElement("LedControllerIntensites", "Camera1Intensities", "Intensity");
                                break;
                        }                  
                    }
                    else
                    {
                        // We are editing calibration
                        switch (Frm_ToolBlockList.SelectedToolBlockFullName)
                        {
                            default:
                                EditingCamera = Camera01;
                                EditingExposureFirst = (float)EditingParameters.GetElement("Exposures", "Camera1Exposures", "ExposureCalibration");
                                EditingIntensity = (byte)EditingParameters.GetElement("LedControllerIntensites", "Camera1Intensities", "Intensity");
                                break;
                        }                            
                    }
                }
                catch (Exception e)
                {
                    // Just Add entry to a log and set error flag
                    Logger.AddEntry(LoggingLevel.Error, $"Station03: {e.Message}");
                }


                TriggerOneImageCtrl.ButtonName = "Trigger images";
                TriggerOneImageCtrl.CameraExposure = EditingExposureFirst;
                SFXDriverCtrl.IntensityValue = EditingIntensity;


                Frm_EditToolBlock.AddControl(TriggerOneImageCtrl);
                Frm_EditToolBlock.AddControl(SFXDriverCtrl);

                // Subscribe to events
                TriggerOneImageCtrl.LoadImagesFromDisc += LoadImage1FromDisc;
                TriggerOneImageCtrl.SaveImage1ToDisc += SaveImage1ToDisc;
                TriggerOneImageCtrl.SaveImage2ToDisc += SaveImage2ToDisc;

                Frm_EditToolBlock.Show(this);
                Application.DoEvents();

                // Load editing toolblock async               
                await Task.Run(() =>
                {            

                    // Set LastImages so that they are not empty, allways save and load images from calibration!
                    try
                    {
                       
                    }
                    catch { }

                                    
                });                              
            }
            else
            {
                Frm_ToolBlockList.FormIsConfirmed -= Frm_ToolBlockListIsConfirmed;
                Frm_ToolBlockList.Close();
                Frm_ToolBlockList.Dispose();

                InEditing.Value = false;
                InEditCalibration.Value = false;
                SetNativeEnabled(true);
            }          
        }

        /// <summary>
        /// Event fires when editing toolblock is saved.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="res">Event args.</param>
        private void Frm_EditToolBlockIsConfirmed(object sender, DialogResult res)
        {
            if (res == DialogResult.OK)
            {
                try
                { 
                    // Save parameters here                 
                    if (!Frm_ToolBlockList.IsSelectedToolBlockCalibration)
                    {
                        switch (Frm_ToolBlockList.SelectedToolBlockFullName)
                        {
                            // We want to save toolblock parameters
                            default:
                                EditingParameters.SetElement("Exposures", "Camera1Exposures", "Exposure", TriggerOneImageCtrl.CameraExposure);
                                EditingParameters.SetElement("LedControllerIntensites", "Camera1Intensities", "Intensity", SFXDriverCtrl.IntensityValue);
                                break;
                        }
                    }
                    else
                    {
                        // We want to save calibration parameters
                        switch (Frm_ToolBlockList.SelectedToolBlockFullName)
                        {
                            default:
                                EditingParameters.SetElement("Exposures", "Camera1Exposures", "ExposureCalibration", TriggerOneImageCtrl.CameraExposure);
                                EditingParameters.SetElement("LedControllerIntensites", "Camera1Intensities", "IntensityCalibration", SFXDriverCtrl.IntensityValue);
                                break;
                        }
                    }

                    // Reload toolblocks
                    lock (MainOperationObjLock)
                    {
                        bool IsMainOperationCycleStarted = MainOperationCycleStart.Value;
                       
                    }
                }
                catch (Exception e)
                {
                    // Just Add entry to a log and set error flag
                    Logger.AddEntry(LoggingLevel.Error, $"Station03: {e.Message}");
                }
            }
            else
            {
                // Closed editing form withoud confirm... DO nothing
            }

            // Unsubscribe to events           
            TriggerOneImageCtrl.LoadImagesFromDisc -= LoadImage1FromDisc;
            TriggerOneImageCtrl.SaveImage1ToDisc -= SaveImage1ToDisc;
            TriggerOneImageCtrl.SaveImage2ToDisc -= SaveImage2ToDisc;

            Frm_EditToolBlock.FormIsConfirmed -= Frm_EditToolBlockIsConfirmed;

            // Close EditToolBlock and show ToolBlockList
            Frm_EditToolBlock.Close();
            Frm_EditToolBlock.Dispose();
            Frm_ToolBlockList.Show(this);
        }

        /// <summary>
        /// Event fires when button for results log is clicked.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void ButtonShowLog_Click(object sender, EventArgs e)
        {
            if (toogleLogView)
            {
                splitContainerDisplay.Panel2Collapsed = true;
            }
            else
            {
                splitContainerDisplay.Panel2Collapsed = false;
            }

            int heightDisplay = splitContainerDisplay.Panel1.Height;
            int widthDisplay = splitContainerDisplay.Panel1.Width;


            toogleLogView = !toogleLogView;
        }
        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            MainOperationCycleStart.Value = true;
        }
    }
}
