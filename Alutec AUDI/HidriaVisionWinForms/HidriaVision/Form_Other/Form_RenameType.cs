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
    public partial class Form_RenameType : Form
    {
        #region Private Fields
        private const string filterRegexPattern = "[^a-zA-Z0-9_-]";
        private const uint stringMaxLength = 32;
        #endregion

        #region Public Properties
        public List<uint> AvailableFreeTypes { private get; set; }
        public string NewTypeName { get; private set; }
        public uint NewTypeNumber { get; set; }
        public string TypeToRenameFullName { private get; set; }
        #endregion

        public Form_RenameType()
        {
            InitializeComponent();
        }

        #region Private Events
        /// <summary>
        /// Event when form is loading
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form_RenameType_Load(object sender, EventArgs e)
        {
            textBoxTypeToRenameFullName.Text = TypeToRenameFullName;
            EnableButtonOK(false);
            // Fill combobox with available free type numbers
            comboBoxRenameTypeToNumber.Items.Clear();
            foreach (uint typeNmb in AvailableFreeTypes)
            {
                comboBoxRenameTypeToNumber.Items.Add(typeNmb.ToString("000"));
            }
            // Add current type number as avaialble free, sort combo box and select current type number
            comboBoxRenameTypeToNumber.Items.Add(NewTypeNumber.ToString("000"));
            comboBoxRenameTypeToNumber.Sorted = true;
            comboBoxRenameTypeToNumber.SelectedItem = NewTypeNumber.ToString("000");
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
            NewTypeName = textBoxRenameTypeToName.Text;
            NewTypeNumber = uint.Parse(comboBoxRenameTypeToNumber.SelectedItem.ToString());
            this.DialogResult = DialogResult.OK;
        }

        /// <summary>
        /// Event when text for new type name is changed,
        /// filters valid string with regex of allowed characters and max length
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBoxRenameTypeToName_TextChanged(object sender, EventArgs e)
        {
            // If we have some text, check allowed character set and remove invalid
            if (!string.IsNullOrEmpty(textBoxRenameTypeToName.Text))
            {
                var text = textBoxRenameTypeToName.Text;
                var newText = Regex.Replace(textBoxRenameTypeToName.Text, filterRegexPattern, "");

                // If string too long, remove characters
                if (newText.Length > stringMaxLength)
                {
                    newText = newText.Substring(0, (int)stringMaxLength);
                }

                // Set carret to correct position after string was modified
                if (text.Length != newText.Length)
                {
                    var selectionStart = textBoxRenameTypeToName.SelectionStart - (text.Length - newText.Length);
                    textBoxRenameTypeToName.Text = newText;
                    textBoxRenameTypeToName.SelectionStart = selectionStart;
                }
            }

            // If we have some characters, enable OK button
            if (!string.IsNullOrEmpty(textBoxRenameTypeToName.Text))
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
