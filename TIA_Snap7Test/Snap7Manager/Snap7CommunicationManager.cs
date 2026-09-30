using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;

namespace Snap7Manager
{
    /// <summary>
    /// Represents snap7 communication manager.
    /// One instance = one PLC connection with its own thread and its own S7 client, so the application
    /// can create one manager per PLC to communicate with several PLCs at the same time.
    /// </summary>
    public class Snap7CommunicationManager
    {
        #region Delegates
        public delegate void SectionChangedCallback(string name, List<object> values);
        #endregion

        #region Events
        /// <summary>
        /// Event triggers when PLC connection status changes.
        /// Raised from the communication thread - subscribers must not block it (use BeginInvoke for UI).
        /// </summary>
        public EventHandler<Snap7onnectionStatusChangedEventArgs> ConnectionStatusChanged;
        #endregion

        #region Private types
        private class WriteSectionState
        {
            public DBSection DbSection;
            public Dictionary<string, List<object>> WriteValues;
            public bool Dirty;
        }
        #endregion

        #region Private fields
        private List<WriteSectionState> WriteSections;
        private List<(DBSection dbSection, Dictionary<string, List<object>> readValues, SectionChangedCallback callback)> ReadSections;
        private HSnap7 Snap7;
        private Thread SnapThrd;
        private CancellationTokenSource TokenSource;
        private string PLCIpAddr;
        private int Rack;
        private int Slot;
        public bool IsManagerRunning;
        #endregion

        #region Public properties
        /// <summary>
        /// Gets PLC ip address.
        /// </summary>
        public string PlcIpAddress => PLCIpAddr;

        /// <summary>
        /// Connection type (PG/OP/Basic). Must be set before Start().
        /// </summary>
        public PLCConnectionType ConnectionType { get; set; } = PLCConnectionType.PG;

        /// <summary>
        /// Pause between read/write cycles in ms.
        /// </summary>
        public int CycleTimeMs { get; set; } = 1;

        /// <summary>
        /// Pause between reconnect attempts in ms (avoids hammering the PLC connection resources).
        /// </summary>
        public int ReconnectDelayMs { get; set; } = 1000;

        /// <summary>
        /// When true, a write section is written only after its values changed (and once after every (re)connect).
        /// When false, write sections are written every cycle.
        /// </summary>
        public bool WriteOnChangeOnly { get; set; } = false;
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

            WriteSections = new List<WriteSectionState>();
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

        /// <summary>
        /// Creates new instance of Snap7CommunicationManager with a given connection type.
        /// </summary>
        /// <param name="plcIpAddress">PLC ip address.</param>
        /// <param name="rack">PLC rack.</param>
        /// <param name="slot">PLC slot.</param>
        /// <param name="connectionType">Connection type (PG/OP/Basic).</param>
        /// <exception cref="ArgumentException"></exception>
        public Snap7CommunicationManager(string plcIpAddress, int rack, int slot, PLCConnectionType connectionType)
            : this(plcIpAddress, rack, slot)
        {
            ConnectionType = connectionType;
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
                CancellationToken token = TokenSource.Token;
                SnapThrd = new Thread(() => SnapThread(token))
                {
                    IsBackground = true,
                    Name = "Snap7 " + PLCIpAddr
                };

                // Start thread
                SnapThrd.Start();

                // Set running flag
                IsManagerRunning = true;
            }
        }

        /// <summary>
        /// Stops snap7 manager. Waits until the communication thread has finished, so the S7 client is never
        /// used by two threads at the same time (e.g. fast Stop/Start while a connect attempt is still running).
        /// </summary>
        public void Stop()
        {
            // Proceed only if manager is running
            if(IsManagerRunning)
            {
                // Stop thread and dispose token source
                TokenSource.Cancel();
                SnapThrd.Join();

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
                WriteSections.Add(new WriteSectionState { DbSection = writeSection, WriteValues = Snap7.CreateDefaultDictionary(writeSection), Dirty = true });
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
                if(WriteSections.Any(x => x.WriteValues.ContainsKey(sectionElementName)))
                {
                    var Section = WriteSections.First(x => x.WriteValues.ContainsKey(sectionElementName));

                    // Update value in the dictionary
                    Section.WriteValues[sectionElementName] = values;
                    Section.Dirty = true;

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
                    if (WriteSections.Any(x => x.WriteValues.ContainsKey(sectionElement.sectionElementName)))
                    {
                        var Section = WriteSections.First(x => x.WriteValues.ContainsKey(sectionElement.sectionElementName));

                        // Update value in the dictionary
                        Section.WriteValues[sectionElement.sectionElementName] = sectionElement.values;
                        Section.Dirty = true;
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
        private void SnapThread(CancellationToken token)
        {
            PLCConnectionStatus ConnectionStatus = PLCConnectionStatus.Offline;
            PLCConnectionStatus PrevConnectionStatus = (PLCConnectionStatus)(-1);

            // Main loop
            while(!token.IsCancellationRequested)
            {
                // Try to connect to PLC
                if(ConnectionStatus == PLCConnectionStatus.Offline || ConnectionStatus == PLCConnectionStatus.Error)
                {
                    try
                    {
                        Snap7.PlcDisconnect();
                        Snap7.PlcConnect(PLCIpAddr, Rack, Slot, ConnectionType);
                        ConnectionStatus = PLCConnectionStatus.Online;

                        // After (re)connect write all sections once, even in WriteOnChangeOnly mode
                        lock (WriteSections)
                            foreach (var section in WriteSections) section.Dirty = true;
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
                                if (WriteOnChangeOnly && !section.Dirty) continue;

                                // Write values
                                Snap7.SetSections(section.WriteValues, section.DbSection);
                                section.Dirty = false;
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

                // Wait (cancellable) - longer while not connected
                int delay = (ConnectionStatus == PLCConnectionStatus.Online) ? CycleTimeMs : ReconnectDelayMs;
                token.WaitHandle.WaitOne(Math.Max(1, delay));
            }

            // Disconnect from PLC
            Snap7.PlcDisconnect();

            // Report offline after stop
            if (PrevConnectionStatus != PLCConnectionStatus.Offline)
                ConnectionStatusChanged?.Invoke(this, new Snap7onnectionStatusChangedEventArgs(PLCConnectionStatus.Offline));
        }
        #endregion
    }
}
