using System;
using UnityEngine;

namespace YukiFrameWork.IOC
{
    public enum LifeTime
    {
        Transient = 0,
        Scoped,
        Singleton
    }

    public interface IContainerBuilder : IEasyBuilder,IEasyTypeBuilder,ICustomBuilder,IDisposable
    {
        IResolveContainer Container { get; }
    }
}
