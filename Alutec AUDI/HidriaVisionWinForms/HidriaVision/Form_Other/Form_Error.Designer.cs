namespace HidriaVision
{
    partial class Form_Error
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
            this.textBoxMessage = new System.Windows.Forms.TextBox();
            this.textBoxLongMessage = new System.Windows.Forms.TextBox();
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
            this.panelMain.Controls.Add(this.textBoxMessage);
            this.panelMain.Controls.Add(this.textBoxLongMessage);
            this.panelMain.Controls.Add(this.panelTopHeader);
            this.panelMain.Controls.Add(this.btnOK);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMain.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.panelMain.Location = new System.Drawing.Point(0, 0);
            this.panelMain.Margin = new System.Windows.Forms.Padding(4);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(454, 315);
            this.panelMain.TabIndex = 0;
            // 
            // textBoxMessage
            // 
            this.textBoxMessage.Location = new System.Drawing.Point(11, 50);
            this.textBoxMessage.Multiline = true;
            this.textBoxMessage.Name = "textBoxMessage";
            this.textBoxMessage.ReadOnly = true;
            this.textBoxMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxMessage.Size = new System.Drawing.Size(430, 61);
            this.textBoxMessage.TabIndex = 3;
            // 
            // textBoxLongMessage
            // 
            this.textBoxLongMessage.Location = new System.Drawing.Point(11, 117);
            this.textBoxLongMessage.Multiline = true;
            this.textBoxLongMessage.Name = "textBoxLongMessage";
            this.textBoxLongMessage.ReadOnly = true;
            this.textBoxLongMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxLongMessage.Size = new System.Drawing.Size(430, 106);
            this.textBoxLongMessage.TabIndex = 2;
            // 
            // panelTopHeader
            // 
            this.panelTopHeader.BackColor = System.Drawing.Color.Transparent;
            this.panelTopHeader.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelTopHeader.Controls.Add(this.labelMain);
            this.panelTopHeader.Location = new System.Drawing.Point(0, 0);
            this.panelTopHeader.Margin = new System.Windows.Forms.Padding(4);
            this.panelTopHeader.Name = "panelTopHeader";
            this.panelTopHeader.Size = new System.Drawing.Size(453, 43);
            this.panelTopHeader.TabIndex = 0;
            // 
            // labelMain
            // 
            this.labelMain.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.labelMain.Location = new System.Drawing.Point(-1, 7);
            this.labelMain.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelMain.Name = "labelMain";
            this.labelMain.Size = new System.Drawing.Size(453, 25);
            this.labelMain.TabIndex = 0;
            this.labelMain.Text = "Critical error";
            this.labelMain.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnOK
            // 
            this.btnOK.BackColor = System.Drawing.Color.Transparent;
            this.btnOK.BackgroundImage = global::HidriaVision.Properties.Resources.ok;
            this.btnOK.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOK.Location = new System.Drawing.Point(188, 229);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(87, 80);
            this.btnOK.TabIndex = 0;
            this.btnOK.UseVisualStyleBackColor = false;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // Form_Error
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.BackgroundImage = global::HidriaVision.Properties.Resources.HeaderTopBlank;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(454, 315);
            this.Controls.Add(this.panelMain);
            this.DoubleBuffered = true;
            this.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form_Error";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FormWarning";
            this.TopMost = true;
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
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TextBox textBoxLongMessage;
        private System.Windows.Forms.TextBox textBoxMessage;
    }
}