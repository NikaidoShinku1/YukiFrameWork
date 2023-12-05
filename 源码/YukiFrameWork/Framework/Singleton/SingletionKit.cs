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

namespace YukiFrameWork
{
    public interface ISingletonKit
    {
        void OnInit();
        void OnDestroy();
    }

    public interface ISingletonKit<T> : ISingletonKit
    {
        
    }

    public class Singleton
    {
        public static T GetInstance<T>() where T : class
            => SingletonFectory.CreateSingleton<T>();     

        public static T GetMonoInstance<T>() where T : MonoBehaviour
            => SingletonFectory.CreateMonoSingleton<T>();             

        public static T GetScriptableInstance<T>() where T : ScriptableObject
            => SingletonFectory.CreateScriptableObjectSinleton<T>();
    }

    public static class SingletonFectory
    {
        private static readonly Dictionary<Type, object> singletonInstance = DictionaryPools<Type, object>.Get();
        public static T CreateSingleton<T>() where T : class
        {
            return CreateNoPublicConstructorObject<T>();
        }

        private static T CreateNoPublicConstructorObject<T>() where T : class
        {
            Type type = typeof(T);
            if (singletonInstance.TryGetValue(type, out var instance))
            {
                return (T)instance;
            }

            var constructorInfoes = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);

            var constructor = constructorInfoes.Where(x => x.GetParameters().Length == 0).FirstOrDefault();

            if (constructor == null)
                throw new NullReferenceException("当前类没有对应的私有构造函数，无法创建对应实例！Not Public Type：" + type);

            instance = constructor.Invoke(null);

            return instance as T;
        }

        public static T CreateMonoSingleton<T>() where T : MonoBehaviour
        {
            return CreateMonoConstructorObject<T>();
        }

        private static T CreateMonoConstructorObject<T>() where T : MonoBehaviour
        {
            if (singletonInstance.TryGetValue(typeof(T), out var instance))
            {
                return (T)instance; 
            }
#if UNITY_2020_1_OR_NEWER
            instance = UnityEngine.Object.FindObjectOfType<T>(true);
#endif
            if (instance == null)
            {
                GameObject obj = new GameObject
                {
                    name = typeof(T).Name
                };
                instance = obj.AddComponent<T>();
            }
            singletonInstance.Add(typeof(T), instance);
            return (T)instance;
        }

        public static T CreateScriptableObjectSinleton<T>() where T : ScriptableObject
        {
            return CreateScriptableConstructorObject<T>();           
        }

        private static T CreateScriptableConstructorObject<T>() where T : ScriptableObject
        {
            Type type = typeof(T);
            if (singletonInstance.TryGetValue(type, out var instance))
            {
                return instance as T;
            }
            instance = ScriptableObject.CreateInstance<T>();

            return (T)instance;
        }
    }
}