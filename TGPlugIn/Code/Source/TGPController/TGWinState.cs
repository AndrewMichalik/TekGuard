using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;

namespace TGPController
{
	public class TGWinState : System.ComponentModel.Component
	{
		// Class members
		private Controller			m_Controller		= null;		// Pointer to parent TGMController instance
		private Form				m_Parent;						// parent window for this operation
		private string				m_WinStateName;					// Window state DB entry name

		private int					m_normalLeft;
		private int					m_normalTop;
		private int					m_normalWidth;
		private int					m_normalHeight;
		private FormWindowState		m_windowState;

		// Database access constants
		private const string		QRY_SETTINGS			= "qryApplicationSettings";

		#region Constructors

		public TGWinState(Controller Controller, Form Parent, string WinStateName)
		{
			// Set pointer to parent instance
			m_Controller = Controller;
			m_WinStateName = WinStateName;
			m_Parent = Parent;

			try
			{
				// Subscribe to parent form events
				m_Parent.Closing += new System.ComponentModel.CancelEventHandler(OnClosing);
				m_Parent.Resize += new System.EventHandler(OnResize);
				m_Parent.Move += new System.EventHandler(OnMove);
				m_Parent.Load += new System.EventHandler(OnLoad);

				// Get initial position and size in case form is never moved or resized
				m_normalLeft = m_Parent.Left;
				m_normalTop = m_Parent.Top;
				m_normalWidth = m_Parent.Width;
				m_normalHeight = m_Parent.Height;
			}
			catch {}

		}
		#endregion

		#region OnLoad

		private void OnLoad(object sender, System.EventArgs e)
		{
			try
			{
				// Load the application settings
				DataRow drRow = m_Controller.DBCfgQuery.DSSelectRow ("SELECT * FROM " + QRY_SETTINGS);

				// Retrieve the entries; load the form elements
				string sState = drRow[m_WinStateName].ToString();

				// Attempt to read state from string
				if (sState != null)
				{
					Rectangle recPos = Read (m_Parent, sState);
					// FormWindowState windowState = (FormWindowState) key.GetValue("WindowState", (int)m_Parent.WindowState);

					m_Parent.Location = new Point(recPos.Left, recPos.Top);
					m_Parent.Size = new Size(recPos.Width, recPos.Height);
					// m_Parent.WindowState = windowState;
				}
			}
			catch(Exception ex)
			{
				m_Controller.FireLogException(ex, null, null);
			}
		}
		#endregion

		#region OnResize

		private void OnResize(object sender, System.EventArgs e)
		{
			// Save width and height
			if(m_Parent.WindowState == FormWindowState.Normal)
			{
				m_normalWidth = m_Parent.Width;
				m_normalHeight = m_Parent.Height;
			}
		}
		#endregion

		#region OnMove

		private void OnMove(object sender, System.EventArgs e)
		{
			// Save position
			if (m_Parent.WindowState == FormWindowState.Normal)
			{
				m_normalLeft = m_Parent.Left;
				m_normalTop = m_Parent.Top;
			}
			// save state
			m_windowState = m_Parent.WindowState;
		}
		#endregion

		#region OnClosing

		private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try
			{
				string sState;

				// Save position, size and state
				if (m_Parent.WindowState == FormWindowState.Normal)
				{
					sState = new Rectangle(m_Parent.Left, m_Parent.Top, m_Parent.Width, m_Parent.Height).ToString();
				}
				else
				{
					if (m_Parent.WindowState == FormWindowState.Minimized) m_windowState = FormWindowState.Normal;
					sState = new Rectangle(m_normalLeft, m_normalTop, m_normalWidth, m_normalHeight).ToString();
				}

				// Override if state is minimized
				sState += "{State=" + ((Int32)m_windowState).ToString() + "}";

				// Create the global settings
				DataSet dsGlobal = new DataSet();

				// Select the global settings
				string SQL = "SELECT * FROM " + QRY_SETTINGS;
				
				// Create the adapter and command builder
				OleDbDataAdapter adGlobal = m_Controller.DBCfgQuery.ADCreate ();
				m_Controller.DBCfgQuery.ADSelect (adGlobal, ref dsGlobal, SQL, QRY_SETTINGS);
				
				// Get the global settings row
				DataRow drRow = (dsGlobal.Tables[0]).Rows[0];

				// Update the entries
				drRow[m_WinStateName] = sState;

				// Write the data back to the database
				adGlobal.Update(dsGlobal);

				// Accept the changes to clear the HasChanges() flag
				dsGlobal.AcceptChanges();
			}
			catch(Exception ex)
			{
				m_Controller.FireLogException(ex, null, null);
			}
		}
		#endregion

		#region Read

		private Rectangle Read (Form frmParent, string sStored)
		{
			// Save default form position
			Rectangle recParent = new Rectangle(frmParent.Left, frmParent.Top, frmParent.Width, frmParent.Height);

			try 
			{
				// Retrieve stored position string
				string[]aStored = sStored.Split(new char[] {'{', '=', ',', '}'});
			
				// Valid position string format?			
				if (aStored.Length != 13) 
				{
					return (recParent);
				}

				// Extract rectangle entries
				Rectangle recStored = new Rectangle(Convert.ToInt32(aStored[2]), Convert.ToInt32(aStored[4]), Convert.ToInt32(aStored[6]), Convert.ToInt32(aStored[8]));

				// Valid width & height entries?			
				recStored.Width	 = Math.Max(recStored.Width, 2* SystemInformation.MinimumWindowSize.Width);
				recStored.Width	 = Math.Min(recStored.Width, SystemInformation.PrimaryMonitorSize.Width);
				recStored.Height = Math.Max(recStored.Height, 2 * SystemInformation.MinimumWindowSize.Height);
				recStored.Height = Math.Min(recStored.Height, SystemInformation.PrimaryMonitorSize.Height);

				// Valid border position entries?
				recStored.X = Math.Max(recStored.Left, (SystemInformation.MenuButtonSize.Width * 2) - recStored.Width);
				recStored.X = Math.Min(recStored.Left, SystemInformation.WorkingArea.Width - (SystemInformation.MenuButtonSize.Width * 2));
				recStored.Y = Math.Max(recStored.Top, (SystemInformation.MenuButtonSize.Height * 2) - recStored.Height);
				recStored.Y = Math.Min(recStored.Top, SystemInformation.WorkingArea.Height - (SystemInformation.MenuButtonSize.Height * 2));

				// Extract Window State entry
				FormWindowState windowState = (FormWindowState) Convert.ToInt32(aStored[11]);

				// Saved position is OK
				return (recStored);
			}
			catch
			{
				return (recParent);
			}
		}
		#endregion
	
	}
}
