using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using TGPAnalyzer;
using TGPAssist;
using TGPConnector;
using TGPController;

namespace TGPlugIn
{
	using System;
	using Microsoft.Office.Core;
	using Extensibility;
	using System.Reflection;
	using System.Runtime.InteropServices;

	#region Read me for Add-in installation and setup information.
	// When run, the Add-in wizard prepared the registry for the Add-in.
	// At a later time, if the Add-in becomes unavailable for reasons such as:
	//   1) You moved this project to a computer other than which is was originally created on.
	//   2) You chose 'Yes' when presented with a message asking if you wish to remove the Add-in.
	//   3) Registry corruption.
	// you will need to re-register the Add-in by building the MyAddin21Setup project 
	// by right clicking the project in the Solution Explorer, then choosing install.
	#endregion
	
	/// <summary>
	///   The object for implementing an Add-in.
	/// </summary>
	/// <seealso class='IDTExtensibility2' />
	[GuidAttribute("565941FF-BCD9-424F-B0C2-8B912EDC9081"), ProgId("TGPlugIn.Connect")]
	public class PlugIn : System.ComponentModel.Component, Extensibility.IDTExtensibility2
	{
		private		System.ComponentModel.IContainer	components;
		private		TGPConnector.Connector				svrConnector;
		private		TGPController.Controller			svrController;
		private		TGPAnalyzer.Analyzer				svrAnalyzer;

		// Add-in members
		private		TGPOutlookApp						m_OutlookApp;
		private		TGPAssist.Assist					m_Assist;
		private		InBoxQueue							m_InBoxQueue;

		// Command Bar Buttons
		private		CommandBarButton					m_btnAnalyze;
		private		CommandBarButton					m_btnIsGood;
		private		CommandBarButton					m_btnIsJunk;
		private		CommandBarButton					m_btnController;
		
		// Shared access component globals
		private string				m_PlugInPath		= "";			// Program home path name
		private LogLocal			m_LogTools			= null;			// Application file logging class
		private string				m_UserDataPath		= "";			// UserData directory
		private string				m_ResourceFilePath	= "";			// Resource files directory
		private string				m_ConfigFilePath	= "";			// Configuration files directory
		private Bootstrap			m_Bootstrap			= null;			// Configuration database access component
		private TokenList			m_TokenList			= null;			// Analyzer token list
		private	bool				m_AnalyzerFirst		= true;			// First time through this instance
		private System.Timers.Timer tmrPerformance;
//		private TGPAssist.Cleanup_Test	m_oCleanup			= null;			// Outlook cleanup object

		// Category handling
		private	CategoryList		m_AnalyzerCategories;				// List of active category options
		private	Outlook.MAPIFolder[]m_CategoryFolder;					// AutoMove folders for categorized items

		// Logging and Configuration constants
		private const string		EVT_PROGRAMNAME		= "TekGuard EMail Interceptor PlugIn";
		private const string		DIR_LOGS			= "Logs\\";
		private const string		DIR_CONFIG			= "Config\\";
		private const string		DIR_RESOURCES		= "Resources\\";
		private const string		DIR_USERDATA		= "UserData\\";
		private const string		DIR_USERNAME		= "Default\\";

		// File name constants
		private const string		FILE_ALERTS			= "TGPlugIn";
		private const string		FILE_ANALYZER		= "TGPAnalyzer";
		private const string		FILE_BUTTON			= "TGPButton.ico";

		// User Interface Constants
		private const Int32			TMR_PERFORMANCE		= 60000;		// Performance statistics every 60 seconds
		private	const Int32			PERSIST_PERIOD		= 15;			// Persist/Backup timer period (15 minutes)
		private	const Int32			PERSIST_BACKUP		= 30;			// Backup on the half-hour
		private	const Int32			NOTIFY_PROGRESS		= 1000;			// Progress indicator notification period
		private const Int32			DEFAULT_USERID		= 1;			// Default User ID
		private const Int32			CATIDX_GOOD			= 0;			// Default Category Index for "Good" mail
		private const Int32			CATIDX_JUNK			= 1;			// Default Category Index for "Junk" mail

		// Button Captions
		private const string		CAP_EMIDASH			= "Dashboard";
		private const string		CAP_ANALYZE			= "Analyze";
		private const string		CAP_ISGOOD			= "Is Good";
		private const string		CAP_ISJUNK			= "Is Junk";

		// Analyzer status window
		private frmAnalyzerRebuild	m_AnalyzerRebuild;

		#region Constructors
		/// <summary>
		///		Implements the constructor for the TGPlugIn object.
		///		Place your initialization code within this method.
		/// </summary>
		public PlugIn()
		{
			InitializeComponent();
		}
		#endregion

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.svrConnector = new TGPConnector.Connector(this.components);
			this.svrController = new TGPController.Controller(this.components);
			this.svrAnalyzer = new TGPAnalyzer.Analyzer();
			this.tmrPerformance = new System.Timers.Timer();
			((System.ComponentModel.ISupportInitialize)(this.tmrPerformance)).BeginInit();
			// 
			// AppDomain
			// 
			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(TGPResolveEventHandler);
			// 
			// svrConnector
			// 
			this.svrConnector.OnLogAlert += new TGPConnector.LogAlertEventHandler(this.LogAlert);
			this.svrConnector.OnLogException += new TGPConnector.LogExceptionEventHandler(this.LogException);
			this.svrConnector.OnLogException_NoDisplay += new TGPConnector.LogExceptionEventHandler(this.LogException_NoDisplay);
			this.svrConnector.OnAnalyzerRebuildItemize += new TGPConnector.RemoteRequestEventHandler(this.Controller_OnAnalyzerRebuildItemize);
			this.svrConnector.OnAnalyzerRebuildExecute += new TGPConnector.RemoteRequestEventHandler(this.Controller_OnAnalyzerRebuildExecute);
			this.svrConnector.OnConfigurationApply += new TGPConnector.RemoteRequestEventHandler(this.Controller_OnConfigurationApply);
			// this.svrConnector.OnLogRemoting += new TGPConnector.LogRemotingEventHandler(this.LogException);
			// 
			// svrController
			// 
			this.svrController.OnLogAlert += new TGPController.LogAlertEventHandler(this.LogAlert);
			this.svrController.OnLogException += new TGPController.LogExceptionEventHandler(this.LogException);
			this.svrController.OnCloseConfirm += new TGPController.FormClosingEventHandler(this.svrController_OnCloseConfirm);
			// 
			// svrAnalyzer
			// 
			this.svrAnalyzer.OnLogAlert += new TGPAnalyzer.LogAlertEventHandler(this.LogAlert);
			this.svrAnalyzer.OnLogException += new TGPAnalyzer.LogExceptionEventHandler(this.LogException);
			this.svrAnalyzer.OnEnableDisable += new TGPAnalyzer.EnableDisableEventHandler(this.svrAnalyzer_OnEnableDisable);
			this.svrAnalyzer.OnLogAnalyzer += new TGPAnalyzer.LogAnalyzerEventHandler(this.LogAnalyzer);
			this.svrAnalyzer.LogSecurityMask = ((long)(0));
			// 
			// tmrPerformance
			// 
			this.tmrPerformance.Enabled = true;
			this.tmrPerformance.Interval = TMR_PERFORMANCE;
			this.tmrPerformance.Elapsed += new System.Timers.ElapsedEventHandler(this.PerformanceMonitor);
			((System.ComponentModel.ISupportInitialize)(this.tmrPerformance)).EndInit();
		}
		#endregion

