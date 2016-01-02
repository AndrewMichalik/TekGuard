using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Drawing;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using TGPConnector;

namespace TGPController
{
	/// <summary>
	/// Summary for frmSelector
	/// </summary>
	public class frmSelector : System.Windows.Forms.Form
	{
		private System.ComponentModel.IContainer	components;
		private System.Windows.Forms.ImageList		imgNameList;
		private System.Windows.Forms.ContextMenu	ctmMachineList;
		private System.Windows.Forms.MenuItem		mnuMLAddUser;
		private System.Windows.Forms.ContextMenu	ctmUserList;
		private System.Windows.Forms.MenuItem		mnuULDelete;
		private System.Windows.Forms.Panel			pnlContainer;
		private System.Windows.Forms.MenuItem		mnuMLRefresh;
		private System.Windows.Forms.MenuItem		mnuMLDash;
		private System.Windows.Forms.TreeView		trvNameList;

		// Component members
		private Controller			m_Controller		= null;		// Pointer to parent TGPController instance
		private TGWinState		m_windowState;				// Persistent Window State

		// Treeview constants
		private const int			IDX_MACHINENAME		= 0;		// Machine name node
		private const int			IDX_USERNAME		= 1;		// User name node
		private const int			IDX_ANALYZER		= 2;		// Analyzer settings node
		private const int			IDX_PASSLIST		= 3;		// PassList settings node
		private const string		NOD_ANALYZER		= "Analyzer";	// Users node title
		private const string		NOD_MAILFROM		= "Mail From";	// Users node title

		// Database table constants
//		private const string		DBS_MODE			= "Mode=ReadWrite";
		private const string		QRY_USERIDNAME		= "qryUserIDName";
		private const string		COL_USERNAME		= "UserName";
		private const string		COL_WINSTATEDASH	= "WinStateDashboard";

