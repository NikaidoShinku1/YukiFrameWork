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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using YukiFrameWork.Events;
using YukiFrameWork.Extension;
namespace YukiFrameWork
{
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


    public abstract class AbstractController : IController
    {
        private object _object = new object();
        private IArchitecture mArchitecture;
     
        /// <summary>
        /// 可重写的架构属性,不使用特性初始化时需要重写该属性
        /// </summary>
        protected virtual IArchitecture RuntimeArchitecture
        {
            get
            {
                lock (_object)
                {
                    if(mArchitecture == null)
                        Build();
                    return mArchitecture;
                }
            }
        }       
        IArchitecture IGetArchitecture.GetArchitecture()
        {
            return RuntimeArchitecture;
        }
       
       internal void Build()
        {
            if (mArchitecture == null)
            {
                mArchitecture = ArchitectureConstructor.I.Enquene(this);
            }            
        }
        /// <summary>
        /// 框架封装AbstractController自带的OnInit初始化方法
        /// </summary>
        public abstract void OnInit();     
    }

    public partial class ViewController : ISerializedFieldInfo
    {
        [HideInInspector]
        public CustomData Data = new CustomData();   
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

        SerializeFieldData ISerializedFieldInfo.Find(Func<SerializeFieldData, bool> func)
        {
            for (int i = 0; i < _fields.Count; i++)
            {
                if (func(_fields[i]))
                {
                    return _fields[i];
                }
            }

            return null;
        }
        #endregion
    }
    public partial class ViewController : YMonoBehaviour, IController
    {
        private object _object = new object();
        private IArchitecture mArchitecture;                   
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
                    }
                    return mArchitecture;
                }
            }
        }              
        IArchitecture IGetArchitecture.GetArchitecture()
        {
            return RuntimeArchitecture;
        }

        protected virtual void OnDestroy()
        {
            mArchitecture = null;
        }

#if UNITY_EDITOR || DEBUG || DEBUG
        /// <summary>
        /// 默认返回False，如果需要自定义编辑器拓展则在这里写逻辑后返回True即可;
        /// <para>Tip:该方法需要在UnityEditor宏定义下重写</para>
        /// </summary>
        /// <returns></returns>
        public virtual bool OnInspectorGUI()
        {
            return false; //默认返回False，如果需要自定义编辑器拓展则在这里写逻辑后返回True即可;
        }

        /// <summary>
        /// 默认返回False，如果需要自定义场景拓展则可以重写该方法后返回True即可
        /// <para>Tip:该方法需要在UnityEditor宏定义下重写</para>
        /// </summary>
        /// <returns></returns>
        public virtual bool OnSceneGUI()
        {
            return false; //默认返回False，如果需要自定义场景拓展则可以重写该方法后返回True即可
        }
#endif
    }
}