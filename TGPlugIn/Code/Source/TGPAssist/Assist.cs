using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace TGPAssist
{
	/// <summary>
	/// Summary description for Assist.
	/// </summary>
	public class Assist : System.Windows.Forms.Control
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Assist()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitForm call

		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if( components != null )
					components.Dispose();
			}
			base.Dispose( disposing );
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

		#region xImageConverter
		/// <summary> 
		/// Summary description for ImageConverter. 
		/// </summary> 
		public class xImageConverter : System.Windows.Forms.AxHost 
		{ 
			public xImageConverter():base("59EE46BA-677D-4d20-BF10-8D8067CB8B33") 
			{     
			} 

			public static stdole.IPictureDisp ImageToIpicture(System.Drawing.Image image) 
			{ 
				return (stdole.IPictureDisp)xImageConverter.GetIPictureDispFromPicture(image); 
			} 

			public static System.Drawing.Image IPictureToImage(stdole.StdPicture picture) 
			{ 
				return xImageConverter.GetPictureFromIPicture(picture); 
			} 
		}
		#endregion

		#region Properties

		#endregion

		#region Public Event Handlers
		
		#region OnLogException
		/// <summary>OnLogException Summary</summary>
		[		
		Browsable(true),
		Category("Events"),
		Description("Exception Process Logging"),
		DefaultValue(null),
		]		
		public event LogExceptionEventHandler	OnLogException = null;
		#endregion

		#region MessageAnalyzeEventHandler
		/// <summary>MessageAnalyzeEventHandler Summary</summary>
		public event MessageAnalyzeEventHandler	OnMessageAnalyze = null;
		#endregion

		#endregion

		#region Event Firing Functions

		#region FireLogException
		internal void FireLogException(Exception ex, string ParamName, string ParamValue) 
		{
			try 
			{
				// AJM - separate try block for missing PDB?
				// Get caller stack information (also request FileName and Line#)
				StackFrame Frame = new StackFrame(1, true);
			
				// Fire event
				if (this.OnLogException != null)
				{
					this.OnLogException(Frame, ex.Message, new string[][] {new string[] {ParamName, ParamValue}});
				}
			}
			catch {}
		}
		#endregion

		#region FireMessageAnalyze
		internal void FireMessageAnalyze(object Sender, Outlook.MailItem[] MailItems) 
		{
			// Fire event
			if (this.OnMessageAnalyze != null)
			{
				// Analyze message
				OnMessageAnalyze(Sender, MailItems);
			}
		}
		#endregion

		#endregion
	}

	#region Event Delegates and Argument Structures

	#region Logging Delegates
	/// <summary>Log Exception EventHandler Summary</summary>
	public delegate void LogExceptionEventHandler (StackFrame Sender, string Message, string[][] Params);
	/// <summary>Message Analysis Processing EventHandler Summary</summary>
	public delegate void MessageAnalyzeEventHandler (object Sender, Outlook.MailItem[] MailItems);
	#endregion

	#endregion

}