		#region OnConnection / OnDisconnection
		/// <summary>
		///      Implements the OnConnection method of the IDTExtensibility2 interface.
		///      Receives notification that the Add-in is being loaded.
		/// </summary>
		/// <param term='application'>
		///      Root object of the host application.
		/// </param>
		/// <param term='connectMode'>
		///      Describes how the Add-in is being loaded.
		/// </param>
		/// <param term='addInInst'>
		///      Object representing this Add-in.
		/// </param>
		/// <seealso class='IDTExtensibility2' />
		public void OnConnection(object application, Extensibility.ext_ConnectMode connectMode, object addInInst, ref System.Array custom)
		{
			try
			{
				m_OutlookApp	= new TGPOutlookApp (application, m_Assist);
				m_Assist		= new TGPAssist.Assist();
				m_Bootstrap		= new Bootstrap(this);
//				m_oCleanup		= new TGPAssist.Cleanup_Test (m_Assist);
			}
			catch(Exception ex)
			{
				MessageBox.Show ("Cannot initialize " + ", " + ex.Message, EVT_PROGRAMNAME);
			}

			// Continue
			if(connectMode != Extensibility.ext_ConnectMode.ext_cm_Startup)
			{
				OnStartupComplete(ref custom);
			}
		
		}

		#region OnDisconnection
		/// <summary>
		///     Implements the OnDisconnection method of the IDTExtensibility2 interface.
		///     Receives notification that the Add-in is being unloaded.
		/// </summary>
		/// <param term='disconnectMode'>
		///      Describes how the Add-in is being unloaded.
		/// </param>
		/// <param term='custom'>
		///      Array of parameters that are host application specific.
		/// </param>
		/// <seealso class='IDTExtensibility2' />
		public void OnDisconnection(Extensibility.ext_DisconnectMode disconnectMode, ref System.Array custom)
		{
			if(disconnectMode != Extensibility.ext_DisconnectMode.ext_dm_HostShutdown)
			{
				OnBeginShutdown(ref custom);
			}
		}
		#endregion

		#region OnAddInsUpdate
		/// <summary>
		///      Implements the OnAddInsUpdate method of the IDTExtensibility2 interface.
		///      Receives notification that the collection of Add-ins has changed.
		/// </summary>
		/// <param term='custom'>
		///      Array of parameters that are host application specific.
		/// </param>
		/// <seealso class='IDTExtensibility2' />
		public void OnAddInsUpdate(ref System.Array custom)
		{
		}
		#endregion

		#endregion

		#region OnStartupComplete
		/// <summary>
		///      Implements the OnStartupComplete method of the IDTExtensibility2 interface.
		///      Receives notification that the host application has completed loading.
		/// </summary>
		/// <param term='custom'>
		///      Array of parameters that are host application specific.
		/// </param>
		/// <seealso class='IDTExtensibility2' />
		public void OnStartupComplete(ref System.Array custom)
		{
			bool bSuccess;

			// Initialize work-around handler for OnBeginShutdown
			// m_oCleanup.InitHandler(m_OutlookApp.Application);

			// Initialize paths and logging information, licensing parameters and database connection settings
			bSuccess = OnStartupComplete_TGPInit();

			// Indicate that plugin has started
			LogAlert("Start", EVT_PROGRAMNAME + " active" + " (" + svrAnalyzer.LicenseType + ")", null);

			// Yield to show results
			Application.DoEvents();			
		}
		#endregion

