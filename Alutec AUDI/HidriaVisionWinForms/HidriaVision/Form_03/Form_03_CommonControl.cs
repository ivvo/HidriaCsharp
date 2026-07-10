using System;
using System.Drawing;
using System.Windows.Forms;

namespace HidriaVision
{
    // User control for controls
    public partial class Form_03_CommonControl : UserControl
    {    
        /// <summary>
        /// Init user control with defaults         
        /// </summary>   
        public Form_03_CommonControl()
        {
            InitializeComponent();          
        }

        /// <summary>
        /// Sets common status of the control.
        /// </summary>
        /// <param name="commonStatus">Common status.</param>
        public void SetCommonStatus(Station03CommonStatus commonStatus)
        {
            if (InvokeRequired)
                BeginInvoke(new Action(() => SetCommonStatus(commonStatus)));
            else
            {

                // Set ready, error and part bad colors
                textBoxREADY.BackColor = commonStatus.Status.HasFlag(Station03CommonStatusFlags.Ready) ? Color.LimeGreen : Color.Firebrick;
                textBoxERROR.BackColor = commonStatus.Status.HasFlag(Station03CommonStatusFlags.Error) ? Color.LimeGreen : Color.Firebrick;
                

                // Set cycle time
                textBoxCycleTime.Text = $"{commonStatus.CycleTime}ms";
            }
        }

    }
}
