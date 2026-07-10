using System;
using System.Collections.Generic;
using Sharp7;
using System.Runtime.InteropServices;

namespace Snap7Manager
{
    public class HSnap7
    {
        #region Constants
        private const string err_CONNECTION = "Error Connection";
        private const string err_READING = "Error when Reading";
        private const string err_WRITING = "Error when Writing";
        private const string err_CONNECTING = "Error Connecting to PLC";
        #endregion

        #region Private fields
        private S7Client Client;
        private Dictionary<string, List<dynamic>> SectionsData;
        #endregion

        public HSnap7()
        {
            Client = new S7Client();
            SectionsData = new Dictionary<string, List<dynamic>>();
        }

        #region Public methods
        /// <summary>
        /// Connect to PLC.
        /// </summary>
        /// <param name="address">IP address.</param>
        /// <param name="rack">PLC rack.</param>
        /// <param name="slot">PLC slot.</param>
        /// <exception cref="Snap7Exception"></exception>
        public void PlcConnect(string address, int rack, int slot)
        {
            int res = Client.ConnectTo(address, rack, slot);
            if (res != 0) { throw new Snap7Exception(err_CONNECTING); };
        }

        /// <summary>
        /// Disconnect form PLC
        /// </summary>
        public void PlcDisconnect()
        {
            Client.Disconnect();
        }

