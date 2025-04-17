///=====================================================
/// - FileName:      DeepSeekSettingWindow.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/4/15 17:47:33
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using YukiFrameWork.Extension;
using Sirenix.OdinInspector;

using System.Security.Cryptography;



#if UNITY_EDITOR
using UnityEditor;
namespace YukiFrameWork
{
    public class DeepSeekSettingWindow 
    {
        
        private Texture2D mLogo;
        private GUIStyle labelStyle;
        [OnInspectorGUI]
        private void OnGUI()
        {
            ReLoad();
            var settings = ScriptGeneratorSettings.instance;
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("DeepSeek代码生成设置", labelStyle);
            GUILayout.FlexibleSpace();
            GUILayout.Label("Version:" + settings.version);
            EditorGUILayout.EndHorizontal();
            GUILayout.Label(mLogo, GUILayout.Width(300), GUILayout.Height(75));

           
            using var rsa = new RSACryptoServiceProvider();

            // Get the user's Unity Cloud account email
            var userEmail = CloudProjectSettings.userName;
            if (string.IsNullOrEmpty(userEmail))
            {
                EditorGUILayout.HelpBox("你必须登录Unity才可以使用该功能!", MessageType.Error);
                return;
            }
         
            var url = settings.Url;
            var key = string.IsNullOrEmpty(settings.apiKey) ? "" : DeepSeekAPI.Encryption.Decrypt(settings.apiKey, userEmail);
            var model = settings.model;
            var useTimeout = settings.useTimeout;
            var timeout = settings.timeout;
            var path = settings.path;

            // Display instructions for obtaining an API key   
            EditorGUILayout.HelpBox("从DeepSeek API开放网站获取API密钥:\n" + //
                                "    1. 使用网站下面的按钮登录/注册你的DeepSeek账户.\n" + // 
                                "    2. 导航到你的账户仪表盘中的\"查看API密钥部分\".\n" + //
                                "    3. 点击\"创建 API Key\"创建你的密钥并复制到此.", MessageType.Info);

            // Button to open DeepSeek's website
            if (GUILayout.Button("DeepSeek API开放平台", GUILayout.ExpandHeight(false)))
            {
                Application.OpenURL("https://platform.deepseek.com/");
            }

            EditorGUILayout.Space(20);


            EditorGUI.BeginChangeCheck();

            url = EditorGUILayout.TextField("URL", url);
            key = EditorGUILayout.PasswordField("API Key", key);

            // Warning about storing the API key in a local file
            EditorGUILayout.HelpBox("将API秘钥储存在文件: " + //
                                    "UserSettings/DeepSeekScriptGeneratorSettings.asset. \n" + //
                                    "当与他人共享目录时，请确保排除API秘钥的\'UserSetting\'目录以确保防止未经使用授权而使用你的秘钥.", MessageType.Warning);

            EditorGUILayout.Space(20);


            EditorGUILayout.HelpBox("将模型名称复制到这里 如deepseek-chat", MessageType.Info);
            EditorGUILayout.BeginHorizontal();
            model = EditorGUILayout.TextField("模型", model);
            if (GUILayout.Button("DeepSeek 模型导航"))
            {
                Application.OpenURL("https://api-docs.deepseek.com/zh-cn/quick_start/pricing");
            }

            EditorGUILayout.EndHorizontal();


            var rect = EditorGUILayout.BeginHorizontal();
            path = EditorGUILayout.TextField("输出的脚本路径", path);
            bool dragop = false;
            if (DragObject(rect, out string temp))
            {
                path = temp;
                dragop = true;
            }
            
            if (!path.EndsWith("/")) path += "/";
            if (!path.StartsWith("Assets/")) path = "Assets/" + path;
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(20);

          
            useTimeout = EditorGUILayout.Toggle("超时设置", useTimeout);

            if (useTimeout)
            {
                EditorGUILayout.HelpBox("开启后超过指定时间意为访问超时", MessageType.Info);
                EditorGUI.indentLevel++;
                timeout = EditorGUILayout.IntField("持续时间 (秒)", timeout);
                if (timeout < 1) timeout = 1;
                EditorGUI.indentLevel--;
            }
            
            if (EditorGUI.EndChangeCheck() || dragop)
            {
                settings.apiKey = DeepSeekAPI.Encryption.Encrypt(key, userEmail);
                settings.model = model;
                settings.useTimeout = useTimeout;
                settings.timeout = timeout;
                settings.path = path;
                settings.Url = url;
                settings.Save();
            }
        }
        public static bool DragObject(Rect rect, out string path)
        {
            Event e = Event.current;
            path = string.Empty;
            if (rect.Contains(e.mousePosition))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;

                if (e.type == EventType.DragPerform)
                {
                    var assets = DragAndDrop.objectReferences;

                    if (assets[0].GetType().Equals(typeof(DefaultAsset)))
                    {
                        path = AssetDatabase.GetAssetPath(assets[0]);
                    }
                    e.Use();
                }
            }

            return !string.IsNullOrEmpty(path);
        }

        public static string SelectFolder(string targetPath)
        {
            string path = EditorUtility.OpenFolderPanel("", targetPath, "");
            string target = string.Empty;
            bool append = false;

            string[] values = path.Split('/');

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i].Contains("Assets") || values[i] == "Assets")
                {
                    append = true;
                }
                if (append)
                {
                    if (i < values.Length - 1)
                        target += values[i] + "/";
                    else
                        target += values[i];
                }

            }

            return target;
        }
        private void ReLoad()
        {
            if (!mLogo)
            {
                mLogo = AssetDatabase.LoadAssetAtPath<Texture2D>(ImportSettingWindow.packagePath + "/Plugins/DeepSeek/Editor/Logo.png");
            }
            if (labelStyle == null)
            {
                labelStyle = new GUIStyle();
                labelStyle.alignment = TextAnchor.UpperLeft;
                labelStyle.normal.textColor = Color.white;
                labelStyle.fontSize = 24;
            }
        }
    }
}
#endif