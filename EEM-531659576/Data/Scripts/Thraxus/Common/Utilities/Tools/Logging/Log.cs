using System;
using System.IO;
using Sandbox.ModAPI;
using VRage;
using VRage.Utils;

namespace Eem.Thraxus.Common.Utilities.Tools.Logging
{
	public class Log
	{
		private string LogName { get; set; }

		private TextWriter TextWriter { get; set; }

		private static string TimeStamp => DateTime.Now.ToString("ddMMMyy_HH:mm:ss:ffff");

		private const int DefaultIndent = 4;

		private static string Indent { get; } = new string(' ', DefaultIndent);

		public Log(string logName)
		{
			LogName = logName + ".log";
			Init();
		}

        private void Init()
		{
			if (TextWriter != null) return;
			TextWriter = MyAPIGateway.Utilities.WriteFileInLocalStorage(LogName, typeof(Log));
		}

		public void Close()
		{
			TextWriter?.Flush();
            TextWriter?.Dispose();
            TextWriter?.Close();
			TextWriter = null;
		}

		public void WriteGeneral(string caller = "", string message = "")
		{
			BuildLogLine(caller, message);
		}

        private readonly FastResourceLock _lockObject = new FastResourceLock();

		private void BuildLogLine(string caller, string message)
		{
            using (_lockObject.AcquireExclusiveUsing())
            {
                var newMessage = $"{TimeStamp}{Indent}{caller}{Indent}{message}";
                WriteLine(newMessage);
                MyLog.Default.WriteLineAndConsole(newMessage);
            }
		}

		private void WriteLine(string line)
		{
            TextWriter?.WriteLine(line);
            TextWriter?.Flush();
		}
	}
}