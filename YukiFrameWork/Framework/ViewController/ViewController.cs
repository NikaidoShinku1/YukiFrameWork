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

using System.Collections.Generic;
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
        private bool isAutoMation = true;
        [SerializeField]
        private int autoArchitectureIndex;
        [SerializeField]
        private List<string> autoInfos = new List<string>();
        [SerializeField]
        private string assetPath = "Assets";
        [SerializeField]
        private bool isCustomAssembly = false;

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
        public bool IsAutoMation
        {
            get => isAutoMation;
            set => isAutoMation = value;
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

    public partial class ViewController : MonoBehaviour, IController
    {
        private object _object = new object();
        private IArchitecture mArchitecture;

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