using System;
using System.IO;
using System.IO.Ports;
using System.Management;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace LedController
{
    public class RGBWController : IDisposable
    {
        #region Constants
        private const string SetLedChannelCmd = "STC{0}{1}";
        private const string SetLedChannelsCmd = "STCS{0};{1};{2};{3}";
        private const string SetLedTriggerModeCmd = "STLTM{0}";
        private const string SetLedTriggerInternalModeCmd = "STLTIM{0}";
        private const string SetSurfaceFxCmd = "STFX{0}";
        private const string SetSurfaceFxModeCmd = "STFXM{0}";
        private const string ResetSurfaceFxCmd = "RSTFX";
        private const string StartSurfaceFxCmd = "SSFX";
        private const string SetSurfaceFxTriggerDelayCmd = "STFXTDLY{0}";
        private const string SetRs232BaudrateCmd = "STB{0}";
        private const string SaveParametersCmd = "SV";
        private const string SelfTestCom = "ST";
        private const string PingCom = "PING";
        #endregion

        #region Private fields
        private SerialPort Rs232;
        private bool DisposedValue = false;
        #endregion

        /// <summary>
        /// Constructs an object of type RGBWController
        /// </summary>
        /// <param name="portName"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public RGBWController(string portName)
        {
            // Check if port name string is null or empty
            if (string.IsNullOrEmpty(portName))
                throw new ArgumentException("Port name string is null or empty.");

            Rs232 = new SerialPort();

            // Set communication parameters
            Rs232.Parity = Parity.None;
            Rs232.DataBits = 8;
            Rs232.StopBits = StopBits.One;
            Rs232.Handshake = Handshake.None;
            Rs232.ReadTimeout = 100;
            Rs232.WriteTimeout = 100;
            Rs232.NewLine = "\r\n";
            Rs232.PortName = portName;
        }

        #region Private and protected methods
        /// <summary>
        /// Disposes the object.
        /// </summary>
        /// <param name="disposing">Disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!DisposedValue)
            {
                if (disposing)
                {
                    // Dispose serial port resources
                    Rs232.Dispose();
                }

                DisposedValue = true;
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Returns device connection status.
        /// </summary>
        /// <returns>True if connected.</returns>
        /// <exception cref="ObjectDisposedException"
        public bool IsConnected()
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Returns device connection status
            return PortConnected();
        }

        /// <summary>
        /// Opens the connection with a given COM port.
        /// </summary>
        /// <param name="baudrate">Connection speed in baudrate.</param>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public void OpenPort(Rs232baudrate baudrate)
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Check connection status
            if (!IsConnected())
                throw new InvalidOperationException("Device not connected.");

            // Set baudrate
            switch (baudrate)
            {
                case Rs232baudrate.Baudrate_9600:
                    Rs232.BaudRate = 9600;
                    break;

                case Rs232baudrate.Baudrate_14400:
                    Rs232.BaudRate = 14400;
                    break;

                case Rs232baudrate.Baudrate_19200:
                    Rs232.BaudRate = 19200;
                    break;

                case Rs232baudrate.baudrate_38400:
                    Rs232.BaudRate = 38400;
                    break;

                case Rs232baudrate.Baudrate_57600:
                    Rs232.BaudRate = 57600;
                    break;

                case Rs232baudrate.Baudrate_115200:
                    Rs232.BaudRate = 115200;
                    break;

                default:
                    throw new ArgumentException("Invalid baudrate.");
            }

            // Open connection
            Rs232.Open();
        }

        /// <summary>
        /// Sets the intensity of a specified led channel.
        /// </summary>
        /// <param name="channel">Led channel.</param>
        /// <param name="ledIntensity">Led channel intensity.</param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ControllerOperationFailedException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SetLedChannel(LedChannel channel, byte ledIntensity)
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            //// Check connection status
            //if (!IsConnected())
            //    throw new InvalidOperationException("Device not connected.");

            // Check if channel value is within range
            if (!(channel >= LedChannel.Channel_1 && channel <= LedChannel.Channel_4))
                throw new ArgumentException("Invalid led channel.");

            // Send command
            Rs232.WriteLine(string.Format(SetLedChannelCmd, (int)channel, ledIntensity));

            // Check if the operation succeded
            if (Rs232.ReadLine() == "NOK")
                throw new ControllerOperationFailedException("Failed to set the led channel intensity.");
        }

        /// <summary>
        /// Sets the intensity of all channels.
        /// </summary>
        /// <param name="ledIntensityCh1">Led channel1 intensity.</param>
        /// <param name="ledIntensityCh2">Led channel2 intensity.</param>
        /// <param name="ledIntensityCh3">Led channel3 intensity.</param>
        /// <param name="ledIntensityCh4">Led channel4 intensity.</param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ControllerOperationFailedException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SetLedChannels(byte ledIntensityCh1, byte ledIntensityCh2, byte ledIntensityCh3, byte ledIntensityCh4)
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            //// Check connection status
            //if (!IsConnected())
            //    throw new InvalidOperationException("Device not connected.");

            // Send command
            Rs232.WriteLine(string.Format(SetLedChannelsCmd, ledIntensityCh1, ledIntensityCh2, ledIntensityCh3, ledIntensityCh4));

            // Check if the operation succeded
            if (Rs232.ReadLine() == "NOK")
                throw new ControllerOperationFailedException("Failed to set the led channels intensities.");
        }

        /// <summary>
        /// Sets led trigger mode.
        /// </summary>
        /// <param name="ledTriggerMode">Led trigger mode.</param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ControllerOperationFailedException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SetLedTriggerMode(LedTriggerMode ledTriggerMode)
        {
            Stopwatch test = new Stopwatch();
            test.Restart();

            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            //// Check connection status
            //if (!IsConnected())
            //    throw new InvalidOperationException("Device not connected.");

            // Check if led trigger mode is within range
            if (!(ledTriggerMode >= LedTriggerMode.Internal && ledTriggerMode <= LedTriggerMode.External))
                throw new ArgumentException("Invalid led trigger mode.");
            long tst = test.ElapsedMilliseconds;
            // Send command

            Rs232.WriteLine(string.Format(SetLedTriggerModeCmd, (int)ledTriggerMode));
           
           
            // Check if the operation succeded
            string received = Rs232.ReadLine();
            if (received == "NOK")
                throw new ControllerOperationFailedException("Failed to set the led trigger mode.");
            


        }

        /// <summary>
        /// Sets led trigger internal mode.
        /// </summary>
        /// <param name="ledTriggerInternalMode">Led trigger internal mode.</param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ControllerOperationFailedException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SetLedTriggerInternalMode(LedTriggerInternalMode ledTriggerInternalMode)
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            //// Check connection status
            //if (!IsConnected())
            //    throw new InvalidOperationException("Device not connected.");

            // Check if led trigger internal mode is within range
            if (!(ledTriggerInternalMode >= LedTriggerInternalMode.LedOff && ledTriggerInternalMode <= LedTriggerInternalMode.LedOn))
                throw new ArgumentException("Invalid led trigger internal mode.");

            // Send command
            Rs232.WriteLine(string.Format(SetLedTriggerInternalModeCmd, (int)ledTriggerInternalMode));

            // Check if the operation succeded
            if (Rs232.ReadLine() == "NOK")
                throw new ControllerOperationFailedException("Failed to set the led trigger internal mode.");
        }

        /// <summary>
        /// Sets surface fx state.
        /// </summary>
        /// <param name="surfaceFxState">Surface fx state.</param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ControllerOperationFailedException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SetSurfaceFx(SurfaceFxState surfaceFxState)
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            //// Check connection status
            //if (!IsConnected())
            //    throw new InvalidOperationException("Device not connected.");

            // Check if surface fx state is within range
            if (!(surfaceFxState >= SurfaceFxState.Disabled && surfaceFxState <= SurfaceFxState.Enabled))
                throw new ArgumentException("Invalid surface fx state.");

            // Send command
            Rs232.WriteLine(string.Format(SetSurfaceFxCmd, (int)surfaceFxState));

            // Check if the operation succeded
            if (Rs232.ReadLine() == "NOK")
                throw new ControllerOperationFailedException("Failed to set the surface fx state.");
        }

        /// <summary>
        /// Sets surface fx mode.
        /// </summary>
        /// <param name="surfaceFxMode">Surface fx mode.</param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ControllerOperationFailedException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SetSurfaceFxMode(SurfaceFxMode surfaceFxMode)
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            //// Check connection status
            //if (!IsConnected())
            //    throw new InvalidOperationException("Device not connected.");

            // Check if surface fx mode is within range
            if (!(surfaceFxMode >= SurfaceFxMode.Normal && surfaceFxMode <= SurfaceFxMode.Trigger))
                throw new ArgumentException("Invalid surface fx state mode.");

            // Send command
            Rs232.WriteLine(string.Format(SetSurfaceFxModeCmd, (int)surfaceFxMode));

            // Check if the operation succeded
            if (Rs232.ReadLine() == "NOK")
                throw new ControllerOperationFailedException("Failed to set the surface fx mode.");
        }

        /// <summary>
        /// Resets surface fx.
        /// </summary>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ControllerOperationFailedException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void ResetSurfaceFx()
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            //// Check connection status
            //if (!IsConnected())
            //    throw new InvalidOperationException("Device not connected.");

            // Send command
            Rs232.WriteLine(ResetSurfaceFxCmd);

            // Check if the operation succeded
            if (Rs232.ReadLine() == "NOK")
                throw new ControllerOperationFailedException("Failed to reset surface fx.");
        }

        /// <summary>
        /// Starts surface fx.
        /// </summary>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ControllerOperationFailedException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void StartSurfaceFx()
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            //// Check connection status
            //if (!IsConnected())
            //    throw new InvalidOperationException("Device not connected.");

            // Send command
            Rs232.WriteLine(StartSurfaceFxCmd);

            // Check if the operation succeded
            if (Rs232.ReadLine() == "NOK")
                throw new ControllerOperationFailedException("Failed to start surface fx.");
        }

        /// <summary>
        /// Sets surface fx trigger delay.
        /// </summary>
        /// <param name="triggerDelay">Surface fx trigger delay.</param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ControllerOperationFailedException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SetSurfaceFxTriggerDelay(short triggerDelay)
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            //// Check connection status
            //if (!IsConnected())
            //    throw new InvalidOperationException("Device not connected.");

            // Check if trigger delay is within range
            if (!(triggerDelay >= 0 && triggerDelay <= 1000))
                throw new ArgumentException("Invalid trigger delay value.");

            // Send command
            Rs232.WriteLine(string.Format(SetSurfaceFxTriggerDelayCmd, triggerDelay));

            // Check if the operation succeded
            if (Rs232.ReadLine() == "NOK")
                throw new ControllerOperationFailedException("Failed to set surfacefx trigger delay.");
        }

        /// <summary>
        /// Sets the rs232 baudrate.
        /// </summary>
        /// <param name="baudrate">baudrate.</param>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ControllerOperationFailedException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SetRs232Baudrate(Rs232baudrate baudrate)
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            //// Check connection status
            //if (!IsConnected())
            //    throw new InvalidOperationException("Device not connected.");

            // Check if baudrate is within range
            if (!(baudrate >= Rs232baudrate.Baudrate_9600 && baudrate <= Rs232baudrate.Baudrate_115200))
                throw new ArgumentException("Invalid baudrate value.");

            // Send command
            Rs232.WriteLine(string.Format(SetRs232BaudrateCmd, (int)baudrate));

            // Check if the operation succeded
            if (Rs232.ReadLine() == "NOK")
                throw new ControllerOperationFailedException("Failed to set baudrate.");
        }

        /// <summary>
        /// Saves parameters to EEPROM.
        /// </summary>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ControllerOperationFailedException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SaveParameters()
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            //// Check connection status
            //if (!IsConnected())
            //    throw new InvalidOperationException("Device not connected.");

            // Send command
            Rs232.WriteLine(SaveParametersCmd);

            // Check if the operation succeded
            if (Rs232.ReadLine() == "NOK")
                throw new ControllerOperationFailedException("Failed to save parameters.");
        }

        /// <summary>
        /// Makes a self test.
        /// </summary>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ControllerOperationFailedException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void SelfTest()
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            //// Check connection status
            //if (!IsConnected())
            //    throw new InvalidOperationException("Device not connected.");

            // Send command
            Rs232.WriteLine(SelfTestCom);

            // Check if the operation succeded
            if (Rs232.ReadLine() == "NOK")
                throw new ControllerOperationFailedException("Failed to do a self test.");
        }

        /// <summary>
        /// Makes a ping.
        /// </summary>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="ControllerOperationFailedException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void Ping()
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            //// Check connection status
            //if (!IsConnected())
            //    throw new InvalidOperationException("Device not connected.");

            // Send command
            Rs232.WriteLine(PingCom);

            // Check if the operation succeded
            if (Rs232.ReadLine() == "NOK")
                throw new ControllerOperationFailedException("Failed to do a self test.");
        }

        /// <summary>
        /// Clears buffers.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void ClearBuffers()
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Clear input and output buffers
            Rs232.DiscardInBuffer();
            Rs232.DiscardOutBuffer();
        }

        /// <summary>
        /// Closes the connection with a given COM port
        /// </summary>
        /// <exception cref="IOException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        public void ClosePort()
        {
            // Check if object has been already disposed
            if (DisposedValue)
                throw new ObjectDisposedException(GetType().FullName);

            // Close the port
            Rs232.Close();
        }

        /// <summary>
        /// Disposes the object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Checks if a given port is connected.
        /// </summary>
        /// <returns>Returns true if port is connected.</returns>
        private bool PortConnected()
        {
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"Select * From Win32_PnPEntity"))
            using (ManagementObjectCollection devices = searcher.Get())
            {
                Regex ComNameRegex = new Regex("COM[0-9]+");

                // Go through devices
                foreach (ManagementBaseObject device in devices)
                {
                    string DeviceCaption = (string)device.GetPropertyValue("Caption");

                    // Check if port exists
                    if (DeviceCaption != null && ComNameRegex.IsMatch(DeviceCaption) && ComNameRegex.Match(DeviceCaption).Value == Rs232.PortName)
                        return true;
                }
            }

            return false;
        }
        #endregion
    }
}
