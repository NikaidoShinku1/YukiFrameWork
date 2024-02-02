#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
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
                string directionPath = "Assets/Resources";
                if (!Directory.Exists(directionPath))
                {
                    Directory.CreateDirectory(directionPath);
                    AssetDatabase.Refresh();
                }
                AssetDatabase.CreateAsset(config, directionPath + "/frameworkConfig.asset");              
                config.NameSpace = "YukiFrameWork.Project";
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

        public virtual void GenericScripts() { }

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
