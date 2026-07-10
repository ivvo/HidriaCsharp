namespace ITW_Database_Viewer
{
    partial class FormMain
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.panelMain = new System.Windows.Forms.Panel();
            this.panelControls = new System.Windows.Forms.Panel();
            this.panelLanguage = new System.Windows.Forms.Panel();
            this.radioButtonLanguageSlovenian = new System.Windows.Forms.RadioButton();
            this.radioButtonLanguageEnglish = new System.Windows.Forms.RadioButton();
            this.labelLanguageSelect = new System.Windows.Forms.Label();
            this.panelExportResults = new System.Windows.Forms.Panel();
            this.buttonExportXlsx = new System.Windows.Forms.Button();
            this.buttonExportCsv = new System.Windows.Forms.Button();
            this.labelExportResults = new System.Windows.Forms.Label();
            this.panelResultsSelection = new System.Windows.Forms.Panel();
            this.buttonGetResults = new System.Windows.Forms.Button();
            this.groupBoxResultsSelection = new System.Windows.Forms.GroupBox();
            this.checkBoxParameters = new System.Windows.Forms.CheckBox();
            this.checkBoxArea7 = new System.Windows.Forms.CheckBox();
            this.checkBoxArea6 = new System.Windows.Forms.CheckBox();
            this.checkBoxArea5 = new System.Windows.Forms.CheckBox();
            this.checkBoxArea4 = new System.Windows.Forms.CheckBox();
            this.checkBoxArea3 = new System.Windows.Forms.CheckBox();
            this.checkBoxArea2 = new System.Windows.Forms.CheckBox();
            this.checkBoxArea1 = new System.Windows.Forms.CheckBox();
            this.checkBoxMeasurement10 = new System.Windows.Forms.CheckBox();
            this.checkBoxMeasurement8 = new System.Windows.Forms.CheckBox();
            this.checkBoxMeasurement6 = new System.Windows.Forms.CheckBox();
            this.checkBoxMeasurement4 = new System.Windows.Forms.CheckBox();
            this.checkBoxMeasurement2 = new System.Windows.Forms.CheckBox();
            this.groupBoxStationSelection = new System.Windows.Forms.GroupBox();
            this.radioButtonStation02 = new System.Windows.Forms.RadioButton();
            this.radioButtonStation01 = new System.Windows.Forms.RadioButton();
            this.groupBoxResultStatusSelect = new System.Windows.Forms.GroupBox();
            this.radioButtonAll = new System.Windows.Forms.RadioButton();
            this.radioButtonNok = new System.Windows.Forms.RadioButton();
            this.radioButtonOk = new System.Windows.Forms.RadioButton();
            this.labelResultsSelection = new System.Windows.Forms.Label();
            this.panelDates = new System.Windows.Forms.Panel();
            this.labelDateSelection = new System.Windows.Forms.Label();
            this.dateTimePickerTo = new System.Windows.Forms.DateTimePicker();
            this.labelDateTo = new System.Windows.Forms.Label();
            this.dateTimePickerFrom = new System.Windows.Forms.DateTimePicker();
            this.labelDateFrom = new System.Windows.Forms.Label();
            this.panelGridView = new System.Windows.Forms.Panel();
            this.checkBoxMeasurement1 = new System.Windows.Forms.CheckBox();
            this.checkBoxMeasurement3 = new System.Windows.Forms.CheckBox();
            this.checkBoxMeasurement5 = new System.Windows.Forms.CheckBox();
            this.checkBoxMeasurement7 = new System.Windows.Forms.CheckBox();
            this.checkBoxMeasurement9 = new System.Windows.Forms.CheckBox();
            this.checkBoxMeasurement11 = new System.Windows.Forms.CheckBox();
            this.dataGridViewSql = new System.Windows.Forms.DataGridView();
            this.panelExtendedOption = new System.Windows.Forms.Panel();
            this.panelMain.SuspendLayout();
            this.panelControls.SuspendLayout();
            this.panelLanguage.SuspendLayout();
            this.panelExportResults.SuspendLayout();
            this.panelResultsSelection.SuspendLayout();
            this.groupBoxResultsSelection.SuspendLayout();
            this.groupBoxStationSelection.SuspendLayout();
            this.groupBoxResultStatusSelect.SuspendLayout();
            this.panelDates.SuspendLayout();
            this.panelGridView.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewSql)).BeginInit();
            this.panelExtendedOption.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelMain
            // 
            resources.ApplyResources(this.panelMain, "panelMain");
            this.panelMain.Controls.Add(this.panelControls);
            this.panelMain.Controls.Add(this.panelGridView);
            this.panelMain.Name = "panelMain";
            // 
            // panelControls
            // 
            resources.ApplyResources(this.panelControls, "panelControls");
            this.panelControls.Controls.Add(this.panelLanguage);
            this.panelControls.Controls.Add(this.panelExportResults);
            this.panelControls.Controls.Add(this.panelResultsSelection);
            this.panelControls.Controls.Add(this.panelDates);
            this.panelControls.Name = "panelControls";
            // 
            // panelLanguage
            // 
            resources.ApplyResources(this.panelLanguage, "panelLanguage");
            this.panelLanguage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelLanguage.Controls.Add(this.radioButtonLanguageSlovenian);
            this.panelLanguage.Controls.Add(this.radioButtonLanguageEnglish);
            this.panelLanguage.Controls.Add(this.labelLanguageSelect);
            this.panelLanguage.Name = "panelLanguage";
            // 
            // radioButtonLanguageSlovenian
            // 
            resources.ApplyResources(this.radioButtonLanguageSlovenian, "radioButtonLanguageSlovenian");
            this.radioButtonLanguageSlovenian.Name = "radioButtonLanguageSlovenian";
            this.radioButtonLanguageSlovenian.UseVisualStyleBackColor = true;
            this.radioButtonLanguageSlovenian.CheckedChanged += new System.EventHandler(this.radioButtonLanguage_CheckedChanged);
            // 
            // radioButtonLanguageEnglish
            // 
            resources.ApplyResources(this.radioButtonLanguageEnglish, "radioButtonLanguageEnglish");
            this.radioButtonLanguageEnglish.Checked = true;
            this.radioButtonLanguageEnglish.Name = "radioButtonLanguageEnglish";
            this.radioButtonLanguageEnglish.TabStop = true;
            this.radioButtonLanguageEnglish.UseVisualStyleBackColor = true;
            this.radioButtonLanguageEnglish.CheckedChanged += new System.EventHandler(this.radioButtonLanguage_CheckedChanged);
            // 
            // labelLanguageSelect
            // 
            resources.ApplyResources(this.labelLanguageSelect, "labelLanguageSelect");
            this.labelLanguageSelect.Name = "labelLanguageSelect";
            // 
            // panelExportResults
            // 
            resources.ApplyResources(this.panelExportResults, "panelExportResults");
            this.panelExportResults.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelExportResults.Controls.Add(this.buttonExportXlsx);
            this.panelExportResults.Controls.Add(this.buttonExportCsv);
            this.panelExportResults.Controls.Add(this.labelExportResults);
            this.panelExportResults.Name = "panelExportResults";
            // 
            // buttonExportXlsx
            // 
            resources.ApplyResources(this.buttonExportXlsx, "buttonExportXlsx");
            this.buttonExportXlsx.Name = "buttonExportXlsx";
            this.buttonExportXlsx.UseVisualStyleBackColor = true;
            this.buttonExportXlsx.Click += new System.EventHandler(this.buttonExport_Click);
            // 
            // buttonExportCsv
            // 
            resources.ApplyResources(this.buttonExportCsv, "buttonExportCsv");
            this.buttonExportCsv.Name = "buttonExportCsv";
            this.buttonExportCsv.UseVisualStyleBackColor = true;
            this.buttonExportCsv.Click += new System.EventHandler(this.buttonExport_Click);
            // 
            // labelExportResults
            // 
            resources.ApplyResources(this.labelExportResults, "labelExportResults");
            this.labelExportResults.Name = "labelExportResults";
            // 
            // panelResultsSelection
            // 
            resources.ApplyResources(this.panelResultsSelection, "panelResultsSelection");
            this.panelResultsSelection.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelResultsSelection.Controls.Add(this.buttonGetResults);
            this.panelResultsSelection.Controls.Add(this.groupBoxResultsSelection);
            this.panelResultsSelection.Controls.Add(this.groupBoxStationSelection);
            this.panelResultsSelection.Controls.Add(this.groupBoxResultStatusSelect);
            this.panelResultsSelection.Controls.Add(this.labelResultsSelection);
            this.panelResultsSelection.Name = "panelResultsSelection";
            // 
            // buttonGetResults
            // 
            resources.ApplyResources(this.buttonGetResults, "buttonGetResults");
            this.buttonGetResults.Name = "buttonGetResults";
            this.buttonGetResults.UseVisualStyleBackColor = true;
            this.buttonGetResults.Click += new System.EventHandler(this.buttonGetResults_Click);
            // 
            // groupBoxResultsSelection
            // 
            this.groupBoxResultsSelection.Controls.Add(this.checkBoxMeasurement1);
            this.groupBoxResultsSelection.Controls.Add(this.checkBoxParameters);
            this.groupBoxResultsSelection.Controls.Add(this.checkBoxArea7);
            this.groupBoxResultsSelection.Controls.Add(this.checkBoxArea6);
            this.groupBoxResultsSelection.Controls.Add(this.checkBoxArea5);
            this.groupBoxResultsSelection.Controls.Add(this.checkBoxArea4);
            this.groupBoxResultsSelection.Controls.Add(this.checkBoxArea3);
            this.groupBoxResultsSelection.Controls.Add(this.checkBoxArea2);
            this.groupBoxResultsSelection.Controls.Add(this.checkBoxArea1);
            this.groupBoxResultsSelection.Controls.Add(this.panelExtendedOption);
            resources.ApplyResources(this.groupBoxResultsSelection, "groupBoxResultsSelection");
            this.groupBoxResultsSelection.Name = "groupBoxResultsSelection";
            this.groupBoxResultsSelection.TabStop = false;
            // 
            // checkBoxParameters
            // 
            resources.ApplyResources(this.checkBoxParameters, "checkBoxParameters");
            this.checkBoxParameters.Name = "checkBoxParameters";
            this.checkBoxParameters.UseVisualStyleBackColor = true;
            // 
            // checkBoxArea7
            // 
            resources.ApplyResources(this.checkBoxArea7, "checkBoxArea7");
            this.checkBoxArea7.Name = "checkBoxArea7";
            this.checkBoxArea7.UseVisualStyleBackColor = true;
            // 
            // checkBoxArea6
            // 
            resources.ApplyResources(this.checkBoxArea6, "checkBoxArea6");
            this.checkBoxArea6.Name = "checkBoxArea6";
            this.checkBoxArea6.UseVisualStyleBackColor = true;
            // 
            // checkBoxArea5
            // 
            resources.ApplyResources(this.checkBoxArea5, "checkBoxArea5");
            this.checkBoxArea5.Name = "checkBoxArea5";
            this.checkBoxArea5.UseVisualStyleBackColor = true;
            // 
            // checkBoxArea4
            // 
            resources.ApplyResources(this.checkBoxArea4, "checkBoxArea4");
            this.checkBoxArea4.Name = "checkBoxArea4";
            this.checkBoxArea4.UseVisualStyleBackColor = true;
            // 
            // checkBoxArea3
            // 
            resources.ApplyResources(this.checkBoxArea3, "checkBoxArea3");
            this.checkBoxArea3.Name = "checkBoxArea3";
            this.checkBoxArea3.UseVisualStyleBackColor = true;
            // 
            // checkBoxArea2
            // 
            resources.ApplyResources(this.checkBoxArea2, "checkBoxArea2");
            this.checkBoxArea2.Name = "checkBoxArea2";
            this.checkBoxArea2.UseVisualStyleBackColor = true;
            // 
            // checkBoxArea1
            // 
            resources.ApplyResources(this.checkBoxArea1, "checkBoxArea1");
            this.checkBoxArea1.Name = "checkBoxArea1";
            this.checkBoxArea1.UseVisualStyleBackColor = true;
            // 
            // checkBoxMeasurement10
            // 
            resources.ApplyResources(this.checkBoxMeasurement10, "checkBoxMeasurement10");
            this.checkBoxMeasurement10.Name = "checkBoxMeasurement10";
            this.checkBoxMeasurement10.UseVisualStyleBackColor = true;
            // 
            // checkBoxMeasurement8
            // 
            resources.ApplyResources(this.checkBoxMeasurement8, "checkBoxMeasurement8");
            this.checkBoxMeasurement8.Name = "checkBoxMeasurement8";
            this.checkBoxMeasurement8.UseVisualStyleBackColor = true;
            // 
            // checkBoxMeasurement6
            // 
            resources.ApplyResources(this.checkBoxMeasurement6, "checkBoxMeasurement6");
            this.checkBoxMeasurement6.Name = "checkBoxMeasurement6";
            this.checkBoxMeasurement6.UseVisualStyleBackColor = true;
            // 
            // checkBoxMeasurement4
            // 
            resources.ApplyResources(this.checkBoxMeasurement4, "checkBoxMeasurement4");
            this.checkBoxMeasurement4.Name = "checkBoxMeasurement4";
            this.checkBoxMeasurement4.UseVisualStyleBackColor = true;
            // 
            // checkBoxMeasurement2
            // 
            resources.ApplyResources(this.checkBoxMeasurement2, "checkBoxMeasurement2");
            this.checkBoxMeasurement2.Name = "checkBoxMeasurement2";
            this.checkBoxMeasurement2.UseVisualStyleBackColor = true;
            // 
            // groupBoxStationSelection
            // 
            this.groupBoxStationSelection.Controls.Add(this.radioButtonStation02);
            this.groupBoxStationSelection.Controls.Add(this.radioButtonStation01);
            resources.ApplyResources(this.groupBoxStationSelection, "groupBoxStationSelection");
            this.groupBoxStationSelection.Name = "groupBoxStationSelection";
            this.groupBoxStationSelection.TabStop = false;
            // 
            // radioButtonStation02
            // 
            resources.ApplyResources(this.radioButtonStation02, "radioButtonStation02");
            this.radioButtonStation02.Name = "radioButtonStation02";
            this.radioButtonStation02.UseVisualStyleBackColor = true;
            this.radioButtonStation02.CheckedChanged += new System.EventHandler(this.radioButtonStation_CheckedChanged);
            // 
            // radioButtonStation01
            // 
            resources.ApplyResources(this.radioButtonStation01, "radioButtonStation01");
            this.radioButtonStation01.Checked = true;
            this.radioButtonStation01.Name = "radioButtonStation01";
            this.radioButtonStation01.TabStop = true;
            this.radioButtonStation01.UseVisualStyleBackColor = true;
            this.radioButtonStation01.CheckedChanged += new System.EventHandler(this.radioButtonStation_CheckedChanged);
            // 
            // groupBoxResultStatusSelect
            // 
            this.groupBoxResultStatusSelect.Controls.Add(this.radioButtonAll);
            this.groupBoxResultStatusSelect.Controls.Add(this.radioButtonNok);
            this.groupBoxResultStatusSelect.Controls.Add(this.radioButtonOk);
            resources.ApplyResources(this.groupBoxResultStatusSelect, "groupBoxResultStatusSelect");
            this.groupBoxResultStatusSelect.Name = "groupBoxResultStatusSelect";
            this.groupBoxResultStatusSelect.TabStop = false;
            // 
            // radioButtonAll
            // 
            resources.ApplyResources(this.radioButtonAll, "radioButtonAll");
            this.radioButtonAll.Checked = true;
            this.radioButtonAll.Name = "radioButtonAll";
            this.radioButtonAll.TabStop = true;
            this.radioButtonAll.UseVisualStyleBackColor = true;
            // 
            // radioButtonNok
            // 
            resources.ApplyResources(this.radioButtonNok, "radioButtonNok");
            this.radioButtonNok.Name = "radioButtonNok";
            this.radioButtonNok.UseVisualStyleBackColor = true;
            // 
            // radioButtonOk
            // 
            resources.ApplyResources(this.radioButtonOk, "radioButtonOk");
            this.radioButtonOk.Name = "radioButtonOk";
            this.radioButtonOk.UseVisualStyleBackColor = true;
            // 
            // labelResultsSelection
            // 
            resources.ApplyResources(this.labelResultsSelection, "labelResultsSelection");
            this.labelResultsSelection.Name = "labelResultsSelection";
            // 
            // panelDates
            // 
            resources.ApplyResources(this.panelDates, "panelDates");
            this.panelDates.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelDates.Controls.Add(this.labelDateSelection);
            this.panelDates.Controls.Add(this.dateTimePickerTo);
            this.panelDates.Controls.Add(this.labelDateTo);
            this.panelDates.Controls.Add(this.dateTimePickerFrom);
            this.panelDates.Controls.Add(this.labelDateFrom);
            this.panelDates.Name = "panelDates";
            // 
            // labelDateSelection
            // 
            resources.ApplyResources(this.labelDateSelection, "labelDateSelection");
            this.labelDateSelection.Name = "labelDateSelection";
            // 
            // dateTimePickerTo
            // 
            resources.ApplyResources(this.dateTimePickerTo, "dateTimePickerTo");
            this.dateTimePickerTo.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerTo.MaxDate = new System.DateTime(2100, 12, 31, 0, 0, 0, 0);
            this.dateTimePickerTo.MinDate = new System.DateTime(2000, 1, 1, 0, 0, 0, 0);
            this.dateTimePickerTo.Name = "dateTimePickerTo";
            // 
            // labelDateTo
            // 
            resources.ApplyResources(this.labelDateTo, "labelDateTo");
            this.labelDateTo.Name = "labelDateTo";
            // 
            // dateTimePickerFrom
            // 
            resources.ApplyResources(this.dateTimePickerFrom, "dateTimePickerFrom");
            this.dateTimePickerFrom.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
            this.dateTimePickerFrom.MaxDate = new System.DateTime(2100, 12, 31, 0, 0, 0, 0);
            this.dateTimePickerFrom.MinDate = new System.DateTime(2000, 1, 1, 0, 0, 0, 0);
            this.dateTimePickerFrom.Name = "dateTimePickerFrom";
            // 
            // labelDateFrom
            // 
            resources.ApplyResources(this.labelDateFrom, "labelDateFrom");
            this.labelDateFrom.Name = "labelDateFrom";
            // 
            // panelGridView
            // 
            resources.ApplyResources(this.panelGridView, "panelGridView");
            this.panelGridView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelGridView.Controls.Add(this.dataGridViewSql);
            this.panelGridView.Name = "panelGridView";
            // 
            // checkBoxMeasurement1
            // 
            resources.ApplyResources(this.checkBoxMeasurement1, "checkBoxMeasurement1");
            this.checkBoxMeasurement1.Name = "checkBoxMeasurement1";
            this.checkBoxMeasurement1.UseVisualStyleBackColor = true;
            // 
            // checkBoxMeasurement3
            // 
            resources.ApplyResources(this.checkBoxMeasurement3, "checkBoxMeasurement3");
            this.checkBoxMeasurement3.Name = "checkBoxMeasurement3";
            this.checkBoxMeasurement3.UseVisualStyleBackColor = true;
            // 
            // checkBoxMeasurement5
            // 
            resources.ApplyResources(this.checkBoxMeasurement5, "checkBoxMeasurement5");
            this.checkBoxMeasurement5.Name = "checkBoxMeasurement5";
            this.checkBoxMeasurement5.UseVisualStyleBackColor = true;
            // 
            // checkBoxMeasurement7
            // 
            resources.ApplyResources(this.checkBoxMeasurement7, "checkBoxMeasurement7");
            this.checkBoxMeasurement7.Name = "checkBoxMeasurement7";
            this.checkBoxMeasurement7.UseVisualStyleBackColor = true;
            // 
            // checkBoxMeasurement9
            // 
            resources.ApplyResources(this.checkBoxMeasurement9, "checkBoxMeasurement9");
            this.checkBoxMeasurement9.Name = "checkBoxMeasurement9";
            this.checkBoxMeasurement9.UseVisualStyleBackColor = true;
            // 
            // checkBoxMeasurement11
            // 
            resources.ApplyResources(this.checkBoxMeasurement11, "checkBoxMeasurement11");
            this.checkBoxMeasurement11.Name = "checkBoxMeasurement11";
            this.checkBoxMeasurement11.UseVisualStyleBackColor = true;
            // 
            // dataGridViewSql
            // 
            this.dataGridViewSql.AllowUserToAddRows = false;
            this.dataGridViewSql.AllowUserToDeleteRows = false;
            this.dataGridViewSql.AllowUserToResizeColumns = false;
            this.dataGridViewSql.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            resources.ApplyResources(this.dataGridViewSql, "dataGridViewSql");
            this.dataGridViewSql.MultiSelect = false;
            this.dataGridViewSql.Name = "dataGridViewSql";
            this.dataGridViewSql.ReadOnly = true;
            this.dataGridViewSql.RowHeadersVisible = false;
            this.dataGridViewSql.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            // 
            // panelExtendedOption
            // 
            this.panelExtendedOption.Controls.Add(this.checkBoxMeasurement11);
            this.panelExtendedOption.Controls.Add(this.checkBoxMeasurement9);
            this.panelExtendedOption.Controls.Add(this.checkBoxMeasurement2);
            this.panelExtendedOption.Controls.Add(this.checkBoxMeasurement7);
            this.panelExtendedOption.Controls.Add(this.checkBoxMeasurement4);
            this.panelExtendedOption.Controls.Add(this.checkBoxMeasurement5);
            this.panelExtendedOption.Controls.Add(this.checkBoxMeasurement6);
            this.panelExtendedOption.Controls.Add(this.checkBoxMeasurement3);
            this.panelExtendedOption.Controls.Add(this.checkBoxMeasurement8);
            this.panelExtendedOption.Controls.Add(this.checkBoxMeasurement10);
            resources.ApplyResources(this.panelExtendedOption, "panelExtendedOption");
            this.panelExtendedOption.Name = "panelExtendedOption";
            // 
            // FormMain
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panelMain);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "FormMain";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.panelMain.ResumeLayout(false);
            this.panelControls.ResumeLayout(false);
            this.panelLanguage.ResumeLayout(false);
            this.panelLanguage.PerformLayout();
            this.panelExportResults.ResumeLayout(false);
            this.panelResultsSelection.ResumeLayout(false);
            this.groupBoxResultsSelection.ResumeLayout(false);
            this.groupBoxResultsSelection.PerformLayout();
            this.groupBoxStationSelection.ResumeLayout(false);
            this.groupBoxStationSelection.PerformLayout();
            this.groupBoxResultStatusSelect.ResumeLayout(false);
            this.groupBoxResultStatusSelect.PerformLayout();
            this.panelDates.ResumeLayout(false);
            this.panelDates.PerformLayout();
            this.panelGridView.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewSql)).EndInit();
            this.panelExtendedOption.ResumeLayout(false);
            this.panelExtendedOption.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.Panel panelControls;
        private System.Windows.Forms.Panel panelGridView;
        private System.Windows.Forms.DataGridView dataGridViewSql;
        private System.Windows.Forms.Panel panelDates;
        private System.Windows.Forms.Label labelDateFrom;
        private System.Windows.Forms.DateTimePicker dateTimePickerFrom;
        private System.Windows.Forms.DateTimePicker dateTimePickerTo;
        private System.Windows.Forms.Label labelDateTo;
        private System.Windows.Forms.Label labelDateSelection;
        private System.Windows.Forms.Panel panelResultsSelection;
        private System.Windows.Forms.GroupBox groupBoxResultStatusSelect;
        private System.Windows.Forms.RadioButton radioButtonAll;
        private System.Windows.Forms.RadioButton radioButtonNok;
        private System.Windows.Forms.RadioButton radioButtonOk;
        private System.Windows.Forms.Label labelResultsSelection;
        private System.Windows.Forms.GroupBox groupBoxStationSelection;
        private System.Windows.Forms.RadioButton radioButtonStation02;
        private System.Windows.Forms.RadioButton radioButtonStation01;
        private System.Windows.Forms.GroupBox groupBoxResultsSelection;
        private System.Windows.Forms.CheckBox checkBoxMeasurement1;
        private System.Windows.Forms.CheckBox checkBoxMeasurement6;
        private System.Windows.Forms.CheckBox checkBoxMeasurement4;
        private System.Windows.Forms.CheckBox checkBoxMeasurement2;
        private System.Windows.Forms.CheckBox checkBoxMeasurement5;
        private System.Windows.Forms.CheckBox checkBoxMeasurement3;
        private System.Windows.Forms.CheckBox checkBoxMeasurement7;
        private System.Windows.Forms.CheckBox checkBoxMeasurement8;
        private System.Windows.Forms.CheckBox checkBoxMeasurement10;
        private System.Windows.Forms.CheckBox checkBoxMeasurement9;
        private System.Windows.Forms.CheckBox checkBoxMeasurement11;
        private System.Windows.Forms.CheckBox checkBoxArea1;
        private System.Windows.Forms.CheckBox checkBoxArea2;
        private System.Windows.Forms.CheckBox checkBoxArea3;
        private System.Windows.Forms.CheckBox checkBoxArea4;
        private System.Windows.Forms.CheckBox checkBoxArea5;
        private System.Windows.Forms.CheckBox checkBoxArea6;
        private System.Windows.Forms.CheckBox checkBoxArea7;
        private System.Windows.Forms.Button buttonGetResults;
        private System.Windows.Forms.Panel panelExportResults;
        private System.Windows.Forms.Label labelExportResults;
        private System.Windows.Forms.Button buttonExportCsv;
        private System.Windows.Forms.Button buttonExportXlsx;
        private System.Windows.Forms.CheckBox checkBoxParameters;
        private System.Windows.Forms.Panel panelLanguage;
        private System.Windows.Forms.Label labelLanguageSelect;
        private System.Windows.Forms.RadioButton radioButtonLanguageSlovenian;
        private System.Windows.Forms.RadioButton radioButtonLanguageEnglish;
        private System.Windows.Forms.Panel panelExtendedOption;
    }
}

