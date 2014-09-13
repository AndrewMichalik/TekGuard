// Sample TekGuard MailServer database driven configuration code
// Contact Vector Information Systems, Inc (www.VInfo.com) for
// information regarding our commercial and SQL Server versions.
using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;
using TGMSupport;

namespace TGMailServer
{
	public class AppMain : System.ServiceProcess.ServiceBase
	{
		// Component members
		private			System.ComponentModel.IContainer	components		= null;
//		private			AppMain								m_AppMain		= null;	// Main application object
		private			string[]							m_StartupArgs	= null;	// Save any startup parameters
		private			MailServer							m_MailServer	= null;	// MailServer main component
		private static	ManualResetEvent					m_evtMainDone	= new ManualResetEvent(false);	// Reset the semaphore to wait state
		private	static	ServiceStatus						m_ServiceStatus	= new ServiceStatus(AppInstaller.TGMSERVICE_NAME);
		private	static	bool								m_IsService		= false;
		private			bool								m_OnStartOK		= false;
		private const	string								STARTUP_ASAPP	= "Application";
		private const	int									COMMAND_COCKPIT = 128 + 8;
		private const	Int32								SCM_STARTMS		= 60000;
		private const	Int32								SCM_STOPMS		= 30000;

		#region Constructors / Destructors

		public AppMain()
		{
			// This call is required by the Windows.Forms Component Designer.
			InitializeComponent();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#endregion

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			// 
			// AppMain
			// 
			this.CanShutdown = true;
			this.ServiceName = AppInstaller.TGMSERVICE_NAME;

		}
		#endregion

		#region Main Entry Point
		//Application main entry point
		[MTAThread]
		public static void Main(string[] args) 
		{
			// Create initial application object
			AppMain AppMain = new AppMain();

			// Run as an application or a service?
			m_IsService = (args.Length == 0) || (args[0].ToLower() != STARTUP_ASAPP.ToLower());

			// Note: Allow either one service or multiple application instances
			if (m_IsService)
			{
				// Run as a service; return (exit) to SCM on successful execution
				if (AppMain.RunAsService(AppMain)) return;

				// Failed Service startup - already running?
				if (AppMain.IsRunning()) 
				{
					// Already running as a service?
					ServiceController Controller = GetActiveService();
					if (Controller != null)
					{
						// Yes, simply activate the Controller window
						Controller.ExecuteCommand(COMMAND_COCKPIT);

						// Don't run a second instance if a service
						return;
					}
				}
			
				// Service execution failed; try running as an application?
				string Msg = "Would you like to run this as an application?\n\nNote: Specify 'Application' on the command\nline when not running as a service.";
				if (DialogResult.No == MessageBox.Show (Msg, AppInstaller.TGMDISPLAY_NAME, MessageBoxButtons.YesNo)) return;

				// Try running as an application
				m_IsService = false;
			}

			// Run as an application; emulate service startup sequence
			AppMain.OnStart(new string[] {STARTUP_ASAPP});

			// Wait until read is completed before continuing.
			AppMain.m_evtMainDone.WaitOne();

			// End Mail processing
			AppMain.OnStop();

			// Exit application (after all threads complete)
			Application.Exit();
		}
		#endregion

		#region RunAsService
		private bool RunAsService(AppMain AppMain)
		{
			// More than one user Service may run within the same process.
			System.ServiceProcess.ServiceBase[] ServicesToRun;
	
			// Force ability to interact with the desktop
			// Note: .Net 1.1 ignores this parameter
			// Controller.ServiceType = System.ServiceProcess.ServiceType.InteractiveProcess;
					
			// Service: Register Service Status Handler with SCM
			string ErrorText;
			if ((ErrorText = m_ServiceStatus.Register(new ServiceCustomEventHandler(AppMain.OnCustomCommand), new ServiceStopEventHandler(AppMain.OnStop))) != null)
			{
				MessageBox.Show (ErrorText, AppInstaller.TGMSERVICE_NAME);
			}
			else 
			{
				// Let the SCM know we are starting this process
				if (m_ServiceStatus.IsRegistered) m_ServiceStatus.SendStatus(ServiceControllerStatus.StartPending, 0, SCM_STARTMS); 
			}

			// Install application as a Service Control Manager (SCM) service
			// Calls OnStart() and OnStop()
			ServicesToRun = new System.ServiceProcess.ServiceBase[] { AppMain };
			System.ServiceProcess.ServiceBase.Run(ServicesToRun);

			// Successfully run as a service?
			return (m_OnStartOK);
		}

		#endregion

		#region OnStart / OnStop
		protected override void OnStart(string[] args)
		{
			// Save the startup parameters for future reference
			m_StartupArgs = args;

			// Create the TekGuard Mail Server
			m_MailServer = new MailServer();

			// Initialize internal settings
			m_MailServer.Initialize();

			// Begin Mail processing
			m_MailServer.Enabled = true;

			// Let the SCM know we have started
			if (m_ServiceStatus.IsRegistered) m_ServiceStatus.SendStatus(ServiceControllerStatus.Running); 

			// Indicate that OnStart was called successfully
			m_OnStartOK = true;
		}
		/// <summary>Stop this service</summary>
		protected override void OnStop()
		{
			// Let the SCM know we are stopping this process
			if (m_ServiceStatus.IsRegistered) m_ServiceStatus.SendStatus(ServiceControllerStatus.StopPending, 0, SCM_STOPMS); 

			// End Mail processing
			m_MailServer.Enabled = false;

			// Terminate internal processing
			m_MailServer.Terminate();
		}
		#endregion

		#region OnCustomCommand
		protected override void OnCustomCommand (int command)
		{
			// Open or close the Cockpit window
			if (command == COMMAND_COCKPIT) m_MailServer.Visible = true;
		}
		#endregion

		#region IsRunning
		private bool IsRunning()
		{
			// GetProcessesByName() returns array of processes with the specified name
			Process[] processes = Process.GetProcessesByName(AppInstaller.TGMSERVICE_NAME);

			// Running?			
			if (processes.Length > 1) 
				return true;	// Another copy is running
			else
				return false;	// No instance exists
		}
		#endregion

		#region IsApplication
		internal static bool IsApplication 
		{
			get {return (!m_IsService);}
		}
		#endregion

		#region GetActiveService
		internal static ServiceController GetActiveService()
		{
			// Installed as a service?
			ServiceController Controller = new ServiceController(AppInstaller.TGMSERVICE_NAME);

			try 
			{
				// Catch Security Error: "Service TGMailServer was not found on computer '.'"
				ServiceControllerStatus Status = Controller.Status;
				bool bActive = (Status != ServiceControllerStatus.StopPending) && (Status != ServiceControllerStatus.Stopped);
				return (bActive ? Controller : null);
			} 
			catch
			{
				// Not an installed service
				return (null);
			}
		}
		#endregion

		#region ServiceStartUpdate
		internal static void ServiceStartUpdate()
		{
			// Let the SCM know what is happening with the process
			if (m_ServiceStatus.IsRegistered) m_ServiceStatus.SendStatus(ServiceControllerStatus.StartPending); 
		}
		#endregion

		#region SignalMainDone
		internal static ManualResetEvent SignalMainDone 
		{
			get {return m_evtMainDone;}
		}
		#endregion

	}

}
