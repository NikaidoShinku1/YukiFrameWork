using System;
using UnityEngine;

namespace YukiFrameWork
{
    public class ContainerBuilder : IContainerBuilder
    {
        public ObjectContainer Container { get; set; }
   
        public bool IsDebugLog { get; set; }

        public ContainerBuilder()
        {
            Container = new ObjectContainer(new IOCContainer());             
        }
        #region 注册实例
        public void Register<TInterface, Instance>(LifeTime lifeTime = LifeTime.Transient, params object[] args) where Instance : class
        {
            Type interfaceType = typeof(TInterface);
            Type instanceType = typeof(Instance);
            Register(interfaceType, instanceType, lifeTime, args);
        }
        
        public void Register<T>(LifeTime lifeTime = LifeTime.Transient, params object[] args) where T : class
        {
            Type type = typeof(T);
            Register(type, lifeTime, args);
        }
        public void Register<T>(LifeTime lifeTime = LifeTime.Transient) where T : class
        {
            Type type = typeof(T);
            Register(type, lifeTime);
        }

        public void Register(Type interfaceType, Type instanceType, LifeTime lifeTime = LifeTime.Transient, params object[] args)
        {
            switch (lifeTime)
            {
                case LifeTime.Transient:
                    if (args.Length < 1)
                    {
                        Container.AddRestrainTransient(interfaceType,instanceType);
                    }
                    else
                    {
                        Container.AddRestrainTransient(interfaceType, instanceType,args);
                    }
                    if (IsDebugLog)
                    {
                        Debug.Log("瞬时实例注册成功，接口约束: "+interfaceType +"实例类型:" + instanceType);
                    }
                    break;
                case LifeTime.Scope:
                    {
                        var instance = Activator.CreateInstance(instanceType, args);
                        if (instance != null)
                        {
                            if (IsDebugLog)
                                Debug.Log("限制单例注册成功，接口约束: " + interfaceType + "实例类型:" + instance.GetType());
                            Container.AddScopeInstance(interfaceType, instance);
                        }
                        else
                        {
                            if (IsDebugLog)
                                Debug.LogError("限制单例注册失败，接口约束: " + interfaceType + "实例类型:" + instanceType);
                        }
                    }
                    break;
                case LifeTime.Singleton:
                    if (Container.Equals(interfaceType, LifeTime.Singleton))
                    {
                        if (IsDebugLog)
                        {
                            Debug.Log("当前全局实例已存在，接口类型:" + interfaceType);
                        }
                        return;
                    }
                    var obj = Activator.CreateInstance(instanceType, args);
                    if (obj != null)
                    {
                        if (IsDebugLog)
                            Debug.Log("全局单例注册成功，接口约束: " + interfaceType + "实例类型:" + obj.GetType());

                        Container.AddSingleton(interfaceType, obj);
                    }
                    else
                    {
                        if (IsDebugLog)
                            Debug.LogError("全局单例注册失败，接口约束: " + interfaceType + "实例类型:" + instanceType);
                    }
                    break;
            }
        }
        
        public void Register(Type type, LifeTime lifeTime = LifeTime.Transient, params object[] args)
        {
            switch (lifeTime)
            {
                case LifeTime.Transient:
                    if (args.Length < 1)
                    {                      
                        Container.AddTransient(type);
                    }
                    else
                    {                      
                        Container.AddTransient(type, args);
                    }
                    if (IsDebugLog)
                    {
                        Debug.Log("瞬时实例注册成功，类型:" + type);
                    }
                    break;
                case LifeTime.Scope:
                    {
                        var instance = Activator.CreateInstance(type, args);
                        if (instance != null)
                        {                          
                            if (IsDebugLog)
                                Debug.Log("限制单例注册成功，类型:" + instance.GetType());
                            Container.AddScopeInstance(type, instance);
                        }
                        else
                        {                            
                            if (IsDebugLog)
                                Debug.LogError("限制单例注册失败，类型：" + type);
                        }
                    }
                    break;
                case LifeTime.Singleton:
                    if (Container.Equals(type, LifeTime.Singleton))
                    {
                        if (IsDebugLog)
                        {
                            Debug.Log("当前全局实例已存在，类型:" + type);
                        }
                        return;
                    }
                    var obj = Activator.CreateInstance(type,args);
                    if (obj != null)
                    {
                        if (IsDebugLog)
                            Debug.Log("全局单例注册成功，类型:" + obj.GetType());

                        Container.AddSingleton(type, obj);
                    }
                    else
                    {
                        if (IsDebugLog)
                            Debug.LogError("全局单例注册失败，类型：" + type);
                    }
                    break;                                
            }
        }
        #endregion

        #region 注册全局单例
        public void RegisterInstance<T>(T instance) where T : class
        {          
            Type type = typeof(T);
            if (instance != null && IsDebugLog)
            {
                Debug.Log("全局单例注册成功，类型:" + instance.GetType());
            }
            Container.AddSingleton(type, instance);
        }
        public void RegisterInstance(Type type, params object[] args)
        {
             Register(type, LifeTime.Singleton,args);
        }

