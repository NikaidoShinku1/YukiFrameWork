///=====================================================
/// - FileName:      Container.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/9/30 17:22:27
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
namespace YukiFrameWork
{
	
    /// <summary>
    /// 框架通用基本容器类，可进行控制反转，容器会自动对注册的MonoBehaviour组件生命周期进行管理，当对象不再合法时，会自动进行对象的注销。
    /// </summary>
    public class Container : IDisposable
    {
        private Dictionary<Type, Dictionary<string, object>> mInstances = new Dictionary<Type, Dictionary<string, object>>();
        public void Register<T>(string key = "") where T : class,new()
        {
            key = key.IsNullOrEmpty() ? typeof(T).Name : key;

            Register<T>(new T(), key);
            
        }

        public void Register(Type type,string key = "")
        {
            key = key.IsNullOrEmpty() ? type.Name : key;
            object instance = Activator.CreateInstance(type);
            Register(type,key,instance);

        }
        private List<(Type,string)> releaseComponents = new List<(Type, string)>();
        internal void Init()
        {
            MonoHelper.Update_AddListener(Update);
        }

        private void Update(MonoHelper monoHelper)
        {
            foreach (var type in mInstances.Keys)
            {
                //如果不是组件，就不遍历
                if (!type.IsSubclassOf(typeof(UnityEngine.Component))) continue;

                foreach (var key in mInstances[type].Keys)
                {
                    object item = mInstances[type][key];
                    Component component = item as Component;

                    //如果组件已经销毁/不存在了
                    if (!component || !component.gameObject)
                    {
                        releaseComponents.Add((type,key));
                    }
                }
            }

            //如果没有则不需要回收
            if (releaseComponents.Count == 0) return;

            for (int i = 0; i < releaseComponents.Count; i++)
            {
                var item = releaseComponents[i];
                UnRegister(item.Item1, item.Item2);
            }

            releaseComponents.Clear();
        }

        public void RegisterComponent<T>(T component,string key = "") where T : Component
        {
            key = key.IsNullOrEmpty() ? typeof(T).Name : key;
            Register(typeof(T), key,component);
        }

        private void Register(Type type, string key,object value)
        {
            if (mInstances.TryGetValue(type, out var dict))
            {
                
            }
            else
            {
                dict = new Dictionary<string, object>();
                mInstances[type] = dict;
            }
            dict[key] = value;
        }

        public T Resolve<T>(string key = "") where T : class
        {
            key = key.IsNullOrEmpty() ? typeof(T).Name : key;
            if (mInstances.TryGetValue(typeof(T), out var dict))
            {
                if (dict.TryGetValue(key, out var value))
                {
                    return value as T;
                }
            }

            return null;
        }     

        public void Register<T>(T instance, string key = "") where T : class
        {
            Register(typeof(T), key, instance);
        }

        public bool UnRegister<T>(string key = "")
        {
            return UnRegister(typeof(T), key);
        }

        public bool UnRegister(Type type, string key = "")
        {
            if (key.IsNullOrEmpty())
                mInstances.Remove(type);

            try
            {

#if UNITY_2021_1_OR_NEWER
                mInstances[type].Remove(key, out object instance);

                if (instance is IDestroy destroy)
                    destroy.Destroy();
#else
            if (mInstances[type].TryGetValue(key, out object instance) && instance is IDestroy destroy)
                    destroy.Destroy();
                mInstances[type].Remove(key);
#endif
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            MonoHelper.Update_RemoveListener(Update);

            Dictionary<Type, Dictionary<string, object>> copyInstances = new Dictionary<Type, Dictionary<string, object>>(mInstances);

            foreach (var item in copyInstances.Keys)
            {
                Type type = item;
                Dictionary<string, object> instances = copyInstances[item];

                foreach (var key in instances.Keys)              
                {
                    UnRegister(type, key);
                }
            }
        }
    }
}
