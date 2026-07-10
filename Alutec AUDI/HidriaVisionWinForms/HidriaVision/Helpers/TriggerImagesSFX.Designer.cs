namespace HidriaVision.Helpers
{
    partial class TriggerImagesSFX
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
            this.btnTriggerSFX = new System.Windows.Forms.Button();
            this.btnLoad1 = new System.Windows.Forms.Button();
            this.btnSave1 = new System.Windows.Forms.Button();
            this.numericUpDownExposure = new System.Windows.Forms.NumericUpDown();
            this.labelExposure = new System.Windows.Forms.Label();
            this.btnSave2 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownExposure)).BeginInit();
            this.SuspendLayout();
            // 
            // btnTriggerSFX
            // 
            this.btnTriggerSFX.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.btnTriggerSFX.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTriggerSFX.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnTriggerSFX.Location = new System.Drawing.Point(7, 6);
            this.btnTriggerSFX.Name = "btnTriggerSFX";
            this.btnTriggerSFX.Size = new System.Drawing.Size(270, 23);
            this.btnTriggerSFX.TabIndex = 1;
            this.btnTriggerSFX.Text = "Trigger SFX";
            this.btnTriggerSFX.UseVisualStyleBackColor = false;
            this.btnTriggerSFX.Click += new System.EventHandler(this.btnTriggerSFX_Click);
            // 
            // btnLoad1
            // 
            this.btnLoad1.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.btnLoad1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLoad1.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnLoad1.Location = new System.Drawing.Point(7, 35);
            this.btnLoad1.Name = "btnLoad1";
            this.btnLoad1.Size = new System.Drawing.Size(65, 23);
            this.btnLoad1.TabIndex = 2;
            this.btnLoad1.Text = "Load 1";
            this.btnLoad1.UseVisualStyleBackColor = false;
            this.btnLoad1.Click += new System.EventHandler(this.btnLoad1_Click);
            // 
            // btnSave1
            // 
            this.btnSave1.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.btnSave1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave1.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnSave1.Location = new System.Drawing.Point(7, 64);
            this.btnSave1.Name = "btnSave1";
            this.btnSave1.Size = new System.Drawing.Size(65, 23);
            this.btnSave1.TabIndex = 6;
            this.btnSave1.Text = "Save 1";
            this.btnSave1.UseVisualStyleBackColor = false;
            this.btnSave1.Click += new System.EventHandler(this.btnSave1_Click);
            // 
            // numericUpDownExposure
            // 
            this.numericUpDownExposure.DecimalPlaces = 3;
            this.numericUpDownExposure.Location = new System.Drawing.Point(157, 93);
            this.numericUpDownExposure.Name = "numericUpDownExposure";
            this.numericUpDownExposure.Size = new System.Drawing.Size(120, 20);
            this.numericUpDownExposure.TabIndex = 10;
            this.numericUpDownExposure.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDownExposure.ValueChanged += new System.EventHandler(this.numericUpDownExposure_ValueChanged);
            // 
            // labelExposure
            // 
            this.labelExposure.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.labelExposure.ForeColor = System.Drawing.Color.White;
            this.labelExposure.Location = new System.Drawing.Point(9, 91);
            this.labelExposure.Name = "labelExposure";
            this.labelExposure.Size = new System.Drawing.Size(128, 20);
            this.labelExposure.TabIndex = 26;
            this.labelExposure.Text = "Exposure (ms):";
            this.labelExposure.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnSave2
            // 
            this.btnSave2.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.btnSave2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave2.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnSave2.Location = new System.Drawing.Point(75, 64);
            this.btnSave2.Name = "btnSave2";
            this.btnSave2.Size = new System.Drawing.Size(65, 23);
            this.btnSave2.TabIndex = 7;
            this.btnSave2.Text = "Save 2";
            this.btnSave2.UseVisualStyleBackColor = false;
            this.btnSave2.Click += new System.EventHandler(this.btnSave2_Click);
            // 
            // TriggerImagesSFX
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.Controls.Add(this.btnSave2);
            this.Controls.Add(this.btnTriggerSFX);
            this.Controls.Add(this.btnLoad1);
            this.Controls.Add(this.btnSave1);
            this.Controls.Add(this.numericUpDownExposure);
            this.Controls.Add(this.labelExposure);
            this.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.Name = "TriggerImagesSFX";
            this.Size = new System.Drawing.Size(284, 123);
            this.Load += new System.EventHandler(this.TriggerImagesSFX_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownExposure)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnTriggerSFX;
        private System.Windows.Forms.Button btnLoad1;
        private System.Windows.Forms.Button btnSave1;
        private System.Windows.Forms.NumericUpDown numericUpDownExposure;
        private System.Windows.Forms.Label labelExposure;
        private System.Windows.Forms.Button btnSave2;
    }
}

