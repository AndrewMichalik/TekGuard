using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using TGPConnector;

namespace TGPAnalyzer
{
	/// <summary>
	/// Summary description for Analyzer.
	/// </summary>
	public class Analyzer : System.ComponentModel.Component
	{
		private System.ComponentModel.Container components = null;

		// Analyzer Event Logging Masks
		public const	long		LOGMASK_NONE		= 0x0000;
		public const	long		LOGMASK_FAIL		= 0x0002;
		public const	long		LOGMASK_SUCCESS		= 0x0008;
		private	const	string		LOGTEXT_FAIL		= "Failed";
		private	const	string		LOGTEXT_SUCCESS		= "Succeeded";

		// Component members
		private	bool				m_Enabled			= false;		// Start / Stop Messenger
		private	long				m_LogLevel			= 0;			// Analyzer logging level, 0 == no logging
		private	Thread				m_thrCount			= null;			// Analyzer DB initialization thread
		private	MsgAnalyzer			m_MsgAnalyzer		= null;			// Analyzer message object
		private TokenList			m_TokenList			= null;			// Analyzer token list

		// Licensing members
		private const string		LIC_FREEWARE		= "Freeware";	// Default freeware license type
		private string				m_Licensee			= "";			// User licensing description string
		private string				m_LicenseNumber		= "";			// User license serial number string
		private string				m_LicenseCode		= "";
		private string				m_LicenseType		= "";			// User license type description string

		// Shared members
		internal const int			MAXTOKENCHECK		= 60;

		#region Constructors / Destructors
		public Analyzer()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitForm call


			//        'check for spam database
			//        DBPath = DLLPath & "TGPAnalyzer.mdb"
			//        EmptyDBPath = DLLPath & "TGPAnalyzer_New.mdb"
			//        Dim fso As New Scripting.FileSystemObject()
			//        If Not FileExists(DBPath) Then
			//            If FileExists(EmptyDBPath) Then
			//                fso.CopyFile(EmptyDBPath, DBPath, False)
			//            Else
			//                MsgBox("Cannot initialize database", MsgBoxStyle.Critical, "You really screwed up this time!")
			//                Exit Sub
			//            End If
			//        End If


		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if( components != null )
					components.Dispose();
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
			components = new System.ComponentModel.Container();
		}
		#endregion
	
		#region Initialize
		public bool Initialize (MsgAnalyzer MsgAnalyzer, MsgTokens MsgTokens, out string ErrorText)
		{
			// Pass the error text back to caller since the Controller Form is not yet active
			ErrorText = null;

			try
			{
				// Save the Message Analyzer object
				m_MsgAnalyzer = MsgAnalyzer;

				// Initialize the Token List
				// string regexString = @"[\s\,\.\?\!'""\(\)\;\:]+";
				string regexString = @"[\s_\!""\#\%\&\(\)\*\+\,\-\.\/\:\;\<\=\>\?\@\[\\\]\^\~\`]+";
				m_TokenList = new TokenList(this, MsgTokens, regexString);

				// Return success
				return (true);
			}
			catch(Exception ex)
			{
				ErrorText = ex.Message;
				FireLogException(ex, null, null);
				return (false);
			}
		}
		public bool Terminate()
		{
			// Release the "expensive" database query object
			m_TokenList = null;

			// Indicate success
			return (true);			
		}

		#endregion

		#region Start / Stop

		private void Start()
		{
			// Begin processing
			try
			{
				// Set IP and domain limits based on the license type
				if (m_LicenseType == LIC_FREEWARE)
				{
				}
				
				// Indicate that Start was successful
				m_Enabled = true;
			}
			catch(Exception ex)
			{
				FireLogException(ex, null, null);
			}						
		}

		private void Stop()
		{
			// Indicate that Stop was successful
			m_Enabled = false;
		}

		#endregion

		#region TokenCountInitialize
		public TokenList TokenCountInitialize()
		{
			TokenCountThreadStart();
			return (m_TokenList);
		}
		#endregion

		#region TokenCountReset
		public bool TokenCountReset ()
		{
			m_TokenList.Reset();
			return (true);
		}
		#endregion

		#region TokenCountThreadStart
		private void TokenCountThreadStart ()
		{
			// Create a new thread for the process
			m_thrCount = new Thread(new ThreadStart(TokenCountThreadMain));
			m_thrCount.Priority = ThreadPriority.BelowNormal;
			m_thrCount.IsBackground = true;
			m_thrCount.Start();
		}
		#endregion

