///=====================================================
/// - FileName:      MethodInfoLayer.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/1 23:12:25
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using System;
using YukiFrameWork.Extension;
using Object = UnityEngine.Object;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

namespace YukiFrameWork
{
	public class MethodInfoLayer : GenericLayer
	{
        class Method
        {
            public string name;
            public PropertyMethodDrawedInfo drawedInfo;

            public Method(string name,PropertyMethodDrawedInfo drawedInfo)
            {
                this.name = name;
                this.drawedInfo = drawedInfo;
            }
        }
		private List<Method> methodInfoDicts = new List<Method>();

        private Object target;
        public MethodInfoLayer(SerializedObject serializedObject, Type type)
        {
            InitMethodInfos(serializedObject, type);
        }

        IEnumerable<MethodInfo> methodInfos = null;

        private void InitMethodInfos(SerializedObject serializedObject, Type type)
        {
            this.target = serializedObject.targetObject;
            methodInfos ??= type.GetRuntimeMethods();

            foreach (MethodInfo methodInfo in methodInfos) 
            {
                MethodButtonAttribute method = methodInfo.GetCustomAttribute<MethodButtonAttribute>();

                if (method == null) continue;

                methodInfo.CreateAllSettingAttribute
                (out LabelAttribute label
                , out GUIColorAttribute color, out EnableEnumValueIfAttribute[] enableEnumValueIfAttribute
                , out DisableEnumValueIfAttribute[] disable, out EnableIfAttribute[] enableIf, out DisableIfAttribute[] disableIf
                , out HelperBoxAttribute helperBox, out _
                , out _, out _
                , out _, out _
                , out RuntimeDisabledGroupAttribute runtimeDisabledGroup, out EditorDisabledGroupAttribute editorDisabledGroup
                ,out _,out _,out _,out _,out _);    
                methodInfoDicts.Add(new Method( methodInfo.Name,new PropertyMethodDrawedInfo(serializedObject,target
                    ,methodInfo, method,color,enableEnumValueIfAttribute,disable
                    ,enableIf,disableIf,helperBox,runtimeDisabledGroup,editorDisabledGroup))); 
            }         
        }     
        public override void OnInspectorGUI()
        {
            foreach (var method in methodInfoDicts)
            {
                PropertyMethodDrawedInfo info = method.drawedInfo;      

                if (!info.DrawConditionIf(target.GetType(), target))
                    continue;
                EditorGUILayout.BeginVertical();

                info.OnGUI();
                
                EditorGUILayout.Space();              
                EditorGUILayout.EndVertical();
            }         
        }
    }
}
#endif