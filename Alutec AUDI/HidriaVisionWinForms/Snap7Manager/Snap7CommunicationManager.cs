using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;

namespace Snap7Manager
{
    /// <summary>
    /// Represents snap7 communication manager.
    /// </summary>
    public class Snap7CommunicationManager
    {
        #region Delegates
        public delegate void SectionChangedCallback(string name, List<object> values);
        #endregion

        #region Events
        /// <summary>
        /// Event triggers when PLC connection status changes.
        /// </summary>
        public EventHandler<Snap7onnectionStatusChangedEventArgs> ConnectionStatusChanged;
        #endregion

        #region Private fields
        private List<(DBSection dbSection, Dictionary<string, List<object>> writeValues)> WriteSections;
        private List<(DBSection dbSection, Dictionary<string, List<object>> readValues, SectionChangedCallback callback)> ReadSections;
        private HSnap7 Snap7;
        private Thread SnapThrd;
        private CancellationTokenSource TokenSource;
        private string PLCIpAddr;
        private int Rack;
        private int Slot;
        private bool IsManagerRunning;
        #endregion

        /// <summary>
        /// Creates new instance of Snap7CommunicationManager.
        /// </summary>
        /// <param name="plcIpAddress">PLC ip address.</param>
        /// <param name="rack">PLC rack.</param>
        /// <param name="slot">PLC slot.</param>
        /// <exception cref="ArgumentException"></exception>
        public Snap7CommunicationManager(string plcIpAddress, int rack, int slot)
        {
            IPAddress IPAddr;

            WriteSections = new List<(DBSection, Dictionary<string, List<object>>)>();
            ReadSections = new List<(DBSection, Dictionary<string, List<object>>, SectionChangedCallback)>();
            Snap7 = new HSnap7();
            IsManagerRunning = false;

            // Check if ip address is null or empty
            if (string.IsNullOrEmpty(plcIpAddress))
                throw new ArgumentException("Is address is null or empty.");

            // Check if ip address is valid
            if(!IPAddress.TryParse(plcIpAddress, out IPAddr))
                throw new ArgumentException("IP address not valid.");

            // Check if rack is positive
            if (rack < 0)
                throw new ArgumentException("Rack value must be positive.");

            // Check if slot is positive
            if (slot < 0)
                throw new ArgumentException("Slot value must be positive.");

            Rack = rack;
            Slot = slot;
            PLCIpAddr = plcIpAddress;
        }

        #region Public methods
        /// <summary>
        /// Starts snap7 manager.
        /// </summary>
        public void Start()
        {
            // Proceed only if manager is not running
            if(!IsManagerRunning)
            {
                // Create new thread and token source
                TokenSource = new CancellationTokenSource();
                SnapThrd = new Thread(SnapThread);

                // Start thread
                SnapThrd.Start();

                // Set running flag
                IsManagerRunning = true;
            }
        }

        /// <summary>
        /// Stops snap7 manager.
        /// </summary>
        public void Stop()
        {
            // Proceed only if manager is running
            if(IsManagerRunning)
            {
                // Stop thread and dispose token source
                TokenSource.Cancel();
                SnapThrd.Join(500);

                TokenSource.Dispose();

                // Reset running flag
                IsManagerRunning = false;
            }
        }

        /// <summary>
        /// Adds new read section.
        /// </summary>
        /// <param name="readSection">Read section.</param>
        /// <param name="callback">Callback.</param>
        public void AddReadSection(DBSection readSection, SectionChangedCallback callback)
        {
            // Lock and add new read section
            lock (ReadSections)
                ReadSections.Add((readSection, Snap7.CreateDefaultDictionary(readSection), callback));
        }

        /// <summary>
        /// Adds new write section.
        /// </summary>
        /// <param name="writeSection">Write section.</param>
        public void AddWriteSection(DBSection writeSection)
        {
            // Lock and add new write section
            lock (WriteSections)
                WriteSections.Add((writeSection, Snap7.CreateDefaultDictionary(writeSection)));
        }

