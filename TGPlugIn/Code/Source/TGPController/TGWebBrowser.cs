using System;
using System.Runtime.InteropServices;
using SHDocVw;
using mshtml;

namespace TGPController
{
	public class TGWebBrowser : AxSHDocVw.AxWebBrowser, DWebBrowserEvents
	{
		// Component members
		private Controller				m_Controller;			// Pointer to parent TGPController instance
		private	string					m_URL;					// Destination URL
		private	string					m_CropStart;			// HTML Cropping start string
		private	string					m_CropStop;				// HTML Cropping stop string

		// Capture BeforeNavigate2 variables
		private	UCOMIConnectionPoint	m_icp;
		private int						m_cookie	= -1;
		private	int						m_Task;					// _DocumentComplete task entry control

		// Event Handlers for parent control
		private BeforeNavigateEventHandler	m_OnBeforeNavigate;			// Event handler to share events across instances

		#region Constructors / Destructors
		public TGWebBrowser (Controller Controller)
		{
			// Save the parent instance information
			m_Controller = Controller;

			// Rework the HTML when the document load completes
			this.DocumentComplete += new AxSHDocVw.DWebBrowserEvents2_DocumentCompleteEventHandler(OnDocumentComplete);

			// On Navigation Error, show blank (gray) panel
			this.NavigateError += new AxSHDocVw.DWebBrowserEvents2_NavigateErrorEventHandler(OnNavigateError);
		}
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			try 
			{
				if( disposing )
				{
					// Release event sink
					if (-1 != m_cookie) m_icp.Unadvise(m_cookie);
					m_cookie = -1;

					// Clear events
					this.NavigateError -= new AxSHDocVw.DWebBrowserEvents2_NavigateErrorEventHandler(OnNavigateError);
					this.DocumentComplete -= new AxSHDocVw.DWebBrowserEvents2_DocumentCompleteEventHandler(OnDocumentComplete);
				}
				base.Dispose( disposing );
			}
			catch {}
		}
		#endregion

		#region Initialize
		public bool Initialize(string URL, string CropStart, string CropStop)
		{
			/// Valid request?
			if ((URL == null) || (URL.Length == 0)) return (false);

			// Save the URL and HTML cropping information.
			// Set to lower case to guarantee matches.
			m_URL		= URL.ToLower();
			m_CropStart	= CropStart.ToLower();
			m_CropStop	= CropStop.ToLower();

			try
			{
				// From: The BeforeNavigate2 Event of WebBrowser Control Does Not Fire If Hosted in a Visual C# .NET Application
				// http://support.microsoft.com/default.aspx?scid=kb;EN-US;325079
				// Right-click in the Toolbox window, and then click Customize Toolbox. 
				// On the COM Components tab, select the Microsoft Web Browser check box.
				// AJM Note: Parent must complete ".Add" for the next call to work
				UCOMIConnectionPointContainer icpc = (UCOMIConnectionPointContainer)base.GetOcx(); // ADDed
				Guid g = typeof(DWebBrowserEvents).GUID;
				icpc.FindConnectionPoint(ref g, out m_icp);
				m_icp.Advise(this, out m_cookie);
				return (true);
			}
			catch(Exception ex)
			{
				m_Controller.FireLogException(ex, null, null);
				return (false);
			}
		}
		#endregion

		#region DocumentStart
		public bool DocumentStart ()
		{
			try
			{
				// Initialize the re-navigate parameters
				m_Task = 1;

				// Make browser control "invisible"
				base.Dock = System.Windows.Forms.DockStyle.None;
				base.Width = 0;

				// AJM Note: Possibly set timeout period here?

				// Navigate to the web site
				object oURL = m_URL;
				object sNull = "";
				object oNull = null;
				base.Navigate2(ref oURL , ref oNull, ref oNull, ref sNull, ref sNull);

				// Success
				return (true);
			}
			catch (Exception ex)
			{
				m_Controller.FireLogException(ex, "m_URL", m_URL);
				return (false);
			}
		}
		#endregion

