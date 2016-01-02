using System;
using System.Collections;
using System.Data;
using TGPConnector;

namespace TGPlugIn
{
	[Serializable()] 
	public class FolderTreeView : TreeSerializer
	{
		#region Constructors
		public FolderTreeView(ArrayList Folders) 
		{
			// Initialize the Nodes array
			m_Nodes = new FolderTreeNode[Folders.Count];

			// Check to see if there are any root folders in the Folder array
			if (Folders.Count != 0) 
			{ 
				// Populate the Nodes array with child nodes
				for (int ii=0; ii<Folders.Count; ii++) 
				{

					m_Nodes[ii] = new FolderTreeNode((Outlook.MAPIFolder) Folders[ii], true); 
				} 
			}

		} 
		#endregion

		#region FlattenFolders
		public static ArrayList FlattenFolders(TreeSerializer TreeSerializer, ArrayList Folders, ref Int32 ItemCount) 
		{
			// Initialize the Nodes array
			// FolderTreeNode[] Nodes = new FolderTreeNode[Folders.Length];

			// Use an arraylist to hold the initial set of Folders
			ArrayList FlatList = new ArrayList();

			// Check to see if there are any root folders in the Folder array
			if (Folders.Count != 0) 
			{ 
				// Populate the Nodes array with child nodes
				for (int ii=0; ii<Folders.Count; ii++) 
				{ 
					FlatList.AddRange(FolderTreeNode.ToFolderNode(TreeSerializer.Nodes[ii], (Outlook.MAPIFolder) Folders[ii], ref ItemCount)); 
				} 
			}
			return (FlatList);
		} 
		#endregion

	} 

	[Serializable()] 
	internal class FolderTreeNode : TreeNodeData 
	{ 
		#region Constructors
		public FolderTreeNode(Outlook.MAPIFolder objFolder, bool bExpanded) 
		{ 
			// Set the basic TreeNode properties
			m_Text					= objFolder.Name + " (" + objFolder.Items.Count + ")"; 
			m_ImageIndex			= 0; 
			m_SelectedImageIndex	= 0; 
			m_Checked				= false; 
			m_Expanded				= bExpanded;
			m_Tag					= objFolder.Name;

			// Initialize the Nodes array
			m_Nodes = new TreeNodeData[objFolder.Folders.Count];

			// Check to see if there are any root folders in the Folder
			if (objFolder.Folders.Count != 0) 
			{ 
				// Populate the Nodes array with child nodes
				for (int ii=0; ii<objFolder.Folders.Count; ii++) 
				{ 
					m_Nodes[ii] = new FolderTreeNode(objFolder.Folders.Item(ii+1), bExpanded); 
				} 
			}
		}
		#endregion

		#region ToFolderNode
		public static ArrayList ToFolderNode(TreeNodeData TreeNodeData, Outlook.MAPIFolder objFolder, ref Int32 ItemCount) 
		{ 
			ArrayList alFolders = new ArrayList();
			
			// Add this folder to the flattened list if it has been selected
			if (TreeNodeData.Checked)
			{
				alFolders.Add(objFolder);

				// Indicate total number of items to process
				ItemCount = ItemCount + objFolder.Items.Count;
			}

			// Recurse through child nodes adding to Folders ArrayList collection
			if ((TreeNodeData.Nodes != null) && (TreeNodeData.Nodes.Length != 0))
			{
				for (int ii=0; ii< TreeNodeData.Nodes.Length; ii++) 
				{ 
					alFolders.AddRange(ToFolderNode(TreeNodeData.Nodes[ii], objFolder.Folders.Item(TreeNodeData.Nodes[ii].Tag), ref ItemCount)); 
				}
			}
			return(alFolders);
		}
		#endregion

	}

}
