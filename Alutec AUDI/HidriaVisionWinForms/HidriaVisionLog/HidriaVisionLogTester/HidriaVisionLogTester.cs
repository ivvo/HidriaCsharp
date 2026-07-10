using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HidriaVisionLog;

namespace HidriaVisionLogTester
{
    public partial class HidriaVisionLogTester : Form
    {
        int count = 0;
        int count1 = 0;
        int count2 = 0;

        public HidriaVisionLogTester()
        {
            InitializeComponent();           
            timer1.Enabled = true;
            timer2.Enabled = true;
            timer3.Enabled = true;        
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //Random randomGen = new Random();
            //KnownColor[] names = (KnownColor[]) Enum.GetValues(typeof(KnownColor));
            //KnownColor randomColorName = names[randomGen.Next(names.Length)];
            //Color randomColor = Color.FromKnownColor(randomColorName);

            //hidriaVisionLog1.addString(count.ToString(), randomColor);
            hidriaVisionLog1.H_LogString(VisionLog.TextType.INFO,count.ToString());
            //hidriaVisionLog1.H_AddString()
            count++;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            hidriaVisionLog1.H_LogString(VisionLog.TextType.WARNING, count.ToString());
            count1++;
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
            hidriaVisionLog1.H_LogString(VisionLog.TextType.ERROR, count.ToString());
            count2++;
        }


    }
}
