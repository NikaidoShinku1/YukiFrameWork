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
        /// 单例初始化方法，如果使用Mono单例父类则效力等同Awake
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

    public class Singleton
    {
        public static T GetInstance<T>() where T : class, ISingletonKit
            => SingletonFectory.GetSingleton<T>();

        public static ISingletonKit GetInstance(Type type)
            => SingletonFectory.GetSingleton(type);

        public static T GetMonoInstance<T>(bool isDonDestoryLoad = false) where T : Component,ISingletonKit
            => SingletonFectory.GetMonoSingleton<T>(isDonDestoryLoad);

        public static ISingletonKit GetMonoInstance(Type type, bool isDonDestoryLoad = false)
            => SingletonFectory.GetMonoSingleton(type,isDonDestoryLoad);

        public static T GetScriptableInstance<T>() where T : ScriptableObject, ISingletonKit
            => SingletonFectory.GetScriptableSingleton<T>();

        public static ISingletonKit GetScriptableInstance(Type type)
            => SingletonFectory.GetScriptableSingleton(type);
    }

    public static class SingletonFectory
    {
        private static readonly Dictionary<Type,ISingletonKit> singletonInstance = DictionaryPools<Type,ISingletonKit>.Get();
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

        public static T CreateMonoSingleton<T>(bool IsDonDestoryLoad = false) where T : Component, ISingletonKit
        {
            return CreateMonoConstructorObject(typeof(T),IsDonDestoryLoad) as T;
        }
        public static ISingletonKit CreateMonoSingleton(Type type)
            => CreateMonoConstructorObject(type);
        private static ISingletonKit CreateMonoConstructorObject(Type type, bool IsDonDestoryLoad = false)
        {
            if (singletonInstance.TryGetValue(type, out var instance))
            {
                return instance;
            }
#if UNITY_2020_1_OR_NEWER
            instance = UnityEngine.Object.FindObjectOfType(type,true) as ISingletonKit;
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
        public static T GetSingleton<T>() where T : class, ISingletonKit
        {
            return GetSingleton(typeof(T)) as T;
        }

        public static ISingletonKit GetSingleton(Type type)
        {
            singletonInstance.TryGetValue(type, out var instance);
            if (instance == null)
            {
                instance = CreateNoPublicConstructorObject(type);
                if (instance == null)                
                    Debug.LogError("创建单例失败，请检查单例是否继承ISingletonKit接口!");
                else
                    instance.OnInit();
            }           
            return instance;
        }

        public static T GetMonoSingleton<T>(bool isDonDestoryLoad = false) where T : Component, ISingletonKit
        {
            return GetMonoSingleton(typeof(T), isDonDestoryLoad) as T;
        }

        public static ISingletonKit GetMonoSingleton(Type type, bool isDonDestoryLoad = false)
        {
            singletonInstance.TryGetValue(type, out var instance);
            if (instance == null)
            {
                instance = CreateMonoConstructorObject(type,isDonDestoryLoad);
                if (instance == null)
                    Debug.LogError("创建单例失败，请检查单例是否继承ISingletonKit接口!");
                else
                    instance.OnInit();
            }
           
            return instance;
        }     
        public static T GetScriptableSingleton<T>() where T : ScriptableObject, ISingletonKit
        {
            return GetScriptableSingleton(typeof(T)) as T;
        }

        public static ISingletonKit GetScriptableSingleton(Type type)      
        {
            singletonInstance.TryGetValue(type, out var instance);
            if (instance == null)
            {
                instance = CreateScriptableConstructorObject(type);
                if (instance == null)
                    Debug.LogError("创建单例失败，请检查单例是否继承ISingletonKit接口!");
                else
                    instance.OnInit();
            }          

            return instance;
        }
        #endregion
    }
}