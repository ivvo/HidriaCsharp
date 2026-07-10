using LedController;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HidriaVision.Helpers
{
    public class EncoderTrigger : IDisposable
    {
        #region Constants
        private const string PingCom = "PING_CMD/0000000000*";
        private const string SetAngleCmd = "SETANGLE/{0}*";
        private const string SetEncoderStepsCmd = "ENCPULSE/{0}*";
        private const string SetRs232BaudrateCmd = "STB{0}";
        private const double EncoderSteps = 10000.0;
        private const int STRING_LENGTH_MAX = 20;
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
        public EncoderTrigger(string portName)
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
            Rs232.ReadTimeout = 1000;
            Rs232.WriteTimeout = 1000;
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

            // Check connection status
            if (!IsConnected())
                throw new InvalidOperationException("Device not connected.");

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

            // Check connection status
            if (!IsConnected())
                throw new InvalidOperationException("Device not connected.");

            // Send command
            Rs232.Write(PingCom);

            // Check if the operation succeded
            if (Rs232.ReadLine() == "NOK")
            {
                throw new ControllerOperationFailedException("Failed to do a self test.");
            }

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

        //TODO
        /// <summary>
        /// Set angle to trigger camera
        /// </summary>
        public void SetAngle(double encoderAngleTrigger)
        {
            int EncoderTriggerNumber = Convert.ToUInt16((EncoderSteps / 360.0) * encoderAngleTrigger * 4);

            string encoderTriggerValueString = EncoderTriggerNumber.ToString("D10");

            char[] charArray = new char[10];
            charArray = encoderTriggerValueString.ToCharArray();
            Array.Reverse(charArray);
            encoderTriggerValueString = new string(charArray);


            StringBuilder endString = new StringBuilder(string.Format(SetAngleCmd, encoderTriggerValueString));

            Rs232.Write(endString.ToString());

            // Check if the operation succeded

            string readstring = Rs232.ReadLine();
            if (readstring == "NOK")
            {
                throw new ControllerOperationFailedException("Failed to set angle on encoder controller");
            }


        }

        /// <summary>
        /// Set encoder step in circle
        /// </summary>
        public void SetEncoderSteps(int encoderSteps)
        {

            encoderSteps = Math.Abs(encoderSteps);
            string encoderTriggerValueString = encoderSteps.ToString("D10");

            char[] charArray = new char[10];
            charArray = encoderTriggerValueString.ToCharArray();
            Array.Reverse(charArray);
            encoderTriggerValueString = new string(charArray);


            StringBuilder endString = new StringBuilder(string.Format(SetEncoderStepsCmd, encoderTriggerValueString));

            Rs232.Write(endString.ToString());

            // Check if the operation succeded

            string readstring = Rs232.ReadLine();
            if (readstring == "NOK")
            {
                throw new ControllerOperationFailedException("Failed to set angle on encoder controller");
            }


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
