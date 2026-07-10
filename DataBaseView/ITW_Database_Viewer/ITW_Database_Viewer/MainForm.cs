using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Collections.Specialized;
using System.Xml.Linq;

namespace ITW_Database_Viewer
{
    public partial class FormMain : Form
    {
        #region Constants
        private readonly string SqlConnectionString = @"Data Source=RAYCAP01-PC\SQLEXPRESS;Initial Catalog=Hidria;Integrated Security=True;User ID=hidriatc;Password=hidria#tc";
        private readonly string SqlTableNameStation01 = "Products";
        private readonly string SqlTableNameStation02 = "U_A07_Measure_1";
		private readonly string SqlTableNameStation03 = "U_B03_Measure_2";
		private readonly string SqlTableNameStation04 = "U_A07_ETALON";
		private readonly string SqlQueryTemplateNoResultStatus = "SELECT {0} FROM {1} WHERE CAST(entry_date AS DATE) >= '{2}' AND CAST(entry_date AS DATE) <= '{3}' ORDER BY entry_date DESC";
        private readonly string SqlQueryTemplateResultStatus = "SELECT {0} FROM {1} WHERE result_status = '{2}' AND CAST(entry_date AS DATE) >= '{3}' AND  CAST(entry_date AS DATE) <= '{4}' ORDER BY entry_date DESC";

        private static string XMLsettingsPath = AppDomain.CurrentDomain.BaseDirectory + "\\Settings.xml";      
        private static XmlDocument xmlSettings = new XmlDocument();

        private bool extendedOption = true;
       
        #endregion

        public FormMain()
        {
            CultureInfo Culture;

            InitializeComponent();

            //xmlSettings.Load(@XMLsettingsPath);        
            //extendedOption = bool.Parse(xmlSettings.DocumentElement.SelectSingleNode("/Settings/ExtendedMeasurements/Value").InnerText);
            //if (!extendedOption)
           //{
              //  panelExtendedOption.Visible = false;
            //}

            // Select proper language
            if (Properties.Settings.Default.SelectedLanguage == "en")
            {
                // English language
                Culture = CultureInfo.GetCultureInfo("en");
                ApplyResourceToControl(this, new ComponentResourceManager(this.GetType()), Culture);
                radioButtonLanguageEnglish.Checked = true;
                radioButtonLanguageSlovenian.Checked = false;
            }
            else
            {
                // Slovenian language
                Culture = CultureInfo.GetCultureInfo("sl");
                ApplyResourceToControl(this, new ComponentResourceManager(this.GetType()), Culture);
                radioButtonLanguageEnglish.Checked = false;
                radioButtonLanguageSlovenian.Checked = true;
            }

            Thread.CurrentThread.CurrentUICulture = Culture;

            // Disable station 01 results controls
            DisableResultsControls();
        }

        #region Events
        /// <summary>
        /// Event fires when one of the language radio buttons has been clicked
        /// </summary>
        /// <param name="sender">Source object<</param>
        /// <param name="e">Args</param>
        private void radioButtonLanguage_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton CheckedRadioButton = (RadioButton)sender;
            CultureInfo Culture;

            // Set english or slovenian language
            if(CheckedRadioButton == radioButtonLanguageEnglish)
            {
                Culture = CultureInfo.GetCultureInfo("en");
                Properties.Settings.Default.SelectedLanguage = "en";
                ApplyResourceToControl(this, new ComponentResourceManager(this.GetType()), Culture);
            }
            else
            {
                Culture = CultureInfo.GetCultureInfo("sl");
                Properties.Settings.Default.SelectedLanguage = "sl";
                ApplyResourceToControl(this, new ComponentResourceManager(this.GetType()), Culture);
            }

            Thread.CurrentThread.CurrentUICulture = Culture;

            // Save settings
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// Event fires when one of the station radio buttons has been clicked
        /// </summary>
        /// <param name="sender">Source object</param>
        /// <param name="e">Args</param>
        private void radioButtonStation_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton CheckedRadioButton = (RadioButton)sender;

            // Disable or enable controls based on the selected station
            if (CheckedRadioButton == radioButtonStation01)
                DisableResultsControls();
            else
                EnableResultsControls();
        }

        /// <summary>
        /// Event fires when button for results retrieval has been clicked
        /// </summary>
        /// <param name="sender">Source object</param>
        /// <param name="e">Args</param>
        private async void buttonGetResults_Click(object sender, EventArgs e)
        {
            // Disable panel
            panelControls.Enabled = false;

            try
            {
                // Construct query based on selected results
                string SqlQuery = GenerateQuery();

                // Get data
                DataTable Data = await GetDataTableAsync(SqlQuery);
                dataGridViewSql.DataSource = null;
                // Populate grid view with data
                dataGridViewSql.DataSource = Data;
            }
            catch(SqlException ex)
            {
                MessageBox.Show(Properties.MainFormMessages.Msg1 + ex.Message);
            }
            finally
            {
                // Enable panel
                panelControls.Enabled = true;
            }
        }

