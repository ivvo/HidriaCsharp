namespace HidriaVision
{
    partial class Form_EditToolBlock
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
            
            this.splitContainerMainWindow = new System.Windows.Forms.SplitContainer();
            this.pictureBoxLoading = new System.Windows.Forms.PictureBox();
            this.panelAdditionalControls = new System.Windows.Forms.Panel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOk = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMainWindow)).BeginInit();
            this.splitContainerMainWindow.Panel1.SuspendLayout();
            this.splitContainerMainWindow.Panel2.SuspendLayout();
            this.splitContainerMainWindow.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLoading)).BeginInit();
            this.SuspendLayout();
          
            // splitContainerMainWindow
            // 
            this.splitContainerMainWindow.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerMainWindow.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainerMainWindow.IsSplitterFixed = true;
            this.splitContainerMainWindow.Location = new System.Drawing.Point(0, 0);
            this.splitContainerMainWindow.Name = "splitContainerMainWindow";
            // 
            // splitContainerMainWindow.Panel1
            // 
            this.splitContainerMainWindow.Panel1.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.splitContainerMainWindow.Panel1.Controls.Add(this.pictureBoxLoading);
           
            this.splitContainerMainWindow.Panel1.Resize += new System.EventHandler(this.splitContainerMainWindow_Panel1_Resize);
            // 
            // splitContainerMainWindow.Panel2
            // 
            this.splitContainerMainWindow.Panel2.Controls.Add(this.panelAdditionalControls);
            this.splitContainerMainWindow.Panel2.Controls.Add(this.btnCancel);
            this.splitContainerMainWindow.Panel2.Controls.Add(this.btnOk);
            this.splitContainerMainWindow.Size = new System.Drawing.Size(1243, 539);
            this.splitContainerMainWindow.SplitterDistance = 950;
            this.splitContainerMainWindow.TabIndex = 1;
            // 
            // pictureBoxLoading
            // 
            this.pictureBoxLoading.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBoxLoading.Image = global::HidriaVision.Properties.Resources.loading;
            this.pictureBoxLoading.Location = new System.Drawing.Point(428, 176);
            this.pictureBoxLoading.Name = "pictureBoxLoading";
            this.pictureBoxLoading.Size = new System.Drawing.Size(200, 200);
            this.pictureBoxLoading.TabIndex = 1;
            this.pictureBoxLoading.TabStop = false;
            // 
            // panelAdditionalControls
            // 
            this.panelAdditionalControls.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelAdditionalControls.BackColor = System.Drawing.Color.Black;
            this.panelAdditionalControls.Location = new System.Drawing.Point(2, 0);
            this.panelAdditionalControls.Name = "panelAdditionalControls";
            this.panelAdditionalControls.Size = new System.Drawing.Size(284, 441);
            this.panelAdditionalControls.TabIndex = 21;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.BackColor = System.Drawing.Color.Transparent;
            this.btnCancel.BackgroundImage = global::HidriaVision.Properties.Resources.cancel;
            this.btnCancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCancel.Location = new System.Drawing.Point(8, 448);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 80);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOk
            // 
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOk.BackColor = System.Drawing.Color.Transparent;
            this.btnOk.BackgroundImage = global::HidriaVision.Properties.Resources.ok;
            this.btnOk.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOk.Location = new System.Drawing.Point(198, 448);
            this.btnOk.Margin = new System.Windows.Forms.Padding(4);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(80, 80);
            this.btnOk.TabIndex = 8;
            this.btnOk.UseVisualStyleBackColor = false;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // Form_EditToolBlock
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.ClientSize = new System.Drawing.Size(1243, 539);
            this.ControlBox = false;
            this.Controls.Add(this.splitContainerMainWindow);
            this.DoubleBuffered = true;
            this.ForeColor = System.Drawing.SystemColors.AppWorkspace;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "Form_EditToolBlock";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Form_EditToolBlock";
            this.Load += new System.EventHandler(this.Form_EditToolBlock_Load);
            ;
            this.splitContainerMainWindow.Panel1.ResumeLayout(false);
            this.splitContainerMainWindow.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerMainWindow)).EndInit();
            this.splitContainerMainWindow.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxLoading)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.SplitContainer splitContainerMainWindow;
        private System.Windows.Forms.Panel panelAdditionalControls;
        private System.Windows.Forms.PictureBox pictureBoxLoading;
    }
}