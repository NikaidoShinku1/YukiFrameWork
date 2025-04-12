///=====================================================
/// - FileName:      SingletionKit.cs
/// - NameSpace:     YukiFrameWork.Project
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   单例管理套件
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using YukiFrameWork.Pools;
using YukiFrameWork.Extension;
namespace YukiFrameWork
{
    /// <summary>
    /// 使用非父类继承单例时必须继承这个接口
    /// </summary>
    public interface ISingletonKit
    {
        /// <summary>
        /// 单例初始化方法,Mono类如果提前挂在项目中请使用Awake
        /// </summary>
        [MethodAPI("单例初始化方法")]
        void OnInit();

        /// <summary>
        /// 单例销毁方法,如果作用Mono则效力等同OnDestroy
        /// </summary>
        [MethodAPI("单例销毁")]
        void OnDestroy();
    }

    public interface ISingletonKit<T> : ISingletonKit
    {
       
    }

    public static class SingletonFectory
    {
        private static readonly Dictionary<Type,ISingletonKit> singletonInstance = DictionaryPools<Type,ISingletonKit>.Get();

        public static void ClearCache()
        {
            foreach (var item in singletonInstance.Values.ToArray())
            {
                item.OnDestroy();
            }

            singletonInstance.Clear();
        }
        static SingletonFectory()
        {
            singletonInstance.Clear();
        }
        #region 创建实例
        public static T CreateSingleton<T>() where T : class, ISingletonKit
        {
            return CreateNoPublicConstructorObject<T>();
        }

        public static object CreateSingleton(Type type)
            => CreateNoPublicConstructorObject(type);

        private static T CreateNoPublicConstructorObject<T>() where T : class, ISingletonKit
        {
            return CreateNoPublicConstructorObject(typeof(T)) as T;
        }
        private static ISingletonKit CreateNoPublicConstructorObject(Type type)
        {
            ISingletonKit instance = null;
            if (singletonInstance.TryGetValue(type, out instance))
            {
                return instance;
            }
            var constructorInfoes = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);

            var constructor = constructorInfoes.Where(x => x.GetParameters().Length == 0).FirstOrDefault();

            if (constructor == null)
                throw new NullReferenceException("当前类没有对应的私有构造函数，无法创建对应实例！Not Public Constructor：" + type);

            instance = constructor.Invoke(null) as ISingletonKit;
            singletonInstance.Add(type, instance);
            return instance;
        }

        public static T CreateMonoSingleton<T>(bool IsDonDestoryLoad = false) where T : MonoBehaviour, ISingletonKit
        {
            return CreateMonoConstructorObject(typeof(T),IsDonDestoryLoad) as T;
        }
        public static ISingletonKit CreateMonoSingleton(Type type, bool IsDonDestoryLoad = false)
            => CreateMonoConstructorObject(type, IsDonDestoryLoad);
        private static ISingletonKit CreateMonoConstructorObject(Type type, bool IsDonDestoryLoad = false)
        {
            if (singletonInstance.TryGetValue(type, out var instance))
            {
                return instance;
            }
#if UNITY_2023_1_OR_NEWER
            instance = UnityEngine.Object.FindAnyObjectByType(type, FindObjectsInactive.Include) as ISingletonKit;
#elif UNITY_2020_1_OR_NEWER
            instance = UnityEngine.Object.FindObjectOfType(type, true) as ISingletonKit;
#endif
            if (instance == null)
            {
                GameObject obj = new GameObject(type.Name);
                if (IsDonDestoryLoad) UnityEngine.Object.DontDestroyOnLoad(obj);
                instance = obj.AddComponent(type) as ISingletonKit;
            }
            singletonInstance.Add(type, instance);
            return instance;
        }

        public static T CreateScriptableObjectSingleton<T>() where T : ScriptableObject, ISingletonKit
        {
            return CreateScriptableConstructorObject<T>();           
        }

        public static object CreateScriptableObjectSingleton(Type type)
        {
            return CreateScriptableConstructorObject(type);
        }

        private static T CreateScriptableConstructorObject<T>() where T : ScriptableObject, ISingletonKit
        {
            return CreateScriptableConstructorObject(typeof(T)) as T;
        }

        private static ISingletonKit CreateScriptableConstructorObject(Type type)
        {          
            if (singletonInstance.TryGetValue(type, out var instance))
            {
                return instance;
            }
            instance = ScriptableObject.CreateInstance(type) as ISingletonKit;
            singletonInstance.Add(type, instance);
            return instance;
        }
        #region 释放实例
        public static void ReleaseInstance<T>() where T : ISingletonKit
        {
            ReleaseInstance(typeof(T));           
        }

        public static void ReleaseInstance(Type type)
        {
            if (singletonInstance.ContainsKey(type))
            {
                singletonInstance[type] = null;
                singletonInstance.Remove(type);
            }
        }
        #endregion
        #endregion
        #region 获取实例

        #endregion
    }

    public class SingletonProperty<T> where T :class, ISingletonKit
    {
        private static T instance;

        private static object mLock = new object();

        /// <summary>
        /// 获取实例，
        /// </summary>        
        public static T GetInstance()
        {
            lock (mLock)
            {               
                if (instance == null)
                {
                    if (typeof(T).IsSubclassOf(typeof(MonoBehaviour)))                  
                        instance = SingletonFectory.CreateMonoSingleton(typeof(T), false) as T;
                    
                    else if (typeof(T).IsSubclassOf(typeof(ScriptableObject)))                    
                        instance = SingletonFectory.CreateScriptableObjectSingleton(typeof(T)) as T;                   
                    else
                        instance = SingletonFectory.CreateSingleton<T>();

                    instance.OnInit();
                }
                return instance;
            }
        }

        public static T Instance => GetInstance();
    }
}