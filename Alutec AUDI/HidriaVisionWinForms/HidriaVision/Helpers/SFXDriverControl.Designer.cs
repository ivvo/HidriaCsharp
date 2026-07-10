namespace HidriaVision.Helpers
{
    partial class SFXDriverControl
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
            this.trackBarIntensity = new System.Windows.Forms.TrackBar();
            this.labelIntensity = new System.Windows.Forms.Label();
            this.textBoxValueIntensity = new System.Windows.Forms.TextBox();
            this.panelIntensity = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarIntensity)).BeginInit();
            this.panelIntensity.SuspendLayout();
            this.SuspendLayout();
            // 
            // trackBarIntensity
            // 
            this.trackBarIntensity.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.trackBarIntensity.Dock = System.Windows.Forms.DockStyle.Fill;
            this.trackBarIntensity.Location = new System.Drawing.Point(0, 0);
            this.trackBarIntensity.Margin = new System.Windows.Forms.Padding(0);
            this.trackBarIntensity.Maximum = 255;
            this.trackBarIntensity.Name = "trackBarIntensity";
            this.trackBarIntensity.Size = new System.Drawing.Size(240, 45);
            this.trackBarIntensity.TabIndex = 0;
            this.trackBarIntensity.ValueChanged += new System.EventHandler(this.trackBarIntensity_ValueChanged);
            // 
            // labelIntensity
            // 
            this.labelIntensity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelIntensity.AutoSize = true;
            this.labelIntensity.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.labelIntensity.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.labelIntensity.Location = new System.Drawing.Point(2, 30);
            this.labelIntensity.Name = "labelIntensity";
            this.labelIntensity.Size = new System.Drawing.Size(46, 13);
            this.labelIntensity.TabIndex = 1;
            this.labelIntensity.Text = "Intensity";
            // 
            // textBoxValueIntensity
            // 
            this.textBoxValueIntensity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxValueIntensity.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBoxValueIntensity.Location = new System.Drawing.Point(188, 30);
            this.textBoxValueIntensity.Name = "textBoxValueIntensity";
            this.textBoxValueIntensity.Size = new System.Drawing.Size(50, 13);
            this.textBoxValueIntensity.TabIndex = 1;
            this.textBoxValueIntensity.TabStop = false;
            this.textBoxValueIntensity.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBoxValueIntensity.TextChanged += new System.EventHandler(this.textBoxValueIntensity_TextChanged);
            // 
            // panelIntensity
            // 
            this.panelIntensity.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelIntensity.Controls.Add(this.labelIntensity);
            this.panelIntensity.Controls.Add(this.textBoxValueIntensity);
            this.panelIntensity.Controls.Add(this.trackBarIntensity);
            this.panelIntensity.Location = new System.Drawing.Point(0, 0);
            this.panelIntensity.Margin = new System.Windows.Forms.Padding(0);
            this.panelIntensity.Name = "panelIntensity";
            this.panelIntensity.Size = new System.Drawing.Size(240, 45);
            this.panelIntensity.TabIndex = 3;
            // 
            // SFXDriverControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this.panelIntensity);
            this.Name = "SFXDriverControl";
            this.Size = new System.Drawing.Size(240, 45);
            this.Load += new System.EventHandler(this.SFXDriverControl_Load);
            ((System.ComponentModel.ISupportInitialize)(this.trackBarIntensity)).EndInit();
            this.panelIntensity.ResumeLayout(false);
            this.panelIntensity.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TrackBar trackBarIntensity;
        private System.Windows.Forms.Label labelIntensity;
        private System.Windows.Forms.TextBox textBoxValueIntensity;
        private System.Windows.Forms.Panel panelIntensity;
    }
}
