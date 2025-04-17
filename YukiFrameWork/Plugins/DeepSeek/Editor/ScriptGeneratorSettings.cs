///=====================================================
/// - FileName:      ScriptGeneratorSettings.cs
/// - NameSpace:     YukiFrameWork.ItemDemo
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/4/15 17:22:53
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Security.Cryptography;

#if UNITY_EDITOR
using UnityEditor;
namespace YukiFrameWork
{
   
    [FilePath("UserSettings/DeepSeekScriptGeneratorSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class ScriptGeneratorSettings : ScriptableSingleton<ScriptGeneratorSettings>
    {
        public string version = "1.0";

        public string Url = "https://api.deepseek.com/v1/chat/completions";

        // API key for authentication with the DeepSeek API
        public string apiKey;

        // Model to be used for the API requests
        public string model = "deepseek-chat";

        // Default path where generated scripts will be stored
        public string path = "Assets/Scripts/";

        // Flag to determine if a timeout should be used for API requests
        public bool useTimeout = true;

        // Timeout duration in seconds for API requests
        public int timeout = 45;

        // Saves the settings when called
        public void Save() => Save (true);

        // Ensures settings are saved when the scriptable object is disabled
        void OnDisable() => Save();

    } 
}
#endif