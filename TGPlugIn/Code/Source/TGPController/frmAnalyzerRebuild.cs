using System;
using System.Data;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using TGPConnector;

namespace TGPController
{
	/// <summary>
	/// Summary description for frmAnalyzerRebuild.
	/// </summary>
	public class frmAnalyzerRebuild : System.Windows.Forms.Form
	{
		// Component members
		private Controller			m_Controller	= null;		// Pointer to parent TGPlugIn instance
		private	bool				m_Continue		= true;		// Cancel operation indicator

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.ProgressBar barProgress;
		private System.Windows.Forms.Button btnStart;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.TreeView trvIsGood;
		private System.Windows.Forms.TreeView trvIsJunk;
		private System.Windows.Forms.Label lblIsGood;
		private System.Windows.Forms.Label lblIsJunk;
		private System.Windows.Forms.Label lblRatio;		
		private System.Windows.Forms.Label lblPercent;

		#region Constructors / Destructors
		public frmAnalyzerRebuild(string Title, Icon Icon)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			// Set the window title and icon
			if (Title != null) this.Text = Title;
			if (Icon != null) this.Icon = Icon;
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(frmAnalyzerRebuild));
			this.trvIsGood = new System.Windows.Forms.TreeView();
			this.btnStart = new System.Windows.Forms.Button();
			this.barProgress = new System.Windows.Forms.ProgressBar();
			this.btnCancel = new System.Windows.Forms.Button();
			this.trvIsJunk = new System.Windows.Forms.TreeView();
			this.lblIsGood = new System.Windows.Forms.Label();
			this.lblIsJunk = new System.Windows.Forms.Label();
			this.lblPercent = new System.Windows.Forms.Label();
			this.lblRatio = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// trvIsGood
			// 
			this.trvIsGood.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.trvIsGood.BackColor = System.Drawing.SystemColors.Window;
			this.trvIsGood.CheckBoxes = true;
			this.trvIsGood.ImageIndex = -1;
			this.trvIsGood.Location = new System.Drawing.Point(16, 40);
			this.trvIsGood.Name = "trvIsGood";
			this.trvIsGood.SelectedImageIndex = -1;
			this.trvIsGood.Size = new System.Drawing.Size(168, 232);
			this.trvIsGood.TabIndex = 9;
			// 
			// btnStart
			// 
			this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnStart.Location = new System.Drawing.Point(312, 296);
			this.btnStart.Name = "btnStart";
			this.btnStart.Size = new System.Drawing.Size(64, 24);
			this.btnStart.TabIndex = 28;
			this.btnStart.Text = "Start";
			this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
			// 
			// barProgress
			// 
			this.barProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.barProgress.Location = new System.Drawing.Point(208, 296);
			this.barProgress.Name = "barProgress";
			this.barProgress.Size = new System.Drawing.Size(96, 24);
			this.barProgress.Step = 1;
			this.barProgress.TabIndex = 29;
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.btnCancel.Enabled = false;
			this.btnCancel.Location = new System.Drawing.Point(16, 296);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(64, 24);
			this.btnCancel.TabIndex = 30;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// trvIsJunk
			// 
			this.trvIsJunk.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.trvIsJunk.BackColor = System.Drawing.SystemColors.Window;
			this.trvIsJunk.CheckBoxes = true;
			this.trvIsJunk.ImageIndex = -1;
			this.trvIsJunk.Location = new System.Drawing.Point(200, 40);
			this.trvIsJunk.Name = "trvIsJunk";
			this.trvIsJunk.SelectedImageIndex = -1;
			this.trvIsJunk.Size = new System.Drawing.Size(176, 232);
			this.trvIsJunk.TabIndex = 31;
			// 
			// lblIsGood
			// 
			this.lblIsGood.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblIsGood.Location = new System.Drawing.Point(16, 16);
			this.lblIsGood.Name = "lblIsGood";
			this.lblIsGood.Size = new System.Drawing.Size(88, 16);
			this.lblIsGood.TabIndex = 32;
			this.lblIsGood.Text = "Is Good:";
			this.lblIsGood.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblIsJunk
			// 
			this.lblIsJunk.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblIsJunk.Location = new System.Drawing.Point(200, 16);
			this.lblIsJunk.Name = "lblIsJunk";
			this.lblIsJunk.Size = new System.Drawing.Size(88, 16);
			this.lblIsJunk.TabIndex = 33;
			this.lblIsJunk.Text = "Is Junk:";
			this.lblIsJunk.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblPercent
			// 
			this.lblPercent.Location = new System.Drawing.Point(88, 296);
			this.lblPercent.Name = "lblPercent";
			this.lblPercent.Size = new System.Drawing.Size(48, 24);
			this.lblPercent.TabIndex = 34;
			this.lblPercent.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// lblRatio
			// 
			this.lblRatio.Location = new System.Drawing.Point(136, 296);
			this.lblRatio.Name = "lblRatio";
			this.lblRatio.Size = new System.Drawing.Size(64, 24);
			this.lblRatio.TabIndex = 35;
			this.lblRatio.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// frmAnalyzerRebuild
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(390, 331);
			this.Controls.Add(this.lblRatio);
			this.Controls.Add(this.lblPercent);
			this.Controls.Add(this.lblIsJunk);
			this.Controls.Add(this.lblIsGood);
			this.Controls.Add(this.trvIsJunk);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.barProgress);
			this.Controls.Add(this.btnStart);
			this.Controls.Add(this.trvIsGood);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "frmAnalyzerRebuild";
			this.Text = "TekGuard EMI PlugIn Initialization Wizard";
			this.ResumeLayout(false);

		}
		#endregion

		#region Initialize
		internal bool Initialize (Controller Controller)
		{
			try
			{
				// Set pointer to parent instance
				m_Controller = Controller;

				// Set the window title and icon
				this.Text = m_Controller.Title;
				this.Icon = m_Controller.Icon;

				// Retrieve the serialized tree
				TreeSerializer FolderTree = (TreeSerializer) m_Controller.DBAnaQuery.AnalyzerRebuildItemize(this, null);

				// Successful itemization of the list of folders?
				if (FolderTree == null) return (false);

				// Populate the treeview 
				FolderTree.PopulateTree(trvIsGood); 
				FolderTree.PopulateTree(trvIsJunk); 

				// Set the default for for "Good" items
				foreach (TreeNode Node in trvIsGood.Nodes)
				{
					CheckNodes (Node, "Sent Items");
				}

				// Set the default for for "Junk" items
				foreach (TreeNode Node in trvIsJunk.Nodes)
				{
					CheckNodes (Node, "Junk");
					CheckNodes (Node, "Spam");
				}

				// Return success
				return (true);
			}
			catch(Exception ex)
			{
				m_Controller.FireLogException(ex, null, null);
				return (false);
			}
		}
		private void CheckNodes (TreeNode Node, string Text)
		{
			// Search for this string in the list of MailBoxes
			if (Node.Text.IndexOf(Text) != -1) Node.Checked = true;

			// Set the default for for "Junk" items
			foreach (TreeNode SubNode in Node.Nodes)
			{
				CheckNodes (SubNode, Text);
			}
		}
		#endregion

		private void btnStart_Click(object sender, System.EventArgs ev)
		{
			try
			{
				btnStart.Enabled = false;
				btnCancel.Text = "Cancel";
				m_Continue = true;
				btnCancel.Enabled = true;
				lblPercent.ForeColor = System.Drawing.SystemColors.ControlText;
				lblRatio.ForeColor = System.Drawing.SystemColors.ControlText;

				barProgress.Value = 0;
				lblPercent.Text = "";
				lblRatio.Text = "";

				// Add the event for the Analyzer Status callback
				// m_Controller.DBAnaQuery.AnalyzerStatusEventAdd(new WinFormInvokeHandler(StatusInvokeLocalThread));
				m_Controller.DBAnaQuery.AnalyzerStatusEventAdd(new TGPConnector.WinFormInvokeHandler(StatusInvoke));

				// Load the tree into the serializer; pass it as an object
				object obj = m_Controller.DBAnaQuery.AnalyzerRebuildExecute(this, (object) new AnalyzerRebuildRequestArgs(new TreeSerializer(trvIsGood), new TreeSerializer(trvIsJunk)));
				
				// Retrieve the arguments and add this record to the control
				AnalyzerStatusEventArgs AnalyzerStatus = (AnalyzerStatusEventArgs) obj;

				// Remove the event for the Analyzer Status callback
				m_Controller.DBAnaQuery.AnalyzerStatusEventRemove();

				btnCancel.Enabled = false;
				btnCancel.Text = "Done";
				barProgress.Value = 0;
				lblPercent.ForeColor = System.Drawing.SystemColors.ControlDark;
				lblRatio.ForeColor = System.Drawing.SystemColors.ControlDark;
				btnStart.Enabled = true;
			}		
			catch(Exception ex)
			{
				m_Controller.FireLogException(ex, null, null);
				return;
			}
		}

		#region StatusInvoke
		public object StatusInvoke(object[] objArray)
		{
			// Indicate percent complete
			return (StatusBegin((object[])objArray[0]));
		}
		private object StatusInvokeLocalThread(object[] objArray)
		{
			// Call the control using the local form thread
			// this.Invoke(new WinFormInvokeHandler(StatusBegin), objArray);
			// barProgress.Invoke(new WinFormInvokeHandler(StatusBegin), objArray);
			this.Invoke(new WinFormInvokeHandler(StatusBegin), objArray);
			return(m_Continue);
		}
		private object StatusBegin(object[] objArray)
		{
			try
			{
				// Retrieve the arguments and add this record to the control
				AnalyzerStatusEventArgs AnalyzerStatus = (AnalyzerStatusEventArgs) objArray[0];

				// Set the progress bar parameters
				barProgress.Minimum = 0;
				barProgress.Maximum = AnalyzerStatus.Total;
				barProgress.Value = AnalyzerStatus.Current;

				// Set percent complete
				if (AnalyzerStatus.Total != 0)
				{
					lblPercent.Text = (AnalyzerStatus.Current / (float) AnalyzerStatus.Total).ToString("P1");
					lblRatio.Text =  "(" + AnalyzerStatus.Current + "/" + AnalyzerStatus.Total + ")";
				}
				return(m_Continue);
			}
			catch(Exception ex)
			{
				m_Controller.FireLogException(ex, null, null);
				return (false);
			}
		}
		#endregion

		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			m_Continue = false;
		}

	}
}
