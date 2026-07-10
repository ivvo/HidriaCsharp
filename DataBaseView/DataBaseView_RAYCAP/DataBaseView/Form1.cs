using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;


namespace DataBaseView
{
	public partial class Form1 : Form
	{
		
		string connectionstring = @"Data Source = RAYCAP01-PC\SQLEXPRESS; Initial Catalog = Hidria; Integrated Security = true";

		public Form1()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Izvozi Podatke iz DataGreedView v csv datoteko.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonIzvoziCSV_Click(object sender, EventArgs e)
		{
			string CsvFpath = "";

			// Save File Dialog
			if (saveFileDialogCSV.ShowDialog() == DialogResult.OK)
				CsvFpath = saveFileDialogCSV.FileName;

			try
			{
				System.IO.StreamWriter csvFileWriter = new StreamWriter(CsvFpath, false);

				string columnHeaderText = "";

				int countColumn = dataBaseGridView.ColumnCount - 1;

				// Napiši header
				if (countColumn >= 0)
				{
					columnHeaderText = dataBaseGridView.Columns[0].HeaderText;
				}

				for (int i = 1; i <= countColumn; i++)
				{
					columnHeaderText = columnHeaderText + ';' + dataBaseGridView.Columns[i].HeaderText;
				}

				// Napiši ostale vrstice
				csvFileWriter.WriteLine(columnHeaderText);

				foreach (DataGridViewRow dataRowObject in dataBaseGridView.Rows)
				{
					if (!dataRowObject.IsNewRow)
					{
						string dataFromGrid = "";

						dataFromGrid = dataRowObject.Cells[0].Value.ToString();

						for (int i = 1; i <= countColumn; i++)
						{
							dataFromGrid = dataFromGrid + ';' + dataRowObject.Cells[i].Value.ToString();
						}
						csvFileWriter.WriteLine(dataFromGrid);
					}
				}

				csvFileWriter.Flush();
				csvFileWriter.Close();
			}
			catch (Exception exceptionObject)
			{
				MessageBox.Show(exceptionObject.ToString());
			}
		}

		/// <summary>
		/// Odpri Tabelo in jo prikaži v DataGreedView
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void buttonISCI_Click(object sender, EventArgs e)
		{
			string sql;
			string tableName = comboBoxTable.Text;

			//dataBaseGridView.cl

			using (SqlConnection sqlCon = new SqlConnection(connectionstring))
			{

				try
				{
					sqlCon.Open();

					sql = "SELECT * FROM " + tableName + " WHERE Date BETWEEN '" + dateTimeOD.Value.ToString("MM/dd/yyyy") + "' AND '" + dateTimeDO.Value.ToString("MM/dd/yyyy") + "'";
					//sql = "SELECT * FROM " + tableName;
					SqlDataAdapter sqlData = new SqlDataAdapter(sql, sqlCon);
					DataTable dataTbl = new DataTable();
					sqlData.Fill(dataTbl);

					// DataGreedView
					dataBaseGridView.DataSource = dataTbl;
				}
				catch(Exception ex)
				{
					MessageBox.Show(ex.Message);
				}

			}

		}

		private void buttonSettings_Click(object sender, EventArgs e)
		{
			FormNastavitve FormNast = new FormNastavitve();
			FormNast.ShowDialog();
		}
	}
}
