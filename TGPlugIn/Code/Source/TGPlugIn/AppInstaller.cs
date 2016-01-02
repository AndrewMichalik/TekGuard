using System;
using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using System.Runtime.InteropServices;

namespace TGPlugIn
{
	/// <summary>
	/// Summary description for ComInstaller.
	/// </summary>
	[RunInstaller(true)]
	public class AppInstaller : Installer
	{
		private		System.ComponentModel.IContainer	components;

		public override void Install(System.Collections.IDictionary
			stateSaver)
		{
			base.Install(stateSaver);

			RegistrationServices regsrv = new RegistrationServices();
			if (!regsrv.RegisterAssembly(this.GetType().Assembly,
				AssemblyRegistrationFlags.SetCodeBase))
			{
				throw new InstallException("Failed To Register for COM");
			}
		}

		public override void Uninstall(System.Collections.IDictionary
			savedState)
		{
			base.Uninstall(savedState);

			RegistrationServices regsrv = new RegistrationServices();
			if (!regsrv.UnregisterAssembly(this.GetType().Assembly))
			{
				throw new InstallException("Failed To Unregister for COM");
			}
		}

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}
		#endregion
	}
}
