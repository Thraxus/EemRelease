using System;
using Eem.Thraxus.Common.Interfaces;

namespace Eem.Thraxus.Common.BaseClasses
{
    public abstract class BaseLoggingClass : ICommon
    {
        private string _logPrefix;
        public event Action<string, string> OnWriteToLog;
        public event Action<IClose> OnClose;

        public bool IsClosed { get; protected set; }

        public virtual void Close()
        {
            if (IsClosed) return;
            IsClosed = true;
            OnClose?.Invoke(this);
        }

        public virtual void Update(ulong tick)
        {
        }

        public virtual void WriteGeneral(string caller, string message)
        {
            if (string.IsNullOrEmpty(_logPrefix))
                SetLogPrefix();
            OnWriteToLog?.Invoke($"{_logPrefix}{caller}", message);
        }

        protected void OverrideLogPrefix(string prefix)
        {
            _logPrefix = "[" + prefix + "] ";
        }

        private void SetLogPrefix()
        {
            _logPrefix = "[" + GetType().Name + "] ";
        }
    }
}