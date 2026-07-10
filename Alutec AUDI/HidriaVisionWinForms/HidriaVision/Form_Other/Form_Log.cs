using Logger;
using System;
using System.Windows.Forms;
using VisualLogger;

namespace HidriaVision
{
    public partial class Form_Log : Form
    {   
        public Form_Log(int numOfEntries)
        {
            InitializeComponent();

            // Set maximum number of log entries
            visualLog1.H_MaxEventsLogged = numOfEntries;
        }

        #region Private methods
        /// <summary>
        /// Add new log entry to the visual log.
        /// </summary>
        /// <param name="entry">Log entry.</param>
        public void AddEntry(LogEntry entry)
        {
            if (InvokeRequired)
                Invoke(new Action(() => AddEntry(entry)));
            else
            {
                // Make a conversion between loggers
                switch(entry.EventSeverity)
                {
                    case LoggingLevel.Info:
                        visualLog1.H_LogString(VisualLog.TextType.INFO, entry.Message);
                        break;

                    case LoggingLevel.Warning:
                        visualLog1.H_LogString(VisualLog.TextType.WARNING, entry.Message);
                        break;

                    case LoggingLevel.Error:
                        visualLog1.H_LogString(VisualLog.TextType.ERROR, entry.Message);
                        break;

                    default:
                        visualLog1.H_LogString(VisualLog.TextType.ERROR, entry.Message);
                        break;
                }
            }
        }
        #endregion

        #region Private events
        /// <summary>
        /// Event fires when main form is resized.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void Form_Log_Resize(object sender, EventArgs e)
        {
            int heightForm = this.Height;
            int widthForm = this.Width;

            this.panel1.Size = new System.Drawing.Size(widthForm - 24, heightForm - 36);
            this.panel1.Location = new System.Drawing.Point(12, 12);
        }
        #endregion
    }
}
