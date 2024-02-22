using System;
namespace YukiFrameWork.IOC
{
    public interface IDefaultContainer
    {
        object Resolve(Type type);
        object Resolve(Type type, string name);
        IEntryPoint ResolveEntry(Type type, string name);
    }
}