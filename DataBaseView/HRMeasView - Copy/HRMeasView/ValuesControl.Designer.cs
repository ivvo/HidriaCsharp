namespace HRMeasView
{
    partial class ValuesControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.maxValue = new System.Windows.Forms.Label();
            this.actValue = new System.Windows.Forms.Label();
            this.minValue = new System.Windows.Forms.Label();
            this.textBoxMax = new System.Windows.Forms.TextBox();
            this.textBoxAct = new System.Windows.Forms.TextBox();
            this.textBoxMin = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // maxValue
            // 
            this.maxValue.AutoSize = true;
            this.maxValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.maxValue.Location = new System.Drawing.Point(7, 24);
            this.maxValue.Name = "maxValue";
            this.maxValue.Size = new System.Drawing.Size(41, 17);
            this.maxValue.TabIndex = 0;
            this.maxValue.Text = "max :";
            // 
            // actValue
            // 
            this.actValue.AutoSize = true;
            this.actValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.actValue.Location = new System.Drawing.Point(7, 53);
            this.actValue.Name = "actValue";
            this.actValue.Size = new System.Drawing.Size(37, 17);
            this.actValue.TabIndex = 1;
            this.actValue.Text = "izm :";
            // 
            // minValue
            // 
            this.minValue.AutoSize = true;
            this.minValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.minValue.Location = new System.Drawing.Point(7, 82);
            this.minValue.Name = "minValue";
            this.minValue.Size = new System.Drawing.Size(38, 17);
            this.minValue.TabIndex = 2;
            this.minValue.Text = "min :";
            // 
            // textBoxMax
            // 
            this.textBoxMax.BackColor = System.Drawing.Color.LightSalmon;
            this.textBoxMax.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.textBoxMax.Location = new System.Drawing.Point(48, 21);
            this.textBoxMax.Name = "textBoxMax";
            this.textBoxMax.Size = new System.Drawing.Size(73, 23);
            this.textBoxMax.TabIndex = 3;
            // 
            // textBoxAct
            // 
            this.textBoxAct.BackColor = System.Drawing.Color.LightGreen;
            this.textBoxAct.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.textBoxAct.Location = new System.Drawing.Point(48, 50);
            this.textBoxAct.Name = "textBoxAct";
            this.textBoxAct.Size = new System.Drawing.Size(73, 23);
            this.textBoxAct.TabIndex = 4;
            // 
            // textBoxMin
            // 
            this.textBoxMin.BackColor = System.Drawing.Color.LightYellow;
            this.textBoxMin.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.textBoxMin.Location = new System.Drawing.Point(48, 79);
            this.textBoxMin.Name = "textBoxMin";
            this.textBoxMin.Size = new System.Drawing.Size(73, 23);
            this.textBoxMin.TabIndex = 5;
            // 
            // ValuesControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textBoxMin);
            this.Controls.Add(this.textBoxAct);
            this.Controls.Add(this.textBoxMax);
            this.Controls.Add(this.minValue);
            this.Controls.Add(this.actValue);
            this.Controls.Add(this.maxValue);
            this.Name = "ValuesControl";
            this.Size = new System.Drawing.Size(131, 125);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label maxValue;
        private System.Windows.Forms.Label actValue;
        private System.Windows.Forms.Label minValue;
        public System.Windows.Forms.TextBox textBoxMax;
        public System.Windows.Forms.TextBox textBoxAct;
        public System.Windows.Forms.TextBox textBoxMin;
    }
}
