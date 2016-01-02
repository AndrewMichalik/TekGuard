using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.IO;
using System.Windows.Forms;
using Microsoft.Office.Core;

namespace TGPAssist
{
	/// <summary>
	/// Summary description for TGPOutlookApp
	/// </summary>
	public class TGPOutlookApp
	{
		private	static	Assist					m_Assist;			// Pointer to parent TGPAssist instance
		private static	Outlook.Application		m_OutlookApp;		// Pointer to parent Outlook Application
		private static	Redemption.MAPIUtils	m_RedemptionUtils;	// Redemption helper utility reference

		// Use a local main thread, otherwise MAPI objects are not created
		// and disposed of properly, and application hangs on close.
		private	static	Control					m_CtlInMainThread;	// Control in main thread for callback

		#region Constructors / Destructors
		public TGPOutlookApp(object OutlookApp, Assist Assist)
		{
			// Create Outlook COM objects
			m_OutlookApp	= (Outlook.Application) OutlookApp;

			// Set pointer to parent instance
			m_Assist = Assist;

			// Create Outlook Redemption Utility object
			m_RedemptionUtils = new Redemption.MAPIUtils();

			// Local thread
			m_CtlInMainThread = new Control();
			m_CtlInMainThread.CreateControl();
		}
		~TGPOutlookApp()
		{
			// Release all COM objects
			Dispose();
		}
		public void Dispose()
		{
			try 
			{
				// Release all COM objects
				// DisposeObject(m_oNameSpace);
				DisposeObject(m_OutlookApp);
				DisposeObject();

				// Clean up Redemption references
				if (m_RedemptionUtils != null)
				{
					m_RedemptionUtils.Cleanup();
					m_RedemptionUtils = null;
				}
			}
			catch {}
		}
		#endregion

		#region Enabled
		public bool Enabled
		{
			get 
			{
				return (CheckLoad.Check());
			}
		}
		#endregion

		#region OutlookApp
		/// <summary>
		/// Summary description for OutlookApp
		/// </summary>
		public Outlook.Application OutlookApp
		{
			get
			{
				return (m_OutlookApp);
			}
		}
		#endregion

		#region MAPIUtils (internal)
		/// <summary>
		/// Summary description for MAPIUtils
		/// </summary>
		internal static Redemption.MAPIUtils MAPIUtils
		{
			get
			{
				return (m_RedemptionUtils);
			}
		}
		#endregion

		/*
			From http://support.microsoft.com/default.aspx?scid=kb;en-us;q290500
			A dialog box is displayed that asks you to confirm access to this information.

			For the following properties of a MailItem object:
			SentOnBehalfOfName
			SenderName
			ReceivedByName
			ReceivedOnBehalfOfName
			ReplyRecipientNames
			To
			Cc
			Bcc 
		*/

		#region TGPExplorers
		/// <summary>
		/// Summary description for TGPExplorers
		/// </summary>
		public class TGPExplorers
		{
			internal	Outlook.Explorers	m_objExplorers;

			#region Constructors / Destructors
			public TGPExplorers(Outlook.Explorers objExplorers)
			{
				m_objExplorers = objExplorers;
			}
			~TGPExplorers()
			{
				DisposeObject(m_objExplorers);
			}
			#endregion

		}
		#endregion

		#region TGPExplorer
		/// <summary>
		/// Summary description for TGPExplorer
		/// </summary>
		public class TGPExplorer
		{
			protected	Outlook.Explorer	m_objExplorer;

			#region Constructors / Destructors
			public TGPExplorer(object objExplorer)
			{
				m_objExplorer = (Outlook.Explorer) objExplorer;
			}
			~TGPExplorer()
			{
				// Release all COM objects
				DisposeObject(m_objExplorer);
			}
			#endregion

			#region TGPCommandBar
			/// <summary>
			/// Summary description for TGPCommandBar
			/// </summary>
			public class TGPCommandBar
			{
				private	CommandBars	m_objCommandBars;
				private	CommandBar	m_objCommandBar;

