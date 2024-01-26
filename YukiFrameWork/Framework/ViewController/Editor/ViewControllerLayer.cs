
#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using YukiFrameWork.Extension;
using System.Reflection;
namespace YukiFrameWork
{
    [Serializable]
    public class ViewControllerLayer : GenericLayer
    {            
        private CustomData Data;
        private Type targetType;
        public ViewControllerLayer(GenericDataBase data, Type targetType) : base(data, targetType)
        {
            this.Data = data as CustomData;
            this.targetType = targetType;
        }
        public override void OnInspectorGUI()
        {                       
            GUIStyle style = new GUIStyle("AM HeaderStyle")
            {
                alignment = TextAnchor.MiddleLeft,
                fontSize = 16,
            };
            style.normal.textColor = Color.white;
            style.fontStyle = FontStyle.Bold;
            GUILayout.BeginHorizontal();
            GUILayout.Label(GenericScriptDataInfo.TitleTip, style);
            EditorGUILayout.BeginHorizontal(GUILayout.Width(100));
            GUILayout.Label("EN");
            GenericScriptDataInfo.IsEN = EditorGUILayout.Toggle(GenericScriptDataInfo.IsEN);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();          
            
            EditorGUI.BeginDisabledGroup(IsPlaying);
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(GenericScriptDataInfo.Email, GUILayout.Width(200));
            Data.CreateEmail = EditorGUILayout.TextField(Data.CreateEmail);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            Data.SystemNowTime = DateTime.Now.ToString();

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(GenericScriptDataInfo.NameSpace, GUILayout.Width(200));
            Data.ScriptNamespace = EditorGUILayout.TextField(Data.ScriptNamespace);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(GenericScriptDataInfo.Name, GUILayout.Width(200));
            Data.ScriptName = EditorGUILayout.TextField(Data.ScriptName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            GUILayout.Label(GenericScriptDataInfo.Path, GUILayout.Width(200));
            GUILayout.TextField(Data.ScriptPath);
            SelectFolder(Data);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(!targetType.Equals(typeof(ViewController)));
            GUILayout.Label(GenericScriptDataInfo.AutoInfo, GUILayout.Width(GenericScriptDataInfo.IsEN ? 300 : 200));
            Data.IsAutoMation = EditorGUILayout.Toggle(Data.IsAutoMation, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();
            if (Data.IsAutoMation)
            {
                EditorGUILayout.Space(10);
                SelectArchitecture(Data);
            }
            EditorGUILayout.Space(10);
             EditorGUI.EndDisabledGroup();     
            if(Data.IsAutoMation)
            {
                Data.IsCustomAssembly = EditorGUILayout.ToggleLeft(GenericScriptDataInfo.AssemblyInfo, Data.IsCustomAssembly);
                if(Data.IsCustomAssembly)
                {       
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(GenericScriptDataInfo.IsEN ? "Input Assembly Name:" : "输入程序集名称:");
                    Data.CustomAssemblyName = EditorGUILayout.TextField(Data.CustomAssemblyName);
                    EditorGUILayout.EndHorizontal();
                }           
                else Data.CustomAssemblyName = "Assembly-CSharp";
            }

            EditorGUI.EndDisabledGroup();      
        }

        private void SelectArchitecture(CustomData Data)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Width(300));
            EditorGUILayout.LabelField(GenericScriptDataInfo.IsEN ? "Select Architecture:" : "架构选择:",GUILayout.Width(120));

            var list = Data.AutoInfos;
            list.Clear();
            list.Add("None");
            try
            {
                var types = AssemblyHelper.GetTypes(Assembly.Load(Data.CustomAssemblyName));
                if(types != null)
                {
                    foreach (var type in types)            
                        if (type.BaseType != null)              
                            foreach (var baseInterface in type.BaseType.GetInterfaces())                                          
                                if (baseInterface.ToString().Equals(typeof(IArchitecture).ToString()))                                                   
                                    list.Add(type.Name);                                                                                     
                }
            }
            catch{ }
            Data.AutoArchitectureIndex = EditorGUILayout.Popup(Data.AutoArchitectureIndex, list.ToArray());          
            EditorGUILayout.EndHorizontal();          
        }     
    }
}
#endif