using System;
using System.Data;

namespace TGMConnector
{
	public class DataSetHelper
	{
		public	DataSet							ds;

		private System.Collections.ArrayList	m_FieldInfo;
		private string							m_FieldList;
		private System.Collections.ArrayList	GroupByFieldInfo;
		private string							GroupByFieldList;
		private class FieldInfo
		{
			public string RelationName;
			public string FieldName;	//source table field name
			public string FieldAlias;	//destination table field name
			public string Aggregate;
		}
				
		#region Constructors
		public DataSetHelper(ref DataSet DataSet)
		{
			ds = DataSet;
		}
		public DataSetHelper()
		{
			ds = null;
		}
		#endregion

		private bool ColumnEqual(object A, object B)
		{
			// Compares two values to see if they are equal. Also compares DBNULL.Value.
			// Note: If your DataTable contains object fields, then you must extend this
			// function to handle them in a meaningful way if you intend to group on them.
			if ( A == DBNull.Value && B == DBNull.Value ) //  both are DBNull.Value
				return true;
			if ( A == DBNull.Value || B == DBNull.Value ) //  only one is DBNull.Value
				return false;
			
			// Date comparison
			if (A.GetType() == typeof(System.DateTime)) return (((DateTime)A).ToShortDateString().Equals(((DateTime)B).ToShortDateString()));
			
			// Value type standard comparison
			return ( A.Equals(B) ); 
		}

		#region SELECT TOP
		public DataTable SelectTop(DataTable SourceTable, int Start, int RowCount)
		{
			DataTable Table = SourceTable.Clone();

			// DataTable  Sorted = SourceTable.Select(

			for (int ii=Start; ii<Start + RowCount; ii++)
			{
				if (ii >= SourceTable.Rows.Count) break;
				else Table.ImportRow(SourceTable.Rows[ii]);
			}
			return (Table);
		}
		#endregion

		#region CREATE TABLE
		public DataTable CreateTable(string TableName, DataTable SourceTable, string FieldList)
		{
			/*
			 * This code creates a DataTable by using the SourceTable as a template and creates the fields in the
			 * order that is specified in the FieldList. If the FieldList is blank, the code uses DataTable.Clone().
			*/
			DataTable dt;
			if (FieldList.Trim() == "")
			{
				dt = SourceTable.Clone();
				dt.TableName = TableName;
			}
			else
			{
				dt = new DataTable(TableName);
				ParseFieldList(FieldList,false);
				DataColumn dc;
				foreach (FieldInfo Field in m_FieldInfo)
				{
					dc = SourceTable.Columns[Field.FieldName];
					dt.Columns.Add(Field.FieldAlias, dc.DataType);
				}
			}
			if (ds!=null)
				ds.Tables.Add(dt);
			return dt;
		}
		#endregion

		#region INSERT INTO
		/* The following is the calling convention for the InsertInto method:
		 * dsHelper.InsertInto(ds.Tables["TestTable"], ds.Tables["Employees"], "FirstName FName,LastName LName,BirthDate", "EmployeeID<5", "BirthDate") ;
		*/
		public void InsertInto(DataTable DestTable, DataTable SourceTable,
			string FieldList, string RowFilter, string Sort)
		{
			//
			// This code copies the selected rows and columns from SourceTable and inserts them into DestTable.
			//

			ParseFieldList(FieldList, false);
			DataRow[] Rows = SourceTable.Select(RowFilter, Sort);
			DataRow DestRow;
			foreach(DataRow SourceRow in Rows)
			{
				DestRow = DestTable.NewRow();
				if (FieldList == "")
				{
					foreach(DataColumn dc in DestRow.Table.Columns)
					{
						if (dc.Expression == "")
							DestRow[dc] = SourceRow[dc.ColumnName];
					}
				}
				else
				{
					foreach(FieldInfo Field in m_FieldInfo)
					{
						DestRow[Field.FieldAlias] = SourceRow[Field.FieldName];
					}
				}
				DestTable.Rows.Add(DestRow);
			}
		}
		#endregion

		#region SELECT INTO
		/* The following is the calling convention for the SelectInto method:
		 * dt = dsHelper.SelectInto("TestTable", ds.Tables["Employees"], "FirstName FName,LastName LName,BirthDate", "EmployeeID<5", "BirthDate") ;
		*/
		public DataTable SelectInto(string TableName, DataTable SourceTable,
			string FieldList, string RowFilter, string Sort)
		{
			/*
			 *  This code selects values that are sorted and filtered from one DataTable into another.
			 *  The FieldList specifies which fields are to be copied.
			*/
			DataTable dt = CreateTable(TableName, SourceTable, FieldList);
			InsertInto(dt, SourceTable, FieldList, RowFilter, Sort);
			return dt;
		}
		#endregion

		#region SELECT DISTINCT
		/* The following is the calling convention for the SelectDistinct method:
		 * dsHelper.SelectDistinct("DistinctEmployees", ds.Tables["Orders"], "EmployeeID");
		*/
		public DataTable SelectDistinct(string TableName, DataTable SourceTable, string FieldName, bool bDesc)
		{
			DataTable dt = new DataTable(TableName);
			dt.Columns.Add(FieldName, SourceTable.Columns[FieldName].DataType);

			object LastValue = null;
			foreach (DataRow dr in SourceTable.Select("", FieldName + (bDesc ? " DESC" : "")))
			{
				if (  LastValue == null || !(ColumnEqual(LastValue, dr[FieldName])) )
				{
					LastValue = dr[FieldName];
					dt.Rows.Add(new object[]{LastValue});
				}
			}
			if (ds != null)
				ds.Tables.Add(dt);
			return dt;
		}
		#endregion

