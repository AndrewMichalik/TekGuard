using System;
using System.Data;
using System.Drawing;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace TGPController
{
	/// <summary>
	/// Summary TGDataGrid description
	/// </summary>
	public class TGDataGrid : DataGrid
	{
		private void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this)).EndInit();

		}

		#region DGClientWidth
		// Subclass the datagrid in order to access the protected VertScrollBar.Visible property
		public int DGClientWidth
		{
			get
			{
				// Determine size of inner rectangle
				const int WID_GRIDBORDER = 4;
				int RemainingWidth = (ClientSize.Width - RowHeaderWidth) - (VertScrollBar.Visible ? SystemInformation.VerticalScrollBarWidth : 0) - WID_GRIDBORDER; 
				return (RemainingWidth);
			}		
		}
		#endregion

		public void SizeColumnsToContent_Example(int nRowsToScan)
		{
			// Create graphics object for measuring widths.
			Graphics Graphics = this.CreateGraphics();

			// Define new table style.
			DataGridTableStyle tableStyle = new DataGridTableStyle();

			try
			{
				DataTable dataTable = ((DataSet) this.DataSource).Tables[0];

				if (-1 == nRowsToScan)
				{
					nRowsToScan = dataTable.Rows.Count;
				}
				else
				{
					// Can only scan rows if they exist.
					nRowsToScan = System.Math.Min(nRowsToScan, dataTable.Rows.Count);
				}

				// Clear any existing table styles.
				this.TableStyles.Clear();

				// Use mapping name that is defined in the data source.
				tableStyle.MappingName = dataTable.TableName;

				// Now create the column styles within the table style.
				DataGridTextBoxColumn columnStyle;
				int iWidth;

				for (int iCurrCol = 0;
					iCurrCol < dataTable.Columns.Count; iCurrCol++)
				{
					DataColumn dataColumn = dataTable.Columns[iCurrCol];

					columnStyle = new DataGridTextBoxColumn();

					columnStyle.TextBox.Enabled = true;
					columnStyle.HeaderText = dataColumn.ColumnName;
					columnStyle.MappingName = dataColumn.ColumnName;

					// Set width to header text width.
					iWidth = (int)(Graphics.MeasureString(columnStyle.HeaderText, this.Font).Width);

					// Change width, if data width is
					// wider than header text width.
					// Check the width of the data in the first X rows.
					DataRow dataRow;
					for (int iRow = 0; iRow < nRowsToScan; iRow++)
					{
						dataRow = dataTable.Rows[iRow];

						if (null != dataRow[dataColumn.ColumnName])
						{
							int iColWidth = (int)(Graphics.MeasureString(dataRow.ItemArray[iCurrCol].ToString(), this.Font).Width);
							iWidth = (int)System.Math.Max(iWidth, iColWidth);
						}
					}
					columnStyle.Width = iWidth + 4;

					// Add the new column style to the table style.
					tableStyle.GridColumnStyles.Add(columnStyle);
				}
				// Add the new table style to the data grid.
				this.TableStyles.Add(tableStyle);
			}
			catch(Exception e)
			{
				MessageBox.Show(e.Message);
			}
			finally
			{
				Graphics.Dispose();
			}
		}

	}
								
	/// <summary>
	/// Summary ColumnComboBox description
	/// </summary>
	// Step 1. Derive a custom column style from DataGridTextBoxColumn
	//	a) add a ComboBox member
	//  b) track when the combobox has focus in Enter and Leave events
	//  c) override Edit to allow the ComboBox to replace the TextBox
	//  d) override Commit to save the changed data
	public class CBDataGridColumn : DataGridTextBoxColumn
	{
		public NoKeyUpCombo ColumnComboBox = null;
		private System.Windows.Forms.CurrencyManager _source = null;
		private int _rowNum;
		private bool _isEditing = false;
		ComboValueChanged _valueChanging;
		
		public CBDataGridColumn(ComboValueChanged valueChanging) : base()
		{
			_valueChanging = valueChanging;
			ColumnComboBox = new NoKeyUpCombo();
		
			ColumnComboBox.Leave += new EventHandler(LeaveComboBox);
//			ColumnComboBox.Enter += new EventHandler(ComboMadeCurrent);
			ColumnComboBox.SelectedIndexChanged += new System.EventHandler(ComboIndexChanged);
			ColumnComboBox.SelectionChangeCommitted += new System.EventHandler(ComboStartEditing);
			
		}
		
		private void ComboStartEditing(object sender, EventArgs e)
		{
			_isEditing = true;
			base.ColumnStartedEditing((Control) sender);
		}
		
		private void ComboIndexChanged(object sender, EventArgs e)
		{
			_valueChanging(_rowNum , ColumnComboBox.Text); 	
		}

//		private void ComboMadeCurrent(object sender, EventArgs e)
//		{
//			//_isEditing = true; 	
//		}
		
		private void LeaveComboBox(object sender, EventArgs e)
		{
			if(_isEditing)
			{
				//AJM to-do: Make general
				string sText;
				switch (ColumnComboBox.Text)
				{
					case "Good":	sText = "1";	break;
					case "Junk":	sText = "2";	break;
					default:		sText = "0";	break;
				}
				SetColumnValueAtRow(_source, _rowNum, sText);

				_isEditing = false;
				Invalidate();
			}
			ColumnComboBox.Hide();
		}

		#region GetColumnValueAtRow
		// Subclass the column to handle IP addresses
		protected override object GetColumnValueAtRow(CurrencyManager cm, int RowNum)
		{
			// Get data from the underlying record and format for display.
			object oVal = base.GetColumnValueAtRow(cm, RowNum);
			if (oVal is DBNull)
			{
				return ("");				// String to display for DBNull
			}
			else
			{
				// Convert will throw an exception if this  column style
				// is bound to a column containing non-numeric data

				//AJM to-do: Make general
				switch (oVal.ToString())
				{
					case "1":	return ("Good");
					case "2":	return ("Junk");
					default:	return ("Unknown");
				}
			}
		}
		#endregion

		#region Edit
		protected override void Edit(System.Windows.Forms.CurrencyManager source, int rowNum, System.Drawing.Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible)
		{
			base.Edit(source,rowNum, bounds, readOnly, instantText , cellIsVisible);

			_rowNum = rowNum;
			_source = source;
		
			ColumnComboBox.Parent = this.TextBox.Parent;
			ColumnComboBox.Location = this.TextBox.Location;
			ColumnComboBox.Size = new Size(this.TextBox.Size.Width, ColumnComboBox.Size.Height);
			ColumnComboBox.SelectedIndexChanged -= new System.EventHandler(ComboIndexChanged);
			ColumnComboBox.Text =  this.TextBox.Text;
			ColumnComboBox.SelectedIndexChanged += new System.EventHandler(ComboIndexChanged);

			this.TextBox.Visible = false;
			ColumnComboBox.Visible = true;
			ColumnComboBox.BringToFront();
			ColumnComboBox.Focus();	
		}
		#endregion

		#region Commit
		protected override bool Commit(System.Windows.Forms.CurrencyManager dataSource, int rowNum)
		{
			if(_isEditing)
			{
				_isEditing = false;

				//AJM to-do: Make general
				string sText;
				switch (ColumnComboBox.Text)
				{
					case "Good":	sText = "1";	break;
					case "Junk":	sText = "2";	break;
					default:		sText = "0";	break;
				}
				// SetColumnValueAtRow(dataSource, rowNum, ColumnComboBox.Text);
				SetColumnValueAtRow(dataSource, rowNum, sText);
			}
			return true;
		}
		#endregion

		#region NoKeyUpCombo
		public class NoKeyUpCombo : ComboBox
		{
			const int WM_KEYUP = 0x101;
			protected override void WndProc(ref System.Windows.Forms.Message m)
			{
				if(m.Msg == WM_KEYUP)
				{
					//ignore keyup to avoid problem with tabbing & dropdownlist;
					return;
				}
				base.WndProc(ref m);
			}
		}
		#endregion
	
	}
	public delegate void ComboValueChanged( int changingRow, object newValue );
												   
	/// <summary>
	/// Summary IPDataGridColumn description
	/// </summary>
	public class IPDataGridColumn : DataGridTextBoxColumn
	{
		#region GetColumnValueAtRow
		// Subclass the column to handle IP addresses
		protected override object GetColumnValueAtRow(CurrencyManager cm, int RowNum)
		{
			// Get data from the underlying record and format for display.
			object oVal = base.GetColumnValueAtRow(cm, RowNum);
			if (oVal is DBNull)
			{
				return ("");				// String to display for DBNull
			}
			else
			{
				// Convert will throw an exception if this  column style
				// is bound to a column containing non-numeric data
				return (BuildIP(Convert.ToInt64(oVal)));
			}
		}
		#endregion

		#region Commit
		protected override bool Commit(CurrencyManager cm, int RowNum)
		{
			// Parse the data and write to underlying record
			DataGridTextBox box = (DataGridTextBox) this.TextBox;
			decimal Value;
			// Do not write data if not editing.
			if (box.IsInEditOrNavigateMode) return (true);
			if (TextBox.Text == "")			// Map "" to DBNull
			{
				SetColumnValueAtRow(cm, RowNum, DBNull.Value);
			}
			else							// Parse the IP address string
			{
				try
				{
					Value = ParseIP(TextBox.Text);
					if (Value < 0) return (false);
				}	
				catch
				{
					return (false);			 // Exit on error and display old "good" value
				}
				SetColumnValueAtRow(cm, RowNum, Value);   // Write new value
			}
			this.EndEdit();					// Let the DataGrid know that processing is completed.
			return (true);					// Success
		}
		#endregion

		#region ParseIP
		private Int64 ParseIP(string IPString)
		{
			try
			{
				// Process as a 32 bit unsigned long
				Int64 iIP64 = 0;

				// Separate the octects
				string[] Octects = IPString.Split(new char[]{'.'});

				// Correct number of entries?
				if (Octects.Length != 4) return (-1);

				// Loop through all octects
				for (int ii=0; ii<Octects.Length; ii++)
				{
					iIP64 += (Convert.ToInt64(Octects[ii]) << (8 * (3-ii)));
				}
				return (iIP64);
			}
			catch
			{
				return (-1);
			}
		}
		#endregion

		#region BuildIP
		private string BuildIP(Int64 IPValue)
		{
			try
			{
				// Within range?
				if ((IPValue < 0) || (IPValue > 0xffffffff)) return (null);
			
				// Convert octects after bit shifting and accounting for negative sign
				Int64 Octet1 = (IPValue & 0xff000000) >> 24;
				Int64 Octet2 = (IPValue & 0xff0000)   >> 16;
				Int64 Octet3 = (IPValue & 0xff00)     >>  8;
				Int64 Octet4 = (IPValue & 0xff);

				// Build string
				StringBuilder IPString = new StringBuilder();
				IPString.AppendFormat ("{0}.{1}.{2}.{3}", Octet1, Octet2, Octet3, Octet4);

				// Return string
				return (IPString.ToString());
			}
			catch
			{
				return (null);
			}
		}
		#endregion

	}
	
}
