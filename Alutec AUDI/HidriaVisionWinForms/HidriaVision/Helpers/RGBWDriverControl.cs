using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LedController;
using Logger;

namespace HidriaVision.Helpers
{
    public partial class RGBWDriverControl : UserControl
    {
        #region Public Fields
        /// <summary>
        /// Enum used for setting visual options (none default)
        /// </summary>
        [Flags]
        public enum RGBWOptions
        {
            /// <summary> This option disables all channels </summary>
            UseNone = 0x00,
            /// <summary> This option enables channel 1 </summary>
            UseCh1 = 0x01,
            /// <summary> This option enables channel 2 </summary>
            UseCh2 = 0x02,
            /// <summary> This option enables channel 3 </summary>
            UseCh3 = 0x04,
            /// <summary> This option enables channel 4 </summary>
            UseCh4 = 0x08,
            /// <summary> This option enables CH markings instead of RGBW </summary>
            UseChMarking = 0x10           
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Set Visual options property
        /// </summary>
        public RGBWOptions SetRGBWOptions
        {
            private get
            {
                return _setRGBWOptions;
            }
            set
            {
                _setRGBWOptions = value;
                determineControlHeight();
            }
        }

        /// <summary>
        /// Get or set value for channel 1
        /// </summary>
        public byte Channel1Value { get; set; } = 0;

        /// <summary>
        /// Get or set value for channel 2
        /// </summary>
        public byte Channel2Value { get; set; } = 0;

        /// <summary>
        /// Get or set value for channel 3
        /// </summary>
        public byte Channel3Value { get; set; } = 0;

        /// <summary>
        /// Get or set value for channel 4
        /// </summary>
        public byte Channel4Value { get; set; } = 0;

        #endregion

        #region Private Fields
        private RGBWController LedController;
        private SharedVar<bool> LedControllerError;
        private FileEventLogger Logger;
        private RGBWOptions _setRGBWOptions = RGBWOptions.UseNone;

        #endregion

        public RGBWDriverControl(RGBWController controller, SharedVar<bool> ledControllerError, FileEventLogger logger)
        {          
            InitializeComponent();

            LedController = controller;
            LedControllerError = ledControllerError;
            Logger = logger;          
        }

        #region Private Events
        /// <summary>
        /// Event when tcontrol is loaded to set visual style
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RGBWDriverControl_Load(object sender, EventArgs e)
        {
            setVisualStyle();
        }

        /// <summary>
        /// Event when trackbarChannel1 value has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trackBarChannel1_ValueChanged(object sender, EventArgs e)
        {
            setChannel1();
        }

        /// <summary>
        /// Event when trackbarChannel2 value has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trackBarChannel2_ValueChanged(object sender, EventArgs e)
        {
            setChannel2();
        }

        /// <summary>
        /// Event when trackbarChannel3 value has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trackBarChannel3_ValueChanged(object sender, EventArgs e)
        {
            setChannel3();
        }

