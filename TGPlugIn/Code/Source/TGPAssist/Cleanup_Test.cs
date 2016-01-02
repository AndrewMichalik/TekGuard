using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Outlook;
using Microsoft.Office.Core;

namespace TGPAssist
{
	/// <summary>
	/// Summary description for Cleanup_Test
	/// </summary>
	public class Cleanup_Test
	{
		// Component members
		private Assist				m_Assist;				// Pointer to parent TGPlugIn instance

		private bool					blnRunUnInitHandler;
		private	Outlook.Application		objOutlookApp;
		private Outlook.Explorers		objExplorers;
		private Outlook.ExplorerClass	objExplorer;
		private Outlook.Inspectors		objInspectors;
		private Outlook.InspectorClass	objInspector;

		#region Constructors
		public Cleanup_Test(Assist Assist)
		{
			// Set pointer to parent instance
			m_Assist = Assist;
		}
		#endregion

		#region InitHandler
		public bool InitHandler(Outlook.Application oOutlookApp)
		{
			objOutlookApp = oOutlookApp;

			objInspectors = oOutlookApp.Inspectors;
			objExplorers = oOutlookApp.Explorers;
			objExplorer = (Outlook.ExplorerClass) oOutlookApp.ActiveExplorer();

			// Set up a new event handler for the inspector events
			// HandleNewInspector();
			HandleCloseInspector();

			// Set up a new event handler for the explorer events
			// HandleNewExplorer();
			HandleCloseExplorer();

			return (true);
		}
		#endregion

		#region UnInitHandler
		/// <summary>
		/// Releases all global objects so Outlook can properly shut down
		/// </summary>
		protected void UnInitHandler()
		{
			try
			{
				if(blnRunUnInitHandler)
				{
					return;
				}
				else
				{
					blnRunUnInitHandler = true;
				}

				// if(mainButton!=null)
				// {
				// 	object omissing = System.Reflection.Missing.Value;
				// 	mainButton.Delete(omissing);
				// }

//ajm - cleanup
//m_PlugIn.CommandButtonRemove(m_PlugIn.m_btnAnalyze);
//m_PlugIn.CommandButtonRemove(m_PlugIn.m_btnIsGood);
//m_PlugIn.CommandButtonRemove(m_PlugIn.m_btnIsJunk);
//m_PlugIn.CommandButtonRemove(m_PlugIn.m_btnDashboard);
//				m_PlugIn.m_Dashboard.Terminate();

				// DisposeObject(objMailItem);
				TGPOutlookApp.DisposeObject(objInspector);
				TGPOutlookApp.DisposeObject(objInspectors);
				TGPOutlookApp.DisposeObject(objExplorer);
				TGPOutlookApp.DisposeObject(objExplorers);


				// DisposeObject(m_PlugIn.oInspectors);
//				m_PlugIn.m_Dashboard = null;
//				m_PlugIn.m_Analyzer = null;
//				m_PlugIn.m_Assist = null;

// m_oOutlookApp = null;
//DisposeObject(m_PlugIn.oOutlookApp);

			}
			catch(SystemException ex)
			{
				m_Assist.FireLogException(ex, null, null);
			}
			finally
			{
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}
		}

		#endregion

		#region xOnNewInspectorEvent
		protected void xOnNewInspectorEvent(Outlook.Inspector inspector)
		{
			try
			{
				if(objInspector==null)
				{
					objInspector = (Outlook.InspectorClass)inspector;
					HandleCloseInspector();   //THIS MAY NOT BE NEEDED!!!!
				} 
			}
			catch(SystemException ex)
			{
				m_Assist.FireLogException(ex, null, null);
			}
		}
		#endregion

		#region OnInspectorClosedEvent
		protected void OnInspectorClosedEvent()
		{
			try
			{
				if(objOutlookApp.ActiveExplorer()==null && objOutlookApp.Inspectors.Count <= 1)
				{
					UnInitHandler();
				} 
			}
			catch(SystemException ex)
			{
				m_Assist.FireLogException(ex, null, null);
			}
		}
		#endregion

		#region OnNewExplorersEvent
		protected void OnNewExplorersEvent(Outlook.Explorer explorer)
		{
			try
			{
				if(objExplorer==null)
				{
					objExplorer = (Outlook.ExplorerClass)explorer;
					HandleCloseExplorer();   //THIS MAY NOT BE NEEDED!!!!
				} 
			}
			catch(SystemException ex)
			{
				m_Assist.FireLogException(ex, null, null);
			}
		}
 		#endregion

		#region OnExplorerClosedEvent
		protected void OnExplorerClosedEvent()
		{
			try
			{
				objExplorer = (Outlook.ExplorerClass) objOutlookApp.ActiveExplorer();
				if(objExplorer == null && objOutlookApp.Inspectors.Count == 0)
				{
					UnInitHandler();
				} 
			}
			catch(SystemException ex)
			{
				m_Assist.FireLogException(ex, null, null);
			}
		}
		#endregion

		#region xHandleNewInspector
		/// <summary>
		/// Sets up a new event handler for the new inspector event
		/// </summary>
		protected void xHandleNewInspector()
		{
			objInspectors.NewInspector += new
				InspectorsEvents_NewInspectorEventHandler(xOnNewInspectorEvent);
		}
		#endregion

		#region HandleCloseInspector
		/// <summary>
		/// Sets up a new event handler for the inspector closed event
		/// </summary>
		protected void HandleCloseInspector()
		{
			if(objInspector != null)
			{
				objInspector.InspectorEvents_Event_Close += new
					InspectorEvents_CloseEventHandler(OnInspectorClosedEvent);
			}
		}
		#endregion

		#region xHandleNewExplorer
		/// <summary>
		/// Sets up a new event handler for the new explorer event
		/// </summary>
		protected void xHandleNewExplorer()
		{
			try
			{
				objExplorers.NewExplorer += new
					ExplorersEvents_NewExplorerEventHandler(OnNewExplorersEvent);
			}
			catch(SystemException ex)
			{
				m_Assist.FireLogException(ex, null, null);
			}
		}
		#endregion

		#region HandleCloseExplorer
		/// <summary>
		/// Sets up a new event handler for the explorer closed event.
		/// </summary>
		protected void HandleCloseExplorer()
		{
			if(objExplorer == null) return;
			try
			{
				objExplorer.ExplorerEvents_Event_Close += new
					ExplorerEvents_CloseEventHandler(OnExplorerClosedEvent);
			}
			catch(SystemException ex)
			{
				m_Assist.FireLogException(ex, null, null);
			}
		}
		#endregion

	}
}
