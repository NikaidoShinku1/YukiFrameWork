using System;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork
{
    public class ObjectContainer : IOCContainer, IObjectContainer
    {
        public T Resolve<T>() where T : class
        {
            Type type = typeof(T);          
            return Resolve(type) as T;
        }     

        private object Resolve(Type type)
        {
            object obj;

            if (singletonDict.ContainsKey(type))
            {
                return GetSingleton(type);
            }

            if (instanceDict.ContainsKey(type))
            {
                return GetScopeInstance(type);
            }

            if (transientDict.Contains(type))
            {
                if (transientObject == null || transientObject[type] == null)
                {                   
                    obj = Activator.CreateInstance(type);
                }
                else obj = Activator.CreateInstance(type,GetConStructObjects(type));
                return obj;
            }
            Debug.LogError($"无法获取对象,对象类型为{type}");
            return null;
        }

        public T ResolveComponent<T>(string name = "") where T : MonoBehaviour
        {
            T component = GetComponent<T>(name);
            return component;
        }            

        public bool Equals(Type type, LifeTime life = LifeTime.Transient)
        {
            switch (life)
            {
                case LifeTime.Transient:
                    return transientDict.Contains(type);
                case LifeTime.Singleton:
                    return instanceDict.ContainsKey(type);
                case LifeTime.Scope:
                    return singletonDict.ContainsKey(type);                   
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

            if (instanceDict.TryGetValue(type, out var obj))
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

        public List<GameObject> GetGameObjects()
           => gameObjectContainer;

        public List<object> GetAllScopeInstance()
        {
            List<object> newObj = new List<object>();
            foreach (var info in instanceDict.Values)
            {
                newObj.Add(info);
            }
            return newObj;
        }

        public List<object> GetAllSingleton()
        {
            List<object> newObj = new List<object>();
            foreach (var info in singletonDict.Values)
            {
                newObj.Add(info);
            }
            return newObj;
        }
    }
}
