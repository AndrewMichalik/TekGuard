// Sample TekGuard MailServer database driven configuration code
// Contact Vector Information Systems, Inc (www.VInfo.com) for
// information regarding our commercial and SQL Server versions.
using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using TGMConnector;
using TGMController;
using TGMDataStore;
using TGMMessenger;
using TGMSecurity;
using TGMailServer.com.vinfo.www;

namespace TGMailServer
{
	/// <summary>
	/// Summary description for Bootstrap
	/// </summary>
	public class Bootstrap
	{
		// Component members
		private MailServer			m_MailServer			= null;			// Pointer to parent TGMailServer instance
		private DBQuery				m_DBIniQuery			= null;			// Initialization Database query object
		private DBQuery				m_DBCfgQuery			= null;			// Configuration Database query object
//		private DBQuery				m_DBSecQuery			= null;			// Security Database query object
		private DBQuery				m_DBMsgQuery			= null;			// Message Template query object
		private HostQuery			m_HostQuery				= null;			// Remote Host Query object
		private LogQueue			m_LogQueue				= null;			// Application display logging object

		// Bootstrap initialization file table constants
		private const string		INI_BASENAME			= "TGMInitialize";
		private const string		INI_CONNTEMPATE			= "{2}";
		private const string		TBL_INITIALIZE			= "tblApplicationInitialize";
		private const string		TBL_DATASOURCE			= "tblDataSource";
		private const string		TBL_REMOTESERVER		= "tblRemoteServer";

		// Connection List data names
		private const string		NAM_INITIALIZE			= "Initialize";
		private const string		NAM_CONFIGURE			= "Configure";
		private const string		NAM_SECURITY			= "Security";
		private const string		NAM_MESSAGE				= "Message";

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
		private const int			WEB_TIMEOUT				= 10000;			// WebMethod wait for 10 seconds
		private	bool				m_bCheckUpdates			= true;
		private	Thread				m_thrChecking			= null;
		private	DateTime			m_dtLastCheck			= DateTime.MinValue;

// Configuration Database table constants
		private const string		QRY_DOMAINPOIPPREF		= "qryDomainIDPostIPPref";
		private const string		QRY_DOMAINHOSTPOST		= "qryDomainIDHostPost";
		private const string		COL_POSTOFFICE			= "PostOffice";
		private const string		COL_LOGSMTPCMDS			= "LogSMTPCmds";
		private const string		COL_LOGPOPCMDS			= "LogPOPCmds";
		private const string		COL_PORTSMTP			= "PortSMTP";
		private const string		COL_PORTPOP				= "PortPOP";
		private const string		COL_THREADMAXSMTP		= "ThreadMaxSMTP";
		private const string		COL_THREADMAXPOP		= "ThreadMaxPOP";
		private const string		COL_RETRYPERIOD			= "RetryPeriod";
		private const string		COL_RETRYCOUNT			= "RetryCount";
		private const string		COL_LOCALIP				= "LocalIP";
		private const string		COL_PREFERREDOUT		= "PreferredOut";
		private const string		COL_DNS					= "Dns";

		#region Constructors
		public Bootstrap(MailServer MailServer)
		{
			// Set pointer to parent instance
			m_MailServer = MailServer;
		}
		#endregion

		#region Initialize (Connector)
		internal bool Initialize(Connector Connector, string ConfigFilePath, out string ErrorText)
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

				// AJM Note 5/2005: Ignore for this release
				// Read the Security database entry
				// drRow = (dtTbl.Select(COL_DATANAME + " = '" + NAM_SECURITY + "'"))[0];

				// Retrieve the Security connection parameters (MDB)
				// DBConnEntry = new DBConnEntry(drRow[COL_CONNTEMPLATE].ToString(), drRow[COL_USERID].ToString(), drRow[COL_PASSWORD].ToString(), ConfigFilePath, ConfigFilePath, drRow[COL_DATASOURCE].ToString(), drRow[COL_DATATYPE].ToString(), DBConnList.DBS_MODE_RW);

				// Check/Copy the blank database on new installation, return false on failure
				// if (!CheckVersionCopy(DBConnEntry, CFG_VERSIONFORMAT, out ErrorText)) return (false);

