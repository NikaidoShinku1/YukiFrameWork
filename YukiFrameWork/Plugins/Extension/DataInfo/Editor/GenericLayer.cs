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
        public GenericLayer(GenericDataBase data, Type targetType)
        {
            
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

        protected bool OnDisableGroup(PropertyDrawedInfo info,bool currentCheckValue = false)
        {
            if (info is PropertyReorderableListDrawedInfo || currentCheckValue)
            {
                return false;
            }
            if (info.RuntimeDisabledGroup != null && info.EditorDisabledGroup != null)
                return true;

            if (info.RuntimeDisabledGroup != null)
                return IsPlaying;
            else if (info.EditorDisabledGroup != null)
                return !IsPlaying;

            return false;
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
