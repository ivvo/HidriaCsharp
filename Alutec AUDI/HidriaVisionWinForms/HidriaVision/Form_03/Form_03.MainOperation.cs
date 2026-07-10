using BaslerCam;
using LedController;
using Logger;
using Logger.ImageLogger;
using ResultsView;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace HidriaVision
{
    public partial class Form_03 : Form
    {
        #region Private methods
        /// <summary>
        /// This method is the main operation task for the vision logic.
        /// </summary>
        /// <param name="token">Cancellation token for cancelling the task operation.</param>
        private void MainOperation(CancellationToken token)
        {
            int MainOperationStep = 0;
  
            string CycleStartId = default;
            DateTime CycleTimeStamp = default;
           
            Station03CommonLogResults CommonLogResults = default;
            Stopwatch CycleWatch = new Stopwatch();
            Stopwatch CameraTimeout = new Stopwatch();
            Stopwatch LedDriverTimeout = new Stopwatch();
            Stopwatch AngleTimeout = new Stopwatch();


            LedDriverTimeout.Start();

            // Main loop
            while(!token.IsCancellationRequested)
            {
                // Handle errors
                HandleErrors();

                // Check if operation cycle cancellation has been requested
                if (MainOperationCycleCancel.Value)
                {
                    // Reset the flags and steps
                    MainOperationCycleStart.Value = false;
                    MainOperationStep = 0;
                }

            
            // Check if operation cycle start has been requested
            if (MainOperationCycleStart.Value)
                {
                    switch (MainOperationStep)
                    {
                        case (int)Station03OperationSteps.StartStep:
                            // Reset all flags and results
                            CycleTime.Value = 0;
                            
                            CommonLogResults = default;
                            CommonResults.Value = default;
                            ToolblockResultsValid.Value = false;
                            Camera01SFXImagesReady.Value = false;
                            ToolblockProcessed.Value = false;
                            OrientationOK.Value = false;
                            // Update common status control.
                            UpdateCommonStatusControl();
                            

                            // Write data to snap7
                            WriteSnap7Data();
                                                        
                            // Generate start id
                            CycleTimeStamp = DateTime.Now;
                            CycleStartId = $"{CycleTimeStamp.ToString("yyyyMMdd")}_{CycleTimeStamp.ToString("HHmmss")}_{CycleTimeStamp.ToString("fff")}";

                            // Start measuring cycle time
                            CycleWatch.Restart();
                            MainOperationStep = (int)Station03OperationSteps.Initialization;
                            break;

                        case (int)Station03OperationSteps.Initialization:

                           
                                lock (Camera01)
                                {
                                    float CameraExposure = (float)Parameters.Value.GetElement("Exposures", "Camera1Exposures", "Exposure");

                                    try
                                    {
                                        // Set camera exposure if there is a difference
                                        if (CameraExposure * 1000 != Camera01.GetExposure())
                                            Camera01.SetExposure((long)(CameraExposure * 1000));

                                        //Camera01.SetCameraOutput(true);

                                        // Grab ImageGrabNumber images in SFX mode
                                        Camera01SFXImagesReady.Value = false;
                                        Camera01.StopGrab();
                                        Camera01.OneShot();

                                        // Restart camera timer for timeout
                                        CameraTimeout.Restart();
                                        // Wait that PLC send back angle answer, if not in timeout throw exception
                                        AngleTimeout.Restart();
                                        // Go to next step
                                        MainOperationStep = (int)Station03OperationSteps.CalibrationRun;
                                    }
                                    catch (Exception e) when (e is CameraException || e is InvalidOperationException)
                                    {
                                        // Add entry to a log
                                        Logger.AddEntry(LoggingLevel.Error, $"Station06: {e.Message}");
                                        Camera01Error.Value = true;

                                        // Terminate the cycle
                                        MainOperationStep = (int)Station03OperationSteps.ShowResults;
                                    }

                                }
                                                         
                            break;

                        case (int)Station03OperationSteps.CalibrationRun:
                            // Check if image has been grabbed. Check also for timeout
                                                       
                            if (Camera01SFXImagesReady.Value)
                            {
                               
                                // Go to next step
                                MainOperationStep = (int)Station03OperationSteps.ToolblockRun;
                            }
                            else if (CameraTimeout.ElapsedMilliseconds > 2000)
                            {
                                // Add entry to a log
                                Logger.AddEntry(LoggingLevel.Error, $"Station06: Camera01 timeout");
                                Camera01Error.Value = true;

                                // Terminate the cycle
                                MainOperationStep = (int)Station03OperationSteps.ShowResults;
                            }
                            break;

                        case (int)Station03OperationSteps.ToolblockRun:
                           

                                // Go to next step
                                MainOperationStep = (int)Station03OperationSteps.ProcessResults;
                            
                            break;

                        case (int)Station03OperationSteps.ProcessResults:

                            
                            // Check if toolblock has been processed
                            MainOperationStep = (int)Station03OperationSteps.ShowResults;

                            break;
                        case (int)Station03OperationSteps.ShowResults:
                            lock (MainOperationObjLock)
                            {
                                bool CycleCancelled = MainOperationCycleCancel.Value;
                                bool EndStatusOK = !Camera01Error.Value;

                                //string DMC_CODE_NAME = DMC_codeRequest.Value.Replace('*', '_');

                                string ConstructedImageSFX1Name = $"{CycleStartId}_I01.bmp";
                               // string ConstructedImageSFX1Name = $"{DMC_CODE_NAME}_I01.bmp";
                                

                                //Camera01.SetCameraOutput(false);

                                // Stop measuring cycle time
                                CycleWatch.Stop();

                                // Construct log result
                                CommonLogResults.StartID = CycleStartId;
                                
                                CommonLogResults.CycleTime = $"{CycleWatch.ElapsedMilliseconds.ToString()}ms";

                                //CommonLogResults.Image1 = new ImageSource($"./Log/ImageLog/Station06/Type000/P000/OK/{ConstructedImageSFX1Name}");
                                CommonLogResults.Image1 = new ImageSource($@"C:\Slike\Station06\Type000\P000\OK\{ConstructedImageSFX1Name}");
                                //CommonLogResults.Image2 = new ImageSource($"./Log/ImageLog/Station03/Type{TypeResponse.Value:000}/P000/OK/{ConstructedImageSFX2Name}");


                                if (CycleCancelled)
                                    CommonLogResults.EndStatus = new ResultStatus<string>("INTERRUPTED", ResultStatus.Interrupted);
                                else if (EndStatusOK)
                                    CommonLogResults.EndStatus = new ResultStatus<string>("OK", ResultStatus.Ok);
                                else
                                    CommonLogResults.EndStatus = new ResultStatus<string>("NOK", ResultStatus.Nok);

                                // Add log result to results log view 
                                Form_03_ResultsLogView.AddEntry(CommonLogResults);   

                                // Save a copy of the last captured image, overwriting the previous one
                                try
                                {
                                    Directory.CreateDirectory(@"C:\Slike\ZadnjaSlika");
                                    using (Bitmap imgCopy = (Bitmap)Camera01SFXImages.Value?.Clone())
                                    {
                                        imgCopy?.Save(@"C:\Slike\ZadnjaSlika\ZadnjaSlika.bmp");
                                    }
                                }
                                catch (Exception e)
                                {
                                    Logger.AddEntry(LoggingLevel.Error, $"Station03: Napaka pri shranjevanju zadnje slike: {e.Message}");
                                }

                                // Show the last captured image on screen
                                try
                                {
                                    Bitmap displayImage = (Bitmap)Camera01SFXImages.Value?.Clone();
                                    if (displayImage != null && pictureBoxDisplay.IsHandleCreated)
                                    {
                                        pictureBoxDisplay.BeginInvoke(new Action(() =>
                                        {
                                            Image oldImage = pictureBoxDisplay.Image;
                                            pictureBoxDisplay.Image = displayImage;
                                            oldImage?.Dispose();
                                        }));
                                    }
                                }
                                catch (Exception e)
                                {
                                    Logger.AddEntry(LoggingLevel.Error, $"Station03: Napaka pri prikazu slike: {e.Message}");
                                }

                                ImageLogger.Value.AddEntry(new ImageLogEntry(Camera01SFXImages.Value, CycleTimeStamp, 1, true));
                                //ImageLogger.Value.AddEntry(new ImageLogEntry(Camera01SFXImages.Value[1], CycleTimeStamp, 2, true));

                                // Add entry to productivity control
                                if (CycleCancelled)
                                    productivityControl.CountUpINTERRUPTED();
                                else if (EndStatusOK)
                                    productivityControl.CountUpOK();
                                else
                                    productivityControl.CountUpNOK();
                            }

                            MainOperationStep = (int)Station03OperationSteps.EndStep;
                            break;

                        case (int)Station03OperationSteps.EndStep:
                            
                            ResultResponse.Value = ResultRequest.Value;

                            UpdateCommonStatusControl();
                            // Write data to snap7
                            WriteSnap7Data();
                            // Reset the flags and steps
                            MainOperationCycleStart.Value = false;
                            MainOperationStep = (int)Station03OperationSteps.StartStep;
                            break;

                        default:
                            break;
                    }
                }

                Thread.Sleep(50);
            }
        }

        /// <summary>
        /// Method handles the errors and sets the appropriate flags.
        /// </summary>
        private void HandleErrors()
        {
            // Handle error flag
            if (Camera01Error.Value || LedControllerError.Value)
                StationError.Value = true;
            else
                StationError.Value = false;

            if (StationError.Value != PreviousStationError.Value)
            {
                if (StationError.Value)
                    Status.Value |= Station03CommonStatusFlags.Error;
                else
                    Status.Value &= ~Station03CommonStatusFlags.Error;

                // Update common status control. Set common status on a remote machine
                lock (MainOperationObjLock)
                {
                    UpdateCommonStatusControl();
                }

                // Write data to snap7
                WriteSnap7Data();

                PreviousStationError.Value = StationError.Value;
            }

            // Handle ready flag
            if (StationError.Value == true)
                StationReady.Value = false;
            else
                StationReady.Value = true;

            if (StationReady.Value != PreviousStationReady.Value)
            {
                if (StationReady.Value)
                    Status.Value |= Station03CommonStatusFlags.Ready;
                else
                    Status.Value &= ~Station03CommonStatusFlags.Ready;

                // Update common status control. Set common status on a remote machine
                lock (MainOperationObjLock)
                {
                    UpdateCommonStatusControl();
                    
                }

                // Write data to snap7
                WriteSnap7Data();

                PreviousStationReady.Value = StationReady.Value;
            }

            // If we have led controller error try to do reinitialization of the controller
            if (LedControllerError.Value)
            {
                // Lock
                lock (LedController)
                {
                    try
                    {
                        // Add entry to a log
                        if (LedControllerNumOfReconnectErrorLogs < 5)
                            Logger.AddEntry(LoggingLevel.Info, "Station06: Initializing led controller");

                        // Close and reopen the port
                        LedController.ClosePort();
                        LedController.OpenPort(Rs232baudrate.Baudrate_115200);

                        // Make a ping
                        LedController.Ping();

                        // Add entry to a log
                        Logger.AddEntry(LoggingLevel.Info, "Station06: Led controller initialized");

                        // Reset reconnect error log counter
                        LedControllerNumOfReconnectErrorLogs = 0;
                        LedControllerError.Value = false;
                    }
                    catch (Exception e) when (e is InvalidOperationException || e is TimeoutException || e is ControllerOperationFailedException)
                    {
                        if (LedControllerNumOfReconnectErrorLogs < 5)
                        {
                            // Add entry to a log
                            Logger.AddEntry(LoggingLevel.Error, $"Station06: {e.Message}");
                            LedControllerNumOfReconnectErrorLogs++;
                        }
                    }
                }
            }

            // If we have a camera01 error try to do reinitialization of the camera
            if (Camera01Error.Value)
            {
                // Lock
                lock(Camera01)
                {
                    try
                    {
                        // Add entry to a log
                        if (Camera01NumOfReconnectErrorLogs < 5)
                            Logger.AddEntry(LoggingLevel.Info, "Station06: Initializing camera 01");

                        // Close the camera
                        Camera01.CameraClose();

                        // Open the camera and apply initial settings
                        Camera01.CameraOpen(Camera01ID.Value);

                        // Add entry to a log
                        Logger.AddEntry(LoggingLevel.Info, "Station06: Camera 01 initialized");

                        // Reset the flag and reconnect error log counter
                        Camera01NumOfReconnectErrorLogs = 0;
                        Camera01Error.Value = false;
                    }
                    catch (CameraException e)
                    {
                        if (Camera01NumOfReconnectErrorLogs < 5)
                        {
                            // Add entry to a log
                            Logger.AddEntry(LoggingLevel.Error, $"Station06: {e.Message}");
                            Camera01NumOfReconnectErrorLogs++;
                        }
                    }
                }
            }
        }
        #endregion
    }
}
