using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging ;
using System.Text;
using System.Threading;

namespace TGMConnector
{
	/// <summary>
	/// Summary for Database Connection Class
	/// </summary>
	public class DBRemote : MarshalByRefObject
	{
		// Static members for logging & connection information
		internal static Connector			m_Connector			= null;		// Pointer to parent TGMConnector object			
		internal static DBConnList			m_DBConnList		= null;		// List of available database connections

		// Analyzer callback static members
		private	static	AnalyzerStatusEventHandler	m_OnAnalyzerStatus;		// Event handler to share events across instances

		// Message constants
		private const string		MSG_ENTRYERRORS		= "Sorry, some of the data fields are invalid. Please check again.";

		#region Constructors

		// Note: Remoted object constructor is called once for the first call
		// to System.Activator.GetObject by DBQuery independent of the number
		// of instances activated or remoted by RegisterWellKnownServiceType
		internal DBRemote ()
		{
			try
			{
				// Open all of the databases handled by this object
				// foreach (DBConnEntry DBEntry in m_DBConnList) // Hashtable return IDictionary, no easy overrride
				foreach(DictionaryEntry dEntry in m_DBConnList) 
				{
					try
					{
						// Retrieve Connection Entry object
						DBConnEntry DBConnEntry = ((DBConnEntry)dEntry.Value);
						
						// OleDB disconnected dataset
						if (DBConnEntry.DataType == DBConnEntry.FILE_TYPE_MDB) 
						{
							// Get the database connection string associated with
							// this DataName and open the database connection
							DBConnEntry.OleDbConn = new OleDbConnection(DBConnEntry.ConnString);
						}

						// XML in-memory dataset
						if (DBConnEntry.DataType == DBConnEntry.FILE_TYPE_XML) 
						{
							// Fill the dataset using the existing connection
							DBConnEntry.DataSet = new DataSet();
							DBConnEntry.DataSet.ReadXml(DBConnEntry.FileFullPath);
						}
					}
					catch(Exception ex)
					{
						m_Connector.FireLogException(ex, "dEntry.Key", dEntry.Key.ToString());
						throw new Exception(ex.Message);
					}
				}
			}
			catch(Exception ex)
			{
				// Log error and throw exception on failure
				m_Connector.FireLogException(ex, null, null);
				throw new Exception(ex.Message);
			}
		}

		~DBRemote ()
		{
			// Close all of the databases handled by this object
			// foreach (DBConnEntry DBEntry in m_DBConnList) // Hashtable return IDictionary, no easy overrride
			foreach(DictionaryEntry dEntry in m_DBConnList) 
			{
				try
				{
					// Close the "expensive" connection object
					if (((DBConnEntry)dEntry.Value).OleDbConn != null) 
					{
						((DBConnEntry)dEntry.Value).OleDbConn.Close();
						((DBConnEntry)dEntry.Value).OleDbConn = null;
					}
				}
				catch(Exception ex)
				{
					m_Connector.FireLogException(ex, null, null);
				}
			}
		}
		#endregion

		#region InitializeLifetimeService
		// If we want remoted object to live more than 5 minutes
		// public override object InitializeLifetimeService()
		// {
		// 	ILease lease = (ILease)base.InitializeLifetimeService();
		// 	if (lease.CurrentState == LeaseState.Initial )
		// 	{
		//		// InitialLeaseTime  The time of a lease. The object will be ready for garbage collection after this amount of time. Setting this property to null gives an infinite lease time.
		//		// RenewOnCallTime  Every call to the object will increase the lease time by this amount.
		//		// SponsorshipTimeout  When the lease has expired, the lease will contact any registered sponsors. The sponsor then has the opportunity of extending the lease. The SponsorshipTimeout value is the amount of time that the object will wait for a response from the sponsor. The sponsor class will be introduced shortly in the client-side code.
		// 		lease.InitialLeaseTime = TimeSpan.FromSeconds(4);
		// 		lease.RenewOnCallTime = TimeSpan.FromSeconds(3);
		// 		lease.SponsorshipTimeout = TimeSpan.FromMinutes(3);
		// 	} return lease;
		// }

