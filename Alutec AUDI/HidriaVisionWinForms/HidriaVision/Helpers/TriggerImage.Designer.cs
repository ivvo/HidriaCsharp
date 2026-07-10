namespace HidriaVision.Helpers
{
    partial class TriggerImage
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
            this.btnTrigger = new System.Windows.Forms.Button();
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.numericUpDownExposure = new System.Windows.Forms.NumericUpDown();
            this.labelExposure = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownExposure)).BeginInit();
            this.SuspendLayout();
            // 
            // btnTrigger
            // 
            this.btnTrigger.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.btnTrigger.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTrigger.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnTrigger.Location = new System.Drawing.Point(7, 6);
            this.btnTrigger.Name = "btnTrigger";
            this.btnTrigger.Size = new System.Drawing.Size(270, 23);
            this.btnTrigger.TabIndex = 23;
            this.btnTrigger.Text = "Trigger camera";
            this.btnTrigger.UseVisualStyleBackColor = false;
            this.btnTrigger.Click += new System.EventHandler(this.btnTrigger_Click);
            // 
            // btnLoad
            // 
            this.btnLoad.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.btnLoad.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLoad.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnLoad.Location = new System.Drawing.Point(7, 35);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(130, 23);
            this.btnLoad.TabIndex = 24;
            this.btnLoad.Text = "Load BMP";
            this.btnLoad.UseVisualStyleBackColor = false;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnSave.Location = new System.Drawing.Point(147, 35);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(130, 23);
            this.btnSave.TabIndex = 25;
            this.btnSave.Text = "Save BMP";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // numericUpDownExposure
            // 
            this.numericUpDownExposure.DecimalPlaces = 3;
            this.numericUpDownExposure.Location = new System.Drawing.Point(157, 67);
            this.numericUpDownExposure.Name = "numericUpDownExposure";
            this.numericUpDownExposure.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownExposure.TabIndex = 27;
            this.numericUpDownExposure.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDownExposure.ValueChanged += new System.EventHandler(this.numericUpDownExposure_ValueChanged);
            // 
            // labelExposure
            // 
            this.labelExposure.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.labelExposure.ForeColor = System.Drawing.Color.White;
            this.labelExposure.Location = new System.Drawing.Point(9, 65);
            this.labelExposure.Name = "labelExposure";
            this.labelExposure.Size = new System.Drawing.Size(128, 20);
            this.labelExposure.TabIndex = 26;
            this.labelExposure.Text = "Exposure (ms):";
            this.labelExposure.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // TriggerImage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.Controls.Add(this.btnTrigger);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.numericUpDownExposure);
            this.Controls.Add(this.labelExposure);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.Name = "TriggerImage";
            this.Size = new System.Drawing.Size(284, 94);
            this.Load += new System.EventHandler(this.TriggerOneImage_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownExposure)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnTrigger;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.NumericUpDown numericUpDownExposure;
        private System.Windows.Forms.Label labelExposure;
    }
}
