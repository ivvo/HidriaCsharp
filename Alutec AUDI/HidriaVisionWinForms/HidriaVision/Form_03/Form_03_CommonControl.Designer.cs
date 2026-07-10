namespace HidriaVision
{
    partial class Form_03_CommonControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelERROR = new System.Windows.Forms.Label();
            this.textBoxERROR = new System.Windows.Forms.TextBox();
            this.labelREADY = new System.Windows.Forms.Label();
            this.textBoxREADY = new System.Windows.Forms.TextBox();
            this.textBoxCycleTime = new System.Windows.Forms.TextBox();
            this.labelCycleTime = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelERROR
            // 
            this.labelERROR.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.labelERROR.ForeColor = System.Drawing.SystemColors.ActiveBorder;
            this.labelERROR.Location = new System.Drawing.Point(-11, 41);
            this.labelERROR.Name = "labelERROR";
            this.labelERROR.Size = new System.Drawing.Size(84, 19);
            this.labelERROR.TabIndex = 61;
            this.labelERROR.Text = "ERROR";
            this.labelERROR.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBoxERROR
            // 
            this.textBoxERROR.BackColor = System.Drawing.Color.Firebrick;
            this.textBoxERROR.Location = new System.Drawing.Point(79, 41);
            this.textBoxERROR.Name = "textBoxERROR";
            this.textBoxERROR.ReadOnly = true;
            this.textBoxERROR.Size = new System.Drawing.Size(20, 20);
            this.textBoxERROR.TabIndex = 60;
            // 
            // labelREADY
            // 
            this.labelREADY.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.labelREADY.ForeColor = System.Drawing.SystemColors.ActiveBorder;
            this.labelREADY.Location = new System.Drawing.Point(3, 15);
            this.labelREADY.Name = "labelREADY";
            this.labelREADY.Size = new System.Drawing.Size(70, 19);
            this.labelREADY.TabIndex = 59;
            this.labelREADY.Text = "READY";
            this.labelREADY.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // textBoxREADY
            // 
            this.textBoxREADY.BackColor = System.Drawing.Color.Firebrick;
            this.textBoxREADY.Location = new System.Drawing.Point(79, 15);
            this.textBoxREADY.Name = "textBoxREADY";
            this.textBoxREADY.ReadOnly = true;
            this.textBoxREADY.Size = new System.Drawing.Size(20, 20);
            this.textBoxREADY.TabIndex = 58;
            // 
            // textBoxCycleTime
            // 
            this.textBoxCycleTime.Location = new System.Drawing.Point(206, 27);
            this.textBoxCycleTime.Name = "textBoxCycleTime";
            this.textBoxCycleTime.ReadOnly = true;
            this.textBoxCycleTime.Size = new System.Drawing.Size(143, 20);
            this.textBoxCycleTime.TabIndex = 72;
            this.textBoxCycleTime.Text = "0 ms";
            this.textBoxCycleTime.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // labelCycleTime
            // 
            this.labelCycleTime.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.labelCycleTime.ForeColor = System.Drawing.SystemColors.ActiveBorder;
            this.labelCycleTime.Location = new System.Drawing.Point(119, 27);
            this.labelCycleTime.Name = "labelCycleTime";
            this.labelCycleTime.Size = new System.Drawing.Size(81, 19);
            this.labelCycleTime.TabIndex = 73;
            this.labelCycleTime.Text = "Cycle time:";
            this.labelCycleTime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // Form_03_CommonControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.Controls.Add(this.labelCycleTime);
            this.Controls.Add(this.textBoxCycleTime);
            this.Controls.Add(this.labelERROR);
            this.Controls.Add(this.textBoxERROR);
            this.Controls.Add(this.labelREADY);
            this.Controls.Add(this.textBoxREADY);
            this.Name = "Form_03_CommonControl";
            this.Size = new System.Drawing.Size(352, 85);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label labelERROR;
        private System.Windows.Forms.TextBox textBoxERROR;
        private System.Windows.Forms.Label labelREADY;
        private System.Windows.Forms.TextBox textBoxREADY;
        private System.Windows.Forms.TextBox textBoxCycleTime;
        private System.Windows.Forms.Label labelCycleTime;
    }
}
