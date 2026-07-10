namespace HidriaVision
{
    partial class Form_Settings
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
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.splitContainerSettings = new System.Windows.Forms.SplitContainer();
            this.labelMain = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.tabLanguage = new System.Windows.Forms.TabPage();
            this.labelSelected = new System.Windows.Forms.Label();
            this.comboBoxLanguage = new System.Windows.Forms.ComboBox();
            this.tabLogging = new System.Windows.Forms.TabPage();
            this.numericUpDownMaxLogged = new System.Windows.Forms.NumericUpDown();
            this.labelMaxLogged = new System.Windows.Forms.Label();
            this.tabAdministrator = new System.Windows.Forms.TabPage();
            this.labelRepeat = new System.Windows.Forms.Label();
            this.labelNew = new System.Windows.Forms.Label();
            this.numericUpDownTimeout = new System.Windows.Forms.NumericUpDown();
            this.textBoxNew = new System.Windows.Forms.TextBox();
            this.textBoxRepeat = new System.Windows.Forms.TextBox();
            this.textBoxOld = new System.Windows.Forms.TextBox();
            this.labelTimeout = new System.Windows.Forms.Label();
            this.labelOld = new System.Windows.Forms.Label();
            this.buttonAccept = new System.Windows.Forms.Button();
            this.labelConfirm = new System.Windows.Forms.Label();
            this.tabStation03 = new System.Windows.Forms.TabPage();
            this.textBoxStation03ID = new System.Windows.Forms.TextBox();
            this.labelStation1ID = new System.Windows.Forms.Label();
            this.TabPLC = new System.Windows.Forms.TabPage();
            this.label3 = new System.Windows.Forms.Label();
            this.numericUpDownRack = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDownSlot = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.numericUpDownIP4 = new System.Windows.Forms.NumericUpDown();
            this.labelRack = new System.Windows.Forms.Label();
            this.numericUpDownIP3 = new System.Windows.Forms.NumericUpDown();
            this.labelSlot = new System.Windows.Forms.Label();
            this.numericUpDownIP2 = new System.Windows.Forms.NumericUpDown();
            this.numericUpDownIP1 = new System.Windows.Forms.NumericUpDown();
            this.labelIP = new System.Windows.Forms.Label();
            this.tabControlSettings = new System.Windows.Forms.TabControl();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerSettings)).BeginInit();
            this.splitContainerSettings.Panel1.SuspendLayout();
            this.splitContainerSettings.Panel2.SuspendLayout();
            this.splitContainerSettings.SuspendLayout();
            this.tabLanguage.SuspendLayout();
            this.tabLogging.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxLogged)).BeginInit();
            this.tabAdministrator.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimeout)).BeginInit();
            this.tabStation03.SuspendLayout();
            this.TabPLC.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRack)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSlot)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownIP4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownIP3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownIP2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownIP1)).BeginInit();
            this.tabControlSettings.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainerSettings
            // 
            this.splitContainerSettings.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainerSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerSettings.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainerSettings.IsSplitterFixed = true;
            this.splitContainerSettings.Location = new System.Drawing.Point(0, 0);
            this.splitContainerSettings.Name = "splitContainerSettings";
            this.splitContainerSettings.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerSettings.Panel1
            // 
            this.splitContainerSettings.Panel1.Controls.Add(this.labelMain);
            this.splitContainerSettings.Panel1.Controls.Add(this.btnCancel);
            this.splitContainerSettings.Panel1.Controls.Add(this.btnOk);
            // 
            // splitContainerSettings.Panel2
            // 
            this.splitContainerSettings.Panel2.Controls.Add(this.tabControlSettings);
            this.splitContainerSettings.Size = new System.Drawing.Size(601, 200);
            this.splitContainerSettings.SplitterDistance = 35;
            this.splitContainerSettings.TabIndex = 1;
            // 
            // labelMain
            // 
            this.labelMain.AutoSize = true;
            this.labelMain.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelMain.ForeColor = System.Drawing.Color.White;
            this.labelMain.Location = new System.Drawing.Point(11, 7);
            this.labelMain.Name = "labelMain";
            this.labelMain.Size = new System.Drawing.Size(133, 20);
            this.labelMain.TabIndex = 11;
            this.labelMain.Text = "Common settings";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.BackColor = System.Drawing.Color.Transparent;
            this.btnCancel.BackgroundImage = global::HidriaVision.Properties.Resources.cancel;
            this.btnCancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCancel.Location = new System.Drawing.Point(566, 0);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(33, 33);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.BtnCancel_Click);
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.BackColor = System.Drawing.Color.Transparent;
            this.btnOk.BackgroundImage = global::HidriaVision.Properties.Resources.ok;
            this.btnOk.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOk.Location = new System.Drawing.Point(526, 0);
            this.btnOk.Margin = new System.Windows.Forms.Padding(4);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(33, 33);
            this.btnOk.TabIndex = 10;
            this.btnOk.UseVisualStyleBackColor = false;
            this.btnOk.Click += new System.EventHandler(this.BtnOk_Click);
            // 
            // tabLanguage
            // 
            this.tabLanguage.Controls.Add(this.comboBoxLanguage);
            this.tabLanguage.Controls.Add(this.labelSelected);
            this.tabLanguage.Location = new System.Drawing.Point(4, 22);
            this.tabLanguage.Name = "tabLanguage";
            this.tabLanguage.Size = new System.Drawing.Size(591, 133);
            this.tabLanguage.TabIndex = 6;
            this.tabLanguage.Text = "Language";
            this.tabLanguage.UseVisualStyleBackColor = true;
            // 
            // labelSelected
            // 
            this.labelSelected.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.labelSelected.Location = new System.Drawing.Point(174, 18);
            this.labelSelected.Name = "labelSelected";
            this.labelSelected.Size = new System.Drawing.Size(210, 20);
            this.labelSelected.TabIndex = 12;
            this.labelSelected.Text = "Selected language";
            this.labelSelected.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // comboBoxLanguage
            // 
            this.comboBoxLanguage.FormattingEnabled = true;
            this.comboBoxLanguage.Items.AddRange(new object[] {
            "English",
            "Slovenian"});
            this.comboBoxLanguage.Location = new System.Drawing.Point(20, 18);
            this.comboBoxLanguage.Name = "comboBoxLanguage";
            this.comboBoxLanguage.Size = new System.Drawing.Size(148, 21);
            this.comboBoxLanguage.TabIndex = 13;
            // 
            // tabLogging
            // 
            this.tabLogging.Controls.Add(this.labelMaxLogged);
            this.tabLogging.Controls.Add(this.numericUpDownMaxLogged);
            this.tabLogging.Location = new System.Drawing.Point(4, 22);
            this.tabLogging.Name = "tabLogging";
            this.tabLogging.Size = new System.Drawing.Size(591, 133);
            this.tabLogging.TabIndex = 5;
            this.tabLogging.Text = "Logging";
            this.tabLogging.UseVisualStyleBackColor = true;
            // 
            // numericUpDownMaxLogged
            // 
            this.numericUpDownMaxLogged.Location = new System.Drawing.Point(20, 18);
            this.numericUpDownMaxLogged.Maximum = new decimal(new int[] {
            60000,
            0,
            0,
            0});
            this.numericUpDownMaxLogged.Name = "numericUpDownMaxLogged";
            this.numericUpDownMaxLogged.Size = new System.Drawing.Size(110, 20);
            this.numericUpDownMaxLogged.TabIndex = 8;
            this.numericUpDownMaxLogged.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // labelMaxLogged
            // 
            this.labelMaxLogged.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.labelMaxLogged.Location = new System.Drawing.Point(136, 19);
            this.labelMaxLogged.Name = "labelMaxLogged";
            this.labelMaxLogged.Size = new System.Drawing.Size(207, 19);
            this.labelMaxLogged.TabIndex = 12;
            this.labelMaxLogged.Text = "Max logged events";
            this.labelMaxLogged.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tabAdministrator
            // 
            this.tabAdministrator.Controls.Add(this.labelConfirm);
            this.tabAdministrator.Controls.Add(this.buttonAccept);
            this.tabAdministrator.Controls.Add(this.labelOld);
            this.tabAdministrator.Controls.Add(this.labelTimeout);
            this.tabAdministrator.Controls.Add(this.textBoxOld);
            this.tabAdministrator.Controls.Add(this.textBoxRepeat);
            this.tabAdministrator.Controls.Add(this.textBoxNew);
            this.tabAdministrator.Controls.Add(this.numericUpDownTimeout);
            this.tabAdministrator.Controls.Add(this.labelNew);
            this.tabAdministrator.Controls.Add(this.labelRepeat);
            this.tabAdministrator.Location = new System.Drawing.Point(4, 22);
            this.tabAdministrator.Name = "tabAdministrator";
            this.tabAdministrator.Size = new System.Drawing.Size(591, 133);
            this.tabAdministrator.TabIndex = 4;
            this.tabAdministrator.Text = "Administrator";
            this.tabAdministrator.UseVisualStyleBackColor = true;
            // 
            // labelRepeat
            // 
            this.labelRepeat.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.labelRepeat.Location = new System.Drawing.Point(158, 70);
            this.labelRepeat.Name = "labelRepeat";
            this.labelRepeat.Size = new System.Drawing.Size(126, 20);
            this.labelRepeat.TabIndex = 19;
            this.labelRepeat.Text = "Repeat password";
            this.labelRepeat.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelNew
            // 
            this.labelNew.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.labelNew.Location = new System.Drawing.Point(158, 43);
            this.labelNew.Name = "labelNew";
            this.labelNew.Size = new System.Drawing.Size(126, 20);
            this.labelNew.TabIndex = 17;
            this.labelNew.Text = "New password";
            this.labelNew.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numericUpDownTimeout
            // 
            this.numericUpDownTimeout.Location = new System.Drawing.Point(20, 97);
            this.numericUpDownTimeout.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDownTimeout.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDownTimeout.Name = "numericUpDownTimeout";
            this.numericUpDownTimeout.Size = new System.Drawing.Size(132, 20);
            this.numericUpDownTimeout.TabIndex = 17;
            this.numericUpDownTimeout.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.numericUpDownTimeout.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // textBoxNew
            // 
            this.textBoxNew.Location = new System.Drawing.Point(20, 44);
            this.textBoxNew.Name = "textBoxNew";
            this.textBoxNew.PasswordChar = '*';
            this.textBoxNew.Size = new System.Drawing.Size(132, 20);
            this.textBoxNew.TabIndex = 18;
            this.textBoxNew.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBoxRepeat
            // 
            this.textBoxRepeat.Location = new System.Drawing.Point(20, 71);
            this.textBoxRepeat.Name = "textBoxRepeat";
            this.textBoxRepeat.PasswordChar = '*';
            this.textBoxRepeat.Size = new System.Drawing.Size(132, 20);
            this.textBoxRepeat.TabIndex = 20;
            this.textBoxRepeat.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBoxOld
            // 
            this.textBoxOld.Location = new System.Drawing.Point(20, 18);
            this.textBoxOld.Name = "textBoxOld";
            this.textBoxOld.PasswordChar = '*';
            this.textBoxOld.Size = new System.Drawing.Size(132, 20);
            this.textBoxOld.TabIndex = 16;
            this.textBoxOld.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // labelTimeout
            // 
            this.labelTimeout.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.labelTimeout.Location = new System.Drawing.Point(158, 96);
            this.labelTimeout.Name = "labelTimeout";
            this.labelTimeout.Size = new System.Drawing.Size(197, 19);
            this.labelTimeout.TabIndex = 18;
            this.labelTimeout.Text = "Log in timeout (min)";
            this.labelTimeout.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelOld
            // 
            this.labelOld.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.labelOld.Location = new System.Drawing.Point(158, 18);
            this.labelOld.Name = "labelOld";
            this.labelOld.Size = new System.Drawing.Size(126, 20);
            this.labelOld.TabIndex = 14;
            this.labelOld.Text = "Old password";
            this.labelOld.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // buttonAccept
            // 
            this.buttonAccept.BackgroundImage = global::HidriaVision.Properties.Resources.ok;
            this.buttonAccept.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.buttonAccept.Location = new System.Drawing.Point(498, 42);
            this.buttonAccept.Name = "buttonAccept";
            this.buttonAccept.Size = new System.Drawing.Size(75, 75);
            this.buttonAccept.TabIndex = 21;
            this.toolTip1.SetToolTip(this.buttonAccept, "Accept new password");
            this.buttonAccept.UseVisualStyleBackColor = true;
            this.buttonAccept.Click += new System.EventHandler(this.ButtonAccept_Click);
            // 
            // labelConfirm
            // 
            this.labelConfirm.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.labelConfirm.Location = new System.Drawing.Point(498, 19);
            this.labelConfirm.Name = "labelConfirm";
            this.labelConfirm.Size = new System.Drawing.Size(75, 20);
            this.labelConfirm.TabIndex = 22;
            this.labelConfirm.Text = "Confirm";
            this.labelConfirm.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tabStation03
            // 
            this.tabStation03.Controls.Add(this.labelStation1ID);
            this.tabStation03.Controls.Add(this.textBoxStation03ID);
            this.tabStation03.Location = new System.Drawing.Point(4, 22);
            this.tabStation03.Name = "tabStation03";
            this.tabStation03.Size = new System.Drawing.Size(591, 133);
            this.tabStation03.TabIndex = 7;
            this.tabStation03.Text = "Station 03";
            this.tabStation03.UseVisualStyleBackColor = true;
            // 
            // textBoxStation03ID
            // 
            this.textBoxStation03ID.Location = new System.Drawing.Point(20, 18);
            this.textBoxStation03ID.Name = "textBoxStation03ID";
            this.textBoxStation03ID.Size = new System.Drawing.Size(151, 20);
            this.textBoxStation03ID.TabIndex = 18;
            this.textBoxStation03ID.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // labelStation1ID
            // 
            this.labelStation1ID.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.labelStation1ID.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.labelStation1ID.Location = new System.Drawing.Point(177, 18);
            this.labelStation1ID.Name = "labelStation1ID";
            this.labelStation1ID.Size = new System.Drawing.Size(106, 18);
            this.labelStation1ID.TabIndex = 17;
            this.labelStation1ID.Text = "Camera ID";
            this.labelStation1ID.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // TabPLC
            // 
            this.TabPLC.Controls.Add(this.labelIP);
            this.TabPLC.Controls.Add(this.numericUpDownIP1);
            this.TabPLC.Controls.Add(this.numericUpDownIP2);
            this.TabPLC.Controls.Add(this.labelSlot);
            this.TabPLC.Controls.Add(this.numericUpDownIP3);
            this.TabPLC.Controls.Add(this.labelRack);
            this.TabPLC.Controls.Add(this.numericUpDownIP4);
            this.TabPLC.Controls.Add(this.label1);
            this.TabPLC.Controls.Add(this.numericUpDownSlot);
            this.TabPLC.Controls.Add(this.label2);
            this.TabPLC.Controls.Add(this.numericUpDownRack);
            this.TabPLC.Controls.Add(this.label3);
            this.TabPLC.Location = new System.Drawing.Point(4, 22);
            this.TabPLC.Name = "TabPLC";
            this.TabPLC.Padding = new System.Windows.Forms.Padding(3);
            this.TabPLC.Size = new System.Drawing.Size(591, 133);
            this.TabPLC.TabIndex = 0;
            this.TabPLC.Text = "PLC";
            this.TabPLC.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(196, 23);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(10, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = ".";
            // 
            // numericUpDownRack
            // 
            this.numericUpDownRack.Location = new System.Drawing.Point(20, 45);
            this.numericUpDownRack.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownRack.Name = "numericUpDownRack";
            this.numericUpDownRack.Size = new System.Drawing.Size(110, 20);
            this.numericUpDownRack.TabIndex = 8;
            this.numericUpDownRack.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(133, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(10, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = ".";
            // 
            // numericUpDownSlot
            // 
            this.numericUpDownSlot.Location = new System.Drawing.Point(20, 73);
            this.numericUpDownSlot.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownSlot.Name = "numericUpDownSlot";
            this.numericUpDownSlot.Size = new System.Drawing.Size(110, 20);
            this.numericUpDownSlot.TabIndex = 9;
            this.numericUpDownSlot.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(70, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(10, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = ".";
            // 
            // numericUpDownIP4
            // 
            this.numericUpDownIP4.Location = new System.Drawing.Point(209, 18);
            this.numericUpDownIP4.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownIP4.Name = "numericUpDownIP4";
            this.numericUpDownIP4.Size = new System.Drawing.Size(48, 20);
            this.numericUpDownIP4.TabIndex = 4;
            this.numericUpDownIP4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // labelRack
            // 
            this.labelRack.BackColor = System.Drawing.Color.Transparent;
            this.labelRack.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.labelRack.Location = new System.Drawing.Point(143, 45);
            this.labelRack.Name = "labelRack";
            this.labelRack.Size = new System.Drawing.Size(114, 17);
            this.labelRack.TabIndex = 12;
            this.labelRack.Text = "Rack";
            this.labelRack.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numericUpDownIP3
            // 
            this.numericUpDownIP3.Location = new System.Drawing.Point(146, 18);
            this.numericUpDownIP3.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownIP3.Name = "numericUpDownIP3";
            this.numericUpDownIP3.Size = new System.Drawing.Size(48, 20);
            this.numericUpDownIP3.TabIndex = 3;
            this.numericUpDownIP3.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // labelSlot
            // 
            this.labelSlot.BackColor = System.Drawing.Color.Transparent;
            this.labelSlot.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.labelSlot.Location = new System.Drawing.Point(143, 73);
            this.labelSlot.Name = "labelSlot";
            this.labelSlot.Size = new System.Drawing.Size(114, 17);
            this.labelSlot.TabIndex = 13;
            this.labelSlot.Text = "Slot";
            this.labelSlot.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // numericUpDownIP2
            // 
            this.numericUpDownIP2.Location = new System.Drawing.Point(83, 18);
            this.numericUpDownIP2.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownIP2.Name = "numericUpDownIP2";
            this.numericUpDownIP2.Size = new System.Drawing.Size(48, 20);
            this.numericUpDownIP2.TabIndex = 2;
            this.numericUpDownIP2.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // numericUpDownIP1
            // 
            this.numericUpDownIP1.Location = new System.Drawing.Point(20, 18);
            this.numericUpDownIP1.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.numericUpDownIP1.Name = "numericUpDownIP1";
            this.numericUpDownIP1.Size = new System.Drawing.Size(48, 20);
            this.numericUpDownIP1.TabIndex = 1;
            this.numericUpDownIP1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // labelIP
            // 
            this.labelIP.AutoSize = true;
            this.labelIP.BackColor = System.Drawing.Color.Transparent;
            this.labelIP.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.labelIP.Location = new System.Drawing.Point(272, 20);
            this.labelIP.Name = "labelIP";
            this.labelIP.Size = new System.Drawing.Size(17, 13);
            this.labelIP.TabIndex = 16;
            this.labelIP.Text = "IP";
            this.labelIP.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tabControlSettings
            // 
            this.tabControlSettings.Controls.Add(this.TabPLC);
            this.tabControlSettings.Controls.Add(this.tabStation03);
            this.tabControlSettings.Controls.Add(this.tabAdministrator);
            this.tabControlSettings.Controls.Add(this.tabLogging);
            this.tabControlSettings.Controls.Add(this.tabLanguage);
            this.tabControlSettings.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControlSettings.Location = new System.Drawing.Point(0, 0);
            this.tabControlSettings.Name = "tabControlSettings";
            this.tabControlSettings.SelectedIndex = 0;
            this.tabControlSettings.Size = new System.Drawing.Size(599, 159);
            this.tabControlSettings.TabIndex = 22;
            // 
            // Form_Settings
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ClientSize = new System.Drawing.Size(601, 200);
            this.ControlBox = false;
            this.Controls.Add(this.splitContainerSettings);
            this.DoubleBuffered = true;
            this.ForeColor = System.Drawing.SystemColors.AppWorkspace;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_Settings";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form_Settings";
            this.splitContainerSettings.Panel1.ResumeLayout(false);
            this.splitContainerSettings.Panel1.PerformLayout();
            this.splitContainerSettings.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerSettings)).EndInit();
            this.splitContainerSettings.ResumeLayout(false);
            this.tabLanguage.ResumeLayout(false);
            this.tabLogging.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxLogged)).EndInit();
            this.tabAdministrator.ResumeLayout(false);
            this.tabAdministrator.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownTimeout)).EndInit();
            this.tabStation03.ResumeLayout(false);
            this.tabStation03.PerformLayout();
            this.TabPLC.ResumeLayout(false);
            this.TabPLC.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownRack)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownSlot)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownIP4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownIP3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownIP2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownIP1)).EndInit();
            this.tabControlSettings.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.SplitContainer splitContainerSettings;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Label labelMain;
        private System.Windows.Forms.TabControl tabControlSettings;
        private System.Windows.Forms.TabPage TabPLC;
        private System.Windows.Forms.Label labelIP;
        private System.Windows.Forms.NumericUpDown numericUpDownIP1;
        private System.Windows.Forms.NumericUpDown numericUpDownIP2;
        private System.Windows.Forms.Label labelSlot;
        private System.Windows.Forms.NumericUpDown numericUpDownIP3;
        private System.Windows.Forms.Label labelRack;
        private System.Windows.Forms.NumericUpDown numericUpDownIP4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown numericUpDownSlot;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numericUpDownRack;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TabPage tabStation03;
        private System.Windows.Forms.Label labelStation1ID;
        private System.Windows.Forms.TextBox textBoxStation03ID;
        private System.Windows.Forms.TabPage tabAdministrator;
        private System.Windows.Forms.Label labelConfirm;
        private System.Windows.Forms.Button buttonAccept;
        private System.Windows.Forms.Label labelOld;
        private System.Windows.Forms.Label labelTimeout;
        private System.Windows.Forms.TextBox textBoxOld;
        private System.Windows.Forms.TextBox textBoxRepeat;
        private System.Windows.Forms.TextBox textBoxNew;
        private System.Windows.Forms.NumericUpDown numericUpDownTimeout;
        private System.Windows.Forms.Label labelNew;
        private System.Windows.Forms.Label labelRepeat;
        private System.Windows.Forms.TabPage tabLogging;
        private System.Windows.Forms.Label labelMaxLogged;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxLogged;
        private System.Windows.Forms.TabPage tabLanguage;
        private System.Windows.Forms.ComboBox comboBoxLanguage;
        private System.Windows.Forms.Label labelSelected;
    }
}