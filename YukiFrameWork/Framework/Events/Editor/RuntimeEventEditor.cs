#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YukiFrameWork.Extension;

namespace YukiFrameWork.Events
{
    [CustomEditor(typeof(RuntimeEventCenter))]
    public class RuntimeEventEditor : Editor
    {     
        private SerializedProperty centerProperty;
        private int selectIndex = 0;
        public override void OnInspectorGUI()
        {
            RuntimeEventCenter eventCenter = target as RuntimeEventCenter;

            if (eventCenter == null) return;

            DrawEventGUI(eventCenter);
        }

        private void OnEnable()
        {         
           centerProperty = serializedObject.FindProperty("centers");
            RuntimeEventCenter eventCenter = target as RuntimeEventCenter;
            foreach (var index in eventCenter.GetEventCenterIndex())
                SetEnumType(eventCenter, index);
        }

        private void DrawEventGUI(RuntimeEventCenter eventCenter)
        {
#if UNITY_2021_1_OR_NEWER
            Texture2D icon = EditorGUIUtility.IconContent("AssemblyLock").image as Texture2D;
            EditorGUIUtility.SetIconForObject(target, icon);
#endif
            EditorGUILayout.BeginVertical("OL box NoExpand");
            GUIStyle style = new GUIStyle("AM HeaderStyle")
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 14,
            };
            style.normal.textColor = Color.white;
            style.fontStyle = FontStyle.Bold;

          
            GUILayout.Label(EventEditorInfo.Tip,style);
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal(GUILayout.Width(100));          
            GUILayout.Label("EN");
            EventEditorInfo.IsEN = EditorGUILayout.Toggle(EventEditorInfo.IsEN);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(EventEditorInfo.RegisterTypeInfo,GUILayout.Width(100));
            eventCenter.registerType = (RegisterType)EditorGUILayout.EnumPopup(eventCenter.registerType,GUILayout.Width(120));          
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+",GUILayout.Width(30)))
            {
                Undo.RecordObject(eventCenter,"Add Event");
                eventCenter.AddEventCenter(new EventCenter());
                eventCenter.SaveData();
            }
            if (GUILayout.Button("-",GUILayout.Width(30)))
            {
                Undo.RecordObject(eventCenter, "Delete Event");
                eventCenter.RemoveEventCenter(selectIndex == -1 ? default : selectIndex);
                eventCenter.SaveData();
            }                     

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            GUIStyle tipBox = new GUIStyle(GUI.skin.box)
            {
                fontStyle = FontStyle.Bold,
            };

            tipBox.alignment = TextAnchor.MiddleLeft;
            tipBox.normal.textColor = Color.white;
            switch (eventCenter.registerType)
            {              
                case RegisterType.String:
                    GUILayout.Label(EventEditorInfo.StringDescriptionInfo, tipBox);
                    break;
                case RegisterType.Enum:
                    GUILayout.Label(EventEditorInfo.EnumDescriptionInfo, tipBox);                  
                    break;
            }
            EditorGUILayout.Space();
                                 
            serializedObject.Update();

            foreach (var index in eventCenter.GetEventCenterIndex())
            {
                var box = GUI.skin.box;
                if (index == selectIndex)
                    box = new GUIStyle("SelectionRect");
                EditorGUILayout.Space();
                var rect = EditorGUILayout.BeginHorizontal(box);               
                EditorGUILayout.PropertyField(centerProperty.GetArrayElementAtIndex(index), new GUIContent((EventEditorInfo.IsEN ? "Logger":"×¢²áÆ÷ ") + index), true);                
                Update_EventBox(rect, index);
               
                EditorGUILayout.EndHorizontal();
                var center = eventCenter.GetEventCenter(index);  
                Type enumType = center.EnumType; 
                EditorGUILayout.BeginHorizontal(GUILayout.Width(enumType == null ? 200 : 300));                
                if (eventCenter.registerType == RegisterType.Enum)
                {                                      
                    SetEnumInCenter(enumType,center);
                    if (GUILayout.Button(EventEditorInfo.Update_EnumBtnInfo))
                    {
                        if (SetEnumType(eventCenter, index))
                        {
                            $"{EventEditorInfo.Update_SuccessInfo}{center.name}".LogInfo();
                            center.enumIndex = 0;
                        }
                        else
                        {
                            $"{EventEditorInfo.Update_ErrorInfo}{center.name}".LogInfo(Log.E);
                        }

                    }
                }
                EditorGUILayout.EndHorizontal();
              
            }
            serializedObject.ApplyModifiedProperties();

        }

        private void Update_EventBox(Rect rect,int index)
        {
            Event e = Event.current;

            if (rect.Contains(e.mousePosition) && e.type == EventType.MouseDown)
            {                
                e.Use();
                selectIndex = index;
            }
        }

        private bool SetEnumType(RuntimeEventCenter eventCenter, int index)
        {
            var center = eventCenter.GetEventCenter(index);

            center.EnumType = AssemblyHelper.GetType(center.name);

            return center.EnumType != null;
        }

        private void SetEnumInCenter(Type enumType,EventCenter center)
        {          
            if (enumType != null)
            {
                Array enums = Enum.GetValues(enumType);

                if (enums != null && enums.Length > 0)
                {
                    List<string> enumInfos = center.mEnumInfos;
                    enumInfos.Clear();
                    foreach (var e in enums)
                    {
                        enumInfos.Add(e.ToString());
                    }
                    center.enumIndex = EditorGUILayout.Popup(center.enumIndex, enumInfos.ToArray());                           
                }
                
            }           
        }
        
    }
}
#endif