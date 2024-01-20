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
namespace YukiFrameWork
{
    public enum RuntimeInitialized
    {
        Automation,
        Awake,        
    }

    public partial class ViewController
    {
        [HideInInspector]
        public CustomData Data = new CustomData();

        [HideInInspector]
        public RuntimeInitialized initialized = RuntimeInitialized.Automation;

        [System.Serializable]
        public class CustomData
        {
            [SerializeField]
            private string scriptName;
            [SerializeField]
            private string scriptPath = @"Assets/Scripts";
            [SerializeField]
            private string scriptNamespace = "YukiFrameWork.Project";
            [SerializeField]
            private string createName = "Yuki";
            [SerializeField]
            private string createEmail = "1274672030@qq.com";
            [SerializeField]
            private string completedFile;
            [SerializeField]
            private string systemNowTime;
            [SerializeField]
            private string description = "这是一个框架工具创建的脚本";

            [SerializeField]
            private bool onLoading = false;

            [SerializeField]
            private string assetPath = "Assets";

            public string AssetPath
            {
                get { return assetPath; }
                set { assetPath = value; }
            }

            public string ScriptName
            {
                get => scriptName;
                set => scriptName = value;
            }

            public string ScriptPath
            {
                get => scriptPath;
                set => scriptPath = value;
            }

            public string ScriptNamespace
            {
                get => scriptNamespace;
                set => scriptNamespace = value;
            }

            public string CompletedFile
            {
                get => completedFile;
                set => completedFile = value;
            }

            public string SystemNowTime
            {
                get => systemNowTime;
                set => systemNowTime = value;
            }

            public string Description
            {
                get => description;
                set => description = value;
            }

            public string CreateName
            {
                get => createName;
                set => createName = value;
            }

            public string CreateEmail
            {
                get => createEmail;
                set => createEmail = value;
            }

            public bool OnLoading
            {
                get => onLoading;
                set => onLoading = value;
            }

        }       
    }

    public partial class ViewController : MonoBehaviour, IController
    {
        private object _object = new object();
        private IArchitecture mArchitecture;

        protected virtual void Awake()
        {
            if (initialized == RuntimeInitialized.Awake)            
                RuntimeEventInitializationFactory.Initialization(this);           
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