// Sample TekGuard MailServer database driven configuration code
// Contact Vector Information Systems, Inc (www.VInfo.com) for
// information regarding our commercial and SQL Server versions.
using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using TGPConnector;
using TGPController;
using TGPDashboard.com.vinfo.www;

namespace TGPDashboard
{
	/// <summary>
	/// Summary description for Bootstrap
	/// </summary>
	public class Bootstrap
	{
		// Component members
		private Dashboard			m_Dashboard				= null;		// Pointer to parent TGDashboard instance
		private DBQuery				m_DBIniQuery			= null;		// Initialization Database query object
		private DBQuery				m_DBCfgQuery			= null;		// Configuration Database query object
		private DBQuery				m_DBAnaQuery			= null;		// Analyzer Database query object
		private HostQuery			m_HostQuery				= null;		// Remote Host Query object
		private LogQueue			m_LogQueue				= null;		// Application display logging object

		// Bootstrap initialization file table constants
		private const string		INI_BASENAME			= "TGPInitialize";
		private const string		INI_CONNTEMPATE			= "{2}";
		private const string		TBL_INITIALIZE			= "tblApplicationInitialize";
		private const string		TBL_REMOTESERVER		= "tblRemoteServer";
		private const string		TBL_REMOTECLIENT		= "tblRemoteClient";
		private const string		NAM_INITIALIZE			= "Initialize";
		private const string		NAM_CONFIGURE			= "Configure";
		private const string		NAM_ANALYZER			= "Analyzer";

		// Bootstrap initialization and Configuration database versions
		private const string		INI_VERSIONFORMAT		= "0.7.9";
		private const string		CFG_VERSIONFORMAT		= "0.7.9";

		// Initialization Database table constants
		private	const string		COL_VERSIONFORMAT		= "VersionFormat";
		private const string		COL_SERVERNAME			= "ServerName";
		private const string		COL_ADDRESS				= "Address";
		private const string		COL_PORT				= "Port";
		private const string		COL_APPNAME				= "AppName";
		private const string		COL_USERID				= "UserID";
		private const string		COL_PASSWORD			= "Password";
		private const string		COL_CRYPTKEY			= "CryptKey";
		private const string		COL_ENABLED				= "Enabled";
		private const string		COL_CLIENTNAME			= "ClientName";
		private const string		COL_CHECKUPDATES		= "StartupCheckUpdates";
		private const string		COL_SHOWINTERFACE		= "StartupShowInterface";
		private const string		COL_SHOWTRAYICON		= "StartupShowTrayIcon";

		// Version and Updates checking
		private const int			VER_CHECKWEEKLY			= 7;
		private const int			WEB_TIMEOUT				= 10000;		// WebMethod wait for 10 seconds
		private	bool				m_bCheckUpdates			= true;
		private	Thread				m_thrChecking			= null;
		private	DateTime			m_dtLastCheck			= DateTime.MinValue;

		#region Constructors
		public Bootstrap(Dashboard Dashboard)
		{
			// Set pointer to parent instance
			m_Dashboard = Dashboard;
		}
		#endregion

		#region Initialize (Connector)
		internal bool Initialize(Connector svrConnector, string ConfigFilePath, out string ErrorText)
		{
			// Pass the error text back to caller since logging is not yet fully active

			// Initialize the remote connections
			RMServerEntry RMServerEntry = null;
			RMClientEntry RMClientEntry = null;

			// Load the list of available database connections
			try
			{
				// Build the Initialization file connection parameters
				DBConnEntry DBConnEntry = new DBConnEntry(INI_CONNTEMPATE, "", "", ConfigFilePath, ConfigFilePath, INI_BASENAME, DBConnEntry.FILE_TYPE_XML, DBConnList.DBS_MODE_RW);

				// Is this DB version OK?
				if (DBConnEntry.VersionFormat != INI_VERSIONFORMAT)
				{
					ErrorText = DBConnEntry.FileFullPath + ": Initialization file version mismatch";
					return (false);
				}
			
				// Get the remote management server initialization settings
				DataRow drRemote = IniQueryRow (DBConnEntry.FileFullPath, TBL_REMOTESERVER);
				RMServerEntry = new RMServerEntry(drRemote[COL_SERVERNAME].ToString(), drRemote[COL_ADDRESS].ToString(), drRemote[COL_PORT].ToString(), drRemote[COL_APPNAME].ToString(), drRemote[COL_USERID].ToString(), drRemote[COL_PASSWORD].ToString(), drRemote[COL_CRYPTKEY].ToString());

				// Initialize the remote database connector
				if (!svrConnector.InitializeRemote(RMServerEntry, out ErrorText)) return (false);

				// Get the remote management client initialization settings
				DataRow drClient = IniQueryRow (DBConnEntry.FileFullPath, TBL_REMOTECLIENT);
				RMClientEntry = new RMClientEntry(drClient[COL_CLIENTNAME].ToString(), drClient[COL_PORT].ToString());

				// Initialize the Remote Client Callback interface
				if (!svrConnector.RMClientListen(RMClientEntry,  out ErrorText)) return (false);
			}
			catch(Exception ex)
			{
				ErrorText = ex.Message;
				return (false);
			}

			// Initialize the Remote management settings database object
			if (null == (m_DBIniQuery = svrConnector.DBConnectRemote(RMServerEntry, NAM_INITIALIZE, out ErrorText))) return (false);

			// Is this DB version OK?
			if (m_DBIniQuery.DBVersionFormat() != INI_VERSIONFORMAT)
			{
				ErrorText = "Initialization file version mismatch at " + m_DBIniQuery.RemoteURL;
				return (false);
			}

			// Initialize the Configuration settings database object
			if (null == (m_DBCfgQuery = svrConnector.DBConnectRemote(RMServerEntry, NAM_CONFIGURE, out ErrorText))) return (false);

			// Is this DB version OK?
			if (m_DBCfgQuery.DBVersionFormat() != CFG_VERSIONFORMAT)
			{
				ErrorText = "Configuration file version mismatch at " + m_DBCfgQuery.RemoteURL;
				return (false);
			}

			// Initialize the Analyzer settings database object
			if (null == (m_DBAnaQuery = svrConnector.DBConnectRemote(RMServerEntry, NAM_ANALYZER, out ErrorText))) return (false);

			// Is this DB version OK?
			if (m_DBAnaQuery.DBVersionFormat() != CFG_VERSIONFORMAT)
			{
				ErrorText = "Analyzer file version mismatch at " + m_DBAnaQuery.RemoteURL;
				return (false);
			}

			// Initialize the remote host information component
			m_HostQuery = svrConnector.HostConnectRemote(RMServerEntry, out ErrorText);

			// Initialize the remote logging component
			m_LogQueue = svrConnector.LogConnectRemote(RMServerEntry, out ErrorText);

			// Success
			return (true);
		}
		#endregion

