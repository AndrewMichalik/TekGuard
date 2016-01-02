using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.Text;
using System.Threading;
using TGPConnector;

namespace TGPController
{
	/// <summary>
	/// Summary for Controller
	/// </summary>
	public class Controller : System.ComponentModel.Component
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.PictureBox picSplash;

		// Component members
		private frmMain				m_frmMain			= null;				// Main application form
		private	Thread				m_thrMain			= null;				// Main thread for window form
		private ManualResetEvent	m_evtShow			= null;				// Semaphore for the ShowDialog wait state
		private HostQuery			m_HostQuery			= null;				// Host Information Query class
		private DBQuery				m_DBIniQuery		= null;				// Initialization Database query object
		private DBQuery				m_DBCfgQuery		= null;				// Configuration Database query object
		private DBQuery				m_DBAnaQuery		= null;				// Analyzer Database query object
		private DBData			m_DBData		= null;				// Database data get/set helper class

		#region Constructors
		public Controller(System.ComponentModel.IContainer container)
		{
			/// <summary>
			/// Required for Windows.Forms Class Composition Designer support
			/// </summary>
			container.Add(this);
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}
		public Controller()
		{
			/// <summary>
			/// Required for Windows.Forms Class Composition Designer support
			/// </summary>
			InitializeComponent();

			// Inhibit warning for unused field
			components = components;
			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}
		#endregion

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Controller));
			this.picSplash = new System.Windows.Forms.PictureBox();
			// 
			// picSplash
			// 
			this.picSplash.Image = ((System.Drawing.Bitmap)(resources.GetObject("picSplash.Image")));
			this.picSplash.Location = new System.Drawing.Point(17, 17);
			this.picSplash.Name = "picSplash";
			this.picSplash.TabIndex = 0;
			this.picSplash.TabStop = false;

		}
		#endregion

		#region Initialize / Terminate
		public bool Initialize (string Title, Icon Icon, LogQueue LogQueue, HostQuery HostQuery, DBQuery DBIniQuery, DBQuery DBCfgQuery, DBQuery DBAnaQuery, out string ErrorText)
		{
			// Pass the error text back to caller since the Controller Form is not yet active
			ErrorText = null;

			try
			{
				// Save the host query object
				m_HostQuery = HostQuery;

				// Save the database query objects
				m_DBIniQuery = DBIniQuery;
				m_DBCfgQuery = DBCfgQuery;
				m_DBAnaQuery = DBAnaQuery;

				// Initialize the Main form (not visible)
				m_frmMain = new frmMain(Title, Icon, LogQueue);

				// Database data get/set helper class
				m_DBData = new DBData(this);	

				// Initialize the Form and the thread
				ShowThreaded();

				// Return success
				return (true);
			}
			catch(Exception ex)
			{
				ErrorText = "Error initializing user interface forms, " + ex.Message;
				return (false);
			}
		}
		public bool Terminate()
		{
			// Close the Main window
			if (m_frmMain != null)
			{
				m_frmMain.Close();
				m_frmMain = null;
			}

			// Release the "expensive" database query objects
			m_DBIniQuery = null;
			m_DBCfgQuery = null;
			m_DBAnaQuery = null;

			// Indicate success
			return (true);			
		}

		#endregion

		#region ShowThreaded
		private void ShowThreaded ()
		{
			// Reset the semaphore to wait state
			m_evtShow = new ManualResetEvent(false);

			// Create a new thread for the window
			m_thrMain = new Thread(new ThreadStart(ThreadMain));
			m_thrMain.Priority = ThreadPriority.AboveNormal;
			m_thrMain.ApartmentState = ApartmentState.STA;
			// AJM 5/2005 - I think "IsBackground = true" is messing up the user interface
			// when the frmSettingsUserAnalyzer calls DBAnaQuery.ConfigurationApply. The
			// UI seems to slow down on W2K, and the "explorer proccess" goes to 100%
			// on the Task Manager.
			// m_thrMain.IsBackground = true;
			m_thrMain.Start();
		}

		private void ThreadMain()
		{
			// Don't shutdown the application because of a Windows Form error
			try
			{
				// The Threaded_Reader for the LogQueue depends on this thread
				// to be active to broadcast the events to the TGDashboard.
				// The Dashboard does not get any events until m_frmMain.Initialize
				// has completed binding the controls.
				m_frmMain.Initialize(this);

				// Wait until user is ready before showing the form
				m_evtShow.WaitOne();

				// Show the form as a modal window
				m_frmMain.ShowDialog();

				// Indicate that the form has closed
				FireCloseComplete(this);
			}
			catch(Exception ex)
			{
				// Abnormal exit from window - try to recover
				FireLogException(ex, null, null);
			}
		}
		#endregion

		#region Properties (Public)

		#region Visible
		/// <summary>
		/// Visible Summary
		/// </summary>
		[		
		Browsable(true),
		Category("Behavior"),
		Description("Show and hide the Controller interface"),
		DefaultValue(true),
		]
		public bool Visible 
		{
			get
			{
				// Ignore request if not initialized or already terminated
				return ((m_frmMain != null) ? m_frmMain.Visible : false);
			}
			set
			{
				// Ignore request if not initialized or already terminated
				if (m_frmMain == null) return;

				// Show or hide this component window
				if (value)
				{
					// Show the Main form

					// Is the form thread blocked?
					if (!m_evtShow.WaitOne(0, false))
					{
						// Release the waiting thread, ShowDialog() the first time
						// through. Note that if m_frmMain.WindowState or m_frmMain.Visible
						// is accidentally set by this thread during ShowDialog, the 
						// system behavies erratically.
						m_evtShow.Set();
					}
					else if (!m_frmMain.Visible) 
					{
						m_frmMain.WindowState = System.Windows.Forms.FormWindowState.Normal;
						m_frmMain.Visible = true;
					}
					else
					{
						m_frmMain.WindowState = System.Windows.Forms.FormWindowState.Normal;
						m_frmMain.Activate();
					}
				}
				else
				{
					// Hide the Main form
					if (m_frmMain.Visible) m_frmMain.Visible = false;
				}
			}
		}
		#endregion

		#region Sponsors
		public string[] Sponsors 
		{
			set { m_frmMain.Sponsors = value; }
		}
		#endregion

		#endregion

		#region Properties (Internal)

		#region DBIniQuery
		internal DBQuery DBIniQuery 
		{
			get {return m_DBIniQuery;}
		}
		#endregion

		#region DBCfgQuery
		internal DBQuery DBCfgQuery 
		{
			get {return m_DBCfgQuery;}
		}
		#endregion

		#region DBAnaQuery
		internal DBQuery DBAnaQuery 
		{
			get {return m_DBAnaQuery;}
		}
		#endregion

		#region HostQuery
		internal HostQuery HostQuery 
		{
			get {return m_HostQuery;}
		}
		#endregion

		#region DBData
		internal DBData DBData 
		{
			get {return m_DBData;}
		}
		#endregion

		#region Title
		internal string Title 
		{
			get {return m_frmMain.Text;}
		}
		#endregion

		#region Icon
		internal Icon Icon 
		{
			get {return m_frmMain.Icon;}
		}
		#endregion

		#endregion

		#region Public Event Handlers
		
		#region OnLogAlert
		/// <summary>OnLogException Summary</summary>
		[		
		Browsable(true),
		Category("Events"),
		Description("Application Alerts Logging"),
		DefaultValue(null),
		]		
		public event LogAlertEventHandler	OnLogAlert = null;
		#endregion

		#region OnLogException
		/// <summary>OnLogException Summary</summary>
		[		
		Browsable(true),
		Category("Events"),
		Description("Exception Process Logging"),
		DefaultValue(null),
		]		
		public event LogExceptionEventHandler	OnLogException = null;
		#endregion

		#region OnCloseConfirm
		/// <summary>OnCloseConfirm Summary</summary>
		[		
		Browsable(true),
		Category("Events"),
		Description("Form OnClose Confirmation Processing"),
		DefaultValue(null),
		]		
		public event FormClosingEventHandler OnCloseConfirm = null;
		#endregion

		#region OnCloseComplete
		/// <summary>OnCloseComplete Summary</summary>
		[		
		Browsable(true),
		Category("Events"),
		Description("Form Close Complete Notification"),
		DefaultValue(null),
		]		
		public event FormClosingEventHandler OnCloseComplete = null;
		#endregion

		#endregion

		#region Public Display and Logging Functions

		#region DisplayLogAlert
		public void DisplayLogAlert(string Text)
		{
			try
			{
				if (m_frmMain != null) m_frmMain.LogEvent(Text);
			}
			catch
			{
				// Do not try to display error
			}
		}
		#endregion
		
		#region DisplayLogException
		public void DisplayLogException(string Text)
		{
			try
			{
				if (m_frmMain != null) m_frmMain.LogEvent(Text);
			}
			catch
			{
				// Do not try to display error
			}
		}
		#endregion
		
		#region DisplayLogAnalyzer
		public void DisplayLogAnalyzer(string Text)
		{
			try
			{
				if (m_frmMain != null) m_frmMain.LogAnalyzer(Text);
			}
			catch
			{
				// Do not try to display error
			}
		}
		#endregion
		
		#region DisplayPercentReady
		public void DisplayPercentReady(string Text)
		{
			try
			{
				if (m_frmMain != null) m_frmMain.PercentReady(Text);
			}
			catch
			{
				// Do not try to display error
			}
		}
		#endregion
		
		#region DisplayRebuildWizard
		public frmAnalyzerRebuild DisplayRebuildWizard(string Title, Icon Icon)
		{
			// Set callback
			frmAnalyzerRebuild frmAnalyzerRebuild = new frmAnalyzerRebuild(Title, Icon);

			try
			{
				// Load Rebuild Wizard
				return (frmAnalyzerRebuild.Initialize(this) ? frmAnalyzerRebuild : null);
			}
			catch(Exception ex)
			{
				FireLogException(ex, null, null);
				return (null);
			}
		}
		#endregion
		
		#endregion

		#region Internal Event Firing Functions

		#region FireLogAlert
		internal void FireLogAlert(string Message, string ParamName, string ParamValue) 
		{
			try 
			{
				// AJM - separate try block for missing PDB?
				// Get caller stack information (also request FileName and Line#)
				StackFrame Frame = new StackFrame(1, true);
				
				// Get calling method (where exception occurred)
				System.Reflection.MethodBase mCaller = Frame.GetMethod();
				string Sender = mCaller.DeclaringType.FullName.ToString() + "." + mCaller.Name;
			
				// Fire event
				if (this.OnLogAlert != null)
				{
					this.OnLogAlert(Sender, Message, new string[][] {new string[] {ParamName, ParamValue}});
				}
			}
			catch {}
		}
		#endregion

		#region FireLogException
		internal void FireLogException(Exception ex, string ParamName, string ParamValue) 
		{
			try 
			{
				// AJM - separate try block for missing PDB?
				// Get caller stack information (also request FileName and Line#)
				StackFrame Frame = new StackFrame(1, true);
			
				// Fire event
				if (this.OnLogException != null)
				{
					this.OnLogException(Frame, ex.Message, new string[][] {new string[] {ParamName, ParamValue}});
				}
			}
			catch {}
		}
		#endregion

		#region FireCloseConfirm
		internal bool FireCloseConfirm(object sender) 
		{
			// Fire event
			if (this.OnCloseConfirm != null)
			{
				return (this.OnCloseConfirm(sender));
			}
			// Default exit
			return (true);
		}
		#endregion

		#region FireCloseComplete
		internal bool FireCloseComplete(object sender) 
		{
			// Fire event
			if (this.OnCloseComplete != null)
			{
				return (this.OnCloseComplete(sender));
			}
			// Default exit
			return (true);
		}
		#endregion

		#endregion

	}

	#region Event Delegates and Argument Structures

	#region Logging Delegates
	/// <summary>Log Alert EventHandler Summary</summary>
	public delegate void LogAlertEventHandler (string Sender, string Message, string[][] Params);
	/// <summary>Log Exception EventHandler Summary</summary>
	public delegate void LogExceptionEventHandler (StackFrame Sender, string Message, string[][] Params);
	#endregion

	#region Controller Request Delegates
	/// <summary>Complete Close EventHandler Summary</summary>
	public delegate bool FormClosingEventHandler (object Sender);
	#endregion

	#region Analyzer Rebuild Request and Status Event Args
	// Note: Use a struct to force pass by value for BeginInvoke delayed
	// executions. A reference will not pick up the correct count in progress.
	[Serializable]
	public class AnalyzerRebuildRequestArgs
	{
		public	TreeSerializer[]		TreeSerializer;
		public	bool					bResetTokens;
		public	bool					bResetMailFrom;

		public AnalyzerRebuildRequestArgs(TreeSerializer IsGood, TreeSerializer IsJunk)
		{
			this.TreeSerializer	= new TreeSerializer[] {IsGood, IsJunk};
			this.bResetTokens	= false;
			this.bResetMailFrom	= false;
		}
	}
	[Serializable]
	public class AnalyzerStatusEventArgs
	{
		public	Int32				NotifyPeriod;			// Progress indicator notification period
		public	Int32				Current;
		public	Int32				Total;

		private	const Int32			NOTIFY_PERIOD		= 1000;			

		public AnalyzerStatusEventArgs(Int32 Current, Int32 Total)
		{
			this.NotifyPeriod	= NOTIFY_PERIOD;
			this.Current		= Current;
			this.Total			= Total;
		}
	}
	#endregion

	#endregion

}
