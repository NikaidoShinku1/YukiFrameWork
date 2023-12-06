using System.Collections.Generic;
using UnityEngine;
using System;
using YukiFrameWork.Pools;

namespace YukiFrameWork
{
    public enum LifeTime
    {
        //瞬时实例,每次获取都将得到新的实例
        Transient,      
        //限制实例，在分支内唯一，不同分支则会创建新的实例
        Scope,
        //全局实例，全局唯一，单例模式
        Singleton,
    }

    public interface IContainerBuilder 
    {
        ObjectContainer Container { get;set; }
        bool IsDebugLog { get; set; }
       
        void Register<TInterface,Instance>(LifeTime lifeTime = LifeTime.Transient, params object[] args) where Instance : class;
        void Register<T>(LifeTime lifeTime = LifeTime.Transient, params object[] args) where T : class;   
        void Register(Type interfaceType, Type instanceType, LifeTime lifeTime = LifeTime.Transient, params object[] args);   
        void Register(Type type, LifeTime lifeTime = LifeTime.Transient, params object[] args);

        void RegisterScopeInstance<T>(params object[] args) where T : class;
        void RegisterScopeInstance<T>(T instance) where T : class;
        void RegisterScopeInstance<TInterface, Instance>(params object[] args) where Instance : class;
        void RegisterScopeInstance<TInterface, Instance>(Instance instance) where Instance : class;       
        void RegisterScopeInstance(Type type, params object[] args);
        void RegisterScopeInstance(Type interfaceType, Type instanceType, params object[] args);

        void RegisterInstance<T>(params object[] args) where T : class;
        void RegisterInstance<T>(T instance) where T : class;
        void RegisterInstance<TInterface, Instance>(params object[] args) where Instance : class;
        void RegisterInstance<TInterface, Instance>(Instance instance) where Instance : class;      
        void RegisterInstance(Type type,params object[] args);
        void RegisterInstance(Type interfaceType, Type instanceType, params object[] args);

        void RegisterGameObject(GameObject gameObject);
        [Obsolete("该方法不建议使用,请使用RegisterGameObject方法来进行Unity GameObject对象的注册")]
        void RegisterMono<T>(bool isStatic = false) where T : MonoBehaviour;
        [Obsolete("该方法不建议使用,请使用RegisterGameObject方法来进行Unity GameObject对象的注册")]
        void RegisterMono<T>(GameObject obj, bool isStatic = false) where T : MonoBehaviour;
    }

    public interface IObjectContainer
    {
        T Resolve<T>() where T : class;
        T ResolveComponent<T>(string name = "") where T : Component;
        object InstanceInject<T>(object obj);
        bool Equals(Type type, LifeTime life = LifeTime.Transient);
        List<object> GetAllScopeInstance();
        List<object> GetAllSingleton();
    }

    public class IOCContainer : IDisposable
    {        
        //限制分支唯一实例储存
        public readonly Dictionary<Type, object> instanceDict = DictionaryPools<Type, object>.Get();

        public readonly static Dictionary<Type, object> singletonDict = new Dictionary<Type, object>();

        //瞬时实例储存类型
        public readonly HashSet<Type> transientDict = new HashSet<Type>();

        public readonly Dictionary<Type, Type> restrainTransientDict = DictionaryPools<Type, Type>.Get();

        public readonly Dictionary<string, Transform[]> componentDict = DictionaryPools<string, Transform[]>.Get();   
        
        //储存类型注册时构造函数所带有的参数
        public Dictionary<Type,object[]> transientObject = new Dictionary<Type, object[]>();    

        public void AddScopeInstance<T>(Type type,T instance) where T : class
        {
            if (instanceDict.ContainsKey(type))
            {
                Debug.LogWarning($"当前实例在当前分支内已存在！实例类型为{instance.GetType()}");
                return;
            }
            instanceDict.Add(type, instance);
        }

        public void AddSingleton<T>(Type type, T instance) where T : class
        {
            if(singletonDict.ContainsKey(type))
            {               
                return;
            }
            singletonDict.Add(type, instance);
        }

        public void AddGameObject(GameObject obj)
        {
            Transform[] transforms = obj.GetComponentsInChildren<Transform>(true);

            componentDict.Add(obj.name,transforms);
        }

        public void AddTransient(Type type,params object[] args)
        {
            if (transientDict.Contains(type)) return;
            transientDict.Add(type);
           
            StorageConStructObject(type, args);
        }

        public void AddRestrainTransient(Type interfaceType, Type instanceType,params object[] args)
        {
            restrainTransientDict[interfaceType] = instanceType;
            StorageConStructObject(interfaceType, args);
        }

        public object GetScopeInstance(Type type)
        {
            return instanceDict[type];
        }

        public object GetSingleton(Type type)
        {
            return singletonDict[type];
        }

        public object[] GetConStructObjects(Type type)
        {
            return transientObject[type];
        }

        public T GetComponent<T>(string name = "") where T : class
        {          
            if (name != string.Empty)
            {
                Transform[] transforms = null;
                if(componentDict.ContainsKey(name))
                   transforms = componentDict[name];

                if (transforms == null || transforms.Length == 0)
                {
                    Debug.LogError($"指定的GameObject下不存在组件或GameObject不存在！GameObject name: {name}");
                    return null;
                }
                T component = null;
                for (int j = 0; j < transforms.Length; j++)
                {
                    component = transforms[j].GetComponent<T>();                   
                    if (component != null)                  
                        break;
                    
                }               
                if(!component.Equals(null))
                    return component;
                else
                    Debug.LogWarning($"查找名为{name}的对象中并没有该组件,将自动检索该分支所有的注册对象！组件类型：" + typeof(T));              
            }
            foreach(var transforms in componentDict.Values)
            {              
                for (int j = 0; j < transforms.Length; j++)
                {
                    T component = transforms[j].GetComponent<T>();
                    if (!component.Equals(null))
                    {                       
                        return component;
                    }
                }
            }
            Debug.LogError("查找所有的注册对象中并没有该组件！组件类型：" + typeof(T));
            return null;
        }

        public void StorageConStructObject(Type type,params object[] args)
        {        
            transientObject.Add(type,args);
        }

        /// <summary>
        /// 释放类型下保存的构造函数参数
        /// </summary>
        public void RemoveConStructionObject(Type type)
        {
            if (transientObject.ContainsKey(type))
            {
                transientObject[type] = null;
                transientObject.Remove(type);
            }
        }
        
       
        public void Clear()
        {
            transientDict.Clear();
            transientObject.Release();
            instanceDict.Release();
            singletonDict.Clear();
            componentDict.Release();
        }

        public void Dispose()
        {
            Clear();
        }
    }
}
