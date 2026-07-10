using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HidriaVision
{
    public partial class Form_CopyToNewType : Form
    {
        #region Private Fields
        private const string filterRegexPattern = "[^a-zA-Z0-9_-]";
        private const uint stringMaxLength = 32;
        #endregion

        #region Public Properties
        public List<uint> AvailableFreeTypes { get; set; }
        public string NewTypeName { get; set; }
        public uint NewTypeNumber { get; set; }      
        public string TypeToCopyFullName { get; set; }
        #endregion

        public Form_CopyToNewType()
        {
            InitializeComponent();
        }

        #region Private Events
        /// <summary>
        /// Event when form is loading
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form_CopyToNewProgram_Load(object sender, EventArgs e)
        {            
            textBoxTypeToCopyFullName.Text = TypeToCopyFullName;
            EnableButtonOK(false);
            // Fill combobox with available free type numbers
            comboBoxNewTypeNumber.Items.Clear();
            foreach (uint typeNmb in AvailableFreeTypes)
            {
                comboBoxNewTypeNumber.Items.Add(typeNmb.ToString("000"));
            }
            comboBoxNewTypeNumber.SelectedIndex = 0;
        }

        /// <summary>
        /// Event Cancel button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// Event OK button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            NewTypeName = textBoxNewTypeName.Text;
            NewTypeNumber = uint.Parse(comboBoxNewTypeNumber.SelectedItem.ToString());
            this.DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// Event when text for new type name is changed,
        /// filters valid string with regex of allowed characters and max length
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxNewTypeName_TextChanged(object sender, EventArgs e)
        {
            // If we have some text, check allowed character set and remove invalid
            if (!string.IsNullOrEmpty(textBoxNewTypeName.Text))
            {
                var text = textBoxNewTypeName.Text;
                var newText = Regex.Replace(textBoxNewTypeName.Text, filterRegexPattern, "");

                // If string too long, remove characters
                if (newText.Length > stringMaxLength)
                {
                    newText = newText.Substring(0, (int)stringMaxLength);
                }

                // Set carret to correct position after string was modified
                if (text.Length != newText.Length)
                {
                    var selectionStart = textBoxNewTypeName.SelectionStart - (text.Length - newText.Length);
                    textBoxNewTypeName.Text = newText;
                    textBoxNewTypeName.SelectionStart = selectionStart;
                }
            }

            // If we have some characters, enable OK button
            if (!string.IsNullOrEmpty(textBoxNewTypeName.Text))
            {
                EnableButtonOK(true);
            }
            else
            {
                EnableButtonOK(false);
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// This method enables or disables OK button!
        /// </summary>
        /// <param name="enable"></param>
        private void EnableButtonOK(bool enable)
        {
            if (enable)
            {
                btnOK.Enabled = true;
            }
            else
            {
                btnOK.Enabled = false;
            }
        }
        #endregion      
    }
}
