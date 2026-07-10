using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ToolblockManager;

namespace HidriaVision
{
    public partial class Form_ToolBLockList : Form
    {
        #region Public Delegates
        public EventHandler<DialogResult> FormIsConfirmed;
        #endregion

        #region Public Fields
        /// <summary>
        /// Enum used for setting visual options (If none selected)
        /// </summary>
        [Flags]
        public enum EditingOptions
        {
            /// <summary> This option disables all except selection of toolblock! </summary>
            UseNone = 0x0,
            /// <summary> This option enables type selection, creation, renaming and deleting! </summary>
            UseTypes = 0x1,
            /// <summary> This option enables selecting programs! </summary>
            UsePrograms = 0x2,
            /// <summary> This option enables adding new types! </summary>
            EnableAddingTypes = 0x4

        }
        #endregion

        #region Private Fields            
     
        private List<string> availableTypesFullName;
        private List<string> availableTypesName;
        private List<uint> availableTypesNumbers;
        private List<string> availablePrograms;
        private List<uint> availableProgramsNumbers;
        private List<string> availableToolBlocks;
        private List<uint> availableToolBlocksNumbers;
        private List<uint> availableFreeTypesNumbers = new List<uint>();      
        #endregion

        #region Public Properties
        /// <summary>
        /// Set EditingOptions property
        /// </summary>
        public EditingOptions SetEditingOptions { private get; set; } = EditingOptions.UseNone;

        /// <summary>
        /// Get or set selected type for editing property
        /// </summary>       
        public uint SelectedType { get; set; } = 0;

        /// <summary>
        /// Get or set selected program for editing property
        /// </summary>
        public uint SelectedProgram { get; set; } = 0;

        /// <summary>
        /// Get or set selected toolblock for editing property
        /// </summary>
        public uint SelectedToolBlock { get; set; } = 0;

        /// <summary>
        /// Get or set selected type name for editing property
        /// </summary>       
        public string SelectedTypeFullName { get; set; }

        /// <summary>
        /// Get or set selected program name for editing property
        /// </summary>
        public string SelectedProgramFullName { get; set; }

        /// <summary>
        /// Get or set selected toolblock name for editing property
        /// </summary>
        public string SelectedToolBlockFullName { get; set; }

        /// <summary>
        /// Is for selected toolblock calibration property
        /// </summary>
        public bool IsSelectedToolBlockCalibration { get; set; } = false;

        /// <summary>
        /// Set header name
        /// </summary>       
        public string HeaderName
        {
            set
            {
                if (this.InvokeRequired)
                {
                    BeginInvoke(new Action(() => this.labelMain.Text = value));
                }
                else
                {
                    this.labelMain.Text = value;
                }
            }
        }
        #endregion


        #region Private Events
        /// <summary>
        /// Event when the form is loading
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form_ToolBlockList_Load(object sender, EventArgs e)
        {
            // Set visual style
            ChangeVisual();
            RefreshLists();
        }

        /// <summary>
        /// Event when cancel is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            SelectedType = 0;
            SelectedProgram = 0;
            SelectedToolBlock = 0;
            IsSelectedToolBlockCalibration = false;
            FormIsConfirmed?.Invoke(this, DialogResult.Cancel);
        }

        /// <summary>
        /// Event when EditCalibration is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonEditCalibration_Click(object sender, EventArgs e)
        {
            IsSelectedToolBlockCalibration = true;
            FormIsConfirmed?.Invoke(this, DialogResult.OK);
        }

        /// <summary>
        /// Event when EditToolBlock is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonEditToolBlock_Click(object sender, EventArgs e)
        {
            IsSelectedToolBlockCalibration = false;
            FormIsConfirmed?.Invoke(this, DialogResult.OK);
        }

