using System;
using System.Collections;
using System.Data;
using System.Data.OleDb;
using System.Runtime.Remoting.Messaging ;
using System.Text;
using System.Threading;

namespace TGMConnector
{
	/// <summary>
	/// Summary for Remote Management Server
	/// </summary>
	public class RMServer
	{
		// Component members
		private Connector			m_Connector			= null;		// Pointer to parent TGPConfigure instance
		private	string				m_ConnString;					// Configuration Connection string
		private	DataTable			m_IPPTable;						// Remote IP address range permissions

		// Static members for remote security
		internal static Credentials	m_Credentials		= null;		// Authentication credentials
		internal static string		m_CryptKey			= null;		// Encryption / Decryption keys

		// Authentication constants
		private const	int			DLY_AUTHERROR		= 1000;		// Authentication error message delay
		private int					m_AuthDelay			= 0;		// Authentication error count

		// Message strings
		internal const	string		MSG_FAILEDLOGIN		= "Failed remote authentication";
		internal const	string		MSG_INVALIDUSER		= "Invalid username/password";
		internal const	string		MSG_FAILEDREMOTE	= "Remote failed, connection removed";

		// Database table constants
		private	const string		QRY_DOMAINSIPRANGE	= "qrySecurityDomainsIPRange";
		private	const Int32			PRO_REMOTEMGMT		= 20;		// Remoting Configuration Protocol Identifier
		private	const string		COL_PROTOCOLID		= "ProtocolID";
		private	const string		COL_IPBEGIN			= "IPBegin";
		private	const string		COL_IPEND			= "IPEnd";
		private	const string		COL_PERMIT			= "Permit";

		#region Constructors
		public RMServer (Connector Connector, string ConnString)
		{
			// Set pointer to parent instance
			m_Connector = Connector;

			// Save the configuration data connection string
			m_ConnString = ConnString;

			// Save the security members
			// public RMServer (Connector Connector, string ConnString, Credentials Credentials, CryptKey CryptKey)
			// m_Credentials = Credentials;
			// m_CryptKey = CryptKey;

			// Load Remote IP address range permissions
			m_IPPTable = IPRemoteLoad (ConnString);
		}
		#endregion

		#region Authenticate
		public bool Authenticate(Credentials crRemote)
		{
			try 
			{
				// Assume Success
				bool Success = true;

				// Check the client IP Address
				string ClientIP = (string) CallContext.GetData("ClientIPAddress");

				// Note: May need to set the credentials for both local and remote access.
				//		 If a remote client calls DBRemote (for example), a call to display
				//		 an exception on the controller will come back as having a local
				//		 127.0.0.1 CallContext if the controller tries to use a remoted
				//		 DB connection.

				// Is this a direct connection (ie, local to the process with no remoting?)
				if (ClientIP == null) return (true);
				
				// AJM Test: Log remote process call
				if (!IPRemoteCheck(ClientIP))
				{
					m_Connector.FireLogAlert(MSG_FAILEDLOGIN, "ClientIP", ClientIP);
					return (false);
				}

				// Check the remote access security settings
				Success &= (DBRemoteDecrypt(crRemote.UserID, m_CryptKey) == m_Credentials.UserID);
				Success &= (DBRemoteDecrypt(crRemote.Password, m_CryptKey) == m_Credentials.Password);

				// Wait x seconds to return failed authentication
				if (Success) 
				{
					// Success
					m_AuthDelay = 0;
					return (true);
				}
				else
				{
					// Failure
					m_AuthDelay++;
					m_Connector.FireLogAlert(MSG_INVALIDUSER, "crRemote", Credentials.ToStringSafe(crRemote));
					for (int i=0; i<m_AuthDelay; i++) Thread.Sleep(DLY_AUTHERROR);
					return (false);
				}
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, "crRemote", Credentials.ToStringSafe(crRemote));
				return (false);
			}
		}
		#endregion

		#region Encrypt / Decrypt
		public string DBRemoteEncrypt (string Value, string Key)
		{
			return (Value);
		}
		public string DBRemoteDecrypt (string Value, string Key)
		{
			return (Value);
		}
		#endregion

		#region IPRemoteCheck

