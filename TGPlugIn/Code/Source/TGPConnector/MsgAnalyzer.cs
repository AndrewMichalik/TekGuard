using System;
using System.Collections;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Text;

namespace TGPConnector
{
	/// <summary>
	/// Summary description for MsgAnalyzer
	/// </summary>
	public class MsgAnalyzer
	{
		// Class members
		private Connector			m_Connector			= null;		// Pointer to parent TGPConfigure instance
		private	DBQuery				m_AnaQuery			= null;		// Database Analyzer query object - pooled

		// Database Application Settings constants
		private const string		QRY_SETTINGS		= "qryApplicationSettings";
		private const string		COL_TOKENCOUNT		= "TokenCount";
		private const string		COL_DAYKEEPCOUNT	= "DayKeepCount";
		private const string		COL_LOGRESULTS		= "LogAnalyzerRes";
		private const string		COL_SPLITTERREGEX	= "SplitterRegEx";

		// Database Message Category constants
		private const string		QRY_MSGCATEGORY		= "qryMessageCategory";
		private const string		COL_CATEGORYID		= "CategoryID";
		private const string		COL_CATEGORYNAME	= "CategoryName";
		private const string		COL_MAILBOXNAME		= "MailboxName";
		private const string		COL_BUTTONNAME		= "ButtonName";
		private const string		COL_THRESHOLD		= "Threshold";
		private const string		COL_AUTOMOVE		= "AutoMove";
		private const string		COL_AUTOREAD		= "AutoRead";

		// Database Mail From constants
		private const string		QRY_MAILFROM		= "qryMessageMailFrom";
		private const string		COL_MFRULEID		= "MFRuleID";
		private const string		COL_MAILNAME		= "MailName";
		private const string		COL_DOMAINNAME		= "DomainName";
		private const string		COL_LOCKED			= "Locked";
		private const string		COL_DESCRIPTION		= "Description";
		private const string		COL_CREATEDBY		= "CreatedBy";
		private const string		COL_CREATEDON		= "CreatedOn";
		private const string		COL_MODIFIEDBY		= "ModifiedBy";
		private const string		COL_MODIFIEDON		= "ModifiedOn";

		#region Constructors
		public MsgAnalyzer (Connector Connector, DBQuery AnaQuery)
		{
			// Set pointer to parent instance & Database Query object
			m_Connector = Connector;
			m_AnaQuery = AnaQuery;
		}
		#endregion

		#region Properties

		#endregion

		#region Methods

		#region CategoryListGet
		public CategoryList CategoryListGet()
		{
			CategoryList CategoryList = new CategoryList();

			// Check category associated with this UserID
			try
			{
				// Load the Analyzer application settings
				// AJM: Put in block because variable drRow used later in foreach
				{
					DataRow drRow = m_AnaQuery.DSSelectRow ("SELECT * FROM " + QRY_SETTINGS, null);
					CategoryList.TokenCount = Convert.ToInt32(drRow[COL_TOKENCOUNT]);
					CategoryList.DayKeepCount = Convert.ToInt32(drRow[COL_DAYKEEPCOUNT]);
					CategoryList.LogResults = Convert.ToBoolean(drRow[COL_LOGRESULTS]);
					CategoryList.SplitterRegEx = Convert.ToString(drRow[COL_SPLITTERREGEX]);
				}

				// Create a new dataset
				DataSet dsFrom = new DataSet();

				// Build the category options Select clause
				StringBuilder Select = new StringBuilder();
				string SQL = Select.AppendFormat ("SELECT [{0}], [{1}], [{2}], [{3}], [{4}], [{5}], [{6}] FROM {7}", COL_CATEGORYID, COL_CATEGORYNAME, COL_MAILBOXNAME, COL_BUTTONNAME, COL_THRESHOLD, COL_AUTOMOVE, COL_AUTOREAD, QRY_MSGCATEGORY).ToString();

				// Get the From address MailNames
				OleDbDataAdapter adFrom = m_AnaQuery.ADCreate();
				bool bSuccess = m_AnaQuery.ADSelect (adFrom, ref dsFrom, SQL, QRY_MSGCATEGORY);
				if (!bSuccess) return (null);

				// Return the array of category option information
				foreach (DataRow drRow in dsFrom.Tables[0].Rows)
				{
					CategoryEntry CategoryEntry = new CategoryEntry();
					CategoryEntry.CategoryID = Convert.ToInt32 (drRow[COL_CATEGORYID]);
					CategoryEntry.CategoryName = Convert.ToString (drRow[COL_CATEGORYNAME]);
					CategoryEntry.MailBoxName = Convert.ToString (drRow[COL_MAILBOXNAME]);
					CategoryEntry.Threshold = Convert.ToDouble (drRow[COL_THRESHOLD]);
					CategoryEntry.AutoMove = Convert.ToBoolean (drRow[COL_AUTOMOVE]);
					CategoryEntry.AutoRead = Convert.ToBoolean (drRow[COL_AUTOREAD]);
					CategoryList.Add (CategoryEntry);
				}

			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, null, null);
			}
			return (CategoryList);
		}
		#endregion