        /// <summary>
        /// Event when type is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxType_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedType = availableTypesNumbers[availableTypesFullName.FindIndex(a => a.Equals(comboBoxType.SelectedItem.ToString()))];
            SelectedTypeFullName = comboBoxType.SelectedItem.ToString();
        }

        /// <summary>
        /// Event when program is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBoxProgram_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedProgram = availableProgramsNumbers[availablePrograms.FindIndex(a => a.Equals(comboBoxProgram.SelectedItem.ToString()))];
            SelectedProgramFullName = comboBoxProgram.SelectedItem.ToString();
            RefreshLists();
        }

        /// <summary>
        /// Event when toolblock is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBoxToolBlocks_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedToolBlock = availableToolBlocksNumbers[availableToolBlocks.FindIndex(a => a.Equals(listBoxToolBlocks.SelectedItem.ToString()))];
            SelectedToolBlockFullName = listBoxToolBlocks.SelectedItem.ToString();
        }

        /// <summary>
        /// Event when button create new is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCreateNew_Click(object sender, EventArgs e)
        {
            Form_CreateNewType Frm_CreateNewProgram = new Form_CreateNewType();
            this.Hide();
            Frm_CreateNewProgram.AvailableFreeTypes = availableFreeTypesNumbers;
            Frm_CreateNewProgram.ShowDialog(this);

            if (Frm_CreateNewProgram.DialogResult == DialogResult.OK)
            {
               
                SelectedType = Frm_CreateNewProgram.NewTypeNumber;
            }

            Frm_CreateNewProgram.Dispose();
            RefreshLists();
            this.Show();
        }

        /// <summary>
        /// Event when copy to new button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCopyToNew_Click(object sender, EventArgs e)
        {
            Form_CopyToNewType Frm_CopyToNewProgram = new Form_CopyToNewType();
            this.Hide();
            Frm_CopyToNewProgram.AvailableFreeTypes = availableFreeTypesNumbers;
            Frm_CopyToNewProgram.TypeToCopyFullName = SelectedTypeFullName;
            Frm_CopyToNewProgram.ShowDialog(this);

            if (Frm_CopyToNewProgram.DialogResult == DialogResult.OK)
            {
             
                SelectedType = Frm_CopyToNewProgram.NewTypeNumber;
            }

            Frm_CopyToNewProgram.Dispose();
            RefreshLists();
            this.Show();
        }

        /// <summary>
        /// Event when rename button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRename_Click(object sender, EventArgs e)
        {
            Form_RenameType Frm_RenameType = new Form_RenameType();
            this.Hide();
            Frm_RenameType.AvailableFreeTypes = availableFreeTypesNumbers;
            Frm_RenameType.TypeToRenameFullName = SelectedTypeFullName;
            Frm_RenameType.NewTypeNumber = SelectedType;
            Frm_RenameType.ShowDialog(this);

            if (Frm_RenameType.DialogResult == DialogResult.OK)
            {
              
                SelectedType = Frm_RenameType.NewTypeNumber;
            }

            Frm_RenameType.Dispose();
            RefreshLists();
            this.Show();
        }

        /// <summary>
        /// Event when delete button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            Form_DeleteType Frm_DeleteType = new Form_DeleteType();
            this.Hide();
            Frm_DeleteType.TypeToDeleteFullName = SelectedTypeFullName;
            Frm_DeleteType.ShowDialog(this);

            if (Frm_DeleteType.DialogResult == DialogResult.OK)
            {
                
            }

            Frm_DeleteType.Dispose();
            RefreshLists();
            this.Show();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// This method sets visual layout based on EditingOptions selected
        /// </summary>
        private void ChangeVisual()
        {
            // Set visual style for types and programs
            if (SetEditingOptions.HasFlag(EditingOptions.UseTypes) && SetEditingOptions.HasFlag(EditingOptions.UsePrograms))
            {
                if (SetEditingOptions.HasFlag(EditingOptions.EnableAddingTypes))
                {
                    // Adding new types is enabled
                    btnCreateNew.Visible = true;
                    btnCopyToNew.Visible = true;
                    btnRename.Visible = true;
                    btnDelete.Visible = true;

                    comboBoxType.Visible = true;
                    comboBoxType.Location = new Point(12, 54);
                    comboBoxType.Size = new Size(349, 24);
                    comboBoxProgram.Visible = true;
                    comboBoxProgram.Location = new Point(12, 84);
                    comboBoxProgram.Size = new Size(349, 24);
                    listBoxToolBlocks.Location = new Point(12, 114);
                    listBoxToolBlocks.Size = new Size(350, 284);
                }
                else
                {
                    // Adding new types is disabled
                    btnCreateNew.Visible = false;
                    btnCopyToNew.Visible = false;
                    btnRename.Visible = false;
                    btnDelete.Visible = false;

                    comboBoxType.Visible = true;
                    comboBoxType.Location = new Point(12, 54);
                    comboBoxType.Size = new Size(442, 24);
                    comboBoxProgram.Visible = true;
                    comboBoxProgram.Location = new Point(12, 84);
                    comboBoxProgram.Size = new Size(442, 24);
                    listBoxToolBlocks.Location = new Point(12, 114);
                    listBoxToolBlocks.Size = new Size(442, 284);
                }
            }
            // Set visual style for just types
            else if (SetEditingOptions.HasFlag(EditingOptions.UseTypes))
            {
                if (SetEditingOptions.HasFlag(EditingOptions.EnableAddingTypes))
                {
                    // Adding new types is enabled
                    btnCreateNew.Visible = true;
                    btnCopyToNew.Visible = true;
                    btnRename.Visible = true;
                    btnDelete.Visible = true;

                    comboBoxType.Visible = true;
                    comboBoxType.Location = new Point(12, 54);
                    comboBoxType.Size = new Size(349, 24);
                    comboBoxProgram.Visible = false;
                    listBoxToolBlocks.Location = new Point(12, 84);
                    listBoxToolBlocks.Size = new Size(350, 314);
                }
                else
                {
                    // Adding new types is disabled
                    btnCreateNew.Visible = false;
                    btnCopyToNew.Visible = false;
                    btnRename.Visible = false;
                    btnDelete.Visible = false;

                    comboBoxType.Visible = true;
                    comboBoxType.Location = new Point(12, 54);
                    comboBoxType.Size = new Size(442, 24);
                    comboBoxProgram.Visible = false;
                    listBoxToolBlocks.Location = new Point(12, 84);
                    listBoxToolBlocks.Size = new Size(442, 314);
                }
            }
            // Set visual style for just programs - here adding types is disabled by default
            else if (SetEditingOptions == (EditingOptions.UsePrograms))
            {
                btnCreateNew.Visible = false;
                btnCopyToNew.Visible = false;
                btnRename.Visible = false;
                btnDelete.Visible = false;

                comboBoxType.Visible = false;
                comboBoxProgram.Visible = true;
                comboBoxProgram.Location = new Point(12, 54);
                comboBoxProgram.Size = new Size(442, 24);
                listBoxToolBlocks.Location = new Point(12, 84);
                listBoxToolBlocks.Size = new Size(442, 314);
            }
            // Set visual style for selecting only toolblocks - here adding types is disabled by default
            else
            {
                btnCreateNew.Visible = false;
                btnCopyToNew.Visible = false;
                btnRename.Visible = false;
                btnDelete.Visible = false;

                comboBoxType.Visible = false;
                comboBoxProgram.Visible = false;               
                listBoxToolBlocks.Location = new Point(12, 54);
                listBoxToolBlocks.Size = new Size(442, 344);
            }
        }

        /// <summary>
        /// Method to refresh and show lists of types, programs and toolblocks
        /// </summary>
        private void RefreshLists()
        {
            listBoxToolBlocks.SelectedIndexChanged -= new EventHandler(this.listBoxToolBlocks_SelectedIndexChanged);
            comboBoxType.SelectedIndexChanged -= new EventHandler(this.comboBoxType_SelectedIndexChanged);
            comboBoxProgram.SelectedIndexChanged -= new EventHandler(this.comboBoxProgram_SelectedIndexChanged);

                  
            int cntTypes = availableTypesNumbers.Count;

            // Create a list of free available types
            availableFreeTypesNumbers.Clear();
            for (uint i = 0; i < 256; i++)
            {
                if (!availableTypesNumbers.Contains(i))
                {
                    availableFreeTypesNumbers.Add(i);
                }
            }

            // If default exists enable button for create new
            if (cntTypes > 0)
            {
                EnableButtonCreateNew(true);
            }

            if (cntTypes > 1)
            {
                // We have some types - enable buttons for manipulation of types
                EnableButtons(true);

                comboBoxType.Items.Clear();
                bool wasSelected = false;
                for (int i = 0; i < cntTypes; i++)
                {
                    // Add avaialble types in combo box except type 0 
                    if (availableTypesNumbers[i] != 0)
                    {
                        comboBoxType.Items.Add(availableTypesFullName[i]);
                        // If the type is the selected one - select it in combobox
                        if (SelectedType == availableTypesNumbers[i])
                        {
                            comboBoxType.SelectedItem = availableTypesFullName[i];                          
                            SelectedTypeFullName = availableTypesFullName[i];
                            wasSelected = true;
                        }
                    }
                }
                if (!wasSelected)
                {
                    comboBoxType.SelectedIndex = 0;
                    SelectedType = availableTypesNumbers[1];
                    SelectedTypeFullName = availableTypesFullName[1];
                }
             
                comboBoxProgram.Items.Clear();
                int cntPrograms = availableProgramsNumbers.Count;
                if (cntPrograms > 0)
                {
                    wasSelected = false;
                    for (int i = 0; i < cntPrograms; i++)
                    {
                        // Add avaialble programs in combo box                   
                        comboBoxProgram.Items.Add(availablePrograms[i]);
                        // If the program is the selected one - select it in combobox
                        if (SelectedProgram == availableProgramsNumbers[i])
                        {
                            comboBoxProgram.SelectedItem = availablePrograms[i];                           
                            SelectedProgramFullName = availablePrograms[i];
                            wasSelected = true;
                        }
                    }
                    if (!wasSelected)
                    {
                        comboBoxProgram.SelectedIndex = 0;
                        SelectedProgram = availableProgramsNumbers[0];
                        SelectedProgramFullName = availablePrograms[0];
                    }
                }

                
                listBoxToolBlocks.Items.Clear();
                int cntToolBlocks = availableToolBlocksNumbers.Count;
                if (cntToolBlocks > 0)
                {
                    wasSelected = false;
                    for (int i = 0; i < cntToolBlocks; i++)
                    {
                        // Add avaialble toolblocks in list box                   
                        listBoxToolBlocks.Items.Add(availableToolBlocks[i]);
                        // If the toolblock is the selected one - select it in list box
                        if (SelectedToolBlock == availableToolBlocksNumbers[i])
                        {
                            listBoxToolBlocks.SelectedItem = availableToolBlocks[i];                            
                            SelectedToolBlockFullName = availableToolBlocks[i];
                            wasSelected = true;
                        }
                    }
                    if (!wasSelected)
                    {
                        listBoxToolBlocks.SelectedIndex = 0;
                        SelectedToolBlock = availableToolBlocksNumbers[0];
                        SelectedToolBlockFullName = availableToolBlocks[0];
                    }
                }
            }
            else
            {
                comboBoxType.Items.Clear();
                comboBoxType.Text = "";
                comboBoxProgram.Items.Clear();
                comboBoxProgram.Text = "";
                listBoxToolBlocks.Items.Clear();
                EnableButtons(false);
            }

            listBoxToolBlocks.SelectedIndexChanged += new EventHandler(this.listBoxToolBlocks_SelectedIndexChanged);
            comboBoxType.SelectedIndexChanged += new EventHandler(this.comboBoxType_SelectedIndexChanged);
            comboBoxProgram.SelectedIndexChanged += new EventHandler(this.comboBoxProgram_SelectedIndexChanged);
        }

        /// <summary>
        /// This method enables or disables buttons
        /// </summary>
        /// <param name="enable"></param>
        private void EnableButtons(bool enable)
        {
            if (enable)
            {
                btnCopyToNew.Enabled = true;
                btnRename.Enabled = true;
                btnDelete.Enabled = true;
                buttonEditCalibration.Enabled = true;
                buttonEditToolBlock.Enabled = true;
            }
            else
            {
                btnCopyToNew.Enabled = false;
                btnRename.Enabled = false;
                btnDelete.Enabled = false;
                buttonEditCalibration.Enabled = false;
                buttonEditToolBlock.Enabled = false;
            }
        }

        /// <summary>
        /// This method enables or disables create new button
        /// </summary>
        /// <param name="enable"></param>
        private void EnableButtonCreateNew(bool enable)
        {
            if (enable)
            {
                btnCreateNew.Enabled = true;               
            }
            else
            {
                btnCreateNew.Enabled = false;            
            }
        }       
        #endregion
    }
}
