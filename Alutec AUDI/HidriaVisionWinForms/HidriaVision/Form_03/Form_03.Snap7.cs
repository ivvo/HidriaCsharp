using Snap7Manager;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace HidriaVision
{
    public partial class Form_03 : Form
    {
        #region Private methods
        /// <summary>
        /// Loads db sections for snap7.
        /// </summary>
        private void LoadSnap7DbSections()
        {
            DBSection ReadSection = new DBSection() { Data = new List<DBData>() };
            DBSection WriteSection = new DBSection() { Data = new List<DBData>() };

            // Load read db section data
            dynamic ReadDBSection = StationSettings.GetSegments("DBSection", "Read", "DBData");

            // Go through the sections
            foreach (var section in ReadDBSection)
            {
                DBData DataSection = new DBData
                {

                    // Create db data section
                    Name = section.Name,
                    DataType = Type.GetType(section.DataType),
                    StartAddress = section.StartAddress,
                    Length = section.Length
                };

                // Add data section
                ReadSection.Data.Add(DataSection);
            }

            // Add db number to read section
            ReadSection.DBNumber = (int)StationSettings.GetElement("DBSection", "Read", "DBNumber");

            // Load write db sections
            dynamic WriteDBSection = StationSettings.GetSegments("DBSection", "Write", "DBData");

            // Go through the sections
            foreach (var section in WriteDBSection)
            {
                DBData DataSection = new DBData
                {

                    // Create db data section
                    Name = section.Name,
                    DataType = Type.GetType(section.DataType),
                    StartAddress = section.StartAddress,
                    Length = section.Length
                };

                // Add data section
                WriteSection.Data.Add(DataSection);
            }

            // Add db number to write section
            WriteSection.DBNumber = (int)StationSettings.GetElement("DBSection", "Write", "DBNumber");

            // Add db sections to snap7 library
            Snap7ComManager.AddReadSection(ReadSection, SectionChangedCallback);
            Snap7ComManager.AddWriteSection(WriteSection);
        }

        /// <summary>
        /// Writes data to snap7.
        /// </summary>
        private void WriteSnap7Data()
        {
            // Send common status and results data via snap7
            Snap7ComManager.WriteValues(new List<(string, List<object>)>()
            {               
                ("Station03_Status:Byte", new List<object>(){ Status.Value }),
                ("Station03_ResultNumber_Response:Byte", new List<object>(){ResultResponse.Value})              
            });
        }
        #endregion

        #region Private events and callbacks
        /// <summary>
        /// Callback fires when there is change in particular section.
        /// </summary>
        /// <param name="name">Name of the section</param>
        /// <param name="values">Values.</param>
        private void SectionChangedCallback(string name, List<object> values)
        {
            lock (MainOperationObjLock)
            {
                bool IsMainOperationCycleStarted = MainOperationCycleStart.Value;

               
                if (name == "Station03_ResultNumber_Request:Byte")
                {
                    ResultRequest.Value = (byte)values[0];

                    // Check if result has changes
                    if (ResultRequest.Value != ResultResponse.Value)
                    {
                        // Update common control.
                        UpdateCommonStatusControl();
                        

                        // Reset handle
                        if (ResultRequest.Value == 0 || ResultRequest.Value == 255)
                        {
                            // Cancel the cycle if running
                            if (IsMainOperationCycleStarted)
                                MainOperationCycleCancel.Value = true;

                            ResultResponse.Value = ResultRequest.Value;                        

                            // Write data to snap7
                            WriteSnap7Data();

                            // Update common control.
                            UpdateCommonStatusControl();
                            
                        }
                        else
                        {
                            // Cancel the cycle if running. Otherwise start the cycle
                            if (IsMainOperationCycleStarted)
                                MainOperationCycleCancel.Value = true;
                            else
                            {
                                MainOperationCycleCancel.Value = false;
                                MainOperationCycleStart.Value = true;
                            }
                        }
                    }
                }

                if (name == "Station06_DMC_Request:Byte")
                {
                    string DMC_String = "";


                    foreach (byte b in values)
                    {
                        DMC_String += (Char)b;

                    }

                    DMC_codeRequest.Value = DMC_String;

                    // Check if type has changed
                    if (DMC_codeRequest.Value != DMC_codeResponse.Value)
                    {
                        DMC_codeResponse.Value = DMC_codeRequest.Value;

                        // Write data to snap7
                        WriteSnap7Data();

                        // Update common control. 
                        UpdateCommonStatusControl();
                    }
                }

                
            }
        }
        #endregion
    }
}