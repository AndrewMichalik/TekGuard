// Sample TekGuard MailServer database driven configuration code
// Contact Vector Information Systems, Inc (www.VInfo.com) for
// information regarding our commercial and SQL Server versions.
using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using TGMConnector;
using TGMController;
using TGMMessenger;
using TGMSupport;

namespace TGMailServer
{
	/// <summary>
	/// Summary description for MailServer
	/// </summary>
	public class MailServer : System.ComponentModel.Component
	{
		private System.ComponentModel.IContainer	components = null;
		private TGMController.Controller			svrController;
		private TGMConnector.Connector				svrConnector;
		private TGMDataStore.DataStore				svrDataStore;
		private TGMMessenger.Messenger				svrMessenger;
		private TGMSecurity.Security				svrSecurity;
		private System.Timers.Timer					tmrPerformance;		// Note: System.Windows.Forms.Timer does not work in this context

		// Shared access component globals
		private	bool				m_Enabled			= false;		// Start / Stop Messenger
		private string				m_StartupPath		= null;			// Program home path name
		private LogLocal			m_LogTools			= null;			// Application file logging class
		private Bootstrap			m_Bootstrap			= null;			// Bootstrap Configuration component

		// Start/Stop Processing		
		private ManualResetEvent	m_evtStartStopDone	= new ManualResetEvent(false);	// Reset the semaphore to wait state
		private bool				m_bStartStopArgs	= false;

		// Logging and Configuration constants
		private const string		EVT_PROGRAMNAME		= "TekGuard EMail Interceptor Server";
		private const string		DIR_CONFIG			= "Config\\";
		private const string		DIR_LOGS			= "Logs\\";
		private const string		DIR_POSTOFFICE		= "PostOffice\\";

		// File name constants
		private const string		FILE_ALERTS			= "TGMailServer";
		private const string		FILE_SECURITY		= "Security";

		// Performance timer constants
		private const int			TMR_ERRORWAIT		= 2000;		// Initialization error message delay
		private const int			TMR_PERFORMANCE		= 10000;	// Performance statistics every 10 seconds
		private const int			TMR_SOCKETPERIOD	= 6;		// Socket information every minute

		#region Constructors / Destructors
		public MailServer(System.ComponentModel.IContainer container)
		{
			/// <summary>
			/// Required for Windows.Forms Class Composition Designer support
			/// </summary>
			container.Add(this);
			InitializeComponent();
		}

