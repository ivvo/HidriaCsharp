using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HidriaSnap7;
using System.Xml;
using System.Collections.Specialized;
using System.Xml.Linq;
using System.Windows.Forms.DataVisualization.Charting;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;

namespace HRMeasView
{
    public partial class FormView : Form
    {
        private Task _task;
        static bool _update;
        string IP_Address;
        HSnap7 PLC;
        List<HSnap7.DBSection> DBSections;
        Dictionary<string, List<dynamic>> SectionsData = new Dictionary<string, List<dynamic>>();
       
        List<double> valuesListChart1 = new List<double>();
        List<double> valuesListChart2 = new List<double>();
        List<double> valuesListChart3 = new List<double>();
        List<double> valuesListChart4 = new List<double>();
        List<double> valuesListChart5 = new List<double>();
        List<double> valuesListChart6 = new List<double>();
        List<double> valuesListChart7 = new List<double>();
        List<double> valuesListChart8 = new List<double>();

        Single[] MeasData = new Single[3];
        bool[] Enable = new bool[8];
        byte[] EnableCheck = new byte[8];
          
        public FormView()
        {
            InitializeComponent();
            PLC = new HSnap7();
            DBSections = new List<HSnap7.DBSection>();
        }

        private void FormView_Shown(object sender, EventArgs e)
        {
            ReadXML();
            GetSections();
            ExtractData();
            readTimer.Enabled = true;
            this._task = new Task(this.TaskWorker);
            _task.Start();
        }

        private void ConnectToPLC()
        {
            try
            {
                PLC.PlcConnect(IP_Address, 0, 1);              
            }
            catch (Exception ex)
            {         
                DialogResult result = MessageBox.Show("Napaka povezave na PLC.", this.Text, MessageBoxButtons.RetryCancel, MessageBoxIcon.Warning);
                if (result == System.Windows.Forms.DialogResult.Retry) { ConnectToPLC(); } else { Application.Exit(); }
            }
        }

        private void ReadXML()
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(@"C:\Hidria\Settings\settings.xml");

                XDocument loaded = XDocument.Load(@"C:\Hidria\Settings\settings.xml");
                IP_Address = loaded.Descendants("HidriaSnap7").Select(x => x.Element("IP_Address").Value).FirstOrDefault();

                XmlNodeList nodeList = xmlDoc.DocumentElement.SelectNodes("/Settings/HidriaSnap7/DBSection");

                DBSections.Clear();
                foreach (XmlNode node in nodeList)
                {
                    HSnap7.DBSection dbsection = new HSnap7.DBSection();
                    dbsection.Name = node.SelectSingleNode("Name").InnerText;
                    dbsection.DataType = Type.GetType(node.SelectSingleNode("DataType").InnerText);
                    dbsection.DBNumber = Convert.ToInt32(node.SelectSingleNode("DBNumber").InnerText);
                    dbsection.StartAddress = Convert.ToInt32(node.SelectSingleNode("StartAddress").InnerText);
                    dbsection.Length = Convert.ToInt32(node.SelectSingleNode("Length").InnerText);

                    DBSections.Add(dbsection);
                }                           
            }
            catch (Exception)
            {
                MessageBox.Show("Napaka branja nastavitev.(XML)", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Application.Exit();
            }   
        }