        /// <summary>
        /// Event fires when one of the export buttons has been clicked
        /// </summary>
        /// <param name="sender">Source object</param>
        /// <param name="e">Args</param>
        private void buttonExport_Click(object sender, EventArgs e)
        {
            // Check if there are entries inside data grid view to export
            if (dataGridViewSql.RowCount > 0)
            {
                try
                {
                    Button ExportButton = (Button)sender;

                    // Write data to csv or xlsx file depending on the clicked button
                    if (ExportButton == buttonExportCsv)
                        WriteDataToCSV();
                    else
                        WriteDataToXlsx();
                }
                catch (Exception ex)
                {
                    if (ex is UnauthorizedAccessException || ex is SecurityException || ex is IOException)
                    {
                        MessageBox.Show(Properties.MainFormMessages.Msg2 + ex.Message);

                        return;
                    }

                    throw;
                }
            }
            else
                MessageBox.Show(Properties.MainFormMessages.Msg3);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Generates query according to selected results
        /// </summary>
        /// <returns>Returns generated query</returns>
        private string GenerateQuery()
        {
            StringBuilder Query = new StringBuilder();
            StringBuilder DynamicQueryPart = new StringBuilder();

            // Construct static part of the query
            DynamicQueryPart.Append("start_id,result_status");

            // Measurement 1
            if ((checkBoxMeasurement1.Checked && extendedOption))
            {
                DynamicQueryPart.Append(",measurement_1");
                // Parameters
                if (checkBoxParameters.Checked)
                {
                    // Append common measurement parameters
                    DynamicQueryPart.Append(",measurement_1_min,measurement_1_max");
                }
            }

            // Measurement 2
            if (checkBoxMeasurement2.Checked)
            {
                DynamicQueryPart.Append(",measurement_2");
                // Parameters
                if (checkBoxParameters.Checked)
                {
                    // Append common measurement parameters
                    DynamicQueryPart.Append(",measurement_2_min,measurement_2_max");
                }
            }

            // Measurement 3
            if (checkBoxMeasurement3.Checked)
            {
                DynamicQueryPart.Append(",measurement_3");
                // Parameters
                if (checkBoxParameters.Checked)
                {
                    // Append common measurement parameters
                    DynamicQueryPart.Append(",measurement_3_min,measurement_3_max");
                }
            }

            // Measurement 4
            if (checkBoxMeasurement4.Checked)
            {
                DynamicQueryPart.Append(",measurement_4");
                // Parameters
                if (checkBoxParameters.Checked)
                {
                    // Append common measurement parameters
                    DynamicQueryPart.Append(",measurement_4_min,measurement_4_max");
                }
            }

            // Measurement 5
            if (radioButtonStation02.Checked && checkBoxMeasurement5.Checked)
            {
                DynamicQueryPart.Append(",measurement_5");
                // Append additional measurement parameters for station 2
                if (checkBoxParameters.Checked && radioButtonStation02.Checked)
                {
                    DynamicQueryPart.Append(",measurement_5_min,measurement_5_max");
                }
            }

            // Measurement 6
            if (radioButtonStation02.Checked && checkBoxMeasurement6.Checked)
            {
                DynamicQueryPart.Append(",measurement_6");
                // Append additional measurement parameters for station 2
                if (checkBoxParameters.Checked && radioButtonStation02.Checked)
                {
                    DynamicQueryPart.Append(",measurement_6_min,measurement_6_max");
                }
            }

            // Measurement 7
            if (radioButtonStation02.Checked && checkBoxMeasurement7.Checked)
            {
                DynamicQueryPart.Append(",measurement_7");
                // Append additional measurement parameters for station 2
                if (checkBoxParameters.Checked && radioButtonStation02.Checked)
                {
                    DynamicQueryPart.Append(",measurement_7_min,measurement_7_max");
                }
            }

            // Measurement 8
            if (radioButtonStation02.Checked && checkBoxMeasurement8.Checked)
            {
                DynamicQueryPart.Append(",measurement_8");
                // Append additional measurement parameters for station 2
                if (checkBoxParameters.Checked && radioButtonStation02.Checked)
                {
                    DynamicQueryPart.Append(",measurement_8_min,measurement_8_max");
                }
            }

            // Measurement 9
            if (radioButtonStation02.Checked && checkBoxMeasurement9.Checked)
            {
                DynamicQueryPart.Append(",measurement_9");
                // Append additional measurement parameters for station 2
                if (checkBoxParameters.Checked && radioButtonStation02.Checked)
                {
                    DynamicQueryPart.Append(",measurement_9_min,measurement_9_max");
                }
            }

            // Measurement 10
            if ((radioButtonStation02.Checked && checkBoxMeasurement10.Checked) || (!extendedOption && radioButtonStation02.Checked && checkBoxMeasurement1.Checked))
            {
                DynamicQueryPart.Append(",measurement_10");
                // Append additional measurement parameters for station 2
                if (checkBoxParameters.Checked && radioButtonStation02.Checked)
                {
                    DynamicQueryPart.Append(",measurement_10_min,measurement_10_max");
                }
            }

            // Measurement 11
            if (radioButtonStation02.Checked && checkBoxMeasurement11.Checked)
            {
                DynamicQueryPart.Append(",measurement_11");
                // Append additional measurement parameters for station 2
                if (checkBoxParameters.Checked && radioButtonStation02.Checked)
                {
                    DynamicQueryPart.Append(",measurement_11_min,measurement_11_max");
                }
            }

            // Area 1
            if (checkBoxArea1.Checked)
            {
                DynamicQueryPart.Append(",area_1");
                // Append additional measurement parameters for station 2
                if (checkBoxParameters.Checked)
                {
                    DynamicQueryPart.Append(",area_1_min,area_1_max");
                }
            }

            // Area 2
            if (checkBoxArea2.Checked)
            {
                DynamicQueryPart.Append(",area_2");
                // Append additional measurement parameters for station 2
                if (checkBoxParameters.Checked)
                {
                    DynamicQueryPart.Append(",area_2_min,area_2_max");
                }
            }

            // Area 3
            if (checkBoxArea3.Checked)
            {
                DynamicQueryPart.Append(",area_3");
                // Append additional measurement parameters for station 2
                if (checkBoxParameters.Checked)
                {
                    DynamicQueryPart.Append(",area_3_min,area_3_max");
                }
            }

            // Area 4
            if (checkBoxArea4.Checked)
            {
                DynamicQueryPart.Append(",area_4");
                // Append additional measurement parameters for station 2
                if (checkBoxParameters.Checked)
                {
                    DynamicQueryPart.Append(",area_4_min,area_4_max");
                }
            }

            // Area 5
            if (radioButtonStation02.Checked && checkBoxArea5.Checked)
            {
                DynamicQueryPart.Append(",area_5");
                // Append additional measurement parameters for station 2
                if (checkBoxParameters.Checked && radioButtonStation02.Checked)
                {
                    DynamicQueryPart.Append(",area_5_min,area_5_max");
                }
            }

            // Area 6
            if (radioButtonStation02.Checked && checkBoxArea6.Checked)
            {
                DynamicQueryPart.Append(",area_6");
                // Append additional measurement parameters for station 2
                if (checkBoxParameters.Checked && radioButtonStation02.Checked)
                {
                    DynamicQueryPart.Append(",area_6_min,area_6_max");
                }
            }

            // Area 7
            if (radioButtonStation02.Checked && checkBoxArea7.Checked)
            {
                DynamicQueryPart.Append(",area_7");
                // Append additional measurement parameters for station 2
                if (checkBoxParameters.Checked && radioButtonStation02.Checked)
                {
                    DynamicQueryPart.Append(",area_7_min,area_7_max");
                }
            }

            // Construct the final query
            if (radioButtonAll.Checked)
                Query.AppendFormat(SqlQueryTemplateNoResultStatus, DynamicQueryPart, radioButtonStation01.Checked ? SqlTableNameStation01 : SqlTableNameStation02, dateTimePickerFrom.Value.ToString("yyyy-MM-dd"), dateTimePickerTo.Value.ToString("yyyy-MM-dd"));
            else
                Query.AppendFormat(SqlQueryTemplateResultStatus, DynamicQueryPart, radioButtonStation01.Checked ? SqlTableNameStation01 : SqlTableNameStation02, radioButtonOk.Checked ? "OK" : "NOK", dateTimePickerFrom.Value.ToString("yyyy-MM-dd"), dateTimePickerTo.Value.ToString("yyyy-MM-dd"));

            return Query.ToString();
        }

        /// <summary>
        /// Writes data to CSV file
        /// </summary>
        private void WriteDataToCSV()
        {
            // Open save dialog
            using (SaveFileDialog SaveFile = new SaveFileDialog())
            {
                // Set file filter
                SaveFile.Filter = "Csv files (*.csv)|*.csv";

                if (SaveFile.ShowDialog() == DialogResult.OK)
                {
                    StringBuilder CsvBuild = new StringBuilder();
                    DataTable Dt = (DataTable)dataGridViewSql.DataSource;

                    // Get columns
                    IEnumerable<string> columnNames = Dt.Columns.Cast<DataColumn>().
                                                      Select(column => column.ColumnName);

                    // Append columns
                    CsvBuild.AppendLine(string.Join(";", columnNames));

                    // Append rows
                    foreach (DataRow row in Dt.Rows)
                    {
                        IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());

                        CsvBuild.AppendLine(string.Join(";", fields));
                    }

                    // Save content
                    File.WriteAllText(SaveFile.FileName, CsvBuild.ToString());
                }
            }
        }