		#region OnStartupComplete_TGPInit
		private bool OnStartupComplete_TGPInit()
		{
			// Initialize paths and logging information
			try
			{
				// Get PlugIn startup directory
				m_PlugInPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
				if (!m_PlugInPath.EndsWith("\\")) m_PlugInPath = m_PlugInPath + "\\";

				// Make sure paths are relative to the startup directory
				Directory.SetCurrentDirectory(m_PlugInPath);

				// Set Configuration directory
				m_ConfigFilePath = m_PlugInPath + DIR_CONFIG;

				// Set Resources directory
				m_ResourceFilePath = m_PlugInPath + DIR_RESOURCES;

				// Set up log file so any future errors can be reported
				string LogFilePath = m_PlugInPath + DIR_LOGS;

				// Set log file location, throw exception on error
				if (!Directory.Exists(LogFilePath)) Directory.CreateDirectory(LogFilePath);

				// Initialize the local logging component
				m_LogTools = new LogLocal(EVT_PROGRAMNAME, LogFilePath, FILE_ALERTS, FILE_ANALYZER);

				// Set user User Appplication Data root directory, throw exception on error
				m_UserDataPath = m_PlugInPath + DIR_USERDATA + DIR_USERNAME;
				if (!Directory.Exists(m_UserDataPath)) Directory.CreateDirectory(m_UserDataPath);
			}
			catch(Exception ex)
			{
				m_LogTools.WriteWinAppEvent ("Cannot log alerts, " + ex.Message, EventLogEntryType.Error);
				MessageBox.Show ("Cannot log alerts, " + ex.Message, EVT_PROGRAMNAME);

				// Return failure
				return (false);
			}						

			// Prepare the InBoxAnalyzer Queue immediately so no evetns are lost;
			// this will queue any incoming messages for analysis when ready.
			try
			{
				// Initialize the TekGuard PlugIn Outlook Assistant
				m_Assist.OnLogException += new TGPAssist.LogExceptionEventHandler(this.LogException);

				// Initialize m_QueueAnalyzer
				m_InBoxQueue = new InBoxQueue(m_Assist);
				m_Assist.OnMessageAnalyze += new TGPAssist.MessageAnalyzeEventHandler(MessageAnalyzeQueued);

				// Initialize Outlook events
				// Set the InBox new item event; Use TekGuard memory-safe item list
				m_InBoxQueue.OnItemAdd += new Outlook.ItemsEvents_ItemAddEventHandler(Outlook_InBox_ItemAdd);

				// Yield for user interface
				Application.DoEvents();			
			}
			catch(Exception ex)
			{
				m_LogTools.WriteWinAppEvent ("Cannot initialize Analyzer Queuing components" + ", " + ex.Message, EventLogEntryType.Error);
				MessageBox.Show ("Cannot initialize Analyzer Queuing components" + ", " + ex.Message, EVT_PROGRAMNAME);

				// Return failure
				return (false);
			}

			// Get bootstrap initialization settings
			try
			{
				string ErrorText;
				if (!m_Bootstrap.Initialize(svrConnector, m_ConfigFilePath, m_UserDataPath, out ErrorText))
				{
					m_LogTools.WriteWinAppEvent (ErrorText, EventLogEntryType.Error);
					MessageBox.Show (ErrorText, EVT_PROGRAMNAME);
					return (false);
				}
			}
			catch(Exception ex)
			{
				m_LogTools.WriteWinAppEvent ("Error setting bootstrap initialization settings (see log file for details), " + ex.Message, EventLogEntryType.Error);
				MessageBox.Show ("Error setting bootstrap initialization settings (see log file for details), " + ex.Message, EVT_PROGRAMNAME);
				return (false);
			}

			// Bootstrap initialization complete, get and set configuration parameters
			try
			{
				string ErrorText;

				if (!m_Bootstrap.Initialize(svrController, out ErrorText))
				{
					m_LogTools.WriteWinAppEvent (ErrorText, EventLogEntryType.Error);
					MessageBox.Show (ErrorText, EVT_PROGRAMNAME);
					return (false);
				}
				if (!m_Bootstrap.Initialize(svrAnalyzer, out ErrorText))
				{
					m_LogTools.WriteWinAppEvent (ErrorText, EventLogEntryType.Error);
					MessageBox.Show (ErrorText, EVT_PROGRAMNAME);
					return (false);
				}

				// Initialize category information
				Controller_OnConfigurationApply (null);

				// Yield for user interface
				Application.DoEvents();			
			}
			catch(Exception ex)
			{
				m_LogTools.WriteWinAppEvent ("Error setting configuration parameters, " + ex.Message, EventLogEntryType.Error);
				MessageBox.Show ("Error setting configuration parameters, " + ex.Message, EVT_PROGRAMNAME);
				return (false);
			}

			// Initialize command bar buttons
			try
			{
				// Use the TekGuard memory-safe command bar
				TGPOutlookApp.TGPExplorer.TGPCommandBar oStandardBar = new TGPOutlookApp.TGPExplorer.TGPCommandBar("Standard");

				// Set up a custom Dashboard button on the "Standard" commandbar.
				CommandButtonAdd (oStandardBar.Get, ref m_btnController, CAP_EMIDASH, "TekGuard Dashboard", CAP_EMIDASH, FILE_BUTTON, new Microsoft.Office.Core._CommandBarButtonEvents_ClickEventHandler(this.m_btnController_Click), true);

				// Is the TekGuard Outlook Application Assistant enabled?
				bool bEnabled = m_OutlookApp.Enabled;
				if (!bEnabled) LogAlert("Start", "TekGuard Outlook Application Assistant not enabled", null);

				// Set up the Category buttons
				CommandButtonAdd (oStandardBar.Get, ref m_btnIsGood, CAP_ISGOOD, "This Is Good", CATIDX_GOOD.ToString(), FILE_BUTTON, new Microsoft.Office.Core._CommandBarButtonEvents_ClickEventHandler(this.m_btnIsGood_Click), bEnabled);
				CommandButtonAdd (oStandardBar.Get, ref m_btnIsJunk, CAP_ISJUNK, "This Is Junk", CATIDX_JUNK.ToString(), FILE_BUTTON, new Microsoft.Office.Core._CommandBarButtonEvents_ClickEventHandler(this.m_btnIsJunk_Click), bEnabled);

				// Set up a custom Analyze button on the "Standard" commandbar.
				CommandButtonAdd (oStandardBar.Get, ref m_btnAnalyze, CAP_ANALYZE, "Analyze Messages", "Analyze", FILE_BUTTON, new Microsoft.Office.Core._CommandBarButtonEvents_ClickEventHandler(this.m_btnAnalyze_Click), false);

				// Yield for user interface
				Application.DoEvents();			
			}
			catch(Exception ex)
			{
				m_LogTools.WriteWinAppEvent ("Cannot initialize command bar buttons" + ", " + ex.Message, EventLogEntryType.Error);
				MessageBox.Show ("Cannot initialize command bar buttons" + ", " + ex.Message, EVT_PROGRAMNAME);

				// Return failure
				return (false);
			}
			
			// New version available?
			m_Bootstrap.TGPCheckUpdates();

			// Initialize Token List (threaded - may take some time)
			m_TokenList = svrAnalyzer.TokenCountInitialize();

			// Return success
			return (true);
		}
		
		#region CommandButtonAdd
		private bool CommandButtonAdd (CommandBar oCommandBar, ref CommandBarButton btnButton, string Name, string ToolTip, string Tag, string IconFile, Microsoft.Office.Core._CommandBarButtonEvents_ClickEventHandler evtHandler, bool bEnabled)
		{
			// In case the button was not deleted, use the existing one.
			try
			{
				btnButton = (CommandBarButton)oCommandBar.Controls[Name];
			}
			catch(Exception)
			{
				object omissing = System.Reflection.Missing.Value ;
				
				// Create the buttons; set the Temporary attribute to true
				btnButton = (CommandBarButton) oCommandBar.Controls.Add(1, omissing, omissing, omissing, true);
				btnButton.Caption = Name;
				btnButton.TooltipText = ToolTip;
			}

			// Load the button image
			try
			{
				// Indicate the button style
				btnButton.Style = MsoButtonStyle.msoButtonIconAndCaption;

				// Outlook work-around: cut and paste to clipboard for the button image
				Image image = System.Drawing.Image.FromFile(m_ResourceFilePath + IconFile);
				Clipboard.SetDataObject(image);
				btnButton.PasteFace();

				// Bitmap bmp = new Bitmap(m_ResourceFilePath + IconFile);
				// stdole.IPictureDisp objPicture;
				// stdole.IPictureDisp objMask;

				// objPicture = ImageConverter.ImageToIpicture(System.Drawing.Image.FromFile(m_PlugInPath + DIR_CONFIG + "EMIPlugIn_0.bmp"));
				// objMask = ImageConverter.ImageToIpicture(System.Drawing.Image.FromFile(m_PlugInPath + DIR_CONFIG + "EMIPlugIn_1.bmp"));

				// btnButton.Picture = objPicture;
				// btnButton.Mask = objMask;
			}
			catch
			{
				btnButton.Style = MsoButtonStyle.msoButtonCaption;
			}
			
			try
			{
				// The following items are optional, but recommended. 
				// The Tag property lets you quickly find the control 
				// and helps MSO keep track of it when more than
				// one application window is visible. The property is required
				// by some Office applications and should be provided.
				btnButton.Tag = Tag;

				// The OnAction property is optional but recommended. 
				// It should be set to the ProgID of the add-in, so that if
				// the add-in is not loaded when a user presses the button,
				// MSO loads the add-in automatically and then raises
				// the Click event for the add-in to handle. 
				
				btnButton.OnAction = "!<TGPlugIn." + Name + ">";
				btnButton.Click += evtHandler;
				btnButton.Enabled = bEnabled;
				btnButton.Visible = true;
			}
			catch(Exception ex)
			{
				LogException(new System.Diagnostics.StackFrame(), ex.Message, null);
				return false;
			}

			return true;
		}

