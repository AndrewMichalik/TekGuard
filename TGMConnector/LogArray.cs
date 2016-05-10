using System;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace TGMConnector
{
	public class LogArray : ArrayList, IBindingList, IDisposable
	{
		// Class members
		private LogQueue				m_LogQueue;						// Local or Remoted Queued Log Display
		private	LogDisplayType			m_DisplayType;					// Display arraylist identifier
		private Control					m_ParentControl;				// Parent WinForm Control for Thread Safe invocation
		private	UpdateEventHandlerClass m_EventClass;					// Event handler for server events
		private	ListUpdateEventHandler	m_EventHandler;					// Event handler for server events
		private	int						m_MaxCount;						// Local maximum line count (<= remote count)
		private	const int				LOC_MAXCOUNT = 50;				// Local array maximum count

		#region Constructors
			
		public LogArray(LogQueue LogQueue, LogDisplayType DisplayType)
		{
			// Save the remote connection information
			m_LogQueue		= LogQueue;
			m_DisplayType	= DisplayType;

			// Create event handler for server events
			m_EventClass = new UpdateEventHandlerClass(this);
			m_EventHandler = new ListUpdateEventHandler(m_EventClass.RemoteEventCallback);
		}
		#endregion

		#region Connect
		public LogArray Connect(Control ParentControl)
		{
			// Thread-safe, lock the shared resource (== Monitor.Enter)
			try
			{
				// Save parent window information
				m_ParentControl	= ParentControl;

				// Ignore lines if over local Maximum Count
				ArrayList List = m_LogQueue.CopyOf(m_DisplayType);
				m_MaxCount = Math.Min(m_LogQueue.MaxCount(m_DisplayType), LOC_MAXCOUNT);

				// Initialize base ArrayList with the tail of the remote array
				base.AddRange(List.GetRange(Math.Max(0, List.Count - m_MaxCount), Math.Min(List.Count, m_MaxCount)));
			
				// Connect the event handler for server events
				m_LogQueue.QueueChangedEventAdd(m_DisplayType, m_EventHandler);

				// Success
				return (this);
			}
			catch(Exception ex)
			{
				ex = ex;
				return (null);
			}
		}
		#endregion

		#region Add
		public override int Add(object Record)
		{
			// Add this record to the event queue.
			try
			{
				// Specify the local event handler for failed remote connections
				m_LogQueue.Enqueue(m_EventHandler, m_DisplayType, ListChangedType.ItemAdded, Record);
			}
			catch {}

			// Always return last position
			return (this.Count);
		}
		#endregion

		#region Clear
		public override void Clear()
		{
			// Add this clear request to the event queue.
			try
			{
				// Specify the local event handler for failed remote connections
				m_LogQueue.Enqueue(m_EventHandler, m_DisplayType, ListChangedType.Reset, null);
			}
			catch {}
		}
		#endregion

		#region OnAdd
		private int OnAdd(ListUpdateEventArgs UpdateArgs)
		{
			// Remove lines if over max count
			if (base.Count >= m_MaxCount)
			{
				base.RemoveAt(0);
				OnRemoveComplete_BeginInvoke(UpdateArgs);
			}

			int Index;
			Index = base.Add(UpdateArgs.Record);
			OnInsertComplete_BeginInvoke(UpdateArgs);
			return Index;
		}
		#endregion

		#region OnClear
		private void OnClear(ListUpdateEventArgs UpdateArgs)
		{
			// Remove all lines
			base.Clear();
			OnRemoveComplete_BeginInvoke(UpdateArgs);
		}
		#endregion

		#region OnRemoveComplete_BeginInvoke
		private void OnRemoveComplete_BeginInvoke(ListUpdateEventArgs ListUpdateArgs)
		{
			// AJM Note: This hangs Dashboard if Dashboard calls Log Alert from
			// the main thread. Also, Outlook does not do proper object cleanup.

			// Pack the "Invoke" arguments
			object[] objArray = new object[1];
			objArray[0] = new object[] {ListUpdateArgs};

			// Call the control using the parent thread
			m_ParentControl.BeginInvoke(new WinFormInvokeHandler(OnRemoveComplete_Invoked), objArray);
		}
		private object OnRemoveComplete_Invoked(object[] objArray)
		{
			// Unpack the arguments
			ListUpdateEventArgs ListUpdateArgs = (ListUpdateEventArgs) objArray[0];

			// Invoke the function using the control thread
			OnRemoveComplete(ListUpdateArgs.Index, ListUpdateArgs.Record);

			return (null);
		}
		#endregion

		#region OnInsertComplete_BeginInvoke
		private void OnInsertComplete_BeginInvoke(ListUpdateEventArgs ListUpdateArgs)
		{
			// AJM Note: This hangs Dashboard if Dashboard calls Log Alert from
			// the main thread. Also, Outlook does not do proper object cleanup.

			// Pack the "Invoke" arguments
			object[] objArray = new object[1];
			objArray[0] = new object[] {ListUpdateArgs};

			// Call the control using the parent thread
			m_ParentControl.BeginInvoke(new WinFormInvokeHandler(OnInsertComplete_Invoked), objArray);
		}
		private object OnInsertComplete_Invoked(object[] objArray)
		{
			// Unpack the arguments
			ListUpdateEventArgs ListUpdateArgs = (ListUpdateEventArgs) objArray[0];

			// Invoke the function using the control thread
			OnInsertComplete(ListUpdateArgs.Index, ListUpdateArgs.Record);

			return (null);
		}
		#endregion

		#region IBindingList

		public bool AllowEdit { get { return false; }}
		public bool AllowNew { get { return false; }}
		public bool AllowRemove { get { return false; }}

		private ListChangedEventHandler listChangedHandler;
		public event ListChangedEventHandler ListChanged
		{
			add { listChangedHandler += value; }
			remove { listChangedHandler -= value; }
		}

		private void FireListChanged(ListChangedEventArgs UpdateArgs)
		{
			if (listChangedHandler != null)
			{
				listChangedHandler(this, UpdateArgs);
			}
		}

		protected virtual void OnRemoveComplete(int Index, object value)
		{
			FireListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, Index) );
		}

		protected virtual void OnInsertComplete(int Index, object value)
		{
			FireListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, Index) );

			// Scroll down
			if (m_ParentControl.GetType() == typeof(ListBox))
			{
				((ListBox)m_ParentControl).TopIndex = ((ListBox)m_ParentControl).Items.Count - 1;
			}
		}

		public object AddNew() { throw new NotSupportedException(); }
		public void AddIndex(PropertyDescriptor pd) { throw new NotSupportedException(); }
		public void ApplySort(PropertyDescriptor pd, ListSortDirection dir) { throw new NotSupportedException(); }
		public int Find(PropertyDescriptor property, object key) { throw new NotSupportedException(); }
		public bool IsSorted { get { return false; }}
		public void RemoveIndex(PropertyDescriptor pd) { throw new NotSupportedException(); }
		public void RemoveSort() { throw new NotSupportedException(); }
		public ListSortDirection SortDirection 
		{
			get { throw new NotSupportedException(); }
		}
		public PropertyDescriptor SortProperty 
		{
			get { throw new NotSupportedException(); }
		}
		public bool SupportsChangeNotification {get { return true; }}
		public bool SupportsSearching { get { return false; }}
		public bool SupportsSorting { get { return false; }}

		#endregion

		#region IDisposable Members

		public void Dispose()
		{
			try 
			{
				// Remove server event handler
				m_LogQueue.QueueChangedEventRemove(m_DisplayType, m_EventHandler);
			}
			catch {}
		}

		#endregion

		#region UpdateEventHandlerClass
		// Client implementation of the event handler class
		private class UpdateEventHandlerClass : RemoteEventHandlerClass
		{
			// Class members
			private	LogArray				m_LogArray;

			#region Constructors
			public UpdateEventHandlerClass (LogArray LogArray)
			{
				// Save the LogArray parent 
				m_LogArray = LogArray;
			}
			#endregion

			#region InternalEventCallback
			protected override bool InternalEventCallback (object sender, object ServerArgs)
			{
				try
				{
					// Retrieve the arguments passed by the server
					ListUpdateEventArgs UpdateArgs = (ListUpdateEventArgs) ServerArgs;
				
					// Call the component (LogArray will use the parent's thread)
					if (UpdateArgs.ChangeType == ListChangedType.ItemAdded)
					{
						m_LogArray.OnAdd(UpdateArgs);
					}
					else if (UpdateArgs.ChangeType == ListChangedType.Reset)
					{
						m_LogArray.OnClear(UpdateArgs);
					}
				}
				catch(Exception ex)
				{
					ex = ex;
				}

				// Success
				return (true);
			}
			#endregion

			#region Authenticate
			protected override bool Authenticate (Credentials Credentials)
			{
				// Check remote authentication
				// if (!m_Connector.RMServer.Authenticate(Credentials)) return (null);

				return (true);
			}
			#endregion

		}
		#endregion

	}

}
