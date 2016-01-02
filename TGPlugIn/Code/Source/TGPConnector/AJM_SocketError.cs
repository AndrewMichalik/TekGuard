using System;
using System.Net.Sockets;

namespace TGPConnector
{
	/// <summary>
	/// Summary description for SocketError.
	/// </summary>
	public class SocketError
	{
		public SocketError()
		{
			//
			// TODO: Add constructor logic here
			//
			try
			{
				// code that causes a SocketException
			}
			catch(SocketException se)
			{
				SocketErrorCodes errorCode = (SocketErrorCodes)se.ErrorCode;
    
				switch(errorCode)
				{
					case SocketErrorCodes.PermissionDenied:
						// error handling
						break;

					case SocketErrorCodes.AddressInUse:
						// error handling
						break;

						// etc..
				}
			}

		}
	}

	/// <summary>
	/// Summary description for SocketError.
	/// </summary>
	public enum SocketErrorCodes
	{
		InterruptedFunctionCall         = 10004,
		PermissionDenied                = 10013,
		BadAddress                      = 10014,
		InvalidArgument                 = 10022,
		TooManyOpenFiles                = 10024,
		ResourceTemporarilyUnavailable  = 10035,
		OperationNowInProgress          = 10036,
		OperationAlreadyInProgress      = 10037,
		SocketOperationOnNonSocket      = 10038,
		DestinationAddressRequired      = 10039,
		MessgeTooLong                   = 10040,
		WrongProtocolType               = 10041,
		BadProtocolOption               = 10042,
		ProtocolNotSupported            = 10043,
		SocketTypeNotSupported          = 10044,
		OperationNotSupported           = 10045,
		ProtocolFamilyNotSupported      = 10046,
		AddressFamilyNotSupported       = 10047,
		AddressInUse                    = 10048,
		AddressNotAvailable             = 10049,
		NetworkIsDown                   = 10050,
		NetworkIsUnreachable            = 10051,
		NetworkReset                    = 10052,
		ConnectionAborted               = 10053,
		ConnectionResetByPeer           = 10054,
		NoBufferSpaceAvailable          = 10055,
		AlreadyConnected                = 10056,
		NotConnected                    = 10057,
		CannotSendAfterShutdown         = 10058,
		ConnectionTimedOut              = 10060,
		ConnectionRefused               = 10061,
		HostIsDown                      = 10064,
		HostUnreachable                 = 10065,
		TooManyProcesses                = 10067,
		NetworkSubsystemIsUnavailable   = 10091,
		UnsupportedVersion              = 10092,
		NotInitialized                  = 10093,
		ShutdownInProgress              = 10101,
		ClassTypeNotFound               = 10109,
		HostNotFound                    = 11001,
		HostNotFoundTryAgain            = 11002,
		NonRecoverableError             = 11003,
		NoDataOfRequestedType           = 11004
	}

	public enum WinSockErrorCodes
	{
		WSAEINTR           = 10004,
		WSAEACCES          = 10013,
		WSAEFAULT          = 10014,
		WSAEINVAL          = 10022,
		WSAEMFILE          = 10024,
		WSAEWOULDBLOCK     = 10035,
		WSAEINPROGRESS     = 10036,
		WSAEALREADY        = 10037,
		WSAENOTSOCK        = 10038,
		WSAEDESTADDRREQ    = 10039,
		WSAEMSGSIZE        = 10040,
		WSAEPROTOTYPE      = 10041,
		WSAENOPROTOOPT     = 10042,
		WSAEPROTONOSUPPORT = 10043,
		WSAESOCKTNOSUPPORT = 10044,
		WSAEOPNOTSUPP      = 10045,
		WSAEPFNOSUPPORT    = 10046,
		WSAEAFNOSUPPORT    = 10047,
		WSAEADDRINUSE      = 10048,
		WSAEADDRNOTAVAIL   = 10049,
		WSAENETDOWN        = 10050,
		WSAENETUNREACH     = 10051,
		WSAENETRESET       = 10052,
		WSAECONNABORTED    = 10053,
		WSAECONNRESET      = 10054,
		WSAENOBUFS         = 10055,
		WSAEISCONN         = 10056,
		WSAENOTCONN        = 10057,
		WSAESHUTDOWN       = 10058,
		WSAETIMEDOUT       = 10060,
		WSAECONNREFUSED    = 10061,
		WSAEHOSTDOWN       = 10064,
		WSAEHOSTUNREACH    = 10065,
		WSAEPROCLIM        = 10067,
		WSASYSNOTREADY     = 10091,
		WSAVERNOTSUPPORTED = 10092,
		WSANOTINITIALIZED  = 10093,
		WSAEDISCON         = 10101,
		WSATYPE_NOT_FOUND  = 10109,
		WSAHOST_NOT_FOUND  = 11001,
		WSATRY_AGAIN       = 11002,
		WSANO_RECOVERY     = 11003,
		WSANO_DATA         = 11004
	}
}
