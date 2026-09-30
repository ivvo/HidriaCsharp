using System;
using System.Windows.Forms;
using Snap7Manager;
using System.Collections.Generic;

namespace IvoSnap7Test
{
    public partial class Form1 : Form
    {
        private Snap7CommunicationManager _manager;
        // Define connection parameters
        private string _plcIpAddress; // Will be populated from Settings.Xml
        private int _rackNumber = 0;     // Default value if not found in settings
        private int _slotNumber = 1;      // Default value if not found in settings

        // UI Controls and State Fields
        private Label StatusLabel { get; set; }
        private Button StartButton { get; set; }
        private Button StopButton { get; set; }
        private TextBox IpAddressTextBox { get; set; }
        private Label IpAddressLabel { get; set; }


        public Form1()
        {
            InitializeControls(); 
        }

        /// <summary>
        /// Programmatically initializes all necessary UI controls for the application.
        /// </summary>
        private void InitializeControls()
        {
            // Setup the main form size and appearance for visibility
            this.ClientSize = new System.Drawing.Size(350, 200);
            this.Text = "PLC Communication Client";

            // IP Address Label and TextBox
            IpAddressLabel = new Label();
            IpAddressLabel.Text = "IP Address:";
            IpAddressLabel.Location = new System.Drawing.Point(10, 15);
            IpAddressLabel.AutoSize = true;
            this.Controls.Add(IpAddressLabel);

            IpAddressTextBox = new TextBox();
            IpAddressTextBox.Text = _plcIpAddress;
            IpAddressTextBox.Location = new System.Drawing.Point(100, 12);
            IpAddressTextBox.Width = 150;
            this.Controls.Add(IpAddressTextBox);

            // Status Label (Shows connection status)
            StatusLabel = new Label();
            StatusLabel.Location = new System.Drawing.Point(10, 50);
            StatusLabel.AutoSize = true;
            this.Controls.Add(StatusLabel);

            // Start Button
            StartButton = new Button();
            StartButton.Text = "Connect";
            StartButton.Location = new System.Drawing.Point(40, 90);
            StartButton.Click += StartCommunicationButton_Click;
            this.Controls.Add(StartButton);

            // Stop Button
            StopButton = new Button();
            StopButton.Text = "Disconnect";
            StopButton.Location = new System.Drawing.Point(150, 90);
            StopButton.Click += StopCommunicationButton_Click;
            this.Controls.Add(StopButton);

            // Initialize the status label appearance to offline state
            UpdateUIForStatus(PLCConnectionStatus.Offline);
        }


        /// <summary>
        /// Updates the visual representation of connection status on the UI thread.
        /// </summary>
        private void UpdateUIForStatus(PLCConnectionStatus status)
        {
            if (this.InvokeRequired) // Ensure thread safety for UI updates
            {
                this.Invoke(new Action(() => UpdateUIForStatus(status)));
                return;
            }

            string text = $"Status: {status}";
            System.Drawing.Color color = System.Drawing.Color.Gray;

            switch (status)
            {
                case PLCConnectionStatus.Online:
                    color = System.Drawing.Color.Green;
                    break;
                case PLCConnectionStatus.Error:
                    color = System.Drawing.Color.Red;
                    break;
                default: // Offline, Initializing etc.
                    color = System.Drawing.Color.Gray;
                    break;
            }

            // Update the label text and color
            StatusLabel.Text = text;
            StatusLabel.ForeColor = color;

            // Enable/Disable buttons based on status to prevent illegal operations
            StartButton.Enabled = (status == PLCConnectionStatus.Offline || status == PLCConnectionStatus.Error);
            StopButton.Enabled = (status != PLCConnectionStatus.Offline && status != PLCConnectionStatus.Error);
        }


        /// <summary>
        /// Initializes and sets up the communication manager instance, including registering event handlers.
        /// </summary>
        public void SetupCommunicationManager()
        {
            try
            {
                _plcIpAddress = XMLSettings.XMLSettings.XMLSettingsManager.GetSetting<string>("PLCConnection/IPAddress"); // Load IP from settings
                _manager = new Snap7CommunicationManager(_plcIpAddress, _rackNumber, _slotNumber); 

                // 1. Register event handler for connection status changes
                _manager.ConnectionStatusChanged += Manager_ConnectionStatusChanged;

                // 2. Configure Read Sections (Example: Reading DB Number X)
                int dbReadNum = XMLSettings.XMLSettings.XMLSettingsManager.GetSetting<int>("Database/ReadDBNumber");
                DBSection readDb = new DBSection { DBNumber = dbReadNum }; // Read from configured DB number
                _manager.AddReadSection(readDb, ReadDataCallback);

                // 3. Configure Write Sections (Example: Writing to a specific element in DB Number Y)
                int dbWriteNum = XMLSettings.XMLSettings.XMLSettingsManager.GetSetting<int>("Database/WriteDBNumber");
                DBSection writeDb = new DBSection { DBNumber = dbWriteNum }; // Write to configured DB number
                _manager.AddWriteSection(writeDb);

            }
            catch (ArgumentException ex)
            {
                MessageBox.Show("Configuration Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void Manager_ConnectionStatusChanged(object sender, Snap7onnectionStatusChangedEventArgs e)
        {
            // Update UI based on connection status (e.ConnectionStatus contains the current state: Online/Offline/Error)
            if (this.InvokeRequired) // Ensure thread safety for UI updates
            {
                this.Invoke(new Action(() => HandleConnectionStatusChangeUI(e.ConnectionStatus)));
            }
            else
            {
                HandleConnectionStatusChangeUI(e.ConnectionStatus);
            }
        }

        private void HandleConnectionStatusChangeUI(PLCConnectionStatus status)
        {
            Console.WriteLine($"PLC Connection Status Changed to: {status}");
            UpdateUIForStatus(status); // Use the new UI update method

            if (status == PLCConnectionStatus.Online)
            {
                MessageBox.Show("Successfully connected and polling data.");
            }
            else if (status == PLCConnectionStatus.Error)
            {
                // Handle specific error states here
            }
        }


        private void ReadDataCallback(string sectionName, List<object> values)
        {
            if (this.InvokeRequired) // Ensure thread safety for UI updates
            {
                this.Invoke(new Action(() => ProcessReadDataUI(sectionName, values)));
            }
            else
            {
                ProcessReadDataUI(sectionName, values);
            }
        }

        private void ProcessReadDataUI(string sectionName, List<object> values)
        {
            // Display the received data in a UI control (e.g., TextBox or DataGrid)
            Console.WriteLine($"Received updated data for {sectionName}: {string.Join(", ", values)}");
        }

        private void StartCommunicationButton_Click(object sender, EventArgs e)
        {
            // Check manager state using the exposed IsManagerRunning property (if fixed in Snap7Manager)
            if (_manager == null)
            {
                SetupCommunicationManager();
            }

            if (_manager != null && !_manager.IsManagerRunning) 
            {
                _manager.Start();
            }
        }

        private void StopCommunicationButton_Click(object sender, EventArgs e)
        {
            // Check manager state using the exposed IsManagerRunning property (if fixed in Snap7Manager)
            if (_manager != null && _manager.IsManagerRunning) 
            {
                _manager.Stop();
            }
        }
    }
}