		#region GROUP BY
		/* The following is the calling convention for the CreateGroupByTable method:
		 * dt = dsHelper.CreateGroupByTable("OrderSummary", ds.Tables["Orders"], "EmployeeID, sum(Amount) Total, min(Amount) Min,max(Amount) Max");
		 * This call sample creates a new DataTable with a TableName of OrderSummary and four fields (EmployeeID, Total, Min, and
		 * Max). The four fields have the same data type as the EmployeeID and the Amount
		 * fields in the Orders table.
		*/																																																																																																																																																																																																																																																																																																																																						   
		public DataTable CreateGroupByTable(string TableName, DataTable SourceTable, string FieldList)
		{
			/*
			 * Creates a table based on aggregates of fields of another table
			 *
			 * RowFilter affects rows before GroupBy operation. No "Having" support
			 * though this can be emulated by subsequent filtering of the table that results
			 *
			 *  FieldList syntax: fieldname[ alias]|aggregatefunction(fieldname)[ alias], ...
			*/
			if (FieldList == null)
			{
				throw new ArgumentException("You must specify at least one field in the field list.");
				//return CreateTable(TableName, SourceTable);
			}
			else
			{
				DataTable dt = new DataTable(TableName);
				ParseGroupByFieldList(FieldList);
				foreach (FieldInfo Field in GroupByFieldInfo)
				{
					DataColumn dc  = SourceTable.Columns[Field.FieldName];
					if (Field.Aggregate==null)
						dt.Columns.Add(Field.FieldAlias, dc.DataType, dc.Expression);
					else
						dt.Columns.Add(Field.FieldAlias, dc.DataType);
				}
				if (ds != null)
					ds.Tables.Add(dt);
				return dt;
			}
		}
		
		private void ParseFieldList(string FieldList, bool AllowRelation)
		{
			/*
			 * This code parses FieldList into FieldInfo objects  and then
			 * adds them to the m_FieldInfo private member
			 *
			 * FieldList syntax:  [relationname.]fieldname[ alias], ...
			*/
			if (m_FieldList == FieldList) return;
			m_FieldInfo = new System.Collections.ArrayList();
			m_FieldList = FieldList;
			FieldInfo Field; string[] FieldParts; string[] Fields=FieldList.Split(',');
			int i;
			for (i=0; i<=Fields.Length-1; i++)
			{
				Field=new FieldInfo();
				//parse FieldAlias
				FieldParts = Fields[i].Trim().Split(' ');
				switch (FieldParts.Length)
				{
					case 1:
						//to be set at the end of the loop
						break;
					case 2:
						Field.FieldAlias=FieldParts[1];
						break;
					default:
						throw new Exception("Too many spaces in field definition: '" + Fields[i] + "'.");
				}
				//parse FieldName and RelationName
				FieldParts = FieldParts[0].Split('.');
				switch (FieldParts.Length)
				{
					case 1:
						Field.FieldName=FieldParts[0];
						break;
					case 2:
						if (AllowRelation==false)
							throw new Exception("Relation specifiers not permitted in field list: '" + Fields[i] + "'.");
						Field.RelationName = FieldParts[0].Trim();
						Field.FieldName=FieldParts[1].Trim();
						break;
					default:
						throw new Exception("Invalid field definition: " + Fields[i] + "'.");
				}
				if (Field.FieldAlias==null)
					Field.FieldAlias = Field.FieldName;
				m_FieldInfo.Add (Field);
			}
		}

		private void ParseGroupByFieldList(string FieldList)
		{
			/*
			* Parses FieldList into FieldInfo objects and adds them to the GroupByFieldInfo private member
			*
			* FieldList syntax: fieldname[ alias]|operatorname(fieldname)[ alias],...
			*
			* Supported Operators: count,sum,max,min,first,last
			*/
			if (GroupByFieldList == FieldList) return;
			GroupByFieldInfo = new System.Collections.ArrayList();
			FieldInfo Field; string[] FieldParts; string[] Fields = FieldList.Split(',');
			for (int i=0; i<=Fields.Length-1;i++)
			{
				Field = new FieldInfo();
				//Parse FieldAlias
				FieldParts = Fields[i].Trim().Split(' ');
				switch (FieldParts.Length)
				{
					case 1:
						//to be set at the end of the loop
						break;
					case 2:
						Field.FieldAlias = FieldParts[1];
						break;
					default:
						throw new ArgumentException("Too many spaces in field definition: '" + Fields[i] + "'.");
				}
				//Parse FieldName and Aggregate
				FieldParts = FieldParts[0].Split('(');
				switch (FieldParts.Length)
				{
					case 1:
						Field.FieldName = FieldParts[0];
						break;
					case 2:
						Field.Aggregate = FieldParts[0].Trim().ToLower();    //we're doing a case-sensitive comparison later
						Field.FieldName = FieldParts[1].Trim(' ', ')');
						break;
					default:
						throw new ArgumentException("Invalid field definition: '" + Fields[i] + "'.");
				}
				if (Field.FieldAlias==null)
				{
					if (Field.Aggregate==null)
						Field.FieldAlias=Field.FieldName;
					else
						Field.FieldAlias = Field.Aggregate + "of" + Field.FieldName;
				}
				GroupByFieldInfo.Add(Field);
			}
			GroupByFieldList = FieldList;
		}
		
		#endregion

	}
}
