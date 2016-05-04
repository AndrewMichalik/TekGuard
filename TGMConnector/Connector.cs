using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.IO;

namespace TGMConnector
{
	/// <summary>
	/// Summary for Connector
	/// </summary>
	public class Connector : System.ComponentModel.Component
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components	= null;
		private System.Windows.Forms.PictureBox picSplash;

		// Members for Remote Management
		private DBRemote			m_DBRemote;					// Local or Remoted Database connection
		private	RMServer			m_RMServer;					// Shared Remote Management Server instance

		// Remoting channel members
		private TcpChannel			m_ServerRemotingChannel;
		private TcpChannel			m_ClientReturnChannel;

		// Configuration database constants
		private const string		NAM_CONFIGURE		= "Configure";

		#region Constructors
		public Connector(System.ComponentModel.IContainer container)
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
		public Connector()
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
		~Connector()
		{
			// Unregister channels
			// RemotingServices.Disconnect(objStatic);
			// ChannelServices.UnregisterChannel(m_ClientReturnChannel);
			// ChannelServices.UnregisterChannel(m_ServerRemotingChannel);

		}
		#endregion

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Connector));
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

		#region InitializeLocal
		public bool InitializeLocal(DBConnList DBConnList, out string ErrorText)
		{
			// Pass the error text back to caller since logging is not yet fully active
			ErrorText = null;

			// Set the DBRemote (local) internal static members for logging & connection information
			DBRemote.m_Connector	= this;
			DBRemote.m_DBConnList	= DBConnList;

			// Set the LogRemote (local) internal static members for logging & connection information
			LogRemote.m_Connector	= this;

			// Set the HostRemote (local) internal static members for logging & connection information
			HostRemote.m_Connector	= this;

			// Initialize security class, Load Remote IP address range permissions
			// Note: The system will need to be restarted for IP Changes to take effect
			m_RMServer = new RMServer(this, DBConnList[NAM_CONFIGURE].ConnString);

			// Open the local database connections
			// Note: remote access security settings are ignored for local instances
			try
			{
				// Create a local copy of Database connection object
				m_DBRemote = new DBRemote();
				ErrorText = null;
			}
			catch(Exception ex)
			{
				ErrorText = ex.Message;
				return (false);
			}

			// Success
			return (true);
		}
		#endregion

		#region InitializeRemote
		public bool InitializeRemote(RMServerEntry RMServerEntry, out string ErrorText)
		{
			// Open the database connections
			try
			{
				// Better Solution: Step by Step guide to CAO creation through SAO class factories:
				// http://www.glacialcomponents.com/ArticleDetail.aspx?articleID=CAOGuide
				// Create an exclusive remote copy (CAO) of Database connection object
				// Activator.CreateInstance()
				
//AJM 12/04 - TGPDashboard occasional null reference error just before this line

				// Activate the remote copy (SAO) of the Database connection object
				m_DBRemote = (DBRemote)System.Activator.GetObject(typeof(DBRemote), RMServerEntry.URL(typeof(DBRemote)));

				// Authentication successful?
				if (m_DBRemote.DBAuthenticate(RMServerEntry.Credentials))
				{
					ErrorText = null;
					return (true);
				} 
				else
				{
					ErrorText =  RMServer.MSG_FAILEDLOGIN + ": " + RMServerEntry.URL(typeof(DBRemote));
					return (false);
				}
			}
			catch(Exception ex)
			{
				ErrorText = ex.Message + ": " + RMServerEntry.URL(typeof(DBRemote));
				return (false);
			}
		}
		#endregion

		#region RMServeRemote
		public bool RMServeRemote (RMServerEntry RMServerEntry, out string ErrorText) 
		{
			// Pass the error text back to caller since logging is not yet fully active
			ErrorText = null;

			try
			{
				// Set the DBRemote (local) internal static members for remote security
				RMServer.m_Credentials	= RMServerEntry.Credentials;
				RMServer.m_CryptKey		= RMServerEntry.CryptKey;
			
				// Create channel with a unique name every time
				// Instantiate more than one channel using different names
				// ex: oProps["name"] = Server + "_" + Guid.NewGuid().ToString();
				IDictionary ChannelProps = new Hashtable();
				ChannelProps["name"] = RMServerEntry.ServerName;
				ChannelProps["port"] = RMServerEntry.Port;

				// Use a custom channel sink to capture the IP address
				// Framework 1.1 Note: To change the security level to allow passing of delegates
				// and object references over Remoting boundaries, set the "typeFilterLevel". 
				BinaryClientFormatterSinkProvider ClientSink = new BinaryClientFormatterSinkProvider();
				IServerChannelSinkProvider Chain = new IPCheckerSinkProvider(null, null);
				IServerChannelSinkProvider Sink = Chain;
				Sink.Next = new BinaryServerFormatterSinkProvider();
				((BinaryServerFormatterSinkProvider)Sink.Next).TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;
				Sink = Sink.Next;
				m_ServerRemotingChannel = new TcpChannel(ChannelProps, ClientSink, Chain);
				ChannelServices.RegisterChannel(m_ServerRemotingChannel);

				// Better Solution: Step by Step guide to CAO creation through SAO class factories:
				// http://www.glacialcomponents.com/ArticleDetail.aspx?articleID=CAOGuide
				// Load the server for LogRemote Client Activated Object (CAO)
				// RemotingConfiguration.RegisterActivatedServiceType(GetType(LogRemote));

				// Load the server for LogRemote Server Activated Object (SAO)
				RemotingConfiguration.RegisterWellKnownServiceType(typeof(LogRemote), RMServerEntry.URI(typeof(LogRemote)), WellKnownObjectMode.Singleton);

				// Load the server for DBRemote Server Activated Object (SAO)
				RemotingConfiguration.RegisterWellKnownServiceType(typeof(DBRemote), RMServerEntry.URI(typeof(DBRemote)), WellKnownObjectMode.Singleton);

				// Load the server for HostRemote Server Activated Object (SAO)
				RemotingConfiguration.RegisterWellKnownServiceType(typeof(HostRemote), RMServerEntry.URI(typeof(HostRemote)), WellKnownObjectMode.Singleton);

				// Success
				return (true);
			}
			catch(Exception ex)
			{
				ErrorText = "Error connecting to server at " + RMServerEntry.URL(typeof(DBRemote)) + ", " + ex.Message;
				return (false);
			}
		}
		#endregion

		#region RMClientListen

		public bool RMClientListen(RMClientEntry RMClientEntry, out string ErrorText)
		{
			// Pass the error text back to caller since logging is not yet fully active
			ErrorText = null;

			try
			{
				// Force the ChannelName to avoid default name "tcp" colllision
				IDictionary ChannelProps = new Hashtable();
				ChannelProps["name"] = RMClientEntry.ClientName;
				ChannelProps["port"] = RMClientEntry.Port;

				// Set up return channel
				// Use a custom channel sink to capture the IP address
				// Framework 1.1 Note: To change the security level to allow passing of delegates
				// and object references over Remoting boundaries, set the "typeFilterLevel". 
				BinaryClientFormatterSinkProvider ClientSink = new BinaryClientFormatterSinkProvider();
				IServerChannelSinkProvider Chain = new IPCheckerSinkProvider(null, null);
				IServerChannelSinkProvider Sink = Chain;
				Sink.Next = new BinaryServerFormatterSinkProvider();
				((BinaryServerFormatterSinkProvider)Sink.Next).TypeFilterLevel = System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;
				Sink = Sink.Next;
				m_ClientReturnChannel = new TcpChannel(ChannelProps, ClientSink, Chain);
				ChannelServices.RegisterChannel(m_ClientReturnChannel);

				// Success
				return (true);
			}
			catch(Exception ex)
			{
				ErrorText = "Error connecting from client at " + RMClientEntry.URI + ", " + ex.Message;
				return (false);
			}
		}
		#endregion

		#region HostConnectLocal
		public HostQuery HostConnectLocal(out string ErrorText)
		{
			// Pass the error text back to caller since logging is not yet fully active

			try
			{
				// Attach the query object to the local copy of the Host Query object
				// Note: remote access security settings are ignored for local instances
				HostQuery HostQuery = new HostQuery(this, out ErrorText);

				// Success?
				return (HostQuery.Enabled ? HostQuery : null);
			}
			catch(Exception ex)
			{
				ErrorText = "Error connecting to Host Query, " + ex.Message;
				return (null);
			}
		}
		#endregion

		#region HostConnectRemote
		public HostQuery HostConnectRemote(RMServerEntry RMServerEntry, out string ErrorText)
		{
			// Pass the error text back to caller since logging is not yet fully active
			ErrorText = null;

			try
			{
				// Attach the query object to the connection object
				HostQuery HostQuery = new HostQuery(this, RMServerEntry.URL(typeof(HostRemote)), RMServerEntry.Credentials, RMServerEntry.CryptKey, out ErrorText);

				// Success?
				return (HostQuery.Enabled ? HostQuery : null);
			}
			catch(Exception ex)
			{
				ErrorText = "Error connecting to server at " + RMServerEntry.URL(typeof(HostRemote)) + ", " + ex.Message;
				return (null);
			}
		}
		#endregion

		#region DBConnectLocal
		public DBQuery DBConnectLocal(string DataName, out string ErrorText)
		{
			// Attach the query object to the local copy of the Database Query object
			// Note: remote access security settings are ignored for local instances
			DBQuery DBQuery = new DBQuery(this, DataName);

			// Set return error text
			ErrorText = DBQuery.Enabled ? null : "Failed to connect to " + DataName;

			// Success?
			return (DBQuery.Enabled ? DBQuery : null);
		}
		#endregion

		#region DBConnectRemote
		public DBQuery DBConnectRemote(RMServerEntry RMServerEntry, string DataName, out string ErrorText)
		{
			// Attach the query object to the connection object
			DBQuery DBQuery = new DBQuery(this, DataName, RMServerEntry.Credentials, RMServerEntry.CryptKey);

			// Set return error text
			ErrorText = DBQuery.Enabled ? null : "Failed to connect to " + RMServerEntry.URL(typeof(DBRemote)) + "/" + DataName;

			// Success?
			return (DBQuery.Enabled ? DBQuery : null);
		}
		#endregion

		#region LogConnectLocal
		public LogQueue LogConnectLocal(out string ErrorText)
		{
			// Pass the error text back to caller since logging is not yet fully active

			try
			{
				// Attach the query object to the local copy of the Log Display connection object
				// Note: remote access security settings are ignored for local instances
				LogQueue LogQueue = new LogQueue(this, out ErrorText);

				// Success?
				return (LogQueue.Enabled ? LogQueue : null);
			}
			catch(Exception ex)
			{
				ErrorText = "Error connecting to log display, " + ex.Message;
				return (null);
			}
		}
		#endregion

		#region LogConnectRemote
		public LogQueue LogConnectRemote(RMServerEntry RMServerEntry, out string ErrorText)
		{
			// Pass the error text back to caller since logging is not yet fully active
			ErrorText = null;

			try
			{
				// Attach the query object to the connection object
				LogQueue LogQueue = new LogQueue(this, RMServerEntry.URL(typeof(LogRemote)), RMServerEntry.Credentials, RMServerEntry.CryptKey, out ErrorText);

				// Success?
				return (LogQueue.Enabled ? LogQueue : null);
			}
			catch(Exception ex)
			{
				ErrorText = "Error connecting to server at " + RMServerEntry.URL(typeof(LogRemote)) + ", " + ex.Message;
				return (null);
			}
		}
		#endregion

		#region AnalyzerStatusUpdate
		// Fire the remote event
		public bool AnalyzerStatusUpdate(object sender, object args) 
		{
			return (m_DBRemote.AnalyzerStatusUpdate(sender, args, RMServer.m_Credentials));
		}
		#endregion

		#region Properties

		#region DBRemote
		/// <summary>
		/// DBRemote Summary
		/// </summary>
		internal DBRemote DBRemote 
		{
			get {return (m_DBRemote);}
		}
		#endregion

		#region RMServer
		/// <summary>
		/// RMServer Summary
		/// </summary>
		internal RMServer RMServer 
		{
			get {return (m_RMServer);}
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

		#region OnLogException_NoDisplay
		/// <summary>OnLogException_NoDisplay Summary</summary>
		[		
		Browsable(true),
		Category("Events"),
		Description("Exception Process Logging (without Display Logging)"),
		DefaultValue(null),
		]		
		public event LogExceptionEventHandler	OnLogException_NoDisplay = null;
		#endregion

		#region OnConfigurationApply
		/// <summary>OnConfigureApply Summary</summary>
		[		
		Browsable(true),
		Category("Events"),
		Description("PlugIn Configuration Apply Change Request"),
		DefaultValue(null),
		]		
		public event RemoteRequestEventHandler	OnConfigurationApply = null;
		#endregion

		#region OnAnalyzerRebuildItemize
		/// <summary>OnAnalyzerRebuildItemize Summary</summary>
		[		
		Browsable(true),
		Category("Events"),
		Description("PlugIn Analyzer Rebuild Request"),
		DefaultValue(null),
		]		
		public event RemoteRequestEventHandler	OnAnalyzerRebuildItemize = null;
		#endregion

		#region OnAnalyzerRebuildExecute
		/// <summary>OnAnalyzerRebuildExecute Summary</summary>
		[		
		Browsable(true),
		Category("Events"),
		Description("PlugIn Analyzer Rebuild Itemize Request"),
		DefaultValue(null),
		]		
		public event RemoteRequestEventHandler	OnAnalyzerRebuildExecute = null;
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

		#region FireLogException_NoDisplay
		internal void FireLogException_NoDisplay(Exception ex, string ParamName, string ParamValue) 
		{
			try 
			{
				// AJM - separate try block for missing PDB?
				// Get caller stack information (also request FileName and Line#)
				StackFrame Frame = new StackFrame(1, true);
			
				// Fire event
				if (this.OnLogException_NoDisplay != null)
				{
					this.OnLogException_NoDisplay(Frame, ex.Message, new string[][] {new string[] {ParamName, ParamValue}});
				}
			}
			catch {}
		}
		#endregion

		#region FireConfigurationApply
		internal object FireConfigurationApply(object sender, object args) 
		{
			// Fire event
			if (this.OnConfigurationApply != null)
			{
				return (this.OnConfigurationApply(sender, args));
			}
			return (null);
		}
		#endregion

		#region FireAnalyzerRebuildItemize
		internal object FireAnalyzerRebuildItemize(object sender, object args) 
		{
			// Fire event
			if (this.OnAnalyzerRebuildItemize != null)
			{
				return (this.OnAnalyzerRebuildItemize(sender, args));
			}
			return (null);
		}
		#endregion

		#region FireAnalyzerRebuildExecute
		internal object FireAnalyzerRebuildExecute(object sender, object args) 
		{
			// Fire event
			if (this.OnAnalyzerRebuildExecute != null)
			{
				return (this.OnAnalyzerRebuildExecute(sender, args));
			}
			return (null);
		}
		#endregion

		#endregion

	}

	#region Event Delegates

	#region Logging Delegates
	/// <summary>Log Alert EventHandler Summary</summary>
	public delegate void LogAlertEventHandler (string Sender, string Message, string[][] Params);
	/// <summary>Log Exception EventHandler Summary</summary>
	public delegate void LogExceptionEventHandler (StackFrame Sender, string Message, string[][] Params);
	#endregion

	#endregion

}