		#region TokenCountThreadMain
		private void TokenCountThreadMain()
		{
			// Don't tie up the application while loading the database
			try
			{
				// FireEnableDisable(this, false, false);			
				m_TokenList.Initialize();
				FireEnableDisable(this, m_TokenList.Ready() >= 1.0, true);			
			}
			catch(Exception ex)
			{
				// Abnormal exit
				FireLogException(ex, null, null);
			}
			// Terminate this thread
			Thread.CurrentThread.Abort();
		}
		#endregion
		
		#region TokenCount Functions

		public double TokenCountReady ()
		{
			return (m_TokenList.Ready());
		}

		public bool TokenCountBegin(Int32 ItemCount, Int32 NotifyPeriod, Int32 CategoryCount)
		{
			m_TokenList.DoCountBegin(CategoryCount);
			return (true);
		}
		
		public bool TokenCountUpdate (Int32 CategoryID, int CategoryIndex, string Headers, string SenderName, string SenderAddress, string Subject, string Body, DateTime LastReceived)
		{
			m_TokenList.DoCount (CategoryID, CategoryIndex, SenderName + " " + SenderAddress + " " + Subject + " " + Body, LastReceived);
			return (true);
		}

		public bool TokenCountCommit (Int32 CategoryID, int CategoryIndex)
		{
			m_TokenList.DoCountCommit(CategoryID, CategoryIndex);
			return (true);
		}

		public bool TokenCountPersist ()
		{
			// Persist if the Token Table not match the persisted binary version, otherwise return success
			m_TokenList.DoCountPersist(m_MsgAnalyzer.CategoryListGet().DayKeepCount);

			// if (TestReady) FireEnableDisable(this, m_TokenList.Ready() >= 1.0, true);			
			return (true);
		}

		public bool TokenCountBackup ()
		{
			// Backup if the persisted binary version does not match the XML backup, otherwise return success
			m_TokenList.DoCountBackup();
			return (true);
		}

		#endregion

		#region MsgAnalyzer Functions

		public int AnalyzerMailFromGet (CategoryList AnalyzerCategories, string Address)
		{
			return (AnalyzerCategories.IndexFromID(m_MsgAnalyzer.AnalyzerMailFromGet(Address)));
		}
		public bool AnalyzerMailFromSet (string Address, Int32 CategoryID)
		{
			return (m_MsgAnalyzer.AnalyzerMailFromSet(Address, CategoryID));
		}
		public void AnalyzerMailReset ()
		{
			m_MsgAnalyzer.AnalyzerMailFromReset();
		}
		public CategoryList AnalyzerCategoriesGet ()
		{
			return (m_MsgAnalyzer.CategoryListGet());
		}
		#endregion

		#region MessageCheck Functions

		public bool MessageCheckBegin ()
		{
			return (true);
		}

		public int MessageCheckUpdate (CategoryList AnalyzerCategories, string Headers, string SenderName, string SenderAddress, string Subject, string Body)
		{
			ArrayList CategoryProbList;
			double[] Percent;

			// Calculate probabilities for this message
			CategoryProbList = m_TokenList.DoCheckBegin(AnalyzerCategories, new string[] {SenderName, SenderAddress, Subject, Body});
			Percent = m_TokenList.DoCheck (CategoryProbList, AnalyzerCategories.Count);

			// Determine which category is most likely
			int		CategoryIndex = 0;
			double	CategoryMargin = 0;
			for (int ii=0; ii<Percent.Length; ii++)
			{
				if ((Percent[ii] - AnalyzerCategories[ii].Threshold) > CategoryMargin)
				{
					CategoryIndex = ii;
				}
			}

			// Log Analyzer results (if requested)
			// Log Successes || Failures?
			// bLog = ((bResult) && (LOGMASK_SUCCESS == (m_LogLevel & LOGMASK_SUCCESS))) || ((!bResult) && (LOGMASK_FAIL == (m_LogLevel & LOGMASK_FAIL)));
			if (AnalyzerCategories.LogResults) FireLogAnalyzer (Percent, CategoryProbList);

			return (CategoryIndex);
		}

		public bool MessageCheckEnd ()
		{
			return (true);
		}

		#endregion

		#region Properties

