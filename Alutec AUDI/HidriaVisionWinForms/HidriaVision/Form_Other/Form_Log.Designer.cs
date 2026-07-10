namespace HidriaVision
{
    partial class Form_Log
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {           
            this.panel1 = new System.Windows.Forms.Panel();
            this.visualLog1 = new VisualLogger.VisualLog();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.visualLog1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(948, 539);
            this.panel1.TabIndex = 0;
            // 
            // visualLog1
            // 
            this.visualLog1.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.visualLog1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.visualLog1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.visualLog1.H_ButtonPanelColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.visualLog1.H_ControlBackgroundColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.visualLog1.H_CustomColor = System.Drawing.SystemColors.ActiveBorder;
            this.visualLog1.H_ErrorColor = System.Drawing.Color.Firebrick;
            this.visualLog1.H_InfoColor = System.Drawing.SystemColors.ActiveBorder;
            this.visualLog1.H_LogColor = System.Drawing.SystemColors.ControlText;
            this.visualLog1.H_MaxEventsLogged = 100;
            this.visualLog1.H_WarningColor = System.Drawing.Color.ForestGreen;
            this.visualLog1.Location = new System.Drawing.Point(0, 0);
            this.visualLog1.Margin = new System.Windows.Forms.Padding(2);
            this.visualLog1.Name = "visualLog1";
            this.visualLog1.Size = new System.Drawing.Size(946, 537);
            this.visualLog1.TabIndex = 0;
            // 
            // Form_Log
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(948, 539);
            this.ControlBox = false;
            this.Controls.Add(this.panel1);
            this.ForeColor = System.Drawing.SystemColors.AppWorkspace;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_Log";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Form_Log";
            this.Resize += new System.EventHandler(this.Form_Log_Resize);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private VisualLogger.VisualLog visualLog1;
    }
}