		// If we want remoted object to live forever
		public override object InitializeLifetimeService()
		{
			return null;
		}
		#endregion

		#region DBCommand (private)
		private OleDbCommand DBCommand (string DataName, string Command)
		{
			// Valid request?
			if (m_DBConnList.OleDbConn(DataName) == null) return (null);

			// AJM 3/11/04 - Locking at this level seems to cause access conflicts
			// Serialize access to the database connection object AND Adapter SelectCommand
			// lock (m_DBConnList.OleDbConn(DataName))try
			try
			{
				// Fill the adapter data channel
				return (new OleDbCommand(Command, m_DBConnList.OleDbConn(DataName)));
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, "Command", Command);
				return (null);
			}
		}
		#endregion

		#region Properties (Public)

		#region RemoteURL
		/// <summary>
		/// RemoteURL Summary
		/// </summary>
		public String RemoteURL 
		{
			get {
				// Get the remoted objects URL
				String Address = null;
				try
				{
					Address = RemotingIDHelper.GetURLForObject(this);
				}
				catch(Exception ex)
				{
					m_Connector.FireLogException(ex, null, null);
				}
				return (Address != null ? Address : "localhost");
			}
		}
		#endregion

		#endregion

		#region Methods (Public)

		#region DBAuthenticate
		public bool DBAuthenticate(Credentials Credentials)
		{
			// Check remote authentication
			return (m_Connector.RMServer.Authenticate(Credentials));
		}
		#endregion

		#region DBDataType
		public string DBDataType (string DataName, Credentials Credentials)
		{
			// Check remote authentication
			if (!m_Connector.RMServer.Authenticate(Credentials)) return (null);

			// Return database format version based on the database type
			return (m_DBConnList.DataType(DataName));
		}
		#endregion

