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
using TGPAnalyzer;
using TGPConnector;
using TGPController;
using TGPlugIn.com.vinfo.www;

namespace TGPlugIn
{
	/// <summary>
	/// Summary description for Bootstrap
	/// </summary>
	public class Bootstrap
	{
		// Component members
		private PlugIn				m_PlugIn				= null;		// Pointer to parent TGPlugIn instance
		private DBQuery				m_DBIniQuery			= null;		// Initialization Database query object
		private DBQuery				m_DBCfgQuery			= null;		// Configuration Database query object
		private DBQuery				m_DBAnaQuery			= null;		// Analyzer Database query object
		private HostQuery			m_HostQuery				= null;		// Remote Host Query object
		private LogQueue			m_LogQueue				= null;		// Application display logging object

		// Objects shared with parent process		
		private MsgTokens			m_MsgTokens				= null;		// Database MsgTokens object
		private MsgAnalyzer			m_MsgAnalyzer			= null;		// Database Category object

		// Bootstrap initialization file table constants
		private const string		INI_BASENAME			= "TGPInitialize";
		private const string		INI_CONNTEMPATE			= "{2}";
		private const string		TBL_INITIALIZE			= "tblApplicationInitialize";
		private const string		TBL_DATASOURCE			= "tblDataSource";
		private const string		TBL_REMOTESERVER		= "tblRemoteServer";

		// Connection List data names
		private const string		NAM_INITIALIZE			= "Initialize";
		private const string		NAM_CONFIGURE			= "Configure";
		private const string		NAM_ANALYZER			= "Analyzer";
		private const string		NAM_TOKENS				= "Tokens";

		// Bootstrap initialization and Configuration database versions
		private const string		INI_VERSIONFORMAT		= "0.7.9";
		private const string		CFG_VERSIONFORMAT		= "0.7.9";

		// Initialization Database table constants
		private	const string		COL_VERSIONFORMAT		= "VersionFormat";
		private const string		COL_DATANAME			= "DataName";
		private const string		COL_CONNTEMPLATE		= "ConnTemplate";
		private const string		COL_USERID				= "UserID";
		private const string		COL_PASSWORD			= "Password";
		private const string		COL_DATASOURCE			= "DataSource";
		private const string		COL_DATATYPE			= "DataType";
		private const string		COL_SERVERNAME			= "ServerName";
		private const string		COL_ADDRESS				= "Address";
		private const string		COL_PORT				= "Port";
		private const string		COL_APPNAME				= "AppName";
		private const string		COL_CRYPTKEY			= "CryptKey";
		private const string		COL_ENABLED				= "Enabled";
		private const string		COL_CHECKUPDATES		= "StartupCheckUpdates";
		private const string		COL_SHOWINTERFACE		= "StartupShowInterface";
		private const string		COL_SHOWTRAYICON		= "StartupShowTrayIcon";

		// Configuration Database table constants
		private const string		QRY_SETTINGS			= "qryApplicationSettings";
		private const string		COL_LICENSEE			= "Licensee";
		private const string		COL_LICENSENUMBER		= "LicenseNumber";
		private const string		COL_LICENSECODE			= "LicenseCode";

		// Version and Updates checking
		private const int			VER_CHECKWEEKLY			= 7;
		private const int			WEB_TIMEOUT				= 10000;		// WebMethod wait for 10 seconds
		private	bool				m_bCheckUpdates			= true;
		private	Thread				m_thrChecking			= null;
		private	DateTime			m_dtLastCheck			= DateTime.MinValue;

		#region Constructors
		public Bootstrap(PlugIn PlugIn)
		{
			// Set pointer to parent instance
			m_PlugIn = PlugIn;
		}
		#endregion

