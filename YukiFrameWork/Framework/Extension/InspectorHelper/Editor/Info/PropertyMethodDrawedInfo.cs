///=====================================================
/// - FileName:      PropertyMethodDrawedInfo.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/3 20:03:15
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;

namespace YukiFrameWork
{
    [Serializable]
    public sealed class PropertyMethodDrawedInfo : PropertyDrawedInfo
    {
        public MethodInfo MethodInfo { get; private set; }
        public MethodButtonAttribute Method { get; private set; }
        private SerializedObject serializedObject;
        public PropertyMethodDrawedInfo(Object target,SerializedProperty property,PropertyRangeAttribute propertyRange, LabelAttribute label, MethodInfo methodInfo
            , MethodButtonAttribute method, GUIColorAttribute color, EnableEnumValueIfAttribute[] e
            , DisableEnumValueIfAttribute[] d, EnableIfAttribute[] e2, DisableIfAttribute[] d2, HelperBoxAttribute helperBox,RuntimeDisabledGroupAttribute rd,EditorDisabledGroupAttribute ed)
            : base(target,property,null,null,null,null,propertyRange, label, color, e, d, e2, d2, helperBox,rd, ed)
        {
            MethodInfo = methodInfo;
            Method = method;
        }

        public PropertyMethodDrawedInfo(SerializedObject serializedObject,Object target,MethodInfo methodInfo
            , MethodButtonAttribute method, GUIColorAttribute color, EnableEnumValueIfAttribute[] e
            , DisableEnumValueIfAttribute[] d, EnableIfAttribute[] e2, DisableIfAttribute[] d2, HelperBoxAttribute helperBox, RuntimeDisabledGroupAttribute rd, EditorDisabledGroupAttribute ed)
        {
            MethodInfo = methodInfo;
            Method = method;
            this.GUIColor = color;
            this.EnableEnumValueIf = e;
            this.DisableEnumValueIf = d;
            this.EnableIf = e2;
            this.DisableIf = d2;
            this.HelperBox = helperBox;
            this.RuntimeDisabledGroup = rd;
            this.EditorDisabledGroup = ed;
            this.target = target;
            this.serializedObject = serializedObject;
            Content = new GUIContent(string.IsNullOrEmpty(method.Label) ? methodInfo.Name : method.Label);
        }

        private bool OnDisableGroup(PropertyMethodDrawedInfo methodInfo)
        {
            bool IsPlaying = Application.isPlaying;
            if (methodInfo.RuntimeDisabledGroup != null && methodInfo.EditorDisabledGroup != null)
                return true;

            if (methodInfo.RuntimeDisabledGroup != null)
                return IsPlaying;
            else if (methodInfo.EditorDisabledGroup != null)
                return !IsPlaying;

            return false;
        }


        public override void OnGUI()
        {
            serializedObject.Update();
            DrawHelperBox();
            EditorGUI.BeginDisabledGroup(OnDisableGroup(this));
            if (GUIColor != null)
                GUI.color = GUIColor.Color;
            bool executed = Method.Width == -1
            ? GUILayout.Button(string.IsNullOrEmpty(Method.Label) ? MethodInfo.Name : Method.Label, GUILayout.Height(Method.Height))
            : GUILayout.Button(string.IsNullOrEmpty(Method.Label) ? MethodInfo.Name : Method.Label, GUILayout.Height(Method.Height), GUILayout.Width(Method.Width));
            GUI.color = Color.white;
            EditorGUI.EndDisabledGroup();
            if (executed)
            {
                object[] args = null;
                if (MethodInfo.GetParameters().Length != Method.Args?.Length)
                    args = new object[MethodInfo.GetParameters().Length];
                else args = Method.Args;
                MethodInfo.Invoke(target, args);
                AssetDatabase.Refresh();
                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();

                serializedObject.ApplyModifiedProperties();
            }
           
        }
    }
}
#endif