		#region Enabled
		public bool Enabled (string DataName, Credentials Credentials)
		{
			// Check remote authentication
			if (!m_Connector.RMServer.Authenticate(Credentials)) return (false);

			try
			{
				// Get the connection entry
				DBConnEntry DBConnEntry = m_DBConnList[DataName];
				if (DBConnEntry == null) return (false);

				// Check for entries based on data type
				if (DBConnEntry.DataType == DBConnEntry.FILE_TYPE_MDB) return (DBConnEntry.OleDbConn != null);
				if (DBConnEntry.DataType == DBConnEntry.FILE_TYPE_XML) return (DBConnEntry.FileFullPath != null);
				return (false);			
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, "DataName", DataName);
				return (false);
			}
		}
		#endregion

		#region ADCreate
		public OleDbDataAdapter ADCreate (Credentials Credentials)
		{
			// Check remote authentication
			if (!m_Connector.RMServer.Authenticate(Credentials)) return (null);

			// Note: The DataAdapter was not designed to submit changes stored
			// in multiple DataTables in a single call.  You'll need to use 
			// separate DataAdapters for each DataTable.
			try
			{
				// Create the adapter
				OleDbDataAdapter Adapter = new OleDbDataAdapter();
				// Request schemas to be added if keys are missing
				Adapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
				// Create default commands for append, update, delete
				OleDbCommandBuilder cbCmd = new OleDbCommandBuilder(Adapter);
				return (Adapter);
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, null, null);
				return (null);
			}
		}
		#endregion

		#region ADCreateCommand
		public OleDbCommand ADCreateCommand (string DataName, OleDbDataAdapter Adapter, string cmdText, Credentials Credentials)
		{
			// Check remote authentication
			if (!m_Connector.RMServer.Authenticate(Credentials)) return (null);

			try
			{
				// Create the adapter command
				return (new OleDbCommand(cmdText, m_DBConnList.OleDbConn(DataName)));
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, "DataName", DataName);
				return (null);
			}
		}
		#endregion

		#region ADSelect
		public bool ADSelect (string DataName, OleDbDataAdapter Adapter, ref DataSet dsSet, string Select, string TableName, Credentials Credentials)
		{
			// Check remote authentication
			if (!m_Connector.RMServer.Authenticate(Credentials)) return (false);

			// Valid request?
			if ((m_DBConnList.OleDbConn(DataName) == null) || (Adapter == null) || (dsSet == null)  || (Select.Length == 0)  || (TableName.Length == 0)) return (false);
				
			// Serialize access to the database connection object AND Adapter SelectCommand
			lock (m_DBConnList.OleDbConn(DataName)) try
			{
				// What table number is this?
				int TableCount = dsSet.Tables.Count;

				// Fill the adapter data channel
				Adapter.SelectCommand = DBCommand(DataName, Select);

				// Fill the dataset, give it a name
				Adapter.Fill(dsSet, TableName);

				// Anything new?
				if (TableCount == dsSet.Tables.Count) return (false);
				if (dsSet.Tables[TableCount].Rows.Count == 0) return (false);

				// Tell the adapter the table name
				string adName = "Table" + ((TableCount == 0) ? "" : (TableCount + 1).ToString());
				Adapter.TableMappings.Add(adName, TableName);

				// Create default commands for append, update, delete
				// if (TableCount == 0) {
				//	 OleDbCommandBuilder cbCmd = new OleDbCommandBuilder(Adapter);
				//	 string updSQL = cbCmd.GetUpdateCommand().CommandText;
				//	 string delSQL = cbCmd.GetDeleteCommand().CommandText;
				//	 string insSQL = cbCmd.GetInsertCommand().CommandText;
				//	 Adapter.UpdateCommand = new OleDbCommand(updSQL);
				//	 Adapter.DeleteCommand = new OleDbCommand(delSQL);
				//	 Adapter.InsertCommand = new OleDbCommand(insSQL);
				// }

				// Success
				return (true);
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, "Select", Select);
				return (false);
			}
		}
		#endregion

		#region DSSelect (MDB)
		public DataSet DSSelect (string DataName, string strCommand, OleDbDataAdapter Adapter, Credentials Credentials)
		{
			// Check remote authentication
			if (!m_Connector.RMServer.Authenticate(Credentials)) return (null);

			// Valid request?
			if ((m_DBConnList.OleDbConn(DataName) == null) || (strCommand.Length == 0)) return (null);

			// Serialize access to the database connection object AND Adapter SelectCommand
			lock (m_DBConnList.OleDbConn(DataName)) try
			{
				// Adapter passed for update (otherwise, no database update allowed)?
				if (Adapter == null) Adapter = new OleDbDataAdapter();

				// Initialize the database connection parameters
				Adapter.SelectCommand = DBCommand(DataName, strCommand);

				// Fill the dataset using the existing connection
				DataSet dsSet = new DataSet();
				Adapter.Fill(dsSet);

				// Return success
				return (dsSet);
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, "strCommand", strCommand);
				return null;
			}
		}
		#endregion

		#region DSUpdate (MDB)
		// AJM 10/19/04: I tried every possible combination of calls to get
		//   the databound textbox to update the underlying dataset when remoted.
		//   Works fine when part of the same process.
		// DataSet dsTemp = (DataSet) edtTokenCount.DataBindings["Text"].DataSource;
		// DataRow drRow = dsTemp.Tables[QRY_ANALYZER].Rows[0];
		// string sTemp = drRow[COL_TOKENCOUNT].ToString();
		// DataSet dsNew = dsTemp.GetChanges(System.Data.DataRowState.Modified);
		// bool bTemp = dsTemp.HasChanges();
		public bool DSUpdate (string DataName, string strCommand, OleDbDataAdapter Adapter, string strTable, string Column, Int32 iRow, string Value, Credentials Credentials)
		{
			// Check remote authentication
			if (!m_Connector.RMServer.Authenticate(Credentials)) return (false);

			// Valid request?
			if ((m_DBConnList.OleDbConn(DataName) == null) || (Adapter == null)) return (false);
			if ((strCommand.Length == 0) || (strTable.Length == 0) || (Column.Length == 0)) return (false);

			// Serialize access to the database connection object AND Adapter SelectCommand
			lock (m_DBConnList.OleDbConn(DataName)) try
			{
				// Get the dataset
				DataSet dsSet = DSSelect (DataName, strCommand, Adapter, Credentials);
				if (dsSet == null) return (false);

				// Get the datarow entry
				DataRow drRow = dsSet.Tables[strTable].Rows[iRow];
				if (drRow == null) return (false);

				// Update specified column value
				drRow[Column] = Value;

				// Note: AcceptChanges() causes change to be ingored when remoted										
				// drRow.AcceptChanges();

				// Any data violations?
				if (dsSet.HasErrors)
				{
					// Update failed
					m_Connector.FireLogAlert(MSG_ENTRYERRORS, "Value", Value);
					return (false);
				}

				// Write the data back to the database (named table)
				Adapter.Update(dsSet, strTable);

				// Return success
				return (true);
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, "strCommand", strCommand);
				return false;
			}
		}
		#endregion

		#region DSGet / DSUpdate / Commit (XML)

		public DataSet DSGet_XML (string DataName, Credentials Credentials)
		{
			// Check remote authentication
			if (!m_Connector.RMServer.Authenticate(Credentials)) return (null);

			// Valid request?
			if (m_DBConnList.DataSet(DataName) == null) return (null);

			// Serialize access to the database connection object
			lock (m_DBConnList.DataSet(DataName)) try
			{
				// Return dataset
				return (m_DBConnList.DataSet(DataName));
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, "FileFullPath", m_DBConnList.FileFullPath(DataName));
				return null;
			}
		}

		public bool DSUpdate_XML (string DataName, string strTable, string Column, Int32 iRow, string Value, Credentials Credentials)
		{
			// Check remote authentication
			if (!m_Connector.RMServer.Authenticate(Credentials)) return (false);

			// Valid request?
			if ((m_DBConnList.DataSet(DataName) == null) || (strTable.Length == 0) || (Column.Length == 0)) return (false);

			// Serialize access to the database connection object
			lock (m_DBConnList.DataSet(DataName)) try
			{
				// Get the dataset
				DataSet dsSet = m_DBConnList.DataSet(DataName);
				if (dsSet == null) return (false);

				// Get the datarow entry
				DataRow drRow = dsSet.Tables[strTable].Rows[iRow];
				if (drRow == null) return (false);

				// Update specified column value
				drRow[Column] = Value;
				drRow.AcceptChanges();

				// Any data violations?
				if (dsSet.HasErrors)
				{
					// Update failed
					m_Connector.FireLogAlert(MSG_ENTRYERRORS, "Value", Value);
					return (false);
				}

				// Return success
				return (true);
			}
			catch(Exception ex)
			{
				// Update failed
				m_Connector.FireLogException(ex, "FileFullPath", m_DBConnList.FileFullPath(DataName));
				return (false);
			}
		}
	
		public bool DSMerge_XML (string DataName, DataTable dtTbl, Credentials Credentials)
		{
			// Check remote authentication
			if (!m_Connector.RMServer.Authenticate(Credentials)) return (false);

			// Valid request?
			if ((m_DBConnList.DataSet(DataName) == null) || (dtTbl == null)) return (false);

			// Serialize access to the database connection object
			lock (m_DBConnList.DataSet(DataName)) try
			{
				// Get the dataset
				DataSet dsSet = m_DBConnList.DataSet(DataName);
				if (dsSet == null) return (false);

				// Merge the incoming entry
				dsSet.Merge(dtTbl);

				// Any data violations?
				if (dsSet.HasErrors)
				{
					// Update failed
					m_Connector.FireLogAlert(MSG_ENTRYERRORS, "FileFullPath", m_DBConnList.FileFullPath(DataName));
					return (false);
				}

				// Return success
				return (true);
			}
			catch(Exception ex)
			{
				// Update failed
				m_Connector.FireLogException(ex, "FileFullPath", m_DBConnList.FileFullPath(DataName));
				return (false);
			}
		}
	

		public bool DSCommit_XML (string DataName, Credentials Credentials)
		{

			// Check remote authentication
			if (!m_Connector.RMServer.Authenticate(Credentials)) return (false);

			// Valid request?
			if (m_DBConnList.DataSet(DataName) == null) return (false);

			// Serialize access to the database connection object
			lock (m_DBConnList.DataSet(DataName)) try
			{
				// Write the XML
				m_DBConnList.DataSet(DataName).WriteXml(m_DBConnList.FileFullPath(DataName), System.Data.XmlWriteMode.WriteSchema);

				// Return success
				return (true);
			}
			catch(Exception ex)
			{
				// Update failed
				m_Connector.FireLogException(ex, "FileFullPath", m_DBConnList.FileFullPath(DataName));
				return (false);
			}

		}
		#endregion

		#endregion

		#region Analyzer Remoting Functions

		#region ConfigurationApply
		public object ConfigurationApply(object sender, object args, Credentials Credentials) 
		{
			// Check remote authentication
			if (!m_Connector.RMServer.Authenticate(Credentials)) return (false);

			// Fire the remote event
			return (m_Connector.FireConfigurationApply(sender, args));
		}
		#endregion

		#region AnalyzerRebuildItemize
		public object AnalyzerRebuildItemize(object sender, object args, Credentials Credentials) 
		{
			// Check remote authentication
			if (!m_Connector.RMServer.Authenticate(Credentials)) return (false);

			// Fire the remote event
			return (m_Connector.FireAnalyzerRebuildItemize(sender, args));
		}
		#endregion

		#region AnalyzerRebuildExecute
		public object AnalyzerRebuildExecute(object sender, object args, Credentials Credentials) 
		{
			// Check remote authentication
			if (!m_Connector.RMServer.Authenticate(Credentials)) return (false);

			// Fire the remote event
			return (m_Connector.FireAnalyzerRebuildExecute(sender, args));
		}
		#endregion

		#region AnalyzerStatusUpdate
		public bool AnalyzerStatusUpdate(object sender, object args, Credentials Credentials) 
		{
			// Anyone to notify?
			if (m_OnAnalyzerStatus == null) return (false);

			// Check remote authentication
			if (!m_Connector.RMServer.Authenticate(Credentials)) return (false);

			// Get the invocation list of subscribers to be notified
			System.Delegate[] InvocationList = m_OnAnalyzerStatus.GetInvocationList();

			// loop thru the list and try and notify each one
			foreach (System.Delegate Subscriber in InvocationList) 
			{
				try 
				{
					// Cast the delegate to our type of delegate
					AnalyzerStatusEventHandler EventHandler = (AnalyzerStatusEventHandler) Subscriber;                                                

					// Call the subscriber using the callback delegate
					// AJM Note: Outlook hangs hard here when TGPlugin calls into TGPDashboard
					return (EventHandler (sender, args, Credentials));                                                
				}
				catch(Exception ex) 
				{
					// The subscriber is a dead client and will be removed from the chain
					m_OnAnalyzerStatus -= (AnalyzerStatusEventHandler) Subscriber;

					// Log the event *after* removing the offending connection to prevent an infinite logging looop
					m_Connector.FireLogException(ex, RMServer.MSG_FAILEDREMOTE, this.RemoteURL);
				}
			}
			return (false);
		}
		#endregion

		#region AnalyzerStatusEvent Add/Remove
		public void AnalyzerStatusEventAdd (AnalyzerStatusEventHandler AnalyzerStatusHandler, Credentials crRemote)
		{
			if (DBAuthenticate(crRemote))
			{
				m_OnAnalyzerStatus += AnalyzerStatusHandler;
			}
		}

		public void AnalyzerStatusEventRemove (AnalyzerStatusEventHandler AnalyzerStatusHandler, Credentials crRemote)
		{
			if (DBAuthenticate(crRemote))
			{
				m_OnAnalyzerStatus -= AnalyzerStatusHandler;
			}
		}
		#endregion
		
		#endregion
	
	}

	#region Public Remoting Delegates
	/// <summary>Analyzer Status EventHandler Summary that will be thrown to clients</summary>
	public	delegate bool AnalyzerStatusEventHandler(object sender, object ServerArgs, Credentials Credentials);
	#endregion

}