		public MailServer()
		{
			/// <summary>
			/// Required for Windows.Forms Class Composition Designer support
			/// </summary>
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
			this.svrConnector = new TGMConnector.Connector();
			this.svrController = new TGMController.Controller(this.components);
			this.svrDataStore = new TGMDataStore.DataStore(this.components);
			this.svrMessenger = new TGMMessenger.Messenger(this.components);
			this.svrSecurity = new TGMSecurity.Security(this.components);
			this.tmrPerformance = new System.Timers.Timer();
			((System.ComponentModel.ISupportInitialize)(this.tmrPerformance)).BeginInit();
			// 
			// svrConnector
			// 
			this.svrConnector.OnLogAlert += new TGMConnector.LogAlertEventHandler(this.LogAlert);
			this.svrConnector.OnLogException += new TGMConnector.LogExceptionEventHandler(this.LogException);
			this.svrConnector.OnLogException_NoDisplay += new TGMConnector.LogExceptionEventHandler(this.LogException_NoDisplay);
			this.svrConnector.OnStartStop += new TGMConnector.RemoteRequestEventHandler(this.Controller_OnStartStop);
			this.svrConnector.OnConfigurationApply += new TGMConnector.RemoteRequestEventHandler(this.Controller_OnConfigurationApply);

			// this.svrConnector.OnLogRemoting += new TGMConnector.LogRemotingEventHandler(this.LogException);
			// 
			// svrMessenger
			// 
			this.svrMessenger.IPPOPrefList = new TGMMessenger.IPPOPrefEntry[0];
			this.svrMessenger.OnLogException += new TGMMessenger.LogExceptionEventHandler(this.LogException);
			this.svrMessenger.OnMessageDelete += new TGMMessenger.MessageDeleteEventHandler(this.svrMessenger_OnMessageDelete);
			this.svrMessenger.OnCheckCommandCount += new TGMMessenger.SecurityCheckCommandEventHandler(this.svrMessenger_OnCheckCommandCount);
			this.svrMessenger.OnCheckMailTo += new TGMMessenger.SecurityCheckToEventHandler(this.svrMessenger_OnCheckMailTo);
			this.svrMessenger.OnCheckHELOName += new TGMMessenger.SecurityCheckEventHandler(this.svrMessenger_OnCheckHELOName);
			this.svrMessenger.OnLogNetCommand += new TGMMessenger.LogNetCommandEventHandler(this.LogNetCommand);
			this.svrMessenger.OnMessageNew += new TGMMessenger.MessageNewEventHandler(this.svrMessenger_OnMessageNew);
			this.svrMessenger.OnCheckRemoteIP += new TGMMessenger.SecurityCheckEventHandler(this.svrMessenger_OnCheckRemoteIP);
			this.svrMessenger.OnCheckMailFrom += new TGMMessenger.SecurityCheckFromEventHandler(this.svrMessenger_OnCheckMailFrom);
			this.svrMessenger.OnCheckLogin += new TGMMessenger.SecurityCheckEventHandler(this.svrMessenger_OnCheckLogin);
			this.svrMessenger.OnMessageList += new TGMMessenger.MessageListEventHandler(this.svrMessenger_OnMessageList);
			this.svrMessenger.OnMessageRead += new TGMMessenger.MessageReadEventHandler(this.svrMessenger_OnMessageRead);
			// 
			// svrDataStore
			// 
			this.svrDataStore.OnMessageSendOne += new TGMDataStore.DSMessageSendOneEventHandler(this.svrDataStore_OnMessageSendOne);
			this.svrDataStore.OnMessageFailed += new TGMDataStore.DSMessageFailedEventHandler(this.svrDataStore_OnMessageFailed);
			this.svrDataStore.OnMessageUserForward += new TGMDataStore.DSMessageUserForwardEventHandler(this.svrDataStore_OnMessageUserForward);
			this.svrDataStore.OnMessageUserMailSlot += new TGMDataStore.DSMessageUserMailSlotEventHandler(this.svrDataStore_OnMessageUserMailSlot);
			this.svrDataStore.OnLogAlert += new TGMDataStore.LogAlertEventHandler(this.LogAlert);
			this.svrDataStore.OnLogException += new TGMDataStore.LogExceptionEventHandler(this.LogException);
			// 
			// svrSecurity
			// 
			this.svrSecurity.LogSecurityMask = ((long)(0));
			this.svrSecurity.OnLogSecurity += new TGMSecurity.LogSecurityEventHandler(this.LogSecurity);
			this.svrSecurity.OnNetTestFromAddress += new TGMSecurity.NetTestFromAddressEventHandler(this.svrSecurity_OnNetTestFromAddress);
			this.svrSecurity.OnLogAlert += new TGMSecurity.LogAlertEventHandler(this.LogAlert);
			this.svrSecurity.OnLogException += new TGMSecurity.LogExceptionEventHandler(this.LogException);
			// 
			// svrController
			// 
			this.svrController.OnMailBoxCreateDelete += new TGMController.MailBoxCreateDeleteEventHandler(this.svrController_OnMailBoxCreateDelete);
			this.svrController.OnMailBoxRename += new TGMController.MailBoxRenameEventHandler(this.svrController_OnMailBoxRename);
			this.svrController.OnCloseConfirm += new FormClosingEventHandler(this.svrController_OnCloseConfirm);
			this.svrController.OnCloseComplete += new FormClosingEventHandler(this.svrController_OnCloseComplete);
			this.svrController.OnLogAlert += new TGMController.LogAlertEventHandler(this.LogAlert);
			this.svrController.OnLogException += new TGMController.LogExceptionEventHandler(this.LogException);
			// 
			// tmrPerformance
			// 
			this.tmrPerformance.Enabled = true;
			this.tmrPerformance.Interval = TMR_PERFORMANCE;
			this.tmrPerformance.Elapsed += new System.Timers.ElapsedEventHandler(this.PerformanceMonitor);
			((System.ComponentModel.ISupportInitialize)(this.tmrPerformance)).EndInit();

		}
		#endregion