        /// <summary>
        /// Writes a value to the write section.
        /// </summary>
        /// <param name="sectionElementName">Element name.</param>
        /// <param name="values">Values to write.</param>
        /// <exception cref="ArgumentException"></exception>
        public void WriteValue(string sectionElementName, List<object> values)
        {
            // Lock
            lock(WriteSections)
            {
                // Check if section element name exists
                if(WriteSections.Any(x => x.writeValues.ContainsKey(sectionElementName)))
                {
                    var Section = WriteSections.First(x => x.writeValues.ContainsKey(sectionElementName));

                    // Update value in the dictionary
                    Section.writeValues[sectionElementName] = values;

                    return;
                }

                // Element with a given name does not exist
                throw new ArgumentException("Element with a given name does not exist.");
            }
        }

        /// <summary>
        /// Writes multiples values to the write sections.
        /// </summary>
        /// <param name="sectionElements">Section elements.</param>
        /// <exception cref="ArgumentException"></exception>
        public void WriteValues(List<(string sectionElementName, List<object> values)> sectionElements)
        {
            // Lock
            lock (WriteSections)
            {
                // Go through section element names
                foreach(var sectionElement in sectionElements)
                {
                    // Check if section element name exists
                    if (WriteSections.Any(x => x.writeValues.ContainsKey(sectionElement.sectionElementName)))
                    {
                        var Section = WriteSections.First(x => x.writeValues.ContainsKey(sectionElement.sectionElementName));

                        // Update value in the dictionary
                        Section.writeValues[sectionElement.sectionElementName] = sectionElement.values;
                    }
                    else
                    {
                        // One or more elements don't exist
                        throw new ArgumentException("One or more elements with a given name do not exist.");
                    }
                }
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Thread loop.
        /// </summary>
        private void SnapThread()
        {
            PLCConnectionStatus ConnectionStatus = PLCConnectionStatus.Offline;
            PLCConnectionStatus PrevConnectionStatus = (PLCConnectionStatus)(-1);

            // Main loop
            while(!TokenSource.IsCancellationRequested)
            {
                // Try to connect to PLC
                if(ConnectionStatus == PLCConnectionStatus.Offline || ConnectionStatus == PLCConnectionStatus.Error)
                {
                    try
                    {
                        Snap7.PlcDisconnect();
                        Snap7.PlcConnect(PLCIpAddr, Rack, Slot);
                        ConnectionStatus = PLCConnectionStatus.Online;
                    }
                    catch (Snap7Exception) { /* Exception is swallowed in this case because we cannot do anything about it. Connection status is reported through the event.*/}
                }

                // Proceed only if connection is alive
                if (ConnectionStatus == PLCConnectionStatus.Online)
                {
                    try
                    {
                        // Read sections
                        lock (ReadSections)
                        {
                            // Go through db sections
                            foreach (var section in ReadSections)
                            {
                                // Get db read section
                                Dictionary<string, List<object>> CurrentDbReadSection = new Dictionary<string, List<object>>(Snap7.GetSections(section.dbSection));

                                // Go through the sections of a db read section
                                foreach(string dbSectionKey in CurrentDbReadSection.Keys)
                                {
                                    // Check if section differs
                                    if (!CurrentDbReadSection[dbSectionKey].SequenceEqual(section.readValues[dbSectionKey]))
                                    {
                                        // Update values and call callback
                                        section.readValues[dbSectionKey] = new List<object>(CurrentDbReadSection[dbSectionKey]);
                                        section.callback(dbSectionKey, new List<object>(CurrentDbReadSection[dbSectionKey]));
                                    }
                                }
                            }
                        }

                        // Write sections
                        lock (WriteSections)
                        {
                            // Go through db sections
                            foreach (var section in WriteSections)
                            {
                                // Write values
                                Snap7.SetSections(section.writeValues, section.dbSection);
                            }
                        }
                    }
                    catch(Snap7Exception e)
                    {
                        // Set connection status based on the exception message
                        if (e.Message == "Error when Reading" || e.Message == "Error when Writing")
                            ConnectionStatus = PLCConnectionStatus.Error;
                        else
                            ConnectionStatus = PLCConnectionStatus.Offline;
                    }
                }

                // If there is a connection change, raise an event
                if (ConnectionStatus != PrevConnectionStatus)
                {
                    PrevConnectionStatus = ConnectionStatus;
                    ConnectionStatusChanged?.Invoke(this, new Snap7onnectionStatusChangedEventArgs(ConnectionStatus));
                }

                Thread.Sleep(1);
            }

            // Disconnect from PLC if online
            if (ConnectionStatus == PLCConnectionStatus.Online)
            {
                Snap7.PlcDisconnect();
            }
        }
        #endregion
    }
}
