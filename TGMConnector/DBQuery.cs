using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Threading;

namespace TGMConnector
{
	/// <summary>
	/// Summary description for DBQuery
	/// </summary>
	public class DBQuery
	{
		// Component members
		private Connector				m_Connector			= null;		// Pointer to parent TGPConfigure instance
		private string					m_DataName			= null;		// DataName for selected database

		// Members for remote security
		// AJM Note: Could use m_Connector.RMServer.Credentials instead
		private	Credentials				m_Credentials;					// Authentication credentials
		private string					m_CryptKey;						// Encryption / Decryption keys

		// Database table constants
		private const string			TBL_INITIALIZE		= "tblApplicationInitialize";
		private	const string			QRY_SETTINGS		= "qryApplicationSettings";
		private	const string			COL_VERSIONFORMAT	= "VersionFormat";

		// Analyzer callback members
		private	AnalyzerStatusHandlerClass	m_EventClass;

		#region Constructors / Destructors
		// Local
		internal DBQuery (Connector Connector, string DataName)
		{
			// Set pointer to parent instance
			m_Connector = Connector;
			
			// Save DataName for selected database
			m_DataName = DataName;
		}

		// Remote
		internal DBQuery (Connector Connector, string DataName, Credentials Credentials, string CryptKey)
		{
			// Set pointer to parent instance
			m_Connector = Connector;

			// Save DataName for selected database
			m_DataName = DataName;

			// Set the remote access security settings
			m_Credentials = Credentials;
			m_CryptKey = CryptKey;
		}

		~DBQuery()
		{
			try 
			{
				// Remove server event handler
				AnalyzerStatusEventRemove();
			}
			catch {}
		}
		#endregion

		#region Properties

		#region Enabled
		/// <summary>
		/// Enabled Summary
		/// </summary>
		public bool Enabled 
		{
			get {return ((m_Connector.DBRemote != null) && m_Connector.DBRemote.Enabled(m_DataName, m_Credentials));}
		}
		#endregion

		#region RemoteURL
		/// <summary>
		/// RemoteURL Summary
		/// </summary>
		public string RemoteURL 
		{
			get {return (m_Connector.DBRemote.RemoteURL);}
		}
		#endregion

		#endregion

		#region DBAuthenticate
		public bool DBAuthenticate()
		{
			// Check remote authentication
			return (m_Connector.DBRemote.DBAuthenticate(m_Credentials));
		}
		#endregion

		#region ADCreate
		public OleDbDataAdapter ADCreate ()
		{
			return (m_Connector.DBRemote.ADCreate(m_Credentials));
		}
		#endregion

		#region ADCreateCommand
		public OleDbCommand ADCreateCommand (OleDbDataAdapter Adapter, string cmdText)
		{
			return (m_Connector.DBRemote.ADCreateCommand(m_DataName, Adapter, cmdText, m_Credentials));
		}
		#endregion

		#region ADSelect
		public bool ADSelect (OleDbDataAdapter Adapter, ref DataSet dsSet, string Select, string TableName)
		{
			return (m_Connector.DBRemote.ADSelect (m_DataName, Adapter, ref dsSet, Select, TableName, m_Credentials));
		}
		#endregion

		#region DSSelect (MDB)
		public DataSet DSSelect (string strCommand)
		{
			return (DSSelect (strCommand, null));
		}
		public DataSet DSSelect (string strCommand, OleDbDataAdapter Adapter)
		{
			return (m_Connector.DBRemote.DSSelect (m_DataName, strCommand, Adapter, m_Credentials));
		}
		#endregion

		#region DSSelectTable (MDB)
		public DataTable DSSelectTable (string strCommand)
		{
			return DSSelectTable(strCommand, null);
		}
		public DataTable DSSelectTable (string strCommand, OleDbDataAdapter Adapter)
		{
			try
			{
				// Fill the dataset using the existing connection
				DataSet dsSet = m_Connector.DBRemote.DSSelect (m_DataName, strCommand, Adapter, m_Credentials);

				// Any dataset or tables?
				if (dsSet == null) return (null);

				// Note: Empty (non-null) table may be normal on a fresh install
				if (dsSet.Tables.Count == 0) return (null);

				// Return table
				return (dsSet.Tables[0]);
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, "strCommand", strCommand);
				return null;
			}
		}
		#endregion

