using System;
using System.Collections;
using System.ComponentModel;

namespace TGMConnector
{
	/// <summary>
	/// Summary for Remote Logging Connection Class
	/// </summary>
	public class LogRemote : MarshalByRefObject
	{
		// Static members for logging & connection information
		internal static	Connector		m_Connector		= null;		// Pointer to parent TGMConnector object			

		// Static members for logging display
		// Note: Local variables are instantiated once for server, once for activated object
		private	static	LogRemoteList	m_LogAlerts		= new LogRemoteList();
		private	static	LogRemoteList	m_NetCommands	= new LogRemoteList();
		private	static	LogRemoteList	m_Security		= new LogRemoteList();
		private	static	LogRemoteList	m_SocketInfo	= new LogRemoteList();
		private	static	LogRemoteList	m_ThreadCount	= new LogRemoteList();
		private	static	LogRemoteList	m_BytesIn		= new LogRemoteList();
		private	static	LogRemoteList	m_BytesOut		= new LogRemoteList();
		private	static	LogRemoteList	m_EventCount	= new LogRemoteList();
		private const	int				MAX_LISTBOX		= 100;
		private const	int				MAX_LABEL		= 1;

		#region Constructors
		// Note: Remoted object constructor is called once for the first call
		// to System.Activator.GetObject by LogDisplay independent of the number
		// of instances activated or remoted by RegisterWellKnownServiceType

		internal LogRemote ()
		{
			// LogRemoteList must be static so that the event delegates are
			// not overwritten by a "new" one when the remote object is activated.
			m_LogAlerts.Initialize(m_Connector, MAX_LISTBOX);
			m_NetCommands.Initialize(m_Connector, MAX_LISTBOX);
			m_Security.Initialize(m_Connector, MAX_LISTBOX);
			m_SocketInfo.Initialize(m_Connector, MAX_LISTBOX);
			m_ThreadCount.Initialize(m_Connector, MAX_LABEL);
			m_BytesIn.Initialize(m_Connector, MAX_LABEL);
			m_BytesOut.Initialize(m_Connector, MAX_LABEL);
			m_EventCount.Initialize(m_Connector, MAX_LABEL);
		}

		public override object InitializeLifetimeService()
		{
			return null;
		}

		#endregion

		#region Properties (Public)

		#region RemoteURL
		/// <summary>
		/// RemoteURL Summary
		/// </summary>
		public String RemoteURL 
		{
			get 
			{
				// Get the remoted objects URL
				String Address = null;
				try
				{
					Address = RemotingIDHelper.GetURLForObject(this);
				}
				catch(Exception ex)
				{
					m_Connector.FireLogException_NoDisplay(ex, null, null);
				}
				return (Address != null ? Address : "localhost");
			}
		}
		#endregion

		#endregion

		#region Methods (Public)

		#region MaxCount
		public int MaxCount(LogDisplayType DisplayType, Credentials crRemote)
		{
			return (GetRemoteBase(DisplayType).MaxCount);
		}
		#endregion

		#region CopyOf
		public ArrayList CopyOf(LogDisplayType DisplayType, Credentials crRemote)
		{
			return ((ArrayList) GetRemoteBase(DisplayType));
		}
		#endregion

		#region UpdateEventHandler
		public void UpdateEventHandler(ListUpdateEventArgs UpdateArgs, Credentials crRemote)
		{
			try
			{
				// Get current remote base (Array)List
				LogRemoteList RemoteBase = GetRemoteBase(UpdateArgs.DisplayType);
				if (RemoteBase == null) return;

				// Call appropriate Add / Remove function
				if (UpdateArgs.ChangeType == ListChangedType.ItemAdded)
				{
					RemoteBase.OnAdd(UpdateArgs, crRemote);
				}
				else if (UpdateArgs.ChangeType == ListChangedType.Reset)
				{
					RemoteBase.OnClear(UpdateArgs, crRemote);
				}
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException_NoDisplay(ex, null, null);
			}
		}
		#endregion

		#region ListUpdateEvent Add/Remove
		public void ListUpdateEventAdd (LogDisplayType DisplayType, ListUpdateEventHandler RemoteHandler, Credentials crRemote)
		{
			if (LogAuthenticate(crRemote))
			{
				GetRemoteBase(DisplayType).BaseUpdateEventAdd(RemoteHandler);
			}
		}

		public void ListUpdateEventRemove (LogDisplayType DisplayType, ListUpdateEventHandler RemoteHandler, Credentials crRemote)
		{
			if (LogAuthenticate(crRemote))
			{
				GetRemoteBase(DisplayType).BaseUpdateEventRemove(RemoteHandler);
			}
		}
		#endregion

		#endregion
	
		#region LogAuthenticate
		private bool LogAuthenticate(Credentials Credentials)
		{
			// Check remote authentication
			return (m_Connector.RMServer.Authenticate(Credentials));
		}
		#endregion

		#region GetRemoteBase
		private LogRemoteList GetRemoteBase(LogDisplayType DisplayType)
		{
			switch (DisplayType)
			{
				case LogDisplayType.Alerts:
					lock (m_LogAlerts) return(m_LogAlerts);
				case LogDisplayType.NetCommands:
					lock (m_NetCommands) return(m_NetCommands);
				case LogDisplayType.Security:
					lock (m_Security) return(m_Security);
				case LogDisplayType.SocketInfo:
					lock (m_SocketInfo) return(m_SocketInfo);
				case LogDisplayType.ThreadCount:
					lock (m_ThreadCount) return(m_ThreadCount);
				case LogDisplayType.BytesIn:
					lock (m_BytesIn) return(m_BytesIn);
				case LogDisplayType.BytesOut:
					lock (m_BytesOut) return(m_BytesOut);
				case LogDisplayType.EventCount:
					lock (m_EventCount) return(m_EventCount);
				default:
					return (null);
			}
		}
		#endregion

	}

	#region List Update Delegates, Arguments & Abstract Classes

	/// <summary>List Update EventHandler Summary that will be thrown to clients</summary>
	public delegate bool ListUpdateEventHandler(object sender, object UpdateArgs, Credentials Credentials);

	// Event "Server" object and event handler classes:
	[Serializable]
	public class ListUpdateEventArgs : EventArgs
	{
		public	ListUpdateEventHandler	SenderHandler;
		public	LogDisplayType			DisplayType;
		public	ListChangedType			ChangeType;
		public	int						Index;
		public	object					Record;
		public	int						Count;

		public ListUpdateEventArgs(ListUpdateEventHandler SenderHandler, LogDisplayType DisplayType, ListChangedType ChangeType, object Record, int Index, int Count)
		{
			this.SenderHandler	= SenderHandler;
			this.DisplayType	= DisplayType;
			this.ChangeType		= ChangeType;
			this.Record			= Record;
			this.Index			= Index;
			this.Count			= Count;
		}
	}

	#region LogDisplayType
	// Public members for logging display
	// AJM Note: THese are not shared with other modules, and could be
	// auto-generated unique ID's or hash codes
	public enum LogDisplayType : int
	{
		Alerts = 0,
		NetCommands = 1,
		Security = 2,
		SocketInfo = 3,
		ThreadCount = 4,
		BytesIn = 5,
		BytesOut = 6,
		EventCount = 7
}
	#endregion

	#endregion

}
