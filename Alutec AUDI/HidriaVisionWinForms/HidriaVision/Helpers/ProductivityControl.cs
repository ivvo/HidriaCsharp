using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace HidriaVisionProductivity
{
    public partial class ProductivityControl : UserControl
    {
        private string[] diagnosticsChartLanguage = { "Diagnostics", "OK", "NOK", "INTERRUPTED", "START", "STOP", "RESET" };


        private Object _lock = new Object();

        private bool count = true;
        private bool exploded = false;

        private uint OK = 0;
        private uint NOK = 0;
        private uint INTERRUPTED = 0;

        private uint all = 0;

        private delegate void SafeUpdate();
        private delegate void SafeStringUpdate(string text);

        public ProductivityControl()
        {
            InitializeComponent();
            defaultPie();

            SetLanguage();
        }

        private void labelStartStop_Click(object sender, EventArgs e)
        {
            lock (_lock)
            {
                if (count)
                {
                    count = false;
                    labelStartStop.Text = diagnosticsChartLanguage[4];
                }
                else
                {
                    count = true;
                    labelStartStop.Text = diagnosticsChartLanguage[5];
                }
            }
        }

        private void labelReset_Click(object sender, EventArgs e)
        {
            resetToZero();
        }

        private void resetToZero()
        {
            lock (_lock)
            {
                OK = 0;
                NOK = 0;
                INTERRUPTED = 0;
                all = 0;
                defaultPie();
            }
        }

        public void CountUpOK()
        {
            if (this.InvokeRequired)
            {
                SafeUpdate d = new SafeUpdate(CountUpOK);
                this.Invoke(d, new object[] { });
            }
            else
            {
                if (count)
                {
                    lock (_lock)
                    {
                        all++;
                        OK++;
                        calculatePie();
                    }
                }
            }
        }

        public void CountUpNOK()
        {
            if (this.InvokeRequired)
            {
                SafeUpdate d = new SafeUpdate(CountUpNOK);
                this.Invoke(d, new object[] { });
            }
            else
            {
                if (count)
                {
                    lock (_lock)
                    {
                        all++;
                        NOK++;
                        calculatePie();
                    }
                }
            }
        }

        public void CountUpINTERRUPTED()
        {
            if (this.InvokeRequired)
            {
                SafeUpdate d = new SafeUpdate(CountUpINTERRUPTED);
                this.Invoke(d, new object[] { });
            }
            else
            {
                if (count)
                {
                    lock (_lock)
                    {
                        all++;
                        INTERRUPTED++;
                        calculatePie();
                    }
                }
            }
        }

        private void defaultPie()
        {
            lock (_lock)
            {
                chartProductivity.Series.SuspendUpdates();
                chartProductivity.Series[0].Points[0].YValues[0] = 1.0d;
                chartProductivity.Series[0].Points[0].SetCustomProperty("Exploded", "False");
                chartProductivity.Series[0].Points[1].YValues[0] = 0.0d;
                chartProductivity.Series[0].Points[1].SetCustomProperty("Exploded", "False");
                chartProductivity.Series[0].Points[2].YValues[0] = 0.0d;
                chartProductivity.Series[0].Points[2].SetCustomProperty("Exploded", "False");

                textBoxOkCount.Text = "0x";
                textBoxNokCount.Text = "0x";
                textBoxInterruptCount.Text = "0x";

                textBoxOkPercent.Text = "0.0%";
                textBoxNokPercent.Text = "0.0%";
                textBoxInterruptPercent.Text = "0.0%";

                exploded = false;
                chartProductivity.Update();
                chartProductivity.Series.ResumeUpdates();
            }
        }

        private void calculatePie()
        {
            if (this.InvokeRequired)
            {
                SafeUpdate d = new SafeUpdate(calculatePie);
                this.Invoke(d, new object[] { });
            }
            else
            {
                lock (_lock)
                {
                    chartProductivity.Series.SuspendUpdates();
                    int tmpCount = 0;
                    {
                        if (OK != 0) tmpCount++;
                        if (NOK != 0) tmpCount++;
                        if (INTERRUPTED != 0) tmpCount++;
                    }
                    if (tmpCount > 1 && !exploded)
                    {
                        chartProductivity.Series[0].Points[0].SetCustomProperty("Exploded", "True");
                        chartProductivity.Series[0].Points[1].SetCustomProperty("Exploded", "True");
                        chartProductivity.Series[0].Points[2].SetCustomProperty("Exploded", "True");
                        exploded = true;
                    }
                    chartProductivity.Series[0].Points[0].YValues[0] = OK;
                    chartProductivity.Series[0].Points[1].YValues[0] = NOK;
                    chartProductivity.Series[0].Points[2].YValues[0] = INTERRUPTED;

                    textBoxOkCount.Text = OK.ToString() + "x";
                    textBoxNokCount.Text = NOK.ToString() + "x";
                    textBoxInterruptCount.Text = INTERRUPTED.ToString() + "x";

                    uint ALL = OK + NOK + INTERRUPTED;
                    double tmpOk = ((double)OK / (double)ALL) * 100.0d;
                    double tmpNok = ((double)NOK / (double)ALL) * 100.0d;
                    double tmpInterrupted = ((double)INTERRUPTED / (double)ALL) * 100.0d;

                    textBoxOkPercent.Text = tmpOk.ToString("0.0") + "%";
                    textBoxNokPercent.Text = tmpNok.ToString("0.0") + "%";
                    textBoxInterruptPercent.Text = tmpInterrupted.ToString("0.0") + "%";

                    chartProductivity.Update();
                    chartProductivity.Series.ResumeUpdates();
                }

                if (all >= 999999)
                {
                    resetToZero();
                }
            }
        }

        public string[] SetLanguages
        {
            set
            {
                diagnosticsChartLanguage = (string[])value.Clone();
                SetLanguage();
            }
        }

        public void SetLanguage()
        {
            labelProductivity.Text = diagnosticsChartLanguage[0];
            labelOk.Text = diagnosticsChartLanguage[1];
            labelNok.Text = diagnosticsChartLanguage[2];
            labelInterrupted.Text = diagnosticsChartLanguage[3];
            if (count)
            {              
                labelStartStop.Text = diagnosticsChartLanguage[5];
            }
            else
            {              
                labelStartStop.Text = diagnosticsChartLanguage[4];
            }
            labelReset.Text = diagnosticsChartLanguage[6];
        }
    }
}
