///=====================================================
/// - FileName:      InjectBuilderExtension.cs
/// - NameSpace:     YukiFrameWork.IOC
/// - Description:   Container拓展
/// - Creation Time: 2024/2/22 12:44:05
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
namespace YukiFrameWork.IOC
{
	public static class InjectBuilderExtension
	{
        public static bool Destroy<T>(this IReleaseContainer release,string name)
        {
            return release.Destroy(typeof(T), name);
        }

        public static bool Destroy<T>(this IReleaseContainer release)
        {
            return release.Destroy(typeof(T));
        }

        public static T Resolve<T>(this IDefaultContainer d) where T : class
        {
            return d.Resolve(typeof(T)) as T;
        }
        public static T Resolve<T>(this IDefaultContainer d,string name) where T : class
        {
            return d.Resolve(typeof(T),name) as T;
        }
       
        public static IEntryPoint ResolveEntry<T>(this IDefaultContainer d)
        {
            return d.ResolveEntry( typeof(T),typeof(T).Name);
        }

        public static IEntryPoint ResolveEntry<T>(this IDefaultContainer d,string name)
        {
            return d.ResolveEntry(typeof(T), name);
        }
    }
}
