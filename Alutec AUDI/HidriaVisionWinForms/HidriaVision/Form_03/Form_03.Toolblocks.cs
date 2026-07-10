using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace HidriaVision
{
    public partial class Form_03 : Form
    {
        #region Private methods
        /// <summary>
        /// Method attaches handlers to the toolblocks
        /// </summary>
        /// <param name="toolblocks">List of toolblocks.</param>
        private void AttachToolBlockHandlers()
        {
        }


        #endregion

        #region Events
        /// <summary>
        /// Event handler for T000 toolblock
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event arguments.</param>
        private void T000_Toolblock_Ran(object sender, EventArgs e)
        {
          
            Station03CommonResults CommonResultsLocal = default;
       
            /*
            // Update cogRecord display and set cog record on the remote machine with correct image
            if (ToolblockResultsValid.Value)
            {
               UpdateRecordDisplay(cogRecordsDisplay, Record, "LastRun.PartUpsideDown.InputImage");
            }
            else
            {
                UpdateRecordDisplay(cogRecordsDisplay, Record, "LastRun.PartUpsideDown.InputImage");
            }
            */

            // Toolblock processed
            ToolblockProcessed.Value = true;
        }

        
        #endregion
    }
}