		#endregion

		#endregion

		#region OnBeginShutdown
		/// <summary>
		///      Implements the OnBeginShutdown method of the IDTExtensibility2 interface.
		///      Receives notification that the host application is being unloaded.
		/// </summary>
		/// <param term='custom'>
		///      Array of parameters that are host application specific.
		/// </param>
		/// <seealso class='IDTExtensibility2' />
		public void OnBeginShutdown(ref System.Array custom)
		{
			// Indicate that plugin is unloading
			LogAlert("Stop", EVT_PROGRAMNAME + " unloading", null);

			// Clear the InBox event Handler, dispose of the object
			m_InBoxQueue.OnItemAdd -= new Outlook.ItemsEvents_ItemAddEventHandler(Outlook_InBox_ItemAdd);
			m_InBoxQueue.Dispose();

			// Dispose of the old category folder objects (if created)
			if (m_CategoryFolder != null) for (int ii=0; ii<m_CategoryFolder.Length; ii++)
			{
				  TGPOutlookApp.DisposeObject(m_CategoryFolder[ii]);
			}

			// Write the token database binary to disk (if dirty)
			svrAnalyzer.TokenCountPersist();

			// Write the token database XML backup to disk (if dirty)
			svrAnalyzer.TokenCountBackup();

			// Terminate EMail Analyzer control
			svrAnalyzer.Terminate();

			// Terminate Controller Interface
			svrController.Terminate();

			// Terminate the application
			m_OutlookApp.Dispose();
				
		}

		#endregion
		
		#region Properties

		#region Sponsors
		public string[] Sponsors 
		{
			set { svrController.Sponsors = value; }
		}
		#endregion

		#endregion

		#region SelectedMark
		private void SelectedMark(int CategoryIndex)
		{
			// Get the selected items and loop through each message; Use TekGuard memory-safe item list
			TGPOutlookApp.TGPExplorer.TGPItems TGPItems = new TGPOutlookApp.TGPExplorer.TGPItems ();
			
			// Anything to do?
			if (TGPItems.Selection.Count == 0) return;

			// Get the CategoryID for this Index
			Int32 CategoryID = m_AnalyzerCategories[CategoryIndex].CategoryID;
			
			// Get Category Entry
			TGPConnector.CategoryEntry CategoryEntry = m_AnalyzerCategories[CategoryIndex];

			// Indicate update processing beginning
			svrAnalyzer.TokenCountBegin(TGPItems.Selection.Count, NOTIFY_PROGRESS, m_AnalyzerCategories.Count);
			using (TGPItems) for (int ii=1; ii <= TGPItems.Selection.Count; ii++) try
			{
				// Only handle MailItems
				Outlook.MailItem MailItem = TGPItems[ii];
				if (MailItem == null) continue;

				// Get the Internet Transport Headers and the previous TekGuard Category ID (if present)
				// Note: Do not set the TekGuard Header entry for marked items; Analyzed only
				Int32  PreviousID;
				string Headers = TGPOutlookApp.TGPExplorer.TGPItems.TGPHeaderGet(MailItem, out PreviousID);

				// Set TekGuard PlugIn Marker in the internet header
				TGPOutlookApp.TGPExplorer.TGPItems.TGPHeaderSet(MailItem, DEFAULT_USERID, CategoryEntry.CategoryID, PreviousID, true);
						
				// Get the Sender Address
				string SenderAddress = TGPOutlookApp.TGPExplorer.TGPItems.SenderAddress(MailItem);

				// Update Analyzer category counts
				svrAnalyzer.TokenCountUpdate(CategoryID, CategoryIndex, Headers, TGPOutlookApp.TGPExplorer.TGPItems.SenderName(MailItem), SenderAddress, MailItem.Subject, TGPOutlookApp.TGPExplorer.TGPItems.Body(MailItem), MailItem.ReceivedTime);
				
				// Set MailFrom category for this sender address
				// Note: TGPlugIn only sets the Analyzer sender address for explicitly marked items
				svrAnalyzer.AnalyzerMailFromSet(SenderAddress, m_AnalyzerCategories[CategoryIndex].CategoryID);

				// Mark as read
				// Note: Outlook caches properties and doesn't show changes in the UI for some time period.
				MailItem.UnRead = false;
				MailItem.Save();

				// Move this to the appropriate folder (if requested)
				MoveItem(MailItem, CategoryIndex, PreviousID);
								
				// Release all COM objects
				TGPOutlookApp.DisposeObject(MailItem);
				TGPOutlookApp.DisposeObject();
							
				// Yield to show results
				Application.DoEvents();			
			}
			catch(Exception ex)
			{
				LogException(new System.Diagnostics.StackFrame(), ex.Message, null);
			}

			// Write the tokens to the database
			svrAnalyzer.TokenCountCommit(CategoryID, CategoryIndex);
		
			// Indicate percent complete
			if (!m_btnAnalyze.Enabled) m_btnAnalyze.Caption = svrAnalyzer.TokenCountReady().ToString("p");
		}
		#endregion

		#region SelectedAnalyze
		private void SelectedAnalyze()
		{
			// Get the selected items and loop through each message; Use TekGuard memory-safe item list
			TGPOutlookApp.TGPExplorer.TGPItems TGPItems = new TGPOutlookApp.TGPExplorer.TGPItems ();
			
			// Get unread e-mail messages.
			// TGPItems[ii] = TGPItems[ii].Restrict("[Unread] = true");

			// Indicate update processing beginning
			svrAnalyzer.TokenCountBegin(TGPItems.Selection.Count, NOTIFY_PROGRESS, m_AnalyzerCategories.Count);

			// Loop through each message
			using (TGPItems) for (int ii=1; ii <= TGPItems.Selection.Count; ii++)
			{
				// Only handle MailItems
				Outlook.MailItem MailItem = TGPItems[ii];
				if (MailItem == null) continue;

				// Analyze and mark this message
				MessageAnalyze(MailItem, false);

				// Release the COM object
				TGPOutlookApp.DisposeObject(MailItem);
				TGPOutlookApp.DisposeObject();
				
				// Yield to show results
				Application.DoEvents();			
			}
		}
		#endregion

