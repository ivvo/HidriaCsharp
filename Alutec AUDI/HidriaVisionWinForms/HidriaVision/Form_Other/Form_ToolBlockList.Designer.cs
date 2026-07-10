namespace HidriaVision
{
    partial class Form_ToolBLockList
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
            this.panelMain = new System.Windows.Forms.Panel();
            this.comboBoxProgram = new System.Windows.Forms.ComboBox();
            this.comboBoxType = new System.Windows.Forms.ComboBox();
            this.buttonEditToolBlock = new System.Windows.Forms.Button();
            this.buttonEditCalibration = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnCopyToNew = new System.Windows.Forms.Button();
            this.btnRename = new System.Windows.Forms.Button();
            this.btnCreateNew = new System.Windows.Forms.Button();
            this.listBoxToolBlocks = new System.Windows.Forms.ListBox();
            this.panelTopHeader = new System.Windows.Forms.Panel();
            this.labelMain = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.panelMain.SuspendLayout();
            this.panelTopHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelMain
            // 
            this.panelMain.BackgroundImage = global::HidriaVision.Properties.Resources.HeaderTopBlank;
            this.panelMain.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.panelMain.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelMain.Controls.Add(this.comboBoxProgram);
            this.panelMain.Controls.Add(this.comboBoxType);
            this.panelMain.Controls.Add(this.buttonEditToolBlock);
            this.panelMain.Controls.Add(this.buttonEditCalibration);
            this.panelMain.Controls.Add(this.btnDelete);
            this.panelMain.Controls.Add(this.btnCopyToNew);
            this.panelMain.Controls.Add(this.btnRename);
            this.panelMain.Controls.Add(this.btnCreateNew);
            this.panelMain.Controls.Add(this.listBoxToolBlocks);
            this.panelMain.Controls.Add(this.panelTopHeader);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Margin = new System.Windows.Forms.Padding(4);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(467, 448);
            this.panelMain.TabIndex = 0;
            // 
            // comboBoxProgram
            // 
            this.comboBoxProgram.FormattingEnabled = true;
            this.comboBoxProgram.Location = new System.Drawing.Point(12, 84);
            this.comboBoxProgram.Name = "comboBoxProgram";
            this.comboBoxProgram.Size = new System.Drawing.Size(349, 24);
            this.comboBoxProgram.TabIndex = 9;
            this.comboBoxProgram.SelectedIndexChanged += new System.EventHandler(this.comboBoxProgram_SelectedIndexChanged);
            // 
            // comboBoxType
            // 
            this.comboBoxType.FormattingEnabled = true;
            this.comboBoxType.Location = new System.Drawing.Point(12, 54);
            this.comboBoxType.Name = "comboBoxType";
            this.comboBoxType.Size = new System.Drawing.Size(349, 24);
            this.comboBoxType.TabIndex = 8;
            this.comboBoxType.SelectedIndexChanged += new System.EventHandler(this.comboBoxType_SelectedIndexChanged);
            // 
            // buttonEditToolBlock
            // 
            this.buttonEditToolBlock.BackColor = System.Drawing.Color.Transparent;
            this.buttonEditToolBlock.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.buttonEditToolBlock.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.buttonEditToolBlock.Location = new System.Drawing.Point(245, 406);
            this.buttonEditToolBlock.Margin = new System.Windows.Forms.Padding(4);
            this.buttonEditToolBlock.Name = "buttonEditToolBlock";
            this.buttonEditToolBlock.Size = new System.Drawing.Size(210, 31);
            this.buttonEditToolBlock.TabIndex = 7;
            this.buttonEditToolBlock.Text = "Edit ToolBlock";
            this.toolTip1.SetToolTip(this.buttonEditToolBlock, "Set common calibration");
            this.buttonEditToolBlock.UseVisualStyleBackColor = false;
            this.buttonEditToolBlock.Click += new System.EventHandler(this.buttonEditToolBlock_Click);
            // 
            // buttonEditCalibration
            // 
            this.buttonEditCalibration.BackColor = System.Drawing.Color.Transparent;
            this.buttonEditCalibration.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.buttonEditCalibration.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.buttonEditCalibration.Location = new System.Drawing.Point(12, 406);
            this.buttonEditCalibration.Margin = new System.Windows.Forms.Padding(4);
            this.buttonEditCalibration.Name = "buttonEditCalibration";
            this.buttonEditCalibration.Size = new System.Drawing.Size(210, 31);
            this.buttonEditCalibration.TabIndex = 6;
            this.buttonEditCalibration.Text = "Edit Calibration";
            this.toolTip1.SetToolTip(this.buttonEditCalibration, "Set common calibration");
            this.buttonEditCalibration.UseVisualStyleBackColor = false;
            this.buttonEditCalibration.Click += new System.EventHandler(this.buttonEditCalibration_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.BackColor = System.Drawing.Color.Transparent;
            this.btnDelete.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnDelete.Location = new System.Drawing.Point(368, 318);
            this.btnDelete.Margin = new System.Windows.Forms.Padding(4);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(87, 80);
            this.btnDelete.TabIndex = 4;
            this.btnDelete.Text = "Delete";
            this.toolTip1.SetToolTip(this.btnDelete, "Cancel");
            this.btnDelete.UseVisualStyleBackColor = false;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnCopyToNew
            // 
            this.btnCopyToNew.BackColor = System.Drawing.Color.Transparent;
            this.btnCopyToNew.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCopyToNew.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCopyToNew.Location = new System.Drawing.Point(368, 142);
            this.btnCopyToNew.Margin = new System.Windows.Forms.Padding(4);
            this.btnCopyToNew.Name = "btnCopyToNew";
            this.btnCopyToNew.Size = new System.Drawing.Size(87, 80);
            this.btnCopyToNew.TabIndex = 2;
            this.btnCopyToNew.Text = "Copy to new";
            this.toolTip1.SetToolTip(this.btnCopyToNew, "Cancel");
            this.btnCopyToNew.UseVisualStyleBackColor = false;
            this.btnCopyToNew.Click += new System.EventHandler(this.btnCopyToNew_Click);
            // 
            // btnRename
            // 
            this.btnRename.BackColor = System.Drawing.Color.Transparent;
            this.btnRename.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnRename.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnRename.Location = new System.Drawing.Point(368, 230);
            this.btnRename.Margin = new System.Windows.Forms.Padding(4);
            this.btnRename.Name = "btnRename";
            this.btnRename.Size = new System.Drawing.Size(87, 80);
            this.btnRename.TabIndex = 3;
            this.btnRename.Text = "Rename";
            this.toolTip1.SetToolTip(this.btnRename, "Cancel");
            this.btnRename.UseVisualStyleBackColor = false;
            this.btnRename.Click += new System.EventHandler(this.btnRename_Click);
            // 
            // btnCreateNew
            // 
            this.btnCreateNew.BackColor = System.Drawing.Color.Transparent;
            this.btnCreateNew.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCreateNew.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCreateNew.Location = new System.Drawing.Point(368, 54);
            this.btnCreateNew.Margin = new System.Windows.Forms.Padding(4);
            this.btnCreateNew.Name = "btnCreateNew";
            this.btnCreateNew.Size = new System.Drawing.Size(87, 80);
            this.btnCreateNew.TabIndex = 1;
            this.btnCreateNew.Text = "Create new";
            this.toolTip1.SetToolTip(this.btnCreateNew, "Cancel");
            this.btnCreateNew.UseMnemonic = false;
            this.btnCreateNew.UseVisualStyleBackColor = false;
            this.btnCreateNew.Click += new System.EventHandler(this.btnCreateNew_Click);
            // 
            // listBoxToolBlocks
            // 
            this.listBoxToolBlocks.FormattingEnabled = true;
            this.listBoxToolBlocks.HorizontalScrollbar = true;
            this.listBoxToolBlocks.IntegralHeight = false;
            this.listBoxToolBlocks.ItemHeight = 16;
            this.listBoxToolBlocks.Location = new System.Drawing.Point(12, 114);
            this.listBoxToolBlocks.Name = "listBoxToolBlocks";
            this.listBoxToolBlocks.ScrollAlwaysVisible = true;
            this.listBoxToolBlocks.Size = new System.Drawing.Size(349, 284);
            this.listBoxToolBlocks.TabIndex = 0;
            this.listBoxToolBlocks.SelectedIndexChanged += new System.EventHandler(this.listBoxToolBlocks_SelectedIndexChanged);
            // 
            // panelTopHeader
            // 
            this.panelTopHeader.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelTopHeader.BackColor = System.Drawing.Color.Transparent;
            this.panelTopHeader.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTopHeader.Controls.Add(this.labelMain);
            this.panelTopHeader.Controls.Add(this.btnCancel);
            this.panelTopHeader.Location = new System.Drawing.Point(0, 0);
            this.panelTopHeader.Margin = new System.Windows.Forms.Padding(4);
            this.panelTopHeader.Name = "panelTopHeader";
            this.panelTopHeader.Size = new System.Drawing.Size(466, 43);
            this.panelTopHeader.TabIndex = 0;
            // 
            // labelMain
            // 
            this.labelMain.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelMain.Location = new System.Drawing.Point(-1, 7);
            this.labelMain.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelMain.Name = "labelMain";
            this.labelMain.Size = new System.Drawing.Size(414, 25);
            this.labelMain.TabIndex = 0;
            this.labelMain.Text = "Station 01";
            this.labelMain.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.Transparent;
            this.btnCancel.BackgroundImage = global::HidriaVision.Properties.Resources.cancel;
            this.btnCancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCancel.Location = new System.Drawing.Point(421, 4);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(33, 33);
            this.btnCancel.TabIndex = 6;
            this.toolTip1.SetToolTip(this.btnCancel, "Cancel");
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // Form_ToolBLockList
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.BackgroundImage = global::HidriaVision.Properties.Resources.HeaderTopBlank;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(467, 448);
            this.Controls.Add(this.panelMain);
            this.DoubleBuffered = true;
            this.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form_ToolBLockList";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form_ToolBlockList";
            this.Load += new System.EventHandler(this.Form_ToolBlockList_Load);
            this.panelMain.ResumeLayout(false);
            this.panelTopHeader.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.Panel panelTopHeader;
        private System.Windows.Forms.Label labelMain;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ListBox listBoxToolBlocks;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnCopyToNew;
        private System.Windows.Forms.Button btnRename;
        private System.Windows.Forms.Button btnCreateNew;
        private System.Windows.Forms.Button buttonEditCalibration;
        private System.Windows.Forms.ComboBox comboBoxProgram;
        private System.Windows.Forms.ComboBox comboBoxType;
        private System.Windows.Forms.Button buttonEditToolBlock;
    }
}