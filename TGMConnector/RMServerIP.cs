using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging ;
using System.Runtime.Remoting.Channels;
using Microsoft.Win32;

namespace TGMConnector
{
	#region IPAdaptersInfo
	// DNS Info:
	// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dns/dns/dns_start_page.asp
	//
	internal class IPAdaptersInfo
	{

		#region GetAdaptersInfo

		// Constants		
		private const int MAX_HOSTNAME_LEN = 132;
		private const int MAX_DOMAIN_NAME_LEN = 132;
		private const int MAX_SCOPE_ID_LEN = 260;
		private const int MAX_ADAPTER_NAME_LENGTH = 260;
		private const int MAX_ADAPTER_ADDRESS_LENGTH = 8;
		private const int MAX_ADAPTER_DESCRIPTION_LENGTH = 132;
		private const int ERROR_BUFFER_OVERFLOW = 111;
		private const int MIB_IF_TYPE_ETHERNET = 6;
		private const int MIB_IF_TYPE_TOKENRING = 9;
		private const int MIB_IF_TYPE_FDDI = 15;
		private const int MIB_IF_TYPE_PPP = 23;
		private const int MIB_IF_TYPE_LOOPBACK = 24;
		private const int MIB_IF_TYPE_SLIP = 28;
	
		[StructLayoutAttribute(LayoutKind.Sequential, CharSet=CharSet.Ansi)] public struct FixedInfo
		{
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=MAX_HOSTNAME_LEN)] public String  HostName;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=MAX_DOMAIN_NAME_LEN)] public String  DomainName;
			public Int32  CurrentDnsServer;
			public IPAddrString DnsServerList;
			public Int32  NodeType;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=MAX_SCOPE_ID_LEN)] public String  ScopeId;
			public Int32  EnableRouting;
			public Int32  EnableProxy;
			public Int32  EnableDns;
		}

		[StructLayoutAttribute(LayoutKind.Sequential, CharSet=CharSet.Ansi)] public struct IPAdapterInfo
		{
			public IntPtr  Next;
			public Int32  ComboIndex;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=260)]
			public String  AdapterName;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=132)]
			public String  Description;

			public Int32  AddressLength;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst=8)]
			public Byte[]  Address;

			public Int32  Index;
			public Int32  Type;
			public Int32  DhcpEnabled;

			public IntPtr  CurrentIPAddress;
			public IPAddrString IPAddressList;
			public IPAddrString GatewayList;
			public IPAddrString DhcpServer;
			public Boolean  HaveWins;
			public IPAddrString PrimaryWinsServer;
			public IPAddrString SecondaryWinsServer;

			public Int32  LeaseObtained;
			public Int32  LeaseExpires;
		}

		[StructLayoutAttribute(LayoutKind.Sequential, CharSet=CharSet.Ansi)] public struct IPAddrString
		{
			public IntPtr  Next;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=16)]
			public String  IpAddress;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst=16)]
			public String  IpMask;
			public Int32  Context;
		}

		[DllImport("iphlpapi.dll", ExactSpelling=true)]
		public static extern int GetNetworkParams (IntPtr buffer, ref int srcIP);

		[DllImport("iphlpapi.dll", ExactSpelling=true)]
		public static extern int GetAdaptersInfo (IntPtr buffer, ref int srcIP);

		public static bool GetAdaptersInfo (Connector Connector)
		{
			// Get the main IP configuration information for this machine 
			// using a FIXED_INFO structure.
			int FixedInfoSize = 0; 
			int error = GetNetworkParams(IntPtr.Zero, ref FixedInfoSize);
			if (error != 0)
			{
				if (error != ERROR_BUFFER_OVERFLOW)
				{
//					Cockpit.FireLogException(this, "GetNetworkParams sizing failed with error ", error.ToString());
					return (false);
				}
			}

			// Get the raw data memory block
			IntPtr FixedInfoBuffer = Marshal.AllocHGlobal (FixedInfoSize);
			error = GetNetworkParams(FixedInfoBuffer, ref FixedInfoSize);
			if (error != 0)
			{
				Marshal.FreeHGlobal (FixedInfoBuffer);
				FixedInfoBuffer = IntPtr.Zero;
				return (false);
			}

			// Get the data structure
			IntPtr Ptr = FixedInfoBuffer;
			FixedInfo IPFI = (FixedInfo) Marshal.PtrToStructure (Ptr, typeof(FixedInfo));

			// Unpack Header info
			string HostName = "Host Name:  " + IPFI.HostName;
			string DNSServers = "DNS Servers:  " + IPFI.DnsServerList.IpAddress;
			Ptr = IPFI.DnsServerList.Next;
               
			// Unpack DNS IP Addresses
			do
			{
					IPAddrString IPAS = (IPAddrString) Marshal.PtrToStructure (Ptr, typeof(IPAddrString));
					string Servers = "DNS Server:  " + IPAS.IpAddress;
					Ptr = IPAS.Next;
			}
			while (Ptr != IntPtr.Zero);

			// Decode the node type
			string NodeType;
			switch (IPFI.NodeType)
			{
				case 1:
					NodeType = "Broadcast";
					break;
				case 2:
					NodeType = "Peer to peer";
					break;
				case 4:
					NodeType = "Mixed";
					break;
				case 8:
					NodeType = "Hybrid";
					break;
				default:
					NodeType = "Unknown node type";
					break;
			}
			NodeType = NodeType;	// Suppress warning...
			
			// Decode the routing
			string ScopeID = IPFI.ScopeId;
			string IPRouting = (IPFI.EnableRouting != 0) ? "IP Routing Enabled" : "IP Routing not enabled";
			string EnableProxy = (IPFI.EnableProxy != 0) ? "WINS Proxy Enabled" : "WINS Proxy not Enabled";
			string EnableDns = (IPFI.EnableDns != 0) ? "NetBIOS Resolution Uses DNS" : "NetBIOS Resolution Does not use DNS";
       
			// Clean up
			Marshal.FreeHGlobal (FixedInfoBuffer);
			FixedInfoBuffer = IntPtr.Zero;

			// Enumerate all of the adapter specific information using the
			// IP_ADAPTER_INFO structure.
			// Note:  IP_ADAPTER_INFO contains a linked list of adapter entries.
			int AdapterInfoSize = 0; 
			error = GetAdaptersInfo(IntPtr.Zero, ref AdapterInfoSize);
			if (error != 0)
			{
				if (error != ERROR_BUFFER_OVERFLOW)
				{
//					Cockpit.FireLogException(this, "GetNetworkParams sizing failed with error ", error.ToString());
					return (false);
				}
			}

			// Get the raw data memory block
			IntPtr AdapterInfoBuffer = Marshal.AllocHGlobal (AdapterInfoSize);
			error = GetAdaptersInfo(AdapterInfoBuffer, ref AdapterInfoSize);
			if (error != 0)
			{
				Marshal.FreeHGlobal (AdapterInfoBuffer);
				AdapterInfoBuffer = IntPtr.Zero;
				return (false);
			}

			IntPtr run = AdapterInfoBuffer;
			IPAdapterInfo iai;
			do
			{
				iai = (IPAdapterInfo) Marshal.PtrToStructure (run, typeof(IPAdapterInfo));
				string mac = "?";
				if ((iai.AddressLength > 0) && (iai.AddressLength <= 8))
					mac = BitConverter.ToString (iai.Address, 0, iai.AddressLength);
				run = iai.Next;
			}
			while( run != IntPtr.Zero );

			// Clean up
			Marshal.FreeHGlobal (AdapterInfoBuffer);
			AdapterInfoBuffer = IntPtr.Zero;

			return (true);
		}
		#endregion
		
		#region GetRegistryIP
		private static string[] GetRegistryIP (Connector Connector)
		{
			// If you have statically assigned DNS servers the value you're looking for is "NameServer".
			// If you get your DNS dynamically assigned via DHCP then the value you want is "DhcpNameServer"
			const string sBaseKey = "SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces\\";
			ArrayList oBuffer = new ArrayList();
			string[] arrInterface;
			string[] arrIPAddress;
			RegistryKey oBaseKey = Registry.LocalMachine.OpenSubKey(sBaseKey, false);

			// Anything there?
			if (oBaseKey == null) return ((string[]) oBuffer.ToArray(Type.GetType("System.String")));

			arrInterface = oBaseKey.GetSubKeyNames();
			foreach (string sInterface in arrInterface)
			{
				RegistryKey oKey = Registry.LocalMachine.OpenSubKey(sBaseKey + sInterface + "\\", false);

				// Make sure that we got a key!
				if (oKey == null) continue;

				// Check to see if DHCP is enabled, if yes then we only want to get one IP address
				// otherwise we need to load all potential IP addresses
				bool bDHCP = (bool) oKey.GetValue("EnableDCHP", false);
				if (bDHCP)
				{
					// Get a single IP address
					string sIPAddress = (string) oKey.GetValue("DhcpIPAddress", "");
					// Make sure that this is at least a half valid IP
					if ((sIPAddress.Length > 0) && (sIPAddress != "0.0.0.0")) oBuffer.Add(sIPAddress);
				}
				else
				{
					// Read and process array
					arrIPAddress = (string[]) oKey.GetValue("IPAddress", "");

					// Make sure that this is at least a half valid IP
					foreach (string sIPAddress in arrIPAddress) 
					{
						if ((sIPAddress.Length > 0) && (sIPAddress != "0.0.0.0")) oBuffer.Add(sIPAddress);
					}
				}
			}
			return ((string[]) oBuffer.ToArray(Type.GetType("System.String")));
		}
		#endregion

		#region GetRegistryIPInfo
		/*
		Step 1 
		Open up the HKEY_LOCAL_MACHINE hive and then open the SOFTWARE\Microsoft\Windows NT\CurrentVersion\NetworkCards\1 subkey. The 1 simply means the first network card. If you have multiple cards on your machine they will be listed in numerical order. In this key there is a value called ServiceName which we need to save as this is refered to in Step 2. You can now close this registry key. 
		Step 2 
		Open up the HKEY_LOCAL_MACHINE hive again and this time open the SYSTEM\CurrentControlSet\Services\#SERVICE-NAME#\Parameters\Tcpip key (Where #SERVICE-NAME# is the name of the service you obtained from Step 1). Make sure you open this key with Write access enabled or you will be denied access. 
		Step 3 
		Now you can change the IP address for the IPAddress, DefaultGateway keys etc. The value type of these keys is binary so you must make sure that you do not write a string to the registry or it will change its value type. Instead, use the GetBytes() method of the Encoding class to write the bytes. 
		*/		
		private static void GetRegistryIPInfo()
		{
			// Get interface cards
			string networkcardKeyPath = "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\NetworkCards";
			RegistryKey start = Registry.LocalMachine;
			RegistryKey serviceNames = start.OpenSubKey(networkcardKeyPath);

			if (serviceNames == null) 
			{
				Console.WriteLine("Bad registry key");
				return;
			}

			string[] networkCards = serviceNames.GetSubKeyNames();
			serviceNames.Close();
           
			// for each network card get its service name
			string serviceKeyPath = "SYSTEM\\CurrentControlSet\\Services\\";

			foreach(string keyName in networkCards)   
			{
				string networkcardKeyName = networkcardKeyPath + "\\" + keyName;
				RegistryKey cardServiceName = start.OpenSubKey(networkcardKeyName);
				if (cardServiceName == null) 
				{
					Console.WriteLine("Bad registry key: {0}", networkcardKeyName);
					return;
				}          

				string deviceServiceName = (string) cardServiceName.GetValue("ServiceName");
				string deviceName = (string)cardServiceName.GetValue("Description");
				Console.WriteLine("card: {0}", deviceName);

				string serviceName = serviceKeyPath + deviceServiceName +  "\\Parameters\\Tcpip";
				RegistryKey networkKey = start.OpenSubKey(serviceName);

				if (networkKey == null)
					Console.WriteLine("    No IP configuration set");
				else 
				{
					string ipaddress = (String) networkKey.GetValue("DhcpIPAddress");
					string[] defaultGateways = (string[])networkKey.GetValue("DhcpDefaultGateway");
					string subnetmask = (string)networkKey.GetValue("DhcpSubnetMask");

					Console.WriteLine("    IP Address: {0}", ipaddress);     
					Console.WriteLine("    Subnet Mask: {0}", subnetmask);

					foreach(string defaultGateway in defaultGateways)
						Console.WriteLine("    Gateway: {0}", defaultGateway);
					networkKey.Close();
				}
			}
			start.Close();
		}
		#endregion
		
		#region GetRegistryDNS
		// If you have statically assigned DNS servers the value you're looking for is "NameServer".
		// If you get your DNS dynamically assigned via DHCP then the value you want is "DhcpNameServer"
		// Depending on how you get your DNS server(s) assigned to you one of the above values
		// will give you a list of IP address for your primary, secondary, etc, DNS servers
		// - and the other one will most likely not be there or will be blank.
		internal static string[] GetRegistryDNS() 
		{
			//Открываем необходимую нам ветвь регистра
			RegistryKey DNSServerKey = Registry.LocalMachine.OpenSubKey(
				"System\\CurrentControlSet\\Services\\Tcpip\\Parameters");

			string[] arDNSServers = null;

			string tempStr;
			//Пробуем получить значение DhcpNameServer
			try 
			{
				tempStr = DNSServerKey.GetValue("DhcpNameServer").ToString();
			}
			catch 
			{
				tempStr = "";
			}
			//Если все нормально - получаем адрес DNS сервера и возвращаем значение
			if (tempStr != null && tempStr != "")
			{
				arDNSServers = tempStr.Split(" ".ToCharArray());
				DNSServerKey.Close();
				return arDNSServers;
			}

			//Те же самые операции проделываем для значения NameServer
			try 
			{
				tempStr = DNSServerKey.GetValue("NameServer").ToString();
			}
			catch 
			{
				tempStr = "";
			}
			if (tempStr != null && tempStr != "")
			{
				arDNSServers = tempStr.Split(" ".ToCharArray());
				DNSServerKey.Close();
				return arDNSServers;
			}

			//Не нашли ни одного DNS сервера - ищем в подветвях Interfaces
			DNSServerKey.Close();
			DNSServerKey = Registry.LocalMachine.OpenSubKey(
				"System\\CurrentControlSet\\Services\\Tcpip\\Parameters\\Interfaces");
			//Получаем массив имен подветвей Interfaces Для последующей обработе в цикле
			string[] iface = DNSServerKey.GetSubKeyNames();
			for (int i = 0; i < iface.Length; i++)
			{
				//Открываем подветвь
				RegistryKey ifaceKey = DNSServerKey.OpenSubKey(iface[i]);
				string servers;
				//Пытаемся найти имя DNS сервера в значении NameServer. 
				try 
				{
					servers = ifaceKey.GetValue("NameServer").ToString();
				}
				catch 
				{
					servers = "";
				}
				if(servers != "")
				{
					arDNSServers = servers.ToString().Split(",".ToCharArray());
					if (arDNSServers[0] != "")
					{
						ifaceKey.Close();
						break;
					}
				}
				ifaceKey.Close();
			}

			DNSServerKey.Close();

			// Return server list (or empty array)
			return arDNSServers;
		}
		#endregion

		#region IPLocal
		internal static string IPLocal()
		{
			return ((Dns.GetHostByName(Dns.GetHostName()).AddressList[0]).ToString());
		}
		#endregion

		#region IPLocalList
		internal static string[] IPLocalList()
		{
			// Get the Local IP address list
			IPAddress[] LocalIPList = Dns.GetHostByName (Dns.GetHostName()).AddressList;
			string[] AddrTable = new String[LocalIPList.Length];
			for (int ii = 0; ii < LocalIPList.Length; ii++)
			{
				AddrTable[ii] = LocalIPList[ii].ToString();
			}
			return (AddrTable);
		}
		#endregion

	}
	#endregion

	#region IPCheckerSinkProvider
	public class IPCheckerSinkProvider: IServerChannelSinkProvider 
	{
		private IServerChannelSinkProvider	m_next = null;

		public IPCheckerSinkProvider(IDictionary properties, ICollection providerData)
		{
		}

		public void GetChannelData (IChannelDataStore channelData)
		{
		}

		public IServerChannelSink CreateSink (IChannelReceiver channel)
		{
			IServerChannelSink nextSink = null;
			if (m_next != null) 
			{
				nextSink = m_next.CreateSink(channel);
			}
			return new IPCheckerSink(nextSink);
		}

		public IServerChannelSinkProvider Next
		{
			get { return m_next; }
			set { m_next = value; }
		}

	}
	#endregion

	#region IPCheckerSink
	public class IPCheckerSink : BaseChannelObjectWithProperties, IServerChannelSink, IChannelSinkBase
	{	
		private IServerChannelSink _next;

		public IPCheckerSink (IServerChannelSink next) 
		{
			_next = next;
		}

		public void AsyncProcessResponse ( System.Runtime.Remoting.Channels.IServerResponseChannelSinkStack sinkStack , System.Object state , System.Runtime.Remoting.Messaging.IMessage msg , System.Runtime.Remoting.Channels.ITransportHeaders headers , System.IO.Stream stream ) 
		{
		}

		public Stream GetResponseStream ( System.Runtime.Remoting.Channels.IServerResponseChannelSinkStack sinkStack , System.Object state , System.Runtime.Remoting.Messaging.IMessage msg , System.Runtime.Remoting.Channels.ITransportHeaders headers ) 
		{
			return null;
		}
		
		public System.Runtime.Remoting.Channels.ServerProcessing ProcessMessage(System.Runtime.Remoting.Channels.IServerChannelSinkStack sinkStack, System.Runtime.Remoting.Messaging.IMessage requestMsg, System.Runtime.Remoting.Channels.ITransportHeaders requestHeaders, System.IO.Stream requestStream, out System.Runtime.Remoting.Messaging.IMessage responseMsg, out System.Runtime.Remoting.Channels.ITransportHeaders responseHeaders, out System.IO.Stream responseStream)
		{
			if (_next != null) 
			{
				object obj = requestHeaders["IPAddress"];

//				foreach (DictionaryEntry dict in requestHeaders) 
//				{
//					m_Configure.FireLogException(this, "ProcessMessage", String.Format("Client at {0}:{1}", dict.Key, dict.Value));
//					Console.WriteLine("{0}:{1}",dict.Key,dict.Value);
//				}

				string RemoteIP = requestHeaders["__IPAddress"].ToString();
				
				CallContext.SetData("ClientIPAddress", RemoteIP);

				ServerProcessing spres =  _next.ProcessMessage (sinkStack,requestMsg, requestHeaders,requestStream,out responseMsg,out responseHeaders,out responseStream);

				return spres;
			} 
			else 
			{
				responseMsg=null;
				responseHeaders=null;
				responseStream=null;
				return new ServerProcessing();
			}
		}
   
		public IServerChannelSink NextChannelSink 
		{
			get {return _next;}
			set {_next = value;}
		}
	}
	#endregion
	
	#region RemotingIDHelper
	public class RemotingIDHelper
	{
		// There are methods in the framework which allow you to retrieve the server
		// side URLs of a given object. Different methods have to be called depending
		// on whether your object is client activated or server activated. This helper
		// class works with both types of objects.
		// Note: For CAOs (which can have multiple URLS) it returns the first
		// server side URL.
		public static String GetURLForObject(MarshalByRefObject RemObj)
		{

			// Server Activated Object (SAO)?
			String Address = RemotingServices.GetObjectUri(RemObj);
			if (Address != null) return (Address);

			// CAOs ?
			ObjRef ObjRef = RemotingServices.GetObjRefForProxy(RemObj);
			if (ObjRef != null) 
			{
				foreach (object data in ObjRef.ChannelInfo.ChannelData) 
				{
					ChannelDataStore ds = data as ChannelDataStore;
					if (ds != null) 
					{
						return ds.ChannelUris[0] + ObjRef.URI;
					}
				}
			} 
			return null;
		}
	}
	#endregion

}