				#region Constructors / Destructors
				public TGPCommandBar(string BarName)
				{
					try
					{
						// Outlook has the CommandBars collection on the Explorer object.
						TGPExplorer TGActiveExplorer = new TGPExplorer (m_OutlookApp.GetType().InvokeMember("ActiveExplorer", BindingFlags.GetProperty, null, m_OutlookApp, null));
						m_objCommandBars = (CommandBars)TGActiveExplorer.Get.GetType().InvokeMember("CommandBars", BindingFlags.GetProperty, null, TGActiveExplorer.Get, null);
						
						// Set up a custom button on the designated commandbar.
						m_objCommandBar = m_objCommandBars[BarName];
					}
					catch 
					{
					}
				}
				~TGPCommandBar()
				{
					// Release all COM objects
					DisposeObject(m_objCommandBar);
					DisposeObject(m_objCommandBars);
				}
				#endregion

				#region TGPButton
				/// <summary>
				/// Summary description for TGPButton
				/// </summary>
				public class TGPButton
				{
					protected	CommandBarButton	m_objCommandBarButton;

					#region Constructors / Destructors
					public TGPButton(CommandBarButton objCommandBarButton)
					{
						m_objCommandBarButton = objCommandBarButton;
					}
					~TGPButton()
					{
						// Release all COM objects
						DisposeObject(m_objCommandBarButton);
					}
					#endregion

					#region Properties
					public CommandBarButton Get
					{
						get
						{
							return (m_objCommandBarButton);
						}
					}
					public CommandBarButton Set
					{
						set
						{
							m_objCommandBarButton = value;
						}
					}
					#endregion

				}
				#endregion

				#region Properties
				public CommandBar Get
				{
					get
					{
						return (m_objCommandBar);
					}
				}
				public CommandBar Set
				{
					set
					{
						m_objCommandBar = value;
					}
				}
				#endregion

			}
			#endregion

			#region TGPItems
			/// <summary>
			/// Summary description for TGPItems
			/// </summary>
			public class TGPItems : IDisposable 
			{
				//protected	TGPExplorer			TGPExplorer;
				protected	Outlook.Explorer	m_objExplorer;
				protected	Outlook.Selection	m_objSelection;
				protected	Outlook.NameSpace	m_objNameSpace;
				protected	Outlook.MAPIFolder	m_objFolder; 
				protected	Outlook.Items		m_objItems;

				#region Constructors / Destructors
				public TGPItems(Outlook.OlDefaultFolders FolderType)
				{
					try
					{
						// create a MAPI Folder Object of the Inbox 
						m_objNameSpace	= m_OutlookApp.GetNamespace("MAPI");
						m_objNameSpace.Logon ("", Missing.Value, false, false);
						m_objFolder		= m_objNameSpace.GetDefaultFolder(FolderType);
						m_objItems		= m_objFolder.Items;
					}
					catch 
					{
					}
				}
				public TGPItems()
				{
					try
					{
						//TGPExplorer = new TGPExplorer(m_OutlookApp.ActiveExplorer());
						//m_objItems = TGPExplorer.Get.Selection;
						m_objExplorer = m_OutlookApp.ActiveExplorer();
						m_objSelection = m_objExplorer.Selection;
					}
					catch 
					{
					}
				}
				~TGPItems()
				{
					// Release all COM objects
					Dispose();
				}
				void IDisposable.Dispose()
				{
					// Release all COM objects
					Dispose();
					GC.SuppressFinalize(this);  // Finalization is now unnecessary
				}
				public void Dispose()
				{
					// Release all COM objects
					DisposeObject(m_objSelection);
					DisposeObject(m_objExplorer);
					DisposeObject(m_objItems);
					DisposeObject(m_objFolder);
					DisposeObject(m_objNameSpace);
				}
				#endregion

				#region this[int index]
				public Outlook.MailItem this[int index]
				{
					get
					{
						// Valid index?
						if ((index > 0) && (index <= m_objSelection.Count))
						{
							// Make sure this is a valid mail item
							if (!(m_objSelection.Item(index) is Outlook.MailItem)) return (null);
							
							// Index OK, return mail item
							return ((Outlook.MailItem)m_objSelection.Item(index));
						}
						// Bad index
						// m_Analyzer.FireLogException(this, "TokenEntry <" + index + ">", e.Message);
						return (null);
					}
				}
				#endregion

				#region Properties