		#region AnalyzerRebuild
		private bool AnalyzerRebuild(Outlook.MAPIFolder objFolder, int CategoryIndex, WinFormInvokeHandler AnalyzerStatusUpdate, ref AnalyzerStatusEventArgs AnalyzerStatus)
		{
			// Anything to do?
			if (objFolder.Items.Count == 0) return (true);

			// Get the CategoryID for this Index
			Int32 CategoryID = m_AnalyzerCategories[CategoryIndex].CategoryID;
			
			// Get Category Entry
			TGPConnector.CategoryEntry CategoryEntry = m_AnalyzerCategories[CategoryIndex];

			// Indicate update processing beginning
			svrAnalyzer.TokenCountBegin(objFolder.Items.Count, NOTIFY_PROGRESS, m_AnalyzerCategories.Count);

			// Retrieve all items in this MailBox
			Outlook.Items FolderItems = objFolder.Items;
			Outlook.MailItem MailItem = (Outlook.MailItem) FolderItems.GetFirst();
			for (int ii=0; ii<FolderItems.Count; ii++) try
			{
				// Yield to get and show results
				Application.DoEvents();			

				// Did the user press "Cancel"?
				if (!AnalyzerRebuildContinue(AnalyzerStatusUpdate, ref AnalyzerStatus))
				{
					// Release all COM objects
					TGPOutlookApp.DisposeObject(MailItem);
					TGPOutlookApp.DisposeObject();

					// Stop processing
					return (false);
				}

				// Is this a MailItem?
				if (MailItem == null) 
				{
					// Get the next Item
					MailItem = (Outlook.MailItem) FolderItems.GetNext();
					continue;
				}

				// Get the Internet Transport Headers and the previous TekGuard Category ID (if present)
				// Note: Do not set the TekGuard Header entry for marked items; Analyzed only
				Int32  PreviousID;
				string Headers = TGPOutlookApp.TGPExplorer.TGPItems.TGPHeaderGet(MailItem, out PreviousID);

				// Set TekGuard PlugIn Marker in the internet header
				TGPOutlookApp.TGPExplorer.TGPItems.TGPHeaderSet(MailItem, DEFAULT_USERID, CategoryEntry.CategoryID, PreviousID, true);
						
				// Get the Sender Address
				string SenderAddress = TGPOutlookApp.TGPExplorer.TGPItems.SenderAddress(MailItem);

				// Update Analyzer category counts
				svrAnalyzer.TokenCountUpdate(CategoryID, CategoryIndex, Headers, TGPOutlookApp.TGPExplorer.TGPItems.SenderName(MailItem), TGPOutlookApp.TGPExplorer.TGPItems.SenderAddress(MailItem), MailItem.Subject, TGPOutlookApp.TGPExplorer.TGPItems.Body(MailItem), MailItem.ReceivedTime);
				
				// Set MailFrom category for this sender address
				// Note: TGPlugIn sets the Analyzer sender address for explicitly marked items only
				svrAnalyzer.AnalyzerMailFromSet(SenderAddress, m_AnalyzerCategories[CategoryIndex].CategoryID);

				// Release all COM objects
				TGPOutlookApp.DisposeObject(MailItem);
				TGPOutlookApp.DisposeObject();

				// Get the next Item
				MailItem = (Outlook.MailItem) FolderItems.GetNext();
			}
			catch(Exception ex)
			{
				LogException(new System.Diagnostics.StackFrame(), ex.Message, null);

				// Keep going...
				MailItem = (Outlook.MailItem) FolderItems.GetNext();
			}

			// Write these tokens to the database
			svrAnalyzer.TokenCountCommit(CategoryID, CategoryIndex);

			// Continue processing
			return (true);
		}
		private bool AnalyzerRebuildContinue(WinFormInvokeHandler AnalyzerStatusUpdate, ref AnalyzerStatusEventArgs AnalyzerStatus)
		{
			// Indicate another item completed
			AnalyzerStatus.Current++;

			try 
			{
				// Pack invocation arguments from caller thread
				object[] objArray = new object[1];
				objArray[0] = new object[] {AnalyzerStatus};

				// Indicate percent complete
				// bool bContinue = svrConnector.AnalyzerStatusUpdate(this, AnalyzerStatus);
				bool bContinue = (bool) AnalyzerStatusUpdate (objArray);

				if (!bContinue) 
				{
					if (DialogResult.Yes == MessageBox.Show ("Cancel Analyzer Rebuild?", EVT_PROGRAMNAME, MessageBoxButtons.YesNo))
					{
						// Indicate cancel
						return (false);
					}
				}
			}
			catch(Exception ex)
			{
				ex = ex;
			}

			// Continue
			return (true);
		}
		#endregion

		#region MessageAnalyze
		private void MessageAnalyzeQueued(object Sender, Outlook.MailItem[] MailItems)
		{
			// Analyze, mark, and release this queued message
			svrAnalyzer.TokenCountBegin(1, NOTIFY_PROGRESS, m_AnalyzerCategories.Count);

			// Loop through each message
			foreach (Outlook.MailItem Item in MailItems) 
			{
				MessageAnalyze(Item, true);
				
				// Yield to show results
				Application.DoEvents();			
			}
		}
		private void MessageAnalyze(Outlook.MailItem MailItem, bool bQueued)
		{
			try
			{
				// Get the Internet Transport Headers and the previous TekGuard Category ID (if present)
				Int32 PreviousID;
				string Headers = TGPOutlookApp.TGPExplorer.TGPItems.TGPHeaderGet(MailItem, out PreviousID);

				// Was this a Queued item? If so, don't analyze if previously marked
				if (bQueued && (PreviousID != 0)) return;

				// Get the Sender Address
				string SenderAddress = TGPOutlookApp.TGPExplorer.TGPItems.SenderAddress(MailItem);
				string SenderName	 = TGPOutlookApp.TGPExplorer.TGPItems.SenderName(MailItem);

				// Get MailFrom category for this sender address
				int CategoryIndex = svrAnalyzer.AnalyzerMailFromGet(m_AnalyzerCategories, SenderAddress);

				// MailFrom not found?
				if (CategoryIndex < 0)
				{
					// Get category statistical results for this message
					CategoryIndex = svrAnalyzer.MessageCheckUpdate(m_AnalyzerCategories, Headers, SenderName, SenderAddress, MailItem.Subject, TGPOutlookApp.TGPExplorer.TGPItems.Body(MailItem));

					// No MailFrom and no statistical determination?
					if (CategoryIndex < 0) return;
				}

				// Get Category Entry
				TGPConnector.CategoryEntry CategoryEntry = m_AnalyzerCategories[CategoryIndex];

				// Set TekGuard PlugIn Marker in the internet header
				TGPOutlookApp.TGPExplorer.TGPItems.TGPHeaderSet(MailItem, DEFAULT_USERID, CategoryEntry.CategoryID, PreviousID, false);
						
				// Update category counts
				// svrAnalyzer.TokenCountBegin(m_TokenList, USERID_DEF, ii, 1, NOTIFY_PROGRESS);
				svrAnalyzer.TokenCountUpdate(CategoryEntry.CategoryID, CategoryIndex, Headers, SenderName, SenderAddress, MailItem.Subject, TGPOutlookApp.TGPExplorer.TGPItems.Body(MailItem), MailItem.ReceivedTime);
				svrAnalyzer.TokenCountCommit(CategoryEntry.CategoryID, CategoryIndex);
				
				// Note: TGPlugIn only sets the Analyzer MailFrom address for explicitly marked [SelectedMark()] items
				// Set value on Subject Line (if requested for this category)
				// MailItem.Subject = MarkUp.Mark(USERID_DEF, m_TokenList.CATEGORYLIST[ii], MailItem.Subject, Percent[ii]);
						
				// Mark as read (if requested for this category)
				if (CategoryEntry.AutoRead) MailItem.UnRead = false;

				// Note: Outlook will not save the header changes unless it see some MailItem change
				MailItem.UnRead = !MailItem.UnRead;
				MailItem.Save();
				MailItem.UnRead = !MailItem.UnRead;
				MailItem.Save();

				// Move this to the appropriate folder (if requested)
				MoveItem(MailItem, CategoryIndex, PreviousID);
			}
			catch(Exception ex)
			{
				LogException(new System.Diagnostics.StackFrame(), ex.Message, null);
			}
		}
		#endregion