		#region Initialize / Terminate
		internal bool Initialize()
		{
			// Initialize internal settings
			try
			{
				// AJM Note 5/2005: Disable if StartupShowInterface set to false?
				// Show Splash Screen
				frmSplash.Display();

				// Get application startup directory
				m_StartupPath = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
				if (!m_StartupPath.EndsWith("\\")) m_StartupPath = m_StartupPath + "\\";

				// Make sure paths are relative to the startup directory
				Directory.SetCurrentDirectory(m_StartupPath);

				// Set up log file so any future errors can be reported
				string LogFilePath = m_StartupPath + DIR_LOGS;

				// Set log file location, throw exception on error
				if (!Directory.Exists(LogFilePath)) Directory.CreateDirectory(LogFilePath);

				// Initialize the local logging component
				m_LogTools = new LogLocal(EVT_PROGRAMNAME, LogFilePath, FILE_ALERTS, FILE_SECURITY);
			}
			catch(Exception ex)
			{
				m_LogTools.WriteWinAppEvent ("Cannot log alerts, " + ex.Message, EventLogEntryType.Error);
				MessageBox.Show ("Cannot log alerts, " + ex.Message, EVT_PROGRAMNAME);
			}						

			// Get bootstrap initialization settings
			try
			{
				// Get and set bootstrap initialization parameters. Catch exceptions here
				m_Bootstrap = new Bootstrap(this);

				string ErrorText;
				if (!m_Bootstrap.Initialize(svrConnector, m_StartupPath + DIR_CONFIG, out ErrorText))
				{
					m_LogTools.WriteWinAppEvent (ErrorText, EventLogEntryType.Error);
					throw new ApplicationException(EVT_PROGRAMNAME + ": " + ErrorText);
				}

				// Let the SCM know we are continuing the startup process (if a service)
				AppMain.ServiceStartUpdate(); 
			}
			catch(Exception ex)
			{
				m_LogTools.WriteWinAppEvent ("Error setting bootstrap initialization settings, " + ex.Message, EventLogEntryType.Error);
				throw new ApplicationException(EVT_PROGRAMNAME + ": Error setting bootstrap initialization settings, " + ex.Message, ex);
			}

			// Bootstrap initialization complete, get and set configuration parameters
			try
			{
				string ErrorText;
				if (!m_Bootstrap.Initialize(svrController, out ErrorText))
				{
					m_LogTools.WriteWinAppEvent (ErrorText, EventLogEntryType.Error);
					throw new ApplicationException(EVT_PROGRAMNAME + ": " + ErrorText);
				}

				// Let the SCM know we are continuing the startup process (if a service)
				AppMain.ServiceStartUpdate(); 
			}
			catch(Exception ex)
			{
				m_LogTools.WriteWinAppEvent ("Error setting configuration parameters, " + ex.Message, EventLogEntryType.Error);
				throw new ApplicationException(EVT_PROGRAMNAME + ": Error setting configuration parameters, " + ex.Message, ex);
			}

			// Configuration complete, get and set datastore parameters
			try
			{
				string ErrorText;
				if (!m_Bootstrap.Initialize(svrDataStore, m_StartupPath + DIR_POSTOFFICE, out ErrorText))
				{
					m_LogTools.WriteWinAppEvent (ErrorText, EventLogEntryType.Error);
					throw new ApplicationException(EVT_PROGRAMNAME + ": " + ErrorText);
				}

				// Let the SCM know we are continuing the startup process (if a service)
				AppMain.ServiceStartUpdate(); 
			}
			catch(Exception ex)
			{
				m_LogTools.WriteWinAppEvent ("Error initializing post office datastore, " + ex.Message, EventLogEntryType.Error);
				throw new ApplicationException(EVT_PROGRAMNAME + ": Error initializing post office datastore, " + ex.Message, ex);
			}

			// Indicate success
			return (true);			
		}

