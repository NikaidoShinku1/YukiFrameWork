using System;

namespace YukiFrameWork.IOC
{
    public interface IEasyTypeBuilder
    {
        void Register(Type type, LifeTime lifeTime = LifeTime.Transient);
        void Register(string name, Type type, LifeTime lifeTime = LifeTime.Transient);
    }
}
