namespace HidriaVisionLogTester
{
    partial class HidriaVisionLogTester
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
            this.components = new System.ComponentModel.Container();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.timer3 = new System.Windows.Forms.Timer(this.components);
            this.hidriaVisionLog1 = new HidriaVisionLog.VisionLog();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // timer2
            // 
            this.timer2.Interval = 170;
            this.timer2.Tick += new System.EventHandler(this.timer2_Tick);
            // 
            // timer3
            // 
            this.timer3.Interval = 236;
            this.timer3.Tick += new System.EventHandler(this.timer3_Tick);
            // 
            // hidriaVisionLog1
            // 
            this.hidriaVisionLog1.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.hidriaVisionLog1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.hidriaVisionLog1.H_ButtonPanelColor = System.Drawing.SystemColors.ActiveBorder;
            this.hidriaVisionLog1.H_ControlBackgroundColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.hidriaVisionLog1.H_LogColor = System.Drawing.SystemColors.Info;
            this.hidriaVisionLog1.H_MaxEventsLogged = 1;
            this.hidriaVisionLog1.Location = new System.Drawing.Point(27, 12);
            this.hidriaVisionLog1.Name = "hidriaVisionLog1";
            this.hidriaVisionLog1.Size = new System.Drawing.Size(759, 415);
            this.hidriaVisionLog1.TabIndex = 0;
            // 
            // HidriaVisionLogTester
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(858, 468);
            this.Controls.Add(this.hidriaVisionLog1);
            this.Name = "HidriaVisionLogTester";
            this.Text = "Form1";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.Timer timer3;
        private HidriaVisionLog.VisionLog hidriaVisionLog1;



    }
}

