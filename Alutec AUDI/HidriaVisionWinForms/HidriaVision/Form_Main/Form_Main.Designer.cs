namespace HidriaVision
{
    partial class Form_Main
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_Main));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panelButton = new System.Windows.Forms.Panel();
            this.btnStation03 = new System.Windows.Forms.Button();
            this.btnLogView = new System.Windows.Forms.Button();
            this.btnSettingsView = new System.Windows.Forms.Button();
            this.picBox_Header = new System.Windows.Forms.PictureBox();
            this.btnExit = new System.Windows.Forms.Button();
            this.btnRestore = new System.Windows.Forms.Button();
            this.btnMinimize = new System.Windows.Forms.Button();
            this.picBox_HeaderBlank = new System.Windows.Forms.PictureBox();
            this.errorTimer = new System.Windows.Forms.Timer(this.components);
            this.btnLogIn = new System.Windows.Forms.Button();
            this.toolTipMain = new System.Windows.Forms.ToolTip(this.components);
            this.labelForm = new System.Windows.Forms.Label();
            this.applicationStatusTimer = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panelButton.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picBox_Header)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBox_HeaderBlank)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.BackColor = System.Drawing.Color.Black;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 62);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.panelButton);
            this.splitContainer1.Size = new System.Drawing.Size(1600, 738);
            this.splitContainer1.SplitterDistance = 106;
            this.splitContainer1.SplitterWidth = 1;
            this.splitContainer1.TabIndex = 14;
            // 
            // panelButton
            // 
            this.panelButton.BackgroundImage = global::HidriaVision.Properties.Resources.HeaderTopBlank;
            this.panelButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panelButton.Controls.Add(this.btnStation03);
            this.panelButton.Controls.Add(this.btnLogView);
            this.panelButton.Controls.Add(this.btnSettingsView);
            this.panelButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelButton.ForeColor = System.Drawing.Color.Transparent;
            this.panelButton.Location = new System.Drawing.Point(0, 0);
            this.panelButton.Margin = new System.Windows.Forms.Padding(4);
            this.panelButton.Name = "panelButton";
            this.panelButton.Size = new System.Drawing.Size(106, 738);
            this.panelButton.TabIndex = 0;
            // 
            // btnStation03
            // 
            this.btnStation03.BackColor = System.Drawing.SystemColors.ControlDark;
            this.btnStation03.BackgroundImage = global::HidriaVision.Properties.Resources.Blank;
            this.btnStation03.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnStation03.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.btnStation03.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnStation03.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold);
            this.btnStation03.ForeColor = System.Drawing.Color.Gray;
            this.btnStation03.Location = new System.Drawing.Point(3, 4);
            this.btnStation03.Margin = new System.Windows.Forms.Padding(4);
            this.btnStation03.Name = "btnStation03";
            this.btnStation03.Size = new System.Drawing.Size(100, 100);
            this.btnStation03.TabIndex = 5;
            this.btnStation03.Text = "Station 06";
            this.btnStation03.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTipMain.SetToolTip(this.btnStation03, "Switch to Station 03 view");
            this.btnStation03.UseVisualStyleBackColor = false;
            this.btnStation03.Click += new System.EventHandler(this.BtnStation03_Click);
            // 
            // btnLogView
            // 
            this.btnLogView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnLogView.BackColor = System.Drawing.SystemColors.ControlDark;
            this.btnLogView.BackgroundImage = global::HidriaVision.Properties.Resources.Log;
            this.btnLogView.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnLogView.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.btnLogView.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnLogView.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.btnLogView.ForeColor = System.Drawing.Color.Gray;
            this.btnLogView.Location = new System.Drawing.Point(3, 532);
            this.btnLogView.Margin = new System.Windows.Forms.Padding(4);
            this.btnLogView.Name = "btnLogView";
            this.btnLogView.Size = new System.Drawing.Size(100, 100);
            this.btnLogView.TabIndex = 4;
            this.btnLogView.Text = "Log";
            this.btnLogView.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTipMain.SetToolTip(this.btnLogView, "Show log");
            this.btnLogView.UseVisualStyleBackColor = false;
            this.btnLogView.Click += new System.EventHandler(this.logView_Click);
            // 
            // btnSettingsView
            // 
            this.btnSettingsView.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSettingsView.BackColor = System.Drawing.SystemColors.ControlDark;
            this.btnSettingsView.BackgroundImage = global::HidriaVision.Properties.Resources.Settings;
            this.btnSettingsView.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnSettingsView.FlatAppearance.BorderColor = System.Drawing.Color.Gray;
            this.btnSettingsView.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnSettingsView.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.btnSettingsView.ForeColor = System.Drawing.Color.Gray;
            this.btnSettingsView.Location = new System.Drawing.Point(3, 635);
            this.btnSettingsView.Margin = new System.Windows.Forms.Padding(4);
            this.btnSettingsView.Name = "btnSettingsView";
            this.btnSettingsView.Size = new System.Drawing.Size(100, 100);
            this.btnSettingsView.TabIndex = 3;
            this.btnSettingsView.Text = "Settings";
            this.btnSettingsView.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTipMain.SetToolTip(this.btnSettingsView, "Show settings");
            this.btnSettingsView.UseVisualStyleBackColor = false;
            this.btnSettingsView.Click += new System.EventHandler(this.BtnSettingsView_Click);
            // 
            // picBox_Header
            // 
            this.picBox_Header.BackColor = System.Drawing.Color.DarkGray;
            this.picBox_Header.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.picBox_Header.ErrorImage = null;
            this.picBox_Header.Image = global::HidriaVision.Properties.Resources.HeaderTopBlank;
            this.picBox_Header.Location = new System.Drawing.Point(0, 0);
            this.picBox_Header.Margin = new System.Windows.Forms.Padding(4);
            this.picBox_Header.Name = "picBox_Header";
            this.picBox_Header.Size = new System.Drawing.Size(711, 63);
            this.picBox_Header.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picBox_Header.TabIndex = 10;
            this.picBox_Header.TabStop = false;
            this.picBox_Header.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.picBox_Header_MouseDoubleClick);
            this.picBox_Header.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picBox_Header_MouseMove);
            // 
            // btnExit
            // 
            this.btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExit.BackgroundImage = global::HidriaVision.Properties.Resources.ExitButton;
            this.btnExit.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExit.Location = new System.Drawing.Point(1520, 6);
            this.btnExit.Margin = new System.Windows.Forms.Padding(4);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(50, 50);
            this.btnExit.TabIndex = 9;
            this.toolTipMain.SetToolTip(this.btnExit, "Exit");
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // btnRestore
            // 
            this.btnRestore.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRestore.BackgroundImage = global::HidriaVision.Properties.Resources.RestoreButton;
            this.btnRestore.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRestore.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRestore.Location = new System.Drawing.Point(1445, 6);
            this.btnRestore.Margin = new System.Windows.Forms.Padding(4);
            this.btnRestore.Name = "btnRestore";
            this.btnRestore.Size = new System.Drawing.Size(50, 50);
            this.btnRestore.TabIndex = 8;
            this.toolTipMain.SetToolTip(this.btnRestore, "Maximize");
            this.btnRestore.UseVisualStyleBackColor = true;
            this.btnRestore.Click += new System.EventHandler(this.btnRestore_Click);
            // 
            // btnMinimize
            // 
            this.btnMinimize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMinimize.BackgroundImage = global::HidriaVision.Properties.Resources.MinimizeButton;
            this.btnMinimize.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnMinimize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMinimize.Location = new System.Drawing.Point(1370, 6);
            this.btnMinimize.Margin = new System.Windows.Forms.Padding(4);
            this.btnMinimize.Name = "btnMinimize";
            this.btnMinimize.Size = new System.Drawing.Size(50, 50);
            this.btnMinimize.TabIndex = 7;
            this.toolTipMain.SetToolTip(this.btnMinimize, "Minimize");
            this.btnMinimize.UseVisualStyleBackColor = true;
            this.btnMinimize.Click += new System.EventHandler(this.btnMinimize_Click);
            // 
            // picBox_HeaderBlank
            // 
            this.picBox_HeaderBlank.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.picBox_HeaderBlank.ErrorImage = null;
            this.picBox_HeaderBlank.Image = global::HidriaVision.Properties.Resources.HeaderTopBlank;
            this.picBox_HeaderBlank.Location = new System.Drawing.Point(700, 0);
            this.picBox_HeaderBlank.Margin = new System.Windows.Forms.Padding(4);
            this.picBox_HeaderBlank.Name = "picBox_HeaderBlank";
            this.picBox_HeaderBlank.Size = new System.Drawing.Size(903, 62);
            this.picBox_HeaderBlank.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picBox_HeaderBlank.TabIndex = 11;
            this.picBox_HeaderBlank.TabStop = false;
            this.picBox_HeaderBlank.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.picBox_HeaderBlank_MouseDoubleClick);
            this.picBox_HeaderBlank.MouseMove += new System.Windows.Forms.MouseEventHandler(this.picBox_HeaderBlank_MouseMove);
            // 
            // errorTimer
            // 
            this.errorTimer.Interval = 1000;
            this.errorTimer.Tick += new System.EventHandler(this.ErrorTimer_Tick);
            // 
            // btnLogIn
            // 
            this.btnLogIn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLogIn.BackColor = System.Drawing.SystemColors.Control;
            this.btnLogIn.BackgroundImage = global::HidriaVision.Properties.Resources.Offline;
            this.btnLogIn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnLogIn.Enabled = false;
            this.btnLogIn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogIn.ForeColor = System.Drawing.Color.Black;
            this.btnLogIn.Location = new System.Drawing.Point(1295, 6);
            this.btnLogIn.Margin = new System.Windows.Forms.Padding(4);
            this.btnLogIn.Name = "btnLogIn";
            this.btnLogIn.Size = new System.Drawing.Size(50, 50);
            this.btnLogIn.TabIndex = 15;
            this.toolTipMain.SetToolTip(this.btnLogIn, "Log in as administrator\r\n");
            this.btnLogIn.UseVisualStyleBackColor = false;
            // 
            // toolTipMain
            // 
            this.toolTipMain.AutomaticDelay = 900;
            // 
            // labelForm
            // 
            this.labelForm.AutoSize = true;
            this.labelForm.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(16)))), ((int)(((byte)(16)))));
            this.labelForm.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelForm.ForeColor = System.Drawing.Color.DarkGray;
            this.labelForm.Location = new System.Drawing.Point(12, 13);
            this.labelForm.Name = "labelForm";
            this.labelForm.Size = new System.Drawing.Size(224, 29);
            this.labelForm.TabIndex = 16;
            this.labelForm.Text = "Hidria Vision - AUDI";
            this.labelForm.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.labelForm_MouseDoubleClick);
            this.labelForm.MouseMove += new System.Windows.Forms.MouseEventHandler(this.labelForm_MouseMove);
            // 
            // applicationStatusTimer
            // 
            this.applicationStatusTimer.Interval = 1000;
            this.applicationStatusTimer.Tick += new System.EventHandler(this.ApplicationStatusTimer_Tick);
            // 
            // Form_Main
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(1600, 800);
            this.Controls.Add(this.labelForm);
            this.Controls.Add(this.btnLogIn);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnRestore);
            this.Controls.Add(this.btnMinimize);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.picBox_HeaderBlank);
            this.Controls.Add(this.picBox_Header);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form_Main";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Hidria Vision";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_Main_FormClosing);
            this.Load += new System.EventHandler(this.Form_Main_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panelButton.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picBox_Header)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picBox_HeaderBlank)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnMinimize;
        private System.Windows.Forms.Button btnRestore;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.PictureBox picBox_Header;
        private System.Windows.Forms.PictureBox picBox_HeaderBlank;
        public System.Windows.Forms.SplitContainer splitContainer1;       
        private System.Windows.Forms.Button btnSettingsView;
        private System.Windows.Forms.Panel panelButton;
        private System.Windows.Forms.Button btnLogView;
        private System.Windows.Forms.Timer errorTimer;
        private System.Windows.Forms.Button btnLogIn;
        private System.Windows.Forms.ToolTip toolTipMain;
        private System.Windows.Forms.Label labelForm;
        private System.Windows.Forms.Button btnStation03;
        private System.Windows.Forms.Timer applicationStatusTimer;
    }
}