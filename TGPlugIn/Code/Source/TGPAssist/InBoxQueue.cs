using System;
using System.Collections;
using System.Windows.Forms;
using System.Threading;

namespace TGPAssist
{
	/// <summary>
	/// Summary description for QueueAnalyzer
	/// </summary>
	public class InBoxQueue : IDisposable
	{
		// Component members
		private Assist				m_Assist			= null;			// Pointer to parent TGPAssist instance
		private	Queue				m_Queue				= null;			// InBox Queue structure
		private ManualResetEvent	m_QActive			= null;			// InBox Queue semaphore
		private	bool				m_Enabled			= true;			// Appplication continue/exit indicator

		// Outlook Explorer InBox event handler members
		private	TGPOutlookApp.TGPExplorer.TGPItems		m_TGInboxItems;	// TekGuard memory-safe InBox items

		// Queue constants		
		private	const int			QUE_MSTIMEOUT		= 1000;
		private const int			QUE_MAX				= 32;			// Initial queue length

		#region Constructors / Destructors
		public InBoxQueue(Assist Assist)
		{
			// Set pointer to parent instance
			m_Assist = Assist;

			// Reset the Queue semaphore to wait state
			m_QActive = new ManualResetEvent(false);

			// Initialize Analysis Queue; initialize size for efficient Insert/Remove operations
			m_Queue = new Queue(QUE_MAX);

			// Set the InBox new item event; Use TekGuard memory-safe item list
			m_TGInboxItems = new TGPOutlookApp.TGPExplorer.TGPItems(Outlook.OlDefaultFolders.olFolderInbox);
		}
		~InBoxQueue()
		{
			// Release all COM objects
			Dispose();
		}
		void IDisposable.Dispose()
		{
			// Release all COM objects
			Dispose();
			GC.SuppressFinalize(this);  // Finalization is now unnecessary
		}
		public void Dispose()
		{
			// Tell thread to exit on next loop
			ThreadedReader_Dispose();

			// Release all COM objects
			m_TGInboxItems.Dispose();
		}
		#endregion

		#region Properties

		#region Enabled
		/// <summary>
		/// Enabled Summary
		/// </summary>
		public bool Enabled 
		{
			get {return (m_Enabled);}
			set {m_Enabled = value;}
		}
		#endregion

		#region OnItemAdd
		/// <summary>
		/// OnListUpdate Summary
		/// </summary>
		public event Outlook.ItemsEvents_ItemAddEventHandler OnItemAdd
		{
			add { m_TGInboxItems.Items.ItemAdd += value; }
			remove { m_TGInboxItems.Items.ItemAdd -= value; }
		}
		#endregion

		#endregion

		#region Enqueue
		public void Enqueue(string EntryID, string StoreID)
		{
			try
			{
				// Thread-safe, lock the shared resource (== Monitor.Enter)
				lock (m_Queue)
				{
					// Push one element.
					m_Queue.Enqueue(new IDLocator (EntryID, StoreID));
				}

				// Release the reader thread to process a new entry
				m_QActive.Set();									
			}
			catch {}
		}
		#endregion

		#region Start
		public void Start ()
		{
			// Simple MAPI inherits some of the COM threading apartment problems of full MAPI.
			// You must set the STA threading model immediately as the first instruction
			// Therefore, create COM objects within this thread
			// Thread.CurrentThread.ApartmentState = ApartmentState.STA;


m_Enabled = true;
			
			// Create a new thread for the Analyzer Queue entries
			Thread thrMain = new Thread(new ThreadStart(ThreadedReader));
			thrMain.Priority = ThreadPriority.BelowNormal;
			thrMain.IsBackground = true;
			thrMain.Start();
		}
		#endregion

		#region ThreadedReader
		private void ThreadedReader()
		{
			QReturnType RCode;
			ArrayList	IDList;	

			while (QReturnType.Exit != (RCode = Dequeue(out IDList)))
			{
				// Timed out or item available?
				if (RCode == QReturnType.Empty) continue;

				// Create a new instance of the App object for successful session disposal
				Outlook.Application oOutlookApp	= new Outlook.Application();

				// Initialize MailItem list
				Outlook.MailItem[] MailItems = new Outlook.MailItem[IDList.Count];	

				// Locate each item
				for (int ii=0; ii< IDList.Count; ii++)
				{
					MailItems[ii] = ((Outlook.MailItem) oOutlookApp.Application.Session.GetItemFromID(((IDLocator)IDList[ii]).EntryID, ((IDLocator)IDList[ii]).StoreID));
				}

				// Analyze, mark, and release these messages
				m_Assist.FireMessageAnalyze(this, MailItems);

				// Release all COM objects
				foreach (Outlook.MailItem Item in MailItems) TGPOutlookApp.DisposeObject(Item);
				TGPOutlookApp.DisposeObject(oOutlookApp);
				TGPOutlookApp.DisposeObject();
			}
		}		
		private QReturnType Dequeue(out ArrayList IDList)
		{
			// Initialize list
			IDList = new ArrayList();

			try 
			{
				// Wait until queue has a request before continuing
				if (m_Queue.Count == 0) m_QActive.WaitOne(QUE_MSTIMEOUT, false);

				// Lock the shared resource (== Monitor.Enter)
				if (m_Queue.Count > 0) lock (m_Queue)
				{
					// Remove all items
					while (m_Queue.Count > 0)
					{
						// Pop the first element
						IDList.Add((IDLocator)m_Queue.Dequeue());
					}

					// Reset the semaphore to wait for new entries
					m_QActive.Reset();
				
					// Indicate success
					return (QReturnType.Available);
				}
				// Timed out
			}
			catch {}

			// Error or Timed out - program exiting?
			return (m_Enabled ? QReturnType.Empty : QReturnType.Exit);
		}
		#endregion

		#region ThreadedReader_Dispose
		private void ThreadedReader_Dispose()
		{
			// Tell thread to exit on next loop
			m_Enabled = false;

			// Wait for the thread loop exit to complete
			lock (m_Queue) {};
		}
		#endregion

		#region Stop
		public void Stop ()
		{
m_Enabled = false;
		}
		#endregion
		
		#region Queue Structures (internal)
		private class IDLocator
		{
			public	string		EntryID				= null;		// Outlook storage entry
			public	string		StoreID				= null;		// Outlook storage identifier

			#region Constructors
			public IDLocator (string EntryID, string StoreID)
			{
				this.EntryID		= EntryID;
				this.StoreID		= StoreID;
			}
			#endregion
		}

		#endregion

		#region QReturnType
		// Public members for Queue return information
		private enum QReturnType : int
		{
			Available = 0,
			Empty = 1,
			Exit = 2
		}
		#endregion

	}

}
