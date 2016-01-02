using System;
using System.Data;
using System.IO;
using System.Text;

namespace TGPConnector
{
	/// <summary>
	/// Summary description for MsgTokens
	/// </summary>
	public class MsgTokens
	{
		// Class members
		private		Connector			m_Connector;					// Pointer to parent instance
		private		DataTable			m_TokenTable;
		private		DBConnEntry			m_TokenConnEntry;
		private		bool				m_PersistIsDirty	= false;	// Indicates that Token Table must be persisted
		private		bool				m_BackupIsDirty		= false;	// Indicates Token Table != Persisted version
		private		DateTime			m_LastPersist;
		private		DateTime			m_LastBackup;

		// File and Version Constants
		private const string			FIL_TOKEN_BIN		= ".bin";
		private const string			TBL_TOKENINIT		= "tblApplicationSettings";
		private	const string			COL_VERSIONFORMAT	= "VersionFormat";
		private	const string			TOK_VERSION			= "0.7.9";

		// Database table constants
		private const string			TBL_TOKENS			= "tblTokens";
		private const string			COL_TOKEN			= "Token";
		private const string			COL_CATEGORY		= "C_";
		private const string			COL_DATELAST		= "DateLast";
		private const int				READY_COUNT			= 5000;			// Require 5000 tokens / category

		#region Constructors
		public MsgTokens(Connector Connector, DBConnEntry TokenConnEntry)
		{
			// Save pointer to parent procedure
			m_Connector = Connector;

			// Save the Token List path information
			m_TokenConnEntry = TokenConnEntry;
		}
		#endregion

		#region Initialize
		public bool Initialize ()
		{
			// int CATEGORYACTIVE = 2;

			// Create the memory resident token table
			// m_MsgTokens = new MsgTokens(CATEGORYACTIVE);

			// Load the Token table from the binary file (fastest load)
			if (File.Exists(m_TokenConnEntry.FileFullPathBinary) && (new FileInfo(m_TokenConnEntry.FileFullPathBinary)).Length > 0)
			{
				try
				{
					// Deserialize from disk
					m_TokenTable = TableSerializer.BinaryDeserialize(m_TokenConnEntry.FileFullPathBinary);		

					// Create the underlying DataSet
					// if (m_MsgTokens.DataSet == null) 
					// {
					// 	//AJM - this is a weird way to get it back into a normal dataset form, but it is faster
					// 	DataSet dsSet = new DataSet();
					// 	dsSet.ReadXml(m_TokenFilePath + FIL_TOKEN + FIL_TOKEN_NEW, System.Data.XmlReadMode.ReadSchema);
					// 	dsSet.Merge(m_MsgTokens);
					// 	m_MsgTokens = dsSet.Tables[TBL_TOKENS];
					// }

					// This is the slow native .Net approach, takes about 24 seconds for 4 Meg
					// FileStream fsRead = File.Open(TokenFile, FileMode.Open, FileAccess.Read); 
					// BinaryFormatter bf = new BinaryFormatter(); 
					// m_dtMsgTokens = (DataTable) bf.Deserialize(fsRead);
					// fsRead.Close();

					// Success?
					if (m_TokenTable != null)
					{
						// Check version
						if (m_TokenTable.TableName == TBL_TOKENS)
						{
							// Indicate that Token Table equals persisted binary
							m_PersistIsDirty = false;

							// Success
							return (true);
						}
					}
				}
				catch(Exception ex)
				{
					m_Connector.FireLogException(ex, "FileFullPathBinary", m_TokenConnEntry.FileFullPathBinary);
				}
			}

			// Load the Token table from the XML file (binary corrupt or not available)
			try
			{
				// Read any available data, pass Adapter for update capability
				// OleDbDataAdapter Adapter = m_Analyzer.ADCreate();
	
				// Select existing token table
				// Note: No tokens may be a normal condition (ex: fresh install)
				// DataTable dtTbl = m_Analyzer.DSSelectTable(DBConn, SQL, Adapter);
				// if (dtTbl == null) return (true);

				// Use "backup" copy or original new file
				string TokenFullName = m_TokenConnEntry.FileFullPath;
				if (!File.Exists(TokenFullName)) TokenFullName = m_TokenConnEntry.FileNewFullPath;

				// Read the dataset with schema from the XML
				DataSet dsSet = new DataSet();
				dsSet.ReadXml(TokenFullName, System.Data.XmlReadMode.ReadSchema);

				// Read data based on version (future release)
				DataTable dtTbl = dsSet.Tables[TBL_TOKENINIT];
				if (dtTbl == null) return (false);
				DataRow drRow	= dtTbl.Rows[0];
				if ((string)drRow[COL_VERSIONFORMAT] != TOK_VERSION) 
				{
					dsSet.ReadXml(m_TokenConnEntry.FileNewFullPath, System.Data.XmlReadMode.ReadSchema);
				}
				
				m_TokenTable = dsSet.Tables[TBL_TOKENS];
				if (m_TokenTable == null) return (true);
				
				// set Primary Key
				m_TokenTable.Columns[COL_TOKEN].Unique = true;
				m_TokenTable.PrimaryKey = new DataColumn[] { m_TokenTable.Columns[COL_TOKEN] };

				// for (int ii=0; ii<dtTbl.Rows.Count; ii++)
				// {
				// 	DataRow drRow = dtTbl.Rows[ii];
				// 	TokenEntry TokenEntry = new TokenEntry(m_MsgTokens.CategoryMax, SafeDateTime(drRow[COL_DATERECEIVED]));
				// }

				// Calculate the Category row names
				// string[] ColCategory = new string[m_MsgTokens.CategoryMax];
				// for (int ii=0; ii<m_MsgTokens.CategoryMax; ii++)
				// {
				// 	ColCategory[ii] = COL_CATEGORY + IDFormat (ii, TBL_CATEGORYIDWID);
				// }

				// Update the memory resident token table
				// foreach (DataRow drRow in dtTbl.Rows)
				// {
				// 	TokenEntry TokenEntry = new TokenEntry(m_MsgTokens.CategoryMax, SafeDateTime(drRow[COL_DATERECEIVED]));
				// 	for (int ii=0; ii<m_MsgTokens.CategoryMax; ii++)
				// 	{
				// 		// Update all the Category columns
				// 		TokenEntry[ii] = SafeInt32(drRow[ColCategory[ii]]);
				// 	}
				// Add this entry
				// 	m_MsgTokens.Add (drRow[COL_TOKEN], TokenEntry);
				
				// }

				// Close the "expensive" datatable object
				// dtTbl = null;

				// Success
				if (m_TokenTable != null) 
				{
					// Indicate that Token Table does not equal persisted binary
					m_PersistIsDirty = true;

					// Success
					return (true);
				}
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, "FileFullPath", m_TokenConnEntry.FileFullPath);
			}
			
