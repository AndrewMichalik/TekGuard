using System;
using System.Collections;
using System.Data;
using System.Data.OleDb;
using System.Text;

namespace TGPConnector
{
	/// <summary>
	/// Summary for Database connection list
	/// </summary>
	public class DBConnList : Hashtable
	{
		// Public Database constants
		public const string			DBS_MODE_R			= "Read";
		public const string			DBS_MODE_RW			= "ReadWrite";

		#region Add (w/ override)
		public override void Add(object key, object value)
		{
			if ((key is string) && (value is DBConnEntry))
			{
				// Add Connection entry
				base.Add(key, value);
			}
		}
		public void Add(string DataName, string ConnTemplate, string UserID, string Password, string NewPath, string FullPath, string DataSource, string DataType, string Mode)
		{
			Add(DataName, new DBConnEntry (ConnTemplate, UserID, Password, NewPath, FullPath, DataSource, DataType, Mode));
		}
		#endregion

		#region this[]
		/// <summary>
		/// this[string] Summary
		/// </summary>
		public DBConnEntry this[string key] 
		{
			// Protect caller from missing key entries
			get {return (base.Contains(key) ? (DBConnEntry)base[key] : null);}
		}
		/// <summary>
		/// this[int] Summary
		/// </summary>
		public DBConnEntry this[int index] 
		{
			get{return ((DBConnEntry)base[index]);}
		}
		#endregion

		#region FileFullPath (safe version)
		public string FileFullPath(string DataName)
		{
			// Protect caller from missing key entries
			return (base.Contains(DataName) ? ((DBConnEntry)base[DataName]).FileFullPath : null);
		}
		#endregion
		
		#region DataSet (safe version)
		public DataSet DataSet(string DataName)
		{
			// Protect caller from missing key entries
			return (base.Contains(DataName) ? ((DBConnEntry)base[DataName]).DataSet : null);
		}
		#endregion
		
		#region OleDbConn (safe version)
		public OleDbConnection OleDbConn(string DataName)
		{
			// Protect caller from missing key entries
			return (base.Contains(DataName) ? ((DBConnEntry)base[DataName]).OleDbConn : null);
		}
		#endregion
		
		#region DataType (safe version)
		public string DataType(string DataName)
		{
			// Protect caller from missing key entries
			return (base.Contains(DataName) ? ((DBConnEntry)base[DataName]).DataType : null);
		}
		#endregion
		
	}

	#region Public Argument Structures

	#region DBConnEntry
	public class DBConnEntry 
	{
		private	string				m_ConnString;				// Connection string
		private DataSet				m_DataSet;					// Persistent dataset
		private OleDbConnection		m_OleDbConn;				// OLE Database connection
		private string				m_NewPath;					// New path name (translated from relative DataPath)
		private string				m_FullPath;					// Full path name (translated from relative DataPath)
		private string				m_DataSource;				// Database source name
		private string				m_DataType;					// Database type
		private bool				m_NewInstall = false;		// Database is the result of a new installation

		// Database file type strings
		public const string			FILE_TYPE_MDB	= ".mdb";	// Microsoft Access
		public const string			FILE_TYPE_XML	= ".xml";	// XML
		public const string			FILE_TYPE_BIN	= ".bin";	// Serialized binary
		public const string			FILE_FILE_NEW	= "_New";	// New file name

		#region Constructors
		public DBConnEntry (string ConnTemplate, string UserID, string Password, string NewPath, string FullPath, string DataSource, string DataType, string Mode)
		{
			// Save the path, file and datatype information for future reference
			m_NewPath		= NewPath.Trim();
			m_FullPath		= FullPath.Trim();
			m_DataSource	= DataSource.Trim();
			m_DataType		= DataType.Trim();

			// Build string with replacement fields
			StringBuilder FullString = new StringBuilder();
			FullString.AppendFormat (ConnTemplate.Trim(), UserID.Trim(), Password.Trim(), FileFullPath, Mode.Trim());

			// Store completed connection string (.MDB database)
			m_ConnString = FullString.ToString();
		}
		#endregion

		#region Properties

		#region VersionFormat
		/// <summary>
		/// VersionFormat Summary
		/// </summary>
		public string VersionFormat 
		{
			get 
			{
				// Return database format version based on the database type
				if (m_DataType == FILE_TYPE_MDB) return (DBQuery.DBVersionFormat_MDB(m_ConnString));
				if (m_DataType == FILE_TYPE_XML) return (DBQuery.DBVersionFormat_XML(m_ConnString));
				return (null);
			}
		}
		#endregion

		#region FileNewFullPath
		/// <summary>
		/// FileNewPath Summary
		/// </summary>
		public string FileNewFullPath 
		{
			get {return(m_NewPath + m_DataSource + FILE_FILE_NEW + m_DataType);}
		}
		#endregion

		#region FileFullPath
		/// <summary>
		/// FileFullPath Summary
		/// </summary>
		public string FileFullPath 
		{
			get {return(m_FullPath + m_DataSource + m_DataType);}
		}
		#endregion

		#region FileFullPathBackup
		/// <summary>
		/// FileFullPathBackup Summary
		/// </summary>
		public string FileFullPathBackup 
		{
			// Create the filename based on the current time.
			get {return(m_FullPath + m_DataSource + "_" + DateTime.Now.ToString("yyyy_MMdd_hhmmss") + m_DataType);}
		}
		#endregion

		#region FileFullPathBinary
		/// <summary>
		/// FileFullPathBackup Summary
		/// </summary>
		public string FileFullPathBinary 
		{
			// Create the filename for the serialized binary
			get {return(m_FullPath + m_DataSource + FILE_TYPE_BIN);}
		}
		#endregion

		#region FileFullPathBinaryBackup
		/// <summary>
		/// FileFullPathBinaryBackup Summary
		/// </summary>
		public string FileFullPathBinaryBackup 
		{
			// Create the filename for the serialized binary
			get {return(m_FullPath + m_DataSource + "_" + DateTime.Now.ToString("yyyy_MMdd_hhmmss") + FILE_TYPE_BIN);}
		}
		#endregion
		
		#region FileNameNew
		/// <summary>
		/// FileNameNew Summary
		/// </summary>
		private string FileNameNew 
		{
			// Create the filename based on the current time.
			get {return(m_DataSource + FILE_FILE_NEW + m_DataType);}
		}
		#endregion

		#region NewInstall
		/// <summary>
		/// NewInstall Summary
		/// </summary>
		public bool NewInstall 
		{
			get {return(m_NewInstall);}
			set {m_NewInstall = value;}
		}
		#endregion

		#region ConnString
		/// <summary>
		/// ConnString Summary
		/// </summary>
		public string ConnString 
		{
			get {return (m_ConnString);}
		}
		#endregion

		#region DataSet
		/// <summary>
		/// OleDbConn Summary
		/// </summary>
		public DataSet DataSet 
		{
			get {return (m_DataSet);}
			set {m_DataSet = value;}
		}
		#endregion

		#region OleDbConn
		/// <summary>
		/// OleDbConn Summary
		/// </summary>
		public OleDbConnection OleDbConn 
		{
			get {return (m_OleDbConn);}
			set {m_OleDbConn = value;}
		}
		#endregion

		#region DataType
		/// <summary>
		/// DataType Summary
		/// </summary>
		public string DataType 
		{
			get {return (m_DataType);}
		}
		#endregion

		#endregion

		#region Methods

		#endregion

	}
	#endregion

	#endregion

}
