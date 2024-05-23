///=====================================================
/// - FileName:      ViewController.cs
/// - NameSpace:     YukiFrameWork
/// - Created:       Yuki
/// - Email:         1274672030@qq.com
/// - Description:   控制器基类
/// - Creation Time: 2024/1/14 17:14:33
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using YukiFrameWork.Events;
using YukiFrameWork.Extension;
namespace YukiFrameWork
{
    public enum RuntimeInitialized
    {
        Automation,
        Awake,        
    }

    [System.Serializable]
    public class CustomData : GenericDataBase
    {
        [SerializeField]
        private string completedFile;       
        
        [SerializeField]
        private int autoArchitectureIndex;
        [SerializeField]
        private List<string> autoInfos = new List<string>();
        [SerializeField]
        private string assetPath = "Assets";
        [SerializeField]
        private bool isCustomAssembly = false;

        private List<string> parents = new List<string>();

        private int selectIndex = 0;

        public string AssetPath
        {
            get { return assetPath; }
            set { assetPath = value; }
        }

        public string CompletedFile
        {
            get => completedFile;
            set => completedFile = value;
        }            

        public int AutoArchitectureIndex
        {
            get => autoArchitectureIndex;
            set => autoArchitectureIndex = value;
        }

        public bool IsCustomAssembly
        {
            get => isCustomAssembly;
            set => isCustomAssembly = value;
        }

        public List<string> AutoInfos => autoInfos;

        public List<string> Parent => parents;

        public int SelectIndex
        {
            get => selectIndex;
            set => selectIndex = value;
        }

        private bool isPartialLoading = false;

        public bool IsPartialLoading
        {
            get => isPartialLoading;
            set => isPartialLoading = value;
        }

    }

    public partial class ViewController : ISerializedFieldInfo
    {
        [HideInInspector]
        public CustomData Data = new CustomData();

        [HideInInspector]
        public RuntimeInitialized initialized = RuntimeInitialized.Automation;

        #region ISerializedFieldInfo
        [HideInInspector]
        [SerializeField]
        private List<SerializeFieldData> _fields = new List<SerializeFieldData>();
        void ISerializedFieldInfo.AddFieldData(SerializeFieldData data)
            => _fields.Add(data);

        void ISerializedFieldInfo.RemoveFieldData(SerializeFieldData data)
            => _fields.Remove(data);

        void ISerializedFieldInfo.ClearFieldData()
            => _fields.Clear();

        IEnumerable<SerializeFieldData> ISerializedFieldInfo.GetSerializeFields() => _fields;
        #endregion 
    }

    public partial class ViewController : Sirenix.OdinInspector.SerializedMonoBehaviour, IController
    {
        private object _object = new object();
        private IArchitecture mArchitecture;

        private bool fieldInitting = false;

        [SerializeField,LabelText("是否启动自动赋值功能")]
        [InfoBox("自动赋值功能开启将可以让所有对应挂载了指定特性的字段/属性进行赋值(属性必须有Setter!)\n但请注意，性能的开销是必不可少的，请选择性开启",InfoMessageType.Warning)]
        private bool IsAutoSettingField;

        [SerializeField, LabelText("特性信息"), ShowIf(nameof(IsAutoSettingField)),TableList,ReadOnly]
        [InfoBox("特性同时只能挂一个，多个组件并用会出问题")]
        private ValueDropdownList<string> infos
            = new ValueDropdownList<string>()
            {
                { "VGetComponent","形同GetComponent" },
                { "VGetOrAddComponent","形同GetComponent,如果找不到则添加一个新组件"},
                { "VAddComponent","形同AddComponent" },
                { "VFindObjectOfType","形同FindObjectOfType" }
            };
        protected virtual void Awake()
        {
            try
            {
                if (initialized == RuntimeInitialized.Awake)
                    RuntimeEventInitializationFactory.Initialization(this);
            }
            catch
            {
                throw new System.NullReferenceException($"无法在Awake下注册事件，请检查是否在{GetType()}中正确实现了架构!");
            }
            InitAllFields();
        }

        void InitAllFields()
        {
            if (fieldInitting || !IsAutoSettingField) return;
            fieldInitting = true;
            MemberInfo[] memberInfo = this.GetType().GetRuntimeMemberInfos().ToArray();

            for (int i = 0; i < memberInfo.Length; i++)
            {
                var member = memberInfo[i];
                if (member == null) continue;
                BaseComponentAttribute attribute = member.GetCustomAttributes<BaseComponentAttribute>(true).FirstOrDefault();
         
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
                    else if (attribute is VFindObjectOfType vFind)
                    {
#if UNITY_2021_1_OR_NEWER
                        fieldInfo.SetValue(this,FindAnyObjectByType(fieldInfo.FieldType,vFind.ObjectsInactive));
#else
                        fieldInfo.SetValue(this, FindObjectOfType(fieldInfo.FieldType, vFind.includeInactive));
#endif
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
                    else if (attribute is VFindObjectOfType vFind)
                    {
#if UNITY_2021_1_OR_NEWER
                        propertyInfo.SetValue(this, FindAnyObjectByType(propertyInfo.PropertyType, vFind.ObjectsInactive));
#else
                        propertyInfo.SetValue(this, FindObjectOfType(propertyInfo.PropertyType, vFind.includeInactive));
#endif
                    }
                }
            }
        }

        /// <summary>
        /// 可重写的架构属性,不使用特性初始化时需要重写该属性
        /// </summary>
        protected virtual IArchitecture RuntimeArchitecture
        {
            get
            {
                lock (_object)
                {
                    if (mArchitecture == null)
                    {
                        mArchitecture = ArchitectureConstructor.I.Enquene(this);
                        if(mArchitecture != null && initialized == RuntimeInitialized.Automation)
                            RuntimeEventInitializationFactory.Initialization(this);
                        InitAllFields();
                    }
                    return mArchitecture;
                }
            }
        }      
        IArchitecture IGetArchitecture.GetArchitecture()
        {
            return RuntimeArchitecture;
        }            
    }
}