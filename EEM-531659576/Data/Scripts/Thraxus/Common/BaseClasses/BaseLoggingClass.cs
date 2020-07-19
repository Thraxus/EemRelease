using System;
using Eem.Thraxus.Common.Enums;
using Eem.Thraxus.Common.Interfaces;
using VRage.Game;

namespace Eem.Thraxus.Common.BaseClasses
{
	public abstract class BaseLoggingClass : BaseClosableClass, ILog
	{
		public event Action<string, string, LogType, bool, int, string> OnWriteToLog;

		protected abstract string Id { get; }

		public void WriteToLog(string caller, string message, LogType type, bool showOnHud = false, int duration = Settings.DefaultLocalMessageDisplayTime, string color = MyFontEnum.Green)
		{
			OnWriteToLog?.Invoke($"{Id}: {caller}", message, type, showOnHud, duration, color);
		}
	}
}