        /// <summary>
        /// Writes data to XLSX file
        /// </summary>
        private void WriteDataToXlsx()
        {
            // Open save dialog
            using (SaveFileDialog SaveFile = new SaveFileDialog())
            {
                // Set file filter
                SaveFile.Filter = "Xlsx files (*.xlsx)|*.xlsx";

                if (SaveFile.ShowDialog() == DialogResult.OK)
                {
                    using (XLWorkbook Wb = new XLWorkbook())
                    {
                        // Add DataTable to excel
                        Wb.Worksheets.Add((DataTable)dataGridViewSql.DataSource, "WorksheetName");

                        // Save content
                        Wb.SaveAs(SaveFile.FileName);
                    }   
                }
            }
        }

        /// <summary>
        /// Disable specific measurement and area controls based on the selected station
        /// </summary>
        private void DisableResultsControls()
        {
            // Disable specific measurement and area controls
            checkBoxMeasurement5.Enabled = checkBoxMeasurement6.Enabled = checkBoxMeasurement7.Enabled = checkBoxMeasurement8.Enabled = false;
            checkBoxMeasurement9.Enabled = checkBoxMeasurement10.Enabled = checkBoxMeasurement11.Enabled = false;

            checkBoxArea5.Enabled = checkBoxArea6.Enabled = checkBoxArea7.Enabled = false;

            if (!extendedOption)
            {
                checkBoxMeasurement1.Checked = false;
                checkBoxMeasurement1.Enabled = false;
            }
        }