		#region Constructors / Destructors
		public frmSelector()
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
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(frmSelector));
			this.imgNameList = new System.Windows.Forms.ImageList(this.components);
			this.ctmMachineList = new System.Windows.Forms.ContextMenu();
			this.mnuMLAddUser = new System.Windows.Forms.MenuItem();
			this.mnuMLDash = new System.Windows.Forms.MenuItem();
			this.mnuMLRefresh = new System.Windows.Forms.MenuItem();
			this.ctmUserList = new System.Windows.Forms.ContextMenu();
			this.mnuULDelete = new System.Windows.Forms.MenuItem();
			this.pnlContainer = new System.Windows.Forms.Panel();
			this.trvNameList = new System.Windows.Forms.TreeView();
			this.SuspendLayout();
			// 
			// imgNameList
			// 
			this.imgNameList.ImageSize = new System.Drawing.Size(16, 16);
			this.imgNameList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgNameList.ImageStream")));
			this.imgNameList.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// ctmMachineList
			// 
			this.ctmMachineList.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						   this.mnuMLAddUser,
																						   this.mnuMLDash,
																						   this.mnuMLRefresh});
			// 
			// mnuMLAddUser
			// 
			this.mnuMLAddUser.Index = 0;
			this.mnuMLAddUser.Text = "Add User";
			this.mnuMLAddUser.Click += new System.EventHandler(this.mnuMLAddUser_Click);
			// 
			// mnuMLDash
			// 
			this.mnuMLDash.Index = 1;
			this.mnuMLDash.Text = "-";
			// 
			// mnuMLRefresh
			// 
			this.mnuMLRefresh.Index = 2;
			this.mnuMLRefresh.Text = "Refresh";
			this.mnuMLRefresh.Click += new System.EventHandler(this.mnuMLRefresh_Click);
			// 
			// ctmUserList
			// 
			this.ctmUserList.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						this.mnuULDelete});
			// 
			// mnuULDelete
			// 
			this.mnuULDelete.Index = 0;
			this.mnuULDelete.Text = "Delete User";
			this.mnuULDelete.Click += new System.EventHandler(this.mnuULDelete_Click);
			// 
			// pnlContainer
			// 
			this.pnlContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.pnlContainer.Location = new System.Drawing.Point(184, 8);
			this.pnlContainer.Name = "pnlContainer";
			this.pnlContainer.Size = new System.Drawing.Size(398, 358);
			this.pnlContainer.TabIndex = 4;
			// 
			// trvNameList
			// 
			this.trvNameList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left)));
			this.trvNameList.BackColor = System.Drawing.SystemColors.Window;
			this.trvNameList.ImageList = this.imgNameList;
			this.trvNameList.Location = new System.Drawing.Point(8, 8);
			this.trvNameList.Name = "trvNameList";
			this.trvNameList.Size = new System.Drawing.Size(168, 358);
			this.trvNameList.TabIndex = 8;
			this.trvNameList.MouseUp += new System.Windows.Forms.MouseEventHandler(this.trvNameList_MouseUp);
			this.trvNameList.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.trvNameList_AfterSelect);
			this.trvNameList.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.trvNameList_BeforeSelect);
			// 
			// frmSelector
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(592, 373);
			this.Controls.Add(this.trvNameList);
			this.Controls.Add(this.pnlContainer);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "frmSelector";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "TekGuard EMI PlugIn Controller";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.frmSelector_Closing);
			this.ResumeLayout(false);

		}
		#endregion

		#region Initialize
		internal bool Initialize (Controller Controller)
		{
			try
			{
				// Assume success
				bool bSuccess;

				// Get the parent instance information
				m_Controller = Controller;

				// Set the window title and icon
				this.Text = m_Controller.Title;
				this.Icon = m_Controller.Icon;

				// Initialize the Persistent windows state object
				m_windowState = new TGWinState(Controller, this, COL_WINSTATEDASH);

				//Load the trvNameList nodes
				bSuccess = LoadTreeView();

				// Return success
				return (bSuccess);
			}
			catch(Exception ex)
			{
				m_Controller.FireLogException(ex, null, null);
				return (false);
			}
		}
		#endregion

		private bool LoadTreeView()
		{
			// Wipe out of the old nodes
			trvNameList.Nodes.Clear();

			// Start by adding the machine name
			TreeNode NodeRoot = new TreeNode(Environment.MachineName);
			trvNameList.Nodes.Add(NodeRoot);
			NodeRoot.ImageIndex = IDX_MACHINENAME;
			NodeRoot.SelectedImageIndex = IDX_MACHINENAME;

			// Build SELECT clause for UserName information
			StringBuilder Select = new StringBuilder();
			Select.AppendFormat ("SELECT DISTINCT [{0}] FROM {1}", COL_USERNAME, QRY_USERIDNAME);

			// Get UserName information
			DataTable dtTbl = m_Controller.DBCfgQuery.DSSelectTable (Select.ToString());
			if (dtTbl == null) return (false);

			// Note: No UserName info (non-null) may be a normal condition (ex: fresh install)
			// Fill out the tree for all UserNames
			foreach (DataRow drRow in dtTbl.Rows)
			{
				// Add the UserName
				TreeNode UserName = LoadTreeViewNode (NodeRoot, Convert.ToString(drRow[COL_USERNAME]), IDX_USERNAME);

				// Add the user specific nodes
				LoadTreeViewNode (UserName, NOD_ANALYZER, IDX_ANALYZER);
				LoadTreeViewNode (UserName, NOD_MAILFROM, IDX_PASSLIST);
			}

			// Show the tree expanded at the root node by default
			NodeRoot.Expand();

			return (true);
		}
		private TreeNode LoadTreeViewNode (TreeNode Root, string Text, int Level)
		{
			TreeNode NodeNew = new TreeNode(Text);
			Root.Nodes.Add(NodeNew);
			NodeNew.ImageIndex = Level;
			NodeNew.SelectedImageIndex = Level;
			NodeNew.Expand();
			return (NodeNew);
		}

		private void trvNameList_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			// Logic to show right mouse button context menu
			try
			{
				if(e.Button == MouseButtons.Right) 	
				{ 		
					Point ClickPoint = new Point(e.X,e.Y);
					TreeNode CurrentNode = trvNameList.GetNodeAt(ClickPoint);
					if(CurrentNode == null) return;
			
					// Convert from Tree coordinates to Screen
					Point ScreenPoint = trvNameList.PointToScreen(ClickPoint);
				
					// Convert from Screen to Form
					Point FormPoint = this.PointToClient(ScreenPoint);
			
					// Move the focus to the selected node
					trvNameList.SelectedNode = CurrentNode;

					switch (CurrentNode.ImageIndex)
					{
						case IDX_MACHINENAME:
							ctmMachineList.Show(trvNameList,FormPoint);
							break;
						case IDX_USERNAME:
							ctmUserList.Show(trvNameList,FormPoint);
							break;
					}
				}
			}
			catch(Exception ex)
			{
				m_Controller.FireLogException(ex, null, null);
			}
		}

		private void mnuMLAddUser_Click(object sender, System.EventArgs ev)
		{
			MessageBox.Show ("Adding User...");
		}

		private void mnuMLRefresh_Click(object sender, System.EventArgs ev)
		{
			LoadTreeView();
		}

		private void mnuULDelete_Click(object sender, System.EventArgs ev)
		{
			MessageBox.Show ("Deleting User...");
		}

		private void trvNameList_BeforeSelect(object sender, System.Windows.Forms.TreeViewCancelEventArgs e)
		{
			try
			{
				// Clear the old SubForm
				// From: DeltaTX (delete_this_bit.DeltaTX@yahoo.com)
				// After clearing a control collection (i.e. myController.Controls.Clear())
				// and re-populating it, then the OnClosing event is never fired 
				// (tried clicking "X" and ALT-F4).
				// I found that I can call Form.Close and remove the SubForm. However, if
				// I use From.Closing to ask to save any changes, I lose the close button.
				// AJM: Setting the focus in From.Closing of the target form,
				// after the MessageBox and prior to close, seems to fix the problem.
				foreach (Control SubForm in pnlContainer.Controls)
				{
					if (SubForm is Form)
					{
						((Form)SubForm).Close();
						if (!((Form)SubForm).IsDisposed)
						{
							e.Cancel = true;
							return;
						}
						pnlContainer.Controls.Remove(SubForm);
					}
				}

				// Toggle the color on the nodes so we know which is in the edit state
				if (trvNameList.SelectedNode != null) trvNameList.SelectedNode.ForeColor = Color.FromKnownColor(KnownColor.WindowText);
				e.Node.ForeColor = Color.Red;
			}
			catch(Exception ex)
			{
				m_Controller.FireLogException(ex, null, null);
			}
		}

		private void trvNameList_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
			System.Windows.Forms.Form frmSelected = null;
				
			try
			{
				// Load new form
				switch (e.Node.ImageIndex)
				{
					case IDX_MACHINENAME:
						frmSelected = new frmSettingsGlobal();
						((frmSettingsGlobal)frmSelected).Initialize(m_Controller);
						break;
					case IDX_USERNAME:
						frmSelected = new frmSettingsUser();
						((frmSettingsUser)frmSelected).Initialize(m_Controller, e.Node.Text);
						break;
					case IDX_ANALYZER:
						frmSelected = new frmSettingsUserAnalyzer();
						((frmSettingsUserAnalyzer)frmSelected).Initialize(m_Controller, e.Node.Parent.Text);
						break;
					case IDX_PASSLIST:
						frmSelected = new frmSettingsUserMailFrom();
						((frmSettingsUserMailFrom)frmSelected).Initialize(m_Controller, e.Node.Parent.Text);
						break;
				}

				// Show the selected settings form
				if (frmSelected != null)
				{
					// This form will be within a parent form
					frmSelected.TopLevel = false;
					
					// Fill the container with the new form
					// Note: Form anchors must be set for four sides prior to form creation
					//       Oddly, DockStyle and anchors are mutually exclusive on form designer
					frmSelected.Dock = DockStyle.Fill;
					pnlContainer.Controls.Add(frmSelected);

					// Show the form
					frmSelected.Show();
				}
			}
			catch(Exception ex)
			{
				m_Controller.FireLogException(ex, null, null);
			}
		}

		private void frmSelector_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try
			{
				foreach (Control SubForm in pnlContainer.Controls)
				{
					if (SubForm is Form)
					{
						((Form)SubForm).Close();
						if (!((Form)SubForm).IsDisposed)
						{
							e.Cancel = true;
							return;
						}
						pnlContainer.Controls.Remove(SubForm);
					}
				}
			}
			catch(Exception ex)
			{
				m_Controller.FireLogException(ex, null, null);
			}
		}

	}
}