        /// <summary>
        /// Get Sections.
        /// </summary>
        /// <returns>Returns sections dictionary.</returns>
        /// <exception cref="Snap7Exception"></exception>
        public Dictionary<string, List<dynamic>> GetSections(DBSection dbSections)
        {
            try
            {
                SectionsData.Clear();

                byte[] tmpData = ReadPLC(dbSections);
                foreach (DBData data in dbSections.Data)
                {
                    string nameAndType = data.Name + ":" + data.DataType.Name.ToString();
                    List<dynamic> values = new List<dynamic>();

                    switch (Type.GetTypeCode(data.DataType))
                    {
                        case TypeCode.Byte:
                            foreach (var item in ReadValues<Byte>(tmpData, data.StartAddress, data.Length))
                            {
                                values.Add(item);
                            }
                            break;
                        case TypeCode.Int16:
                            foreach (var item in ReadValues<Int16>(tmpData, data.StartAddress, data.Length))
                            {
                                values.Add(item);
                            }
                            break;
                        case TypeCode.Int32:
                            foreach (var item in ReadValues<Int32>(tmpData, data.StartAddress, data.Length))
                            {
                                values.Add(item);
                            }
                            break;
                        case TypeCode.Single:
                            foreach (var item in ReadValues<Single>(tmpData, data.StartAddress, data.Length))
                            {
                                values.Add(item);
                            }
                            break;
                        default:
                            break;
                    }
                    SectionsData.Add(nameAndType, values);
                }

                return SectionsData;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Set Sections.
        /// </summary>
        /// <exception cref="Snap7Exception"></exception>
        public void SetSections(Dictionary<string, List<dynamic>> valuesAndNames, DBSection dbSections)
        {
            try
            {
                int DBNumber = dbSections.DBNumber;

                int byteLength = 0;

                int tmpCurrentLargestAddress = 0;
                TypeCode tmpCurrentLargestAddressDataType = TypeCode.Byte;
                int tmpCurrentLargestAddressLength = 0;

                foreach (DBData data in dbSections.Data)
                {
                    if (data.StartAddress > tmpCurrentLargestAddress)
                    {
                        tmpCurrentLargestAddressDataType = Type.GetTypeCode(data.DataType);
                        tmpCurrentLargestAddress = data.StartAddress;
                        tmpCurrentLargestAddressLength = data.Length;
                    }
                }

                switch (tmpCurrentLargestAddressDataType)
                {
                    case TypeCode.Byte:
                        byteLength = tmpCurrentLargestAddress + (1 * tmpCurrentLargestAddressLength);
                        break;
                    case TypeCode.Int16:
                        byteLength = tmpCurrentLargestAddress + (2 * tmpCurrentLargestAddressLength);
                        break;
                    case TypeCode.Int32:
                        byteLength = tmpCurrentLargestAddress + (4 * tmpCurrentLargestAddressLength);
                        break;
                    case TypeCode.Single:
                        byteLength = tmpCurrentLargestAddress + (4 * tmpCurrentLargestAddressLength);
                        break;
                }

                if (byteLength == 0) byteLength = 1;
                byte[] outBuffer = new byte[byteLength];

                foreach (DBData data in dbSections.Data)
                {
                    string nameAndType = data.Name + ":" + data.DataType.Name.ToString();

                    switch (Type.GetTypeCode(data.DataType))
                    {
                        case TypeCode.Byte:
                            WriteValues<Byte>(data.StartAddress, data.Length, outBuffer, valuesAndNames[nameAndType]);
                            break;
                        case TypeCode.Int16:
                            WriteValues<Int16>(data.StartAddress, data.Length, outBuffer, valuesAndNames[nameAndType]);
                            break;
                        case TypeCode.Int32:
                            WriteValues<Int32>(data.StartAddress, data.Length, outBuffer, valuesAndNames[nameAndType]);
                            break;
                        case TypeCode.Single:
                            WriteValues<Single>(data.StartAddress, data.Length, outBuffer, valuesAndNames[nameAndType]);
                            break;
                    }
                }
                WritePLC(outBuffer, DBNumber, byteLength);

            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// Create default dictionary
        /// </summary>
        /// <returns>Returns default dictionary.</returns>
        public Dictionary<string, List<dynamic>> CreateDefaultDictionary(DBSection dbSections)
        {
            Dictionary<string, List<dynamic>> tempSectionsData = new Dictionary<string, List<dynamic>>();
            tempSectionsData.Clear();

            foreach (DBData data in dbSections.Data)
            {
                string nameAndType = data.Name + ":" + data.DataType.Name.ToString();
                List<dynamic> values = new List<dynamic>();
                switch (Type.GetTypeCode(data.DataType))
                {
                    case TypeCode.Byte:
                        foreach (var item in CreateDefaults<Byte>(data.StartAddress, data.Length)) { values.Add(item); }
                        break;
                    case TypeCode.Int16:
                        foreach (var item in CreateDefaults<Byte>(data.StartAddress, data.Length)) { values.Add(item); }
                        break;
                    case TypeCode.Int32:
                        foreach (var item in CreateDefaults<Byte>(data.StartAddress, data.Length)) { values.Add(item); }
                        break;
                    case TypeCode.Single:
                        foreach (var item in CreateDefaults<Byte>(data.StartAddress, data.Length)) { values.Add(item); }
                        break;
                    default:
                        break;
                }
                tempSectionsData.Add(nameAndType, values);
            }

            return tempSectionsData;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Read Values From ByteArray As EveryThing - Generic
        /// </summary>
        /// <returns>Returns read values.</returns>
        /// <param name="tmpData">Temp data.</param>
        /// <param name="startaddress">Start address.</param>
        /// <param name="length">Length.</param>
        private IEnumerable<T> ReadValues<T>(byte[] tmpData, int startaddress, int length)
        {
            int typeLenght = Marshal.SizeOf(typeof(T));
            int byteLength = length * typeLenght;

            for (int i = 0; i < byteLength; i = i + typeLenght)
            {
                byte[] tmpBuffer = new byte[typeLenght];
                for (int k = 0; k < typeLenght; k++)
                {
                    tmpBuffer[k] = tmpData[i + startaddress + k];
                }

                if (BitConverter.IsLittleEndian) Array.Reverse(tmpBuffer);
                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Byte:
                        yield return (T)(object)tmpBuffer[0];
                        break;
                    case TypeCode.Int16:
                        yield return (T)(object)BitConverter.ToInt16(tmpBuffer, 0);
                        break;
                    case TypeCode.Int32:
                        yield return (T)(object)BitConverter.ToInt32(tmpBuffer, 0);
                        break;
                    case TypeCode.Single:
                        yield return (T)(object)BitConverter.ToSingle(tmpBuffer, 0);
                        break;
                }
            }

        }

        /// <summary>
        /// Read Datablock To ByteArray.
        /// </summary>
        /// <returns>Returns read data.</returns>
        /// <param name="dbsection">DB section.</param>
        private byte[] ReadPLC(DBSection dbsection)
        {
            int byteLength = 0;

            int tmpCurrentLargestAddress = 0;
            TypeCode tmpCurrentLargestAddressDataType = TypeCode.Byte;
            int tmpCurrentLargestAddressLength = 0;

            foreach (DBData data in dbsection.Data)
            {
                if (data.StartAddress > tmpCurrentLargestAddress)
                {
                    tmpCurrentLargestAddressDataType = Type.GetTypeCode(data.DataType);
                    tmpCurrentLargestAddress = data.StartAddress;
                    tmpCurrentLargestAddressLength = data.Length;
                }
            }

            switch (tmpCurrentLargestAddressDataType)
            {
                case TypeCode.Byte:
                    byteLength = tmpCurrentLargestAddress + (1 * tmpCurrentLargestAddressLength);
                    break;
                case TypeCode.Int16:
                    byteLength = tmpCurrentLargestAddress + (2 * tmpCurrentLargestAddressLength);
                    break;
                case TypeCode.Int32:
                    byteLength = tmpCurrentLargestAddress + (4 * tmpCurrentLargestAddressLength);
                    break;
                case TypeCode.Single:
                    byteLength = tmpCurrentLargestAddress + (4 * tmpCurrentLargestAddressLength);
                    break;
            }

            byte[] outBuffer = new byte[byteLength];

            int clientResult = Client.DBRead(dbsection.DBNumber, 0, byteLength, outBuffer);
            if (clientResult == 0)
            {
                return outBuffer;
            }
            else if ((clientResult == S7Consts.errCliAddressOutOfRange) || (clientResult == S7Consts.errCliItemNotAvailable)) { throw new Snap7Exception(err_READING); }
            else { throw new Snap7Exception(err_CONNECTION); }
        }

        /// <summary>
        /// Write Datablock As ByteArray
        /// </summary>
        /// <typeparam name="T">Type parameter.</typeparam>
        /// <param name="outBuffer">Output buffer.</param>
        /// <param name="DBNumber">DB number.</param>
        /// <param name="byteLength">Byte length.</param>
        private void WritePLC(byte[] outBuffer, int DBNumber, int byteLength)
        {
            if (Client.DBWrite(DBNumber, 0, byteLength, outBuffer) != 0)
            {
                throw new Snap7Exception(err_WRITING);
            }
        }

        /// <summary>
        /// Write Datablock As EveryThing - Generic to ByteArray
        /// </summary>
        /// <typeparam name="T">Type parameter.</typeparam>    
        /// <param name="startaddress">Start address.</param>
        /// <param name="length">Length.</param>
        /// <param name="outBuffer">Output buffer.</param>
        /// <param name="valuesAndNames">Values and names.</param>
        private void WriteValues<T>(int startaddress, int length, byte[] outBuffer, List<dynamic> valuesAndNames)
        {
            int typeLenght = Marshal.SizeOf(typeof(T));
            int byteLength = length * typeLenght;

            for (int i = 0; i < byteLength; i = i + typeLenght)
            {
                byte[] tmpBuffer = new byte[typeLenght];

                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Byte:
                        byte[] oneByte = BitConverter.GetBytes(Convert.ToByte(valuesAndNames[i]));
                        tmpBuffer[0] = oneByte[0];
                        break;
                    case TypeCode.Int16:
                        tmpBuffer = BitConverter.GetBytes(Convert.ToInt16(valuesAndNames[i]));
                        break;
                    case TypeCode.Int32:
                        tmpBuffer = BitConverter.GetBytes(Convert.ToInt32(valuesAndNames[i]));
                        break;
                    case TypeCode.Single:
                        tmpBuffer = BitConverter.GetBytes(Convert.ToSingle(valuesAndNames[i]));
                        break;
                }
                if (BitConverter.IsLittleEndian) Array.Reverse(tmpBuffer);

                for (int k = 0; k < typeLenght; k++)
                {
                    outBuffer[i + startaddress + k] = tmpBuffer[k];
                }
            }
        }

        /// <summary>
        /// Create defaults As EveryThing - Generic
        /// </summary>
        /// <returns>Returns default data.</returns>
        /// <param name="startaddress">Start address.</param>
        /// <param name="length">Length.</param>
        private IEnumerable<T> CreateDefaults<T>(int startaddress, int length)
        {
            int typeLenght = Marshal.SizeOf(typeof(T));
            int byteLength = length * typeLenght;

            for (int i = 0; i < byteLength; i = i + typeLenght)
            {
                byte[] tmpBuffer = new byte[typeLenght];
                for (int k = 0; k < typeLenght; k++)
                {
                    tmpBuffer[k] = 0;   // Set 0
                }

                switch (Type.GetTypeCode(typeof(T)))
                {
                    case TypeCode.Byte:
                        yield return (T)(object)tmpBuffer[0];
                        break;
                    case TypeCode.Int16:
                        yield return (T)(object)BitConverter.ToInt16(tmpBuffer, 0);
                        break;
                    case TypeCode.Int32:
                        yield return (T)(object)BitConverter.ToInt32(tmpBuffer, 0);
                        break;
                    case TypeCode.Single:
                        yield return (T)(object)BitConverter.ToSingle(tmpBuffer, 0);
                        break;
                }
            }
        }
        #endregion
    }
}
