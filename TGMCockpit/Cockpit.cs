// Sample TekGuard Cockpit database driven configuration code
// Contact VISI (www.VectorInfo.com, 1-800-234-VISI) for information
// regarding our commercial and SQL Server versions.
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

namespace TGMCockpit
{
	/// <summary>
	/// Summary description for Cockpit
	/// </summary>
	public class Cockpit : System.ComponentModel.Component
	{
		private System.ComponentModel.IContainer	components = null;
		private TGMController.Controller			svrController;
		private TGMConnector.Connector				svrConnector;
		private System.Timers.Timer					tmrPerformance;		// Note: System.Windows.Forms.Timer does not work in this context

		// Shared access component globals
		private LogLocal			m_LogTools			= null;			// Application file logging class
		private	bool				m_Enabled			= false;		// Start / Stop Messenger
		private string				m_StartupPath		= "";			// Program home path name
		private string				m_ConfigFilePath	= "";			// Configuration File directory
		private Bootstrap			m_Bootstrap			= null;			// Bootstrap Configuration component

		// Performance timer constants
		private const int			TMR_ERRORWAIT		= 2000;			// Initialization error message delay
		private const int			TMR_PERFORMANCE		= 10000;		// Performance statistics every 10 seconds
		private const int			TMR_SOCKETPERIOD	= 6;			// Socket information every minute

		// Directory constants
		private const string		EVT_PROGRAMNAME		= "TekGuard EMail Server Cockpit";
		public const string			DIR_LOGS			= "Logs\\";
		public const string			DIR_CONFIG			= "Config\\";

		// File name constants
		private const string		FILE_ALERTS			= "TGMCockpit";
		private const string		FILE_SECURITY		= "Security";

		#region Constructors / Destructors
		public Cockpit(System.ComponentModel.IContainer container)
		{
			/// <summary>
			/// Required for Windows.Forms Class Composition Designer support
			/// </summary>
			container.Add(this);
			InitializeComponent();
		}

		public Cockpit()
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
			this.tmrPerformance = new System.Timers.Timer();
			((System.ComponentModel.ISupportInitialize)(this.tmrPerformance)).BeginInit();
			// 
			// svrConnector
			// 
			this.svrConnector.OnLogAlert += new TGMConnector.LogAlertEventHandler(this.LogAlert);
			this.svrConnector.OnLogException += new TGMConnector.LogExceptionEventHandler(this.LogException);
			this.svrConnector.OnLogException_NoDisplay += new TGMConnector.LogExceptionEventHandler(this.LogException_NoDisplay);
			// 
			// svrController
			// 
			this.svrController.Visible = false;
			this.svrController.OnCloseComplete += new TGMController.FormClosingEventHandler(this.svrController_OnCloseComplete);
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
				throw new ApplicationException(EVT_PROGRAMNAME + ": Cannot log alerts", ex);
			}						

			// Get bootstrap initialization settings
			try
			{
				// Set Configuration directory
				m_ConfigFilePath = m_StartupPath + DIR_CONFIG;

				// Get and set bootstrap initialization parameters. Catch exceptions here
				m_Bootstrap = new Bootstrap(this);
			}
			catch(Exception ex)
			{
				m_LogTools.WriteWinAppEvent ("Cannot read initialization file (see log file for details), " + ex.Message, EventLogEntryType.Error);
				throw new ApplicationException(EVT_PROGRAMNAME + ": Cannot read initialization file (see log file for details)", ex);
			}

			// Bootstrap initialization complete, initialize Connector and Controller UI component.
			try
			{
				string ErrorText;
				if (!m_Bootstrap.Initialize(svrConnector, m_ConfigFilePath, out ErrorText))
				{
					m_LogTools.WriteWinAppEvent (ErrorText, EventLogEntryType.Error);
					MessageBox.Show (ErrorText, EVT_PROGRAMNAME);
					return (false);			
				}
				if (!m_Bootstrap.Initialize(svrController, out ErrorText))
				{
					m_LogTools.WriteWinAppEvent (ErrorText, EventLogEntryType.Error);
					MessageBox.Show (ErrorText, EVT_PROGRAMNAME);
					return (false);			
				}
			}
			catch(Exception ex)
			{
				m_LogTools.WriteWinAppEvent ("Cannot read initialization file (see log file for details), " + ex.Message, EventLogEntryType.Error);
				MessageBox.Show ("Error getting and setting configuration parameters (see log file for details), " + ex.Message, EVT_PROGRAMNAME);
				return (false);
			}

			// Indicate success
			return (true);			
		}

		internal bool Terminate()
		{
			// Indicate that Cockpit has stopped
			LogAlert("Stop", EVT_PROGRAMNAME + " unloading", null);

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

			// Initialize Dashboard subcomponents and begin processing
			try
			{
				// Initialization successful?
				if (!bSuccess) 
				{
					// Wait 2 seconds to display any error messages
					Thread.Sleep(TMR_ERRORWAIT);

					// Terminate					
					throw new ApplicationException(EVT_PROGRAMNAME + " failed initialization (see log file for details)");
				}

				// Indicate that Cockpit has started
				LogAlert("Start", EVT_PROGRAMNAME + " active", null);

				// New version available?
				// Need to get sponsor updates before going visible
				m_Bootstrap.TGMCheckUpdates_Thread();

				// If successful, make controller visible
				svrController.Visible = true;

				// Indicate that Start was successful
				m_Enabled = true;
			}
			catch(Exception ex)
			{
				LogAlert("Start", ex.Message, null);
				throw new ApplicationException(EVT_PROGRAMNAME + " failed initialization (see log file for details)");
			}						
		}
	
		/// <summary>
		/// Stop processing mail
		/// </summary>
		private void Stop()
		{
			// Indicate that interface has stopped
			LogAlert("Stop", EVT_PROGRAMNAME + " inactive", null);

			// Terminate the main Controller monitor window
			svrController.Terminate();

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
		Description("Start and stop TekGuard Cockpit"),
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

		#region Sponsors
		public string[] Sponsors 
		{
			set { svrController.Sponsors = value; }
		}
		#endregion

		#endregion

		#region Methods

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
				// Log the event to a file
				// Note: Do Not Display the event
				m_LogTools.WriteAlert(Sender, Message, Params);
			}
			catch(Exception ex)
			{
				m_LogTools.WriteWinAppEvent ("Cannot log alert, " + ex.Message, EventLogEntryType.Warning);
			}
		}
		#endregion

		#region LogResetStatus
		internal void LogResetStatus(string Message) 
		{
			// MessageBox.Show(Message);
			if (svrController != null) 
			{
			}
		}
		#endregion

		#endregion

		#region PerformanceMonitor
		private void PerformanceMonitor(object sender, System.Timers.ElapsedEventArgs e)
		{
			try 
			{
				// New version available?
				m_Bootstrap.TGMCheckUpdates();
			}
			catch {}
		}
		#endregion

		#region Connector Event Callbacks

		#endregion

		#region Controller Event Callbacks

		#region svrController_OnStartStop
		// Note: Properties must be wrapped by a method to assign with delegates,
		// i.e., Properties can not be used with delegates directly
		private bool svrController_OnStartStop(object sender, bool StartStop)
		{
			this.Enabled = StartStop;
			return (this.Enabled);
		}
		#endregion

		#region svrController_OnCloseComplete
		private bool svrController_OnCloseComplete(object sender) 
		{
			AppMain.SignalMainDone.Set();
			return (true);						
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
