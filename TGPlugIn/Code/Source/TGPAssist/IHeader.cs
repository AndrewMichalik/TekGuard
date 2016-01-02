using System;
using Outlook;

namespace TGPAssist
{
	/// <summary>
	/// Summary description for IHeaders
	/// </summary>
	public class TGPHeader
	{
		const	int		PR_TRANSPORT_MESSAGE_HEADERS  = 0x007D001E;
		const	string	FLD_MAPI	= "{00020386-0000-0000-C000-000000000046}";
		const	string	FLD_EMICAT	= "X-www.TekGuard.com-TGPlugin";
		
		// Get all headers
		// Let caller catch exceptions
		static private string x_GetHeaderFieldAll()
		{
			// Create Outlook Redemption SafeMailItem object
			Redemption.SafeMailItem SafeItem = new Redemption.SafeMailItem();
			// Get transport headers
			string Headers = SafeItem.get_Fields(PR_TRANSPORT_MESSAGE_HEADERS).ToString();
			// Release objects
			SafeItem = null;
			return (Headers);
		}
			
		// This field will show up on its own line in the Internet Headers of the outgoing message
		// Let caller catch exceptions
		static internal void TGPSetHeaderField(Outlook.MailItem oMessage, string sFieldValue)
		{
			// Create Outlook Redemption Utility object
//ajm			Redemption.MAPIUtils oRedemptionUtils = new Redemption.MAPIUtils();

			// Map the Internet Header field name into a property Id
			int PrCategories = TGPOutlookApp.MAPIUtils.GetIDsFromNames(oMessage.MAPIOBJECT, FLD_MAPI, FLD_EMICAT, true);

			// make sure we have the right property type - PT_STRING8 (0x001E)
			PrCategories = PrCategories | 0x001E;

			// Set the property
			TGPOutlookApp.MAPIUtils.HrSetOneProp(oMessage.MAPIOBJECT, PrCategories, sFieldValue, false);
		
			// Clean up references
//ajm			oRedemptionUtils.Cleanup();
//ajm			oRedemptionUtils = null;
		}

		// Outlook parses the Internet Headers of an incoming email message and maps
		// them to MAPI fields. Access a custom field from the Internet Headers
		// Let caller catch exceptions
		static internal string TGPGetHeaderField(Outlook.MailItem oMessage, out string TGPMarker)
		{
			// Create Outlook Redemption Utility object
//ajm			Redemption.MAPIUtils oRedemptionUtils = new Redemption.MAPIUtils();

			// Get all the headers
			string Headers = (string) TGPOutlookApp.MAPIUtils.HrGetOneProp(oMessage.MAPIOBJECT, PR_TRANSPORT_MESSAGE_HEADERS);

			// Map the Internet Header field name into a property ID
			int PrCategories = TGPOutlookApp.MAPIUtils.GetIDsFromNames(oMessage.MAPIOBJECT, FLD_MAPI, FLD_EMICAT, false);

			// Force the right property type - PT_STRING8 (0x001E)
			PrCategories = PrCategories | 0x001E;

			// Get the TGPlugin Marker Field
			TGPMarker = (string) TGPOutlookApp.MAPIUtils.HrGetOneProp(oMessage.MAPIOBJECT, PrCategories);

			// Clean up references
//ajm			oRedemptionUtils.Cleanup();
//ajm			oRedemptionUtils = null;
			return (Headers);

		}


	}
}
