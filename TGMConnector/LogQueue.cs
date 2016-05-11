using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Threading;
using System.Windows.Forms;

namespace TGMConnector
{	
	/// <summary>
	/// Summary for LogDisplay
	/// AJM Note: Prevent infinite "logging loops" when errors occur in 
	/// the remote logging and display functions.
	/// </summary>
	public class LogQueue
	{
		// Class members
		private Connector			m_Connector			= null;		// Pointer to parent TGPConfigure instance
		private LogRemote			m_LogRemote;					// Local or Remoted Database connection

		// Threaded queue reader update members
		private Queue				m_Queue				= null;	
		private	Thread				m_thrMain			= null;		// Main thread for list changed event queue
//		private ManualResetEvent	m_QActive			= null;		// Logging Queue semaphore

		// Queue constants		
		private	const int			QUE_MSTIMEOUT		= 1000;
		private	const int			QUE_MAX				= 100;		// Initial queue length

		// Members for remote security
		private	Credentials			m_Credentials;						// Authentication credentials
		private string				m_CryptKey;							// Encryption / Decryption keys

		#region Constructors

		// Local
		public LogQueue(Connector Connector, out string ErrorText)
		{
			// Set pointer to parent instance
			m_Connector = Connector;

			// Initialize Logging Queue; initialize size for efficient Insert/Remove operations
			m_Queue = new Queue(QUE_MAX);

			// Open the display connection
			// Note: remote access security settings are ignored for local instances
			try
			{
				// Create a local copy of Database connection object
				m_LogRemote = new LogRemote();

				// Return success
				ErrorText = null;
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException_NoDisplay(ex, null, null);
				ErrorText = ex.Message;
			}
		}

		// Remote
		public LogQueue(Connector Connector, string URL, Credentials Credentials, string CryptKey, out string ErrorText)
		{
			// Set pointer to parent instance
			m_Connector = Connector;

			// Initialize Logging Queue; initialize size for efficient Insert/Remove operations
			m_Queue = new Queue(QUE_MAX);

			// Set the remote access security settings
			m_Credentials = Credentials;
			m_CryptKey = CryptKey;
			
			// Open the display connection
			try
			{
				// Create a remote copy of Database connection object
				m_LogRemote = (LogRemote)System.Activator.GetObject(typeof(LogRemote), URL);

				// Return success
				ErrorText = null;
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException_NoDisplay(ex, null, null);
				ErrorText = ex.Message;
			}
		}
		#endregion

		#region Properties (public)

		#region Enabled
		/// <summary>
		/// Enabled Summary
		/// </summary>
		public bool Enabled 
		{
			get {return (m_LogRemote != null);}
		}
		#endregion

		#region RemoteURL
		/// <summary>
		/// RemoteURL Summary
		/// </summary>
		public string RemoteURL 
		{
			get {return (m_LogRemote.RemoteURL);}
		}
		#endregion

		#endregion

		#region Methods (public)

		#region StartAll
		public void StartAll ()
		{
			// Note: Initial AddItem() events sit in the threaded_Reader Queue until
			// the reader is started, which is after LogArray is fully initialized and
			// bound to the appropriate controls.

			// Activate the threaded Queue reader for *all* LogArray data bound controls
			FireThreadedRead();
		}
		#endregion

		#endregion
		
		#region Enqueue
		internal void Enqueue(ListUpdateEventHandler SenderHandler, LogDisplayType DisplayType, ListChangedType ChangedType, object Record)
		{
			// Thread-safe, lock the shared resource (== Monitor.Enter)
			try
			{
				lock (m_Queue)
				{
					// Push one element.
					m_Queue.Enqueue(new ListUpdateEventArgs(SenderHandler, DisplayType, ChangedType, Record, 0, 1));

					// Release the waiting thread since the state changed
					Monitor.Pulse(m_Queue);
				}
			}
			catch {}
		}
		#endregion

		#region MaxCount
		/// <summary>
		/// MaxCount Summary
		/// </summary>
		internal int MaxCount(LogDisplayType DisplayType)
		{
			return (m_LogRemote.MaxCount(DisplayType, m_Credentials));
		}
		#endregion

		#region CopyOf
		/// <summary>
		/// CopyOf Summary
		/// </summary>
		internal ArrayList CopyOf(LogDisplayType DisplayType)
		{
			return (m_LogRemote.CopyOf(DisplayType, m_Credentials));
		}
		#endregion

		#region Queue Changed Event Handler Add/Remove

		internal void QueueChangedEventAdd (LogDisplayType DisplayType, ListUpdateEventHandler RemoteHandler)
		{
			m_LogRemote.ListUpdateEventAdd (DisplayType, RemoteHandler, m_Credentials);
		}

		internal void QueueChangedEventRemove (LogDisplayType DisplayType, ListUpdateEventHandler RemoteHandler)
		{
			m_LogRemote.ListUpdateEventRemove (DisplayType, RemoteHandler, m_Credentials);
		}
		
		#endregion

		#region FireThreadedRead
		private void FireThreadedRead ()
		{
			// Note: Initial AddItem() events sit in the threaded_Reader Queue until
			// the reader is started, which is after LogArray is fully initialized and
			// bound to the appropriate controls.

			// Exit the thread when the application exits
			Application.ApplicationExit += new EventHandler(ApplicationExitHandler);

			// Create a new thread for the window
			m_thrMain = new Thread(new ThreadStart(ThreadedReader));
			m_thrMain.Priority = ThreadPriority.BelowNormal;
			m_thrMain.Start();
		}
		#endregion

		#region ThreadedReader
		private void ThreadedReader()
		{
			// Lock the shared resource (== Monitor.Enter)
			// Use null thread handle to indicate exit condition
			while (m_thrMain != null) lock (m_Queue)
			{
				try 
				{
					// Wait in the loop while the queue is busy.
					// Wait for the resource state to be changed by another
					// thread/s, so we can unlock, temporarily, the resource.
					while ((m_Queue.Count > 0) || Monitor.Wait(m_Queue, QUE_MSTIMEOUT))
					{
						// Remove all items
						while (m_Queue.Count > 0)
						{
							// Pop the first element
							ListUpdateEventArgs UpdateArgs = (ListUpdateEventArgs) m_Queue.Dequeue();

							// Send the event to the local logger
							UpdateArgs.SenderHandler(this, UpdateArgs, m_Credentials);
							
							// Try to send the event to the remote logger							
							try 
							{
								// Add the element to the remote display
								if (m_LogRemote != null) m_LogRemote.UpdateEventHandler(UpdateArgs, m_Credentials);
							}
							catch(Exception ex)
							{
								// m_LogRemote = null;
								// Log the event *after* removing the offending connection to prevent an infinite logging looop
								// m_Connector.FireLogException(ex, RMServer.MSG_FAILEDREMOTE, m_LogRemote.RemoteURL);
								m_Connector.FireLogException_NoDisplay(ex, RMServer.MSG_FAILEDREMOTE, m_LogRemote.RemoteURL);
							}
						}
					}
					// Timed out - program exiting?
				}
				catch(Exception ex)
				{
					m_Connector.FireLogException_NoDisplay(ex, null, null);
				}
			}
		}
		#endregion

		#region ApplicationExitHandler
		private void ApplicationExitHandler(object sender, EventArgs args)
		{
			// Tell thread to exit on next loop
			m_thrMain = null;

			// Wait for the thread loop exit to complete
			lock (m_Queue) {};
		}
		#endregion

	}
}
