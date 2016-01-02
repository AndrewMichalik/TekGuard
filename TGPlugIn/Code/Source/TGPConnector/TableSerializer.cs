using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace TGPConnector
{
	// Worker class for binary serialization
	[Serializable]
	public class TableSerializer: DataTable, ISerializable
	{

		#region Constructors
		public TableSerializer(DataTable dt)
		{
			// Set the table name
			TableName = dt.TableName;

			// Insert column information (names and types)
			foreach(DataColumn col in dt.Columns)
			{
				DataColumn colNew = new DataColumn(col.ColumnName, col.DataType);
				colNew.Unique = col.Unique;
				this.Columns.Add(colNew);
			}

			// Copy the data
			foreach (DataRow drRow in dt.Rows) this.ImportRow(drRow);

			// Set the primary key (after data load)
			for (int ii = 0; ii < dt.PrimaryKey.Length; ii++)
			{
				this.PrimaryKey = new DataColumn[] { this.Columns[dt.PrimaryKey[ii].ColumnName] };
			}

			// Finish
			this.AcceptChanges();
		}

		// Fill the DataTable object with the embedded data 
		protected TableSerializer(SerializationInfo si, StreamingContext context)
		{
			// Note: Do not call the base class upon deserialization

			// Extract table data from the serialization info
			this.TableName = (string) si.GetValue("TableName", typeof(string));
			ArrayList colNames = (ArrayList) si.GetValue("colNames", typeof(ArrayList));
			ArrayList colTypes = (ArrayList) si.GetValue("colTypes", typeof(ArrayList)); 
			ArrayList colState = (ArrayList) si.GetValue("colState", typeof(ArrayList)); 
			ArrayList dataRows = (ArrayList) si.GetValue("dataRows", typeof(ArrayList)); 
			ArrayList tableKey = (ArrayList) si.GetValue("tableKey", typeof(ArrayList)); 

			// Add columns
			for(int ii=0; ii<colNames.Count; ii++)
			{
				DataColumn col = new DataColumn(colNames[ii].ToString(), Type.GetType(colTypes[ii].ToString())); 	
				//				col.Unique = (bool) colState[ii];
				this.Columns.Add(col);
			}

			// Add rows
			for(int i=0; i<dataRows.Count; i++)
			{
				DataRow row = this.NewRow();
				row.ItemArray = (object[]) dataRows[i];
				this.Rows.Add(row);
			}

			// Set unique state (after data load)
			for(int i=0; i<colNames.Count; i++)
			{
				this.Columns[i].Unique = (bool) colState[i];
			}

			// Set the primary key (after data load)
			for(int i=0; i<tableKey.Count; i++)
			{
				PrimaryKey = new DataColumn[] { Columns[tableKey[i].ToString()] };
			}

			// Finish
			this.AcceptChanges();
		}

		// The base constructor is only available to the deserializer
		protected TableSerializer() : base()
		{
		}
		#endregion

		#region ISerializable
		void System.Runtime.Serialization.ISerializable.GetObjectData(SerializationInfo si, StreamingContext context)
		{
			// DO NOT call the base method otherwise XML will slip into the data
			// base.GetObjectData(si, context);

			// Insert column information (names and types) into worker arrays
			ArrayList colNames = new ArrayList();
			ArrayList colTypes = new ArrayList();
			ArrayList colState = new ArrayList();
			foreach(DataColumn col in this.Columns)
			{
				colNames.Add(col.ColumnName); 
				colTypes.Add(col.DataType.FullName);   
				colState.Add(col.Unique);   
			}

			// Insert rows information into a worker array
			ArrayList dataRows = new ArrayList();
			foreach(DataRow row in this.Rows) dataRows.Add(row.ItemArray);

			// Get the primary key(s)
			ArrayList tableKey = new ArrayList();
			for (int ii = 0; ii < this.PrimaryKey.Length; ii++)
			{
				tableKey.Add(this.PrimaryKey[ii].ColumnName); 
			}

			// Add arrays to the serialization info structure
			si.AddValue("TableName", this.TableName);
			si.AddValue("colNames", colNames);
			si.AddValue("colTypes", colTypes);
			si.AddValue("colState", colState);
			si.AddValue("dataRows", dataRows);
			si.AddValue("tableKey", tableKey);
		}

		#endregion

		#region SerializationBinder
		// Override deserialization using a binder class
		internal class TableBinder : System.Runtime.Serialization.SerializationBinder
		{
			public override Type BindToType(string assemblyName, string typeName)
			{
				// Return the dynamic class Type by either calling GetType() on the class 
				// OR by calling GetType(string) on the dynamic assembly
				return (Type.GetType(typeName));
			}

		}
		#endregion

		#region BinarySerialize
		internal static void BinarySerialize(DataTable dt, string outputFile)
		{
			// Let caller catch any exceptions
			BinaryFormatter bf = new BinaryFormatter();
			StreamWriter sw = new StreamWriter(outputFile);
                        
			// Instantiate and fill the table serializer 
			TableSerializer Serializer = new TableSerializer(dt); 

			// Serialize the table
			bf.Serialize(sw.BaseStream, Serializer);
			sw.Close();
		}
		#endregion

		#region BinaryDeserialize
		internal static DataTable BinaryDeserialize(string sourceFile)
		{
			// Let caller catch any exceptions
			StreamReader sr = new StreamReader(sourceFile);
			BinaryFormatter bf = new BinaryFormatter(); 
				
			// Use the private data binder
			bf.Binder = new TableBinder();

			// Deserialize the table
			TableSerializer Serializer = (TableSerializer) bf.Deserialize(sr.BaseStream);
			sr.Close();
				
			// Return the populated datatable
			return (Serializer);
		}
		#endregion

	}
}
