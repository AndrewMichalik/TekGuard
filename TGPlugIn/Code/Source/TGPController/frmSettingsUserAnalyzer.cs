using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TGPConnector;

namespace TGPController
{
	/// <summary>
	/// Summary for frmSettingsGlobal
	/// </summary>
	public class frmSettingsUserAnalyzer : System.Windows.Forms.Form
	{
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Button btnApply;
		private System.Windows.Forms.GroupBox grpAnalyzer;
		private System.Windows.Forms.Label lblTokenCount;
		private System.Windows.Forms.TextBox edtTokenCount;
		private System.Windows.Forms.Label lblDayKeepCount;
		private System.Windows.Forms.TextBox edtDayKeepCount;
		private System.Windows.Forms.Label lblLogAnalyzerRes;
		private System.Windows.Forms.CheckBox cbxLogAnalyzerRes;
		private TGPController.TGDataGrid dtgMsgCategory;

		// Component members
		private Controller			m_Controller		= null;		// Pointer to parent TGPController instance
		private	OleDbDataAdapter	m_adMCat			= null;		// Adapter for Mailboxes & Categories
		private DataSet				m_dsMCat			= null;		// Current Mailboxes & Categories dataset

		// Database Application Settings constants
		private const string		QRY_SETTINGS		= "qryApplicationSettings";
		private const string		COL_TOKENCOUNT		= "TokenCount";
		private const string		COL_DAYKEEPCOUNT	= "DayKeepCount";
		private const string		COL_LOGRESULTS		= "LogAnalyzerRes";
		private const string		COL_SPLITTERREGEX	= "SplitterRegEx";

		// Database Message Category constants
		private const string		QRY_MSGCATEGORY		= "qryMessageCategory";
		private const string		COL_CATEGORYID		= "CategoryID";
		private const string		COL_CATEGORYNAME	= "CategoryName";
		private const string		COL_MAILBOXNAME		= "MailboxName";
		private const string		COL_BUTTONNAME		= "ButtonName";
		private const string		COL_THRESHOLD		= "Threshold";
		private const string		COL_AUTOMOVE		= "AutoMove";
		private const string		COL_AUTOREAD		= "AutoRead";

		// Datagrid header text
		private const string		HDR_CATEGORYNAME	= "Category";
		private const string		HDR_MAILBOXNAME		= "Mailbox";
		private const string		HDR_BUTTONNAME		= "Button";
		private const string		HDR_THRESHOLD		= "Threshold";
		private const string		HDR_AUTOMOVE		= "Move";
		private const string		HDR_AUTOREAD		= "Read";

		// Message constants
		private const string		MSG_SAVEALERT		= "Save Change Alert";
		private const string		MSG_SAVECHANGES		= "Do you want to save your changes?";
		private const string		MSG_ENTRYERRORS		= "Sorry, some of the data fields are invalid. Please check again.";

