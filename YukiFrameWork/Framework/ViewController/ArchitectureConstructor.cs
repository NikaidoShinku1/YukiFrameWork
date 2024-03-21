///=====================================================
/// - FileName:      ArchitectureConstructor.cs
/// - NameSpace:     YukiFrameWork
/// - Created:       Yuki
/// - Email:         1274672030@qq.com
/// - Description:   控制器基类
/// - Creation Time: 2024/1/14 17:25:22
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using System;
using System.Reflection;
using System.Collections.Generic;
using YukiFrameWork.Pools;
using UnityEngine;

namespace YukiFrameWork
{
    public class ArchitectureConstructor : Singleton<ArchitectureConstructor>
    {
        private static object _object = new object();
#if UNITY_2020_1_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void InitArchitecture()
        {
            LocalGenericScriptInfo info = Resources.Load<LocalGenericScriptInfo>("LocalGenericScriptInfo");

            lock (_object)
            {
                Assembly assembly = null;
                try
                {
                    assembly = Assembly.Load(info.assembly);
                }
                catch (Exception ex)
                {
                    Debug.LogError("请检查是否输入正确的程序集定义!在编辑器左上方YukiFrameWork/LocalScriptsGenerator修改,本次运行不会预生成全局架构，将以首次调用而生成" + ex.Message);
                }

                if (assembly == null) return;

                Type[] types = assembly.GetTypes();

                for (int i = 0; i < types.Length; i++)
                {
                    Type type = types[i];

                    if (type.GetInterface("IArchitecture") == null) continue;

                    if (type.GetCustomAttribute<NoGenericArchitectureAttribute>() != null) continue;

                    I.GetOrAddArchitecture(type);
                    
                }
            }          
        }
#endif
        private ArchitectureConstructor() { }

        ~ArchitectureConstructor()
        {
            globalDicts.Clear();
        }
        /// <summary>
        /// 保存所有的全局架构
        /// </summary>
        private readonly Dictionary<Type,IArchitecture> globalDicts = DictionaryPools<Type,IArchitecture>.Get();

        public IArchitecture GetOrAddArchitecture<T>()
        {
            return GetOrAddArchitecture(typeof(T));
        }

        public IArchitecture GetOrAddArchitecture(Type type)
        {
            if (!globalDicts.TryGetValue(type, out var value))
            {
                value = Activator.CreateInstance(type) as IArchitecture;
                value.OnInit();
                RegisterGlobal(type, value);
            }
            return value;
        }
        private void RegisterGlobal(Type type, IArchitecture architecture)
        {
            globalDicts[type] = architecture;
        }

        public IArchitecture Enquene<T>(T viewController) where T : ViewController
        {
            return ArchiteInlization(viewController);
        }    

        private IArchitecture ArchiteInlization<T>(T viewController) where T : ViewController
        {
            Type type = viewController.GetType();                        

            RuntimeInitializeOnArchitecture runtime = type.GetCustomAttribute<RuntimeInitializeOnArchitecture>();

            if (runtime == null)
            {               
                throw new Exception("无法进行初始化,请在取消RuntimeInitializeOnArchitecture标记后手动重写RuntimeArchitecture属性!");               
            }

            NoGenericArchitectureAttribute noGeneric = runtime.ArchitectureType.GetCustomAttribute<NoGenericArchitectureAttribute>();

            if (noGeneric != null)
            {
                throw new Exception($"无法进行对{runtime.ArchitectureType}架构的初始化,这个架构是标记了NoGenericArchitecture特性的,仅作为全局手动使用");            
            }

            if (!runtime.IsGeneric)
            {
                throw new Exception($"RuntimeInitializeOnArchitecture IsGeneric为False,不会为该控制器获取架构实例!");
            }

            IArchitecture architecture = GetOrAddArchitecture(runtime.ArchitectureType);                            
            return architecture;             
        }       
    }
}