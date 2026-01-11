///=====================================================
/// - FileName:      DynamicValue.cs
/// - NameSpace:     YukiFrameWork.Example
/// - Description:   高级定制脚本生成
/// - Creation Time: 12/25/2025 10:19:46 PM
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Reflection;
namespace YukiFrameWork
{
    public interface IDynamicMonoBehaviour
    {
        Transform transform { get; }
        GameObject gameObject { get; }
    }
    public class DynamicValue 
    {
        public static void Inject(IDynamicMonoBehaviour monoBehaviour)
        {
            InjectAllFields(monoBehaviour.GetType(),monoBehaviour);
        }

        private static Dictionary<Type, IDynamicRegulation> dynamicRegulationCache = new Dictionary<Type, IDynamicRegulation>();

        private static object CreateField(IDynamicMonoBehaviour monoBehaviour,Type fieldType,DynamicValueAttribute valueAttribute)
        {
            if (fieldType != typeof(Component) && !fieldType.IsSubclassOf(typeof(Component)))
            {
                Debug.LogWarning("标记的对象不是组件，无法进行赋值，已跳过 FieldType:" + fieldType);
                return null;
            }
            object obj = null;

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
                var setObj = monoBehaviour.transform.Find(valueAttribute.ChildObjName, !valueAttribute.OnlyMonoEnable);
                if (!setObj)
                    throw new NullReferenceException("特性标记的对象为空，无法查找组件! objName:" + valueAttribute.ChildObjName);
                obj = setObj.GetComponent(fieldType);             
            }
            return obj;

        }
        private static object CreateFieldFromScene(Type fieldType,DynamicValueFromSceneAttribute sceneValueAttribute)
        {
            if (fieldType != typeof(Component) && !fieldType.IsSubclassOf(typeof(Component)))
            {
                Debug.LogWarning("标记的对象不是组件，无法进行赋值，已跳过 FieldType:" + fieldType);
                return null;
            }

            if (sceneValueAttribute.SceneObjLabel.IsNullOrEmpty())
            {
                var obj = UnityEngine.Object.FindAnyObjectByType(fieldType, sceneValueAttribute.OnlyMonoEnable ? FindObjectsInactive.Exclude : FindObjectsInactive.Include);
                return obj;
            }
            else
            {
                Component obj = null;
                switch (sceneValueAttribute.DynamicValueFromSceneMode)
                {
                    case DynamicValueFromSceneMode.Path:
                        obj = GameObject.Find(sceneValueAttribute.SceneObjLabel).GetComponent(fieldType);
                        break;
                    case DynamicValueFromSceneMode.Name:
                        obj = SceneManager.GetActiveScene().FindRootGameObject(sceneValueAttribute.SceneObjLabel).GetComponent(fieldType);
                        break;
                    case DynamicValueFromSceneMode.Tag:
                        obj = GameObject.FindGameObjectWithTag(sceneValueAttribute.SceneObjLabel).GetComponent(fieldType);
                        break;
                    default:
                        break;
                }
                return obj;
            }
        }

        private static IDynamicRegulation GetDynamicRegulation(Type type)
        {
            if (type == null)
            {
                throw new Exception("检测到自定义参数注入的规则器类型为空,无法进行赋值,请尝试重新编译 parameterRegulationType:Null");
            }

            if (!typeof(IDynamicRegulation).IsAssignableFrom(type))
            {
                Debug.LogWarning("类型不继承规则接口IDynamicRegulation Type:" + type);
                return null;
            }

            if (dynamicRegulationCache.TryGetValue(type, out var value))           
                return value;

            value = type.CreateInstance() as IDynamicRegulation;

            if (value != null)
                dynamicRegulationCache[type] = value;

            return value;
           
        }

        private static void GeneratorParameter(MethodInfo methodInfo,ParameterInfo[] parameters,IDynamicMonoBehaviour monoBehaviour)
        {
            object[] values = new object[parameters.Length];

            for (int j = 0; j < parameters.Length; j++)
            {
                var parameter = parameters[j];

                object obj = null;
                if (parameter.HasCustomAttribute<DynamicRegulationAttribute>(true, out var parameterAttribute))
                {
                    IDynamicRegulation dynamicParameterRegulation = GetDynamicRegulation(parameterAttribute.RegulationType);

                    if (dynamicParameterRegulation != null)
                    {
                        obj = dynamicParameterRegulation.Build(parameter.ParameterType, monoBehaviour);
                    }
                }
                else if (parameter.HasCustomAttribute<DynamicValueAttribute>(true, out var valueAttribute))
                {
                    obj = CreateField(monoBehaviour, parameter.ParameterType, valueAttribute);                                 
                }
                else if (parameter.HasCustomAttribute<DynamicValueFromSceneAttribute>(true,out var valueFromSceneAttribute))
                {
                    obj = CreateFieldFromScene(parameter.ParameterType, valueFromSceneAttribute);
                }
                else
                {
                    if (parameter.ParameterType != typeof(UnityEngine.Object) && !parameter.ParameterType.IsSubclassOf(typeof(UnityEngine.Object)))
                    {
                        Debug.LogWarning("对象非UnityEngine.Object类型,无法进行赋值,请为该参数添加DynamicParameter特性自定义规则。Parameter:" + parameter.ParameterType);
                    }
                    else
                    {
                        obj = monoBehaviour.transform.GetComponent(parameter.ParameterType);
                    }
                }
                values[j] = obj;
            }

            methodInfo.Invoke(monoBehaviour,values);
        }
        private static void InjectAllFields(Type type, IDynamicMonoBehaviour monoBehaviour)
        {
            if (typeof(IDynamicBuilder).IsAssignableFrom(type))
            {
                var methods = type
                    .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                for (int i = 0; i < methods.Length; i++)
                {
                    MethodInfo methodInfo = methods[i];

                    if (methodInfo.Name != "Builder") continue;

                    var parameters = methodInfo.GetParameters();

                    if (parameters.Length == 0)
                    {
                        methodInfo.Invoke(monoBehaviour, null);
                        continue;
                    }
                    GeneratorParameter(methodInfo, parameters, monoBehaviour);                  
                }
                    
                
            }

            var fields = type.GetRealFields();

            foreach (var field in fields)
            {
                object obj = null;
                if (field.HasCustomAttribute<DynamicRegulationAttribute>(true, out var regulationAttribute))
                {
                    IDynamicRegulation dynamicParameterRegulation = GetDynamicRegulation(regulationAttribute.RegulationType);

                    if (dynamicParameterRegulation != null)                   
                        obj = dynamicParameterRegulation.Build(field.FieldType, monoBehaviour);                                           
                }
                else if (field.HasCustomAttribute<DynamicValueAttribute>(true, out var valueAttribute))
                {
                    Type fieldType = field.FieldType;

                    obj = CreateField(monoBehaviour, fieldType, valueAttribute);                              
                }
                else if (field.HasCustomAttribute<DynamicValueFromSceneAttribute>(true, out var sceneValueAttribute))
                {
                    Type fieldType = field.FieldType;
                    obj = CreateFieldFromScene(fieldType, sceneValueAttribute);                                     
                }
                if (obj == null) 
                    continue;
                field.SetValue(monoBehaviour, obj);

            }
        }       
    }
}
