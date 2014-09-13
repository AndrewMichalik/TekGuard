// Sample TekGuard MailServer database driven configuration code
// Contact Vector Information Systems, Inc (www.VInfo.com) for
// information regarding our commercial and SQL Server versions.
using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Windows.Forms;
using Microsoft.Win32;

namespace TGMailServer
{
	/// <summary>
	/// Summary description for ProjectInstaller.
	/// </summary>
	[RunInstaller(true)]
	public class AppInstaller : System.Configuration.Install.Installer
	{
		private System.ServiceProcess.ServiceProcessInstaller	serviceProcessInstaller;
		private System.ServiceProcess.ServiceInstaller			serviceInstaller;
		private System.ComponentModel.Container					components = null;
		internal const string									TGMSERVICE_NAME = "TGMailServer";
		internal const string									TGMDISPLAY_NAME = "TekGuard MailServer";

		#region Constructors / Destructors
		public AppInstaller()
		{
			// This call is required by the Designer.
			InitializeComponent();

			// Surpress warning about unused "required designer variable"
			components = components;
		}
		#endregion

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.serviceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
			this.serviceInstaller = new System.ServiceProcess.ServiceInstaller();
			// 
			// serviceProcessInstaller
			// 
			this.serviceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
			this.serviceProcessInstaller.Password = null;
			this.serviceProcessInstaller.Username = null;
			// 
			// serviceInstaller
			// 
			this.serviceInstaller.DisplayName = TGMDISPLAY_NAME;
			this.serviceInstaller.ServiceName = TGMSERVICE_NAME;
			// 
			// AppInstaller
			// 
			this.Installers.AddRange(new System.Configuration.Install.Installer[] {
																					  this.serviceProcessInstaller,
																					  this.serviceInstaller});

		}
		#endregion
	
		public override void Commit(IDictionary stateServer)
		{   
			// RegistryKey ckey = Registry.LocalMachine;
			try
			{
				// Let the project installer finish its job
				base.Commit(stateServer);

				// DotNet 1.0 & 1.1 work-around
				TGMSupport.ServiceReg.ChangeServiceType (this.serviceInstaller.ServiceName, TGMSupport.ServiceControlType.OwnProcess |
					TGMSupport.ServiceControlType.InteractiveProcess );

				// DotNet serviceinstaller does not set ancillary values
				// Unfortunately, setting the Type changes the registry, but does appear to commit the registry change.
				// For example, after the install, the flag is set but I still have to go into the service console
				// and set, unset and apply "Allow service to interact with desktop".
				// Open the service key for editing under HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\TGMailServer
				// ckey = ckey.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\");
				// ckey = ckey.OpenSubKey(this.serviceInstaller.ServiceName, true);
				// if (ckey != null)
				// {
				// 	// Make sure the "Type" value is there, and then do a bitwise operation on it.
				// 	// Set the "Allow service to interact with desktop"
				// 	if(ckey.GetValue("Type") != null)
				// 	{
				// 		ckey.SetValue("Type", ((int) ckey.GetValue("Type") | 0x00000100));
				// 	}
				// }
				// ckey.Close();
			}
			catch(Exception ex)
			{
				MessageBox.Show (TGMSERVICE_NAME + " installation Error: " + ex.Message);
				// ckey.Close();
			}
		}
	
	}
}
