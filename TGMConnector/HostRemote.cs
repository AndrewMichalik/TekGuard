using System;
using System.Net;

namespace TGMConnector
{
	/// <summary>
	/// Summary description for HostRemote
	/// </summary>
	public class HostRemote : MarshalByRefObject
	{
		// Static members for logging & connection information
		internal static Connector	m_Connector			= null;		// Pointer to parent TGMConnector object			

		#region Constructors
		// Note: Remoted object constructor is called once for the first call
		// to System.Activator.GetObject by LogDisplay independent of the number
		// of instances activated or remoted by RegisterWellKnownServiceType
		#endregion

		#region InitializeLifetimeService
		public override object InitializeLifetimeService()
		{
			return null;
		}
		#endregion

		#region Properties (Public)

		#region RemoteURL
		/// <summary>
		/// RemoteURL Summary
		/// </summary>
		public String RemoteURL 
		{
			get 
			{
				// Get the remoted objects URL
				String Address = null;
				try
				{
					Address = RemotingIDHelper.GetURLForObject(this);
				}
				catch(Exception ex)
				{
					m_Connector.FireLogException(ex, null, null);
				}
				return (Address != null ? Address : "localhost");
			}
		}
		#endregion

		#endregion

		#region Methods (Public)

		#region UIAuthenticate
		public bool UIAuthenticate(Credentials Credentials)
		{
			// Check remote authentication
			return (m_Connector.RMServer.Authenticate(Credentials));
		}
		#endregion

		#region MachineFrameworkVersion
		public string MachineFrameworkVersion (Credentials Credentials)
		{
			// Check remote authentication
			if (!m_Connector.RMServer.Authenticate(Credentials)) return (null);

			try
			{
				// Return .Net Framework version
				return (Environment.Version.ToString());
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, null, null);
				return null;
			}
		}
		#endregion

		#region MachineName
		public string MachineName(Credentials Credentials)
		{
			// Check remote authentication
			if (!m_Connector.RMServer.Authenticate(Credentials)) return (null);

			try
			{
				// Return machine name
				return (Environment.MachineName);
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, null, null);
				return null;
			}
		}
		#endregion

		#region IPPrimary
		public string IPPrimary(Credentials Credentials)
		{
			// Check remote authentication
			if (!m_Connector.RMServer.Authenticate(Credentials)) return (null);

			try
			{
				// Return primary IP address
				return (IPLocal());
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, null, null);
				return null;
			}
		}
		#endregion

		#region IPLocal
		private string IPLocal()
		{
			return ((Dns.GetHostByName(Dns.GetHostName()).AddressList[0]).ToString());
		}
		#endregion

		#region IPLocalList
		public string[] IPLocalList(Credentials Credentials)
		{
			// Check remote authentication
			if (!m_Connector.RMServer.Authenticate(Credentials)) return (null);

			try
			{
				// Return list of all IP addresses
				return (IPAdaptersInfo.IPLocalList());
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, null, null);
				return null;
			}

		}
		#endregion

		#region DNSLocalList
		public string[] DNSLocalList(Credentials Credentials)
		{
			// Check remote authentication
			if (!m_Connector.RMServer.Authenticate(Credentials)) return (null);

			try
			{
				// Return list of all Local DNS addresses
				return (IPAdaptersInfo.GetRegistryDNS());
			}
			catch(Exception ex)
			{
				m_Connector.FireLogException(ex, null, null);
				return null;
			}

		}
		#endregion

		#endregion

	}
}
