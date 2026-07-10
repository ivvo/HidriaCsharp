using System;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;

namespace ResultsView
{
    public partial class ResultsGridView<T> : UserControl where T : struct
    {
        #region Private fields
        private uint _MaxNumberOfEntries;
        private bool GridViewAutoScroll;
        private object ObjLock;
        private FieldInfo[] StructFields;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the maximum number of lines.
        /// </summary>
        public uint MaxNumberOfEntries
        {
            get
            {
                lock (ObjLock)
                    return _MaxNumberOfEntries;
            }
            set
            {
                lock (ObjLock)
                    _MaxNumberOfEntries = value;
            }
        }
        #endregion

        /// <summary>
        /// Creates new object of type ResultsGridView.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        public ResultsGridView()
        {
            InitializeComponent();

            Type TypeArg;

            // Check if type is a structure
            TypeArg = typeof(T);

            if (!(TypeArg.IsValueType && !TypeArg.IsPrimitive && !TypeArg.IsEnum))
                throw new ArgumentException("Type is not a structure.");

            // Get all the fields from the structure
            StructFields = TypeArg.GetFields(BindingFlags.Public | BindingFlags.Instance);

            // Check that struct is not empty
            if (StructFields.Length == 0)
                throw new ArgumentException("Structure must have at least one field member.");

            // Go through all fields and check if they are of correct type
            foreach (FieldInfo structField in StructFields)
            {
                if (!structField.FieldType.IsPrimitive && (!structField.FieldType.IsGenericType && structField.FieldType != typeof(string) && structField.FieldType != typeof(ImageSource))
                    || (structField.FieldType.IsGenericType && structField.FieldType.GetGenericTypeDefinition() != typeof(ResultStatus<>)))
                    throw new ArgumentException("One or more fields are not of correct type.");
            }

            // Set default cell style
            dataGridViewResults.DefaultCellStyle.ForeColor = SystemColors.ActiveBorder;
            dataGridViewResults.DefaultCellStyle.BackColor = SystemColors.ActiveCaptionText;
            dataGridViewResults.DefaultCellStyle.SelectionForeColor = SystemColors.ActiveBorder;
            dataGridViewResults.DefaultCellStyle.SelectionBackColor = SystemColors.ActiveCaptionText;

            // Generate columns
            GenerateColumns();

            ObjLock = new object();
            GridViewAutoScroll = false;
            MaxNumberOfEntries = 100;
        }

