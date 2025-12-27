///=====================================================
/// - FileName:      DynamicValue.cs
/// - NameSpace:     YukiFrameWork.Example
/// - Description:   高级定制脚本生成
/// - Creation Time: 12/25/2025 10:19:46 PM
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
namespace YukiFrameWork
{
    public interface IDynamicMonoBehaviour
    {
        Transform transform { get; }
        GameObject gameObject { get; }
    }
    public class DynamicValue 
    {
        #region static API
        internal static void Inject(IDynamicMonoBehaviour monoBehaviour)
        {
            InjectAllFields(monoBehaviour.GetType(),monoBehaviour);

        }
        private static void InjectAllFields(Type type, IDynamicMonoBehaviour monoBehaviour)
        {          
            var fields = type.GetRealFields();

            foreach (var field in fields)
            {
                if (field.HasCustomAttribute<DynamicValueAttribute>(true, out var valueAttribute))
                {
                    Type fieldType = field.FieldType;
                    if (fieldType != typeof(Component) && !fieldType.IsSubclassOf(typeof(Component)))
                    {
                        Debug.LogWarning("标记的对象不是组件，无法进行赋值，已跳过 FieldType:" + fieldType);
                        continue;
                    }
                    Component obj = null;

                   
                    if (valueAttribute.ChildObjName.IsNullOrEmpty())
                    {
                        if (valueAttribute.FindAllChild)
                        {                          
                            obj = monoBehaviour.transform.GetComponentInChildren(fieldType, !valueAttribute.OnlyMonoEnable);                            
                        }
                        else
                        {
                            obj = monoBehaviour.transform.GetComponent(fieldType);
                        }                    
                    }
                    else
                    {
                        var setObj = monoBehaviour.transform.Find(valueAttribute.ChildObjName,!valueAttribute.OnlyMonoEnable);
                        if (!setObj)
                            throw new NullReferenceException("特性标记的对象为空，无法查找组件! objName:" + valueAttribute.ChildObjName);
                        obj = setObj.GetComponent(field.FieldType);
                    }

                    field.SetValue(monoBehaviour, obj);
                }

                if (field.HasCustomAttribute<DynamicValueFromSceneAttribute>(true, out var sceneValueAttribute))
                {
                    Type fieldType = field.FieldType;
                    if (fieldType != typeof(Component) && !fieldType.IsSubclassOf(typeof(Component)))
                    {
                        Debug.LogWarning("标记的对象不是组件，无法进行赋值，已跳过 FieldType:" + fieldType);
                        continue;
                    }

                    if (sceneValueAttribute.SceneObjName.IsNullOrEmpty())
                    {
                        var obj = UnityEngine.Object.FindAnyObjectByType(fieldType, sceneValueAttribute.OnlyMonoEnable ? FindObjectsInactive.Exclude : FindObjectsInactive.Include);
                        field.SetValue(monoBehaviour, obj);
                    }
                    else 
                    {
                        var obj = SceneManager.GetActiveScene().FindRootGameObject(sceneValueAttribute.SceneObjName).GetComponent(fieldType);
                        field.SetValue(monoBehaviour, obj);
                    }
                }
            }
        }

        /// <summary>
        /// 创建一个空的动态字段
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static DynamicValue Empty()
        {
            return new DynamicValue();
        }

        /// <summary>
        /// 通过GameObject创建一套自动绑定自身所有组件的动态字段，可自取
        /// </summary>
        /// <param name="gameObject"></param>
        /// <returns></returns>
        public static DynamicValue Create(GameObject gameObject)
        {
            var components = gameObject.GetComponents<Component>();

            DynamicValue dynamicValue = new DynamicValue();
            foreach (var item in components)
            {
                dynamicValue.Inject(item.GetType(), item);
            }

            return dynamicValue;
        }
        #endregion

        protected DynamicValue() { }

        private readonly Dictionary<Type, object> mInstances = new Dictionary<Type, object>();

        /// <summary>
        /// 转换为指定类型，任何类型都可以进行转换,最终至少会返回(T)default;
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T As<T>()
        {
            try
            {
                return (T)As(typeof(T));
            }
            catch 
            {
                return default(T);
            }
        }

        public object As(Type type)
        {
            try
            {
                if (Convert(type, out object value))
                {
                    return value;
                }
            }
            catch
            {

            }
            return default;
        }

        public object this[Type type]
        {
            get => As(type);
        }

        /// <summary>
        /// 为动态字段注入值,如值已存在则覆盖
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        public void Inject<T>(T value)
        {
            Inject(typeof(T), value);
        }
        /// <summary>
        /// 为动态字段注入值,如值已存在则覆盖
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        public void Inject(Type type, object value)
        {
            if (mInstances.ContainsKey(type))
                mInstances[type] = value;
            else mInstances.Add(type, value);
        }

        /// <summary>
        /// 判断该类型是否在DynamicValue上进行注册，如有值,即使值为default，则返回True
        /// <para>包括default在内，都返回False</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool Is<T>()
        {
            return CheckInstanceByDynamic(typeof(T),out _);
        }

        /// <summary>
        /// 判断该类型是否在DynamicValue上进行注册，如有值,即使值为default，则返回True
        /// <para>包括default在内，都返回False</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool Is<T>(out T value)
        {
            value = (T)default;
            bool v = CheckInstanceByDynamic(typeof(T), out object obj);
            if (obj is T t)
            {
                value = t;
                return true;
            }

            return false;
        }

        private bool CheckInstanceByDynamic(Type type,out object obj)
        {
            if (mInstances.TryGetValue(type, out obj))
            {                
                return true;
            }
            return false;
        }

        private bool Convert(Type type,out object value)
        {
            value = default;
            if (mInstances.TryGetValue(type, out var t))
            {
                value = t;
                return true;
            }

            return false;
        }

       


    }
}