		#region DSSelectRow (MDB)
		public DataRow DSSelectRow (string strCommand)
		{
			return DSSelectRow(strCommand, null);
		}
		public DataRow DSSelectRow (string strCommand, OleDbDataAdapter Adapter)
		{
			try
			{
				// Get the dataset tables using the existing connection
				DataTable dtTbl =  DSSelectTable (strCommand, Adapter);

				// Any tables or rows?
				if (dtTbl == null) return (null);
				if (dtTbl.Rows.Count == 0) return (null);

				// Return row
				return (dtTbl.Rows[0]);
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, "strCommand", strCommand);
				return null;
			}
		}
		#endregion

		#region DSUpdate (MDB)
		public bool DSUpdate (string strCommand, OleDbDataAdapter Adapter, string strTable, string Column, Int32 iRow, string Value)
		{
			return (m_Connector.DBRemote.DSUpdate (m_DataName, strCommand, Adapter, strTable, Column, iRow, Value, m_Credentials));
		}
		#endregion

		#region DSGet (XML)
		public DataSet DSGet_XML ()
		{
			return (m_Connector.DBRemote.DSGet_XML (m_DataName, m_Credentials));
		}
		#endregion

		#region DSGetTable (XML)
		public DataTable DSGetTable_XML (string strTable)
		{
			try
			{
				// Fill the dataset using the existing connection
				DataSet dsSet = m_Connector.DBRemote.DSGet_XML (m_DataName, m_Credentials);

				// Any dataset or tables?
				if (dsSet == null) return (null);

				// Note: Empty (non-null) table may be normal on a fresh install
				if (dsSet.Tables.Count == 0) return (null);

				// Return table
				return (dsSet.Tables[strTable]);
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, "strTable", strTable);
				return null;
			}
		}
		#endregion

		#region DSGetRow (XML)
		public DataRow DSGetRow_XML (string strTable)
		{
			try
			{
				// Get the dataset tables using the existing connection
				DataTable dtTbl =  DSGetTable_XML (strTable);

				// Any tables or rows?
				if (dtTbl == null) return (null);
				if (dtTbl.Rows.Count == 0) return (null);

				// Return row
				return (dtTbl.Rows[0]);
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, "strTable", strTable);
				return null;
			}
		}
		#endregion

		#region DSMerge (XML)
		public bool DSMerge_XML (DataTable dtTbl)
		{
			return (m_Connector.DBRemote.DSMerge_XML (m_DataName, dtTbl, m_Credentials));
		}
		#endregion

		#region DSUpdate (XML)
		public bool DSUpdate_XML (string strTable, string Column, Int32 iRow, string Value)
		{
			return (m_Connector.DBRemote.DSUpdate_XML (m_DataName, strTable, Column, iRow, Value, m_Credentials));
		}
		#endregion

		#region DSCommit (XML)
		public bool DSCommit_XML ()
		{
			return (m_Connector.DBRemote.DSCommit_XML (m_DataName, m_Credentials));
		}
		#endregion

		#region DBVersionFormat (Local, Internal)
		internal static string DBVersionFormat_MDB(string ConnString)
		{
			// Retrieve TekGuard database version format identifier; catch exceptions
			try
			{
				// Open the database connection
				OleDbConnection OleDBConn = new OleDbConnection(ConnString);

				// Get the application settings table
				OleDbDataAdapter adSettings = new OleDbDataAdapter ("SELECT * FROM " + QRY_SETTINGS, OleDBConn);

				// Fill the dataset and get the version format entry
				DataSet dsSet = new DataSet();
				adSettings.Fill (dsSet);
				
				// Any dataset or tables?
				if (dsSet == null) return (null);

				// Return the version format string and release all connections
				string VersionFormat = (dsSet.Tables[0].Rows[0][COL_VERSIONFORMAT]).ToString().Trim();

				// Success
				// ErrorText = null;
				return (VersionFormat);
			}
			catch(Exception ex)
			{
				string ErrorText = ex.Message;
				return (null);
			}
		}

		internal static string DBVersionFormat_XML(string ConfigFile)
		{
			// Retrieve TekGuard database version format identifier; catch exceptions
			try
			{
				// Get bootstrap initialization settings
				DataSet dsSet = new DataSet();
				dsSet.ReadXml(ConfigFile);

				// Any dataset or tables?
				if (dsSet == null) return (null);

				// Return the version format string and release all connections
				string VersionFormat = (dsSet.Tables[0].Rows[0][COL_VERSIONFORMAT]).ToString().Trim();
			
				// Success
				// ErrorText = null;
				return (VersionFormat);
			}
			catch(Exception ex)
			{
				string ErrorText = ex.Message;
				return (null);
			}
		}
		#endregion

		#region DBVersionFormat (Remote)
		public string DBVersionFormat ()
		{
			// Return database format version based on the database type
			if (m_Connector.DBRemote.DBDataType(m_DataName, m_Credentials) == DBConnEntry.FILE_TYPE_MDB) return (DBVersionFormat_MDB());
			if (m_Connector.DBRemote.DBDataType(m_DataName, m_Credentials) == DBConnEntry.FILE_TYPE_XML) return (DBVersionFormat_XML());
			return (null);
		}
		
		private string DBVersionFormat_MDB ()
		{
			try
			{
				// Load the application settings
				DataRow drRow = DSSelectRow ("SELECT * FROM " + QRY_SETTINGS);

				// Is this version OK?
				return (Convert.ToString(drRow[COL_VERSIONFORMAT]));
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, null, null);
				return (null);
			}
		}
		private string DBVersionFormat_XML ()
		{
			try
			{
				// Load the application settings
				DataRow drRow = DSGetRow_XML (TBL_INITIALIZE);

				// Is this version OK?
				return (Convert.ToString(drRow[COL_VERSIONFORMAT]));
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, null, null);
				return (null);
			}
		}
		#endregion

		#region Analyzer Remoting Functions
		
		#region ConfigurationApply
		public object ConfigurationApply(object sender, object args) 
		{
			// Fire the remote event
			return (m_Connector.DBRemote.ConfigurationApply(sender, args, m_Credentials));
		}
		#endregion

		#region AnalyzerRebuildItemize
		public object AnalyzerRebuildItemize(object sender, object args) 
		{
			// Fire the remote event
			return (m_Connector.DBRemote.AnalyzerRebuildItemize(sender, args, m_Credentials));
		}
		#endregion

		#region AnalyzerRebuildExecute
		public object AnalyzerRebuildExecute(object sender, object args) 
		{
			// Fire the remote event
			return (m_Connector.DBRemote.AnalyzerRebuildExecute(sender, args, m_Credentials));
		}
		#endregion

		#region AnalyzerStatusEvent Add/Remove
		public void AnalyzerStatusEventAdd (WinFormInvokeHandler WinInvokeFunction)
		{
			// Create event handler for server events
			m_EventClass = new AnalyzerStatusHandlerClass(new WinFormInvokeHandler(WinInvokeFunction));
			m_Connector.DBRemote.AnalyzerStatusEventAdd (new AnalyzerStatusEventHandler(m_EventClass.RemoteEventCallback), m_Credentials);
		}

		public void AnalyzerStatusEventRemove ()
		{
			m_Connector.DBRemote.AnalyzerStatusEventRemove (new AnalyzerStatusEventHandler(m_EventClass.RemoteEventCallback), m_Credentials);
		}
		#endregion
		
		#endregion

	}

	#region AnalyzerStatusHandlerClass
	// Client implementation of the event handler class
	internal class AnalyzerStatusHandlerClass : RemoteEventHandlerClass
	{
		// Class members
		WinFormInvokeHandler				m_WinInvokeFunction;

		#region Constructors
		public AnalyzerStatusHandlerClass (WinFormInvokeHandler WinInvokeFunction)
		{
			// Save windows Invoke function information
			m_WinInvokeFunction = WinInvokeFunction;
		}
		#endregion

		#region InternalEventCallback
		protected override bool InternalEventCallback (object sender, object ServerArgs)
		{
			try
			{
				// Call the component using the parent's thread
				object[] objArray = new object[1];
				objArray[0] = new object[] {ServerArgs};
				m_WinInvokeFunction(objArray);
			}
			catch(Exception ex)
			{
				ex = ex;
			}

			// Success
			return (true);
		}
		#endregion

		#region Authenticate
		protected override bool Authenticate (Credentials Credentials)
		{
			// Check remote authentication
			// if (!m_Connector.RMServer.Authenticate(Credentials)) return (null);

			return (true);
		}
		#endregion

	}
	#endregion

}