		#region Initialize (Controller)
		internal bool Initialize(Controller Controller, out string ErrorText)
		{
			// Bootstrap initialization complete, initialize Controller component
			// Pass the error text back to caller since logging is not yet fully active
			bool bSuccess = Controller.Initialize(frmSplash.getAssemblyTitle(), frmSplash.getAssemblyIcon(), m_LogQueue, m_HostQuery, m_DBIniQuery, m_DBCfgQuery, m_DBAnaQuery, out ErrorText);

			// Check the program version at startup?
			m_bCheckUpdates = Convert.ToBoolean(m_DBIniQuery.DSGetRow_XML(TBL_INITIALIZE)[COL_CHECKUPDATES]);

			return (bSuccess);
		}
		#endregion

		#region TGPCheckUpdates
		public void TGPCheckUpdates ()
		{
			// Already Checking?
			if (m_thrChecking != null) return;
			
			// Have we checked in the past week since startup?
			if (!m_bCheckUpdates || (m_dtLastCheck.AddDays(VER_CHECKWEEKLY) > DateTime.Now)) return;

			// Create a new thread for the WebMethods call
			m_thrChecking = new Thread(new ThreadStart(TGPCheckUpdates_Thread));
			m_thrChecking.Priority = ThreadPriority.Lowest;
			m_thrChecking.IsBackground = true;
			m_thrChecking.Start();
		}
		internal void TGPCheckUpdates_Thread()
		{
			string NewVersion;
			string[] Sponsors;

			// Connect to VISI web service
			// Note: This is done without sending any information about your computer
			if (CheckUpdates_WebMethod(out NewVersion, out Sponsors))
			{
				// Success - save last version check date
				m_dtLastCheck = DateTime.Now;

				// Update Sponsors list
				m_Dashboard.Sponsors = Sponsors;

				// New version available?
				if (NewVersion != null) m_Dashboard.LogAlert("Start", "A newer version (" + NewVersion + ") is available", null);
			}

			// Done checking
			m_thrChecking = null;
		}		
		private bool CheckUpdates_WebMethod (out string NewVersion, out string[] Sponsors)
		{
			try
			{
				// Get VISI Product info using WebMethods call
				ProductInfo Info = new ProductInfo();

				// Set the timeout to be relaively short
				Info.Timeout = WEB_TIMEOUT;

				// Get this local (running) version
				string ThisVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

				// Future: Use split command to compare embedded values
				string CurrentVersion = Info.getVersion();

				// Return version status
				NewVersion = (CurrentVersion.CompareTo(ThisVersion) > 0) ? Info.getVersion() : null;

				// Return sponsor list
				Sponsors = Info.getSponsors();

				// Success
				return (true);
			}
			catch
			{
				// Assume no updates available if info not available
				NewVersion = null;
				Sponsors = null;
				return (false);
			}
		}
		#endregion

		#region IniQuery (Table / Row)
		private DataTable IniQueryTable (string ConfigFile, string Table)
		{
			try
			{
				// Get bootstrap initialization settings
				DataSet dsSet = new DataSet();
				dsSet.ReadXml(ConfigFile);
				return (dsSet.Tables[Table]);
			}
			catch
			{
				// No information available
				return (null);
			}
		}
		
		private DataRow IniQueryRow (string ConfigFile, string Table)
		{
			try
			{
				// Get bootstrap initialization settings
				DataTable dtTbl = IniQueryTable(ConfigFile, Table);
				return (dtTbl.Rows[0]);
			}
			catch
			{
				// No information available
				return (null);
			}
		}
		#endregion

	}
}
