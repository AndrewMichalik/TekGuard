using System;
using System.Text.RegularExpressions;

namespace TGPAssist
{
	/// <summary>
	/// Summary description for MarkUp
	/// </summary>
	internal class IMarkUp
	{
		// private	const string KEY_CATNAME	= "CNM=";
		private	const string KEY_CATID		= "CID=";
		private	const string KEY_USER		= "User=";
		private	const string KEY_GLOBALID	= "GID=000000";
		private	const string KEY_EMIVER		= "TGP=0.7.9";
		private	const string KEY_SOURCE		= "Source=";
		private	const string VAL_MARKED		= "Marked";
		private	const string VAL_ANALYZED	= "Analyzed";
		private	const string KEY_PRVCATID	= "PrvCID=";
		private	const string KEY_DELIM		= ";";

		public static string Mark(Int32 UserID, Int32 CategoryID, Int32 PreviousID, bool bExplicit)
		{
			// Build the new TekGuard EMI marker string
			string TGPMarker = "[" + KEY_USER + UserID.ToString() + KEY_DELIM + KEY_CATID + CategoryID.ToString() + KEY_DELIM + KEY_GLOBALID + KEY_DELIM + KEY_EMIVER + KEY_DELIM + KEY_SOURCE + (bExplicit ? VAL_MARKED : VAL_ANALYZED) + KEY_DELIM + KEY_PRVCATID + PreviousID.ToString() + KEY_DELIM + "]";
			return(TGPMarker);
		}

		private static string ExtractMarker(ref string TGPMarker)
		{
			try 
			{
				// [Usr=1;CID=1;GID=000000;EMI=0.7.9;Class=Marked;PrvCID=0;]
				// Match left bracket, least of any number of characters,
				// "EMI=", least of any number of characters, closing bracket:
				Regex objRegex = new Regex (@"\[.*?EMI=.*?\]", RegexOptions.IgnoreCase);

				// Look for the delimited string (first occurence only)
				MatchCollection objMatch = objRegex.Matches(TGPMarker);

				// Anything found?
				if (objMatch.Count == 0) return ("");

				// Remove the old category designator  (first occurence only)
				TGPMarker = (objRegex.Replace(TGPMarker, ""));
				return (objMatch[0].ToString());
			}
			catch
			{
				return (null);
			}

		}

		public static Int32 ExtractCategory(string TGPMarker)
		{
			// Anything to do?
			if (TGPMarker == null) return (0);
			
			// Extract TGPlugin Marker
			try 
			{
				// Strip leading and trailing brackets
				if (TGPMarker.StartsWith("["))	TGPMarker = TGPMarker.Substring(1);
				if (TGPMarker.EndsWith("]"))	TGPMarker = TGPMarker.Substring(0, TGPMarker.Length - 1);

				// [Usr=1;CID=1;GID=000000;EMI=0.7.9;Class=Marked;PrvCID=0;]
				// Split on KEY_DELIM (semicolon)
				Regex objRegex = new Regex (KEY_DELIM, RegexOptions.IgnoreCase);

				// Find Category string ID
				foreach (string Category in objRegex.Split(TGPMarker))
				{
					// Convert Category string to an ID value
					if (Category.ToString().StartsWith(KEY_CATID))
						try
						{
							return (Convert.ToInt32(Category.Substring(KEY_CATID.Length)));
						}
						catch
						{
							return (0);
						}
				}
			}
			catch {}
			return (0);

		}

	}
}



