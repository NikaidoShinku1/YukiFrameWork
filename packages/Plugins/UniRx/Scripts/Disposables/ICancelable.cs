using System;
using System.Collections.Generic;
using System.Text;

namespace YukiFrameWork.UniRx
{
    public interface ICancelable : IDisposable
    {
        bool IsDisposed { get; }
    }
}