		#region Initialize (Connector)
		internal bool Initialize(Connector Connector, string ConfigFilePath, string UserDataPath, out string ErrorText)
		{
			// Pass the error text back to caller since logging is not yet fully active

			// Initialize the list of configuration database connections
			DBConnList DBConnList = new DBConnList();

			// Load the list of available database connections
			try
			{
				// Build the Initialization file connection parameters (XML)
				DBConnEntry DBConnEntry = new DBConnEntry(INI_CONNTEMPATE, "", "", ConfigFilePath, ConfigFilePath, INI_BASENAME, DBConnEntry.FILE_TYPE_XML, DBConnList.DBS_MODE_RW);

				// Check/Copy the blank database on new installation, return false on failure
				if (!CheckVersionCopy(DBConnEntry, INI_VERSIONFORMAT, out ErrorText)) return (false);

				// Add the bootstrap initialization file to the list of database connections
				DBConnList.Add(NAM_INITIALIZE, DBConnEntry);

				// Read the initialization file database entries
				DataTable dtTbl = IniQueryTable (DBConnEntry.FileFullPath, TBL_DATASOURCE);

				// Read the Configuration database entry
				DataRow drRow = (dtTbl.Select(COL_DATANAME + " = '" + NAM_CONFIGURE + "'"))[0];

				// Retrieve the Configuration connection parameters (MDB)
				DBConnEntry = new DBConnEntry(drRow[COL_CONNTEMPLATE].ToString(), drRow[COL_USERID].ToString(), drRow[COL_PASSWORD].ToString(), ConfigFilePath, ConfigFilePath, drRow[COL_DATASOURCE].ToString(), drRow[COL_DATATYPE].ToString(), DBConnList.DBS_MODE_RW);

				// Check/Copy the blank database on new installation, return false on failure
				if (!CheckVersionCopy(DBConnEntry, CFG_VERSIONFORMAT, out ErrorText)) return (false);

				// Add this entry to the list
				DBConnList.Add(drRow[COL_DATANAME].ToString(), DBConnEntry);

				// Read the Analyzer database entry
				drRow = (dtTbl.Select(COL_DATANAME + " = '" + NAM_ANALYZER + "'"))[0];

				// Retrieve the Analyzer connection parameters (MDB)
				DBConnEntry = new DBConnEntry(drRow[COL_CONNTEMPLATE].ToString(), drRow[COL_USERID].ToString(), drRow[COL_PASSWORD].ToString(), ConfigFilePath, UserDataPath, drRow[COL_DATASOURCE].ToString(), drRow[COL_DATATYPE].ToString(), DBConnList.DBS_MODE_RW);

				// Check/Copy the blank database on new installation, return false on failure
				if (!CheckVersionCopy(DBConnEntry, CFG_VERSIONFORMAT, out ErrorText)) return (false);

				// Add this entry to the list
				DBConnList.Add(drRow[COL_DATANAME].ToString(), DBConnEntry);

				// Read the Token Table database entry
				drRow = (dtTbl.Select(COL_DATANAME + " = '" + NAM_TOKENS + "'"))[0];

				// Build the Token XML file connection parameters
				DBConnEntry = new DBConnEntry(drRow[COL_CONNTEMPLATE].ToString(), drRow[COL_USERID].ToString(), drRow[COL_PASSWORD].ToString(), ConfigFilePath, UserDataPath, drRow[COL_DATASOURCE].ToString(), drRow[COL_DATATYPE].ToString(), DBConnList.DBS_MODE_RW);

				// Note: Future version may use CheckVersionCopy if "fresh" TGPTokens_New.xml is stored in same directory
				// Check/Copy the blank database on new installation, return false on failure
				// if (!CheckVersionCopy(DBConnEntry, ConfigFilePath + DBConnEntry.FileNameNew, CFG_VERSIONFORMAT, out ErrorText)) return (false);

				// Copy the "new" (empty) database file on fresh installation, rename the old binary file to *_datetime.*
				if (CheckCopy (DBConnEntry))
				{
					CheckRename (DBConnEntry.FileFullPathBinary, DBConnEntry.FileFullPathBinaryBackup);
				}

				// Create the Messsage Token Table object
				m_MsgTokens = new TGPConnector.MsgTokens(Connector, DBConnEntry);

				// Initialize the local database connector
				if (!Connector.InitializeLocal(DBConnList, out ErrorText)) return (false);
			}
			catch(Exception ex)
			{
				ErrorText = ex.Message;
				return (false);
			}

			// Initialize the Initialization settings database object
			if (null == (m_DBIniQuery = Connector.DBConnectLocal(NAM_INITIALIZE, out ErrorText))) return (false);

			// Initialize the configuration settings database object
			if (null == (m_DBCfgQuery = Connector.DBConnectLocal(NAM_CONFIGURE, out ErrorText))) return (false);

			// Initialize the Analyzer settings database object
			if (null == (m_DBAnaQuery = Connector.DBConnectLocal(NAM_ANALYZER, out ErrorText))) return (false);

			// Create the Message Analyzer object
			m_MsgAnalyzer = new TGPConnector.MsgAnalyzer(Connector, m_DBAnaQuery);

			// Randomize the remote management password
			if (DBConnList[NAM_INITIALIZE].NewInstall && !RemotePasswordUpdate(m_DBIniQuery, TBL_REMOTESERVER, COL_PASSWORD, Guid.NewGuid().ToString()))
			{
				ErrorText = DBConnList[NAM_INITIALIZE].FileFullPath + ": Cannot randomize remote management password";
				return (false);
			}

			// Activate the remote management interface?
			if (Convert.ToBoolean(m_DBIniQuery.DSGetRow_XML(TBL_REMOTESERVER)[COL_ENABLED]))
			{
				// Get the remote database server initialization settings
				DataRow drRemote = m_DBIniQuery.DSGetRow_XML(TBL_REMOTESERVER);
				RMServerEntry RMServerEntry = new RMServerEntry(drRemote[COL_SERVERNAME].ToString(), drRemote[COL_ADDRESS].ToString(), drRemote[COL_PORT].ToString(), drRemote[COL_APPNAME].ToString(), drRemote[COL_USERID].ToString(), drRemote[COL_PASSWORD].ToString(), drRemote[COL_CRYPTKEY].ToString());

				// Initialize remote management database server interfaces
				if (!Connector.RMServeRemote(RMServerEntry, out ErrorText)) 
				{
					// Let TGPlugIn start even if DBRemote Server fails (no TGPDashboard)
					m_PlugIn.LogAlert("Initialize", "Error starting the TekGuard Remote Management Server connection", new string[][] {new string[] {"RMServerEntry", RMServerEntry.ToString()}});
				}			
			}

			// Initialize the remote host information component
			m_HostQuery = Connector.HostConnectLocal(out ErrorText);

			// Initialize the remote display logging component
			m_LogQueue = Connector.LogConnectLocal(out ErrorText);

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

			// Show the user interface at startup? (reserved for future use)
			bool bShowInterface = Convert.ToBoolean(m_DBIniQuery.DSGetRow_XML(TBL_INITIALIZE)[COL_SHOWINTERFACE]);

			// Return success
			return (bSuccess);
		}
		#endregion

