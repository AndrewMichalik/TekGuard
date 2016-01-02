using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Windows.Forms;
using TGPConnector;

namespace TGPController
{
	/// <summary>
	/// Summary for frmSettingsGlobal
	/// </summary>
	public class frmSettingsGlobal : System.Windows.Forms.Form
	{
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Button btnApply;
		private System.Windows.Forms.GroupBox grpMachine;
		private System.Windows.Forms.Label lblRemotePassword;
		private System.Windows.Forms.TextBox edtRemotePassword;

		// Component members
		private Controller			m_Controller		= null;		// Pointer to parent TGPController instance

		// Database access constants
		private const string		TBL_REMOTESERVER	= "tblRemoteServer";

		// Database table constants
		private const string		COL_PASSWORD		= "Password";

		// Message constants
		private const string		MSG_SAVEALERT		= "Save Change Alert";
		private const string		MSG_SAVECHANGES		= "Do you want to save your changes?";
		private const string		MSG_ENTRYERRORS		= "Sorry, some of the data fields are invalid. Please check again.";

		#region Constructors / Destructors
		public frmSettingsGlobal()
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
			this.grpMachine = new System.Windows.Forms.GroupBox();
			this.lblRemotePassword = new System.Windows.Forms.Label();
			this.edtRemotePassword = new System.Windows.Forms.TextBox();
			this.grpMachine.SuspendLayout();
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
			// grpMachine
			// 
			this.grpMachine.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.grpMachine.Controls.Add(this.lblRemotePassword);
			this.grpMachine.Controls.Add(this.edtRemotePassword);
			this.grpMachine.Controls.Add(this.btnApply);
			this.grpMachine.Location = new System.Drawing.Point(0, 0);
			this.grpMachine.Name = "grpMachine";
			this.grpMachine.Size = new System.Drawing.Size(398, 358);
			this.grpMachine.TabIndex = 0;
			this.grpMachine.TabStop = false;
			this.grpMachine.Text = "Machine Settings";
			// 
			// lblRemotePassword
			// 
			this.lblRemotePassword.Location = new System.Drawing.Point(24, 52);
			this.lblRemotePassword.Name = "lblRemotePassword";
			this.lblRemotePassword.Size = new System.Drawing.Size(176, 16);
			this.lblRemotePassword.TabIndex = 41;
			this.lblRemotePassword.Text = "Remote Management Password:";
			this.lblRemotePassword.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// edtRemotePassword
			// 
			this.edtRemotePassword.Location = new System.Drawing.Point(24, 72);
			this.edtRemotePassword.Name = "edtRemotePassword";
			this.edtRemotePassword.Size = new System.Drawing.Size(352, 20);
			this.edtRemotePassword.TabIndex = 42;
			this.edtRemotePassword.Text = "";
			// 
			// frmSettingsGlobal
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(398, 358);
			this.Controls.Add(this.grpMachine);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Name = "frmSettingsGlobal";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Closing += new System.ComponentModel.CancelEventHandler(this.frmSettingsGlobal_Closing);
			this.grpMachine.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Initialize
		internal bool Initialize (Controller Controller)
		{
			// Set pointer to parent instance
			m_Controller = Controller;

			try
			{
				// Load the initialization settings
				DataRow drRow = m_Controller.DBIniQuery.DSGetRow_XML (TBL_REMOTESERVER);

				// Retrieve the entries; load the form elements
				m_Controller.DBData.Get (edtRemotePassword, drRow, COL_PASSWORD);

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

		#region IsDirty
		private bool IsDirty()
		{
			// Apply changes?
			bool bDirty = false;

			try
			{
				// Load the Initialization settings
				DataRow drRow = m_Controller.DBIniQuery.DSGetRow_XML (TBL_REMOTESERVER);

				// Check if anything has changed
				bDirty |= m_Controller.DBData.Check (edtRemotePassword, drRow, COL_PASSWORD);
			}
			catch(Exception ex)
			{
				m_Controller.FireLogException(ex, null, null);
				return (false);
			}

			return (bDirty);
		}
		#endregion

		#region btnApply_Click
		private void btnApply_Click(object sender, System.EventArgs ev)
		{
			try
			{
				// Update the remote management entries (use row function)
				m_Controller.DBIniQuery.DSUpdate_XML(TBL_REMOTESERVER, COL_PASSWORD, 0, edtRemotePassword.Text.ToString());
				m_Controller.DBIniQuery.DSCommit_XML();

				// Update the remote management entries (use row function)
				// DataTable dtTemp = (m_Controller.DBIniQuery.DSGet_XML()).Tables[TBL_REMOTE];
				// m_Controller.DBData.Set (edtRemotePassword, dtTemp.Rows[0], COL_PASSWORD);
				// m_Controller.DBIniQuery.DSMerge_XML(dtTemp);
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

	}
}
