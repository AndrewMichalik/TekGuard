using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Text;
using System.Windows.Forms;
using TGPConnector;

namespace TGPController
{
	/// <summary>
	/// Summary for frmSettingsUserMailFrom
	/// </summary>
	public class frmSettingsUserMailFrom : System.Windows.Forms.Form
	{
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Button btnApply;
		private System.Windows.Forms.GroupBox grpMailFrom;
		private TGPController.TGDataGrid dtgMailFrom;

		// Component members
		private Controller			m_Controller		= null;
		private DataSet				m_dsFrom			= null;		// MailFrom dataset
		private	OleDbDataAdapter	m_adFrom			= null;		// Adapter for MailFrom Analyzer dataset
		private	Int32				m_UserID;						// User ID indicator for audit fields

		// Database access constants
		private const string		QRY_MAILFROM		= "qryMessageMailFrom";

		// Database table constants
		private const string		COL_MFRULEID		= "MFRuleID";
		private const string		COL_CATEGORYID		= "CategoryID";
		private const string		COL_MAILNAME		= "MailName";
		private const string		COL_DOMAINNAME		= "DomainName";
		private const string		COL_LOCKED			= "Locked";
		private const string		COL_DESCRIPTION		= "Description";
		private const string		COL_CREATEDBY		= "CreatedBy";
		private const string		COL_MODIFIEDBY		= "ModifiedBy";

		// Datagrid header text
		private const string		HDR_CATEGORYID		= "Category";
		private const string		HDR_MAILNAME		= "Mail Name";
		private const string		HDR_DOMAINNAME		= "Domain Name";
		private const string		HDR_LOCKED			= "Locked";
		private const string		HDR_DESCRIPTION		= "Description";

		// Message constants
		private const string		MSG_ALERT			= "Save Change Alert";
		private const string		MSG_SAVECHANGES		= "Do you want to save your changes?";
		private const string		MSG_ENTRYERRORS		= "Sorry, some of the data fields are invalid. Please check again.";

		// Temporary constants for user lookup
		private const Int32			USR_DEFAULT			= 1;