				// Add this entry to the list
				// DBConnList.Add(drRow[COL_DATANAME].ToString(), DBConnEntry);

				// Read the Message Template entry
				drRow = (dtTbl.Select(COL_DATANAME + " = '" + NAM_MESSAGE + "'"))[0];

				// Build the Message Template XML file connection parameters
				DBConnEntry = new DBConnEntry(drRow[COL_CONNTEMPLATE].ToString(), drRow[COL_USERID].ToString(), drRow[COL_PASSWORD].ToString(), ConfigFilePath, ConfigFilePath, drRow[COL_DATASOURCE].ToString(), drRow[COL_DATATYPE].ToString(), DBConnList.DBS_MODE_RW);

				// Check/Copy the blank template on new installation, return false on failure
				if (!CheckVersionCopy(DBConnEntry, CFG_VERSIONFORMAT, out ErrorText)) return (false);

				// Add this entry to the list
				DBConnList.Add(drRow[COL_DATANAME].ToString(), DBConnEntry);

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

			// AJM Note 5/2005: Ignore for this release
			// Initialize the configuration settings database object
			// if (null == (m_DBSecQuery = Connector.DBConnectLocal(NAM_SECURITY, out ErrorText))) return (false);

			// Initialize the configuration settings database object
			if (null == (m_DBMsgQuery = Connector.DBConnectLocal(NAM_MESSAGE, out ErrorText))) return (false);

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
				if (!Connector.RMServeRemote(RMServerEntry, out ErrorText)) return (false);
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
		internal bool Initialize (Controller svrController, out string ErrorText)
		{
			// Bootstrap initialization complete, initialize Controller component
			// Pass the error text back to caller since logging is not yet fully active
			bool bSuccess = svrController.Initialize(frmSplash.getAssemblyTitle(), frmSplash.getAssemblyIcon(), m_LogQueue, m_HostQuery, m_DBIniQuery, m_DBCfgQuery, out ErrorText);

			// Check the program version at startup?
			m_bCheckUpdates = Convert.ToBoolean(m_DBIniQuery.DSGetRow_XML(TBL_INITIALIZE)[COL_CHECKUPDATES]);

			// Show the user interface at service startup?
			if (bSuccess) 
			{
				bool bShowInterface = AppMain.IsApplication || Convert.ToBoolean(m_DBIniQuery.DSGetRow_XML(TBL_INITIALIZE)[COL_SHOWINTERFACE]);
				if (bShowInterface) svrController.Visible = true;
			}

			// Return success
			return (bSuccess);
		}
		#endregion

		#region Initialize (DataStore)
		internal bool Initialize(DataStore svrDataStore, string PostOfficePath, out string ErrorText)
		{
			// Assume Success
			bool bSuccess = true;

			// Pass the error text back to caller since logging is not yet fully active
			ErrorText = null;

			// Fill the dataset using the existing connection and specified query string
			DataRow drRow = m_DBCfgQuery.DSSelectRow ("SELECT * from " + QRY_SETTINGS);
			if (drRow == null) return false;

			// Get the DataStore polling settings
			svrDataStore.RetryPeriod	= Convert.ToInt32(drRow[COL_RETRYPERIOD]);
			svrDataStore.RetryCount		= Convert.ToInt32(drRow[COL_RETRYCOUNT]);
						
			// Adjust some settings for debug testing
			if (System.Diagnostics.Debugger.IsAttached)
			{
				svrDataStore.RetryPeriod	= 60;	// 60 seconds per period
				svrDataStore.RetryCount		=  4;	//  4 retries per message	
			}

			// Initialize the domain post office paths
			string[] PostOfficeDirs;
			bSuccess &= ConfigPostOffices(out PostOfficeDirs);
			bSuccess &= svrDataStore.Initialize(PostOfficePath, PostOfficeDirs, m_DBMsgQuery, out ErrorText);

			// Return
			return (bSuccess);
		}

