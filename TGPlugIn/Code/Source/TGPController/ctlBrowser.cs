using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Reflection;
using TGPConnector;

namespace TGPController
{
	/// <summary>
	/// Summary description for ctlBrowser.
	/// </summary>
	public class ctlBrowser : System.Windows.Forms.UserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container	components = null;
		private System.Windows.Forms.LinkLabel	lnkAddress;

		// Component members
		private Controller				m_Controller;			// Pointer to parent TGPController instance
		private	TGWebBrowser			m_TGWebBrowser = null;	// TekGuard Web Browser Control
		private	string					m_CropStart;			// HTML Cropping start string
		private	string					m_CropStop;				// HTML Cropping stop string

		// Mini-Browser Google Ads variables
		private	ArrayList				m_AdList = new ArrayList();
		private	Random					m_Rand = new Random();	

		#region Constructors / Destructors
		public ctlBrowser()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}
		#endregion

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.lnkAddress = new System.Windows.Forms.LinkLabel();
			this.SuspendLayout();
			// 
			// lnkAddress
			// 
			this.lnkAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lnkAddress.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lnkAddress.Location = new System.Drawing.Point(0, 0);
			this.lnkAddress.Name = "lnkAddress";
			this.lnkAddress.Size = new System.Drawing.Size(144, 20);
			this.lnkAddress.TabIndex = 0;
			// 
			// ctlBrowser
			// 
			this.Controls.Add(this.lnkAddress);
			this.Name = "ctlBrowser";
			this.Size = new System.Drawing.Size(144, 144);
			this.ResumeLayout(false);

		}
		#endregion

		#region Initialize
		public void Initialize(Controller Controller)
		{
			// Save the parent instance information
			m_Controller = Controller;

			// Initiaize the Mini-Browser Google Ads variables
//			m_AdList.Add("http://www.comla.com/");
//			m_AdList.Add("http://www.funla.com/");
//			m_AdList.Add("http://www.naecp.com/");
//			m_AdList.Add("http://www.vinfo.com/");
//			m_AdList.Add("http://www.voiceinformation.com/");
//			m_AdList.Add("http://www.secretcraving.com/");

			// Set the HTML cropping information
			m_CropStart		= "<script type=text/javascript>";
			m_CropStop		= "</iframe>";

			// AJM Note 12/2004: The ctlBrowser user control does not receive VisibleChanged
			// events when visible goes to "false" when ctlBrowser contains the MS Web Browser
			// control. There are numerous known bugs with the MS COM Browser control.
			// Subscribe to parent (which is System.Windows.Forms.Control) form events
			this.Parent.VisibleChanged += new System.EventHandler(this.ctlBrowser_VisibleChanged);
		}
		#endregion

		#region ctlBrowser_VisibleChanged
		private void ctlBrowser_VisibleChanged(object sender, System.EventArgs e)
		{
			// AJM Note 12/2004: The ctlBrowser user control does not receive VisibleChanged
			// events when visible goes to "false" when ctlBrowser contains the MS Web Browser
			// control. There are numerous known bugs with the MS COM Browser control.
			try
			{
				if (this.Visible) 
				{
					TGWebBrowser_Add(this.Sponsor);			
				}
				else 
				{
					TGWebBrowser_Remove();
				}
			}
			catch (Exception ex)
			{
				ex = ex;
			}
		}
		#endregion

		#region NavigateTo
		public void NavigateTo(string URL)
		{
			try
			{
				// Pack the "Invoke" arguments
				object[] objArray = new object[1];
				objArray[0] = new object[] {URL};

				// Call the control using the parent thread
				Invoke(new WinFormInvokeHandler(NavigateTo_Invoked), objArray);
//				Refresh();
			}
			catch (Exception ex)
			{
				ex = ex;
			}
		}
		private object NavigateTo_Invoked(object[] objArray)
		{
			try
			{
				// Unpack the arguments
				string URL = (string) objArray[0];
				if (URL == null) URL = Sponsor;
				TGWebBrowser_Remove();
				TGWebBrowser_Add(URL);	
			}
			catch (Exception ex)
			{
				// MTA Errro: "Could not instantiate ActiveX control 'xyz' because the current thread is not in a single-threaded apartment."
				ex = ex;
			}
			return (null);
		}
		#endregion

		#region Sponsors
		internal string[] Sponsors 
		{
			get
			{
				lock (m_AdList)
				{
					return ((string[])m_AdList.ToArray(typeof(string)));
				}
			}
			set
			{
				lock (m_AdList)
				{
					m_AdList = new ArrayList();
					m_AdList.AddRange(value);
				}
			}
		}
		#endregion

		#region Sponsor
		private string Sponsor 
		{
			get
			{
				lock (m_AdList)
				{
					if ((m_AdList == null) || (m_AdList.Count == 0)) return (null);
					return (m_AdList[m_Rand.Next(m_AdList.Count)].ToString());
				}
			}
		}
		#endregion

		#region TGWebBrowser Add / Remove
		// The handle of WebBrowser control is destroyed and recreated again
		// during hide and show. Detailed information from the link:
		//	http://support.microsoft.com/support/kb/articles/q182/1/11.asp
		// Unfortunately, after the "Controls.Remove", the object is Disposed, and
		// "COM object that has been separated from its underlying RCW can not be used."
		private void TGWebBrowser_Add(string URL)
		{
			using (m_TGWebBrowser)
			{
				// Already inactive? Anywhere to go?
				if ((m_TGWebBrowser != null) || (URL == null)) return;

				// Set up top navigation link label
				lnkAddress.Text	= URL;
				lnkAddress.Links.Add(0, lnkAddress.Text.Length, URL);
				lnkAddress.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkAddress_LinkClicked);

				// Create and add this control to the form
				this.m_TGWebBrowser = new TGPController.TGWebBrowser(m_Controller);
				this.Controls.Add(m_TGWebBrowser);

				// Initialize and navigate to this address
				m_TGWebBrowser.Initialize(lnkAddress.Text, m_CropStart, m_CropStop);
				m_TGWebBrowser.OnBeforeNavigate += new BeforeNavigateEventHandler(BeforeNavigate);
				m_TGWebBrowser.DocumentStart();
			}
		}

		private void TGWebBrowser_Remove()
		{
			using (m_TGWebBrowser)
			{
				// Already inactive?
				if (m_TGWebBrowser == null) return;

				// Remove this control from the form
				this.Controls.Remove(m_TGWebBrowser);
				m_TGWebBrowser.OnBeforeNavigate -= new BeforeNavigateEventHandler(BeforeNavigate);
				m_TGWebBrowser = null;

				// Remove the top navigation link label
				lnkAddress.Text	= "";
				lnkAddress.Links.Remove(lnkAddress.Links[0]);
				lnkAddress.LinkClicked -= new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkAddress_LinkClicked);
			}
		}
		#endregion

		#region BeforeNavigate
		private void BeforeNavigate(string URL, int Flags, string TargetFrameName,
			ref object PostData, string Headers, ref bool Cancel)
		{
			if (URL != lnkAddress.Text) 
			{
//AJM test
//this.Dock = System.Windows.Forms.DockStyle.Fill;
//lnkAddress.Text = URL;

				// Open a new browser window for this navigation request
				BrowserLaunch (URL);

				// Ignore this control's request to navigate
				Cancel = true;
			}
		}
		#endregion

		#region BrowserLaunch
		private void BrowserLaunch(string URL)
		{
			// To use system default browser
			// System.Diagnostics.Process.Start(URL); 

			// Open a new Internet Explorer browser window for this navigation request
			Type typIE = System.Type.GetTypeFromProgID("InternetExplorer.Application");
			object objIE = Activator.CreateInstance(typIE);
			typIE.InvokeMember("Visible", BindingFlags.SetProperty, null, objIE, new object[] {true});
			typIE.InvokeMember("Navigate", BindingFlags.InvokeMethod, null, objIE, new object[] {URL});
		}
		#endregion

		#region Form Events

		#region lnkAddress_LinkClicked
		private void lnkAddress_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			lnkAddress.Links[lnkAddress.Links.IndexOf(e.Link)].Visited = true;
			ProcessStartInfo psInfo = new ProcessStartInfo("iexplore.exe", "-new " + e.Link.LinkData.ToString());
			Process.Start(psInfo);
		}
		#endregion

		#endregion

	}
}
