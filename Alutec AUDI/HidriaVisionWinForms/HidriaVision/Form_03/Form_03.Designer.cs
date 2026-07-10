using ResultsView;

namespace HidriaVision
{
    partial class Form_03
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form_03));
            this.btnSnapShot = new System.Windows.Forms.Button();
            this.btnEdit = new System.Windows.Forms.Button();
            this.buttonShowLog = new System.Windows.Forms.Button();
            this.btnParameters = new System.Windows.Forms.Button();
            this.panelButtons = new System.Windows.Forms.Panel();
            this.labelProgram = new System.Windows.Forms.Label();
            this.splitContainerData = new System.Windows.Forms.SplitContainer();
            this.button1 = new System.Windows.Forms.Button();
            this.splitContainerDisplay = new System.Windows.Forms.SplitContainer();
            this.pictureBoxDisplay = new System.Windows.Forms.PictureBox();
            this.Form_03_ResultsLogView = new HidriaVision.Form_03_ResultsLogView();
            this.productivityControl = new HidriaVisionProductivity.ProductivityControl();
            this.Form_03_CommonControl1 = new HidriaVision.Form_03_CommonControl();
            this.productivityControl1 = new HidriaVisionProductivity.ProductivityControl();
            this.Form_03_ResultsControl = new HidriaVision.Form_03_ResultsControl();
            this.panelButtons.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerData)).BeginInit();
            this.splitContainerData.Panel1.SuspendLayout();
            this.splitContainerData.Panel2.SuspendLayout();
            this.splitContainerData.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerDisplay)).BeginInit();
            this.splitContainerDisplay.Panel1.SuspendLayout();
            this.splitContainerDisplay.Panel2.SuspendLayout();
            this.splitContainerDisplay.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxDisplay)).BeginInit();
            this.SuspendLayout();
            // 
            // btnSnapShot
            // 
            resources.ApplyResources(this.btnSnapShot, "btnSnapShot");
            this.btnSnapShot.BackColor = System.Drawing.Color.Transparent;
            this.btnSnapShot.BackgroundImage = global::HidriaVision.Properties.Resources.SaveImageIco;
            this.btnSnapShot.Name = "btnSnapShot";
            this.btnSnapShot.UseVisualStyleBackColor = false;
            this.btnSnapShot.Click += new System.EventHandler(this.btnSnapShot_Click);
            // 
            // btnEdit
            // 
            resources.ApplyResources(this.btnEdit, "btnEdit");
            this.btnEdit.BackColor = System.Drawing.Color.Transparent;
            this.btnEdit.BackgroundImage = global::HidriaVision.Properties.Resources.SaveCsvIco;
            this.btnEdit.Name = "btnEdit";
            this.btnEdit.UseVisualStyleBackColor = false;
            this.btnEdit.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // buttonShowLog
            // 
            this.buttonShowLog.BackColor = System.Drawing.Color.Transparent;
            this.buttonShowLog.BackgroundImage = global::HidriaVision.Properties.Resources.Loging;
            resources.ApplyResources(this.buttonShowLog, "buttonShowLog");
            this.buttonShowLog.Name = "buttonShowLog";
            this.buttonShowLog.UseVisualStyleBackColor = false;
            this.buttonShowLog.Click += new System.EventHandler(this.ButtonShowLog_Click);
            // 
            // btnParameters
            // 
            resources.ApplyResources(this.btnParameters, "btnParameters");
            this.btnParameters.BackColor = System.Drawing.Color.Transparent;
            this.btnParameters.BackgroundImage = global::HidriaVision.Properties.Resources.Parameters;
            this.btnParameters.Name = "btnParameters";
            this.btnParameters.UseVisualStyleBackColor = false;
            this.btnParameters.Click += new System.EventHandler(this.btnParameters_Click);
            // 
            // panelButtons
            // 
            resources.ApplyResources(this.panelButtons, "panelButtons");
            this.panelButtons.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.panelButtons.Controls.Add(this.btnParameters);
            this.panelButtons.Controls.Add(this.buttonShowLog);
            this.panelButtons.Controls.Add(this.btnEdit);
            this.panelButtons.Controls.Add(this.btnSnapShot);
            this.panelButtons.Name = "panelButtons";
            // 
            // labelProgram
            // 
            this.labelProgram.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.labelProgram.ForeColor = System.Drawing.SystemColors.ButtonFace;
            resources.ApplyResources(this.labelProgram, "labelProgram");
            this.labelProgram.Name = "labelProgram";
            // 
            // splitContainerData
            // 
            resources.ApplyResources(this.splitContainerData, "splitContainerData");
            this.splitContainerData.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainerData.Name = "splitContainerData";
            // 
            // splitContainerData.Panel1
            // 
            this.splitContainerData.Panel1.Controls.Add(this.button1);
            this.splitContainerData.Panel1.Controls.Add(this.productivityControl);
            this.splitContainerData.Panel1.Controls.Add(this.Form_03_CommonControl1);
            this.splitContainerData.Panel1.Controls.Add(this.labelProgram);
            // 
            // splitContainerData.Panel2
            // 
            this.splitContainerData.Panel2.BackColor = System.Drawing.Color.Black;
            this.splitContainerData.Panel2.Controls.Add(this.productivityControl1);
            this.splitContainerData.Panel2Collapsed = true;
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.SystemColors.Highlight;
            this.button1.ForeColor = System.Drawing.SystemColors.InfoText;
            resources.ApplyResources(this.button1, "button1");
            this.button1.Name = "button1";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // splitContainerDisplay
            // 
            resources.ApplyResources(this.splitContainerDisplay, "splitContainerDisplay");
            this.splitContainerDisplay.BackColor = System.Drawing.SystemColors.HotTrack;
            this.splitContainerDisplay.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.splitContainerDisplay.Name = "splitContainerDisplay";
            // 
            // splitContainerDisplay.Panel1
            //
            this.splitContainerDisplay.Panel1.Controls.Add(this.pictureBoxDisplay);
            //
            // splitContainerDisplay.Panel2
            //
            this.splitContainerDisplay.Panel2.BackColor = System.Drawing.Color.Black;
            this.splitContainerDisplay.Panel2.Controls.Add(this.Form_03_ResultsLogView);
            this.splitContainerDisplay.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainerDisplay_SplitterMoved);
            //
            // pictureBoxDisplay
            //
            this.pictureBoxDisplay.BackColor = System.Drawing.Color.Black;
            this.pictureBoxDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxDisplay.Name = "pictureBoxDisplay";
            this.pictureBoxDisplay.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBoxDisplay.TabStop = false;
            //
            // Form_03_ResultsLogView

            // 
            this.Form_03_ResultsLogView.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            resources.ApplyResources(this.Form_03_ResultsLogView, "Form_03_ResultsLogView");
            this.Form_03_ResultsLogView.MaxNumberOfEntries = ((uint)(200u));
            this.Form_03_ResultsLogView.Name = "Form_03_ResultsLogView";
            // 
            // productivityControl
            // 
            resources.ApplyResources(this.productivityControl, "productivityControl");
            this.productivityControl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.productivityControl.Name = "productivityControl";
            // 
            // Form_03_CommonControl1
            // 
            this.Form_03_CommonControl1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            resources.ApplyResources(this.Form_03_CommonControl1, "Form_03_CommonControl1");
            this.Form_03_CommonControl1.Name = "Form_03_CommonControl1";
            // 
            // productivityControl1
            // 
            resources.ApplyResources(this.productivityControl1, "productivityControl1");
            this.productivityControl1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.productivityControl1.Name = "productivityControl1";
            // 
            // Form_03_ResultsControl
            // 
            resources.ApplyResources(this.Form_03_ResultsControl, "Form_03_ResultsControl");
            this.Form_03_ResultsControl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(20)))), ((int)(((byte)(20)))));
            this.Form_03_ResultsControl.Name = "Form_03_ResultsControl";
            // 
            // Form_03
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            resources.ApplyResources(this, "$this");
            this.ControlBox = false;
            this.Controls.Add(this.splitContainerDisplay);
            this.Controls.Add(this.splitContainerData);
            this.Controls.Add(this.panelButtons);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form_03";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_03_FormClosing);
            this.Resize += new System.EventHandler(this.Form_03_Resize);
            this.panelButtons.ResumeLayout(false);
            this.splitContainerData.Panel1.ResumeLayout(false);
            this.splitContainerData.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerData)).EndInit();
            this.splitContainerData.ResumeLayout(false);
            this.splitContainerDisplay.Panel1.ResumeLayout(false);
            this.splitContainerDisplay.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerDisplay)).EndInit();
            this.splitContainerDisplay.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxDisplay)).EndInit();
            this.ResumeLayout(false);


        }

        #endregion
        private System.Windows.Forms.Panel panelButtons;
        private System.Windows.Forms.Button btnSnapShot;
        private System.Windows.Forms.Label labelProgram;
        private System.Windows.Forms.Button btnEdit;
        
        private Form_03_CommonControl Form_03_CommonControl1;
        private System.Windows.Forms.SplitContainer splitContainerData;
        private Form_03_ResultsControl Form_03_ResultsControl;
        private System.Windows.Forms.Button buttonShowLog;
        private HidriaVisionProductivity.ProductivityControl productivityControl1;
        private System.Windows.Forms.Button btnParameters;
        private System.Windows.Forms.SplitContainer splitContainerDisplay;
        private Form_03_ResultsLogView Form_03_ResultsLogView;
        private HidriaVisionProductivity.ProductivityControl productivityControl;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.PictureBox pictureBoxDisplay;
    }
}