///=====================================================
/// - FileName:      StateInspector.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/13 14:55:41
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
namespace YukiFrameWork.Machine
{
    [CustomEditor(typeof(StateInspectorHelper))]
    public class StateInspector : Editor
    {
        internal static Rect popupRect;

        private GUIContent btn_add_state_script = new GUIContent("添加 状态脚本");

        private GUIStyle ProjectBrowserHeaderBgMiddle = null;

        private GUIStyle DD_HeaderStyle = null;

        private GUIStyle PrefixLabel = null;

        private GUIContent script_gui_content = new GUIContent();
        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();

            if (ProjectBrowserHeaderBgMiddle == null)
                ProjectBrowserHeaderBgMiddle = new GUIStyle("AC BoldHeader");

            if (DD_HeaderStyle == null)
            {
                DD_HeaderStyle = new GUIStyle("IconButton");
            }

            if (PrefixLabel == null)
            {
                PrefixLabel = new GUIStyle("PrefixLabel");
                PrefixLabel.richText = true;
            }

            StateInspectorHelper helper = target as StateInspectorHelper;
            if (helper == null) return;

            bool disabled = EditorApplication.isPlaying || helper.node.IsAnyState || helper.node.IsEntryState || helper.node.IsUpState;

            EditorGUI.BeginDisabledGroup(disabled);
            
            Vector2 mousePosition = Event.current.mousePosition;

            foreach (var item in helper.node.behaviourInfos)
            {
                // 刷新一下
                if (string.IsNullOrEmpty(item.guid) && !string.IsNullOrEmpty(item.typeName))
                    helper.node.RefreshStateScripts(helper.runtimeStateMachineCore);

                // 根据guid加载到脚本信息
                string path = AssetDatabase.GUIDToAssetPath(item.guid);
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script == null) continue;
                Type type = script.GetClass();
                if (type == null) continue;

                var r = EditorGUILayout.BeginHorizontal(GUILayout.Height(25));
                r.x = 0;
                r.width += 30;
                GUI.Box(r, string.Empty, ProjectBrowserHeaderBgMiddle);
                GUILayout.Space(-10);
                GUILayout.Label(EditorGUIUtility.IconContent("d_cs Script Icon"), GUILayout.Width(20), GUILayout.Height(20));

                string displayName = string.Empty;

                if (type.IsSubclassOf(typeof(StateBehaviour)))
                {
                    displayName = type.Name;
                    script_gui_content.tooltip = string.Empty;
                }
                else
                {
                    displayName = string.Format("{0}<color=yellow>(脚本丢失)</color>", type.Name);
                    script_gui_content.tooltip = "脚本丢失,请检查该脚本是否继承自StateBehaviour!";
                }

                script_gui_content.text = displayName;


                GUILayout.Label(script_gui_content, PrefixLabel, GUILayout.Height(20));

                GUILayout.FlexibleSpace();

                GUILayout.BeginVertical();

                GUILayout.Space(5);

                if (GUILayout.Button(EditorGUIUtility.IconContent("d__Menu"), DD_HeaderStyle, GUILayout.Width(25), GUILayout.Height(25)))
                {
                    ShowMenu(script);
                }

                GUILayout.EndVertical();

                GUILayout.EndHorizontal();

                if (Event.current.type == EventType.MouseUp && Event.current.button == 1 && r.Contains(mousePosition))
                {
                    ShowMenu(script);
                    Event.current.Use();
                }

                if (Event.current.type == EventType.MouseUp && Event.current.button == 0 && r.Contains(mousePosition))
                {

                    EditorGUIUtility.PingObject(script);
                    Event.current.Use();
                }
            }


            GUILayout.Space(30);

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            Rect rect = GUILayoutUtility.GetRect(btn_add_state_script, GUI.skin.button, GUILayout.Width(260), GUILayout.Height(25));

            if (GUI.Button(rect, btn_add_state_script))
            {
                popupRect = new Rect(rect);
                popupRect.height = 300;

                PopupWindow.Show(rect, new SelectStateWindow(popupRect, helper.runtimeStateMachineCore, helper.node));
            }

            GUILayout.FlexibleSpace();



            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (helper.runtimeStateMachineCore != null)
                helper.runtimeStateMachineCore.Save();          

            EditorGUI.EndDisabledGroup();
        }

        protected override void OnHeaderGUI()
        {
            //base.OnHeaderGUI();
           StateInspectorHelper helper = target as StateInspectorHelper;
            if (helper == null) return;
            string name = null;
            EditorGUI.BeginChangeCheck();
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();

                GUILayout.Label(EditorGUIUtility.IconContent("icons/processed/unityeditor/animations/animatorstate icon.asset"), GUILayout.Width(30), GUILayout.Height(30));

                EditorGUILayout.LabelField("Name:", GUILayout.Width(60));

                bool disabled = EditorApplication.isPlaying
                    || helper.node.IsAnyState || helper.node.IsEntryState || helper.node.IsUpState;

                EditorGUI.BeginDisabledGroup(disabled);
                name = EditorGUILayout.DelayedTextField(helper.node.DisPlayName);
                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndHorizontal();
            }
            if (EditorGUI.EndChangeCheck())
            {
                string oldName = helper.node.name;
                bool success = StateNodeFactory.Rename(helper.runtimeStateMachineCore, helper.node, name);
                if (success)
                    helper.grap.RenameState(oldName, name);
            }
            EditorGUILayout.Space();
            var rect = EditorGUILayout.BeginHorizontal();

            Handles.color = Color.black;
            Handles.DrawLine(new Vector2(rect.x, rect.y), new Vector3(rect.x + rect.width, rect.y));

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
        }



        private void ShowMenu(MonoScript script)
        {
            StateInspectorHelper helper = target as StateInspectorHelper;
            if (helper == null) return;

            var genricMenu = new GenericMenu();

            genricMenu.AddItem(new GUIContent("移除 脚本"), false, () =>
            {
                helper.node.RemoveStateScript(script);
                helper.runtimeStateMachineCore.Save();
            });

            genricMenu.AddItem(new GUIContent("打开 脚本"), false, () =>
            {
                AssetDatabase.OpenAsset(script);
            });

            genricMenu.ShowAsContext();
        }


    }
}
#endif