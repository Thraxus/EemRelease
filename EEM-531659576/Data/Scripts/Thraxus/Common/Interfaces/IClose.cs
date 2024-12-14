using System;

namespace Eem.Thraxus.Common.Interfaces
{
    public interface IClose
    {
        bool IsClosed { get; }
        event Action<IClose> OnClose;
        void Close();
    }
}