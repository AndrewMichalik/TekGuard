using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;

namespace TGPController
{
	/// <summary>
	/// Summary description for AboutForm.
	/// </summary>
	public class frmAbout : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.Windows.Forms.Button closeButton;
		private System.Windows.Forms.ListView lvwAssembly;
		private System.Windows.Forms.ColumnHeader colAssemblyName;
		private System.Windows.Forms.ColumnHeader colAssemblyVersion;
		private System.Windows.Forms.ColumnHeader colAssemblyDate;
		private System.Windows.Forms.Label DLLLocation;
		private System.Windows.Forms.LinkLabel lnkWebAddress;
		private System.ComponentModel.Container components = null;

		// Component members
		const string DLL_PREFIX = "tgp";

		#region Constructors / Destructors
		public frmAbout()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			// Set up web links
			lnkWebAddress.Links.Add(lnkWebAddress.Text.IndexOf("http"), lnkWebAddress.Text.Length - 1, "http://www.TekGuard.com");

			// Show component drive location
			DLLLocation.Text = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		
		}

		protected override void Dispose( bool disposing )
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		#endregion // Constructor / Dispose

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.lvwAssembly = new System.Windows.Forms.ListView();
			this.colAssemblyName = new System.Windows.Forms.ColumnHeader();
			this.colAssemblyVersion = new System.Windows.Forms.ColumnHeader();
			this.colAssemblyDate = new System.Windows.Forms.ColumnHeader();
			this.closeButton = new System.Windows.Forms.Button();
			this.DLLLocation = new System.Windows.Forms.Label();
			this.lnkWebAddress = new System.Windows.Forms.LinkLabel();
			this.SuspendLayout();
			// 
			// lvwAssembly
			// 
			this.lvwAssembly.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lvwAssembly.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						  this.colAssemblyName,
																						  this.colAssemblyVersion,
																						  this.colAssemblyDate});
			this.lvwAssembly.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.lvwAssembly.Location = new System.Drawing.Point(8, 8);
			this.lvwAssembly.Name = "lvwAssembly";
			this.lvwAssembly.Size = new System.Drawing.Size(416, 128);
			this.lvwAssembly.TabIndex = 0;
			this.lvwAssembly.View = System.Windows.Forms.View.Details;
			// 
			// colAssemblyName
			// 
			this.colAssemblyName.Text = "Module Name";
			this.colAssemblyName.Width = 200;
			// 
			// colAssemblyVersion
			// 
			this.colAssemblyVersion.Text = "Version";
			this.colAssemblyVersion.Width = 96;
			// 
			// colAssemblyDate
			// 
			this.colAssemblyDate.Text = "Date";
			this.colAssemblyDate.Width = 96;
			// 
			// closeButton
			// 
			this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.closeButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.closeButton.Location = new System.Drawing.Point(352, 168);
			this.closeButton.Name = "closeButton";
			this.closeButton.Size = new System.Drawing.Size(72, 24);
			this.closeButton.TabIndex = 0;
			this.closeButton.Text = "&Close";
			this.closeButton.Click += new System.EventHandler(this.closeButton_Click);
			// 
			// DLLLocation
			// 
			this.DLLLocation.Location = new System.Drawing.Point(8, 144);
			this.DLLLocation.Name = "DLLLocation";
			this.DLLLocation.Size = new System.Drawing.Size(312, 16);
			this.DLLLocation.TabIndex = 0;
			// 
			// lnkWebAddress
			// 
			this.lnkWebAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lnkWebAddress.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lnkWebAddress.Location = new System.Drawing.Point(8, 168);
			this.lnkWebAddress.Name = "lnkWebAddress";
			this.lnkWebAddress.Size = new System.Drawing.Size(326, 16);
			this.lnkWebAddress.TabIndex = 9;
			this.lnkWebAddress.TabStop = true;
			this.lnkWebAddress.Text = "Web: http://www.TekGuard.com";
			this.lnkWebAddress.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkWebAddress_LinkClicked);
			// 
			// frmAbout
			// 
			this.AcceptButton = this.closeButton;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(434, 215);
			this.Controls.Add(this.lnkWebAddress);
			this.Controls.Add(this.DLLLocation);
			this.Controls.Add(this.lvwAssembly);
			this.Controls.Add(this.closeButton);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmAbout";
			this.ShowInTaskbar = false;
			this.Text = "About EMail Interceptor PlugIn";
			this.Load += new System.EventHandler(this.frmAbout_Load);
			this.ResumeLayout(false);

		}
		#endregion

		#region Form Events
		private void lnkWebAddress_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			lnkWebAddress.Links[lnkWebAddress.Links.IndexOf(e.Link)].Visited = true;
			ProcessStartInfo psInfo = new ProcessStartInfo("iexplore.exe", "-new " + e.Link.LinkData.ToString());
			Process.Start(psInfo);
		}

		private void frmAbout_Load(object sender, System.EventArgs ev)
		{
			// Fill in loaded modules / version number info list view.
			try 
			{
				// Get all modules
				ArrayList TGPAssemblyList = new ArrayList();
				foreach (ProcessModule module in Process.GetCurrentProcess().Modules)
				{
					ListViewItem Item = new ListViewItem();
					Item.Text = module.ModuleName;

					// Get version info
					FileVersionInfo verInfo = module.FileVersionInfo;
					string versionStr = String.Format("{0}.{1}.{2}.{3}", 
						verInfo.FileMajorPart,
						verInfo.FileMinorPart,
						verInfo.FileBuildPart,
						verInfo.FilePrivatePart);
					Item.SubItems.Add(versionStr);

					// Get the assembly file date
					DateTime lastWriteDate = File.GetLastWriteTime(module.FileName);
					string dateStr = lastWriteDate.ToString("MMM dd, yyyy");
					Item.SubItems.Add(dateStr);
					Item.ForeColor = Color.Gray;

					// Add this entry to the list view
					lvwAssembly.Items.Add(Item);

					// Identify TekGuard PlugIn related assemblies
					if (module.ModuleName.ToLower().StartsWith(DLL_PREFIX))
					{
						TGPAssemblyList.Add(Item);
					}
				}

				// Sort ascending by assembly name
				lvwAssembly.Sorting = SortOrder.Ascending;
				lvwAssembly.Sort();

				// Highlight TekGuard PlugIn related assemblies
				foreach (ListViewItem TopList in TGPAssemblyList)
				{
					TopList.ForeColor = Color.Black;
				}
			}
			catch {}
		}

		private void closeButton_Click(object sender, System.EventArgs ev)
		{
			Close();
		}

		#endregion

	}
}