		internal bool Terminate()
		{
			// Close the Controller Main window
			svrController.Terminate();

			// Indicate success
			return (true);			
		}
		#endregion

		#region Start / Stop
		private void Start()
		{
			// Assume Success
			bool bSuccess = true;

			// Initialize MailServer subcomponents and begin processing
			try
			{
				string ErrorText;

				// Initialize the application configuration settings
				bSuccess &= m_Bootstrap.Initialize(svrSecurity, out ErrorText);
				bSuccess &= m_Bootstrap.Initialize(svrMessenger, out ErrorText);

				// Initialization successful?
				if (!bSuccess) 
				{
					// Wait 2 seconds to display any error messages
					Thread.Sleep(TMR_ERRORWAIT);

					// Terminate					
					throw new ApplicationException(EVT_PROGRAMNAME + " failed initialization: " + ErrorText);
				}

				// Enable the Mail Messenger Server
				svrMessenger.Enabled = true;

				// Enable the Message DataStore processing
				svrDataStore.Enabled = true;

				// Indicate that server has started
				LogAlert("Start", EVT_PROGRAMNAME + " active" + " (" + svrMessenger.LicenseType + ")", null);

				// Any Local IP Addresses set?
				// Note: This may be a normal condition for a new installation
				if (svrMessenger.IPPOPrefList.Length == 0)
				{
					LogAlert("Start", "Warning: No IP Addresses requested; is this a new installation?", null);
					return;
				}

				// New version available?
				m_Bootstrap.TGMCheckUpdates();

				// Indicate that Start was successful
				m_Enabled = true;
			}
			catch(Exception ex)
			{
				LogException(new System.Diagnostics.StackFrame(), ex.Message, null);
				throw new ApplicationException(EVT_PROGRAMNAME + " failed initialization (see log file for details).");
			}						
		}
	
		/// <summary>
		/// Stop processing mail
		/// </summary>
		private void Stop()
		{
			// Disable the Message DataStore processing
			svrDataStore.Enabled = false;

			// Disable the Mail Message Server
			svrMessenger.Enabled = false;

			// Indicate that server has stopped
			LogAlert("Stop", EVT_PROGRAMNAME + " inactive", null);

			// Indicate that Stop was successful
			m_Enabled = false;
		}
		#endregion

		#region Properties

		#region Enabled
		/// <summary>
		/// Enabled Summary
		/// </summary>
		[		
		Browsable(true),
		Category("Behavior"),
		Description("Start and stop TekGuard MailServer"),
		DefaultValue(false),
		]
		public bool Enabled 
		{
			get {return m_Enabled;}
			set
			{				
				// Start or stop this component
				if (value)
				{
					Start();
				}
				else
				{
					Stop();
				}
			}
		}
		#endregion

		#region Visible
		/// <summary>
		/// Visible Summary
		/// </summary>
		[		
		Browsable(true),
		Category("Behavior"),
		Description("Make MailServer Visible/Invisible"),
		DefaultValue(true),
		]
		public bool Visible 
		{
			set
			{
				// Open or close the Cockpit window
				svrController.Visible = value;
			}
		}
		#endregion

		#region Sponsors
		public string[] Sponsors 
		{
			set { svrController.Sponsors = value; }
		}
		#endregion

		#endregion

		#region Methods

