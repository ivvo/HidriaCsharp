namespace HRMeasView
{
    partial class FormView
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
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea2 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend2 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea3 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend3 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea4 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend4 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea5 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend5 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea6 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend6 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea7 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend7 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea8 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend8 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            this.readTimer = new System.Windows.Forms.Timer(this.components);
            this.U_12_Chart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.U_04_Chart_1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.U_04_Chart_3 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.U_04_Chart_2 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.U_02_Chart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.U_14_Chart_3 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.U_14_Chart_2 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.U_14_Chart_1 = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.ClearCharts = new System.Windows.Forms.Button();
            this.Hide_U12_Graph = new System.Windows.Forms.CheckBox();
            this.Hide_U02_Graph = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.valuesCtrl_A2 = new HRMeasView.ValuesControl();
            this.valuesCtrl_B2 = new HRMeasView.ValuesControl();
            this.valuesCtrl_A4_3 = new HRMeasView.ValuesControl();
            this.valuesCtrl_A4_2 = new HRMeasView.ValuesControl();
            this.valuesCtrl_A4_1 = new HRMeasView.ValuesControl();
            this.valuesCtrl_B4_3 = new HRMeasView.ValuesControl();
            this.valuesCtrl_B4_2 = new HRMeasView.ValuesControl();
            this.valuesCtrl_B4_1 = new HRMeasView.ValuesControl();
            ((System.ComponentModel.ISupportInitialize)(this.U_12_Chart)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.U_04_Chart_1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.U_04_Chart_3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.U_04_Chart_2)).BeginInit();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.U_02_Chart)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.U_14_Chart_3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.U_14_Chart_2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.U_14_Chart_1)).BeginInit();
            this.SuspendLayout();
            // 
            // readTimer
            // 
            this.readTimer.Interval = 1000;
            this.readTimer.Tick += new System.EventHandler(this.readTimer_Tick);
            // 
            // U_12_Chart
            // 
            chartArea1.Name = "ChartArea1";
            this.U_12_Chart.ChartAreas.Add(chartArea1);
            legend1.Name = "Legend1";
            this.U_12_Chart.Legends.Add(legend1);
            this.U_12_Chart.Location = new System.Drawing.Point(45, 36);
            this.U_12_Chart.Name = "U_12_Chart";
            this.U_12_Chart.Size = new System.Drawing.Size(650, 200);
            this.U_12_Chart.TabIndex = 27;
            this.U_12_Chart.Text = "chart4";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.valuesCtrl_B2);
            this.groupBox1.Controls.Add(this.U_12_Chart);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.groupBox1.Location = new System.Drawing.Point(98, 35);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(738, 258);
            this.groupBox1.TabIndex = 28;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Postaja B2 - pregled višine";
            // 
            // U_04_Chart_1
            // 
            chartArea2.Name = "ChartArea1";
            this.U_04_Chart_1.ChartAreas.Add(chartArea2);
            legend2.Name = "Legend1";
            this.U_04_Chart_1.Legends.Add(legend2);
            this.U_04_Chart_1.Location = new System.Drawing.Point(48, 41);
            this.U_04_Chart_1.Name = "U_04_Chart_1";
            this.U_04_Chart_1.Size = new System.Drawing.Size(650, 200);
            this.U_04_Chart_1.TabIndex = 21;
            this.U_04_Chart_1.Text = "chart1";
            // 
            // U_04_Chart_3
            // 
            chartArea3.Name = "ChartArea1";
            this.U_04_Chart_3.ChartAreas.Add(chartArea3);
            legend3.Name = "Legend1";
            this.U_04_Chart_3.Legends.Add(legend3);
            this.U_04_Chart_3.Location = new System.Drawing.Point(48, 453);
            this.U_04_Chart_3.Name = "U_04_Chart_3";
            this.U_04_Chart_3.Size = new System.Drawing.Size(650, 200);
            this.U_04_Chart_3.TabIndex = 23;
            this.U_04_Chart_3.Text = "chart3";
            // 
            // U_04_Chart_2
            // 
            chartArea4.Name = "ChartArea1";
            this.U_04_Chart_2.ChartAreas.Add(chartArea4);
            legend4.Name = "Legend1";
            this.U_04_Chart_2.Legends.Add(legend4);
            this.U_04_Chart_2.Location = new System.Drawing.Point(48, 247);
            this.U_04_Chart_2.Name = "U_04_Chart_2";
            this.U_04_Chart_2.Size = new System.Drawing.Size(650, 200);
            this.U_04_Chart_2.TabIndex = 22;
            this.U_04_Chart_2.Text = "chart2";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.valuesCtrl_A4_3);
            this.groupBox4.Controls.Add(this.valuesCtrl_A4_2);
            this.groupBox4.Controls.Add(this.valuesCtrl_A4_1);
            this.groupBox4.Controls.Add(this.U_04_Chart_2);
            this.groupBox4.Controls.Add(this.U_04_Chart_3);
            this.groupBox4.Controls.Add(this.U_04_Chart_1);
            this.groupBox4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.groupBox4.Location = new System.Drawing.Point(1052, 345);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(738, 674);
            this.groupBox4.TabIndex = 30;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Postaja A4 - pregled višine";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.valuesCtrl_A2);
            this.groupBox3.Controls.Add(this.U_02_Chart);
            this.groupBox3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.groupBox3.Location = new System.Drawing.Point(1052, 35);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(738, 258);
            this.groupBox3.TabIndex = 29;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Postaja A2 - pregled višine";
            // 
            // U_02_Chart
            // 
            chartArea5.Name = "ChartArea1";
            this.U_02_Chart.ChartAreas.Add(chartArea5);
            legend5.Name = "Legend1";
            this.U_02_Chart.Legends.Add(legend5);
            this.U_02_Chart.Location = new System.Drawing.Point(48, 36);
            this.U_02_Chart.Name = "U_02_Chart";
            this.U_02_Chart.Size = new System.Drawing.Size(650, 200);
            this.U_02_Chart.TabIndex = 24;
            this.U_02_Chart.Text = "chart1";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.valuesCtrl_B4_3);
            this.groupBox2.Controls.Add(this.valuesCtrl_B4_2);
            this.groupBox2.Controls.Add(this.valuesCtrl_B4_1);
            this.groupBox2.Controls.Add(this.U_14_Chart_3);
            this.groupBox2.Controls.Add(this.U_14_Chart_2);
            this.groupBox2.Controls.Add(this.U_14_Chart_1);
            this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.groupBox2.Location = new System.Drawing.Point(98, 345);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(738, 674);
            this.groupBox2.TabIndex = 34;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Postaja B4 - preged višine";
            // 
            // U_14_Chart_3
            // 
            chartArea6.Name = "ChartArea1";
            this.U_14_Chart_3.ChartAreas.Add(chartArea6);
            legend6.Name = "Legend1";
            this.U_14_Chart_3.Legends.Add(legend6);
            this.U_14_Chart_3.Location = new System.Drawing.Point(45, 453);
            this.U_14_Chart_3.Name = "U_14_Chart_3";
            this.U_14_Chart_3.Size = new System.Drawing.Size(650, 200);
            this.U_14_Chart_3.TabIndex = 37;
            this.U_14_Chart_3.Text = "chart6";
            // 
            // U_14_Chart_2
            // 
            chartArea7.Name = "ChartArea1";
            this.U_14_Chart_2.ChartAreas.Add(chartArea7);
            legend7.Name = "Legend1";
            this.U_14_Chart_2.Legends.Add(legend7);
            this.U_14_Chart_2.Location = new System.Drawing.Point(45, 247);
            this.U_14_Chart_2.Name = "U_14_Chart_2";
            this.U_14_Chart_2.Size = new System.Drawing.Size(650, 200);
            this.U_14_Chart_2.TabIndex = 36;
            this.U_14_Chart_2.Text = "chart6";
            // 
            // U_14_Chart_1
            // 
            chartArea8.Name = "ChartArea1";
            this.U_14_Chart_1.ChartAreas.Add(chartArea8);
            legend8.Name = "Legend1";
            this.U_14_Chart_1.Legends.Add(legend8);
            this.U_14_Chart_1.Location = new System.Drawing.Point(45, 41);
            this.U_14_Chart_1.Name = "U_14_Chart_1";
            this.U_14_Chart_1.Size = new System.Drawing.Size(650, 200);
            this.U_14_Chart_1.TabIndex = 35;
            this.U_14_Chart_1.Text = "chart6";
            // 
            // ClearCharts
            // 
            this.ClearCharts.Location = new System.Drawing.Point(842, 975);
            this.ClearCharts.Name = "ClearCharts";
            this.ClearCharts.Size = new System.Drawing.Size(204, 23);
            this.ClearCharts.TabIndex = 35;
            this.ClearCharts.Text = "Počisti grafe";
            this.ClearCharts.UseVisualStyleBackColor = true;
            this.ClearCharts.Click += new System.EventHandler(this.ClearCharts_Click);
            // 
            // Hide_U12_Graph
            // 
            this.Hide_U12_Graph.AutoSize = true;
            this.Hide_U12_Graph.Location = new System.Drawing.Point(756, 12);
            this.Hide_U12_Graph.Name = "Hide_U12_Graph";
            this.Hide_U12_Graph.Size = new System.Drawing.Size(69, 17);
            this.Hide_U12_Graph.TabIndex = 36;
            this.Hide_U12_Graph.Text = "Skrij Graf";
            this.Hide_U12_Graph.UseVisualStyleBackColor = true;
            this.Hide_U12_Graph.CheckedChanged += new System.EventHandler(this.Hide_U12_Graph_CheckedChanged);
            // 
            // Hide_U02_Graph
            // 
            this.Hide_U02_Graph.AutoSize = true;
            this.Hide_U02_Graph.Location = new System.Drawing.Point(1721, 12);
            this.Hide_U02_Graph.Name = "Hide_U02_Graph";
            this.Hide_U02_Graph.Size = new System.Drawing.Size(69, 17);
            this.Hide_U02_Graph.TabIndex = 37;
            this.Hide_U02_Graph.Text = "Skrij Graf";
            this.Hide_U02_Graph.UseVisualStyleBackColor = true;
            this.Hide_U02_Graph.CheckedChanged += new System.EventHandler(this.Hide_U02_Graph_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 72F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label1.Location = new System.Drawing.Point(855, 439);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 108);
            this.label1.TabIndex = 38;
            this.label1.Text = "1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 72F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label2.Location = new System.Drawing.Point(855, 648);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 108);
            this.label2.TabIndex = 39;
            this.label2.Text = "2";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 72F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label5.Location = new System.Drawing.Point(948, 648);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(98, 108);
            this.label5.TabIndex = 42;
            this.label5.Text = "4";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 72F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label6.Location = new System.Drawing.Point(948, 439);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(98, 108);
            this.label6.TabIndex = 41;
            this.label6.Text = "3";
            // 
            // valuesCtrl_A2
            // 
            this.valuesCtrl_A2.Location = new System.Drawing.Point(548, 80);
            this.valuesCtrl_A2.Name = "valuesCtrl_A2";
            this.valuesCtrl_A2.Size = new System.Drawing.Size(150, 125);
            this.valuesCtrl_A2.TabIndex = 25;
            // 
            // valuesCtrl_B2
            // 
            this.valuesCtrl_B2.Location = new System.Drawing.Point(545, 80);
            this.valuesCtrl_B2.Name = "valuesCtrl_B2";
            this.valuesCtrl_B2.Size = new System.Drawing.Size(150, 125);
            this.valuesCtrl_B2.TabIndex = 28;
            // 
            // valuesCtrl_A4_3
            // 
            this.valuesCtrl_A4_3.Location = new System.Drawing.Point(548, 501);
            this.valuesCtrl_A4_3.Name = "valuesCtrl_A4_3";
            this.valuesCtrl_A4_3.Size = new System.Drawing.Size(150, 125);
            this.valuesCtrl_A4_3.TabIndex = 26;
            // 
            // valuesCtrl_A4_2
            // 
            this.valuesCtrl_A4_2.Location = new System.Drawing.Point(548, 296);
            this.valuesCtrl_A4_2.Name = "valuesCtrl_A4_2";
            this.valuesCtrl_A4_2.Size = new System.Drawing.Size(150, 125);
            this.valuesCtrl_A4_2.TabIndex = 25;
            // 
            // valuesCtrl_A4_1
            // 
            this.valuesCtrl_A4_1.Location = new System.Drawing.Point(548, 90);
            this.valuesCtrl_A4_1.Name = "valuesCtrl_A4_1";
            this.valuesCtrl_A4_1.Size = new System.Drawing.Size(150, 125);
            this.valuesCtrl_A4_1.TabIndex = 24;
            // 
            // valuesCtrl_B4_3
            // 
            this.valuesCtrl_B4_3.Location = new System.Drawing.Point(545, 502);
            this.valuesCtrl_B4_3.Name = "valuesCtrl_B4_3";
            this.valuesCtrl_B4_3.Size = new System.Drawing.Size(150, 125);
            this.valuesCtrl_B4_3.TabIndex = 40;
            // 
            // valuesCtrl_B4_2
            // 
            this.valuesCtrl_B4_2.Location = new System.Drawing.Point(545, 296);
            this.valuesCtrl_B4_2.Name = "valuesCtrl_B4_2";
            this.valuesCtrl_B4_2.Size = new System.Drawing.Size(150, 125);
            this.valuesCtrl_B4_2.TabIndex = 39;
            // 
            // valuesCtrl_B4_1
            // 
            this.valuesCtrl_B4_1.Location = new System.Drawing.Point(545, 89);
            this.valuesCtrl_B4_1.Name = "valuesCtrl_B4_1";
            this.valuesCtrl_B4_1.Size = new System.Drawing.Size(150, 125);
            this.valuesCtrl_B4_1.TabIndex = 38;
            // 
            // FormView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1904, 1042);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Hide_U02_Graph);
            this.Controls.Add(this.Hide_U12_Graph);
            this.Controls.Add(this.ClearCharts);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox2);
            this.Name = "FormView";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "Prikaz meritev";
            this.Load += new System.EventHandler(this.FormView_Load);
            this.Shown += new System.EventHandler(this.FormView_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.U_12_Chart)).EndInit();
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.U_04_Chart_1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.U_04_Chart_3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.U_04_Chart_2)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.U_02_Chart)).EndInit();
            this.groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.U_14_Chart_3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.U_14_Chart_2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.U_14_Chart_1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer readTimer;
        private System.Windows.Forms.DataVisualization.Charting.Chart U_12_Chart;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.DataVisualization.Charting.Chart U_04_Chart_1;
        private System.Windows.Forms.DataVisualization.Charting.Chart U_04_Chart_3;
        private System.Windows.Forms.DataVisualization.Charting.Chart U_04_Chart_2;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.DataVisualization.Charting.Chart U_02_Chart;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.DataVisualization.Charting.Chart U_14_Chart_1;
        private System.Windows.Forms.DataVisualization.Charting.Chart U_14_Chart_3;
        private System.Windows.Forms.DataVisualization.Charting.Chart U_14_Chart_2;
        private ValuesControl valuesCtrl_B2;
        private ValuesControl valuesCtrl_A4_3;
        private ValuesControl valuesCtrl_A4_2;
        private ValuesControl valuesCtrl_A4_1;
        private ValuesControl valuesCtrl_A2;
        private ValuesControl valuesCtrl_B4_3;
        private ValuesControl valuesCtrl_B4_2;
        private ValuesControl valuesCtrl_B4_1;
        private System.Windows.Forms.Button ClearCharts;
        private System.Windows.Forms.CheckBox Hide_U12_Graph;
        private System.Windows.Forms.CheckBox Hide_U02_Graph;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
    }
}