		#region Constructors / Destructors
		public frmSettingsUserMailFrom()
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
			this.grpMailFrom = new System.Windows.Forms.GroupBox();
			this.dtgMailFrom = new TGPController.TGDataGrid();
			this.btnApply = new System.Windows.Forms.Button();
			this.grpMailFrom.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dtgMailFrom)).BeginInit();
			this.SuspendLayout();
			// 
			// grpMailFrom
			// 
			this.grpMailFrom.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.grpMailFrom.Controls.Add(this.dtgMailFrom);
			this.grpMailFrom.Controls.Add(this.btnApply);
			this.grpMailFrom.Location = new System.Drawing.Point(0, 0);
			this.grpMailFrom.Name = "grpMailFrom";
			this.grpMailFrom.Size = new System.Drawing.Size(398, 358);
			this.grpMailFrom.TabIndex = 0;
			this.grpMailFrom.TabStop = false;
			this.grpMailFrom.Text = "Mail From Automatic Categorization";
			// 
			// dtgMailFrom
			// 
			this.dtgMailFrom.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.dtgMailFrom.BackgroundColor = System.Drawing.SystemColors.Window;
			this.dtgMailFrom.CaptionVisible = false;
			this.dtgMailFrom.DataMember = "";
			this.dtgMailFrom.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dtgMailFrom.Location = new System.Drawing.Point(8, 32);
			this.dtgMailFrom.Name = "dtgMailFrom";
			this.dtgMailFrom.Size = new System.Drawing.Size(382, 246);
			this.dtgMailFrom.TabIndex = 35;
			// 
			// btnApply
			// 
			this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnApply.Location = new System.Drawing.Point(8, 326);
			this.btnApply.Name = "btnApply";
			this.btnApply.Size = new System.Drawing.Size(72, 24);
			this.btnApply.TabIndex = 27;
			this.btnApply.Text = "Apply";
			this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
			// 
			// frmSettingsUserMailFrom
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(398, 358);
			this.Controls.Add(this.grpMailFrom);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "frmSettingsUserMailFrom";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Resize += new System.EventHandler(this.frmSettingsUserMailFrom_Resize);
			this.grpMailFrom.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dtgMailFrom)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		#region Initialize
		internal bool Initialize (Controller Controller, string UserName)
		{
			// Assume success
			bool bSuccess = true;

			// Set pointer to parent instance
			m_Controller	= Controller;

			// Save User ID for auditing purposes
			// AJM Temp: Need to do a lookup on UserName
			m_UserID = USR_DEFAULT;

			try
			{
				// Create a new dataset
				m_dsFrom = new DataSet();

				// Build the Select clause
				StringBuilder Select = new StringBuilder();
				string SQL = Select.AppendFormat ("SELECT DISTINCT [{0}], [{1}], [{2}], [{3}], [{4}], [{5}], [{6}], [{7}] FROM {8}", COL_MFRULEID, COL_CATEGORYID, COL_MAILNAME, COL_DOMAINNAME, COL_LOCKED, COL_DESCRIPTION, COL_CREATEDBY, COL_MODIFIEDBY, QRY_MAILFROM).ToString();

				// Get the From address MailNames 
				m_adFrom = m_Controller.DBAnaQuery.ADCreate ();
				bSuccess &= m_Controller.DBAnaQuery.ADSelect (m_adFrom, ref m_dsFrom, SQL, QRY_MAILFROM);
				if (!bSuccess) return (false);




// const string		COL_CATEGORYID		= "CategoryID";
const string		COL_CATEGORYNAME	= "CategoryName";
// const string		REL_USERALIASES		= "relUserAliases";
const string		QRY_MSGCATEGORY		= "qryMessageCategory";

// Select the User Categories (may be 0 entries)
Select = new StringBuilder();
SQL = Select.AppendFormat ("SELECT [{0}], [{1}] FROM {2}", COL_CATEGORYID, COL_CATEGORYNAME, QRY_MSGCATEGORY).ToString();

// Get the User Alias MailNames
//AJM 12/2004: Need to retrieve Unknown for 0 entry in order to use relationship
// OleDbDataAdapter m_adMCat = m_Controller.DBAnaQuery.ADCreate ();
// bSuccess &= m_Controller.DBAnaQuery.ADSelect (m_adMCat, ref m_dsFrom, SQL, QRY_MSGCATEGORY);
// if (!bSuccess) return (false);

// Set the relations
//AJM 12/2004: Need to retrieve Unknown for 0 entry in order to use relationship
// m_dsFrom.Relations.Add(REL_USERALIASES, m_dsFrom.Tables[QRY_MSGCATEGORY].Columns[COL_CATEGORYID],
// m_dsFrom.Tables[QRY_MAILFROM].Columns[COL_CATEGORYID]);

				// Bind the MailFrom information to the DataGrid
				dtgMailFrom.SetDataBinding(m_dsFrom, QRY_MAILFROM);

				// Set the style for the MailFrom information
				GridStyleSet(dtgMailFrom, QRY_MAILFROM);

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
		private void GridStyleSet(TGDataGrid dtgGrid, string BaseName)
		{
			// Set style table name mapping
			DataGridTableStyle ts1 = new DataGridTableStyle();
			ts1.MappingName = BaseName;
			// Set other style properties.
			ts1.AlternatingBackColor = Color.LightGray;

			// Determine size of inner rectangle
			int RemainingWidth = dtgGrid.DGClientWidth;
			
			// Add a GridColumnStyle and set its MappingName to the name of a
			// DataColumn in the DataTable. Set the HeaderText and Width properties.    
			// Add a column style.
			// DataGridTextBoxColumn TextCol = new DataGridTextBoxColumn();
			// TextCol.MappingName = COL_CATEGORYID;
			// TextCol.HeaderText = HDR_CATEGORYID;
			// TextCol.Width = (RemainingWidth * 1) / 5;
			// ts1.GridColumnStyles.Add(TextCol);

			// Add to the column style.
CBDataGridColumn CBTextCol = new CBDataGridColumn(new ComboValueChanged(MyComboValueChanged));
			CBTextCol.MappingName = COL_CATEGORYID;
			CBTextCol.HeaderText = HDR_CATEGORYID;
			CBTextCol.Width = (RemainingWidth * 4) / 20;
			ts1.GridColumnStyles.Add(CBTextCol);

			// a) make the row height a little larger to handle minimum combo height
//			ts1.PreferredRowHeight = CBTextCol.ColumnComboBox.Height + 3;
			// b) Populate the combobox somehow. It is a normal combobox, so whatever...
			CBTextCol.ColumnComboBox.Items.Clear();
			CBTextCol.ColumnComboBox.Items.Add("Unknown");
			CBTextCol.ColumnComboBox.Items.Add("Good");
			CBTextCol.ColumnComboBox.Items.Add("Junk");
			// c) set the dropdown style of the combo...
			CBTextCol.ColumnComboBox.DropDownStyle = ComboBoxStyle.DropDownList;

			const int WID_CHECKBOX = 50;
			DataGridColumnStyle boolCol = new DataGridBoolColumn();
			boolCol = new DataGridBoolColumn();
			boolCol.MappingName = COL_LOCKED;
			boolCol.HeaderText = HDR_LOCKED;
			boolCol.Width = WID_CHECKBOX;
			((DataGridBoolColumn)boolCol).AllowNull = false;	// Disable tri-state
			ts1.GridColumnStyles.Add(boolCol);
			RemainingWidth -= boolCol.Width;

			DataGridTextBoxColumn TextCol = new DataGridTextBoxColumn();
			TextCol.MappingName = COL_MAILNAME;
			TextCol.HeaderText = HDR_MAILNAME;
			TextCol.Width = (RemainingWidth * 6) / 20;
			ts1.GridColumnStyles.Add(TextCol);

			// Add to the column style.
			TextCol = new DataGridTextBoxColumn();
			TextCol.MappingName = COL_DOMAINNAME;
			TextCol.HeaderText = HDR_DOMAINNAME;
			TextCol.Width = (RemainingWidth * 6) / 20;
			ts1.GridColumnStyles.Add(TextCol);

			// Add to column style.
			TextCol = new DataGridTextBoxColumn();
			TextCol.MappingName = COL_DESCRIPTION;
			TextCol.HeaderText = HDR_DESCRIPTION;
			TextCol.Width = (dtgGrid.DGClientWidth * 10) / 20;
			ts1.GridColumnStyles.Add(TextCol);

			// Add the DataGridTableStyle instances to the GridTableStylesCollection.
			dtgGrid.TableStyles.Add(ts1);

		}

		private void MyComboValueChanged(int rowChanging, object newValue)
		{
			// Console.WriteLine("index changed {0} {1}", rowChanging, newValue);
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
				if (ColumnStyle.MappingName == COL_CATEGORYID) ColumnStyle.Width = (RemainingWidth * 3) / 20;
				if (ColumnStyle.MappingName == COL_MAILNAME) ColumnStyle.Width = (RemainingWidth * 6) / 20;
				if (ColumnStyle.MappingName == COL_DOMAINNAME) ColumnStyle.Width = (RemainingWidth * 6) / 20;
				if (ColumnStyle.MappingName == COL_DESCRIPTION) ColumnStyle.Width = (RemainingWidth * 10) / 20;
			}
		}
		#endregion

		#region ParseTo
		internal static bool ParseTo (string strTo, out string User, out string Domain)
		{
			// Verify the basic address format
			int Index = strTo.IndexOf('@');
			if ((Index > 0) && !(strTo.EndsWith("@")))
			{
				User	= strTo.Substring(0, Index);
				Domain	= strTo.Substring(Index + 1);
				return true;
			}

			// Bad address
			User = Domain = null;
			return false;
		}
		#endregion

		#region IsDirty
		private bool IsDirty()
		{
			try
			{
				// Don't try to commit the row if there isn't one
				if (dtgMailFrom.DataSource == null) return (false);

				// .Net Bug? When you databind to a textbox, it does not update the 
				// underlying data source until the focus on the textbox is lost.
				// This is a long-standing Windows convention. The editing of data in a visual
				// control (data-aware or not) is not final until focus is moved from that
				// control. Until that happens, the end-user may still press the Escape key
				// and the changes to the control's value are undone. This undo functionality
				// is intrinsic to Windows, not the application.
				// Call EndEdit() on the current datagrid row
				dtgMailFrom.EndEdit (null, dtgMailFrom.CurrentRowIndex, false);

				// There is no event from the DataGrid on insert; check for new record at the end
				DataGridEndCurrentEdit(m_dsFrom, QRY_MAILFROM, COL_CREATEDBY, m_UserID);
				DataGridEndCurrentEdit(m_dsFrom, QRY_MAILFROM, COL_MODIFIEDBY, m_UserID);

				// Check if anything has changed
				// Note: This solution is inconsistent. If I click the last MailFrom
				// checkbox of a multiple row, the change is not detected on form close.
				// Single or upper rows are OK. (AJM 10/19/2004)
				return (m_dsFrom.HasChanges());
			}
			catch(Exception ex)
			{
				m_Controller.FireLogException(ex, null, null);
				return (false);
			}
		}
		private void DataGridEndCurrentEdit(DataSet dsSet, string TableName, string IndexName, int IndexValue)
		{
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
					// Any data violations?
					if ((m_dsFrom.HasErrors))
					{
						MessageBox.Show(MSG_ENTRYERRORS, MSG_ALERT, MessageBoxButtons.OK);
						return;
					}

					// Write the data back to the database (1st dataset)
					m_adFrom.Update(m_dsFrom, QRY_MAILFROM);

					// Accept the changes to clear the HasChanges() flag
					m_dsFrom.AcceptChanges();

					// Refresh the data grid to insure that the UserID autonumber
					// properly reflected. Otherwise, any child nodes will appear
					// to have disappeared because the grid has the old autonumber
					// m_dsFrom.AcceptChanges();
					// dtgMailFrom.Refresh();
				}
			}
			catch(Exception ex)
			{
				m_Controller.FireLogException(ex, null, null);
			}
		}
		#endregion

		#region frmSettingsUserMailFrom_Closing
		private void frmSettingsUserMailFrom_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			// Any controls changed?
			if (IsDirty()) 
			{
				DialogResult Result = MessageBox.Show(MSG_ALERT, MSG_SAVECHANGES, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
				if (Result == DialogResult.Yes)
				{
					btnApply_Click(this, null);
				} 
				if (Result == DialogResult.Cancel)
				{
					e.Cancel = true;
				}
				// .Net Bug?
				// Reset focus so as not to lose the close button (x) when clearing this
				// sub-form from the container control
				this.Focus();
			}
			return;		
		}
		#endregion

		#region frmSettingsUserMailFrom_Resize
		private void frmSettingsUserMailFrom_Resize(object sender, System.EventArgs e)
		{
			GridStyleReSize(dtgMailFrom);
		}
		#endregion

	}
}