				#region Restrict
				public string Restrict
				{
					set 
					{
						// TGPItems[ii] = TGPItems[ii].Restrict("[Unread] = true");
					}
				}
				#endregion

				#endregion

				#region TGPHeader
				public static string TGPHeaderGet (Outlook.MailItem MailItem, out Int32 PreviousID)
				{
					//AJM To-Do: Break this into separate funtions for TGP headers and all headers
					try
					{
						string TGPMarker;
						string Headers = TGPHeader.TGPGetHeaderField(MailItem, out TGPMarker);
						PreviousID = IMarkUp.ExtractCategory (TGPMarker);
						return (Headers);
					}
					catch (Exception ex)
					{
						m_Assist.FireLogException(ex, null, null);
						PreviousID = 0;
						return (null);
					}
				}
				public static bool TGPHeaderSet (Outlook.MailItem MailItem, Int32 UserID, Int32 CategoryID, Int32 PreviousID, bool bExplicit)
				{
					try
					{
						TGPHeader.TGPSetHeaderField(MailItem, IMarkUp.Mark(UserID, CategoryID, PreviousID, bExplicit));
						return (true);
					}
					catch (Exception ex)
					{
						m_Assist.FireLogException(ex, "MailItem", MailItem.ToString());
						return (false);
					}
				}
				#endregion

				#region SenderName
				public static string SenderName (Outlook.MailItem MailItem)
				{
					try
					{
						string Name = null;
						// Create an instance of Redemption.SafeMailItem
						Redemption.SafeMailItem SafeItem = new Redemption.SafeMailItem();
						// Set Item property
						SafeItem.Item = MailItem;
						// Get name
						Name = SafeItem.SenderName;
						// Release objects
						SafeItem = null;
						return (Name);
					}
					catch (Exception ex)
					{
						m_Assist.FireLogException(ex, null, null);
						try {return (MailItem.SenderName.ToString());} 
						catch {return "";}
					}
				}
				#endregion

				#region SenderAddress
				public static string SenderAddress (Outlook.MailItem MailItem)
				{
					try
					{
						string Address = null;
						// Create an instance of Redemption.SafeMailItem
						Redemption.SafeMailItem SafeItem = new Redemption.SafeMailItem();
						// Set Item property
						SafeItem.Item = MailItem;
						// Get address
						const int PR_SENDER_ADDRTYPE = 0xC1E001E;
						string Type = SafeItem.get_Fields(PR_SENDER_ADDRTYPE).ToString();
						if (Type.ToUpper() == "SMTP") 
						{
							const int PR_EMAIL = 0xC1F001E;
							Address = SafeItem.get_Fields(PR_EMAIL).ToString();
						}
						else if (Type.ToUpper() == "EX") 
						{
							Address = SafeItem.SenderName;
						}
						// Release objects
						SafeItem = null;
						return (Address);
					}
					catch (Exception ex)
					{
						m_Assist.FireLogException(ex, null, null);
						try {return (MailItem.SenderName.ToString());} 
						catch {return "";}
					}
				}
				#endregion

				#region Body
				public static string Body(Outlook.MailItem MailItem)
				{

					try
					{
						// Create an instance of Redemption.SafeMailItem
						Redemption.SafeMailItem SafeItem = new Redemption.SafeMailItem();
						// Set Item property
						SafeItem.Item = MailItem;
						// Get body
						string Content = SafeItem.Body + "/n" + SafeItem.HTMLBody;
						// Get headers
						const int PR_TRANSPORT_MESSAGE_HEADERS = 0x7D001E;
						Content = SafeItem.get_Fields(PR_TRANSPORT_MESSAGE_HEADERS).ToString() + "/n" + Content;
						// Release objects
						SafeItem = null;
						return (Content);
					}
					catch (Exception ex)
					{
						m_Assist.FireLogException(ex, null, null);
						try {return (MailItem.Body + "/n" + MailItem.HTMLBody);}
						catch {return "";}
					}
				}

				#endregion

				#region xTGMailItem (old, now private)
				/// <summary>
				/// Summary description for TGMailItem
				/// </summary>
				private class xTGMailItem
				{
					protected	Outlook.MailItem	m_objMailItem;

