using System;
using System.IO;
using System.Windows.Forms;

namespace TGPConnector
{
	// Serializable TreeView that can be initialized from Outlook Folders
	// http://www.codeproject.com/vb/net/TreeViewDataAccess.asp
	// http://www.developerfusion.com/utilities/convertvbtocsharp.aspx

	[Serializable()] 
	public class TreeSerializer 
	{
		// Class members
		protected TreeNodeData[] m_Nodes;

		#region Constructors
		protected TreeSerializer() 
		{
		}
		public TreeSerializer(TreeView treeview) 
		{
			// Initialize the Nodes array
			m_Nodes = new TreeNodeData[treeview.Nodes.Count];

			// Check to see if there are any root nodes in the TreeView
			if (treeview.Nodes.Count != 0) 
			{ 
				// Populate the Nodes array with child nodes
				for (int ii=0; ii<treeview.Nodes.Count; ii++) 
				{ 
					m_Nodes[ii] = new TreeNodeData(treeview.Nodes[ii]); 
				} 
			}
		} 

		#endregion

		#region Nodes
		public TreeNodeData[] Nodes 
		{
			get {return (m_Nodes);}
		}
		#endregion

		#region PopulateTree
		public bool PopulateTree(TreeView treeView) 
		{ 
			// Check to see if there are any root nodes in the TreeSerializer
			if ((this.Nodes != null) && (this.Nodes.Length != 0)) 
			{ 
				// Populate the TreeView with child nodes
				treeView.BeginUpdate(); 
				for (int ii=0; ii<this.Nodes.Length; ii++) 
				{ 
					treeView.Nodes.Add(this.Nodes[ii].ToTreeNode()); 
				} 
				treeView.EndUpdate(); 
			}
			return (true);
		} 
		#endregion
	} 

	[Serializable()] 
	public class TreeNodeData 
	{ 
		protected string			m_Text; 
		protected int				m_ImageIndex; 
		protected int				m_SelectedImageIndex; 
		protected bool				m_Checked; 
		protected bool				m_Expanded; 
		protected object			m_Tag; 
		protected TreeNodeData[]	m_Nodes; 

		#region Constructors
		protected TreeNodeData() 
		{ 
		}
		public TreeNodeData(TreeNode node) 
		{ 
			// Set the basic TreeNode properties
			m_Text					= node.Text; 
			m_ImageIndex			= node.ImageIndex; 
			m_SelectedImageIndex	= node.SelectedImageIndex; 
			m_Checked				= node.Checked; 
			m_Expanded				= node.IsExpanded;
			m_Tag					= null;

			// Initialize the Nodes array
			m_Nodes = new TreeNodeData[node.Nodes.Count];

			// See if there is an object in the tag property and if it is serializable
			if (node.Tag != null) 
			{ 
				if (node.Tag.GetType().IsSerializable) m_Tag = node.Tag; 
			} 

			// Check to see if there are any child nodes
			if (node.Nodes.Count != 0) 
			{
				// Recurse through child nodes and add to Nodes array
				for (int ii=0; ii<node.Nodes.Count; ii++) 
				{ 
					m_Nodes[ii] = new TreeNodeData(node.Nodes[ii]); 
				} 
			}
		} 

		#endregion

		#region ToTreeNode
		public TreeNode ToTreeNode() 
		{ 
			// Create TreeNode based on instance of TreeNodeData and set basic properties
			TreeNode ToTreeNode = new TreeNode(this.Text, m_ImageIndex, m_SelectedImageIndex); 
			ToTreeNode.Checked = m_Checked; 
			ToTreeNode.Tag = m_Tag; 
			if (m_Expanded) 
			{ 
				ToTreeNode.Expand(); 
			} 

			// Recurse through child nodes adding to Nodes collection
			if ((m_Nodes != null) && (m_Nodes.Length != 0))
			{
				for (int ii=0; ii< m_Nodes.Length; ii++) 
				{ 
					ToTreeNode.Nodes.Add(m_Nodes[ii].ToTreeNode()); 
				}
			}
			return(ToTreeNode);
		}
		#endregion
			
		#region Text
		public string Text
		{
			get {return (m_Text);}
		}
		#endregion

		#region Checked
		public bool Checked
		{
			get {return (m_Checked);}
		}
		#endregion

		#region Nodes
		public TreeNodeData[] Nodes 
		{
			get {return (m_Nodes);}
		}
		#endregion

		#region Tag
		public string Tag 
		{
			get {return ((string) m_Tag);}
		}
		#endregion

		#region x_SaveTreeSerializer
		public static void x_SaveTreeSerializer(TreeView treeView, string path) 
		{ 
			// Create a serializer and file to save TreeSerializer
			System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(TreeSerializer)); 
			System.IO.FileStream file = new System.IO.FileStream(path, FileMode.Create); 
			System.Xml.XmlTextWriter writer = new System.Xml.XmlTextWriter(file, null); 

			// Generate TreeSerializer from TreeView and serialize the file.
			ser.Serialize(writer, new TreeSerializer(treeView)); 

			//Tidy up 
			writer.Close(); 
			file.Close(); 
			file = null; 
		}
		#endregion
			
		#region x_LoadTreeSerializer
		public static void x_LoadTreeSerializer(TreeView treeView, string path) 
		{ 
			// Create as serializer and get the file to deserialize
			System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(TreeSerializer)); 
			System.IO.FileStream file = new System.IO.FileStream(path, FileMode.Open); 
			System.Xml.XmlTextReader reader = new System.Xml.XmlTextReader(file); 
			
			// Deserialize the file and populate the treeview 
			TreeSerializer treeData = ((TreeSerializer)ser.Deserialize(reader)); 
			treeData.PopulateTree(treeView); 

			// Tidy up
			reader.Close(); 
			file.Close(); 
			file = null; 
		}
		#endregion

	}

}
