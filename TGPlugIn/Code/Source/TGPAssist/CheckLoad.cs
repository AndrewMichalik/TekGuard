using System;
using System.Reflection;

namespace TGPAssist
{
	/// <summary>
	/// Summary description for CheckLoad.
	/// </summary>
	public class CheckLoad
	{
		private static	System.Type		m_comType = null;
		private static	object			m_comInstance = null;
		private static	MethodInfo		m_comMethod = null;
		private static	bool			m_isLoaded = false;

		private	const	string			TYPENAME	= "Redemption.MAPIUtils";
		private	const	string			METHOD		= "HrSetOneProp";

		public static bool Check()
		{
			// Default to Redemption (for testing)
			string TypeName = TYPENAME;

			try
			{
				// try loading COM typelib by its prog id (COM fully classified (server.class) name)
				System.Type comType = Type.GetTypeFromProgID(TypeName, true); 
					
				// COM Library Registry Check succeeded
				return (true);
			}
			catch (Exception ex)
			{
				// TypeLib can't be found, COM Library Registry Check failed
				string Error = "TypeLib for class '" + TypeName + "' cannot be loaded from Registry. " + ex;
				return (false);
			}

		}
		
		public static void Load()
		{
			string TypeName = TYPENAME;
			string Method = METHOD;
			System.Type comType = null;
			object comInstance = null;
			object comMethod = null;

			try
			{
				// if already loaded, leave
				if (m_isLoaded) return;

				try
				{
					// try loading COM typelib by its prog id (COM fully classified (server.class) name)
					comType = Type.GetTypeFromProgID(TypeName, true); 
					
					// COM Library Registry Check succeeded
				}
				catch (Exception ex)
				{
					// TypeLib can't be found, COM Library Registry Check failed
					throw new Exception("TypeLib for class '" + TypeName + "' cannot be loaded from Registry.", ex);
				}

				try
				{
					// Create instance from type
					comInstance = Activator.CreateInstance(comType);
					
					// COM Library Load Check succeeded
				}
				
				catch (Exception ex)
				{
					// COM Library Load Check failed

					// TypeLib can't be found
					throw new Exception("Class '" + TypeName + "' was found in Registry but an instance cannot be created. The file '" + comType.Assembly.Location + "' may be missing.", ex);
				}

				// get pointer to method
				try
				{
					comMethod = comType.GetMethod(Method); 
					
					// COM Library Compatibility Check succeeded
				}
				catch (Exception ex)
				{
					// COM Library Compatibility Check failed

					throw new Exception("Method " + Method + " is missing. Possible incompatible version of file '" + comType.Assembly.Location + "'.", ex);
				}

				// store type and instance for later use
				m_comType = comType;
				m_comInstance = comInstance;
				m_comMethod = m_comType.GetMethod(Method); 

				m_isLoaded = true;
			}
			catch (Exception ex)
			{
				// clear all in case it got partially loaded
				m_comType = null;
				m_comInstance = null;
				m_comMethod = null;

				// reset loaded flag
				m_isLoaded = false;

				// unable to load COM object.  tell user 
				throw ex;
			}
		}

		static public void HrSetOneProp(Outlook.MailItem oMessage, string sFieldValue)
		{
			try
			{
				Load();

				// Prepare parameters for HrSetOneProp
				object[] parameters = new object[]
				{
					oMessage,
					sFieldValue
				};
				
				// Call the method
				object validateResult = m_comMethod.Invoke(m_comInstance, parameters);
			
				// Cast value to string
				string resultValue = (string) validateResult;
			}
			catch (Exception ex)
			{
				// COM Library Execution Check failed
				throw ex;
			}
		}

	
	}
}