		#region AnalyzerMailFrom Get/Set/Reset

		public Int32 AnalyzerMailFromGet(string Address)
		{
			// Check category associated with this address
			try
			{
				// Parse the destination address
				string MailName, DomainName;
				if (!ParseTo (Address, out MailName, out DomainName)) return (0);

				// Create a new dataset
				DataSet dsFrom = new DataSet();

				// Build the Select clause
				StringBuilder Select = new StringBuilder();
				Select.AppendFormat ("SELECT [{0}], [{1}], [{2}], [{3}], [{4}], [{5}], [{6}], [{7}] FROM {8}", COL_MFRULEID, COL_CATEGORYID, COL_MAILNAME, COL_DOMAINNAME, COL_LOCKED, COL_DESCRIPTION, COL_CREATEDBY, COL_MODIFIEDBY, QRY_MAILFROM);

				// Set up the WHERE clause (end with ';')
				StringBuilder Where = new StringBuilder();
				Where.AppendFormat (" WHERE [{0}]='{1}' AND [{2}]='{3}';", COL_MAILNAME, MailName, COL_DOMAINNAME, DomainName);
				string SQL = Select.Append(Where).ToString();

				// Get the From address MailNames 
				OleDbDataAdapter adFrom = m_AnaQuery.ADCreate ();
				bool bSuccess = m_AnaQuery.ADSelect (adFrom, ref dsFrom, SQL, QRY_MAILFROM);
				if (!bSuccess) return (0);

				// More than one entry? Return unknown.
				if (dsFrom.Tables[0].Rows.Count == 1) return ((Int32) dsFrom.Tables[0].Rows[0][COL_CATEGORYID]);
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, "Address", Address);
			}
			return (0);
		}

		public bool AnalyzerMailFromSet(string Address, Int32 CategoryID)
		{
			// Check category associated with this address
			try
			{
				// Parse the destination address
				string MailName, DomainName;
				if (!ParseTo (Address, out MailName, out DomainName)) return (false);

				// The adapter cannot handle multiple tables; use base table
				// const string TBL_MAILFROM = "tblMessageMailFrom";

				// Create a new dataset
				DataSet dsFrom = new DataSet();

				// Build the Select clause
				// Note: The CategoryID is unique by user; may need to review decision in future
				StringBuilder Select = new StringBuilder();
				Select.AppendFormat ("SELECT [{0}], [{1}], [{2}], [{3}], [{4}], [{5}], [{6}], [{7}] FROM {8}", COL_MFRULEID, COL_CATEGORYID, COL_MAILNAME, COL_DOMAINNAME, COL_LOCKED, COL_DESCRIPTION, COL_CREATEDBY, COL_MODIFIEDBY, QRY_MAILFROM);

				// Sort the entries in descending entry order
				// const string QRY_SORT = "ORDER BY [ModifiedOn] DESC";

				// Set up the WHERE clause (end with ';')
				StringBuilder Where = new StringBuilder();
				Where.AppendFormat (" WHERE ([{0}]='{1}' AND [{2}]='{3}') ORDER BY [{4}] DESC;", COL_MAILNAME, MailName, COL_DOMAINNAME, DomainName, COL_MODIFIEDON);
				// Where.AppendFormat (" WHERE [{0}]='{1}' AND [{2}]='{3}';", COL_MAILNAME, MailName, COL_DOMAINNAME, DomainName);
				string SQL = Select.Append(Where).ToString();

				// Get the From address MailNames 
				OleDbDataAdapter adFrom = m_AnaQuery.ADCreate ();
				DataTable dtTbl = m_AnaQuery.DSSelectTable(SQL, adFrom);
				if (dtTbl == null) return (false);

				// Only keep the last entry
				if (dtTbl.Rows.Count != 0) 
				{
					// AJM Note: MSDN says Rows.Remove()/Rows.RemoveAt()
					// is the same as "Delete()" plus "AcceptChanges()".
					// That marks the rows as unmodified, which defeats .Update()
					for (int ii=dtTbl.Rows.Count-1; ii>0; ii--) 
					{
                        dtTbl.Rows[ii].Delete();
					}
					// Delete these rows from the database
					adFrom.Update(dtTbl);
				}

				// Record already present?
				if (dtTbl.Rows.Count != 0) 
				{
					// Get the global settings row
					DataRow drRow = dtTbl.Rows[0];

					// Is this record locked?
					if (Convert.ToBoolean(drRow[COL_LOCKED])) return (false);

					// Update the token fields
					drRow[COL_CATEGORYID]	= CategoryID;
					drRow[COL_CREATEDBY]	= 0;
					drRow[COL_MODIFIEDBY]	= 0;
				}
				else
				{
					DataRow drRow = dtTbl.NewRow();
					
					drRow[COL_CATEGORYID]	= CategoryID;
					drRow[COL_MAILNAME]		= MailName;
					drRow[COL_DOMAINNAME]	= DomainName;
					drRow[COL_LOCKED]		= false;
					drRow[COL_DESCRIPTION]	= "[System]";
					drRow[COL_CREATEDBY]	= 0;
					drRow[COL_MODIFIEDBY]	= 0;
				
					// Update Row
					dtTbl.Rows.Add(drRow);
				}

				// Write the data back to the database
				adFrom.Update(dtTbl);

			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, "Address", Address);
				return false;
			}

			return (true);
		}

		public void AnalyzerMailFromReset()
		{
		}
		#endregion

		#endregion

		#region ParseTo
		private bool ParseTo (string strTo, out string User, out string Domain)
		{
			// Anything to do (minimum length of a@b.c)?
			if ((strTo != null) && (strTo.Length >= 5)) 
			{
				// Verify the basic address format
				int Index = strTo.IndexOf('@');
				if ((Index > 0) && !(strTo.EndsWith("@")))
				{
					User	= strTo.Substring(0, Index);
					Domain	= strTo.Substring(Index + 1);
					return true;
				}
			}

			// Bad address
			User = Domain = null;
			return false;
		}
		#endregion

	}

	#region Category Options List
	public class CategoryList : ArrayList
	{
		// Class members
		private Hashtable			m_IDTable		= new Hashtable();	// Pointer to parent TGPConfigure instance
		private Int32				m_TokenCount	= DEF_MAXTOKEN;		// Maximum Token Count
		private Int32				m_DayKeepCount	= TOP_PURGE;		// Maximum Token Count
		private bool				m_LogResults	= false;			// Indicator to log Analyzer results
		private string				m_SplitterRegEx	= DEF_REGEX;		// Regular Expression for Token splitter

		// Constants
		private const int			DEF_MAXTOKEN	= 60;
		private	const int			TOP_PURGE		= 120;
		private	const string		DEF_REGEX		= @"[\s_\!""\#\%\&\(\)\*\+\,\-\.\/\:\;\<\=\>\?\@\[\\\]\^\~\`]+";

		#region Add (w/ override)
		public override int Add(object value)
		{
			if (value is CategoryEntry)
			{
				// Add Category entry
				int Index = base.Add(value);

				// Add CategoryIndex to CategoryID lookup entry
				m_IDTable.Add (((CategoryEntry)value).CategoryID, Index);

				// Return array index
				return (Index);
			}
			else 
			{
				return (-1);
			}
		}
		#endregion

		#region Properties

		#region this[] (override)
		/// <summary>
		/// this[int] Summary
		/// </summary>
		public new CategoryEntry this[int index] 
		{
			get{return ((CategoryEntry)base[index]);}
		}
		#endregion

		#region TokenCount
		/// <summary>
		/// TokenCount Summary
		/// </summary>
		public Int32 TokenCount 
		{
			get{return (m_TokenCount);}
			set{m_TokenCount = value;}
		}
		#endregion

		#region DayKeepCount
		/// <summary>
		/// DayKeepCount Summary
		/// </summary>
		public Int32 DayKeepCount 
		{
			get{return (m_DayKeepCount);}
			set{m_DayKeepCount = value;}
		}
		#endregion

		#region LogResults
		/// <summary>
		/// LogResults Summary
		/// </summary>
		public bool LogResults 
		{
			get{return (m_LogResults);}
			set{m_LogResults = value;}
		}
		#endregion

		#region SplitterRegEx
		/// <summary>
		/// SplitterRegEx Summary
		/// </summary>
		public string SplitterRegEx 
		{
			get{return (m_SplitterRegEx);}
			set{m_SplitterRegEx = value;}
		}
		#endregion

		#endregion

		#region IndexFromID
		public int IndexFromID (Int32 CategoryID)
		{
			return (m_IDTable.ContainsKey(CategoryID) ? (int) m_IDTable[CategoryID] : -1);
		}
		#endregion

	}
	#endregion

	#region Category Options Entry
	public class CategoryEntry 
	{
		public	Int32	CategoryID;
		public	string	CategoryName;
		public	string	MailBoxName;
		public	double	Threshold;
		public	bool	AutoMove;
		public	bool	AutoRead;
	}
	#endregion

}
