using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using TGPConnector;

namespace TGPController
{
	/// <summary>
	/// Summary for frmMain Display Window
	/// </summary>
	public class frmMain : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TabPage	tpgAnalyzer;
		private	System.Windows.Forms.TabPage	tpgEvents;
		private System.Windows.Forms.ListBox	lstLogEvents;
		private System.Windows.Forms.ListBox	lstLogAnalyzer;
		private System.Windows.Forms.Label		lblBytesInCount;
		private System.Windows.Forms.Label		lblBytesOutCount;
		private System.Windows.Forms.Label		lblEventsCount;

		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.Button btnConfigure;
		private System.Windows.Forms.MenuItem menuFileItem;
		private System.Windows.Forms.MenuItem menuTagReferenceItem;
		private System.Windows.Forms.MenuItem menuSpacerItem4;
		private System.Windows.Forms.MainMenu mainMenu;
		private System.Windows.Forms.MenuItem mnuFileExitItem;
		private System.Windows.Forms.MenuItem mnuHelpItem;
		private System.Windows.Forms.MenuItem mnuAboutItem;
		private System.Windows.Forms.TabControl tabMain;
		private System.Windows.Forms.TabPage tpgStatistics;
		private System.Windows.Forms.GroupBox grpStatistics;
		private System.Windows.Forms.Label lblBytesOut;
		private System.Windows.Forms.Label lblBytesIn;
		private System.Windows.Forms.Label lblErrors;
		private System.Windows.Forms.Label lblPercentReady;
		private System.Windows.Forms.Label lblDBReady;
		private System.Windows.Forms.Label lblIPAddress;
		private System.Windows.Forms.Label lblPrimaryIP;				
		private ctlBrowser ctlBrowser;
		private System.Windows.Forms.LinkLabel lnkWebAddress;			

		// Component members
		private Controller			m_Controller	= null;		// Pointer to parent TGPController instance
		private frmAbout			m_frmAbout		= null;		// "About" application form
		private LogQueue			m_LogQueue		= null;		// Log File Notification class
		private TGWinState			m_windowState;				// Persistent Window State

		// Database access constants
		private const string		COL_WINSTATECONTROLLER	= "WinStateController";

		// Remote logging DataSource ArrayLists
		private LogArray			m_laLogEvents;
		private LogArray			m_laLogAnalyzer;
		private LogArray			m_laPercentReady;

		#region Constructors / Destructors

		public frmMain(string Title, Icon Icon, LogQueue LogQueue)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			// Inhibit warning
			components = components;

			// Set the window title and icon
			if (Title != null) this.Text = Title;
			if (Icon != null) this.Icon = Icon;

			// Save the logging objects
			m_LogQueue = LogQueue;

			// Create the Remote logging DataSource ArrayLists
			// Note: This permits immediate use of the .AddItem(Text) functions
			m_laLogEvents = new LogArray(LogQueue, LogDisplayType.Alerts);
			m_laLogAnalyzer = new LogArray(LogQueue, LogDisplayType.Analyzer);
			m_laPercentReady = new LogArray(LogQueue, LogDisplayType.PercentReady);

