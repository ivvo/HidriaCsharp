namespace HidriaVision
{
    partial class Form_03_ResultsControl
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
            this.labelOrientation = new System.Windows.Forms.Label();
            this.textBoxMeasurement = new System.Windows.Forms.TextBox();
            this.labelOrintationText = new System.Windows.Forms.Label();
            this.textBoxOrientation = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // labelOrientation
            // 
            this.labelOrientation.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.labelOrientation.ForeColor = System.Drawing.SystemColors.ActiveBorder;
            this.labelOrientation.Location = new System.Drawing.Point(16, 69);
            this.labelOrientation.Name = "labelOrientation";
            this.labelOrientation.Size = new System.Drawing.Size(96, 20);
            this.labelOrientation.TabIndex = 78;
            this.labelOrientation.Text = "Fount at:";
            this.labelOrientation.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxMeasurement
            // 
            this.textBoxMeasurement.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxMeasurement.Location = new System.Drawing.Point(118, 69);
            this.textBoxMeasurement.Margin = new System.Windows.Forms.Padding(3, 20, 3, 20);
            this.textBoxMeasurement.MaximumSize = new System.Drawing.Size(227, 20);
            this.textBoxMeasurement.Name = "textBoxMeasurement";
            this.textBoxMeasurement.ReadOnly = true;
            this.textBoxMeasurement.Size = new System.Drawing.Size(220, 20);
            this.textBoxMeasurement.TabIndex = 77;
            this.textBoxMeasurement.Text = "0.00 °";
            this.textBoxMeasurement.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // labelOrintationText
            // 
            this.labelOrintationText.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.labelOrintationText.ForeColor = System.Drawing.SystemColors.ActiveBorder;
            this.labelOrintationText.Location = new System.Drawing.Point(16, 99);
            this.labelOrintationText.Name = "labelOrintationText";
            this.labelOrintationText.Size = new System.Drawing.Size(96, 20);
            this.labelOrintationText.TabIndex = 107;
            this.labelOrintationText.Text = "Rotate:";
            this.labelOrintationText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBoxOrientation
            // 
            this.textBoxOrientation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxOrientation.Location = new System.Drawing.Point(118, 99);
            this.textBoxOrientation.Margin = new System.Windows.Forms.Padding(3, 20, 3, 20);
            this.textBoxOrientation.MaximumSize = new System.Drawing.Size(227, 20);
            this.textBoxOrientation.Name = "textBoxOrientation";
            this.textBoxOrientation.ReadOnly = true;
            this.textBoxOrientation.Size = new System.Drawing.Size(220, 20);
            this.textBoxOrientation.TabIndex = 108;
            this.textBoxOrientation.Text = "OK";
            this.textBoxOrientation.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // Form_03_ResultsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.Controls.Add(this.textBoxOrientation);
            this.Controls.Add(this.labelOrintationText);
            this.Controls.Add(this.labelOrientation);
            this.Controls.Add(this.textBoxMeasurement);
            this.Margin = new System.Windows.Forms.Padding(3, 20, 3, 20);
            this.Name = "Form_03_ResultsControl";
            this.Size = new System.Drawing.Size(352, 150);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label labelOrientation;
        private System.Windows.Forms.TextBox textBoxMeasurement;
        private System.Windows.Forms.Label labelOrintationText;
        private System.Windows.Forms.TextBox textBoxOrientation;
    }
}
