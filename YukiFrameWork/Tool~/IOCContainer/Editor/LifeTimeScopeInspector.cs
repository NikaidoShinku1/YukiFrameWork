///=====================================================
/// - FileName:      LifeTimeScopeEditor.cs
/// - NameSpace:     YukiFrameWork
/// - Created:       Yuki
/// - Email:         1274672030@qq.com
/// - Description:   生命容器拓展
/// - Creation Time: 2023/12/30 2:58:55
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
using UnityEditor;

#if UNITY_EDITOR
namespace YukiFrameWork.IOC
{
    [CustomEditor(typeof(LifeTimeScope),true)]
    [CanEditMultipleObjects]
    public class LifeTimeScopeInspector : CustomInspectorEditor
    { 
        public override void OnInspectorGUI()
        {            
            SerializedInjectedGameObjects();
            EditorGUILayout.Space();

            base.OnInspectorGUI();
        }      

        private void SerializedInjectedGameObjects()
        {
            LifeTimeScope scope = target as LifeTimeScope;
            if (scope == null) return;
            serializedObject.Update();

            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginVertical("OL box NoExpand");

            GUILayout.BeginHorizontal();
            GUI.color = Color.yellow;
            GUILayout.Label("编辑器可视化注册器(可拖入脚本必须文件以及类名一致，\n注意:类型不能派生自UnityEngine.Object");
            GUI.color = Color.white;
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add"))
            {
                scope.AddInfo(new InjectInfo());
            }

            if (GUILayout.Button("Delete"))
            {
                if(scope.selectIndex != -1)
                    scope.RemoveInfo(scope.GetInfos()[scope.selectIndex]);
            }
            GUILayout.EndHorizontal();        

            for(int i = 0;i < scope.GetInfos().Count;i++)
            {
                var info = scope.GetInfos()[i];
                var rect = EditorGUILayout.BeginVertical(scope.selectIndex == i ? "SelectionRect" : GUIStyle.none);
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("注册器");

                EditorGUILayout.BeginHorizontal();
                info.monoScript = (MonoScript)EditorGUILayout.ObjectField(info.monoScript, typeof(MonoScript), true);
                if (info.monoScript != null)
                {
                    try
                    {
                        info.typeName = info.monoScript.GetClass().FullName;
                        GUILayout.FlexibleSpace();
                        GUI.color = Color.cyan;
                        EditorGUILayout.LabelField(info.typeName);
                        GUI.color = Color.white;
                    }
                    catch { }
                }
                EditorGUILayout.EndHorizontal();
                info.lifeTime = (LifeTime)EditorGUILayout.EnumPopup(info.lifeTime);

                switch (Event.current.type)
                {
                    case EventType.MouseDown:
                        if (rect.Contains(Event.current.mousePosition))
                        {
                            scope.selectIndex = i;
                        }
                        break;
                }

                EditorGUILayout.EndVertical();

            }

            EditorGUILayout.EndVertical();
            EditorGUI.EndDisabledGroup();
            if (EditorGUI.EndChangeCheck())
            {
                scope.SaveData();
                serializedObject.ApplyModifiedProperties();
            } 

           
        }
    }
}
#endif