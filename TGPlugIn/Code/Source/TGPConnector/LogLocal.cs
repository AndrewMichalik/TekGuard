using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace TGPConnector
{
	/// <summary>
	/// Summary for LogLocal
	/// </summary>
	public class LogLocal
	{
		// Class members
		private string				m_LogFilePath;						// Log File directory
		private	string				m_EvtFileName;						// Event log file name
		private	string				m_AnaFileName;						// Analyzer log file name
		private string				m_EvtProgName;						// Windows event log program name

		// Logging constants
		private const string		WIN_EVENTAPP	= "Application";	// Windows event log type
		private const string		PAD_SESSIONID	= "00000";			// Session ID zero padding
		private const string		LOG_EXTENSION	= ".log";			// Log file extension

		#region Constructors

		public LogLocal(string EvtProgName, string LogFilePath, string EvtFileBase, string AnaFileBase)
		{
			m_EvtProgName = EvtProgName;
			m_LogFilePath = LogFilePath;
			m_EvtFileName = EvtFileBase + LOG_EXTENSION;
			m_AnaFileName = AnaFileBase + FileYearDate + LOG_EXTENSION;

			// Register a windows event log source (if not already registered)
			if (!EventLog.SourceExists(EvtProgName)) 
			{
				EventLog.CreateEventSource(EvtProgName, WIN_EVENTAPP);
			}
		}

		#endregion

		#region Write Methods

		#region WriteAlert

		public void WriteAlert (StackFrame Sender, string Message, string[][] Params)
		{
			WriteAlert (FrameIdentify(Sender), Message, Params);
		}
		
		public void WriteAlert (string Sender, string Message, string[][] Params)
		{
			try
			{
				string sText = "";
				const string EQUALS = "=";
				const string PREFIX = "<";
				const string SUFFIX = ">";

				// Build the parameter values string
				if (Params != null) foreach (string[] ParamSet in Params)
				{
					if (ParamSet[0] != null) sText = sText + ParamSet[0] + EQUALS + PREFIX + ParamSet[1] + SUFFIX;
				}
				sText = Sender + "\t" + Message + "\t" + sText;

				// Write to the App Event Log File
				LogFileWrite(m_EvtFileName, sText);
			}
			catch(Exception e)
			{
				WriteWinAppEvent ("Cannot log to file: '" + m_EvtFileName + "', " + e.Message, EventLogEntryType.Error);
			}
		}

		#endregion

		#region WriteNetEvent

		public void WriteNetEvent(string sNetType, string sSessionID, string sIP, string sPrefix, string sText)
		{
			// Determine log file name
			string sFileName = sNetType + this.FileYearDate + LOG_EXTENSION;

			try
			{
				// Pad SessionID with zeros
				sSessionID = Convert.ToInt32(sSessionID).ToString(PAD_SESSIONID);
			
				// Load descriptive prefix
				if (sPrefix == "S")
					sPrefix = "S >>: ";
				else if (sPrefix == "C")
					sPrefix = "<< C: ";
			
				// Enclose sText in single quotes
				if (sText == null) sText = "";
				if (sText.Length > 0) sText = "'" + sText + "'";

				// Perform actual output
				string logEntry = "SessionID: " + sSessionID + " IP: " + sIP + " " + sPrefix + sText;

				// Write to the Net Event Log File
				LogFileWrite(sFileName, logEntry);
			}
			catch(Exception e)
			{
				WriteWinAppEvent ("Cannot log to file: '" + sFileName + "', " + e.Message, EventLogEntryType.Error);
			}
		}

		#endregion

		#region WriteAnalyzer
		public void WriteAnalyzer(string Sender, string sText) 
		{
			try
			{
				// Log the event
				LogFileWrite(m_AnaFileName, sText);
			}
			catch(Exception e)
			{
				WriteWinAppEvent ("Cannot log to file: '" + m_AnaFileName + "', " + e.Message, EventLogEntryType.Warning);
			}
		}
		#endregion

		#region WriteWinAppEvent

		public void WriteWinAppEvent(string sText, EventLogEntryType EntryType)
		{
			try 
			{
				// Create a new Application EventLog component, set Source property			
				EventLog aLog = new EventLog();
				aLog.Source = m_EvtProgName;

				// Write message
				aLog.WriteEntry(sText, EntryType);
			}
			catch {}
		}
		
		#endregion

		#endregion

		#region Properties

		#region FileYearDate
		/// <summary>
		/// FileYearDate Summary
		/// </summary>
		private string FileYearDate
		{
			get 
			{
				DateTime dtToday = DateTime.Today;

				// Pad Month and Day
				string sMonth = dtToday.Month.ToString("00");
				string sDay   = dtToday.Day.ToString("00");

				// Return Log file name date designator
				return ("_" + dtToday.Year.ToString() + "_" + sMonth + sDay);
			}
		}
	
		#endregion

		#endregion

		#region LogFileWrite

		private bool LogFileWrite(string sFileName, string sText)
		{
			try
			{
				// Get the current date/time
				DateTime now = DateTime.Now;
				
				// Open the specified event log file
				FileStream fs = new FileStream(m_LogFilePath + sFileName, FileMode.OpenOrCreate, FileAccess.Write);

				// Create a writer, position the file pointer to the end
				StreamWriter w = new StreamWriter(fs);		
				w.BaseStream.Seek(0, SeekOrigin.End);

				// Write the date/time and message text
				w.WriteLine(now.ToShortDateString()
					+ " " + now.ToShortTimeString()
					+ ":\t" + sText);
				w.Flush();
				w.Close();

				// Success
				return (true);
			}
			catch(Exception ex)
			{
				// Try to send message to the windows event log
				WriteWinAppEvent(sText + ", " + ex.Message, EventLogEntryType.Error);

				// Failure
				return (false);
			}
		}

		#endregion

		#region FrameIdentify
		private string FrameIdentify(StackFrame Sender)
		{
			try 
			{
				// Get calling method (where exception occurred)
				MethodBase mCaller = Sender.GetMethod();

				// Get line number (requires PDB file to be in binary directory)
				string Linenumber = (Sender.GetFileName() == null) ? "" : "@" + Sender.GetFileLineNumber().ToString();

				// Return logging Sender string
				return (mCaller.DeclaringType.FullName.ToString() + "." + mCaller.Name + Linenumber);
			}
			catch {}
			
			// Unknown Sender type
			return (Sender.ToString());

		}
		#endregion

	}
}