		#region PerformanceMonitor
		private void PerformanceMonitor(object sender, System.Timers.ElapsedEventArgs e)
		{
			try 
			{
				// Note: System.Windows.Forms.Timer does not work in this context
				Statistics Statistics = svrMessenger.Performance;
				svrController.FireDisplayPerformance (Statistics.ThreadCount.ToString(), Statistics.BytesReceived.ToString(), Statistics.BytesSent.ToString(), Statistics.ErrorCount.ToString());

				// Clear the Socket Info display
				svrController.DisplaySocketInfo (null);

				// Display the active ports
				SocketInfo.SocketInfoEntry[] InfoArray = svrMessenger.Performance_POP3.SocketInfoArray;
				if (InfoArray != null) foreach (SocketInfo.SocketInfoEntry InfoEntry in InfoArray)
				{
					svrController.DisplaySocketInfo (InfoEntry.LocalIP + ":" + InfoEntry.LocalPort + " (" + InfoEntry.PostOffice + ")" + InfoEntry.Status);
				}
				InfoArray = svrMessenger.Performance_SMTP.SocketInfoArray;
				if (InfoArray != null) foreach (SocketInfo.SocketInfoEntry InfoEntry in InfoArray)
				{
					svrController.DisplaySocketInfo (InfoEntry.LocalIP + ":" + InfoEntry.LocalPort + " (" + InfoEntry.PostOffice + ")" + InfoEntry.Status);
				}
			}
			catch {}
		}
		#endregion

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
				m_LogTools.WriteWinAppEvent ("Cannot log exception, " + ex.Message, EventLogEntryType.Warning);
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
				m_LogTools.WriteWinAppEvent ("Cannot log exception, " + ex.Message, EventLogEntryType.Warning);
			}
		}
		#endregion

		#region LogSecurity
		private void LogSecurity(object Sender, string Module, Int32 Protocol, string SessionID, string LocalIP, string RemoteIP, string sPrefix, string sText) 
		{
			const string PAD_SESSIONID	= "00000";					// Session ID zero padding
			try
			{
				// Log the event
				m_LogTools.WriteSecurity(SessionID, RemoteIP, Module, sPrefix, sText);

				// Display it on the screen
				svrController.DisplayLogSecurity (Convert.ToInt32(SessionID).ToString(PAD_SESSIONID) + " " + RemoteIP + " " + sPrefix + " " + sText); 
			}
			catch(Exception ex)
			{
				m_LogTools.WriteWinAppEvent ("Cannot log security, " + ex.Message, EventLogEntryType.Warning);
			}
		}
		#endregion

		#region LogNetCommand
		private void LogNetCommand(object Sender, string Module, string Protocol, string SessionID, string LocalIP, string RemoteIP, string sPrefix, string sText) 
		{
			const string PAD_SESSIONID	= "00000";					// Session ID zero padding
			try
			{
				// Log the event
				m_LogTools.WriteNetEvent(Protocol, SessionID, RemoteIP, sPrefix, sText);

				// Display it on the screen
				svrController.DisplayLogNetCommand (Convert.ToInt32(SessionID).ToString(PAD_SESSIONID) + " " + RemoteIP + " " + sPrefix + " " + sText); 
			}
			catch(Exception ex)
			{
				m_LogTools.WriteWinAppEvent ("Cannot log net command, " + ex.Message, EventLogEntryType.Warning);
			}
		}
		#endregion

		#endregion

		#region Connector Remoted Controller Event Callbacks

		#region Controller_OnStartStop
		// Interestingly, when this call is made from TGCockpit running on the
		// same machine, IPSecurity identifies the incoming thread as coming from
		// an IP (127.0.0.0) instead of IP=null (RemotingServices.GetObjectUri).
		private object Controller_OnStartStop(object sender, object args)
		{
			// Reset the semaphore to wait state			
			m_evtStartStopDone	= new ManualResetEvent(false);
			m_bStartStopArgs = (bool) args;

			// Invoke the Start/Stop processing
			Thread thrStartStop = new Thread(new ThreadStart(Controller_OnStartStop));
			thrStartStop.ApartmentState = ApartmentState.STA;
			thrStartStop.Priority = ThreadPriority.Lowest;
			thrStartStop.IsBackground = true;
			thrStartStop.Start();

			// Wait until read is completed before continuing.
			m_evtStartStopDone.WaitOne();
			return (this.Enabled);
		}

		private void Controller_OnStartStop()
		{
			this.Enabled = m_bStartStopArgs;
			m_evtStartStopDone.Set();
		}
		#endregion

		#region Controller_OnConfigurationApply
		private object Controller_OnConfigurationApply(object sender, object args)
		{
			return (null);
		}
		#endregion

		#endregion

		#region Controller Event Callbacks

		#region xsvrController_OnStartStop
		// Note: Properties must be wrapped by a method to assign with delegates,
		// i.e., Properties can not be used with delegates directly
		private bool xsvrController_OnStartStop(object sender, bool StartStop)
		{
			this.Enabled = StartStop;
			return (this.Enabled);
		}
		#endregion

		#region svrController_OnMailBoxCreateDelete
		// Note: Properties must be wrapped by a method to assign with delegates,
		// i.e., Properties can not be used with delegates directly
		private bool svrController_OnMailBoxCreateDelete(object sender, string PostOffice, bool Create, bool Recycle)
		{
			return (svrDataStore.MailBoxCreateDelete(PostOffice, Create, Recycle));
		}
		#endregion

		#region svrController_OnMailBoxRename
		// Note: Properties must be wrapped by a method to assign with delegates,
		// i.e., Properties can not be used with delegates directly
		private bool svrController_OnMailBoxRename(object sender, string PostOffice, string User, string NewName, bool Recycle)
		{
			return (svrDataStore.MailBoxRename(PostOffice, User, NewName, Recycle));
		}
		#endregion

		#region svrController_OnCloseConfirm
		private bool svrController_OnCloseConfirm(object sender) 
		{
			if (this.Enabled && AppMain.IsApplication) 
			{
				DialogResult Result = MessageBox.Show("Do you want to stop all email processing and exit the application?", "Exit Alert", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2);
				if (Result == DialogResult.No)
				{
					return (false);
				}
				else
				{
					// Yes, close application
					return (true);
				}
			}
			else
			{
				// Let Cockpit window close if service is closing
				// return (false);

				// Only "Hide" Cockpit windows, close on termination
				svrController.Visible = false;
				return (false);

				// There is no way to "Hide" the main window - ShowDialog terminates
				// So just close it and handle re-opening in Controller.ShowThreaded loop
				// return (true);
			}
		}
		#endregion

		#region svrController_OnCloseComplete
		private bool svrController_OnCloseComplete(object sender) 
		{
			if (AppMain.IsApplication)
			{
				// Note: This will only be called if started as an application
				// since since service version only hides the main window.
				AppMain.SignalMainDone.Set();
				return (true);					
			}
			else if (null != AppMain.GetActiveService())
			{
				// Running as a service, keep display thread active
				return (false);
			}
			else
			{
				// Inactive, terminate display thread
				return (true);
			}
		}
		#endregion

		#endregion

		#region Messenger Event Callbacks

		#region svrMessenger_OnCheckRemoteIP
		private bool svrMessenger_OnCheckRemoteIP(object Sender, TGMMessenger.SecurityRecvInfo eArg)
		{
			return (svrSecurity.FireSecurityCheckRemoteIP(eArg));
		}
		#endregion

		#region svrMessenger_OnCheckHELOName
		private bool svrMessenger_OnCheckHELOName(object Sender, TGMMessenger.SecurityRecvInfo eArg)
		{
			return (svrSecurity.FireSecurityCheckHELOName(eArg));
		}
		#endregion

		#region svrMessenger_OnCheckLogin
		private bool svrMessenger_OnCheckLogin(object Sender, TGMMessenger.SecurityRecvInfo eArg)
		{
			return (svrSecurity.FireSecurityCheckLogin (eArg));
		}
		#endregion

		#region svrMessenger_OnCheckMailFrom
		private bool svrMessenger_OnCheckMailFrom(object Sender, TGMMessenger.SecurityRecvInfo eArg, string From)
		{
			return (svrSecurity.SecurityCheckMailFrom (eArg, From));
		}
		#endregion

		#region svrMessenger_OnCheckMailTo
		private bool svrMessenger_OnCheckMailTo(object Sender, TGMMessenger.SecurityRecvInfo eArg, string From, string To)
		{
			return (svrSecurity.FireSecurityCheckMailTo(eArg, From, To));
		}
		#endregion

		#region svrMessenger_OnCheckCommandCount
		private bool svrMessenger_OnCheckCommandCount(object Sender, TGMMessenger.SecurityRecvInfo eArg, string From, string To)
		{
			return (svrSecurity.FireSecurityCheckCommandCount(eArg, From, To));
		}
		#endregion

		#region svrMessenger_OnMessageList
		private MessageStoreInfo[] svrMessenger_OnMessageList(object Sender, string PostOffice, string UserName)
		{
			return (svrDataStore.MessageList(PostOffice, UserName));
		}
		#endregion

		#region svrMessenger_OnMessageNew
		private bool svrMessenger_OnMessageNew(object Sender, SecurityRecvInfo RecvInfo, string PostOffice, string[] To, byte[] Message)
		{
			return (svrDataStore.MessageNew(RecvInfo, PostOffice, To, Message));
		}
		#endregion

		#region svrMessenger_OnMessageRead
		private System.Data.DataSet svrMessenger_OnMessageRead(object Sender, string PostOffice, MessageStoreInfo StoreInfo, out MessageSendInfo SendInfo, out byte[] Message)
		{
			return (svrDataStore.MessageRead(PostOffice, StoreInfo, out SendInfo, out Message));
		}
		#endregion

		#region svrMessenger_OnMessageDelete
		private bool svrMessenger_OnMessageDelete(object Sender, string PostOffice, MessageStoreInfo StoreInfo)
		{
			return (svrDataStore.MessageDelete(PostOffice, StoreInfo));
		}
		#endregion

		#endregion

		#region DataStore Event Callbacks

		#region svrDataStore_OnMessageSendOne
		private MessageSendInfo svrDataStore_OnMessageSendOne(object Sender, string PostOffice, MessageSendInfo SendInfo, byte[] Message)
		{
			return (svrMessenger.MessageSendOne (PostOffice, SendInfo, Message));
		}
		#endregion

		#region svrDataStore_OnMessageFailed
		private bool svrDataStore_OnMessageFailed(object Sender, string PostOffice, MessageStoreInfo StoreInfo)
		{
			return (true);
		}
		#endregion

		#region svrDataStore_OnMessageUserForward
		private string[] svrDataStore_OnMessageUserForward(object Sender, string PostOffice, string To)
		{
			return (svrSecurity.FireSecurityUserForward(PostOffice, To));
		}
		#endregion

		#region svrDataStore_OnMessageUserMailSlot
		private string svrDataStore_OnMessageUserMailSlot(object Sender, string PostOffice, string To)
		{
			return (svrSecurity.FireSecurityUserMailSlot(PostOffice, To));
		}
		#endregion

		#endregion

		#region Security Event Callbacks

		#region svrSecurity_OnNetTestFromAddress
		private bool svrSecurity_OnNetTestFromAddress(object Sender, TGMMessenger.SecurityRecvInfo eArg, string From)
		{
			return (svrMessenger.NetTestFromAddress (eArg, From));
		}
		#endregion

		#endregion

	}

	#region Event Delegates and Argument Structures

	#region Logging Delegates
	/// <summary>Exception Process Logging EventHandler Summary</summary>
	public delegate void LogExceptionEventHandler (object Sender, string Module, string Message);
	/// <summary>Network Porotocol Command Process Logging EventHandler Summary</summary>
	public delegate void LogNetCommandEventHandler (object Sender, string Module, string Protocol, string SessionID, string LocalIP, string RemoteIP, string Prefix, string Text);
	#endregion

	#endregion

}