        private void GetSections()
        {
            try
            {
                SectionsData = PLC.GetSections(DBSections);
            }
            catch (Exception ex)
            {
                switch (ex.Message)
                {
                    case "Error Connection" :
                        ConnectToPLC();
                        break;
                    case "Error when Reading":                     
                        MessageBox.Show("Napaka branja podatkov.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Application.Exit();
                        break;
                    default:
                        break;
                }
            }        
        }
      
        private void ExtractData()
        {
            int i;
            foreach (var sectiondata in SectionsData)
            {
                // Check Enables
                if (sectiondata.Key == "ReadTriggers:Byte")
                {
                    i = 0;
                    foreach (var value in sectiondata.Value)
                    {
                        if (EnableCheck[i] != value) 
                        { 
                            EnableCheck[i] = value; Enable[i] = true; 
                        } 
                        i++;                     
                    }
                }
                else
                {
                    i = 0;
                    foreach (var value in sectiondata.Value) { MeasData[i] = value; i++; }
                }

                // Draw Charts If Enabled
                switch (sectiondata.Key.ToString())
                {
                    case "Unit_02:Single" :
                        if (Hide_U02_Graph.Checked)
                            Enable[0] = false;
                        if (Enable[0]) 
                        {
                            FillChart(U_02_Chart, sectiondata.Key.ToString(), valuesListChart1, 75, MeasData[0], MeasData[1], MeasData[2], MeasData[1] - 1.0, MeasData[2] + 1.0);
                            valuesCtrl_A2.MeasValues = MeasData;                         
                            Enable[0] = false;
                        }
                        break;
                    case "Drca-4:Single":
                        if (Enable[1])
                        {
                            FillChart(U_04_Chart_1, sectiondata.Key.ToString(), valuesListChart2, 75, MeasData[0], MeasData[1], MeasData[2], MeasData[1] - 1.0, MeasData[2] + 1.0);
                            valuesCtrl_A4_1.MeasValues = MeasData;
                            Enable[1] = false;
                        }
                        break;
                    case "Drca-5:Single":
                        if (Enable[2])
                        {
                            FillChart(U_04_Chart_2, sectiondata.Key.ToString(), valuesListChart3, 75, MeasData[0], MeasData[1], MeasData[2], MeasData[1] - 1.0, MeasData[2] + 1.0);
                            valuesCtrl_A4_2.MeasValues = MeasData;
                            Enable[2] = false;
                        }
                        break;
                    case "Drca-6:Single":
                        if (Enable[3])
                        {
                            FillChart(U_04_Chart_3, sectiondata.Key.ToString(), valuesListChart4, 75, MeasData[0], MeasData[1], MeasData[2], MeasData[1] - 1.0, MeasData[2] + 1.0);
                            valuesCtrl_A4_3.MeasValues = MeasData;
                            Enable[3] = false;
                        }
                        break;
                    case "Unit_12:Single":
                        if (Hide_U12_Graph.Checked)
                            Enable[4] = false;
                        if (Enable[4])
                        {
                            FillChart(U_12_Chart, sectiondata.Key.ToString(), valuesListChart5, 75, MeasData[0], MeasData[1], MeasData[2], MeasData[1] - 1.0, MeasData[2] + 1.0);
                            valuesCtrl_B2.MeasValues = MeasData;
                            Enable[4] = false;
                        }
                        break;
                    case "Drca-1:Single":
                        if (Enable[5])
                        {
                            FillChart(U_14_Chart_1, sectiondata.Key.ToString(), valuesListChart6, 75, MeasData[0], MeasData[1], MeasData[2], MeasData[1] - 1.0, MeasData[2] + 1.0);
                            valuesCtrl_B4_1.MeasValues = MeasData;
                            Enable[5] = false;
                        }
                        break;
                    case "Drca-2:Single":
                        if (Enable[6])
                        {
                            FillChart(U_14_Chart_2, sectiondata.Key.ToString(), valuesListChart7, 75, MeasData[0], MeasData[1], MeasData[2], MeasData[1] - 1.0, MeasData[2] + 1.0);
                            valuesCtrl_B4_2.MeasValues = MeasData;
                            Enable[6] = false;
                        }
                        break;
                    case "Drca-3:Single":
                        if (Enable[7])
                        {
                            FillChart(U_14_Chart_3, sectiondata.Key.ToString(), valuesListChart8, 75, MeasData[0], MeasData[1], MeasData[2], MeasData[1] - 1.0, MeasData[2] + 1.0);
                            valuesCtrl_B4_3.MeasValues = MeasData;
                            Enable[7] = false;
                        }
                        break;
                    default:
                        break;
                }         
            }      
        }

        private void FillChart(Chart chartview, string seriename, List<double> xvalues, int pointscount, double newvalue, double minvalue, double maxvalue, double chartmin, double chartmax)
        {
            chartview.Series.Clear();
            chartview.Series.Add(seriename);
            chartview.Series[seriename].SetDefault(true);
            chartview.Series[seriename].Enabled = true;
            chartview.Visible = true;
            chartview.ChartAreas[0].AxisX.LabelStyle.Enabled = false;
            chartview.ChartAreas[0].AxisX.Enabled = AxisEnabled.False;

            chartview.Series.Add("Max");
            chartview.Series["Max"].SetDefault(true);
            chartview.Series["Max"].Enabled = true;
            chartview.Series["Max"].ChartType = SeriesChartType.Line;
            chartview.Visible = true;
            chartview.ChartAreas[0].AxisX.LabelStyle.Enabled = false;
            chartview.Series["Max"].IsVisibleInLegend = false;
            chartview.ChartAreas[0].AxisX.Enabled = AxisEnabled.False;

            chartview.Series.Add("Min");
            chartview.Series["Min"].SetDefault(true);
            chartview.Series["Min"].Enabled = true;
            chartview.Series["Min"].ChartType = SeriesChartType.Line;
            chartview.Visible = true;
            chartview.ChartAreas[0].AxisX.LabelStyle.Enabled = false;
            chartview.Series["Min"].IsVisibleInLegend = false;
            chartview.ChartAreas[0].AxisX.Enabled = AxisEnabled.False;
           
      
            if (xvalues.Count == 0)
            {
                for (int i = 0; i < pointscount; i++)
                {
                    xvalues.Add(0);
                }
            }

            for (int i = pointscount - 1; i > 0; i--)
            {
                xvalues[i] = xvalues[i - 1];
            }
            
            xvalues[0] = newvalue;
         
            int yaxiscount = 0;
            foreach (var item in xvalues)
            {
                int index = chartview.Series[seriename].Points.AddXY(yaxiscount, item);
                chartview.Series["Max"].Points.AddXY(yaxiscount, maxvalue);
                chartview.Series["Min"].Points.AddXY(yaxiscount, minvalue);
                chartview.Series[seriename].Points[index].Color = Color.LightGreen;
                chartview.Series["Min"].Points[index].Color = Color.Yellow;
                chartview.Series["Max"].Points[index].Color = Color.Red;
                if (item > maxvalue) { chartview.Series[seriename].Points[index].Color = Color.OrangeRed; }
                if (item < minvalue) { chartview.Series[seriename].Points[index].Color = Color.Yellow; }
                yaxiscount++;
            }
        
            chartview.ChartAreas[0].AxisY.Minimum = Math.Round(chartmin);
            chartview.ChartAreas[0].AxisY.Maximum = Math.Round(chartmax);
          
        }

        private void TaskWorker()
        {
            while (readTimer.Enabled)
            {
                if (!_update)
                {
                    GetSections();
                    _update = true;
                }
                Thread.Sleep(500);
            }
        }

        private void readTimer_Tick(object sender, EventArgs e)
        {
            if (_update)
            {
                ExtractData();
                _update = false;
            }
        }

        private void FormView_Load(object sender, EventArgs e)
        {
            showOnMonitor(1);

        }

        private void showOnMonitor(int showOnMonitor)
        {
            Screen[] sc;
            sc = Screen.AllScreens;
            if (showOnMonitor >= sc.Length)
            {
                showOnMonitor = 0;
            }

            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(sc[showOnMonitor].Bounds.Left, sc[showOnMonitor].Bounds.Top);
            // If you intend the form to be maximized, change it to normal then maximized.
            this.WindowState = FormWindowState.Normal;
            this.WindowState = FormWindowState.Maximized;
        }

        private void ClearCharts_Click(object sender, EventArgs e)
        {        
            valuesListChart1.Clear();
            valuesListChart2.Clear();
            valuesListChart3.Clear();
            valuesListChart4.Clear();
            valuesListChart5.Clear();
            valuesListChart6.Clear();
            valuesListChart7.Clear();
            valuesListChart8.Clear();

            U_02_Chart.Series.Clear();
            U_04_Chart_1.Series.Clear();
            U_04_Chart_2.Series.Clear();
            U_04_Chart_3.Series.Clear();
            U_12_Chart.Series.Clear();
            U_14_Chart_1.Series.Clear();
            U_14_Chart_2.Series.Clear();
            U_14_Chart_3.Series.Clear();
        }

        private void Hide_U12_Graph_CheckedChanged(object sender, EventArgs e)
        {
            if (Hide_U12_Graph.Checked)
            {
                U_12_Chart.Visible = false;
                valuesCtrl_B2.Visible = false;
                groupBox1.Visible = false;
            }
            if (!Hide_U12_Graph.Checked)
            {
                U_12_Chart.Visible = true;
                valuesCtrl_B2.Visible = true;
                groupBox1.Visible = true;
            }
        }

        private void Hide_U02_Graph_CheckedChanged(object sender, EventArgs e)
        {
            if (Hide_U02_Graph.Checked)
            {
                U_02_Chart.Visible = false;
                valuesCtrl_A2.Visible = false;
                groupBox3.Visible = false;
            }
            if (!Hide_U02_Graph.Checked)
            {
                U_02_Chart.Visible = true;
                valuesCtrl_A2.Visible = true;
                groupBox3.Visible = true;
            }
        }


    }
}