			// Load failed
			return false;
		}
		#endregion

		#region Ready
		public double Ready ()
		{
			return (Ready(m_TokenTable));
		}
		private double Ready (DataTable CheckTable)
		{
			long Percent = 0;
			long ColCount = 0;

			try 
			{			
				// Is there at least one entry for every column?
				foreach(DataColumn col in CheckTable.Columns)
				{
					// Check all the integer fields
					if (col.ColumnName.StartsWith (COL_CATEGORY))
					{
						// This is a valid column
						ColCount++;
						DataRow[] drRow = CheckTable.Select(col.ColumnName + " > 0");
						
						// Anything?
						Percent += (Math.Min (drRow.Length, READY_COUNT) * 100) / READY_COUNT;
					}
				}
			}
			catch {}

			// Determine if database is ready for analysis
			return ((ColCount > 0) ? (Percent / (100.0 * ColCount)) : 0);
		}
		#endregion

		#region Reset
		public void Reset ()
		{
			try
			{
				// Clear table contents
				m_TokenTable.Clear();

				// Indicate that Token Table does not equal persisted binary
				m_PersistIsDirty = true;
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, null, null);
			}
		}
		#endregion

		#region UpdateToken
		public bool UpdateToken(string Token, string ColCategory, DateTime LastReceived, Int32 Frequency)
		{
			// Set up the DataSet Filter
			StringBuilder Filter = new StringBuilder();
			Filter.AppendFormat ("[{0}]='{1}'", COL_TOKEN, Token);

			try
			{
				// Select this token
				DataRow[] drRow = m_TokenTable.Select(Filter.ToString());

				// Record already present?
				if (drRow.Length != 0) 
				{
					// Update the token fields
					drRow[0][ColCategory] = SafeInt32(drRow[0][ColCategory]) + Frequency;
					drRow[0][COL_DATELAST] = LastReceived.ToShortDateString();
				}
				else
				{
					DataRow drNew = m_TokenTable.NewRow();
					
					drNew[COL_TOKEN]	= Token;
					drNew[ColCategory]	= Frequency;
					drNew[COL_DATELAST]	= LastReceived.ToShortDateString();
				
					// Update Row
					m_TokenTable.Rows.Add(drNew);
				}
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, "Token", Token);
				return false;
			}

			// Indicate that Token Table does not equal persisted binary
			m_PersistIsDirty = true;

			// Return Success
			return (true);
		}
		#endregion

		#region FindToken
		public TokenEntry FindToken(string[] ColCategory, string Token)
		{
			// Set up the DataSet Filter
			StringBuilder Filter = new StringBuilder();
			Filter.AppendFormat ("[{0}]='{1}'", COL_TOKEN, Token);

			try
			{
				// Select this token
				DataRow[] drRow = m_TokenTable.Select(Filter.ToString());

				// Record already present?
				if (drRow.Length != 0) 
				{
					// Return the token fields
					TokenEntry TokenUpdate = new TokenEntry (ColCategory.Length, SafeDateTime(drRow[0][COL_DATELAST]));
					for (int ii=0; ii<ColCategory.Length; ii++)
					{
						TokenUpdate[ii] = SafeInt32(drRow[0][ColCategory[ii]]);
					}					
					return (TokenUpdate);
				}
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, "Token", Token);
				return null;
			}

			// Token not found
			return null;
		}
		#endregion

		#region Persist
		public bool Persist (Int32 DayKeepCount)
		{
			// Thread-safe, lock the shared resource (== Monitor.Enter)
			lock (m_TokenTable) try 
			{
				// Purge any tokens that are more than x days old
				Persist_Purge(DayKeepCount);

				// Has readiness status changed?
				
				// Serialize to the binary file name
				TableSerializer.BinarySerialize(m_TokenTable, m_TokenConnEntry.FileFullPathBinary);		

				// This is for testing purposes
				// Persist_Text (m_TokenConnEntry.FileFullPathBinary + ".txt");

				// This is the slow native .Net approach, takes about 24 seconds for 4 Meg
				// FileStream fsWrite = File.Open(TokenFile, FileMode.Create, FileAccess.Write); 
				// BinaryFormatter bf = new BinaryFormatter(); 
				// bf.Serialize (fsWrite, m_dtMsgTokens); 
				// fsWrite.Close(); 

				// Indicate that Token Table has been persisted
				// Token Table equals persisted version, backup does not equal persisted binary
				m_LastPersist = DateTime.Now;
				m_PersistIsDirty = false;
				m_BackupIsDirty = true;

				// Success				
				return true;
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, "FileFullPathBinary", m_TokenConnEntry.FileFullPathBinary);
			}

			// Persist did not occur
			return false;
		}

		#region Persist_Purge
		private void Persist_Purge (Int32 DayKeepCount)
		{
			// Anything to do?
			if (DayKeepCount <= 0) return;
			
			try 
			{
				// Query for the number of distinct token accessed days, sort by descending date
				DataSetHelper dsHelper = new DataSetHelper();
				DataTable dtDistinctDate = dsHelper.SelectDistinct(COL_DATELAST, m_TokenTable, COL_DATELAST, true);

				// DataRow[] drRow = dtDistinctDate.Select("", COL_DATELAST + " DESC");
				// if (drRow.Length <= KeepDays) return;
				// DateTime DateEnd = Convert.ToDateTime(drRow[KeepDays-1][COL_DATELAST]);

				// Any days to delete?
				if (dtDistinctDate.Rows.Count <= DayKeepCount) return;

				// What was original status?
				double Readiness = Ready(m_TokenTable);
				
				// Select most recently used tokens
				DateTime DateEnd = Convert.ToDateTime(dtDistinctDate.Rows[DayKeepCount-1][COL_DATELAST]);

				// Re-query table for only the most recent days
				DataTable PurgedTable = dsHelper.SelectInto(m_TokenTable.TableName, m_TokenTable, "", COL_DATELAST + " > " + "'" + DateEnd.ToShortDateString() + "'" , COL_DATELAST);
				
				// Use new table?
				if (Ready(PurgedTable) >= Readiness) m_TokenTable = PurgedTable;
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, "FileFullPathBinary", m_TokenConnEntry.FileFullPathBinary);
			}
		}
		#endregion

		#region Persist_Text
		private void Persist_Text (string sFileName)
		{
			try 
			{
				// Open the specified event log file
				FileStream fs = new FileStream(sFileName, FileMode.OpenOrCreate, FileAccess.Write);

				// Create a writer, position the file pointer to the end
				StreamWriter w = new StreamWriter(fs);		
				w.BaseStream.Seek(0, SeekOrigin.End);

				// Write the token information in comma delimited form
				for (int ii=0; ii<m_TokenTable.Rows.Count; ii++)
				{
					DataRow drRow = m_TokenTable.Rows[ii];
					w.WriteLine(drRow[COL_TOKEN] + "," + ("" + drRow["C_1"]) + "," + ("" + drRow["C_2"]) + "," + Convert.ToDateTime(drRow[COL_DATELAST]).ToShortDateString());
				}

				w.Flush();
				w.Close();
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, "FileFullPathBinary", m_TokenConnEntry.FileFullPathBinary);
			}
		}
		#endregion

		#endregion

		#region Backup
		public bool Backup ()
		{
			try
			{
				// Read any available data, pass Adapter for update capability
				// OleDbDataAdapter Adapter = m_Analyzer.ADCreate();
	
				// Select
				// DataTable dtTbl = m_Analyzer.DSSelectTable(DBConn, SQL, Adapter);

				// Create the underlying DataSet
				if (m_TokenTable.DataSet == null) 
				{
					//AJM - this is a weird way to get it back into a normal dataset form, but it is faster
					DataSet dsSet = new DataSet();
					dsSet.ReadXml(m_TokenConnEntry.FileNewFullPath, System.Data.XmlReadMode.ReadSchema);
					dsSet.Tables[TBL_TOKENS].Clear();
					dsSet.Merge(m_TokenTable);
					m_TokenTable = dsSet.Tables[TBL_TOKENS];
				}

				// DataSet dsSet = new DataSet();
				// dsSet.ReadXml(PlugInPath + TOKEN_FILE, System.Data.XmlReadMode.ReadSchema);
				// DataTable dtTbl = dsSet.Tables[0];

				// foreach (DictionaryEntry item in m_MsgTokens)
				// {
				// 	UpdateToken(dtTbl, item.Key.ToString(), ColCategory, ((TokenEntry) item.Value).LastReceived, ((TokenEntry) item.Value)[CatIndex]);
				// }

				// Write the data back to the database
				// Adapter.Update(dtTbl);

				// Write the token table in XML format
				m_TokenTable.DataSet.WriteXml(m_TokenConnEntry.FileFullPath, System.Data.XmlWriteMode.WriteSchema);
				
				// Close the "expensive" datatable object
				// dtTbl = null;

				// Backup now equals persisted binary
				m_LastBackup = DateTime.Now;
				m_BackupIsDirty = false;

				// Success
				return true;
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, "FileFullPath", m_TokenConnEntry.FileFullPath);
			}

			// Backup did not occur
			return false;
		}
		#endregion

		#region Properties

		#region xTokenTable
		private DataTable xTokenTable 
		{
			get {return m_TokenTable;}
		}
		#endregion

		#region PersistIsDirty
		public bool PersistIsDirty 
		{
			get {return m_PersistIsDirty;}
		}
		#endregion

		#region BackupIsDirty
		public bool BackupIsDirty 
		{
			get {return m_BackupIsDirty;}
		}
		#endregion

		#region LastPersist
		public DateTime LastPersist 
		{
			get {return m_LastPersist;}
		}
		#endregion

		#region LastBackup
		public DateTime LastBackup 
		{
			get {return m_LastBackup;}
		}
		#endregion

		#endregion
		
		#region SafeInt32
		private Int32 SafeInt32 (object value)
		{
			if (value is Int32) return ((Int32) value);
			return (0);
		}
		#endregion

		#region SafeDateTime
		private DateTime SafeDateTime (object value)
		{
			if (value is DateTime) return ((DateTime) value);
			return (DateTime.Now);
		}
		#endregion

	}

}