		#region MoveItem
		private void MoveItem(Outlook.MailItem MailItem, Int32 CategoryIndex, Int32 PreviousID)
		{
			try
			{
				// Move this to the appropriate folder (if requested)
				if (m_AnalyzerCategories[CategoryIndex].AutoMove) 
				{				
					// New location?
					if (m_CategoryFolder[CategoryIndex].EntryID != ((Outlook.MAPIFolder) MailItem.Parent).EntryID) 
					{
						MailItem.Move(m_CategoryFolder[CategoryIndex]);
					}
				}
				else
				{
					// The designated category is not set for AutoMove. However, get the
					// destination mailbox and only move if this item is in the wrong box.
					// Move this to the appropriate folder (if previously moved)
					if (PreviousID != 0) 
					{
						// Get the index for the previous CategoryID
						int PreviousIndex = m_AnalyzerCategories.IndexFromID(PreviousID);

						// Was AutoMove set for the old Category?
						if (m_AnalyzerCategories[PreviousIndex].AutoMove)
						{
							MailItem.Move(m_CategoryFolder[CategoryIndex]);
						}
					}
				}
			}
			catch(Exception ex)
			{
				LogException(new System.Diagnostics.StackFrame(), ex.Message, null);
			}
		}
		#endregion

		#region PerformanceMonitor
		private void PerformanceMonitor(object sender, System.Timers.ElapsedEventArgs e)
		{
			try 
			{
				// New version available?
				m_Bootstrap.TGPCheckUpdates();

				// Time to persist tokens and run backups?
				int iMinute = DateTime.Now.Minute;

				// Persist every 15 minutes, run backups on the half-hour
				if ((iMinute % PERSIST_PERIOD) == 0)
				{
					// Write the token database binary to disk (if dirty)
					svrAnalyzer.TokenCountPersist();

					// Write the token database XML backup to disk (if dirty)
					if (iMinute == PERSIST_BACKUP) svrAnalyzer.TokenCountBackup();
				}
				
				// AJM Test: Write sample update for performance testing
				// static int iTest;
				// svrController.FireDisplayNotify(LogDisplayType.BytesIn, iTest++.ToString());
			}
			catch(Exception ex)
			{
				LogException(new System.Diagnostics.StackFrame(), ex.Message, null);
			}
		}
		#endregion

		#region Button Click Events
		private void m_btnAnalyze_Click(CommandBarButton cmdBarbutton, ref bool cancel) 
		{
			SelectedAnalyze();
		}

		private void m_btnIsGood_Click(CommandBarButton cmdBarbutton, ref bool cancel) 
		{
			SelectedMark(Convert.ToInt32(cmdBarbutton.Tag));
		}

		private void m_btnIsJunk_Click(CommandBarButton cmdBarbutton, ref bool cancel) 
		{
			SelectedMark(Convert.ToInt32(cmdBarbutton.Tag));
		}

		private void m_btnController_Click(CommandBarButton cmdBarbutton, ref bool cancel) 
		{
			// Analyzer Ready? Update Controller statistics
			if (!m_btnAnalyze.Enabled) svrController.DisplayPercentReady (svrAnalyzer.TokenCountReady().ToString("P"));

			// Show the Controller monitor window
			svrController.Visible = true;
		}
		#endregion

		#region Log Event Callbacks

		#region LogAlert
		internal void LogAlert (string Sender, string Message, string[][] Params)
		{
			try
			{
				// Log the event to a file
				m_LogTools.WriteAlert(Sender, Message, Params);
			
				// Display the event
				svrController.DisplayLogAlert(Message);
			}
			catch(Exception ex)
			{
				m_LogTools.WriteWinAppEvent ("Cannot log alert, " + ex.Message, EventLogEntryType.Warning);
			}
		}
		#endregion

		#region LogException
		internal void LogException (StackFrame Sender, string Message, string[][] Params)
		{
			try
			{
				// Log the event to a file
				m_LogTools.WriteAlert(Sender, Message, Params);
			
				// Display the event
				svrController.DisplayLogException(Message);
			}
			catch(Exception ex)
			{
				m_LogTools.WriteWinAppEvent ("Cannot log alert, " + ex.Message, EventLogEntryType.Warning);
			}
		}
		#endregion

		#region LogException_NoDisplay
		internal void LogException_NoDisplay (StackFrame Sender, string Message, string[][] Params)
		{
			try
			{
				// Log the event to a file, but do not Display the event
				// This function is used when inside a Display function
				// to avoid a Display error infinite loop.
				m_LogTools.WriteAlert(Sender, Message, Params);
			}
			catch(Exception ex)
			{
				m_LogTools.WriteWinAppEvent ("Cannot log alert, " + ex.Message, EventLogEntryType.Warning);
			}
		}
		#endregion