		#region LogSecurityMask
		/// <summary>
		/// Set Security logging activity level
		/// </summary>
		[		
		Browsable(true),
		Category("Logging"),
		Description("Set Security logging activity level"),
		DefaultValue(LOGMASK_FAIL),
		]
		public long LogSecurityMask
		{
			get{ return m_LogLevel; }
			set{ m_LogLevel = value; }
		}
		#endregion

		#region Enabled
		/// <summary>
		/// Enabled Summary
		/// </summary>
		[		
		Browsable(true),
		Category("Behavior"),
		Description("Start and stop the Analyzer Component"),
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

		#region Licensee
		/// <summary>User licensing description string</summary>
		[		
		Browsable(true),
		Category("Licensing"),
		Description("User licensing description string"),
		DefaultValue(""),
		]
		public string Licensee
		{
			get {return m_Licensee;}
			set
			{
				m_Licensee = value;
				m_LicenseType = LIC_FREEWARE;
			}
		}
		#endregion

		#region LicenseNumber
		/// <summary>User license serial number string</summary>
		[		
		Browsable(true),
		Category("Licensing"),
		Description("User license serial number string"),
		DefaultValue(""),
		]
		public string LicenseNumber
		{
			get {return m_LicenseNumber;}
			set
			{
				m_LicenseNumber = value;
				m_LicenseType = LIC_FREEWARE;
			}
		}
		#endregion

		#region LicenseCode
		/// <summary>User license activation code</summary>
		[		
		Browsable(true),
		Category("Licensing"),
		Description("User license activation code"),
		DefaultValue(""),
		]
		public string LicenseCode
		{
			get {return m_LicenseCode;}
			set
			{
				m_LicenseCode = value;
				m_LicenseType = LIC_FREEWARE;
			}
		}
		#endregion

		#region LicenseType
		/// <summary>User license type description string</summary>
		[		
		Browsable(true),
		Category("Licensing"),
		Description("User license type description string"),
		DefaultValue(LIC_FREEWARE),
		]
		public string LicenseType
		{
			get {return m_LicenseType;}
		}
		#endregion

		#endregion

		#region Public Event Handlers
		
		#region OnLogAlert
		/// <summary>OnLogAlert Summary</summary>
		[		
		Browsable(true),
		Category("Events"),
		Description("Application Alert Logging"),
		DefaultValue(null),
		]		
		public event LogAlertEventHandler		OnLogAlert = null;
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

		#region OnEnableDisable
		/// <summary>OnEnableDisable Summary</summary>
		[		
		Browsable(true),
		Category("Events"),
		Description("PlugIn Enable/Disable Request"),
		DefaultValue(null),
		]		
		public event EnableDisableEventHandler	OnEnableDisable = null;
		#endregion

		#region OnLogAnalyzer
		/// <summary>OnLogAnalyzer Summary</summary>
		[		
		Browsable(true),
		Category("Events"),
		Description("Analyzer Activity Logging"),
		DefaultValue(null),
		]		
		public event LogAnalyzerEventHandler	OnLogAnalyzer = null;
		#endregion

		#endregion
	
		#region Event Firing Functions

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

		#region FireEnableDisable
		internal bool FireEnableDisable(object sender, bool AnalyzerReady, bool IsEnable) 
		{
			// Fire event
			if (this.OnEnableDisable != null)
			{
				return (this.OnEnableDisable(sender, AnalyzerReady, IsEnable));
			}
			return (false);
		}
		#endregion

		#region FireLogAnalyzer
		internal void FireLogAnalyzer(double[] Percent, ArrayList CategoryProbList) 
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
				if (this.OnLogAnalyzer != null)
				{
					// Pack Probability data into a string array
					string[] ProbList = new string[Math.Min(MAXTOKENCHECK, CategoryProbList.Count)];
					for (int ii=0; ii<ProbList.Length; ii++)
					{
						ProbList[ii] = ((CategoryProbEntry)CategoryProbList[ii]).ToString();
					}
					this.OnLogAnalyzer(this, Percent, ProbList);
				}
			}
			catch {}
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
	/// <summary>Message Analyzer Logging EventHandler Summary</summary>
	public delegate void LogAnalyzerEventHandler (object Sender, double[] Percent, string[] CategoryProbList);
	#endregion

	#region Enable/Disable Delegates
	/// <summary>Enable/Disable EventHandler Summary</summary>
	public delegate bool EnableDisableEventHandler (object Sender, bool AnalyzerReady, bool IsEnable);
	#endregion

	#endregion

}