					#region Constructors / Destructors
					public xTGMailItem(object MailItem)
					{
						try
						{
							// Make sure this is a valid mail item
							if (!(MailItem is Outlook.MailItem)) 
							{
								m_objMailItem = null;
								return;
							}

							// Save Original
							m_objMailItem = (Outlook.MailItem) MailItem;
						}
						catch 
						{
						}
					}
					~xTGMailItem()
					{
						// Release all COM objects
						DisposeObject(m_objMailItem);
					}
					#endregion

					#region Properties

					#region xSenderName
					public string xSenderName
					{
						get 
						{
							try {return (m_objMailItem.SenderName.ToString());} 
							catch {return "";}
						}
					}
					#endregion

					#region xSenderAddress
					public string xSenderAddress
					{
						get 
						{
							// AJM To Do: Get real "from" address
							try {return (m_objMailItem.SenderName.ToString());} 
							catch {return "";}
						}
					}
					#endregion

					#region xSubject
					public string xSubject
					{
						get 
						{
							try {return (m_objMailItem.Subject.ToString());} 
							catch {return "";}
						}
						set 
						{
							try {m_objMailItem.Subject = value;} 
							catch {}
						}
					}
					#endregion

					#region xUnRead
					public bool xUnRead
					{
						get {return (m_objMailItem.UnRead);}
						set {m_objMailItem.UnRead = value;}
					}

					#endregion

					#region xCategories
					public string xCategories
					{
						get 
						{
							try {return (m_objMailItem.Categories.ToString());} 
							catch {return "";}
						}
						set 
						{
							try {m_objMailItem.Categories = value;} 
							catch {}
						}
					}
					#endregion

					#region xBody
					public string xBody 
					{
						get 
						{
							string Content = null;
							try 
							{
								string sBody = m_objMailItem.Body;
								string sHTML = m_objMailItem.HTMLBody;
								Content = sBody + "/n" + sHTML;
				
								// AJM To Do: Get real "from" address
								// This hangs periodically:
								// Outlook.MailItem tmpReply = MailItem.Reply();
								// SenderAddress = tmpReply.To;
								// Marshal.ReleaseComObject(tmpReply);
								// tmpReply = null;
							}
							catch
							{
							}
							return (Content);
						}
					}
					#endregion

					private Outlook.MailItem xGet
					{
						get
						{
							return (m_objMailItem);
						}
					}
					private Outlook.MailItem xSet
					{
						set
						{
							m_objMailItem = value;
						}
					}
					#endregion

					#region xSave
					public bool xSave()
					{
						try {m_objMailItem.Save(); return(true);} 
						catch {return(false);}
					}
					#endregion

				}
				#endregion

				#region Items
				public Outlook.Items Items
				{
					get
					{
						return (m_objItems);
					}
				}
				#endregion

				#region Selection
				public Outlook.Selection Selection
				{
					get
					{
						return (m_objSelection);
					}
				}
				#endregion

			}
			#endregion

			#region Properties
			public Outlook.Explorer Get
			{
				get
				{
					return (m_objExplorer);
				}
			}
			public Outlook.Explorer Set
			{
				set
				{
					m_objExplorer = value;
				}
			}
			#endregion

		}

		#endregion

		#region xTGFolder (old, now private)
		/// <summary>
		/// Summary description for TGFolder
		/// </summary>
		private class xTGFolder : IDisposable 
		{
			protected Outlook.NameSpace m_objNameSpace;
			protected Outlook.MAPIFolder m_objFolder; 

