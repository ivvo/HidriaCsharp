namespace HidriaVisionProductivity
{
    partial class ProductivityControl
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.DataPoint dataPoint1 = new System.Windows.Forms.DataVisualization.Charting.DataPoint(0D, 33.3D);
            System.Windows.Forms.DataVisualization.Charting.DataPoint dataPoint2 = new System.Windows.Forms.DataVisualization.Charting.DataPoint(0D, 33.3D);
            System.Windows.Forms.DataVisualization.Charting.DataPoint dataPoint3 = new System.Windows.Forms.DataVisualization.Charting.DataPoint(0D, 33.4D);
            this.chartProductivity = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.labelOk = new System.Windows.Forms.Label();
            this.textBoxOkCount = new System.Windows.Forms.TextBox();
            this.textBoxOkPercent = new System.Windows.Forms.TextBox();
            this.textBoxNokPercent = new System.Windows.Forms.TextBox();
            this.textBoxNokCount = new System.Windows.Forms.TextBox();
            this.textBoxInterruptPercent = new System.Windows.Forms.TextBox();
            this.textBoxInterruptCount = new System.Windows.Forms.TextBox();
            this.labelNok = new System.Windows.Forms.Label();
            this.labelInterrupted = new System.Windows.Forms.Label();
            this.labelStartStop = new System.Windows.Forms.Label();
            this.labelReset = new System.Windows.Forms.Label();
            this.labelProductivity = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.chartProductivity)).BeginInit();
            this.SuspendLayout();
            // 
            // chartProductivity
            // 
            this.chartProductivity.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            chartArea1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            chartArea1.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            chartArea1.Name = "ChartArea1";
            this.chartProductivity.ChartAreas.Add(chartArea1);
            this.chartProductivity.Cursor = System.Windows.Forms.Cursors.Default;
            this.chartProductivity.Location = new System.Drawing.Point(3, 19);
            this.chartProductivity.Name = "chartProductivity";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series1.CustomProperties = "PieDrawingStyle=Concave, PieStartAngle=270";
            series1.Name = "Series1";
            dataPoint1.Color = System.Drawing.Color.LimeGreen;
            dataPoint1.CustomProperties = "Exploded=True";
            dataPoint2.Color = System.Drawing.Color.Firebrick;
            dataPoint3.Color = System.Drawing.SystemColors.HotTrack;
            series1.Points.Add(dataPoint1);
            series1.Points.Add(dataPoint2);
            series1.Points.Add(dataPoint3);
            this.chartProductivity.Series.Add(series1);
            this.chartProductivity.Size = new System.Drawing.Size(118, 114);
            this.chartProductivity.TabIndex = 52;
            this.chartProductivity.Text = "chart1";
            // 
            // labelOk
            // 
            this.labelOk.BackColor = System.Drawing.Color.LimeGreen;
            this.labelOk.ForeColor = System.Drawing.Color.Black;
            this.labelOk.Location = new System.Drawing.Point(127, 28);
            this.labelOk.Name = "labelOk";
            this.labelOk.Size = new System.Drawing.Size(106, 20);
            this.labelOk.TabIndex = 54;
            this.labelOk.Text = "OK";
            this.labelOk.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // textBoxOkCount
            // 
            this.textBoxOkCount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxOkCount.Location = new System.Drawing.Point(239, 28);
            this.textBoxOkCount.Name = "textBoxOkCount";
            this.textBoxOkCount.ReadOnly = true;
            this.textBoxOkCount.Size = new System.Drawing.Size(50, 20);
            this.textBoxOkCount.TabIndex = 55;
            this.textBoxOkCount.Text = "0x";
            this.textBoxOkCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBoxOkPercent
            // 
            this.textBoxOkPercent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxOkPercent.Location = new System.Drawing.Point(295, 28);
            this.textBoxOkPercent.Name = "textBoxOkPercent";
            this.textBoxOkPercent.ReadOnly = true;
            this.textBoxOkPercent.Size = new System.Drawing.Size(50, 20);
            this.textBoxOkPercent.TabIndex = 56;
            this.textBoxOkPercent.Text = "0.0%";
            this.textBoxOkPercent.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBoxNokPercent
            // 
            this.textBoxNokPercent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxNokPercent.Location = new System.Drawing.Point(295, 54);
            this.textBoxNokPercent.Name = "textBoxNokPercent";
            this.textBoxNokPercent.ReadOnly = true;
            this.textBoxNokPercent.Size = new System.Drawing.Size(50, 20);
            this.textBoxNokPercent.TabIndex = 59;
            this.textBoxNokPercent.Text = "0.0%";
            this.textBoxNokPercent.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBoxNokCount
            // 
            this.textBoxNokCount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxNokCount.Location = new System.Drawing.Point(239, 54);
            this.textBoxNokCount.Name = "textBoxNokCount";
            this.textBoxNokCount.ReadOnly = true;
            this.textBoxNokCount.Size = new System.Drawing.Size(50, 20);
            this.textBoxNokCount.TabIndex = 58;
            this.textBoxNokCount.Text = "0x";
            this.textBoxNokCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBoxInterruptPercent
            // 
            this.textBoxInterruptPercent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxInterruptPercent.Location = new System.Drawing.Point(295, 80);
            this.textBoxInterruptPercent.Name = "textBoxInterruptPercent";
            this.textBoxInterruptPercent.ReadOnly = true;
            this.textBoxInterruptPercent.Size = new System.Drawing.Size(50, 20);
            this.textBoxInterruptPercent.TabIndex = 62;
            this.textBoxInterruptPercent.Text = "0.0%";
            this.textBoxInterruptPercent.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBoxInterruptCount
            // 
            this.textBoxInterruptCount.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBoxInterruptCount.Location = new System.Drawing.Point(239, 80);
            this.textBoxInterruptCount.Name = "textBoxInterruptCount";
            this.textBoxInterruptCount.ReadOnly = true;
            this.textBoxInterruptCount.Size = new System.Drawing.Size(50, 20);
            this.textBoxInterruptCount.TabIndex = 61;
            this.textBoxInterruptCount.Text = "0x";
            this.textBoxInterruptCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // labelNok
            // 
            this.labelNok.BackColor = System.Drawing.Color.Firebrick;
            this.labelNok.ForeColor = System.Drawing.Color.Black;
            this.labelNok.Location = new System.Drawing.Point(127, 54);
            this.labelNok.Name = "labelNok";
            this.labelNok.Size = new System.Drawing.Size(106, 20);
            this.labelNok.TabIndex = 63;
            this.labelNok.Text = "NOK";
            this.labelNok.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelInterrupted
            // 
            this.labelInterrupted.BackColor = System.Drawing.SystemColors.HotTrack;
            this.labelInterrupted.ForeColor = System.Drawing.Color.Black;
            this.labelInterrupted.Location = new System.Drawing.Point(127, 80);
            this.labelInterrupted.Name = "labelInterrupted";
            this.labelInterrupted.Size = new System.Drawing.Size(106, 20);
            this.labelInterrupted.TabIndex = 64;
            this.labelInterrupted.Text = "INTERRUPTED";
            this.labelInterrupted.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelStartStop
            // 
            this.labelStartStop.BackColor = System.Drawing.SystemColors.ControlDark;
            this.labelStartStop.ForeColor = System.Drawing.Color.Black;
            this.labelStartStop.Location = new System.Drawing.Point(127, 106);
            this.labelStartStop.Name = "labelStartStop";
            this.labelStartStop.Size = new System.Drawing.Size(106, 20);
            this.labelStartStop.TabIndex = 65;
            this.labelStartStop.Text = "START";
            this.labelStartStop.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelStartStop.Click += new System.EventHandler(this.labelStartStop_Click);
            // 
            // labelReset
            // 
            this.labelReset.BackColor = System.Drawing.SystemColors.ControlDark;
            this.labelReset.ForeColor = System.Drawing.Color.Black;
            this.labelReset.Location = new System.Drawing.Point(239, 106);
            this.labelReset.Name = "labelReset";
            this.labelReset.Size = new System.Drawing.Size(106, 20);
            this.labelReset.TabIndex = 66;
            this.labelReset.Text = "RESET";
            this.labelReset.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelReset.Click += new System.EventHandler(this.labelReset_Click);
            // 
            // labelProductivity
            // 
            this.labelProductivity.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.labelProductivity.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.labelProductivity.Location = new System.Drawing.Point(7, 5);
            this.labelProductivity.Name = "labelProductivity";
            this.labelProductivity.Size = new System.Drawing.Size(340, 20);
            this.labelProductivity.TabIndex = 51;
            this.labelProductivity.Text = "Productivity";
            this.labelProductivity.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ProductivityControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.Controls.Add(this.labelReset);
            this.Controls.Add(this.labelStartStop);
            this.Controls.Add(this.labelInterrupted);
            this.Controls.Add(this.labelNok);
            this.Controls.Add(this.textBoxInterruptPercent);
            this.Controls.Add(this.textBoxInterruptCount);
            this.Controls.Add(this.textBoxNokPercent);
            this.Controls.Add(this.textBoxNokCount);
            this.Controls.Add(this.textBoxOkPercent);
            this.Controls.Add(this.textBoxOkCount);
            this.Controls.Add(this.labelOk);
            this.Controls.Add(this.chartProductivity);
            this.Controls.Add(this.labelProductivity);
            this.Name = "ProductivityControl";
            this.Size = new System.Drawing.Size(352, 133);
            ((System.ComponentModel.ISupportInitialize)(this.chartProductivity)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.DataVisualization.Charting.Chart chartProductivity;
        private System.Windows.Forms.Label labelOk;
        private System.Windows.Forms.TextBox textBoxOkCount;
        private System.Windows.Forms.TextBox textBoxOkPercent;
        private System.Windows.Forms.TextBox textBoxNokPercent;
        private System.Windows.Forms.TextBox textBoxNokCount;
        private System.Windows.Forms.TextBox textBoxInterruptPercent;
        private System.Windows.Forms.TextBox textBoxInterruptCount;
        private System.Windows.Forms.Label labelNok;
        private System.Windows.Forms.Label labelInterrupted;
        private System.Windows.Forms.Label labelStartStop;
        private System.Windows.Forms.Label labelReset;
        private System.Windows.Forms.Label labelProductivity;
    }
}
