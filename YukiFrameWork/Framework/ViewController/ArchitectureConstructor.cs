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
using System.Linq;

namespace YukiFrameWork
{
    public class ArchitectureConstructor : Singleton<ArchitectureConstructor>
    {
        private static object _object = new object();
        private static bool isInited = false;
#if UNITY_2020_1_OR_NEWER
        /// <summary>
        /// 当接入hybridclr等热更插件时，该特性可能出现无法正常使用的情况，可在项目中将该特性进行注释。手动调用ArchitectureConstructor.InitArchitecture()方法;
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void InitArchitecture()
        {
            if (isInited) return;
            isInited = true;
            FrameworkConfigInfo info = Resources.Load<FrameworkConfigInfo>("FrameworkConfigInfo");           
            lock (_object)
            {
                Loading(info.assembly);

                foreach (var name in info.assemblies)
                {
                    Loading(name, true);
                }     
            }

            _ = ArchitectureConstructor.Instance;
        }

        public override void OnInit()
        {
            InitArchitecture();
            MonoHelper.Destroy_AddListener(_ =>
            {
                globalDicts.Clear();
                runtimeRequests.Clear();
                Dispose();
            });
        }

        public readonly static IList<Assembly> runtime_local_assemblies = new List<Assembly>();
        
        private static void Loading(string name,bool depend = false)
        {
            Assembly assembly = null;
            try
            {               
                assembly = Assembly.Load(name);              
            }
            catch (Exception ex)
            {                   
                string message = depend ? $"该程序集依赖不存在：assembly:{name}  将继续向下检索! " : "请检查是否输入正确的程序集定义!在编辑器左上方YukiFrameWork/LocalConfigGenerator修改,本次运行不会预生成全局架构，将以首次调用而生成! ";
                Debug.LogWarning(message);
            }

            if (assembly == null) return;
            runtime_local_assemblies.Add(assembly);
            Type[] types = assembly.GetTypes();
            Type[] architectureTypes = types.Where(type => typeof(IArchitecture).IsAssignableFrom(type)).ToArray();

            LoadingArchitecture(architectureTypes);                  
        }

        private static void LoadingArchitecture(Type[] types)
        {
            for (int i = 0; i < types.Length; i++)
            {
                Type type = types[i];
                I.GetOrAddArchitecture(type);
            }

        }
#endif
        private ArchitectureConstructor() { }
        
        /// <summary>
        /// 保存所有的全局架构
        /// </summary>
        private readonly Dictionary<Type,IArchitecture> globalDicts = DictionaryPools<Type,IArchitecture>.Get();

        internal readonly Dictionary<string, ArchitectureStartUpRequest> runtimeRequests = new Dictionary<string, ArchitectureStartUpRequest>();

        public IArchitecture GetOrAddArchitecture<T>()
        {
            return GetOrAddArchitecture(typeof(T));
        }

        public IArchitecture GetOrAddArchitecture(Type type)
        {
            if (!globalDicts.TryGetValue(type, out var value))
            {              
                value = Activator.CreateInstance(type) as IArchitecture;                                                   
                RegisterGlobal(type, value);
            }
            return value;
        }
        private void RegisterGlobal(Type type, IArchitecture architecture)
        {
            globalDicts[type] = architecture;
        }

        public IArchitecture Enquene<T>(T viewController) where T : IController
        {
            return ArchiteInlization(viewController);
        }    

        private IArchitecture ArchiteInlization<T>(T viewController) where T : IController
        {
            Type type = viewController.GetType();                        

            RuntimeInitializeOnArchitecture runtime = type.GetCustomAttribute<RuntimeInitializeOnArchitecture>(true);

            if (runtime == null)
            {               
                throw new Exception("无法进行初始化,请在取消RuntimeInitializeOnArchitecture标记后手动重写RuntimeArchitecture属性!");               
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