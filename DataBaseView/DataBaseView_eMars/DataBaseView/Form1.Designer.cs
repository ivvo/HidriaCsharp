namespace DataBaseView
{
	partial class Form1
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
            this.dataBaseGridView = new System.Windows.Forms.DataGridView();
            this.comboBoxTable = new System.Windows.Forms.ComboBox();
            this.lebaelImeTabele = new System.Windows.Forms.Label();
            this.labelTabela = new System.Windows.Forms.Label();
            this.dateTimeOD = new System.Windows.Forms.DateTimePicker();
            this.labelDatumOD = new System.Windows.Forms.Label();
            this.dateTimeDO = new System.Windows.Forms.DateTimePicker();
            this.labelDO = new System.Windows.Forms.Label();
            this.buttonIzvoziCSV = new System.Windows.Forms.Button();
            this.buttonISCI = new System.Windows.Forms.Button();
            this.saveFileDialogCSV = new System.Windows.Forms.SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.dataBaseGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // dataBaseGridView
            // 
            this.dataBaseGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataBaseGridView.Location = new System.Drawing.Point(12, 216);
            this.dataBaseGridView.Name = "dataBaseGridView";
            this.dataBaseGridView.Size = new System.Drawing.Size(1560, 533);
            this.dataBaseGridView.TabIndex = 0;
            // 
            // comboBoxTable
            // 
            this.comboBoxTable.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBoxTable.FormattingEnabled = true;
            this.comboBoxTable.Items.AddRange(new object[] {
            "Alarmi",
            "Etalonization",
            "Kalibration",
            "Measure_A02",
            "Measure_A22",
            "Recept"});
            this.comboBoxTable.Location = new System.Drawing.Point(12, 30);
            this.comboBoxTable.Name = "comboBoxTable";
            this.comboBoxTable.Size = new System.Drawing.Size(300, 33);
            this.comboBoxTable.TabIndex = 2;
            this.comboBoxTable.SelectedIndexChanged += new System.EventHandler(this.comboBoxTable_SelectedIndexChanged);
            // 
            // lebaelImeTabele
            // 
            this.lebaelImeTabele.AutoSize = true;
            this.lebaelImeTabele.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lebaelImeTabele.Location = new System.Drawing.Point(9, 9);
            this.lebaelImeTabele.Name = "lebaelImeTabele";
            this.lebaelImeTabele.Size = new System.Drawing.Size(87, 18);
            this.lebaelImeTabele.TabIndex = 3;
            this.lebaelImeTabele.Text = "Izberi tabelo";
            // 
            // labelTabela
            // 
            this.labelTabela.AutoSize = true;
            this.labelTabela.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTabela.Location = new System.Drawing.Point(12, 195);
            this.labelTabela.Name = "labelTabela";
            this.labelTabela.Size = new System.Drawing.Size(52, 18);
            this.labelTabela.TabIndex = 4;
            this.labelTabela.Text = "Tabela";
            // 
            // dateTimeOD
            // 
            this.dateTimeOD.CalendarFont = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dateTimeOD.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dateTimeOD.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimeOD.Location = new System.Drawing.Point(12, 87);
            this.dateTimeOD.Name = "dateTimeOD";
            this.dateTimeOD.Size = new System.Drawing.Size(300, 30);
            this.dateTimeOD.TabIndex = 5;
            // 
            // labelDatumOD
            // 
            this.labelDatumOD.AutoSize = true;
            this.labelDatumOD.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDatumOD.Location = new System.Drawing.Point(9, 66);
            this.labelDatumOD.Name = "labelDatumOD";
            this.labelDatumOD.Size = new System.Drawing.Size(79, 18);
            this.labelDatumOD.TabIndex = 6;
            this.labelDatumOD.Text = "Datum OD";
            // 
            // dateTimeDO
            // 
            this.dateTimeDO.CalendarFont = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dateTimeDO.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dateTimeDO.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimeDO.Location = new System.Drawing.Point(12, 141);
            this.dateTimeDO.Name = "dateTimeDO";
            this.dateTimeDO.Size = new System.Drawing.Size(300, 30);
            this.dateTimeDO.TabIndex = 7;
            // 
            // labelDO
            // 
            this.labelDO.AutoSize = true;
            this.labelDO.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelDO.Location = new System.Drawing.Point(9, 120);
            this.labelDO.Name = "labelDO";
            this.labelDO.Size = new System.Drawing.Size(79, 18);
            this.labelDO.TabIndex = 8;
            this.labelDO.Text = "Datum DO";
            // 
            // buttonIzvoziCSV
            // 
            this.buttonIzvoziCSV.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonIzvoziCSV.Location = new System.Drawing.Point(1372, 35);
            this.buttonIzvoziCSV.Name = "buttonIzvoziCSV";
            this.buttonIzvoziCSV.Size = new System.Drawing.Size(200, 141);
            this.buttonIzvoziCSV.TabIndex = 9;
            this.buttonIzvoziCSV.Text = "Izvozi v CSV";
            this.buttonIzvoziCSV.UseVisualStyleBackColor = true;
            this.buttonIzvoziCSV.Click += new System.EventHandler(this.buttonIzvoziCSV_Click);
            // 
            // buttonISCI
            // 
            this.buttonISCI.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonISCI.Location = new System.Drawing.Point(335, 30);
            this.buttonISCI.Name = "buttonISCI";
            this.buttonISCI.Size = new System.Drawing.Size(200, 141);
            this.buttonISCI.TabIndex = 10;
            this.buttonISCI.Text = "Išči";
            this.buttonISCI.UseVisualStyleBackColor = true;
            this.buttonISCI.Click += new System.EventHandler(this.buttonISCI_Click);
            // 
            // saveFileDialogCSV
            // 
            this.saveFileDialogCSV.Filter = "Rich Text Format|*.csv";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1584, 761);
            this.Controls.Add(this.buttonISCI);
            this.Controls.Add(this.buttonIzvoziCSV);
            this.Controls.Add(this.labelDO);
            this.Controls.Add(this.dateTimeDO);
            this.Controls.Add(this.labelDatumOD);
            this.Controls.Add(this.dateTimeOD);
            this.Controls.Add(this.labelTabela);
            this.Controls.Add(this.lebaelImeTabele);
            this.Controls.Add(this.comboBoxTable);
            this.Controls.Add(this.dataBaseGridView);
            this.Name = "Form1";
            this.Text = "DataBaseView";
            ((System.ComponentModel.ISupportInitialize)(this.dataBaseGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.DataGridView dataBaseGridView;
		private System.Windows.Forms.ComboBox comboBoxTable;
		private System.Windows.Forms.Label lebaelImeTabele;
		private System.Windows.Forms.Label labelTabela;
		private System.Windows.Forms.DateTimePicker dateTimeOD;
		private System.Windows.Forms.Label labelDatumOD;
		private System.Windows.Forms.DateTimePicker dateTimeDO;
		private System.Windows.Forms.Label labelDO;
		private System.Windows.Forms.Button buttonIzvoziCSV;
		private System.Windows.Forms.Button buttonISCI;
		private System.Windows.Forms.SaveFileDialog saveFileDialogCSV;
	}
}

