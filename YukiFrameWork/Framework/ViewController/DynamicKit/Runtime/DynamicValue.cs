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
    public class DynamicValue 
    {
        internal static void Inject(DynamicViewController viewController)
        {
            // CheckDynamicViewController(viewController);

            InjectAllFields(viewController.GetType(),viewController);

        }

        private static void InjectAllFields(Type type, DynamicViewController viewController)
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
                            obj = viewController.GetComponentInChildren(fieldType, !valueAttribute.OnlyMonoEnable);                            
                        }
                        else
                        {
                            obj = viewController.GetComponent(fieldType);
                        }                    
                    }
                    else
                    {
                        var setObj = viewController.Find(valueAttribute.ChildObjName,!valueAttribute.OnlyMonoEnable);
                        if (!setObj)
                            throw new NullReferenceException("特性标记的对象为空，无法查找组件! objName:" + valueAttribute.ChildObjName);
                        obj = setObj.GetComponent(field.FieldType);
                    }

                    field.SetValue(viewController, obj);
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
                        field.SetValue(viewController, obj);
                    }
                    else 
                    {
                        var obj = SceneManager.GetActiveScene().FindRootGameObject(sceneValueAttribute.SceneObjName).GetComponent(fieldType);
                        field.SetValue(viewController, obj);
                    }
                }
            }
        }
    }



}