			#region Constructors / Destructors
			public xTGFolder(Outlook.OlDefaultFolders FolderType)
			{
				try
				{
					// create a MAPI Folder Object of the selected mailbox 
					m_objNameSpace = m_OutlookApp.GetNamespace("MAPI");
					m_objNameSpace.Logon ("", Missing.Value, false, false);
					m_objFolder  = m_objNameSpace.GetDefaultFolder(FolderType);
				}
				catch 
				{
				}
			}
			public xTGFolder(Outlook.OlDefaultFolders FolderType, string FolderName)
			{
				// create a MAPI Folder Object of the selected mailbox 
				m_objNameSpace = m_OutlookApp.GetNamespace("MAPI");
				m_objNameSpace.Logon ("", Missing.Value, false, false);
				try
				{
					m_objFolder  = m_objNameSpace.GetDefaultFolder(FolderType).Folders.Item(FolderName);
				}
				catch 
				{
					m_objFolder  = m_objNameSpace.GetDefaultFolder(FolderType).Folders.Add(FolderName, FolderType);
				}
			}
			~xTGFolder()
			{
				// Release all COM objects
				Dispose();
			}
			void IDisposable.Dispose()
			{
				// Release all COM objects
				Dispose();
				GC.SuppressFinalize(this);  // Finalization is now unnecessary
			}
			public void Dispose()
			{
				// Release all COM objects
				DisposeObject(m_objFolder);
				DisposeObject(m_objNameSpace);
			}
			public Outlook.MAPIFolder Value
			{
				get { return m_objFolder; }
			}
			#endregion

		}
		#endregion

		#region CreateFolder
		public static void CreateFolder(Outlook.OlDefaultFolders Parent, string MailBoxName, out Outlook.MAPIFolder fldNew)
		{
			// Get or create the folder
			try
			{
				fldNew = m_OutlookApp.Application.Session.GetDefaultFolder(Parent).Folders.Item(MailBoxName);
			}
			catch 
			{
				// Create folder
				fldNew = m_OutlookApp.Application.Session.GetDefaultFolder(Parent).Folders.Add(MailBoxName, Parent);
			}
		
			try
			{
				Outlook.Explorer ActiveExplorer = m_OutlookApp.Application.ActiveExplorer();

				// Save original current folder
				Outlook.MAPIFolder fldCurrent = ActiveExplorer.CurrentFolder;

				// Get parent folder settings
				ActiveExplorer.CurrentFolder = m_OutlookApp.Application.Session.GetDefaultFolder(Parent);
				bool bVisible = ActiveExplorer.IsPaneVisible(Outlook.OlPane.olPreview);

				// Copy parent characteristics
				ActiveExplorer.CurrentFolder = fldNew;
				ActiveExplorer.ShowPane (Outlook.OlPane.olPreview, bVisible);
				
				// Restore original current folder
				ActiveExplorer.CurrentFolder = fldCurrent;

				// Dispose of objects
				TGPOutlookApp.DisposeObject (fldCurrent);
				TGPOutlookApp.DisposeObject (ActiveExplorer);
				TGPOutlookApp.DisposeObject ();
			}
			catch 
			{
			}
		
		}
		#endregion
		
		#region Invoke
		public object Invoke(OutlookInvokeEventHandler EventHandler, object args)
		{

			try
			{
				// Pack the "Invoke" arguments
				object[] objArray = new object[1];
				objArray[0] = new object[] {EventHandler, args};

				// Need to use local main thread, otherwise MAPI objects are not created
				// and disposed of properly, and application hangs on close.
				return(m_CtlInMainThread.Invoke(new OutlookInvokeLocalHandler(Invoke_Catch), objArray));
			}
			catch 
			{
				return (null);
			}
		}
		private object Invoke_Catch(object[] objArray)
		{
			try
			{
				// Unpack "Invoke" arguments
				if (objArray == null) return (null);

				// Call locally threaded invocation function
				return ((OutlookInvokeEventHandler)objArray[0])(objArray[1]);
			}
			catch (Exception ex)
			{
				ex = ex;
				return (null);
			}
		}
		#endregion
		
		#region DisposeObject
		public static void DisposeObject(object obj)
		{
			try
			{
				if (obj != null)
				{
					while(Marshal.ReleaseComObject(obj) > 0);
				}
			}
			catch (SystemException ex)
			{
				m_Assist.FireLogException(ex, null, null);
			}
			finally
			{
				obj = null;
			}
		}
		public static void DisposeObject()
		{
			try
			{
				// AJM 12/2004: Occasionally get "Outlook - Application Exception"
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}
			catch (SystemException ex)
			{
				m_Assist.FireLogException(ex, null, null);
			}
		}
		#endregion

	}

	#region Event Delegates
	internal delegate object OutlookInvokeLocalHandler(object[] objArray);
	public	 delegate object OutlookInvokeEventHandler(object args);
	#endregion

}
