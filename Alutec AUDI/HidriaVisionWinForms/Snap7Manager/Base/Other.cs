using System;
using System.Collections.Generic;

namespace Snap7Manager
{
    /// <summary>
    /// Inner structure of DB data
    /// </summary>
    public struct DBData
    {
        public string Name;
        public Type DataType;
        public int StartAddress;
        public int Length;
    }

    /// <summary>
    /// DBSection
    /// </summary>
    public class DBSection
    {
        public int DBNumber;
        public List<DBData> Data;
    }

    /// <summary>
    /// Represents custom event arguments for changed connection status.
    /// </summary>
    public class Snap7onnectionStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets connection status.
        /// </summary>
        public PLCConnectionStatus ConnectionStatus;

        /// <summary>
        /// Creates new instance of Snap7onnectionStatusChangedEventArgs.
        /// </summary>
        /// <param name="connectionStatus">PLC connection status.</param>
        public Snap7onnectionStatusChangedEventArgs(PLCConnectionStatus connectionStatus)
        {
            ConnectionStatus = connectionStatus;
        }
    }
}
