using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;

namespace TGMCockpit
{
	/// <summary>Summary for frmSplash</summary>
	public class frmSplash : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Timer timerClose;
		private System.Windows.Forms.Button btnSplash;
		private System.Windows.Forms.Label lblTitle;
		private System.Windows.Forms.PictureBox picSplash;
		private System.Windows.Forms.Label lblVersion;
		private System.ComponentModel.IContainer components;

		#region Constructors / Destructors
		public frmSplash()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			Assembly Assembly = Assembly.GetCallingAssembly();

			// name, description and more

			object[] Attributes = Assembly.GetCustomAttributes(true);   
			foreach (object Attribute in Attributes)
			{
				if (Attribute is AssemblyTitleAttribute)
					lblTitle.Text = lblTitle.Text = "\r" + ((AssemblyTitleAttribute)Attribute).Title;
			}

			// version
			AssemblyName Assemblyname = Assembly.GetName();
			Version Version = Assemblyname.Version;
			lblVersion.Text = "Version " + Version.ToString();
		
		}
		/// <summary>Clean up any resources being used</summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}
		#endregion

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(frmSplash));
			this.timerClose = new System.Windows.Forms.Timer(this.components);
			this.btnSplash = new System.Windows.Forms.Button();
			this.picSplash = new System.Windows.Forms.PictureBox();
			this.lblVersion = new System.Windows.Forms.Label();
			this.lblTitle = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// timerClose
			// 
			this.timerClose.Enabled = true;
			this.timerClose.Interval = 3000;
			this.timerClose.Tick += new System.EventHandler(this.timerClose_Tick);
			// 
			// btnSplash
			// 
			this.btnSplash.Name = "btnSplash";
			this.btnSplash.Size = new System.Drawing.Size(296, 280);
			this.btnSplash.TabIndex = 0;
			this.btnSplash.Click += new System.EventHandler(this.btnSplash_Click);
			// 
			// picSplash
			// 
			this.picSplash.Image = ((System.Drawing.Bitmap)(resources.GetObject("picSplash.Image")));
			this.picSplash.Location = new System.Drawing.Point(24, 20);
			this.picSplash.Name = "picSplash";
			this.picSplash.Size = new System.Drawing.Size(240, 112);
			this.picSplash.TabIndex = 7;
			this.picSplash.TabStop = false;
			// 
			// lblVersion
			// 
			this.lblVersion.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblVersion.Location = new System.Drawing.Point(30, 208);
			this.lblVersion.Name = "lblVersion";
			this.lblVersion.Size = new System.Drawing.Size(240, 32);
			this.lblVersion.TabIndex = 6;
			this.lblVersion.Text = "Version";
			this.lblVersion.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// lblTitle
			// 
			this.lblTitle.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblTitle.Location = new System.Drawing.Point(30, 148);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Size = new System.Drawing.Size(240, 56);
			this.lblTitle.TabIndex = 5;
			this.lblTitle.Text = "TekGuard EMail Interceptor User Cockpit";
			this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// frmSplash
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(296, 280);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.picSplash,
																		  this.lblVersion,
																		  this.lblTitle,
																		  this.btnSplash});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "frmSplash";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Click += new System.EventHandler(this.timerClose_Tick);
			this.Load += new System.EventHandler(this.frmSplash_Load);
			this.ResumeLayout(false);

		}
		#endregion

		#region getAssemblyTitle
		internal static string getAssemblyTitle()
		{
			// Get the name of this assembly
			Object[] Attributes = Assembly.GetCallingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), true);
			AssemblyTitleAttribute attTitle = (AssemblyTitleAttribute)(Attributes[0]);
			return(attTitle.Title.ToString());
		}
		#endregion

		#region Display
		internal static void Display ()
		{
			// Create a new thread for the window
			Thread thrMain = new Thread(new ThreadStart(frmSplash_Thread));
			thrMain.Priority = ThreadPriority.Highest;
			thrMain.Start();
		}
		private static void frmSplash_Thread()
		{
			// Show Splash Screen
			(new frmSplash()).ShowDialog();
		}		
		#endregion

		#region getAssemblyIcon
		internal static Icon getAssemblyIcon()
		{
			try
			{
				// [DllImport("user.dll")]
				// internal static extern IntPtr LoadIcon(IntPtr hInst, string IconName);
				// [DllImport("kernel32.dll")]
				// public extern static IntPtr GetModuleHandle(string lpModuleName);
				// IntPtr hIcon = LoadIcon(GetModuleHandle(null), "#32512");
				System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(frmSplash));
				System.Drawing.Icon Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
				return (Icon);
			}
			catch
			{
				return (null);
			}
		}
		#endregion

		private void frmSplash_Load(object sender, System.EventArgs e)
		{
		}

		private void timerClose_Tick(object sender, System.EventArgs e)
		{
			btnSplash_Click(sender, e);
		}

		private void btnSplash_Click(object sender, System.EventArgs e)
		{
			this.SendToBack();
			Close();
		}

	}
}