        #region Public methods
        /// <summary>
        /// Adds new entry to the grid view.
        /// </summary>
        /// <param name="data">Data structure.</param>
        public void AddEntry(T data)
        {
            if (this.InvokeRequired)
                this.BeginInvoke(new Action(()=> AddEntry(data)));
            else
            {
                // Detach event
                dataGridViewResults.SelectionChanged -= new System.EventHandler(dataGridViewResults_SelectionChanged);

                // Remove first row if grid view is full
                if (dataGridViewResults.RowCount == MaxNumberOfEntries)
                    dataGridViewResults.Rows.RemoveAt(0);

                // Adds new row
                dataGridViewResults.Rows.Add();

                for (int i = 0; i < StructFields.Length; i++)
                {
                    dynamic StructFieldObject;
                    DataGridViewCell Cell;

                    // Get current cell and a field from a structure
                    Cell = dataGridViewResults.Rows[dataGridViewResults.RowCount - 1].Cells[i];
                    StructFieldObject = StructFields[i].GetValue(data);

                    // Set current cell
                    if (StructFields[i].FieldType == typeof(ImageSource))
                    {
                        Cell.Value = Properties.Control.CameraIcon;
                        Cell.ToolTipText = StructFieldObject.ImagePath;
                    }
                    else if(StructFields[i].FieldType.IsGenericType && StructFields[i].FieldType.GetGenericTypeDefinition() == typeof(ResultStatus<>))
                    {
                        if (StructFieldObject.ResStatus == ResultStatus.Ok)
                            Cell.Style.BackColor = Color.LimeGreen;
                        else
                            Cell.Style.BackColor = Color.Firebrick;

                        Cell.Value = StructFieldObject.Value;
                    }
                    else
                    {
                        Cell.Value = StructFieldObject;
                    }
                }

                // Set scroll index 
                if(GridViewAutoScroll)
                    dataGridViewResults.FirstDisplayedScrollingRowIndex = dataGridViewResults.RowCount - 1;
                else
                {
                    if (dataGridViewResults.RowCount == 1)
                        dataGridViewResults.FirstDisplayedScrollingRowIndex = 0;
                    else if(dataGridViewResults.RowCount == MaxNumberOfEntries && dataGridViewResults.FirstDisplayedScrollingRowIndex > 0)
                        dataGridViewResults.FirstDisplayedScrollingRowIndex = dataGridViewResults.FirstDisplayedScrollingRowIndex - 1;
                }

                // Refresh grid view
                RefreshGridView();

                // Reattach the event
                dataGridViewResults.SelectionChanged += new System.EventHandler(dataGridViewResults_SelectionChanged);
            }
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Generates columns of the grid view.
        /// </summary>
        private void GenerateColumns()
        {
            // Go through all the fields
            foreach (FieldInfo structField in StructFields)
            {
                ColumnAttribute Attrib = structField.GetCustomAttribute<ColumnAttribute>();

                // Check if attribute exists
                if (Attrib == null)
                    throw new InvalidOperationException("Attribute has not been applied to the field.");

                // Create columns
                if (structField.FieldType == typeof(ImageSource))
                    dataGridViewResults.Columns.Add(GenerateImageColumn(Attrib.Name, Attrib.FillWeight));
                else if (structField.FieldType.IsGenericType && structField.FieldType.GetGenericTypeDefinition() == typeof(ResultStatus<>))
                    dataGridViewResults.Columns.Add(GenerateResultColumn(Attrib.Name, Attrib.FillWeight));
                else
                    dataGridViewResults.Columns.Add(GenerateTextColumn(Attrib.Name, Attrib.FillWeight));
            }
        }

        /// <summary>
        /// Generates text column.
        /// </summary>
        /// <param name="headerName">Header name.</param>
        /// <param name="fillWeight">Fill weight.</param>
        /// <returns>Returns text column.</returns>
        private DataGridViewTextBoxColumn GenerateTextColumn(string headerName, float fillWeight = 100.0f)
        {
            DataGridViewTextBoxColumn TextColumn = new DataGridViewTextBoxColumn();

            // Set parameters
            TextColumn.ReadOnly = true;
            TextColumn.HeaderText = headerName;
            TextColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            TextColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            TextColumn.FillWeight = fillWeight;
            TextColumn.DefaultCellStyle.ForeColor = SystemColors.ActiveBorder;

            return TextColumn;
        }

        /// <summary>
        /// Generates result column.
        /// </summary>
        /// <param name="headerName">Header name.</param>
        /// <param name="fillWeight">Fill weight.</param>
        /// <returns>Returns result column.</returns>
        private DataGridViewTextBoxColumn GenerateResultColumn(string headerName, float fillWeight = 100.0f)
        {
            DataGridViewTextBoxColumn TextColumn = new DataGridViewTextBoxColumn();

            // Set parameters
            TextColumn.ReadOnly = true;
            TextColumn.HeaderText = headerName;
            TextColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            TextColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            TextColumn.FillWeight = fillWeight;
            TextColumn.DefaultCellStyle.ForeColor = SystemColors.ActiveCaptionText;

            return TextColumn;
        }

        /// <summary>
        /// Generates image column.
        /// </summary>
        /// <param name="headerName">Header name.</param>
        /// <param name="fillWeight">Fill weight.</param>
        /// <returns>Returns image column.</returns>
        private DataGridViewImageColumn GenerateImageColumn(string headerName, float fillWeight = 100.0f)
        {
            DataGridViewImageColumn ImageColumn = new DataGridViewImageColumn();

            // Set parameters
            ImageColumn.ReadOnly = true;
            ImageColumn.HeaderText = headerName;
            ImageColumn.SortMode = DataGridViewColumnSortMode.NotSortable;
            ImageColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
            ImageColumn.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            ImageColumn.Resizable = DataGridViewTriState.False;
            ImageColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            ImageColumn.FillWeight = fillWeight;

            return ImageColumn;
        }

        /// <summary>
        /// Resfreshes grid view.
        /// </summary>
        private void RefreshGridView()
        {
            dataGridViewResults.Refresh();
            dataGridViewResults.Update();
        }
        #endregion

        #region Private events
        /// <summary>
        /// Event fires when clear button is clicked.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event parameters.</param>
        private void buttonClear_Click(object sender, EventArgs e)
        {
            dataGridViewResults.Rows.Clear();
            RefreshGridView();
        }

        /// <summary>
        /// Event fires when export button is clicked.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event parameters.</param>
        private void buttonExportData_Click(object sender, EventArgs e)
        {
            bool RowsPresent = false;
            StringBuilder CsvBuilder = new StringBuilder();
            IEnumerable<DataGridViewColumn> Headers;

            // Get header and append it
            Headers = dataGridViewResults.Columns.Cast<DataGridViewColumn>();
            CsvBuilder.AppendLine(string.Join(";", Headers.Select(column => "\"" + column.HeaderText + "\"").ToArray()));

            foreach (DataGridViewRow row in dataGridViewResults.Rows)
            {
                IEnumerable<DataGridViewCell> Cells;

                // Get cells and append them
                Cells = row.Cells.Cast<DataGridViewCell>();
                CsvBuilder.AppendLine(string.Join(";", Cells.Select(cell =>cell is DataGridViewImageCell ? cell.ToolTipText : cell.Value).ToArray()));

                if (!RowsPresent)
                    RowsPresent = true;
            }

            // Rows must be present if we want to save file
            if(RowsPresent)
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    // Set filters and show dialog
                    saveFileDialog.Filter = "|*.csv";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Save file
                        File.WriteAllText(saveFileDialog.FileName, CsvBuilder.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Event fires when auto scroll button is clicked.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event parameters.</param>
        private void buttonAutoScroll_Click(object sender, EventArgs e)
        {
            if(GridViewAutoScroll)
            {
                buttonAutoScroll.Text = Properties.Control.buttonAutoScrollOffText;
                GridViewAutoScroll = false;
            }
            else
            {
                if(dataGridViewResults.Rows.Count > 0)
                    dataGridViewResults.FirstDisplayedScrollingRowIndex = dataGridViewResults.Rows.Count - 1;

                buttonAutoScroll.Text = Properties.Control.buttonAutoScrollOnText;
                GridViewAutoScroll = true;
            }
        }

        /// <summary>
        /// Event is fires when we click a cell.
        /// </summary>
        /// <param name="sender">Sender.</param>
        /// <param name="e">Event parameters.</param>
        private async void dataGridViewResults_SelectionChanged(object sender, EventArgs e)
        {
            if(dataGridViewResults.SelectedCells.Count != 0)
            {
                DataGridViewCell SelectedCell = dataGridViewResults.SelectedCells[0];

                // Show image
                if(SelectedCell is DataGridViewImageCell)
                {
                    if (File.Exists(SelectedCell.ToolTipText))
                        System.Diagnostics.Process.Start(SelectedCell.ToolTipText);
                    else
                        await Task.Run(() => MessageBox.Show(Properties.Control.messageBoxMessage));
                }

                // Clear selection and refresh grid view
                dataGridViewResults.ClearSelection();
                RefreshGridView();
            }
        }
        #endregion
    }
}