		#region OnDocumentComplete
		private void OnDocumentComplete(object sender, AxSHDocVw.DWebBrowserEvents2_DocumentCompleteEvent e)
		{
			try
			{
				switch(m_Task)
				{
					case 1:
						m_Task++;

						try
						{
							IHTMLDocument2 myDoc = (IHTMLDocument2) base.Document;
							string TrimHTML = myDoc.body.innerHTML;
							TrimHTML = TrimHTML.Substring(TrimHTML.ToLower().LastIndexOf(m_CropStart));
							TrimHTML = TrimHTML.Substring(0, TrimHTML.ToLower().IndexOf(m_CropStop) + m_CropStop.Length);

							// AJM Temp: Fix backound color
							TrimHTML = TrimHTML.Replace("color_bg=FF9900", "color_bg=FDFFCA");	// Orange
							TrimHTML = TrimHTML.Replace("color_bg=333333", "color_bg=FDFFCA");	// Dark Gray

							// Load new text
							myDoc.body.innerHTML = TrimHTML;				

							// Set zero width margins for the document
							myDoc.body.setAttribute("bottomMargin", (object) 0, 0);
							myDoc.body.setAttribute("leftMargin", (object) 0, 0);
							myDoc.body.setAttribute("topMargin", (object) 0, 0);
							myDoc.body.setAttribute("rightMargin", (object) 0, 0);
						}
						catch (Exception ex)
						{
							ex = ex;
						}
						break;

					case 2:
						m_Task++;

						// Set the control "Visible" to fill the space
//						base.Dock = System.Windows.Forms.DockStyle.Fill;

						// Note 12/04: Tried to use a "panel" control on ctlBrowser to position
						// the web browser. However, got intermittant ActiveX exceptions that I
						// was unable to trap or diagnose.
						base.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
							| System.Windows.Forms.AnchorStyles.Left) 
							| System.Windows.Forms.AnchorStyles.Right)));
						base.Location = new System.Drawing.Point(0, 24);
						base.Size = new System.Drawing.Size(this.Parent.Size.Width, this.Parent.Size.Height - base.Location.Y);
						break;
				
					default:
						// continuation of automated tasks...
						break;
				}
			}
			catch (Exception ex)
			{
				m_Controller.FireLogException(ex, null, null);
				return;
			}
		}
		#endregion

		#region OnNavigateError
		private void OnNavigateError(object sender, AxSHDocVw.DWebBrowserEvents2_NavigateErrorEvent e)
		{
			// AJM Note: Does not catch "Internet Explorer cannot open the site... Operation aborted"
			// Navigation Cancelled:
			// @"res://C:\WINNT\system32\shdoclc.dll/navcancl.htm#http://www.xxsecretcraving.com/"

			// Ignore navigation errors (show blank gray panel)
			e.cancel = true;
		}
		#endregion

		#region OnBeforeNavigate
		/// <summary>
		/// BeforeNavigateHandler Summary
		/// </summary>
		public BeforeNavigateEventHandler BeforeNavigateHandler
		{
			get {return (m_OnBeforeNavigate);}
		}
		/// <summary>
		/// OnBeforeNavigate Summary
		/// </summary>
		public event BeforeNavigateEventHandler OnBeforeNavigate
		{
			add { m_OnBeforeNavigate += value; }
			remove { m_OnBeforeNavigate -= value; }
		}
		#endregion

		#region DWebBrowserEvents methods (additional)
		new public void PropertyChange(string Property){}

		public void BeforeNavigate(string URL, int Flags, string TargetFrameName,
			ref object PostData, string Headers, ref bool Cancel)
		{
			m_OnBeforeNavigate(URL, Flags, TargetFrameName, ref PostData, Headers, ref Cancel);
		}
		
		public void NavigateComplete(string URL) {}

		public void WindowActivate(){}

		public void FrameBeforeNavigate(string URL, int Flags, string TargetFrameName, ref object PostData, string Headers, ref bool Cancel){}

		public void NewWindow(string URL, int Flags, string TargetFrameName, ref object PostData, string Headers, ref bool Processed){}

		public void FrameNewWindow(string URL, int Flags, string TargetFrameName, ref object PostData, string Headers, ref bool Processed){}

		new public void TitleChange(string Text){}

		new public void DownloadBegin(){}

		new public void DownloadComplete(){}

		public void WindowMove(){}

		public void WindowResize(){}

		public void Quit(ref bool Cancel){}

		new public void ProgressChange(int Progress, int ProgressMax){}

		new public void StatusTextChange(string Text){}

		new public void CommandStateChange(int Command, bool Enable){}

		public void FrameNavigateComplete(string URL){}
		#endregion

	}

	#region Public Event Delegates
	/// <summary>Before Navigate EventHandler Summary that will be thrown to clients</summary>
	public delegate void BeforeNavigateEventHandler(string URL, int Flags, string TargetFrameName,
		ref object PostData, string Headers, ref bool Cancel);
	#endregion

}
