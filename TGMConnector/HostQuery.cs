using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.OleDb;

namespace TGMConnector
{
	/// <summary>
	/// Summary description for HostQuery
	/// </summary>
	public class HostQuery
	{
		// Component members
		private Connector			m_Connector			= null;		// Pointer to parent TGPConfigure instance
		private HostRemote			m_HostRemote		= null;		// Local or Remoted Database connection

		// Members for remote security
		private	Credentials			m_Credentials;					// Authentication credentials
		private string				m_CryptKey;						// Encryption / Decryption keys

		#region Constructors
		// Local
		internal HostQuery (Connector Connector, out string ErrorText)
		{
			// Set pointer to parent instance
			m_Connector = Connector;
			
			// Open the database connections
			// Note: remote access security settings are ignored for local instances
			try
			{
				// Create a local copy of Database connection object
				m_HostRemote = new HostRemote();
				ErrorText = null;
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, null, null);
				ErrorText = ex.Message;
			}
		}

		// Remote
		internal HostQuery (Connector Connector, string URL, Credentials Credentials, string CryptKey, out string ErrorText)
		{
			// Set pointer to parent instance
			m_Connector = Connector;

			// Set the remote access security settings
			m_Credentials = Credentials;
			m_CryptKey = CryptKey;
			
			// Open the database connections
			try
			{
				// Better Solution: Step by Step guide to CAO creation through SAO class factories:
				// http://www.glacialcomponents.com/ArticleDetail.aspx?articleID=CAOGuide
				// Create an exclusive remote copy (CAO) of Database connection object
				// Activator.CreateInstance()
				
				// Activate the remote copy (SAO) of the Database connection object
				m_HostRemote = (HostRemote)System.Activator.GetObject(typeof(HostRemote), URL);

				// Authentication successful?
				if (!m_HostRemote.UIAuthenticate(Credentials))
				{
					ErrorText = RMServer.MSG_FAILEDLOGIN;
				} 
				else
				{
					ErrorText = null;
				}
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, "URL", URL);
				ErrorText = ex.Message;
			}
		}
		#endregion

		#region Properties

		#region Enabled
		/// <summary>
		/// Enabled Summary
		/// </summary>
		public bool Enabled 
		{
			get {return (m_HostRemote != null);}
		}
		#endregion

		#region RemoteURL
		/// <summary>
		/// RemoteURL Summary
		/// </summary>
		public string RemoteURL 
		{
			get {return (m_HostRemote.RemoteURL);}
		}
		#endregion

		#region DBAuthenticate
		public bool UIAuthenticate
		{
			// Check remote authentication
			get {return (m_HostRemote.UIAuthenticate(m_Credentials));}
		}
		#endregion

		#region MachineFrameworkVersion
		public string MachineFrameworkVersion
		{
			get {return (m_HostRemote.MachineFrameworkVersion(m_Credentials));}
		}
		#endregion

		#region MachineName
		public string MachineName
		{
			get {return (m_HostRemote.MachineName(m_Credentials));}
		}
		#endregion

		#region IPPrimary
		public string IPPrimary
		{
			get {return (m_HostRemote.IPPrimary(m_Credentials));}
		}
		#endregion

		#region IPLocalList
		public string[] IPLocalList
		{
			get {return (m_HostRemote.IPLocalList(m_Credentials));}
		}
		#endregion

		#region DNSLocalList
		public string[] DNSLocalList
		{
			get {return (m_HostRemote.DNSLocalList(m_Credentials));}
		}
		#endregion

		#endregion

	}
}
