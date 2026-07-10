using System;
using System.Windows.Forms;

using BaslerCam;
using Logger;
using System.Drawing;


namespace HidriaVision
{
    public partial class Form_EditToolBlock : Form
    {
        #region Public Delegates      
        public EventHandler<DialogResult> FormIsConfirmed;
        #endregion

        #region Public Properties
        /// <summary>
        /// Enables toolblock module.
        /// </summary>
        public bool EnableToolBlockModule
        {
            set
            {
                if (this.InvokeRequired)
                {
                    BeginInvoke(new Action(() => EnableToolBlockModule = value));
                }
                else
                {
                    EnableToolBlockModuleFunction(value);
                }
            }
        }

        /// <summary>
        /// Set header name
        /// </summary>
        public string HeaderName
        {
            set
            {
                if (this.InvokeRequired)
                {
                    BeginInvoke(new Action(() => this.Text = value));
                }
                else
                {
                    this.Text = value;
                }
            }
        }
        #endregion

        #region Private fields       
        private FileEventLogger Logger;      
        private int AddedControlsHeight = 0;    
        #endregion
       
        public Form_EditToolBlock(FileEventLogger logger)
        {
            InitializeComponent();                     
            Logger = logger;               
        }

        #region Private methods
        

        /// <summary>
        /// Adds additional controls.
        /// </summary>
        /// <param name="val">Control.</param>
        public void AddControl(Control val)
        {
            // Match with of panel with control
            val.Width = panelAdditionalControls.Width;
            // Set correct location of added control
            val.Location = new Point(0, AddedControlsHeight);
            AddedControlsHeight += (val.Height + 5);
            // Add control to panel
            panelAdditionalControls.Controls.Add(val);
        }
        /// <summary>
        /// Enables toolblock module.
        /// </summary>
        /// <param name="val">Bool value.</param>
        private void EnableToolBlockModuleFunction(bool val)
        {
            panelAdditionalControls.Enabled = val;
        }
        #endregion

        #region Private events
        /// <summary>
        /// Event fires when form loades.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void Form_EditToolBlock_Load(object sender, EventArgs e)
        {           
            this.pictureBoxLoading.Location = new Point((this.splitContainerMainWindow.Panel1.Width / 2) - (this.pictureBoxLoading.Width / 2), (this.splitContainerMainWindow.Panel1.Height / 2) - (this.pictureBoxLoading.Height / 2));            
        }

        /// <summary>
        /// Event fires when ok button is clicked.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args</param>
        private void btnOk_Click(object sender, EventArgs e)
        {
            FormIsConfirmed?.Invoke(this, DialogResult.OK);
        }

        /// <summary>
        /// Event fires when cancel button is clicked.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            FormIsConfirmed?.Invoke(this, DialogResult.Cancel);
        }

        /// <summary>
        /// Event fires when toolblock is loaded, this shows the control
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void cogToolBlockEditV21_SubjectChanged(object sender, EventArgs e)
        {
            
            pictureBoxLoading.Visible = false;        
        }

        /// <summary>
        /// Event fires when control is resized, sets loading gif in center
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event args.</param>
        private void splitContainerMainWindow_Panel1_Resize(object sender, EventArgs e)
        {
            this.pictureBoxLoading.Location = new Point((this.splitContainerMainWindow.Panel1.Width / 2) - (this.pictureBoxLoading.Width / 2), (this.splitContainerMainWindow.Panel1.Height / 2) - (this.pictureBoxLoading.Height / 2));
        }

        #endregion
    }
}