		#region Constructors / Destructors
		public frmSettingsUserAnalyzer()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}
		#endregion

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.btnApply = new System.Windows.Forms.Button();
			this.grpAnalyzer = new System.Windows.Forms.GroupBox();
			this.lblTokenCount = new System.Windows.Forms.Label();
			this.edtTokenCount = new System.Windows.Forms.TextBox();
			this.lblLogAnalyzerRes = new System.Windows.Forms.Label();
			this.cbxLogAnalyzerRes = new System.Windows.Forms.CheckBox();
			this.lblDayKeepCount = new System.Windows.Forms.Label();
			this.edtDayKeepCount = new System.Windows.Forms.TextBox();
			this.dtgMsgCategory = new TGPController.TGDataGrid();
			this.grpAnalyzer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dtgMsgCategory)).BeginInit();
			this.SuspendLayout();
			// 
			// btnApply
			// 
			this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnApply.Location = new System.Drawing.Point(8, 326);
			this.btnApply.Name = "btnApply";
			this.btnApply.TabIndex = 26;
			this.btnApply.Text = "&Apply";
			this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
			// 
			// grpAnalyzer
			// 
			this.grpAnalyzer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.grpAnalyzer.Controls.Add(this.lblTokenCount);
			this.grpAnalyzer.Controls.Add(this.edtTokenCount);
			this.grpAnalyzer.Controls.Add(this.lblLogAnalyzerRes);
			this.grpAnalyzer.Controls.Add(this.cbxLogAnalyzerRes);
			this.grpAnalyzer.Controls.Add(this.lblDayKeepCount);
			this.grpAnalyzer.Controls.Add(this.edtDayKeepCount);
			this.grpAnalyzer.Controls.Add(this.dtgMsgCategory);
			this.grpAnalyzer.Controls.Add(this.btnApply);
			this.grpAnalyzer.Location = new System.Drawing.Point(0, 0);
			this.grpAnalyzer.Name = "grpAnalyzer";
			this.grpAnalyzer.Size = new System.Drawing.Size(398, 358);
			this.grpAnalyzer.TabIndex = 0;
			this.grpAnalyzer.TabStop = false;
			this.grpAnalyzer.Text = "Anayzer Settings";
			// 
			// lblTokenCount
			// 
			this.lblTokenCount.Location = new System.Drawing.Point(16, 32);
			this.lblTokenCount.Name = "lblTokenCount";
			this.lblTokenCount.Size = new System.Drawing.Size(88, 16);
			this.lblTokenCount.TabIndex = 54;
			this.lblTokenCount.Text = "Token Count";
			this.lblTokenCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// edtTokenCount
			// 
			this.edtTokenCount.Location = new System.Drawing.Point(144, 32);
			this.edtTokenCount.Name = "edtTokenCount";
			this.edtTokenCount.Size = new System.Drawing.Size(40, 20);
			this.edtTokenCount.TabIndex = 55;
			this.edtTokenCount.Text = "";
			// 
			// lblLogAnalyzerRes
			// 
			this.lblLogAnalyzerRes.Location = new System.Drawing.Point(16, 96);
			this.lblLogAnalyzerRes.Name = "lblLogAnalyzerRes";
			this.lblLogAnalyzerRes.Size = new System.Drawing.Size(120, 16);
			this.lblLogAnalyzerRes.TabIndex = 50;
			this.lblLogAnalyzerRes.Text = "Log Analyzer Results:";
			this.lblLogAnalyzerRes.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// cbxLogAnalyzerRes
			// 
			this.cbxLogAnalyzerRes.Location = new System.Drawing.Point(144, 88);
			this.cbxLogAnalyzerRes.Name = "cbxLogAnalyzerRes";
			this.cbxLogAnalyzerRes.Size = new System.Drawing.Size(16, 32);
			this.cbxLogAnalyzerRes.TabIndex = 51;
			// 
			// lblDayKeepCount
			// 
			this.lblDayKeepCount.Location = new System.Drawing.Point(16, 64);
			this.lblDayKeepCount.Name = "lblDayKeepCount";
			this.lblDayKeepCount.Size = new System.Drawing.Size(120, 16);
			this.lblDayKeepCount.TabIndex = 52;
			this.lblDayKeepCount.Text = "Day Keep Count:";
			this.lblDayKeepCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// edtDayKeepCount
			// 
			this.edtDayKeepCount.Location = new System.Drawing.Point(144, 64);
			this.edtDayKeepCount.Name = "edtDayKeepCount";
			this.edtDayKeepCount.Size = new System.Drawing.Size(40, 20);
			this.edtDayKeepCount.TabIndex = 53;
			this.edtDayKeepCount.Text = "";
			// 
			// dtgMsgCategory
			// 
			this.dtgMsgCategory.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.dtgMsgCategory.BackgroundColor = System.Drawing.SystemColors.Window;
			this.dtgMsgCategory.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.dtgMsgCategory.CaptionVisible = false;
			this.dtgMsgCategory.DataMember = "";
			this.dtgMsgCategory.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dtgMsgCategory.Location = new System.Drawing.Point(8, 128);
			this.dtgMsgCategory.Name = "dtgMsgCategory";
			this.dtgMsgCategory.Size = new System.Drawing.Size(382, 156);
			this.dtgMsgCategory.TabIndex = 48;
			this.dtgMsgCategory.Resize += new System.EventHandler(this.dtgMsgCategory_Resize);
			// 
			// frmSettingsUserAnalyzer
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(398, 358);
			this.Controls.Add(this.grpAnalyzer);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "frmSettingsUserAnalyzer";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Closing += new System.ComponentModel.CancelEventHandler(this.frmSettingsGlobal_Closing);
			this.grpAnalyzer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dtgMsgCategory)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		#region Initialize
		internal bool Initialize (Controller Controller, string xUserName)
		{
			// Assume success
			bool bSuccess = true;

			// Set pointer to parent instance
			m_Controller = Controller;

			try
			{
				// Load the application settings
				DataRow drRow = m_Controller.DBAnaQuery.DSSelectRow ("SELECT * FROM " + QRY_SETTINGS);

				// Retrieve the entries; load the form elements
				m_Controller.DBData.Get (edtTokenCount, drRow, COL_TOKENCOUNT);
				m_Controller.DBData.Get (edtDayKeepCount, drRow, COL_DAYKEEPCOUNT);
				m_Controller.DBData.Get (cbxLogAnalyzerRes, drRow, COL_LOGRESULTS);

				// Create a new dataset for the Message Categories
				m_dsMCat = new DataSet();

				// Build the Message Category Select clause
				StringBuilder Select = new StringBuilder();
				string SQL = Select.AppendFormat ("SELECT [{0}], [{1}], [{2}], [{3}], [{4}], [{5}], [{6}] FROM {7}", COL_CATEGORYID, COL_CATEGORYNAME, COL_MAILBOXNAME, COL_BUTTONNAME, COL_THRESHOLD, COL_AUTOMOVE, COL_AUTOREAD, QRY_MSGCATEGORY).ToString();

				// Get the Category/Mailbox information entries (may be 0 entries)
				m_adMCat = m_Controller.DBAnaQuery.ADCreate ();
				bSuccess &= m_Controller.DBAnaQuery.ADSelect (m_adMCat, ref m_dsMCat, SQL, QRY_MSGCATEGORY);
				if (!bSuccess) return (false);

				// Bind the data grid data
				DataTable dtTbl = m_dsMCat.Tables[QRY_MSGCATEGORY];
				dtgMsgCategory.SetDataBinding(m_dsMCat, dtTbl.TableName);

				// Disable add/delete for categories in this release
				((DataView)(((CurrencyManager)(dtgMsgCategory.BindingContext[dtgMsgCategory.DataSource, 
					dtgMsgCategory.DataMember])).List)).AllowNew = false;
				((DataView)(((CurrencyManager)(dtgMsgCategory.BindingContext[dtgMsgCategory.DataSource, 
					dtgMsgCategory.DataMember])).List)).AllowDelete = false;
				
				// Hide the UserID index entry
				GridStyleSet(dtgMsgCategory, dtTbl.TableName);

				// Return success
				return (true);
			}
			catch(Exception ex)
			{
				m_Controller.FireLogException(ex, null, null);
				return (false);
			}
		}
		#endregion

		#region GridStyleSet
		private void GridStyleSet(TGDataGrid dtgGrid, string TableName)
		{
			// Set style table name mapping
			DataGridTableStyle ts1 = new DataGridTableStyle();
			ts1.MappingName = TableName;
			// Set other style properties.
			ts1.AlternatingBackColor = Color.LightGray;

			// Determine size of inner rectangle
			int RemainingWidth = dtgGrid.DGClientWidth;

			// Add a GridColumnStyle and set its MappingName to the name of a
			// DataColumn in the DataTable. Set the HeaderText and Width properties.
			// ajm - should be able to determine the minimum checkbox size programatically
			const int WID_CHECKBOX = 50;
			DataGridColumnStyle boolCol = new DataGridBoolColumn();
			boolCol.MappingName = COL_AUTOMOVE;
			boolCol.HeaderText = HDR_AUTOMOVE;
			boolCol.Width = WID_CHECKBOX;
			((DataGridBoolColumn)boolCol).AllowNull = false;	// Disable tri-state
			ts1.GridColumnStyles.Add(boolCol);
			RemainingWidth -= boolCol.Width;

			boolCol = new DataGridBoolColumn();
			boolCol.MappingName = COL_AUTOREAD;
			boolCol.HeaderText = HDR_AUTOREAD;
			boolCol.Width = WID_CHECKBOX;
			((DataGridBoolColumn)boolCol).AllowNull = false;	// Disable tri-state
			ts1.GridColumnStyles.Add(boolCol);
			RemainingWidth -= boolCol.Width;

			// Add a column style.
			DataGridTextBoxColumn TextCol = new DataGridTextBoxColumn();
			TextCol.MappingName = COL_CATEGORYNAME;
			TextCol.HeaderText = HDR_CATEGORYNAME;
			TextCol.Width = (RemainingWidth * 1) / 4;
			ts1.GridColumnStyles.Add(TextCol);

			// Add to the column style.
			TextCol = new DataGridTextBoxColumn();
			TextCol.MappingName = COL_MAILBOXNAME;
			TextCol.HeaderText = HDR_MAILBOXNAME;
			TextCol.Width = (RemainingWidth * 1) / 4;
			ts1.GridColumnStyles.Add(TextCol);

			// Add to the column style.
			TextCol = new DataGridTextBoxColumn();
			TextCol.MappingName = COL_BUTTONNAME;
			TextCol.HeaderText = HDR_BUTTONNAME;
			TextCol.Width = (RemainingWidth * 1) / 4;
			ts1.GridColumnStyles.Add(TextCol);

			// Add to the column style.
			TextCol = new DataGridTextBoxColumn();
			TextCol.MappingName = COL_THRESHOLD;
			TextCol.HeaderText = HDR_THRESHOLD;
			TextCol.Width = (RemainingWidth * 1) / 4;
			ts1.GridColumnStyles.Add(TextCol);

			// Add the DataGridTableStyle instances to the GridTableStylesCollection.
			dtgGrid.TableStyles.Add(ts1);
		}
		#endregion

		#region GridStyleReSize
		public void GridStyleReSize(TGDataGrid dtgGrid)
		{
			// Determine size of inner rectangle
			int RemainingWidth = dtgGrid.DGClientWidth;
			
			// Get current style
			DataGridTableStyle TableStyle = dtgGrid.TableStyles[0];

			// Get the column styles and remove fixed width for boolean columns
			foreach (DataGridColumnStyle ColumnStyle in TableStyle.GridColumnStyles)
			{
				if (ColumnStyle.GetType() == typeof(DataGridBoolColumn))
				{
					RemainingWidth -= ColumnStyle.Width;
				}
			}

			// Get the column styles and split the remaining space for the text columns
			foreach (DataGridColumnStyle ColumnStyle in TableStyle.GridColumnStyles)
			{
				if (ColumnStyle.GetType() == typeof(DataGridTextBoxColumn))
				{
					ColumnStyle.Width = (RemainingWidth * 1) / 4;
				}
			}
		}
		#endregion

		#region IsDirty
		private bool IsDirty()
		{
			// Apply changes?
			bool bDirty = false;

			try
			{
				// Load the form elements
				DataRow drRow = m_Controller.DBAnaQuery.DSSelectRow ("SELECT * FROM " + QRY_SETTINGS);

				// Check if anything has changed
				bDirty |= m_Controller.DBData.Check (edtTokenCount, drRow, COL_TOKENCOUNT);
				bDirty |= m_Controller.DBData.Check (edtDayKeepCount, drRow, COL_DAYKEEPCOUNT);
				bDirty |= m_Controller.DBData.Check (cbxLogAnalyzerRes, drRow, COL_LOGRESULTS);

				// Don't try to commit the row if there isn't one
				if (dtgMsgCategory.DataSource != null) 
				{
					// Call EndEdit() on the current datagrid row
					dtgMsgCategory.EndEdit (null, dtgMsgCategory.CurrentRowIndex, false);

					// There is no event from the DataGrid on insert; check at the end
					// DataGridEndCurrentEdit(m_dsMCat, QRY_MSGCATEGORY, COL_USERID, m_UserID);

					// Check if anything has changed
					// Note: These solutions are inconsistent. If I click the last Category
					// checkbox of a multiple row, the change is not detected on form close.
					// Single or upper rows are OK. (AJM 10/19/2004)
					// bDirty |= (null != m_dsMCat.GetChanges());
					bDirty |= m_dsMCat.HasChanges();
				}

				// Changes?
				return (bDirty);

			}
			catch(Exception ex)
			{
				m_Controller.FireLogException(ex, null, null);
				return (false);
			}
		}
		private void xDataGridEndCurrentEdit(DataSet dsSet, string TableName, string IndexName, int IndexValue)
		{
			// AJM: This function will be used to generate Category ID's
			// There is no event from the DataGrid on insert; check at the end
			for (int ii=0; ii<dsSet.Tables[TableName].Rows.Count; ii++)
			{
				DataRow dtRow = dsSet.Tables[TableName].Rows[ii];
				if (dtRow.RowState == DataRowState.Deleted) continue;
				if (dtRow[IndexName] == DBNull.Value)
				{
					dtRow[IndexName] = IndexValue;
				}
			}

			// Call EndCurrentEdit() on the binding context
			BindingManagerBase BMan = (CurrencyManager)this.BindingContext[dsSet.Tables[TableName]];
			BMan.EndCurrentEdit();
		}
		#endregion

		#region btnApply_Click
		private void btnApply_Click(object sender, System.EventArgs ev)
		{
			try
			{
				if (IsDirty())
				{
					// Create the global settings
					DataSet dsSettings = new DataSet();

					// Select the global settings
					string SQL = "SELECT * FROM " + QRY_SETTINGS;
				
					// Create the adapter and command builder
					OleDbDataAdapter adSettings = m_Controller.DBAnaQuery.ADCreate ();
					m_Controller.DBAnaQuery.ADSelect (adSettings, ref dsSettings, SQL, QRY_SETTINGS);
				
					// Get the global settings row
					DataRow drRow = (dsSettings.Tables[0]).Rows[0];

					// Update the entries
					m_Controller.DBData.Set (edtTokenCount, drRow, COL_TOKENCOUNT);
					m_Controller.DBData.Set (edtDayKeepCount, drRow, COL_DAYKEEPCOUNT);
					m_Controller.DBData.Set (cbxLogAnalyzerRes, drRow, COL_LOGRESULTS);

					// Any data violations?
					if (dsSettings.HasErrors)
					{
						MessageBox.Show(MSG_ENTRYERRORS, MSG_SAVEALERT, MessageBoxButtons.OK);
						return;
					}

					// Write the data back to the database
					adSettings.Update(dsSettings);

					// Accept the changes to clear the HasChanges() flag
					dsSettings.AcceptChanges();

					// Any data violations?
					if ((m_dsMCat.HasErrors))
					{
						MessageBox.Show(MSG_ENTRYERRORS, MSG_SAVEALERT, MessageBoxButtons.OK);
						return;
					}

					// AJM 10/19/04: I tried every possible combination of calls to get
					//   the databound textbox to update the underlying dataset when remoted.
					// Merge the Analyzer remote dataset changes
					// DataSet dsAnal = new DataSet();
					// m_Controller.DBAnaQuery.ADSelect (m_adAnal, ref dsAnal, "SELECT * FROM " + QRY_ANALYZER, QRY_ANALYZER);
					// DataRow drRow = m_Controller.DBAnaQuery.DSSelectRow("SELECT * FROM " + QRY_ANALYZER, null);
					// DataRow drRow = (dsAnal.Tables[0]).Rows[0];
					// m_Controller.DBData.Set (edtTokenCount, drRow, COL_TOKENCOUNT);
					// DataSet dsTemp = m_dsMCat.GetChanges();
					// if (dsTemp != null) m_dsMCat.Merge(dsTemp);
					// m_adAnal.Update(dsAnal);
					// DataRow drrRow = m_Controller.DBAnaQuery.DSSelectRow("SELECT * FROM " + QRY_ANALYZER, m_adAnal);
					// m_Controller.DBData.Set (edtTokenCount, drrRow, COL_TOKENCOUNT);
					// m_adAnal.Update(m_dsMCat, QRY_ANALYZER);
					// m_Controller.DBAnaQuery.ADUpdate(m_adAnal, m_dsMCat, QRY_ANALYZER);
					// Write the data back to the database (1st table)
					// m_adAnal.Update(m_dsMCat, QRY_ANALYZER);

					// Write the data back to the database (1st table)
					// m_Controller.DBAnaQuery.DSUpdate ("SELECT * FROM " + QRY_ANALYZER, m_adAnal, QRY_ANALYZER, COL_TOKENCOUNT, 0, edtTokenCount.Text.ToString());

					// Write the data back to the database (1st table)
					// m_adAnal.Update(m_dsMCat, QRY_ANALYZER);

					// Write the data back to the database (2nd table)
					m_adMCat.Update(m_dsMCat, QRY_MSGCATEGORY);

					// Accept the changes to clear the HasChanges() flag
					m_dsMCat.AcceptChanges();

					// Notify the main program that the configuration has changed
					// Outlook objects created under "wrong" thread (STA and MTA have no effect)
					// cause this next line to hang Outlook Garbage Collection on closing.
					// The main Outlook entry point must provide a thread for this call.
					m_Controller.DBAnaQuery.ConfigurationApply(this, null);

					// Refresh the data grid to insure that the UserID autonumber is
					// properly reflected. Otherwise, any child nodes will appear
					// to have disappeared because the grid has the old autonumber
					// m_dsMCat.AcceptChanges();
					// dtgMsgCategory.Refresh();
				}
			}
			catch(Exception ex)
			{
				m_Controller.FireLogException(ex, null, null);
			}
		}
		#endregion

		#region frmSettingsGlobal_Closing
		private void frmSettingsGlobal_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			// Any controls changed?
			if (IsDirty()) 
			{
				DialogResult Result = MessageBox.Show("Save Change Alert", "Do you want to save your changes?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
				if (Result == DialogResult.Yes)
				{
					btnApply_Click(this, null);
				} 
				if (Result == DialogResult.Cancel)
				{
					e.Cancel = true;
				}
				// Reset focus so as not to lose the close button (x) when clearing this
				// sub-form from the container control
				this.Focus();
			}
			return;		
		}
		#endregion

		#region dtgMsgCategory_Resize
		private void dtgMsgCategory_Resize(object sender, System.EventArgs e)
		{
			GridStyleReSize(dtgMsgCategory);
		}
		#endregion

	}
}