        /// <summary>
        /// Enable specific measurement and area controls based on the selected station
        /// </summary>
        private void EnableResultsControls()
        {
            // Enable specific measurement and area controls
            checkBoxMeasurement5.Enabled = checkBoxMeasurement6.Enabled = checkBoxMeasurement7.Enabled = checkBoxMeasurement8.Enabled = true;
            checkBoxMeasurement9.Enabled = checkBoxMeasurement10.Enabled = checkBoxMeasurement11.Enabled = true;

            checkBoxArea5.Enabled = checkBoxArea6.Enabled = checkBoxArea7.Enabled = true;

            if (!extendedOption) checkBoxMeasurement1.Enabled = true;
        }

        /// <summary>
        /// Apply the resources to the control
        /// </summary>
        /// <param name="control">Control</param>
        /// <param name="cmp">Resource manager</param>
        /// <param name="cultureInfo">CUlture info</param>
        private void ApplyResourceToControl(Control control, ComponentResourceManager cmp, CultureInfo cultureInfo)
        {
            foreach (Control child in control.Controls)
            {
                // Store current position and size of the control
                Size ChildSize = child.Size;
                Point ChildLoc = child.Location;

                // Apply CultureInfo to child control
                ApplyResourceToControl(child, cmp, cultureInfo);

                // Restore position and size
                child.Location = ChildLoc;
                child.Size = ChildSize;
            }

            // Do the same with the parent control
            Size ParentSize = control.Size;
            Point ParentLoc = control.Location;

            cmp.ApplyResources(control, control.Name, cultureInfo);

            control.Location = ParentLoc;
            control.Size = ParentSize;
        }

        /// <summary>
        /// Gets DataTable async
        /// </summary>
        /// <param name="SqlQuery">Sql query string</param>
        /// <returns>Populated DataTable</returns>
        private Task<DataTable> GetDataTableAsync(string SqlQuery)
        {
            return Task.Run(() =>
            {
                // Get data from the table
                using (SqlConnection SqlConnection = new SqlConnection(SqlConnectionString))
                using (SqlDataAdapter SqlAdapter = new SqlDataAdapter(SqlQuery, SqlConnection))
                {
                    DataSet SqlDataSet = new DataSet();
                    SqlAdapter.Fill(SqlDataSet);
                    return SqlDataSet.Tables[0];
                }
            });
        }
        #endregion
      
    }
}
