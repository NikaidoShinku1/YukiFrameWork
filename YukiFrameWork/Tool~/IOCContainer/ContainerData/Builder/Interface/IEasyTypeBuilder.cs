using System;

namespace YukiFrameWork
{
    public interface IEasyTypeBuilder
    {
        void Register(Type type, LifeTime lifeTime = LifeTime.Transient,params object[] args);
        void Register(string name, Type type, LifeTime lifeTime = LifeTime.Transient, params object[] args);
    }
}
