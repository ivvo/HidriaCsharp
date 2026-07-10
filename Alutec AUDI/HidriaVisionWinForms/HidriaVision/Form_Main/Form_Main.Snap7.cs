using Snap7Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace HidriaVision
{
    public partial class Form_Main : Form
    {
        #region Private methods
        /// <summary>
        /// Loads db sections for snap7.
        /// </summary>
        private void LoadSnap7DataSections()
        {
            DBData ApplicationStatusData = new DBData();
            DBSection WriteSection = new DBSection();

            // Load db number and db section data from settings
            int DBNumber = (int)MainSettings.GetElement("DBSection", "Write", "DBNumber");
            dynamic WriteDBSectionData = MainSettings.GetSegments("DBSection", "Write", "DBData").First();

            // Create db data and db section
            ApplicationStatusData.Name = WriteDBSectionData.Name;
            ApplicationStatusData.DataType = Type.GetType(WriteDBSectionData.DataType);
            ApplicationStatusData.StartAddress = WriteDBSectionData.StartAddress;
            ApplicationStatusData.Length = WriteDBSectionData.Length;

            WriteSection.DBNumber = DBNumber;
            WriteSection.Data = new List<DBData>() { ApplicationStatusData };

            // Add data sections to snap7 library
            Snap7ComManager.AddWriteSection(WriteSection);
        }
        #endregion
    }
}