		#region LogAnalyzer
		internal void LogAnalyzer(object Sender, double[] Percent, string[] CategoryProbList) 
		{
			try
			{
				const	string		CRLF				= "\r\n";
				const	string		COMMA				= ", ";

				string FileText = ""; 
				string DispText = ""; 

				for (int ii=0; ii<Percent.Length; ii++)
				{
					FileText = Percent[ii].ToString("0.000") + " ";
					DispText = Percent[ii].ToString("0.000") + " ";
				}
				FileText += CRLF; 
				DispText += COMMA; 

				for (int ii=0; ii<CategoryProbList.Length; ii++)
				{
					FileText = FileText + CategoryProbList[ii] + CRLF;
					DispText = DispText + CategoryProbList[ii] + COMMA;
				}

				// Log the event to a file
				m_LogTools.WriteAnalyzer(Sender.ToString(), FileText);
			
				// Display the event
				svrController.DisplayLogAnalyzer(DispText);
			}
			catch(Exception ex)
			{
				m_LogTools.WriteWinAppEvent ("Cannot log analyzer activity, " + ex.Message, EventLogEntryType.Warning);
			}
		}
		#endregion

		#endregion

		#region Outlook Event Callbacks

		#region TGPResolveEventHandler
		private Assembly TGPResolveEventHandler(object sender, ResolveEventArgs args)
		{
			// This handler is called only when the common language runtime tries to bind to the assembly and fails.
			if (args.Name.StartsWith("TGPAnalyzer"))
			{
				string AssemblyPath = m_PlugInPath + "TGPAnalyzer.dll";
				return (Assembly.LoadFrom(AssemblyPath));
			}
			if (args.Name.StartsWith("TGPAssist"))
			{
				string AssemblyPath = m_PlugInPath + "TGPAssist.dll";
				return (Assembly.LoadFrom(AssemblyPath));
			}
			if (args.Name.StartsWith("TGPConnector"))
			{
				string AssemblyPath = m_PlugInPath + "TGPConnector.dll";
				return (Assembly.LoadFrom(AssemblyPath));
			}
			if (args.Name.StartsWith("TGPController"))
			{
				string AssemblyPath = m_PlugInPath + "TGPController.dll";
				return (Assembly.LoadFrom(AssemblyPath));
			}
			return (null);
		}
		#endregion

		#region Outlook_InBox_ItemAdd
		private void Outlook_InBox_ItemAdd(object item) 
		{
			try
			{
				// Make sure this is a valid mail item
				if (!(item is Outlook.MailItem)) return;

				// Save it in the new folder location
				Outlook.MailItem MailItem = (Outlook.MailItem) item;

				// Get the item EntryID and StoreID
				string strEntryID = MailItem.EntryID;
				string strStoreID = ((Outlook.MAPIFolder)MailItem.Parent).StoreID;

				// Release all COM objects
				TGPOutlookApp.DisposeObject(MailItem);
				TGPOutlookApp.DisposeObject();

				// Add this new item to the Analyzer Queue
				m_InBoxQueue.Enqueue (strEntryID, strStoreID);

			}
			catch(Exception ex)
			{
				LogException(new System.Diagnostics.StackFrame(), ex.Message, null);
			}
		} 
		#endregion

		#endregion
	
		#region Analyzer Event Callbacks

		#region svrAnalyzer_OnEnableDisable
		private bool svrAnalyzer_OnEnableDisable(object sender, bool AnalyzeEnable, bool IsEnable)
		{
			// Analyzer deactivating?
			if (m_btnAnalyze.Enabled && !AnalyzeEnable)
			{
				m_InBoxQueue.Stop();
				m_btnAnalyze.Enabled = false;
				return (false);
			}

			// Is this the first time Analyzer is not ready?
			if (!AnalyzeEnable) 
			{
				if (m_AnalyzerFirst)
				{
					// Ask the user to train the analyzer (if m_OutlookApp is enabled)
					// Re-check the analyzer readiness training count
					if (m_btnIsGood.Enabled || m_btnIsJunk.Enabled)
					{
						AnalyzeEnable = Controller_OnAnalyzerRebuildStart();
					}

					// Indicate user has been notified
					m_AnalyzerFirst = false;
				}
				// Indicate percent complete
				m_btnAnalyze.Caption = svrAnalyzer.TokenCountReady().ToString("p");
			}

			// Activate the Analyzer InBox Queue Checker the first time
			if (!m_btnAnalyze.Enabled && AnalyzeEnable)
			{
				// Reset caption
				m_btnAnalyze.Caption = CAP_ANALYZE;
				m_InBoxQueue.Start();
			}
			
			// Indicate whether analyzer is enabled (if m_OutlookApp is enabled)
			m_btnAnalyze.Enabled = AnalyzeEnable && (m_btnIsGood.Enabled || m_btnIsJunk.Enabled);

			// Update Controller statistics
			svrController.DisplayPercentReady (svrAnalyzer.TokenCountReady().ToString("P"));

			return (true);
		}
		#endregion

		#endregion

		#region Connector Remoted Controller Event Callbacks

		#region Controller_OnConfigurationApply

		private object Controller_OnConfigurationApply(object sender, object args)
		{
			return (m_OutlookApp.Invoke(new OutlookInvokeEventHandler(Controller_OnConfigurationApply), args));
		}

		private object Controller_OnConfigurationApply(object args)
		{
			// Initialize category information
			try
			{
				// Get the Category settings
				m_AnalyzerCategories = svrAnalyzer.AnalyzerCategoriesGet();

				// Dispose of the old category folder objects (if created)
				if (m_CategoryFolder != null) for (int ii=0; ii<m_CategoryFolder.Length; ii++)
				{
					TGPOutlookApp.DisposeObject(m_CategoryFolder[ii]);
				}

				// Build the mailboxes
				m_CategoryFolder = new Outlook.MAPIFolder[m_AnalyzerCategories.Count];
				
				for (int ii=0; ii<m_AnalyzerCategories.Count; ii++)
				{
					TGPConnector.CategoryEntry CategoryEntry = m_AnalyzerCategories[ii];
					
					// Skip?
					if (!CategoryEntry.AutoMove) 
					{
						// Set default to Inbox
						m_CategoryFolder[ii] = m_OutlookApp.OutlookApp.Session.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderInbox);
						continue;
					}

					// Get or create the folder
					TGPOutlookApp.CreateFolder (Outlook.OlDefaultFolders.olFolderInbox, CategoryEntry.MailBoxName, out m_CategoryFolder[ii]);
				}
			}
			catch(Exception ex)
			{
				m_LogTools.WriteWinAppEvent ("Cannot initialize category information" + ", " + ex.Message, EventLogEntryType.Error);
				MessageBox.Show ("Cannot initialize category information" + ", " + ex.Message, EVT_PROGRAMNAME);

				// Return failure
				return(null);
			}