		#region ConfigPostOffices
		private bool ConfigPostOffices(out string[] PostOfficeDirs)
		{
			// Initialize list of domains and directories
			ArrayList DomainPODirList = new ArrayList();
			PostOfficeDirs = null;

			try
			{
				// Get application startup directory
				string StartupPath = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
				if (!StartupPath.EndsWith("\\")) StartupPath = StartupPath + "\\";

				// Make sure paths are relative to the startup directory
				Directory.SetCurrentDirectory(StartupPath);

				// Get Post Office information
				StringBuilder Select = new StringBuilder();
				Select.AppendFormat ("SELECT DISTINCT [{0}] FROM {1}", COL_POSTOFFICE, QRY_DOMAINHOSTPOST);
				
				// Fill the dataset using the existing connection and specified query string
				DataTable dtTbl = m_DBCfgQuery.DSSelectTable (Select.ToString());
				if (dtTbl == null) return (true);

				// Note: No Post Offices may be a normal condition (ex: fresh install)
				foreach (DataRow drRow in dtTbl.Rows)
				{
					// Read the domain post office directory settings
					string DomainDir = drRow[COL_POSTOFFICE].ToString();
					// Strip the trailing slash (if present)
					if (DomainDir.EndsWith("\\")) DomainDir = DomainDir.Remove(DomainDir.Length - 1, 1);
					DomainPODirList.Add(DomainDir);					
				}

				// Write the string array
				PostOfficeDirs = (string[])(DomainPODirList.ToArray(typeof(string)));

				// Success
				return true;
			}
			catch(Exception ex)
			{
				m_MailServer.LogException(new System.Diagnostics.StackFrame(), ex.Message, null);
				return false;
			}
		}
		#endregion

		#endregion
	
		#region Initialize (Security)
		internal bool Initialize(Security svrSecurity, out string ErrorText)
		{
			// Assume Success
			bool bSuccess = true;

			// Initialize the Security configuration settings
			svrSecurity.LogSecurityMask = Security.LOGMASK_FAIL | Security.LOGMASK_SUCCESS;

			// Connector the local security settings
			bSuccess &= svrSecurity.Initialize(m_DBCfgQuery);
			
			// Set ErrorText
			ErrorText = bSuccess ? null : "Can not initialize Security Component";

			// Return
			return (bSuccess);
		}
		#endregion

		#region Initialize (Messenger)
		internal bool Initialize(Messenger svrMessenger, out string ErrorText)
		{
			// Assume Success
			bool bSuccess = true;

			// Initialize the Messenger configuration settings
			bSuccess &= ConfigSettings(svrMessenger);

			// Initialize Messenger list of domains and IP addresses
			bSuccess &= ConfigIPPOPrefList(svrMessenger);

			// Set ErrorText
			ErrorText = bSuccess ? null : "Can not initialize Messenger Component";

			// Return
			return (bSuccess);
		}

		#region ConfigSettings
		internal bool ConfigSettings(Messenger svrMessenger)
		{
			try
			{
				// Fill the dataset using the existing connection and specified query string
				DataRow drRow = m_DBCfgQuery.DSSelectRow ("SELECT * from " + QRY_SETTINGS);
				if (drRow == null) return false;

				// Get licensing information
				svrMessenger.Licensee		= drRow[COL_LICENSEE].ToString();
				svrMessenger.LicenseNumber	= drRow[COL_LICENSENUMBER].ToString();
				svrMessenger.LicenseCode	= drRow[COL_LICENSECODE].ToString();

				// Set logging parameters
				svrMessenger.LogSMTPCmds	= Convert.ToBoolean(drRow[COL_LOGSMTPCMDS]);
				svrMessenger.LogPOPCmds		= Convert.ToBoolean(drRow[COL_LOGPOPCMDS]);

				// Get the Receiver settings
				svrMessenger.PortSMTP		= Convert.ToInt32(drRow[COL_PORTSMTP]);
				svrMessenger.PortPOP		= Convert.ToInt32(drRow[COL_PORTPOP]);
				svrMessenger.ThreadMaxSMTP	= Convert.ToInt32(drRow[COL_THREADMAXSMTP]);
				svrMessenger.ThreadMaxPOP	= Convert.ToInt32(drRow[COL_THREADMAXPOP]);

				// Set up the Dns Servers
				ArrayList DnsList = new ArrayList();
				for (int ii=1; ii<=4; ii++)
				{
					string Dns =drRow[COL_DNS + ii.ToString()].ToString();
					if (Dns.Length != 0) DnsList.Add(Dns);
				}
				// Minimum Dns entries of 1, max of 4
				if (DnsList.Count == 0) return (false);
				svrMessenger.DnsServers = (string[])DnsList.ToArray(typeof(string));

				// Success
				return true;
			}
			catch(Exception ex)
			{
				m_MailServer.LogException(new System.Diagnostics.StackFrame(), ex.Message, null);
				return false;
			}
		}
		#endregion