        /// <summary>
        /// Event when trackbarChannel4 value has changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trackBarChannel4_ValueChanged(object sender, EventArgs e)
        {
            setChannel4();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Method which detrmines the height of the control
        /// </summary>       
        private void determineControlHeight()
        {
            // Internal value for setting height of controls
            int controlHeight = 0;

            // CH1 is used
            if (SetRGBWOptions.HasFlag(RGBWOptions.UseCh1))
            {
                controlHeight += 50;
            }

            // CH2 is used
            if (SetRGBWOptions.HasFlag(RGBWOptions.UseCh2))
            {
                controlHeight += 50;
            }

            // CH3 is used
            if (SetRGBWOptions.HasFlag(RGBWOptions.UseCh3))
            {
                controlHeight += 50;
            }

            // CH4 is used
            if (SetRGBWOptions.HasFlag(RGBWOptions.UseCh4))
            {
                controlHeight += 50;
            }

            // Set control final height
            this.Height = controlHeight - 5;
        }

        /// <summary>
        /// Method sets visual style and initial values
        /// </summary>       
        private void setVisualStyle()
        {
            // Internal value for setting positions of controls
            int controlPosition = 0;

            // Set panel ch1
            if (SetRGBWOptions.HasFlag(RGBWOptions.UseCh1))
            {
                if (SetRGBWOptions.HasFlag(RGBWOptions.UseChMarking))
                {
                    labelCh1.Text = "CH1";
                }
                else
                {
                    labelCh1.Text = "RED";
                }
                panelChannel1.Visible = true;
                panelChannel1.Location = new Point(0, controlPosition);
                controlPosition += 50;
            }

            // Set panel ch2
            if (SetRGBWOptions.HasFlag(RGBWOptions.UseCh2))
            {
                if (SetRGBWOptions.HasFlag(RGBWOptions.UseChMarking))
                {
                    labelCh2.Text = "CH2";
                }
                else
                {
                    labelCh2.Text = "GREEN";
                }
                panelChannel2.Visible = true;
                panelChannel2.Location = new Point(0, controlPosition);
                controlPosition += 50;
            }

            // Set panel ch3
            if (SetRGBWOptions.HasFlag(RGBWOptions.UseCh3))
            {
                if (SetRGBWOptions.HasFlag(RGBWOptions.UseChMarking))
                {
                    labelCh3.Text = "CH3";
                }
                else
                {
                    labelCh3.Text = "BLUE";
                }
                panelChannel3.Visible = true;
                panelChannel3.Location = new Point(0, controlPosition);
                controlPosition += 50;
            }

            // Set panel ch4
            if (SetRGBWOptions.HasFlag(RGBWOptions.UseCh4))
            {
                if (SetRGBWOptions.HasFlag(RGBWOptions.UseChMarking))
                {
                    labelCh4.Text = "CH4";
                }
                else
                {
                    labelCh4.Text = "WHITE";
                }
                panelChannel4.Visible = true;
                panelChannel4.Location = new Point(0, controlPosition);
                controlPosition += 50;
            }         

            // Set initial values on trackbars
            trackBarChannel1.Value = (int)Channel1Value;
            trackBarChannel2.Value = (int)Channel2Value;
            trackBarChannel3.Value = (int)Channel3Value;
            trackBarChannel4.Value = (int)Channel4Value;

            // Set initial values and send them to controller
            setChannel1();
            setChannel2();
            setChannel3();
            setChannel4();

            // Subscribe to trackbar events
            this.trackBarChannel1.ValueChanged += new EventHandler(this.trackBarChannel1_ValueChanged);
            this.trackBarChannel2.ValueChanged += new EventHandler(this.trackBarChannel2_ValueChanged);
            this.trackBarChannel3.ValueChanged += new EventHandler(this.trackBarChannel3_ValueChanged);
            this.trackBarChannel4.ValueChanged += new EventHandler(this.trackBarChannel4_ValueChanged);
        }

        /// <summary>
        /// Method for setting ch1 values and visual
        /// </summary>        
        private void setChannel1()
        {
            Channel1Value = (byte)trackBarChannel1.Value;
            this.textBoxValueChannel1.Text = Channel1Value.ToString();
        }

        /// <summary>
        /// Method for setting ch2 values and visual
        /// </summary>      
        private void setChannel2()
        {
            Channel2Value = (byte)trackBarChannel2.Value;
            this.textBoxValueChannel2.Text = Channel2Value.ToString();           
        }

        /// <summary>
        /// Method for setting ch3 values and visual
        /// </summary>      
        private void setChannel3()
        {
            Channel3Value = (byte)trackBarChannel3.Value;
            this.textBoxValueChannel3.Text = Channel3Value.ToString();
        }

        /// <summary>
        /// Method for setting ch4 values and visual
        /// </summary>       
        private void setChannel4()
        {
            Channel4Value = (byte)trackBarChannel4.Value;
            this.textBoxValueChannel4.Text = Channel4Value.ToString();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Method to send channel value to controller
        /// </summary>
        /// <param name="channel">Channel number 1-4</param>
        /// <param name="value">Value 0-255</param>
        public void changeControllerValue(LedChannel channel, byte value)
        {
            // If there is no error, send new value
            if (!LedControllerError.Value)
            {
                // Lock
                lock (LedController)
                {
                    try
                    {
                        // Set desired led channel with value
                        LedController.SetLedChannel(channel, value);
                    }
                    catch (Exception e) when (e is InvalidOperationException || e is TimeoutException || e is ControllerOperationFailedException)
                    {
                        // Add entry to a log
                        Logger.AddEntry(LoggingLevel.Error, $"LedController Error: {e.Message}");
                        LedControllerError.Value = true;

                        // Show message
                        MessageBox.Show($"LedController Error: {e.Message}");
                    }
                }
           }         
        }
        #endregion     
    }
}
