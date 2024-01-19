using System;
using UnityEngine;

namespace YukiFrameWork
{
    public enum LifeTime
    {
        Transient = 0,
        Scoped,
        Singleton
    }

    public interface IContainerBuilder : IEasyBuilder,IEasyTypeBuilder,ICustomBuilder
    {
        
    }
}
