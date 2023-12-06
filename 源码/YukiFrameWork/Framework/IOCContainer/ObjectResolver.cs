using System;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork
{
    public class ObjectContainer : IObjectContainer
    {
        private IOCContainer container;

        public ObjectContainer(IOCContainer container)
        {
            this.container = container;
        }

        public T Resolve<T>() where T : class
        {
            Type type = typeof(T);          
            return Resolve(type) as T;
        }     

        private object Resolve(Type type)
        {
            object obj;

            if (IOCContainer.singletonDict.ContainsKey(type))
            {
                return container.GetSingleton(type);
            }

            if (container.instanceDict.ContainsKey(type))
            {
                return container.GetScopeInstance(type);
            }

            if (container.transientDict.Contains(type))
            {
                if (container.transientObject == null || container.transientObject[type] == null)
                {                   
                    obj = Activator.CreateInstance(type);
                }
                else obj = Activator.CreateInstance(type, container.GetConStructObjects(type));
                return obj;
            }
            Debug.LogError($"无法获取对象,对象类型为{type}");
            return null;
        }

        public T ResolveComponent<T>(string name = "") where T : Component
        {
            T component = container.GetComponent<T>(name);
            return component;
        }            

        public bool Equals(Type type, LifeTime life = LifeTime.Transient)
        {
            switch (life)
            {
                case LifeTime.Transient:
                    return container.transientDict.Contains(type);
                case LifeTime.Singleton:
                    return container.instanceDict.ContainsKey(type);
                case LifeTime.Scope:
                    return IOCContainer.singletonDict.ContainsKey(type);                   
            }
            return false;
        }

        /// <summary>
        /// 注入单例模式的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public object InstanceInject<T>(object value)
        {
            Type type = typeof(T);

            if (container.instanceDict.TryGetValue(type, out var obj))
            {
                foreach (var info in value.GetType().GetFields())
                {
                    if (info.FieldType == obj.GetType())
                    {
                        info.SetValue(obj, value);
                    }
                }

                foreach (var info in value.GetType().GetProperties())
                {
                    if (info.PropertyType == obj.GetType())
                    {
                        info.SetValue(obj, value);
                    }
                }
            }
            return value;

        }       

        public List<object> GetAllScopeInstance()
        {
            List<object> newObj = new List<object>();
            foreach (var info in container.instanceDict.Values)
            {
                newObj.Add(info);
            }
            return newObj;
        }

        public List<object> GetAllSingleton()
        {
            List<object> newObj = new List<object>();
            foreach (var info in IOCContainer.singletonDict.Values)
            {
                newObj.Add(info);
            }
            return newObj;
        }

        internal void AddRestrainTransient(Type interfaceType, Type instanceType)
        {
            container?.AddRestrainTransient(interfaceType, instanceType);
        }

        internal void AddRestrainTransient(Type interfaceType, Type instanceType, object[] args)
        {
            container?.AddRestrainTransient(interfaceType, instanceType,args);
        }

        internal void AddScopeInstance(Type interfaceType, object instance)
        {
            container?.AddScopeInstance(interfaceType, instance);
        }

        internal void AddSingleton(Type interfaceType, object obj)
        {
            container?.AddSingleton(interfaceType, obj);
        }

        internal void AddTransient(Type type, object[] args)
        {
            container?.AddTransient(type, args);
        }

        internal void AddTransient(Type type)
        {
            container?.AddTransient(type);
        }

        internal void AddGameObject(GameObject gameObject)
        {
            container?.AddGameObject(gameObject);
        }

        internal void Dispose()
        {
            container?.Dispose();
            container = null;
        }
    }
}
