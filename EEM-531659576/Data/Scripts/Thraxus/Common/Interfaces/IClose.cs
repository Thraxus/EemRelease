using System;

namespace Eem.Thraxus.Common.Interfaces
{
    public interface IClose
    {
        event Action<IClose> OnClose;
        void Close();
        bool IsClosed { get; }
    }
}