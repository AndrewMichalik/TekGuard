using System;
using System.Data;
using System.Windows.Forms;

namespace TGPController
{
	/// <summary>
	/// Summary description for DBData
	/// </summary>
	public class DBData
	{
		// Component members
		private Controller			m_Controller	= null;		// Pointer to parent TGPController instance
		
		#region Constructors / Destructors
		public DBData(Controller Controller)
		{
			// Get the parent instance information
			m_Controller = Controller;
		}
		#endregion
		
		#region Get
		public bool Get (Object Control, DataRow drRow, string strField)
		{
			try
			{
				if (Control is TextBox)
				{
					((TextBox)Control).Text = Convert.ToString(drRow[strField]);
				}
				else if (Control is TreeNode)
				{
					((TreeNode)Control).Text = Convert.ToString(drRow[strField]);
				}
				else if (Control is CheckBox)
				{
					((CheckBox)Control).Checked = Convert.ToBoolean(drRow[strField]);
				}
				return true;
			}
			catch(Exception ex)
			{
				m_Controller.FireLogException(ex, null, null);
				return false;
			}
		}
		#endregion
		
		#region Set
		public bool Set (Object Control, DataRow drRow, string strField)
		{
			try
			{
				if (Control is TextBox)
				{
					drRow[strField] = ((TextBox)Control).Text;
				}
				else if (Control is TreeNode)
				{
					drRow[strField] = ((TreeNode)Control).Text;
				}
				else if (Control is CheckBox)
				{
					drRow[strField] = Convert.ToBoolean(((CheckBox)Control).Checked);
				}
				return true;
			}
			catch(Exception ex)
			{
				m_Controller.FireLogException(ex, null, null);
				return false;
			}
		}
		#endregion
		
		#region Check
		public bool Check (Object Control, DataRow drRow, string strField)
		{
			bool bDifferent = true;
			try
			{
				if (Control is TextBox)
				{
					bDifferent = (drRow[strField].ToString() != ((TextBox)Control).Text);
				}
				else if (Control is TreeNode)
				{
					bDifferent = (drRow[strField].ToString() != ((TreeNode)Control).Text);
				}
				else if (Control is CheckBox)
				{
					bDifferent = (Convert.ToBoolean(drRow[strField]) != Convert.ToBoolean(((CheckBox)Control).Checked));
				}
				return bDifferent;
			}
			catch(Exception ex)
			{
				m_Controller.FireLogException(ex, null, null);
				return true;
			}
		}
		#endregion

	}
}
