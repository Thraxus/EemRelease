using System;
using System.IO;
using System.Text;
using Sandbox.ModAPI;
using VRage;
using VRage.Utils;

namespace Eem.Thraxus.Common.Utilities.Tools.Logging
{
    public class Log
    {
        private const int DefaultIndent = 4;
        private const int FlushBatch = 50;

        private readonly FastResourceLock _lockObject = new FastResourceLock();
        private readonly StringBuilder _logBuilder = new StringBuilder(256);

        private int _flushCounter = 0;

        public Log(string logName)
        {
            LogName = logName + ".log";
            Init();
        }

        private string LogName { get; }

        private TextWriter TextWriter { get; set; }

        private static string Indent { get; } = new string(' ', DefaultIndent);

        private void Init()
        {
            if (TextWriter != null) return;
            try
            {
                TextWriter = MyAPIGateway.Utilities.WriteFileInLocalStorage(LogName, typeof(Log));
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine($"Log init failed for {LogName}: {e.Message}");
            }
        }

        public void Close()
        {
            TextWriter?.Flush();
            TextWriter?.Dispose();
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
                var now = DateTime.UtcNow;
                long ticks = now.Ticks;
                _logBuilder.Clear()
                           .Append(now.ToString("ddMMMyy_HH:mm:ss"))
                           .Append('.')
                           .Append((ticks % 10000000 / 10000).ToString("D4")) // ms precision
                           .Append(Indent)
                           .Append(caller)
                           .Append(Indent)
                           .Append(message)
                           .AppendLine();

                var line = _logBuilder.ToString();
                WriteLine(line);
                MyLog.Default.WriteLineAndConsole(line);
            }
        }

        private void WriteLine(string line)
        {
            TextWriter?.WriteLine(line);
            if (++_flushCounter >= FlushBatch)
            {
                TextWriter?.Flush();
                _flushCounter = 0;
            }
        }
    }
}