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
	/// Summary for frmSettingsUser
	/// </summary>
	public class frmSettingsUser : System.Windows.Forms.Form
	{
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Label lblMailName;
		private System.Windows.Forms.TextBox edtMailName;
		private System.Windows.Forms.Button btnApply;
		private System.Windows.Forms.GroupBox grpUser;
		private System.Windows.Forms.Button btnRebuild;
		private TGDataGrid dtgUserInfo;

		// Database access constants
//		private const string		DBS_MODE			= "Mode=ReadWrite";
		private const string		QRY_USERIDNAME		= "qryUserIDName";
		private const string		QRY_USERINFO		= "qryUserInfo";

		// Database table constants
		private const string		COL_USERID			= "UserID";
		private const string		COL_USERNAME		= "UserName";
		private const string		COL_FULLNAME		= "FullName";
		private const string		COL_DESCRIPTION		= "Description";
		private const string		COL_USERPASSWORD	= "UserPassword";
		private const string		COL_DISABLED		= "Disabled";

		// Datagrid header text
		private const string		HDR_DESCRIPTION		= "Description";
		private const string		HDR_FULLNAME		= "Full Name";
		private const string		HDR_USERPASSWORD	= "User Password";
		private const string		HDR_DISABLED		= "Disabled";

		// Component members
		private Controller			m_Controller		= null;
		private DataSet				m_dsUser			= null;		// Current User Name dataset
		private	OleDbDataAdapter	m_adUser			= null;		// Adapter for UserID and Name
		private	OleDbDataAdapter	m_adInfo			= null;		// Adapter for User information
		private	int					m_UserID			= 0;		// Primary index for table updates

		// Message constants
		private const string		MSG_ALERT			= "Save Change Alert";
		private const string		MSG_SAVECHANGES		= "Do you want to save your changes?";
		private const string		MSG_ENTRYERRORS		= "Sorry, some of the data fields are invalid. Please check again.";

		#region Constructors / Destructors
		public frmSettingsUser()
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
			this.grpUser = new System.Windows.Forms.GroupBox();
			this.dtgUserInfo = new TGPController.TGDataGrid();
			this.lblMailName = new System.Windows.Forms.Label();
			this.edtMailName = new System.Windows.Forms.TextBox();
			this.btnApply = new System.Windows.Forms.Button();
			this.btnRebuild = new System.Windows.Forms.Button();
			this.grpUser.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dtgUserInfo)).BeginInit();
			this.SuspendLayout();
			// 
			// grpUser
			// 
			this.grpUser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.grpUser.Controls.Add(this.dtgUserInfo);
			this.grpUser.Controls.Add(this.lblMailName);
			this.grpUser.Controls.Add(this.edtMailName);
			this.grpUser.Controls.Add(this.btnApply);
			this.grpUser.Controls.Add(this.btnRebuild);
			this.grpUser.Location = new System.Drawing.Point(0, 0);
			this.grpUser.Name = "grpUser";
			this.grpUser.Size = new System.Drawing.Size(398, 358);
			this.grpUser.TabIndex = 0;
			this.grpUser.TabStop = false;
			this.grpUser.Text = "User Settings";
			// 
			// dtgUserInfo
			// 
			this.dtgUserInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.dtgUserInfo.BackgroundColor = System.Drawing.SystemColors.Window;
			this.dtgUserInfo.CaptionVisible = false;
			this.dtgUserInfo.DataMember = "";
			this.dtgUserInfo.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dtgUserInfo.Location = new System.Drawing.Point(20, 108);
			this.dtgUserInfo.Name = "dtgUserInfo";
			this.dtgUserInfo.Size = new System.Drawing.Size(362, 174);
			this.dtgUserInfo.TabIndex = 34;
			// 
			// lblMailName
			// 
			this.lblMailName.Location = new System.Drawing.Point(16, 48);
			this.lblMailName.Name = "lblMailName";
			this.lblMailName.Size = new System.Drawing.Size(64, 16);
			this.lblMailName.TabIndex = 30;
			this.lblMailName.Text = "User Name";
			this.lblMailName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// edtMailName
			// 
			this.edtMailName.Enabled = false;
			this.edtMailName.Location = new System.Drawing.Point(80, 48);
			this.edtMailName.Name = "edtMailName";
			this.edtMailName.Size = new System.Drawing.Size(168, 20);
			this.edtMailName.TabIndex = 31;
			this.edtMailName.Text = "";
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
			// btnRebuild
			// 
			this.btnRebuild.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnRebuild.Location = new System.Drawing.Point(248, 320);
			this.btnRebuild.Name = "btnRebuild";
			this.btnRebuild.Size = new System.Drawing.Size(136, 23);
			this.btnRebuild.TabIndex = 50;
			this.btnRebuild.Text = "&Rebuild Analyzer DB";
			this.btnRebuild.Visible = false;
			this.btnRebuild.Click += new System.EventHandler(this.btnRebuild_Click);
			// 
			// frmSettingsUser
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(398, 358);
			this.Controls.Add(this.grpUser);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "frmSettingsUser";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Resize += new System.EventHandler(this.frmSettingsUser_Resize);
			this.Closing += new System.ComponentModel.CancelEventHandler(this.frmSettingsUser_Closing);
			this.grpUser.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dtgUserInfo)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		#region Initialize
		internal bool Initialize (Controller Controller, string UserName)
		{
			// Assume success
			bool bSuccess = true;

			try
			{
				// Set pointer to parent instance
				m_Controller	= Controller;

				// Create new datasets
				m_dsUser = new DataSet();

				// Select the UserName
				StringBuilder Select = new StringBuilder();
				Select.AppendFormat ("SELECT DISTINCT [{0}], [{1}] FROM {2}", COL_USERID, COL_USERNAME, QRY_USERIDNAME);

				// Set up the WHERE clause (end with ';')
				StringBuilder Where = new StringBuilder();
				Where.AppendFormat (" WHERE [{0}]='{1}';", COL_USERNAME, UserName);
				string SQL = Select.Append(Where).ToString();

				// Get the UserID and UserName
				m_adUser = m_Controller.DBCfgQuery.ADCreate ();
				bSuccess &= m_Controller.DBCfgQuery.ADSelect (m_adUser, ref m_dsUser, SQL, QRY_USERIDNAME);
				if (!bSuccess) return (false);

				// Get the UserID index value for use with queries
				m_UserID = Convert.ToInt32(m_dsUser.Tables[QRY_USERIDNAME].Rows[0][COL_USERID]);

				// Bind the textbox controls to retrieve the entries
				DataTable dtTbl = m_dsUser.Tables[QRY_USERIDNAME];
				edtMailName.DataBindings.Add("Text", dtTbl, COL_USERNAME);

				// Select the UserInfo entries
				Select = new StringBuilder();
				Select.AppendFormat ("SELECT [{0}], [{1}], [{2}], [{3}], [{4}] FROM {5}", COL_USERID, COL_USERPASSWORD, COL_FULLNAME, COL_DESCRIPTION, COL_DISABLED, QRY_USERINFO);

				// Set up the WHERE clause (end with ';')
				Where = new StringBuilder();
				Where.AppendFormat (" WHERE [{0}]={1};", COL_USERID, m_UserID);
				SQL = Select.Append(Where).ToString();

				// Get the UserInfo information entries (may be 0 entries)
				m_adInfo = m_Controller.DBCfgQuery.ADCreate ();
				bSuccess &= m_Controller.DBCfgQuery.ADSelect (m_adInfo, ref m_dsUser, SQL, QRY_USERINFO);
				if (!bSuccess) return (false);

				// Bind the data grid data
				dtTbl = m_dsUser.Tables[QRY_USERINFO];
				dtgUserInfo.SetDataBinding(m_dsUser, dtTbl.TableName);

				// Disable add/delete for categories in this release
				((DataView)(((CurrencyManager)(dtgUserInfo.BindingContext[dtgUserInfo.DataSource, 
					dtgUserInfo.DataMember])).List)).AllowNew = false;
				((DataView)(((CurrencyManager)(dtgUserInfo.BindingContext[dtgUserInfo.DataSource, 
					dtgUserInfo.DataMember])).List)).AllowDelete = false;
				
				// Hide the UserID index entry
				GridStyleSet(dtgUserInfo, dtTbl.TableName);

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
			boolCol.MappingName = COL_DISABLED;
			boolCol.HeaderText = HDR_DISABLED;
			boolCol.Width = WID_CHECKBOX;
			((DataGridBoolColumn)boolCol).AllowNull = false;	// Disable tri-state
			ts1.GridColumnStyles.Add(boolCol);
			RemainingWidth -= boolCol.Width;
      
			// Add a column style.
			DataGridTextBoxColumn TextCol = new DataGridTextBoxColumn();
			TextCol.MappingName = COL_FULLNAME;
			TextCol.HeaderText = HDR_FULLNAME;
			TextCol.Width = (RemainingWidth * 1) / 3;
			ts1.GridColumnStyles.Add(TextCol);

			// Add to the column style.
			TextCol = new DataGridTextBoxColumn();
			TextCol.MappingName = COL_USERPASSWORD;
			TextCol.HeaderText = HDR_USERPASSWORD;
			TextCol.Width = (RemainingWidth * 1) / 3;
			ts1.GridColumnStyles.Add(TextCol);

			// Add to the column style.
			TextCol = new DataGridTextBoxColumn();
			TextCol.MappingName = COL_DESCRIPTION;
			TextCol.HeaderText = HDR_DESCRIPTION;
			TextCol.Width = (RemainingWidth * 1) / 3;
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

			// Get the column styles and remove width for boolean columns
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
				if (ColumnStyle.MappingName == COL_FULLNAME) ColumnStyle.Width = (RemainingWidth * 1) / 3;
				if (ColumnStyle.MappingName == COL_USERPASSWORD) ColumnStyle.Width = (RemainingWidth * 1) / 3;
				if (ColumnStyle.MappingName == COL_DESCRIPTION) ColumnStyle.Width = (RemainingWidth * 1) / 3;
			}
		}
		#endregion

		#region IsDirty
		private bool IsDirty()
		{
			try
			{
				// Don't try to commit the row if there isn't one
				if (dtgUserInfo.DataSource == null) return (false);

				// .Net Bug? When you databind to a textbox, it does not update the 
				// underlying data source until the focus on the textbox is lost.
				// This is a long-standing Windows convention. The editing of data in a visual
				// control (data-aware or not) is not final until focus is moved from that
				// control. Until that happens, the end-user may still press the Escape key
				// and the changes to the control's value are undone. This undo functionality
				// is intrinsic to Windows, not the application.
				// Note: This solution is inconsistent. If I click the last MailFrom
				// checkbox of a multiple row, the change is not detected on form close.
				// Single or upper rows are OK. (AJM 10/19/2004)
				// Call EndEdit() on the current datagrid row
				dtgUserInfo.EndEdit (null, dtgUserInfo.CurrentRowIndex, false);

				// There is no event from the DataGrid on insert; check at the end
				DataGridEndCurrentEdit(m_dsUser, QRY_USERINFO, COL_USERID, m_UserID);

				// Update the dataset with the contents of the text boxes
				// This will mark the dataset as "HasCHanges" for the text box entries
				BindingManagerBase BMan = (CurrencyManager)this.BindingContext[m_dsUser.Tables[QRY_USERIDNAME]];
				BMan.EndCurrentEdit();

				// Check if anything has changed
				// Note: This solution is inconsistent. If I click the last User
				// checkbox of a multiple row, the change is not detected on form close.
				// Single or upper rows are OK. (AJM 10/19/2004)
				return (m_dsUser.HasChanges());
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
					if (m_dsUser.HasErrors)
					{
						MessageBox.Show(MSG_ENTRYERRORS, MSG_ALERT, MessageBoxButtons.OK);
						return;
					}

					// Write the data back to the database
					m_adUser.Update(m_dsUser, QRY_USERIDNAME);
					m_adInfo.Update(m_dsUser, QRY_USERINFO);

					// Accept the changes to clear the HasChanges() flag
					m_dsUser.AcceptChanges();
				}
			}
			catch(Exception ex)
			{
				m_Controller.FireLogException(ex, null, null);
			}
		}
		#endregion

		#region frmSettingsUser_Closing
		private void frmSettingsUser_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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

		#region frmSettingsUser_Resize
		private void frmSettingsUser_Resize(object sender, System.EventArgs e)
		{
			GridStyleReSize(dtgUserInfo);
		}
		#endregion

		#region btnRebuild_Click
		private void btnRebuild_Click(object sender, System.EventArgs ev)
		{
			// Load Rebuild Wizard
			frmAnalyzerRebuild frmAnalyzerRebuild = new frmAnalyzerRebuild(this.Text, this.Icon);
			frmAnalyzerRebuild.Initialize(m_Controller);
			frmAnalyzerRebuild.ShowDialog();
		}
		#endregion

	}
}
