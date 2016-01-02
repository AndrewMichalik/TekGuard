// Sample TekGuard Dashboard application code
// Contact VISI (www.VectorInfo.com, 1-800-234-VISI) for information
// regarding our commercial and SQL Server versions.
using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;

namespace TGPDashboard
{
	public class AppMain : System.ServiceProcess.ServiceBase
	{
		// Component members
		private			System.ComponentModel.IContainer	components		= null;
		private static	AppMain								m_AppMain		= null;	// Main application object
		private			string[]							m_StartupArgs	= null;	// Save any startup parameters
		private			Dashboard							m_Dashboard		= null;	// Dashboard main component
		private 		ManualResetEvent					m_evtMainDone	= new ManualResetEvent(false);	// Reset the semaphore to wait state
		private const	string								STARTUP_ASAPP	= "Application";

		#region Constructors / Destructors

		public AppMain()
		{
			// This call is required by the Windows.Forms Component Designer.
			InitializeComponent();

			// Create Dashboard component
			m_Dashboard = new Dashboard();
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
			this.ServiceName = "TGPDashboard";

		}
		#endregion

		#region Main Entry Point
		//Application main entry point
		[MTAThread]
		public static void Main(string[] args) 
		{
			// Run as an application; simulate service processing sequence
			m_AppMain = new AppMain();
	
			// Copy already running?
			if (m_AppMain.IsRunning()) return;
				
			// Begin Mail monitoring
			m_AppMain.OnStart(args);

			// Successful start?
			if (!m_AppMain.m_Dashboard.Enabled) return;

			// Wait until user is done before exiting
			m_AppMain.m_evtMainDone.WaitOne();

			// End Mail monitoring
			m_AppMain.OnStop();

			// Exit application (after all threads complete)
			Application.Exit();
		}
		#endregion

		#region OnStart / OnStop
		protected override void OnStart(string[] args)
		{
			// Create the TekGuard Mail Server
			m_Dashboard = new Dashboard();

			// Save the startup parameters for future reference
			m_StartupArgs = args;

			// Note: .Net Bug? An instance of m_Dashboard initialized when run 
			// as an application is lost after service execution begins. Therefore,
			// the monitor window must be created after this point.
			// Note: .Net Bug? The timer component does not fire in the monitor
			// or in the Dashboard component.

			// Initialize internal settings and begin Mail processing
			m_Dashboard.Enabled = m_Dashboard.Initialize();
		}
		/// <summary>Stop this service</summary>
		protected override void OnStop()
		{
			// End Mail processing
			m_Dashboard.Enabled = false;

			// Terminate internal processing
			m_Dashboard.Terminate();
		}
		#endregion

		#region IsRunning
		private bool IsRunning()
		{
			// GetProcessesByName() returns array of processes with the specified name
			Process[] processes = Process.GetProcessesByName(this.ServiceName);
			if (processes.Length > 1) 
				return true;	// Another copy is running
			else
				return false;	// No instance exists
		}
		#endregion

		#region IsApplication
		internal static bool IsApplication 
		{
			get {return m_AppMain != null;}
		}
		#endregion

		#region SignalMainDone
		internal static ManualResetEvent SignalMainDone 
		{
			get {return m_AppMain.m_evtMainDone;}
		}
		#endregion

	}
}
