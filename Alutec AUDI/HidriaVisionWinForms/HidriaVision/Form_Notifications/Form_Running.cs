using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HidriaVision
{
    public partial class Form_Running : Form
    {
        private int countDown = 5;
        private string title = "Hidria Vision is allready running!";
        private string infoText = "This warning will close in "; 
 
        public Form_Running()
        {
            InitializeComponent();           
        }

        private void Init()
        {
            labelHeader.Text = title;
            labelInfo.Text = infoText + countDown.ToString();
        }
       
        public void closeForm()
        {
            this.Invoke((MethodInvoker)delegate { this.Close(); });
        }

        private void Form_Running_Load(object sender, EventArgs e)
        {
            Init();
            timerCountDown.Start();
        }

        private void timerCountDown_Tick(object sender, EventArgs e)
        {
            if (countDown > 0)
            {
                countDown--;
                labelInfo.Text = infoText + countDown.ToString();
            }
            else
            {
                timerCountDown.Stop();
                timerCountDown.Enabled = false;
                closeForm();
            }
        }

        public string SetTextHeader
        {
            set
            {
                title = value;
                this.Text = value;
            }
        }

        public string SetInfoText
        {
            set
            {
                infoText = value;
            }      
        }

        public int countDownVal
        {
            set
            {
                countDown = value;
            }
        }

    }
}