        public void RegisterInstance<TInterface, Instance>(Instance instance) where Instance : class
        {
            Type interfaceType = typeof(TInterface);
            Type instanceType = typeof(Instance);
            if (IsDebugLog)
                Debug.Log("全局单例注册成功，接口约束: " + interfaceType + "实例类型:" + instanceType);
            Container.AddSingleton(interfaceType, instance);
        }

        public void RegisterInstance<T>(params object[] args) where T : class
        {
            Register(typeof(T), LifeTime.Singleton, args);
        }

        public void RegisterInstance<TInterface, Instance>(params object[] args) where Instance : class
        {
            Type interfaceType = typeof(TInterface);
            Type instanceType = typeof(Instance);
            Register(interfaceType, instanceType, LifeTime.Singleton, args);
        }

        public void RegisterInstance(Type interfaceType, Type instanceType, params object[] args)
        {
            Register(interfaceType, instanceType, LifeTime.Singleton, args);
        }

        #endregion
        #region 注册限制单例
        public void RegisterScopeInstance<T>(T instance) where T : class
        {
            Type type = typeof(T);
            if (IsDebugLog)
            {
                Debug.Log("限制单例注册成功，类型:" + instance.GetType());
            }
            Container.AddScopeInstance(type, instance);
        }

        public void RegisterScopeInstance(Type type, params object[] args)
        {
            Register(type, LifeTime.Scope, args);
        }

        public void RegisterScopeInstance<TInterface, Instance>(Instance instance) where Instance : class
        {
            Type interfaceType = typeof(TInterface);
            Type instanceType = typeof(Instance);
            if (IsDebugLog)
                Debug.Log("限制单例注册成功，接口约束: " + interfaceType + "实例类型:" + instanceType);
            Container.AddScopeInstance(interfaceType, instance);
        }

        public void RegisterScopeInstance(Type interfaceType, Type instanceType, params object[] args)
        {
            Register(interfaceType, instanceType, LifeTime.Scope, args);
        }

        public void RegisterScopeInstance<T>(params object[] args) where T : class
        {
            Type type= typeof(T);
            Register(type, LifeTime.Scope, args);
        }

        public void RegisterScopeInstance<TInterface, Instance>(params object[] args) where Instance : class
        {
            Type interfaceType = typeof(TInterface);
            Type instanceType = typeof(Instance);
            Register(interfaceType, instanceType, LifeTime.Scope, args);
        }

        #endregion
        #region 注册mono实例
        [Obsolete]
        public void RegisterMono<T>(bool isStatic = false) where T : MonoBehaviour
        {
            RegisterMono<T>(null, isStatic);
        }

        /// <summary>
        /// 注册mono类型的实例(Mono无瞬时实例)
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="obj">注册的发起者</param>
        /// <param name="isStatic">是否是全局模式的mono实例
        /// True：全局实例，可访问
        /// False：只有当前分支可访问
        /// </param>
        [Obsolete]
        public void RegisterMono<T>(GameObject obj, bool isStatic = false) where T : MonoBehaviour
        {
            Type type = typeof(T);
            T instance;
            if (!isStatic)
            {
                if (obj == null)
                {
                    obj = new GameObject()
                    {
                        name = typeof(T).Name,
                    };
                    instance = obj.AddComponent<T>();
                }
                else
                {
                    instance = obj.GetComponent<T>();
                    if (instance == null) instance = obj.AddComponent<T>();
                }              
            }
            else
            {
                var list = GameObject.FindObjectsOfType<T>();
                if (list != null && list.Length != 1)
                {
                    foreach (var info in list)
                    {
                        UnityEngine.Object.Destroy(info.gameObject);
                    }
                    instance = new GameObject()
                    {
                        name = typeof(T).Name,
                    }.AddComponent<T>();
                }
                else
                {
                    instance = UnityEngine.Object.FindObjectOfType<T>();
                }
               
            }
            if (instance != null)
            {
                if(!isStatic)
                Container.AddScopeInstance(type, instance);
                else Container.AddSingleton(type, instance);

                if (instance != null && IsDebugLog)
                {
                    if(isStatic)
                    Debug.Log("Mono实例注册成功！类型：" + instance.GetType() + "Mono模式：" + "单例");
                    else Debug.Log("Mono实例注册成功！类型：" + instance.GetType() + "Mono模式：" + "组件");
                }
                else if(instance == null) throw new NullReferenceException("Mono单例注册失败！类型：" + type);
            }

        }
        #endregion

        #region 注册GameObject实例
        public void AutoRegisterGameObject(GameObject gameObject)
        {
            Container.AddGameObject(gameObject);
            if (IsDebugLog)
                Debug.Log($"自动化注入GameObject,Obj为：{gameObject.name}");
        }

        public void RegisterGameObject(GameObject gameObject)
        {
            Container.AddGameObject(gameObject);
            if (IsDebugLog)
                Debug.Log($"注入GameObject,Obj为：{gameObject.name}");
        }
        #endregion
    }

}