		private bool IPRemoteCheck(string IPString)
		{
			// Valid request?
			if (m_IPPTable == null) return (false);

			// Serialize access to the datatable object
			lock (m_IPPTable) try
			{
				// Note: Empty (non-null) table may be normal on a fresh install
				if (m_IPPTable.Rows.Count == 0) return (false);

				// Get the IP address as a an Int64 value
				Int64 RemoteIP = ParseIP(IPString);

				// Set up the FILTER (SELECT) clause
				StringBuilder Where = new StringBuilder();
				Where.AppendFormat( "({0} >= {1}) AND ({0} <= {2})", RemoteIP, COL_IPBEGIN, COL_IPEND);

				// Filter the datatable using the specified query string
				DataRow[] drRows = m_IPPTable.Select(Where.ToString());

				// No records: Protocol for IP Range Not permitted
				if (drRows.Length == 0) return (false);

				// "OR" each permission record
				bool bPermitted = false;
				foreach (DataRow drRow in drRows)
				{
					bPermitted |= Convert.ToBoolean(drRow[COL_PERMIT]);
				}
				return (bPermitted);
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, "IPString", IPString);
				return (false);
			}
		}

		#region ParseIP
		private Int64 ParseIP(string IPString)
		{
			const Int64 BAD_IP = -1;
			try
			{
				// Process as a 32 bit unsigned long
				Int64 iIP64 = 0;

				// Separate the octects
				string[] Octects = IPString.Split(new char[]{'.'});

				// Correct number of entries?
				if (Octects.Length != 4) return (BAD_IP);

				// Loop through all octects
				for (int ii=0; ii<Octects.Length; ii++)
				{
					if ((Convert.ToInt32(Octects[ii]) < 0) || (Convert.ToInt32(Octects[ii]) > 255)) return (BAD_IP);
					iIP64 += (Convert.ToInt64(Octects[ii]) << (8 * (3-ii)));
				}
				return (iIP64);
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, "IPString", IPString);
				return (BAD_IP);
			}
		}
		#endregion

		#endregion

		#region IPRemoteLoad
		private DataTable IPRemoteLoad(string ConnString)
		{
			// Retrieve TekGuard Remote IP Address permissions; catch exceptions
			try
			{
				// Open the database connection
				OleDbConnection OleDBConn = new OleDbConnection(ConnString);

				// Set up the WHERE clause
				// AJM Temp: PostOffice = null is for all, used for checking remoting permission
				// Change in future?
				StringBuilder Where = new StringBuilder();
				Where.AppendFormat( " WHERE [{0}]={1}", COL_PROTOCOLID, PRO_REMOTEMGMT);

				// Get the IP Remote Permissions settings table
				OleDbDataAdapter adSettings = new OleDbDataAdapter ("SELECT * FROM " + QRY_DOMAINSIPRANGE + Where, OleDBConn);

				// Fill the dataset and get the version format entry
				DataSet dsSet = new DataSet();
				adSettings.Fill (dsSet);

				// Any dataset or tables?
				if (dsSet == null) return (null);

				// Note: Empty (non-null) table may be normal on a fresh install
				if (dsSet.Tables.Count == 0) return (null);

				// Return the IP Remote Permissions table
				return (dsSet.Tables[0]);
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, "ConnString", ConnString);
				return (null);
			}
		}
		#endregion

	}

	#region Public Argument Structures

	#region RMServerEntry
	public class RMServerEntry 
	{
		private	string			m_ServerName;		// Server unique name
		private	string			m_Address;			// Server base address
		private	Int32			m_Port;				// Server port number
		private	string			m_AppName;			// Server application name
		private	Credentials		m_Credentials;		// Authentication credentials
		private	string			m_CryptKey;			// Remote connection encryption key

		#region Constructors
		public RMServerEntry (string ServerName, string Address, string Port, string AppName, string UserID, string Password, string CryptKey)
		{
			// Handle special IP Addresses (127.0.0.1 is localhost)
			// 0.0.0.1 = base IP address
			if (Address == "0.0.0.1") Address = IPAdaptersInfo.IPLocal();
		
			// Set class members
			m_ServerName	= ServerName;
			m_Address		= Address;
			m_Port			= Convert.ToInt32(Port);
			m_AppName		= AppName;
			m_Credentials	= new Credentials(UserID, Password);
			m_CryptKey		= CryptKey;	
		}
		#endregion

		#region Properties

		#region ServerName
		/// <summary>
		/// Server Summary
		/// </summary>
		public string ServerName 
		{
			get {return (m_ServerName);}
		}
		#endregion

		#region Address
		/// <summary>
		/// Server Summary
		/// </summary>
		public string Address 
		{
			get {return (m_Address);}
		}
		#endregion

		#region Port
		/// <summary>
		/// Port Summary
		/// </summary>
		public Int32 Port 
		{
			get {return (m_Port);}
		}
		#endregion

		#region Credentials
		/// <summary>
		/// Credentials Summary
		/// </summary>
		public Credentials Credentials 
		{
			get {return (m_Credentials);}
		}
		#endregion

		#region CryptKey
		/// <summary>
		/// CryptKey Summary
		/// </summary>
		public string CryptKey 
		{
			get {return (m_CryptKey);}
		}
		#endregion

		#endregion

		#region Methods

		#region URL
		public string URL (Type Type)
		{
			// local declarations
			string result = string.Empty;

			// Build URL
			result = string.Format(
				"tcp://{0}:{1}/{2}/{3}",
				Address,
				Port,
				m_AppName,
				Type.FullName);

			// Return URL string
			return result;
		}
		#endregion

		#region URI
		public string URI (Type Type)
		{
			return (string.Format("{0}/{1}", m_AppName, Type.FullName));
		}
		#endregion

		#endregion

	}
	#endregion

	#region RMClientEntry
	public class RMClientEntry 
	{
		private	string			m_ClientName;		// Client unique name
		private	Int32			m_Port;				// Client port number

		#region Constructors
		public RMClientEntry (string ClientName, string Port)
		{
			// Set class members
			m_ClientName	= ClientName;
			m_Port			= Convert.ToInt32(Port);
		}
		#endregion

		#region Properties

		#region ClientName
		/// <summary>
		/// ClientName Summary
		/// </summary>
		public string ClientName 
		{
			get {return (m_ClientName);}
		}
		#endregion

		#region Port
		/// <summary>
		/// Port Summary
		/// </summary>
		public Int32 Port 
		{
			get {return (m_Port);}
		}
		#endregion

		#region URI
		public string URI
		{
			get {return (string.Format("{0}:{1}", ClientName, m_Port.ToString()));}
		}
		#endregion

		#endregion

		#region Methods

		#endregion

	}
	#endregion

	#region Credentials
	[Serializable]
	public class Credentials
	{
		private string m_UserID;
		private string m_Password;

		#region Constructors
		public Credentials(string UserID, string Password) 
		{
			// Note: UserID is case insensitive
			m_UserID = UserID.ToLower();
			m_Password = Password;
		}
		#endregion

		#region Properties

		#region UserID
		public string UserID 
		{
			get {return (m_UserID);}
		}
		#endregion

		#region Password
		public string Password 
		{
			get {return (m_Password);}
		}
		#endregion

		#endregion

		#region Methods

		#region ToString
		internal static string ToStringSafe (Credentials cr)
		{
			if (cr != null) return (cr.UserID.ToString() + "/" + cr.Password.ToString());
			return ("null");
		}
		#endregion

		#endregion

	}
	#endregion

	#endregion

	#region Public Remoting Delegates

	// Delegate for GUI Invoke function to retain form/control threading
	public	delegate object WinFormInvokeHandler(object[] objArray);

	#region Remote Request Delegates
	// AJM Note: May need to replace this with RemoteEventHandlerClass
	/// <summary>Remote Request Event Handler Summary</summary>
	public delegate object RemoteRequestEventHandler (object Sender, object args);
	#endregion

	// Abstract class for encapsulating the event handler callback delegate
	internal abstract class RemoteEventHandlerClass : MarshalByRefObject
	{
		public bool RemoteEventCallback (object sender, object ServerArgs, Credentials Credentials)
		{
			try 
			{
				// Authenticate before accepting callback
				return (Authenticate(Credentials) ? InternalEventCallback(sender, ServerArgs) : false);
			}
			catch
			{
				return (false);
			}
		}

		protected abstract bool InternalEventCallback (object sender, object ServerArgs);
		protected abstract bool Authenticate (Credentials Credentials);

		public override object InitializeLifetimeService()
		{
			return null;
		}
	}
	#endregion

}