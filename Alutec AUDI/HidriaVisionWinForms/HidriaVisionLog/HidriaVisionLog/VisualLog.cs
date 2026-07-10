using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace VisualLogger
{
    public partial class VisualLog: UserControl
    {
        /// <summary>
        /// Fields
        /// </summary>
        static readonly object _lock = new object();

        /// <summary>
        /// Status strings structure with text + color
        /// </summary>   
        private struct statusStringCol
        {
            public string statusString;
            public Color statusStringColor;

            public statusStringCol(string a, Color b)
            {
                statusString = a;
                statusStringColor = b;
            }
        }

        public enum TextType
        {
            Custom,
            INFO,
            WARNING,
            ERROR          
        };

        
        private static List<statusStringCol> statusStringsList = new List<statusStringCol>();
        private int maxCount = 100;
        private Color customColor = Color.Black;
        private Color errorColor = Color.Firebrick;
        private Color warningColor = Color.ForestGreen;
        private Color infoColor = Color.Black;
        private Color tempColor;
      
        public VisualLog()
        {
            InitializeComponent();                      
            numericUpDown1.Value = maxCount;
            checkBox1.Checked = true;
        }

        /// <summary>
        /// Writes to LOG 
        /// </summary>   
        /// <param name="val"> Type of text</param>
        /// <param name="text"> String text</param>
        /// <param name="c"> Optional color</param>
        public void H_LogString(TextType val, string text, Color? c = null)
        {        
            lock (_lock)
            {
                switch (val)
                { 
                    case TextType.Custom:
                        tempColor = c ?? customColor;
                        H_AddString(text);
                        break;
                    case TextType.INFO:
                        tempColor = c ?? infoColor;
                        H_INFO(text);
                        break;
                    case TextType.WARNING:
                        tempColor = c ?? warningColor;
                        H_WARNING(text);
                        break; 
                    default:
                        tempColor = c ?? errorColor;
                        H_ERROR(text);
                        break; 
                }
            }
        }

        /// <summary>
        /// Refresh LOG 
        /// </summary>   
        public void H_Refresh()
        {
            richTextBox1.Update();
            richTextBox1.Refresh();
        }

        /// <summary>
        /// Writes custom string and color to log 
        /// </summary>   
        /// <param name="str"> String to add</param>
        /// <param name="col"> String color</param>
        private void H_AddString(string str)
        {           
                if (statusStringsList.Count >= maxCount)
                {
                    while (statusStringsList.Count >= maxCount)
                    {
                        statusStringsList.RemoveAt(0);
                    }
                }
                statusStringsList.Add(new statusStringCol(str + "\n", tempColor));

                //richTextBox1.SelectionProtected = false;
                if (!checkBox1.Checked) richTextBox1.SuspendPainting();
                
                if (richTextBox1.Lines.Count() > maxCount)
                {
                    while (richTextBox1.Lines.Count() > maxCount)
                    {
                        richTextBox1.ReadOnly = false;
                        richTextBox1.SelectionStart = 0;
                        richTextBox1.SelectionLength = richTextBox1.Text.IndexOf("\n") + 1;
                        richTextBox1.SelectedText = string.Empty;
                        richTextBox1.ReadOnly = true;
                    }                
                }
               
                richTextBox1.HideSelection = true;
                richTextBox1.SelectionStart = richTextBox1.Text.Length;
                richTextBox1.SelectionColor = tempColor;
                
               
                richTextBox1.AppendText(str + Environment.NewLine);
                richTextBox1.ResumePainting();
                //richTextBox1.SelectionProtected = true;
                if(checkBox1.Checked)richTextBox1.ScrollToCaret();                         
        }

        /// <summary>
        /// Writes timestamp and string for ERROR event to log 
        /// </summary>   
        /// <param name="str"></param>
        private void H_ERROR(string str)
        {            
                string pattern = "yyyy-MM-dd   HH:mm:ss";
                DateTime eventNow = DateTime.Now;

                H_AddString(eventNow.ToString(pattern) + "   [ERROR]   " + str);            
        }

        /// <summary>
        /// Writes timestamp and string for WARNING event to log 
        /// </summary>   
        /// <param name="str"></param>
        private void H_WARNING(string str)
        {         
                string pattern = "yyyy-MM-dd   HH:mm:ss";
                DateTime eventNow = DateTime.Now;

                H_AddString(eventNow.ToString(pattern) + "   [WARNING]   " + str);
        }
       

        /// <summary>
        /// Writes timestamp and string for INFO event to log 
        /// </summary>   
        /// <param name="str"></param>
        private void H_INFO(string str)
        {            
                string pattern = "yyyy-MM-dd   HH:mm:ss";
                DateTime eventNow = DateTime.Now;

                H_AddString(eventNow.ToString(pattern) + "   [INFO]   " + str);           
        }

        /// <summary>
        /// Gets logged events from log List<string>
        /// </summary>   
        public List<string> H_GetStrings()
        {
            lock (_lock)
            {
                List<string> temp = new List<string>();
                foreach (statusStringCol st in statusStringsList)
                {
                    temp.Add(st.statusString);
                }
                return temp;
            }
        }

        /// <summary>
        /// Property Get or Set control back color
        /// </summary>
        public Color H_ControlBackgroundColor
        {           
            get
            {
                return (this.BackColor);
            }
            set
            {
                this.BackColor = value;
            }           
        }

        /// <summary>
        /// Property Get or Set button panel back color
        /// </summary>   
        public Color H_ButtonPanelColor
        {
            get
            {
                return (this.panel1.BackColor);
            }
            set
            {
                this.panel1.BackColor = value;
            }
        }

        /// <summary>
        /// Property Get or Set log back color
        /// </summary>   
        public Color H_LogColor
        {
            get
            {
                return (this.richTextBox1.BackColor);
            }
            set
            {
                this.richTextBox1.BackColor = value;
            }
        }

        /// <summary>
        /// Property Get or Set log back color
        /// </summary>   
        public Color H_CustomColor
        {
            get
            {
                return (customColor);
            }
            set
            {
                customColor = value;
            }
        }
        /// <summary>
        /// Property Get or Set log back color
        /// </summary>   
        public Color H_InfoColor
        {
            get
            {
                return (infoColor);
            }
            set
            {
                infoColor = value;
            }
        }
        /// <summary>
        /// Property Get or Set log back color
        /// </summary>   
        public Color H_WarningColor
        {
            get
            {
                return (warningColor);
            }
            set
            {
                warningColor = value;
            }
        }
        /// <summary>
        /// Property Get or Set log back color
        /// </summary>   
        public Color H_ErrorColor
        {
            get
            {
                return (errorColor);
            }
            set
            {
                errorColor = value;
            }
        }

        /// <summary>
        /// Property Get or Set max events to log
        /// </summary>   
        public int H_MaxEventsLogged
        {
            get
            {
                return ((int)numericUpDown1.Value);
            }
            set
            {
                numericUpDown1.Value = value;
            }
        }

        /// <summary>
        /// Property Maximum events to log event
        /// </summary>   
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            lock (_lock)
            {
                maxCount = (int)numericUpDown1.Value;
            }
        }

        /// <summary>
        /// Enable/disable AutoScroll event
        /// </summary>   
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            lock (_lock)
            {
                if (checkBox1.Checked == true)
                {
                    richTextBox1.SuspendLayout();
                    richTextBox1.ScrollBars = RichTextBoxScrollBars.None;
                    richTextBox1.ResumeLayout();
                }
                else
                {
                    richTextBox1.SuspendLayout();
                    richTextBox1.ScrollBars = RichTextBoxScrollBars.ForcedVertical;
                    richTextBox1.SelectionStart = richTextBox1.Text.Length;
                    richTextBox1.ScrollToCaret();
                    richTextBox1.ResumeLayout();
                }
            }
        }

        /// <summary>
        /// Property Set max lines text
        /// </summary>   
        public string setMaxLinesText
        {           
            set
            {
                label1.Text = value;
            }
        }

        /// <summary>
        /// Property Set autoscroll text
        /// </summary>   
        public string setScrollText
        {
            set
            {
                checkBox1.Text = value;
            }
        }

        /// <summary>
        /// Modified RichTextBox for Log
        /// </summary>   
        private class RichTextBoxEx : RichTextBox
        {
            [DllImport("user32.dll")]
            protected static extern IntPtr SendMessage(IntPtr hWnd, Int32 wMsg, Int32 wParam, ref Point lParam);

            [DllImport("user32.dll")]
            protected static extern IntPtr SendMessage(IntPtr hWnd, Int32 wMsg, Int32 wParam, IntPtr lParam);

            protected const int WM_USER = 0x400;
            protected const int WM_SETREDRAW = 0x000B;
            protected const int EM_GETEVENTMASK = WM_USER + 59;
            protected const int EM_SETEVENTMASK = WM_USER + 69;
            protected const int EM_GETSCROLLPOS = WM_USER + 221;
            protected const int EM_SETSCROLLPOS = WM_USER + 222;
            protected const int WM_SETFOCUS = 0x0007;
            protected const int WM_KILLFOCUS = 0x0008;


            protected Point _ScrollPoint;
            protected bool _Painting = true;
            protected IntPtr _EventMask;
            protected int _SuspendIndex = 0;
            protected int _SuspendLength = 0;

            /// <summary>
            /// Suspend painting - disable autoscroll
            /// </summary>        
            public void SuspendPainting()
            {
                if (_Painting)
                {
                    _SuspendIndex = this.SelectionStart;
                    _SuspendLength = this.SelectionLength;
                    SendMessage(this.Handle, EM_GETSCROLLPOS, 0, ref _ScrollPoint);
                    SendMessage(this.Handle, WM_SETREDRAW, 0, IntPtr.Zero);
                    _EventMask = SendMessage(this.Handle, EM_GETEVENTMASK, 0, IntPtr.Zero);
                    _Painting = false;
                }
            }

            /// <summary>
            /// Resume painting - enable autoscroll
            /// </summary>       
            public void ResumePainting()
            {
                if (!_Painting)
                {
                    this.Select(_SuspendIndex, _SuspendLength);
                    SendMessage(this.Handle, EM_SETSCROLLPOS, 0, ref _ScrollPoint);
                    SendMessage(this.Handle, EM_SETEVENTMASK, 0, _EventMask);
                    SendMessage(this.Handle, WM_SETREDRAW, 1, IntPtr.Zero);
                    _Painting = true;
                    this.Invalidate();
                }
            }

            /// <summary>
            /// Kill focus - disable selection highlighting
            /// </summary>       
            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_SETFOCUS) m.Msg = WM_KILLFOCUS;

                base.WndProc(ref m);
            }

        }
    }
}
