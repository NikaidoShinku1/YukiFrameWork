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
using System.Linq;
using System.Collections.Generic;
using YukiFrameWork.Pools;

namespace YukiFrameWork
{
    public class ArchitectureConstructor : Singleton<ArchitectureConstructor>
    {
        private ArchitectureConstructor() { }

        /// <summary>
        /// 保存所有的全局架构
        /// </summary>
        private readonly Dictionary<Type,IArchitecture> globalDicts = DictionaryPools<Type,IArchitecture>.Get();

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

            IArchitecture architecture = null;         
            IArchitecture global = null;

            if (runtime.IsOnly)
            {
                if (!globalDicts.ContainsKey(runtime.ArchitectureType))
                {
                    architecture = Architecture.CreateInstance(runtime.ArchitectureType);                   
                    globalDicts.Add(runtime.ArchitectureType, architecture.Default);                   
                }
                
                global = globalDicts[runtime.ArchitectureType];
            }
            else
            {              
                architecture = Architecture.CreateInstance(runtime.ArchitectureType);
                architecture.OnInit();
            }           

            return runtime.IsOnly ? global : architecture;             
        }       
    }
}