		#region Initialize (Analyzer)
		internal bool Initialize(Analyzer Analyzer, out string ErrorText)
		{
			ErrorText = "";

			// Initialize EMail Analyzer control
			Analyzer.Initialize(m_MsgAnalyzer, m_MsgTokens, out ErrorText);

			// Fill the dataset using the existing connection and specified query string
			DataRow drRow = m_DBCfgQuery.DSSelectRow ("SELECT * from " + QRY_SETTINGS);
			if (drRow == null) 
			{
				ErrorText = "Cannot initialize Analyzer settings.";
				return false;
			}

			// Get licensing information
			Analyzer.Licensee		= drRow[COL_LICENSEE].ToString();
			Analyzer.LicenseNumber	= drRow[COL_LICENSENUMBER].ToString();
			Analyzer.LicenseCode	= drRow[COL_LICENSECODE].ToString();
			Analyzer.LogSecurityMask = Analyzer.LOGMASK_FAIL | Analyzer.LOGMASK_SUCCESS;

			// Success
			return (true);
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
		private void TGPCheckUpdates_Thread()
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
				m_PlugIn.Sponsors = Sponsors;

				// New version available?
				if (NewVersion != null) m_PlugIn.LogAlert("Start", "A newer version (" + NewVersion + ") is available", null);
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

		#region CheckVersionCopy
		private bool CheckVersionCopy (DBConnEntry DBConnEntry, string ExpectedVersion, out string ErrorText)
		{
			try
			{
				// Is this database file version OK?
				if (DBConnEntry.VersionFormat == ExpectedVersion) 
				{
					ErrorText = null;
					return (true);
				}

				// Rename the old file to *_datetime.*
				CheckRename (DBConnEntry.FileFullPath, DBConnEntry.FileFullPathBackup);

				// Copy the "new" (empty) database file on fresh installation
				CheckCopy (DBConnEntry);

				// Everything OK?
				if (DBConnEntry.VersionFormat != ExpectedVersion) 
				{
					// Old & new initialization files are bad or some other error
					ErrorText = DBConnEntry.FileFullPath + ": Initialization file version mismatch";
					m_PlugIn.LogAlert("CheckVersionCopy", ErrorText, null);
					return (false);					
				}

				// Return success
				ErrorText = null;
				return (true);
			}
			catch(Exception ex)
			{
				m_PlugIn.LogException(new System.Diagnostics.StackFrame(), ex.Message, null);
				ErrorText = ex.Message;
				return (false);
			}
		}
		private void CheckRename (string FileFullPath, string FileFullPathBackup)
		{
			// Rename the old file to *_datetime.*
			if (File.Exists(FileFullPath))
			{
				if (File.Exists(FileFullPathBackup)) File.Delete (FileFullPathBackup);
				File.Move (FileFullPath, FileFullPathBackup);

				// Log information regarding the overwrite
				m_PlugIn.LogAlert("CheckRename", FileFullPath + ": Initialization file version mismatch, renamed to " + FileFullPathBackup, null);
			}
		}
		private bool CheckCopy (DBConnEntry DBConnEntry)
		{
			// Copy the "new" (empty) database file on fresh installation
			if (!File.Exists(DBConnEntry.FileFullPath) && File.Exists(DBConnEntry.FileNewFullPath))
			{
				// Copy the empty database file
				File.Copy (DBConnEntry.FileNewFullPath, DBConnEntry.FileFullPath);

				// Indicate that this database is the result of a new installation
				DBConnEntry.NewInstall = true;
			}
			return (DBConnEntry.NewInstall);
		}
		#endregion

		#region IniQuery
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
		#endregion

		#region RemotePasswordUpdate
		private bool RemotePasswordUpdate (DBQuery DBIniQuery, string Table, string Column, string Value)
		{
			try
			{
				// Write updated Remote Password
				if (!DBIniQuery.DSUpdate_XML(Table, Column, 0, Value)) return (false);

				// Write back the XML data, return success indicator
				return (DBIniQuery.DSCommit_XML());
			}
			catch
			{
				// Update failed
				return (false);
			}
		}
	
		#endregion

	}
}
