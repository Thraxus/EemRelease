using System;
using Eem.Thraxus.Common.Interfaces;

namespace Eem.Thraxus.Common.BaseClasses
{
    public abstract class BaseEntity : IHaveSubscriptions, IInit, ILog
    {
        public event Action<string, string> OnWriteToLog;
        public event Action<IClose> OnClose;

        private string _logPrefix;
        public bool IsClosed { get; private set; }
        
        public void Init()
        {
            SubscriptionHandler();
        }

        public void SubscriptionHandler(bool close = false)
        {
            if (close)
            {
                UnSubscribe();
                return;
            }
            Subscribe();
        }

        public abstract void Subscribe();

        public abstract void UnSubscribe();
        
        public void Close()
        {
            if (IsClosed) return;
            IsClosed = true;
            SubscriptionHandler(true);
            OnClose?.Invoke(this);
        }
        
        public void WriteGeneral(string caller, string message)
        {
            OnWriteToLog?.Invoke($"{_logPrefix} {caller}", message);
        }

        protected void SetLogPrefix(string prefix)
        {
            _logPrefix = $"[{prefix}]";
        }
    }
}