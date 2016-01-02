using System;
using System.Collections;
using System.Data;
using System.Data.OleDb;
using System.Text;

namespace TGPConnector
{
	// Base array list for the remoted log entries
	[Serializable]
	public class LogRemoteList : ArrayList
	{
		// Class members
		private Connector					m_Connector		= null;		// Pointer to parent TGPConfigure instance
		private ListUpdateEventHandler		m_OnBaseUpdate	= null;		// Event handler to share events across instances
		private	int							m_MaxCount;					// Maximimum number of entry items

		#region Initialize
		internal void Initialize(Connector Connector, int MaxCount)
		{
			// Set pointer to parent instance
			m_Connector = Connector;
			m_MaxCount = MaxCount;
		}
		#endregion

		#region Properties

		#region MaxCount
		/// <summary>
		/// MaxCount Summary
		/// </summary>
		public int MaxCount
		{
			get {return (m_MaxCount);}
		}
		#endregion

		#endregion

		#region Methods (Internal)

		#region OnAdd
		internal int OnAdd(ListUpdateEventArgs UpdateArgs, Credentials crRemote)
		{
			try
			{
				// Remove lines if over max count
				if (Count >= m_MaxCount)
				{
					base.RemoveAt(0);
				}

				// Add latest line
				int Index = base.Add(UpdateArgs.Record);

				// Notify display clients of change
				FireListUpdate(UpdateArgs, crRemote);
				return (Index);
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException_NoDisplay(ex, null, null);
				return (-1);
			}
		}
		#endregion
		
		#region OnClear
		internal void OnClear(ListUpdateEventArgs UpdateArgs, Credentials crRemote)
		{
			try
			{
				// Remove all lines
				base.Clear();
				FireListUpdate(UpdateArgs, crRemote);
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException_NoDisplay(ex, null, null);
			}
		}
		#endregion
		
		#region FireListUpdate
		private void FireListUpdate(ListUpdateEventArgs UpdateArgs, Credentials Credentials) 
		{
			// Anyone to notify?
			if (m_OnBaseUpdate == null) return;

			// No need to check remote authentication - LogRemote checks before adding delegate to list
			// if (!m_Connector.RMServer.Authenticate(Credentials)) return (false);

			// Get the invocation list of subscribers to be notified
			System.Delegate[] InvocationList = m_OnBaseUpdate.GetInvocationList();

			// loop thru the list and try and notify each one
			foreach (System.Delegate Subscriber in InvocationList) 
			{
				try 
				{
					// Cast the delegate to our type of delegate
					ListUpdateEventHandler EventHandler = (ListUpdateEventHandler) Subscriber;                                                

					// Call all subscribers other than the sender using the callback delegate
					if (UpdateArgs.SenderHandler != EventHandler) EventHandler (this, UpdateArgs, Credentials);                                                
				}
				catch(Exception ex)
				{
					// AJM Note: Could this routine replace the event handler with the local one?
					// Possibly specify the local event handler for failed remote connections?

					// The subscriber is a dead client and will be removed from the chain
					//ajm test			m_OnBaseUpdate -= (ListUpdateEventHandler) Subscriber;
					System.Delegate.Remove(m_OnBaseUpdate, Subscriber);

					// Log the event *after* removing the offending connection to prevent an infinite logging looop
					m_Connector.FireLogException(ex, RMServer.MSG_FAILEDREMOTE, UpdateArgs.SenderHandler.ToString());
				}
			}
			return;
		}
		#endregion

		#region Base Update Event Handler Add/Remove

		internal void BaseUpdateEventAdd (ListUpdateEventHandler RemoteHandler)
		{
			m_OnBaseUpdate += RemoteHandler;
		}

		internal void BaseUpdateEventRemove (ListUpdateEventHandler RemoteHandler)
		{
			m_OnBaseUpdate -= RemoteHandler;
		}
		
		#endregion

		#endregion

	}
}