			return(null);
		}
		#endregion

		#region Controller_OnAnalyzerRebuildStart
		private bool Controller_OnAnalyzerRebuildStart()
		{
			string Msg = "Your Analyzer is " + svrAnalyzer.TokenCountReady().ToString("p") + " ready."
				+ " Would you like to train it now?";
			if (DialogResult.Yes == MessageBox.Show (Msg, "TekGuard Analyzer", MessageBoxButtons.YesNo))
			{
				// Invoke the Analyzer Rebuild form
				m_OutlookApp.Invoke(new OutlookInvokeEventHandler(Controller_OnAnalyzerRebuildStart), null);

				// Re-check the analyzer training count
				return (svrAnalyzer.TokenCountReady() >= 1.0);
			}
			return (false);
		}
		private object Controller_OnAnalyzerRebuildStart(object args)
		{
			m_AnalyzerRebuild = svrController.DisplayRebuildWizard(frmSplash.getAssemblyTitle(), frmSplash.getAssemblyIcon());
			m_AnalyzerRebuild.ShowDialog();
			return (null);
		}
		private object Controller_OnAnalyzerRebuildItemize(object sender, object args)
		{
			return (m_OutlookApp.Invoke(new OutlookInvokeEventHandler(Controller_OnAnalyzerRebuildItemize), args));
		}
		private object Controller_OnAnalyzerRebuildItemize(object args)
		{
			// List all the root folders containing MailItems
			ArrayList Folders = new ArrayList();

			// Find all the "root" folders
			// Note: Outlook 2002 will not detect "while (RootFolder != null)"
			Outlook.MAPIFolder RootFolder = m_OutlookApp.OutlookApp.Session.Folders.GetFirst();
			for (int ii = m_OutlookApp.OutlookApp.Session.Folders.Count; ii>0; ii--)
			{
				if (RootFolder.DefaultItemType == Outlook.OlItemType.olMailItem) Folders.Add (RootFolder);
				RootFolder = m_OutlookApp.OutlookApp.Session.Folders.GetNext();
			}

			// Generate the Folder TreeView data
			FolderTreeView FolderTree = new FolderTreeView(Folders); 

			// Dispose of the old folder objects
			foreach (Outlook.MAPIFolder Folder in Folders) TGPOutlookApp.DisposeObject(Folder);

			// Return the serializeable list of folders
			return ((TreeSerializer) FolderTree);
		}
		#endregion

		#region Controller_OnAnalyzerRebuildExecute
		private object Controller_OnAnalyzerRebuildExecute(object sender, object args)
		{
			return (m_OutlookApp.Invoke(new OutlookInvokeEventHandler(Controller_OnAnalyzerRebuildExecute), args));
		}

		private object Controller_OnAnalyzerRebuildExecute(object args)
		{
			// Indicate the status of items to processed
			AnalyzerStatusEventArgs  AnalyzerStatus = new AnalyzerStatusEventArgs(0, 0);

			try
			{
				// Successful itemization of the list of folders?
				if (args == null) return (false);

				// Retrieve the serialized trees and status handler
				AnalyzerRebuildRequestArgs RequestArgs = (AnalyzerRebuildRequestArgs) args;

				// Retrieve the status callback delegate
				WinFormInvokeHandler AnalyzerStatusUpdate = new WinFormInvokeHandler(m_AnalyzerRebuild.StatusInvoke);
				// WinFormInvokeHandler AnalyzerStatusUpdate = null;

				// List all the root folders containing MailItems
				// Note: Outlook 2002 will not detect "while (RootFolder != null)"
				ArrayList Folders = new ArrayList();
				Outlook.MAPIFolder RootFolder = m_OutlookApp.OutlookApp.Session.Folders.GetFirst();
				for (int ii = m_OutlookApp.OutlookApp.Session.Folders.Count; ii>0; ii--)
				{
					if (RootFolder.DefaultItemType == Outlook.OlItemType.olMailItem) Folders.Add (RootFolder);
					RootFolder = m_OutlookApp.OutlookApp.Session.Folders.GetNext();
				}

				// Generate the IsGood and IsJunk Folder TreeView data
				ArrayList aIsGood = FolderTreeView.FlattenFolders(RequestArgs.TreeSerializer[CATIDX_GOOD], Folders, ref AnalyzerStatus.Total);;
				ArrayList aIsJunk = FolderTreeView.FlattenFolders(RequestArgs.TreeSerializer[CATIDX_JUNK], Folders, ref AnalyzerStatus.Total);;

				// Reset the MailFrom and Analyzer token settings if requested
				if (RequestArgs.bResetMailFrom) svrAnalyzer.AnalyzerMailReset();
				if (RequestArgs.bResetTokens) svrAnalyzer.TokenCountReset();

				// Allow the user to cancel processing
				bool bContinue = true;

				// Calculate the IsGood Analyzer data
				foreach (Outlook.MAPIFolder Folder in aIsGood)
				{
					if (bContinue) bContinue = AnalyzerRebuild (Folder, CATIDX_GOOD, AnalyzerStatusUpdate, ref AnalyzerStatus);
				}

				// Calculate the IsJunk Analyzer data
				foreach (Outlook.MAPIFolder Folder in aIsJunk)
				{
					if (bContinue) bContinue = AnalyzerRebuild (Folder, CATIDX_JUNK, AnalyzerStatusUpdate, ref AnalyzerStatus);
				}

				// Dispose of the old folder objects
				foreach (Outlook.MAPIFolder Folder in aIsGood) TGPOutlookApp.DisposeObject(Folder);
				foreach (Outlook.MAPIFolder Folder in aIsJunk) TGPOutlookApp.DisposeObject(Folder);
				foreach (Outlook.MAPIFolder Folder in Folders) TGPOutlookApp.DisposeObject(Folder);

				// Update Controller statistics
				svrController.DisplayPercentReady (svrAnalyzer.TokenCountReady().ToString("P"));

			}
			catch(Exception ex)
			{
				LogException(new System.Diagnostics.StackFrame(), ex.Message, null);
			}

			return(AnalyzerStatus);
		}
		#endregion

		#endregion

		#region Controller Remoted Event Callbacks

		#region svrController_OnConfirmClose
		private bool svrController_OnCloseConfirm(object sender) 
		{
			// Only "Hide" Dashboard windows, close on Outlook termination
			svrController.Visible = false;
			return (false);
		}
		#endregion

		#endregion

		#region MessageBeep
		[DllImport("user32")] public static extern int MessageBeep(BeepTypes wType);
		public enum BeepTypes : ulong
		{
			MB_OK = 0x0,
			MB_ICONHAND = 0x00000010,
			MB_ICONQUESTION = 0x00000020,
			MB_ICONEXCLAMATION = 0x00000030,
			MB_ICONASTERISK = 0x00000040,
			SIMPLE_BEEP = 0xffffffff
		};		
		#endregion

	}

}