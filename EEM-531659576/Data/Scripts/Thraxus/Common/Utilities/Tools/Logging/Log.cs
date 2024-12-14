using System;
using System.IO;
using Sandbox.ModAPI;
using VRage;
using VRage.Utils;

namespace Eem.Thraxus.Common.Utilities.Tools.Logging
{
    public class Log
    {
        private const int DefaultIndent = 4;

        private readonly FastResourceLock _lockObject = new FastResourceLock();

        public Log(string logName)
        {
            LogName = logName + ".log";
            Init();
        }

        private string LogName { get; }

        private TextWriter TextWriter { get; set; }

        private static string TimeStamp => DateTime.Now.ToString("ddMMMyy_HH:mm:ss:ffff");

        private static string Indent { get; } = new string(' ', DefaultIndent);

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