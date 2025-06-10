///=====================================================
/// - FileName:      BuffKit.cs
/// - NameSpace:     YukiFrameWork.Buffer
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/5/5 16:15:51
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using System;
using System.Collections.Generic;
using System.Collections;
using YukiFrameWork.Pools;
using System.Reflection;
namespace YukiFrameWork.Buffer
{
    internal class BuffBindder
    {
        public IBuff buff;
        public Type controllerType;
    }
    public static class BuffKit
    {
        /// <summary>
        /// 缓存所有的Buff配置
        /// </summary>
        private readonly static Dictionary<string, BuffBindder> buffItems = new Dictionary<string, BuffBindder>();  

        private static IBuffLoader loader = null;

        public static void InitBuffLoader(string projectName)
        {
            loader = new ABManagerBuffLoader(projectName);
        }

        public static void InitBuffLoader(IBuffLoader loader)
        {
            BuffKit.loader = loader;
        }

        public static void LoadBuffDataBase(string dataBasePath)
        {
            BuffDataBase buffDataBase = loader.Load<BuffDataBase>(dataBasePath);
            LoadBuffDataBase(buffDataBase);
        }

        public static IEnumerator LoadBuffDataBaseAsync(string dataBasePath)
        {
            bool isCompleted = false;

            loader.LoadAsync<BuffDataBase>(dataBasePath, dataBase => 
            {
                LoadBuffDataBase(dataBase);
                isCompleted = true;
            });

            yield return CoroutineTool.WaitUntil(() => isCompleted);
        }

        public static void LoadBuffDataBase(BuffDataBase buffDataBase)
        {
            foreach (var buff in buffDataBase.buffConfigs)
            {
                AddBuff(buff);
            }
            if(loader != null)
                loader.UnLoad(buffDataBase);
        }

        public static void BindController<T>(string buffKey) where T : BuffController
        {
            BindController(buffKey, typeof(T));                    
        }

        public static void BindController(string buffKey, Type type)
        {
            if (!typeof(BuffController).IsAssignableFrom(type))
            {
                throw new Exception("Type不继承BuffController Type:" + type);
            }

            if (!buffItems.TryGetValue(buffKey, out var bindder))
            {
                throw new Exception("没有对应的Buff标识，如果需要新增Buff并绑定请先使用BuffKit.AddBuff!如果是来自BuffDataBase管理的Buff，请先调用BuffKit.LoadBuffDataBase!");
            }

            Bind(bindder, type);
        }

        internal static BuffController CreateBuffController(string buffKey)
        {
            if (!buffItems.TryGetValue(buffKey, out var bindder))
            {
                throw new Exception("Buff没有加载到BuffKit内! BuffKey:" + buffKey);
            }

            if (bindder.controllerType == null)
            {
                throw new Exception("该Buff没有绑定控制器，请重试 BuffKey:" + buffKey);
            }
            return GlobalObjectPools.GlobalAllocation(bindder.controllerType) as BuffController;          
        }
      

        public static void AddBuff(IBuff buff)
        {
            BindBuffControllerAttribute bind = buff.GetType().GetCustomAttribute<BindBuffControllerAttribute>();
            Type cType = bind != null ? bind.ControllerType : null;
            buffItems.Add(buff.GetBuffKey, new BuffBindder() { buff = buff, controllerType = cType });
        }

        private static void Bind(BuffBindder buffBindder,Type type)
        {
            buffBindder.controllerType = type;
        }

        public static IBuff GetBuffByKey(string key)
        {
            buffItems.TryGetValue(key, out var buffBindder);
            return buffBindder?.buff;
        }
     
        public static BuffController AddBuffer(this IBuffExecutor executor,IBuff buff)
        {
            return executor.Handler.AddBuffer(buff,executor);
        }

        public static BuffController AddBuffer(this IBuffExecutor executor, string buffKey)
        {
            return executor.Handler.AddBuffer(buffKey, executor);
        }

        public static bool RemoveBuffer(this IBuffExecutor executor, string buffKey)
            => executor.Handler.RemoveBuffer(buffKey);

        public static bool RemoveBuffer(this IBuffExecutor executor, IBuff buff)
           => executor.Handler.RemoveBuffer(buff);
    }
}