			// Set up web links
			lnkWebAddress.Links.Add(lnkWebAddress.Text.IndexOf("http"), lnkWebAddress.Text.Length - 1, "http://www.TekGuard.com");
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(frmMain));
			this.btnConfigure = new System.Windows.Forms.Button();
			this.mainMenu = new System.Windows.Forms.MainMenu();
			this.menuFileItem = new System.Windows.Forms.MenuItem();
			this.mnuFileExitItem = new System.Windows.Forms.MenuItem();
			this.mnuHelpItem = new System.Windows.Forms.MenuItem();
			this.menuTagReferenceItem = new System.Windows.Forms.MenuItem();
			this.menuSpacerItem4 = new System.Windows.Forms.MenuItem();
			this.mnuAboutItem = new System.Windows.Forms.MenuItem();
			this.tabMain = new System.Windows.Forms.TabControl();
			this.tpgEvents = new System.Windows.Forms.TabPage();
			this.lstLogEvents = new System.Windows.Forms.ListBox();
			this.tpgAnalyzer = new System.Windows.Forms.TabPage();
			this.lstLogAnalyzer = new System.Windows.Forms.ListBox();
			this.tpgStatistics = new System.Windows.Forms.TabPage();
			this.grpStatistics = new System.Windows.Forms.GroupBox();
			this.lblIPAddress = new System.Windows.Forms.Label();
			this.lblPrimaryIP = new System.Windows.Forms.Label();
			this.lblEventsCount = new System.Windows.Forms.Label();
			this.lblErrors = new System.Windows.Forms.Label();
			this.lblBytesOutCount = new System.Windows.Forms.Label();
			this.lblBytesInCount = new System.Windows.Forms.Label();
			this.lblPercentReady = new System.Windows.Forms.Label();
			this.lblBytesOut = new System.Windows.Forms.Label();
			this.lblBytesIn = new System.Windows.Forms.Label();
			this.lblDBReady = new System.Windows.Forms.Label();
			this.ctlBrowser = new TGPController.ctlBrowser();
			this.lnkWebAddress = new System.Windows.Forms.LinkLabel();
			this.tabMain.SuspendLayout();
			this.tpgEvents.SuspendLayout();
			this.tpgAnalyzer.SuspendLayout();
			this.tpgStatistics.SuspendLayout();
			this.grpStatistics.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnConfigure
			// 
			this.btnConfigure.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnConfigure.Enabled = false;
			this.btnConfigure.Location = new System.Drawing.Point(510, 300);
			this.btnConfigure.Name = "btnConfigure";
			this.btnConfigure.Size = new System.Drawing.Size(72, 24);
			this.btnConfigure.TabIndex = 3;
			this.btnConfigure.Text = "Configure";
			this.btnConfigure.Click += new System.EventHandler(this.btnConfigure_Click);
			// 
			// mainMenu
			// 
			this.mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuFileItem,
																					 this.mnuHelpItem});
			// 
			// menuFileItem
			// 
			this.menuFileItem.Index = 0;
			this.menuFileItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.mnuFileExitItem});
			this.menuFileItem.Text = "&File";
			// 
			// mnuFileExitItem
			// 
			this.mnuFileExitItem.Index = 0;
			this.mnuFileExitItem.Text = "&Exit";
			this.mnuFileExitItem.Click += new System.EventHandler(this.menuFileExitItem_Click);
			// 
			// mnuHelpItem
			// 
			this.mnuHelpItem.Index = 1;
			this.mnuHelpItem.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						this.menuTagReferenceItem,
																						this.menuSpacerItem4,
																						this.mnuAboutItem});
			this.mnuHelpItem.Text = "&Help";
			// 
			// menuTagReferenceItem
			// 
			this.menuTagReferenceItem.Index = 0;
			this.menuTagReferenceItem.Shortcut = System.Windows.Forms.Shortcut.F1;
			this.menuTagReferenceItem.Text = "&Help Topics";
			this.menuTagReferenceItem.Click += new System.EventHandler(this.menuTagReferenceItem_Click);
			// 
			// menuSpacerItem4
			// 
			this.menuSpacerItem4.Index = 1;
			this.menuSpacerItem4.Text = "-";
			// 
			// mnuAboutItem
			// 
			this.mnuAboutItem.Index = 2;
			this.mnuAboutItem.Text = "&About EMail Interceptor";
			this.mnuAboutItem.Click += new System.EventHandler(this.mnuAboutItem_Click);
			// 
			// tabMain
			// 
			this.tabMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.tabMain.Controls.Add(this.tpgEvents);
			this.tabMain.Controls.Add(this.tpgAnalyzer);
			this.tabMain.Controls.Add(this.tpgStatistics);
			this.tabMain.Location = new System.Drawing.Point(160, 8);
			this.tabMain.Name = "tabMain";
			this.tabMain.SelectedIndex = 0;
			this.tabMain.Size = new System.Drawing.Size(416, 284);
			this.tabMain.TabIndex = 6;
			// 
			// tpgEvents
			// 
			this.tpgEvents.Controls.Add(this.lstLogEvents);
			this.tpgEvents.Location = new System.Drawing.Point(4, 22);
			this.tpgEvents.Name = "tpgEvents";
			this.tpgEvents.Size = new System.Drawing.Size(408, 258);
			this.tpgEvents.TabIndex = 0;
			this.tpgEvents.Text = "Events";
			// 
			// lstLogEvents
			// 
			this.lstLogEvents.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lstLogEvents.ForeColor = System.Drawing.SystemColors.HotTrack;
			this.lstLogEvents.HorizontalScrollbar = true;
			this.lstLogEvents.Location = new System.Drawing.Point(8, 16);
			this.lstLogEvents.Name = "lstLogEvents";
			this.lstLogEvents.Size = new System.Drawing.Size(392, 199);
			this.lstLogEvents.TabIndex = 3;
			// 
			// tpgAnalyzer
			// 
			this.tpgAnalyzer.Controls.Add(this.lstLogAnalyzer);
			this.tpgAnalyzer.Location = new System.Drawing.Point(4, 22);
			this.tpgAnalyzer.Name = "tpgAnalyzer";
			this.tpgAnalyzer.Size = new System.Drawing.Size(408, 258);
			this.tpgAnalyzer.TabIndex = 3;
			this.tpgAnalyzer.Text = "Analyzer";
			// 
			// lstLogAnalyzer
			// 
			this.lstLogAnalyzer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lstLogAnalyzer.ForeColor = System.Drawing.SystemColors.HotTrack;
			this.lstLogAnalyzer.HorizontalScrollbar = true;
			this.lstLogAnalyzer.Location = new System.Drawing.Point(8, 16);
			this.lstLogAnalyzer.Name = "lstLogAnalyzer";
			this.lstLogAnalyzer.Size = new System.Drawing.Size(392, 231);
			this.lstLogAnalyzer.TabIndex = 4;
			// 
			// tpgStatistics
			// 
			this.tpgStatistics.Controls.Add(this.grpStatistics);
			this.tpgStatistics.Location = new System.Drawing.Point(4, 22);
			this.tpgStatistics.Name = "tpgStatistics";
			this.tpgStatistics.Size = new System.Drawing.Size(408, 258);
			this.tpgStatistics.TabIndex = 2;
			this.tpgStatistics.Text = "Statistics";
			// 
			// grpStatistics
			// 
			this.grpStatistics.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.grpStatistics.Controls.Add(this.lblIPAddress);
			this.grpStatistics.Controls.Add(this.lblPrimaryIP);
			this.grpStatistics.Controls.Add(this.lblEventsCount);
			this.grpStatistics.Controls.Add(this.lblErrors);
			this.grpStatistics.Controls.Add(this.lblBytesOutCount);
			this.grpStatistics.Controls.Add(this.lblBytesInCount);
			this.grpStatistics.Controls.Add(this.lblPercentReady);
			this.grpStatistics.Controls.Add(this.lblBytesOut);
			this.grpStatistics.Controls.Add(this.lblBytesIn);
			this.grpStatistics.Controls.Add(this.lblDBReady);
			this.grpStatistics.Location = new System.Drawing.Point(8, 16);
			this.grpStatistics.Name = "grpStatistics";
			this.grpStatistics.Size = new System.Drawing.Size(392, 230);
			this.grpStatistics.TabIndex = 6;
			this.grpStatistics.TabStop = false;
			this.grpStatistics.Text = "Statistics";
			// 
			// lblIPAddress
			// 
			this.lblIPAddress.Location = new System.Drawing.Point(240, 24);
			this.lblIPAddress.Name = "lblIPAddress";
			this.lblIPAddress.Size = new System.Drawing.Size(80, 16);
			this.lblIPAddress.TabIndex = 9;
			this.lblIPAddress.Text = "0";
			this.lblIPAddress.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblPrimaryIP
			// 
			this.lblPrimaryIP.Location = new System.Drawing.Point(168, 24);
			this.lblPrimaryIP.Name = "lblPrimaryIP";
			this.lblPrimaryIP.Size = new System.Drawing.Size(64, 16);
			this.lblPrimaryIP.TabIndex = 8;
			this.lblPrimaryIP.Text = "Primary IP";
			// 
			// lblEventsCount
			// 
			this.lblEventsCount.Location = new System.Drawing.Point(96, 96);
			this.lblEventsCount.Name = "lblEventsCount";
			this.lblEventsCount.Size = new System.Drawing.Size(56, 16);
			this.lblEventsCount.TabIndex = 7;
			this.lblEventsCount.Text = "0";
			// 
			// lblErrors
			// 
			this.lblErrors.Location = new System.Drawing.Point(8, 96);
			this.lblErrors.Name = "lblErrors";
			this.lblErrors.Size = new System.Drawing.Size(80, 16);
			this.lblErrors.TabIndex = 6;
			this.lblErrors.Text = "Errors";
			// 
			// lblBytesOutCount
			// 
			this.lblBytesOutCount.Location = new System.Drawing.Point(96, 72);
			this.lblBytesOutCount.Name = "lblBytesOutCount";
			this.lblBytesOutCount.Size = new System.Drawing.Size(56, 16);
			this.lblBytesOutCount.TabIndex = 5;
			this.lblBytesOutCount.Text = "0";
			// 
			// lblBytesInCount
			// 
			this.lblBytesInCount.Location = new System.Drawing.Point(96, 48);
			this.lblBytesInCount.Name = "lblBytesInCount";
			this.lblBytesInCount.Size = new System.Drawing.Size(56, 16);
			this.lblBytesInCount.TabIndex = 4;
			this.lblBytesInCount.Text = "0";
			// 
			// lblPercentReady
			// 
			this.lblPercentReady.Location = new System.Drawing.Point(96, 24);
			this.lblPercentReady.Name = "lblPercentReady";
			this.lblPercentReady.Size = new System.Drawing.Size(56, 16);
			this.lblPercentReady.TabIndex = 3;
			this.lblPercentReady.Text = "0";
			// 
			// lblBytesOut
			// 
			this.lblBytesOut.Location = new System.Drawing.Point(8, 72);
			this.lblBytesOut.Name = "lblBytesOut";
			this.lblBytesOut.Size = new System.Drawing.Size(80, 16);
			this.lblBytesOut.TabIndex = 2;
			this.lblBytesOut.Text = "Bytes Out";
			// 
			// lblBytesIn
			// 
			this.lblBytesIn.Location = new System.Drawing.Point(8, 48);
			this.lblBytesIn.Name = "lblBytesIn";
			this.lblBytesIn.Size = new System.Drawing.Size(80, 16);
			this.lblBytesIn.TabIndex = 1;
			this.lblBytesIn.Text = "Bytes In";
			// 
			// lblDBReady
			// 
			this.lblDBReady.Location = new System.Drawing.Point(8, 24);
			this.lblDBReady.Name = "lblDBReady";
			this.lblDBReady.Size = new System.Drawing.Size(80, 16);
			this.lblDBReady.TabIndex = 0;
			this.lblDBReady.Text = "DB Ready";
			// 
			// ctlBrowser
			// 
			this.ctlBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left)));
			this.ctlBrowser.Location = new System.Drawing.Point(8, 16);
			this.ctlBrowser.Name = "ctlBrowser";
			this.ctlBrowser.Size = new System.Drawing.Size(140, 308);
			this.ctlBrowser.TabIndex = 9;
			// 
			// lnkWebAddress
			// 
			this.lnkWebAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lnkWebAddress.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lnkWebAddress.Location = new System.Drawing.Point(168, 308);
			this.lnkWebAddress.Name = "lnkWebAddress";
			this.lnkWebAddress.Size = new System.Drawing.Size(326, 16);
			this.lnkWebAddress.TabIndex = 8;
			this.lnkWebAddress.TabStop = true;
			this.lnkWebAddress.Text = "Web: http://www.TekGuard.com";
			this.lnkWebAddress.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkWebAddress_LinkClicked);
			// 
			// frmMain
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(592, 353);
			this.Controls.Add(this.lnkWebAddress);
			this.Controls.Add(this.ctlBrowser);
			this.Controls.Add(this.tabMain);
			this.Controls.Add(this.btnConfigure);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Menu = this.mainMenu;
			this.Name = "frmMain";
			this.Text = "TekGuard EMail Interceptor PlugIn";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.frmMain_OnClosing);
			this.tabMain.ResumeLayout(false);
			this.tpgEvents.ResumeLayout(false);
			this.tpgAnalyzer.ResumeLayout(false);
			this.tpgStatistics.ResumeLayout(false);
			this.grpStatistics.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		#region Initialize
		internal bool Initialize (Controller Controller)
		{
			// Save the parent instance information
			m_Controller = Controller;

			try
			{
				// Initialize the mini-browser window
				ctlBrowser.Initialize(m_Controller);

				// Initialize the Persistent windows state object
				m_windowState = new TGWinState(Controller, this, COL_WINSTATECONTROLLER);

				// Attach the remote logging arraylists to the listboxes
				this.lstLogEvents.DataSource = m_laLogEvents.Connect(lstLogEvents);
				this.lstLogAnalyzer.DataSource = m_laLogAnalyzer.Connect(lstLogAnalyzer);

				// Attach the remote logging arraylists to the labels
				this.lblPercentReady.DataBindings.Add("Text", m_laPercentReady.Connect(lblPercentReady), null);
				// this.lblBytesInCount.DataBindings.Add("Text", new LogArray(LogQueue, LogDisplayType.BytesIn, lblBytesInCount), null);
				// this.lblBytesOutCount.DataBindings.Add("Text", new LogArray(LogQueue, LogDisplayType.BytesOut, lblBytesOutCount), null);
				// this.lblEventsCount.DataBindings.Add("Text", new LogArray(LogQueue, LogDisplayType.EventCount, lblEventsCount), null);

				// Start the Log Queue threaded read processing
				m_LogQueue.StartAll();
	
				// Show the primary IP Address
				this.lblIPAddress.Text = m_Controller.HostQuery.IPPrimary;
				
				// Enable / Disable configuration
				btnConfigure.Enabled = (m_Controller.DBCfgQuery != null);

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

		#region Sponsors
		internal string[] Sponsors 
		{
			get { return (ctlBrowser.Sponsors); }
			set { ctlBrowser.Sponsors = value; ctlBrowser.NavigateTo(null); }
		}
		#endregion

		#region Form Display Functions

		#region LogEvent
		public void LogEvent(string Text)
		{
			m_laLogEvents.Add(Text);
		}
		#endregion
		
		#region LogAnalyzer
		public void LogAnalyzer(string Text)
		{
			m_laLogAnalyzer.Add(Text);
		}
		#endregion

		#region PercentReady
		public void PercentReady(string Text)
		{
			m_laPercentReady.Add(Text);
		}
		#endregion

		#endregion

		#region Form Event Firing Functions
		private void lnkWebAddress_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			lnkWebAddress.Links[lnkWebAddress.Links.IndexOf(e.Link)].Visited = true;
			ProcessStartInfo psInfo = new ProcessStartInfo("iexplore.exe", "-new " + e.Link.LinkData.ToString());
			Process.Start(psInfo);
		}

		private void menuTagReferenceItem_Click(object sender, System.EventArgs ev)
		{
			try
			{
				const string HELP_FILE = "Help\\TGPlugIn.chm";
				// Get PlugIn startup directory
				string PlugInPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
				System.Diagnostics.Process.Start(Path.Combine(PlugInPath, HELP_FILE));
			}
			catch(Exception ex)
			{
				MessageBox.Show(this, ex.Message, "Help Files", 
					MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		private void mnuAboutItem_Click(object sender, System.EventArgs ev)
		{
			try
			{
				// Already created?
				if ((m_frmAbout != null) && !m_frmAbout.IsDisposed)
				{
					m_frmAbout.WindowState = System.Windows.Forms.FormWindowState.Normal;
					m_frmAbout.Activate();
				}
				else 
				{
					m_frmAbout = new frmAbout();
					m_frmAbout.StartPosition = FormStartPosition.CenterParent;
					m_frmAbout.Show();
				}
			}
			catch(Exception ex)
			{
				m_Controller.FireLogException(ex, null, null);
			}
		}

		private void btnConfigure_Click(object sender, System.EventArgs ev)
		{
			try
			{

// AJM Test
//for (int ii=0; ii<1; ii++)
//m_Controller.FireLogAlert("Testing " + ii.ToString(), "123", DateTime.Now.ToLocalTime().ToString());
//ctlBrowser.NavigateTo("http://www.TekGuard.com/");

				frmSelector frmSelector = new frmSelector();
				if (!frmSelector.Initialize(m_Controller)) return;
				
				// Already active?

				frmSelector.StartPosition = FormStartPosition.CenterParent;
				frmSelector.WindowState = FormWindowState.Normal;
				frmSelector.ShowDialog(this);
				frmSelector.Activate();
			}
			catch(Exception ex)
			{
				m_Controller.FireLogException(ex, null, null);
			}
		}

		private void menuFileExitItem_Click (object sender, System.EventArgs ev)
		{
			// Close form based on event response
			if (m_Controller.FireCloseConfirm (sender)) Close();
			else m_Controller.Visible = false;
		}

		private void frmMain_OnClosing(object sender, System.ComponentModel.CancelEventArgs ev)
		{
			// Set ev.Cancel based on event response
			if((ev.Cancel = !m_Controller.FireCloseConfirm (sender)) == true) m_Controller.Visible = false;
		}

		#endregion

		#region xTGVisible
		public bool xTGVisible 
		{
			get
			{
				return (this.Opacity == 1);
			}
			set
			{
				this.Opacity = (value ? 1 : 0);
				this.ShowInTaskbar = value;
			}
		}
		#endregion

	}
}
