namespace HidriaVision
{
    partial class Form_CopyToNewType
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
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textBoxTypeToCopyFullName = new System.Windows.Forms.TextBox();
            this.comboBoxNewTypeNumber = new System.Windows.Forms.ComboBox();
            this.textBoxNewTypeName = new System.Windows.Forms.TextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.panelTopHeader = new System.Windows.Forms.Panel();
            this.labelMain = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
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
            this.panelMain.Controls.Add(this.label3);
            this.panelMain.Controls.Add(this.label1);
            this.panelMain.Controls.Add(this.textBoxTypeToCopyFullName);
            this.panelMain.Controls.Add(this.comboBoxNewTypeNumber);
            this.panelMain.Controls.Add(this.textBoxNewTypeName);
            this.panelMain.Controls.Add(this.btnCancel);
            this.panelMain.Controls.Add(this.panelTopHeader);
            this.panelMain.Controls.Add(this.btnOK);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Margin = new System.Windows.Forms.Padding(4);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(237, 262);
            this.panelMain.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Location = new System.Drawing.Point(70, 123);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(15, 23);
            this.label3.TabIndex = 9;
            this.label3.Text = "_";
            this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.label1.Location = new System.Drawing.Point(11, 98);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(213, 23);
            this.label1.TabIndex = 7;
            this.label1.Text = "Copy to:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // textBoxTypeToCopyFullName
            // 
            this.textBoxTypeToCopyFullName.Location = new System.Drawing.Point(11, 75);
            this.textBoxTypeToCopyFullName.Name = "textBoxTypeToCopyFullName";
            this.textBoxTypeToCopyFullName.ReadOnly = true;
            this.textBoxTypeToCopyFullName.Size = new System.Drawing.Size(213, 22);
            this.textBoxTypeToCopyFullName.TabIndex = 6;
            // 
            // comboBoxNewTypeNumber
            // 
            this.comboBoxNewTypeNumber.DropDownHeight = 100;
            this.comboBoxNewTypeNumber.FormattingEnabled = true;
            this.comboBoxNewTypeNumber.IntegralHeight = false;
            this.comboBoxNewTypeNumber.Location = new System.Drawing.Point(11, 124);
            this.comboBoxNewTypeNumber.Name = "comboBoxNewTypeNumber";
            this.comboBoxNewTypeNumber.Size = new System.Drawing.Size(53, 24);
            this.comboBoxNewTypeNumber.TabIndex = 4;
            // 
            // textBoxNewTypeName
            // 
            this.textBoxNewTypeName.Location = new System.Drawing.Point(91, 125);
            this.textBoxNewTypeName.MaxLength = 32;
            this.textBoxNewTypeName.Name = "textBoxNewTypeName";
            this.textBoxNewTypeName.Size = new System.Drawing.Size(133, 22);
            this.textBoxNewTypeName.TabIndex = 2;
            this.textBoxNewTypeName.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.textBoxNewTypeName.TextChanged += new System.EventHandler(this.textBoxNewTypeName_TextChanged);
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.Transparent;
            this.btnCancel.BackgroundImage = global::HidriaVision.Properties.Resources.cancel;
            this.btnCancel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCancel.Location = new System.Drawing.Point(11, 165);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(87, 80);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // panelTopHeader
            // 
            this.panelTopHeader.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panelTopHeader.BackColor = System.Drawing.Color.Transparent;
            this.panelTopHeader.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTopHeader.Controls.Add(this.labelMain);
            this.panelTopHeader.Location = new System.Drawing.Point(0, 0);
            this.panelTopHeader.Margin = new System.Windows.Forms.Padding(4);
            this.panelTopHeader.Name = "panelTopHeader";
            this.panelTopHeader.Size = new System.Drawing.Size(236, 43);
            this.panelTopHeader.TabIndex = 0;
            // 
            // labelMain
            // 
            this.labelMain.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelMain.Location = new System.Drawing.Point(-1, 7);
            this.labelMain.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelMain.Name = "labelMain";
            this.labelMain.Size = new System.Drawing.Size(236, 25);
            this.labelMain.TabIndex = 0;
            this.labelMain.Text = "Copy to new type?";
            this.labelMain.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnOK
            // 
            this.btnOK.BackColor = System.Drawing.Color.Transparent;
            this.btnOK.BackgroundImage = global::HidriaVision.Properties.Resources.ok;
            this.btnOK.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOK.Location = new System.Drawing.Point(136, 165);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(87, 80);
            this.btnOK.TabIndex = 0;
            this.btnOK.UseVisualStyleBackColor = false;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // Form_CopyToNewProgram
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.BackgroundImage = global::HidriaVision.Properties.Resources.HeaderTopBlank;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(237, 262);
            this.Controls.Add(this.panelMain);
            this.DoubleBuffered = true;
            this.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form_CopyToNewProgram";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form_CopyToNewProgram";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.Form_CopyToNewProgram_Load);
            this.panelMain.ResumeLayout(false);
            this.panelMain.PerformLayout();
            this.panelTopHeader.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Panel panelTopHeader;
        private System.Windows.Forms.Label labelMain;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TextBox textBoxNewTypeName;
        private System.Windows.Forms.ComboBox comboBoxNewTypeNumber;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBoxTypeToCopyFullName;
        private System.Windows.Forms.Label label3;
    }
}