#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
namespace YukiFrameWork.Extension
{
    public abstract class GenericLayer 
    {
        public bool IsPlaying => Application.isPlaying;
        public FrameworkEasyConfig Config { get; }
        public GenericLayer(GenericDataBase data, Type targetType)
        {
            FrameworkEasyConfig config = Resources.Load<FrameworkEasyConfig>("frameworkConfig");

            if (config == null)
            {
                config = ScriptableObject.CreateInstance<FrameworkEasyConfig>();

                AssetDatabase.CreateAsset(config, "Assets/Resources/frameworkConfig.asset");
            }

            Config = config;
        }

        public GenericLayer()
        {
           
        }
        public virtual void OnGUI(Rect rect)
        {
            
        }

        public virtual void OnInspectorGUI()
        {
            
        }

        public void GenericScripts(GenericDataBase Data,string configName,string description = "")
        {
            string scriptFilePath = Data.ScriptPath + @"/" + Data.ScriptName + ".cs";

            if (!File.Exists(scriptFilePath))
            {
                if (GUILayout.Button(GenericScriptDataInfo.GenerateScriptBtn, GUILayout.Height(30)))
                {
                    if (string.IsNullOrEmpty(Data.ScriptPath))
                    {
                         Debug.LogError((GenericScriptDataInfo.IsEN ? "Cannot create script because path is empty!" : "路径为空无法创建脚本!"));
                        return;
                    }

                    if (!Directory.Exists(Data.ScriptPath))
                    {
                        Directory.CreateDirectory(Data.ScriptPath);
                        AssetDatabase.Refresh();
                    }

                    if (File.Exists(scriptFilePath))
                    {
                        Debug.LogError((GenericScriptDataInfo.IsEN ? $"Scripts already exist in this folder! Path:{scriptFilePath}" : $"脚本已经存在该文件夹! Path:{scriptFilePath}"));
                        return;
                    }

                    using (FileStream fileStream = new FileStream(scriptFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        StreamWriter streamWriter = new StreamWriter(fileStream);

                        TextAsset textAsset = Resources.Load<TextAsset>(configName);

                        string GenericScriptDataInfo = textAsset.text;
                        GenericScriptDataInfo = GenericKit.ScriptsInfoChange(GenericScriptDataInfo, Data,description);

                        streamWriter.Write(GenericScriptDataInfo);

                        streamWriter.Close();

                        fileStream.Close();
                        //正在改变脚本
                        Data.OnLoading = true;

                    }                   

                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();

                }
            }
            else
            {
                GUILayout.BeginVertical();
                MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptFilePath);
                if (GUILayout.Button(GenericScriptDataInfo.SelectScriptBtn, GUILayout.Height(30)))
                {
                    Selection.activeObject = monoScript;
                }
                string partialPath = Data.ScriptPath + @"/" + Data.ScriptName + ".Example" + ".cs";
                MonoScript partial = AssetDatabase.LoadAssetAtPath<MonoScript>(partialPath);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(GenericScriptDataInfo.OpenScriptBtn, GUILayout.Height(30)))
                {
                    AssetDatabase.OpenAsset(monoScript);
                }

                if(partial != null)
                {
                    if (GUILayout.Button(GenericScriptDataInfo.OpenPartialScriptBtn, GUILayout.Height(30)))
                    {
                        AssetDatabase.OpenAsset(partial);
                    }
                }

                EditorGUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }

        }             

        public void SelectFolder<T>(T Data) where T : GenericDataBase
        {
            if (GUILayout.Button("...", GUILayout.Width(40)))
            {
                Data.ScriptPath = string.Empty;
                string path = EditorUtility.OpenFolderPanel("", Data.ScriptPath, "");

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
                            Data.ScriptPath += values[i] + "/";
                        else
                            Data.ScriptPath += values[i];
                    }

                }

                GUIUtility.ExitGUI();
            }
        }
    }
}
#endif
