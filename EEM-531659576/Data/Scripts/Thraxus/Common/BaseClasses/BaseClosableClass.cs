using System;
using Eem.Thraxus.Common.Interfaces;

namespace Eem.Thraxus.Common.BaseClasses
{
	public abstract class BaseClosableClass : IClose
	{
		public event Action<BaseClosableClass> OnClose;

		public bool IsClosed;

		public virtual void Close()
		{
			if (IsClosed) return;
			IsClosed = true;
			OnClose?.Invoke(this);
		}
	}
}