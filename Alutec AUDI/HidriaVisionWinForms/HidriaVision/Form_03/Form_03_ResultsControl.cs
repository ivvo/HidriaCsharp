using System;
using System.Drawing;
using System.Windows.Forms;

namespace HidriaVision
{
    // User control for displaying results
    public partial class Form_03_ResultsControl : UserControl
    {
        /// <summary>
        /// Init user control with defaults          
        /// </summary>   
        public Form_03_ResultsControl()
        {
            InitializeComponent();
        }

        public void SetSomething()
        {
            BeginInvoke(new Action(() => textBoxMeasurement.Text = "0"));
        }

        /// <summary>
        /// Sets common results of the control.
        /// </summary>
        /// <param name="commonResults">Common results.</param>
        public void SetCommonResults(Station03CommonResults commonResults)
        {
            if (InvokeRequired)
                BeginInvoke(new Action(() => SetCommonResults(commonResults)));
            else
            {
                // Orientation OK
                if (commonResults.OrientationOk)
                    textBoxMeasurement.Text = "OK";

                else
                    textBoxMeasurement.Text = "ROTATE";

                textBoxMeasurement.BackColor = Color.LimeGreen;

                textBoxMeasurement.Text = commonResults.FoundAt.ToString();

            }
        }
    }
}
