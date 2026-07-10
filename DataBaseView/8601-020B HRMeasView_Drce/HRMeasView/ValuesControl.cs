using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HRMeasView
{
    public partial class ValuesControl : UserControl
    {
        private Single[] measValues;
        private Single maxvalue, minvalue, actvalue;
        public event PropertyChangedEventHandler PropertyChanged;

        public ValuesControl()
        {
            InitializeComponent();
        }

        protected void OnPropertyChanged(string propertieName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertieName));
            }
            
            actvalue = measValues[0];
            minvalue = measValues[1];
            maxvalue = measValues[2];

            textBoxAct.Text = actvalue.ToString();
            textBoxMin.Text = minvalue.ToString();
            textBoxMax.Text = maxvalue.ToString();
            textBoxAct.BackColor = Color.LightGreen;
            if (actvalue > maxvalue) { textBoxAct.BackColor = Color.LightSalmon ; }
            if (actvalue < minvalue) { textBoxAct.BackColor = Color.LightYellow; }
        }
     
        public Single[] MeasValues
        {
            set
            {
                measValues = value;
                OnPropertyChanged("MeasValues");
            }
        }
    }
}
