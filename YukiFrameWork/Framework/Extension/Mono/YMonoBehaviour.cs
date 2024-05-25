///=====================================================
/// - FileName:      YMonoBehaviour.cs
/// - NameSpace:     YukiFrameWork.Tower
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/5/24 17:00:06
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using System.Linq;
using System.Reflection;
using UnityEngine.Events;
namespace YukiFrameWork
{
    public interface IYMonoBehaviour
    {
        void InitAllFields();
    }
	public class YMonoBehaviour : Sirenix.OdinInspector.SerializedMonoBehaviour,IYMonoBehaviour
	{
        private bool fieldInitting = false;
 
        [SerializeField, LabelText("是否启动自动赋值功能")]
        [InfoBox("自动赋值功能开启将可以让所有对应挂载了指定特性的字段/属性进行赋值(属性必须有Setter!)\n但请注意，性能的开销是必不可少的，请选择性开启,详情的特性请查阅框架本地窗口的特性示例", InfoMessageType.Warning)]
        private bool IsAutoSettingField;
 
        protected virtual void Awake()
		{
            (this as IYMonoBehaviour).InitAllFields();
        }

        void IYMonoBehaviour.InitAllFields()
        {
            if (fieldInitting || !IsAutoSettingField) return;
            fieldInitting = true;
            MemberInfo[] memberInfo = this.GetType().GetRuntimeMemberInfos().Where(x => x.GetType() != typeof(MethodInfo)).ToArray();

            for (int i = 0; i < memberInfo.Length; i++)
            {
                var member = memberInfo[i];
                if (member == null) continue;
                BaseComponentAttribute attribute = member.GetCustomAttributes<BaseComponentAttribute>(true).FirstOrDefault();
                if (attribute == null) continue;

                if (member is FieldInfo fieldInfo)
                {
                    if (attribute is VGetComponent)
                    {
                        fieldInfo.SetValue(this, GetComponent(fieldInfo.FieldType));
                    }
                    else if (attribute is VAddComponent)
                    {
                        fieldInfo.SetValue(this, gameObject.AddComponent(fieldInfo.FieldType));
                    }
                    else if (attribute is VGetOrAddComponent)
                    {
                        var value = GetComponent(fieldInfo.FieldType);
                        value ??= gameObject.AddComponent(fieldInfo.FieldType);
                        fieldInfo.SetValue(this, value);
                    }
                    else if (attribute is VGetComponentInChildren children)
                    {
                        fieldInfo.SetValue(this, GetComponentInChildren(fieldInfo.FieldType, children.Include));
                    }
                    else if (attribute is VFindObjectOfType vFind)
                    {
#if UNITY_2021_1_OR_NEWER
                        fieldInfo.SetValue(this, FindAnyObjectByType(fieldInfo.FieldType, vFind.ObjectsInactive));
#else
                        fieldInfo.SetValue(this, FindObjectOfType(fieldInfo.FieldType, vFind.includeInactive));
#endif
                    }
                    else if (attribute is VFindChildComponentByName fName)
                    {
                        fieldInfo.SetValue(this, this.Find(fName.name)?.GetComponent(fieldInfo.FieldType));
                    }
                    else if (attribute is VFindChildComponentByPath fPath)
                    {
                        fieldInfo.SetValue(this, transform.Find(fPath.path)?.GetComponent(fieldInfo.FieldType));
                    }
                }
                else if (member is PropertyInfo propertyInfo)
                {
                    if (attribute is VGetComponent)
                    {
                        propertyInfo.SetValue(this, GetComponent(propertyInfo.PropertyType));
                    }
                    else if (attribute is VAddComponent)
                    {
                        propertyInfo.SetValue(this, gameObject.AddComponent(propertyInfo.PropertyType));
                    }
                    else if (attribute is VGetOrAddComponent)
                    {
                        var value = GetComponent(propertyInfo.PropertyType);
                        value ??= gameObject.AddComponent(propertyInfo.PropertyType);
                        propertyInfo.SetValue(this, value);
                    }
                    else if (attribute is VGetComponentInChildren children)
                    {
                        propertyInfo.SetValue(this, GetComponentInChildren(propertyInfo.PropertyType, children.Include));
                    }
                    else if (attribute is VFindObjectOfType vFind)
                    {
#if UNITY_2021_1_OR_NEWER
                        propertyInfo.SetValue(this, FindAnyObjectByType(propertyInfo.PropertyType, vFind.ObjectsInactive));
#else
                        propertyInfo.SetValue(this, FindObjectOfType(propertyInfo.PropertyType, vFind.includeInactive));
#endif
                    }

                    else if (attribute is VFindChildComponentByName fName)
                    {
                        propertyInfo.SetValue(this, this.Find(fName.name)?.GetComponent(propertyInfo.PropertyType));
                    }
                    else if (attribute is VFindChildComponentByPath fPath)
                    {
                        propertyInfo.SetValue(this, transform.Find(fPath.path)?.GetComponent(propertyInfo.PropertyType));
                    }
                }
            }
        }
    }
}
