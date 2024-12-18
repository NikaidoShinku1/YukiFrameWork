using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork.Extension
{
    [Serializable]
    public class GenericDataBase
    {
        [SerializeField]
        private string scriptName;
        [SerializeField]
        private string scriptPath = "Assets/Scripts";
        [SerializeField]
        private string scriptNamespace;
        [SerializeField]
        private string createName = "Yuki";
        [SerializeField]
        private string createEmail = "1274672030@qq.com";
        [SerializeField]
        private string systemNowTime;
        [SerializeField]
        private string description = "这是一个框架工具创建的脚本";
        [SerializeField]
        private bool onLoading = false;
        [SerializeField]
        private bool isFolderCreateScripts = true;

        public bool IsFolderCreateScripts
        {
            get => isFolderCreateScripts;
            set => isFolderCreateScripts = value;
        }

        public bool OnLoading
        {
            get => onLoading;
            set => onLoading = value;
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
    }
}
