using Sharp7;
using System;
using System.Collections.Generic;
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
        /// <param name="connectionType">Connection type (PG/OP/Basic resource pool on the PLC).</param>
        /// <exception cref="Snap7Exception"></exception>
        public void PlcConnect(string address, int rack, int slot, PLCConnectionType connectionType = PLCConnectionType.PG)
        {
            Client.SetConnectionType((ushort)connectionType);
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
                        case TypeCode.Boolean:
                        case TypeCode.String:
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

                int byteLength = GetSectionByteLength(dbSections);
                if (byteLength == 0) byteLength = 1;
                byte[] outBuffer = new byte[byteLength];

                foreach (DBData data in dbSections.Data)
                {
                    string nameAndType = data.Name + ":" + data.DataType.Name.ToString();

                    switch (Type.GetTypeCode(data.DataType))
                    {
                        case TypeCode.Byte:
                        case TypeCode.Boolean:
                        case TypeCode.String:
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

                // Write only the byte ranges that are defined in the section (adjacent elements are merged
                // into one request). Bytes in between/before are never touched, so several PCs can write
                // to different areas of the same DB without overwriting each other.
                foreach (var range in GetWriteRanges(dbSections))
                {
                    byte[] rangeBuffer = new byte[range.length];
                    Array.Copy(outBuffer, range.start, rangeBuffer, 0, range.length);
                    WritePLC(rangeBuffer, DBNumber, range.start, range.length);
                }
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

        /// <summary>
        /// Gets the size of one element of a given data type in the PLC (bytes).
        /// Byte, Boolean and String (1 byte per character) are handled as bytes.
        /// </summary>
        /// <returns>Returns element size in bytes, 0 if data type is not supported.</returns>
        public static int GetTypeSize(Type dataType)
        {
            switch (Type.GetTypeCode(dataType))
            {
                case TypeCode.Byte:
                case TypeCode.Boolean:
                case TypeCode.String:
                    return 1;
                case TypeCode.Int16:
                    return 2;
                case TypeCode.Int32:
                case TypeCode.Single:
                    return 4;
                default:
                    return 0;
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Gets the number of bytes from DB start to the end of the last element of the section.
        /// </summary>
        private int GetSectionByteLength(DBSection dbSection)
        {
            int byteLength = 0;
            foreach (DBData data in dbSection.Data)
            {
                byteLength = Math.Max(byteLength, data.StartAddress + GetTypeSize(data.DataType) * data.Length);
            }
            return byteLength;
        }

        /// <summary>
        /// Gets byte ranges occupied by the section elements. Overlapping/adjacent ranges are merged.
        /// </summary>
        private List<(int start, int length)> GetWriteRanges(DBSection dbSection)
        {
            var ranges = new List<(int start, int end)>();
            foreach (DBData data in dbSection.Data)
            {
                int size = GetTypeSize(data.DataType) * data.Length;
                if (size > 0) ranges.Add((data.StartAddress, data.StartAddress + size));
            }
            ranges.Sort((a, b) => a.start.CompareTo(b.start));

            var merged = new List<(int start, int length)>();
            int curStart = -1, curEnd = -1;
            foreach (var r in ranges)
            {
                if (curStart >= 0 && r.start <= curEnd)
                {
                    curEnd = Math.Max(curEnd, r.end);
                }
                else
                {
                    if (curStart >= 0) merged.Add((curStart, curEnd - curStart));
                    curStart = r.start;
                    curEnd = r.end;
                }
            }
            if (curStart >= 0) merged.Add((curStart, curEnd - curStart));
            return merged;
        }

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
            int byteLength = GetSectionByteLength(dbsection);
            if (byteLength == 0) byteLength = 1;

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
        /// <param name="startAddress">Start byte address in the DB.</param>
        /// <param name="byteLength">Byte length.</param>
        private void WritePLC(byte[] outBuffer, int DBNumber, int startAddress, int byteLength)
        {
            if (Client.DBWrite(DBNumber, startAddress, byteLength, outBuffer) != 0)
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
                        //byte[] oneByte = BitConverter.GetBytes(Convert.ToByte(valuesAndNames[i / typeLenght]));
                        //tmpBuffer[0] = oneByte[0];
                        tmpBuffer[0] = (byte)Convert.ToByte(valuesAndNames[i / typeLenght]);                        
                        break;
                    case TypeCode.Int16:
                        tmpBuffer = BitConverter.GetBytes(Convert.ToInt16(valuesAndNames[i / typeLenght]));
                        break;
                    case TypeCode.Int32:
                        tmpBuffer = BitConverter.GetBytes(Convert.ToInt32(valuesAndNames[i / typeLenght]));
                        break;
                    case TypeCode.Single:
                        tmpBuffer = BitConverter.GetBytes(Convert.ToSingle(valuesAndNames[i / typeLenght]));
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