		#region ConfigIPPOPrefList
		public bool ConfigIPPOPrefList(Messenger svrMessenger)
		{
			// Initialize list of domains and IP addresses
			ArrayList DomainNameIPPOList = new ArrayList();

			try
			{
				// Get IP and Post Office information
				StringBuilder Select = new StringBuilder();
				Select.AppendFormat ("SELECT DISTINCT [{0}], [{1}], [{2}] FROM {3}", COL_LOCALIP, COL_POSTOFFICE, COL_PREFERREDOUT, QRY_DOMAINPOIPPREF);
				
				// Fill the dataset using the existing connection and specified query string
				DataTable dtTbl = m_DBCfgQuery.DSSelectTable (Select.ToString());
				if (dtTbl == null) return false;

				// Read the domain post office directory settings
				// Note: No settings may be normal on a fresh install
				foreach(DataRow drRow in dtTbl.Rows)
				{
					// Handle special IP Addresses (127.0.0.1 is localhost)
					string	LocalIP			= Convert.ToString(drRow[COL_LOCALIP]);
					string	PostOffice		= Convert.ToString(drRow[COL_POSTOFFICE]);
					bool	PreferredOut	= Convert.ToBoolean(drRow[COL_PREFERREDOUT]);

					// 0.0.0.0 = all IP addresses, 0.0.0.1 = base IP address
					if (LocalIP == "0.0.0.0")
					{
						// Get all local IP Addresses
						string[] LocalList = m_HostQuery.IPLocalList;
						foreach (string IP in LocalList)
						{
							DomainNameIPPOList.Add(new IPPOPrefEntry(IP, PostOffice, PreferredOut));
							PreferredOut = false;
						}
					}
					else 
					{
						if (LocalIP == "0.0.0.1") LocalIP = m_HostQuery.IPPrimary;
						DomainNameIPPOList.Add(new IPPOPrefEntry(LocalIP, PostOffice, PreferredOut));					
					}				
				}

				// Copy to the Messenger component
				svrMessenger.IPPOPrefList = (IPPOPrefEntry[])(DomainNameIPPOList.ToArray(typeof(IPPOPrefEntry)));
				
				// Success
				return true;
			}
			catch(Exception ex)
			{
				DomainNameIPPOList = null;
				m_MailServer.LogException(new System.Diagnostics.StackFrame(), ex.Message, null);
				return false;
			}
		}
		#endregion

		#endregion
			
		#region TGMCheckUpdates
		public void TGMCheckUpdates ()
		{
			// Already Checking?
			if (m_thrChecking != null) return;

			// Have we checked in the past week since startup?
			if (!m_bCheckUpdates || (m_dtLastCheck.AddDays(VER_CHECKWEEKLY) > DateTime.Now)) return;

			// Create a new thread for the WebMethods call
			m_thrChecking = new Thread(new ThreadStart(TGMCheckUpdates_Thread));
			m_thrChecking.Priority = ThreadPriority.Lowest;
			m_thrChecking.IsBackground = true;
			m_thrChecking.Start();
		}
		private void TGMCheckUpdates_Thread()
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
				m_MailServer.Sponsors = Sponsors;

				// New version available?
				if (NewVersion != null) m_MailServer.LogAlert("Start", "A newer version (" + NewVersion + ") is available", null);
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
					m_MailServer.LogAlert("CheckVersionCopy", ErrorText, null);
					return (false);					
				}

				// Return success
				ErrorText = null;
				return (true);
			}
			catch(Exception ex)
			{
				m_MailServer.LogException(new System.Diagnostics.StackFrame(), ex.Message, null);
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
				m_MailServer.LogAlert("CheckRename", FileFullPath + ": Initialization file version mismatch, renamed to " + FileFullPathBackup, null);
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
