using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
namespace YukiFrameWork.Extension
{
    public class BindLayer : GenericLayer
    {
        private ISerializedFieldInfo info;
        public event Action GenericCallBack;
        public BindLayer(GenericDataBase data, Type targetType) : base(data, targetType)
        {
            
        }

        public BindLayer(ISerializedFieldInfo info) 
        {
            this.info = info;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();                   

            EditorGUI.BeginDisabledGroup(IsPlaying);
            EditorGUILayout.Space(20);
            GUILayout.Label(GenericScriptDataInfo.BindExtensionInfo, "PreviewPackageInUse");


            var rect = EditorGUILayout.BeginVertical("FrameBox", GUILayout.Height(100 + info.GetSerializeFields().Count() * 20));
            GUILayout.Label(GenericScriptDataInfo.DragObjectInfo);

            foreach (var data in info.GetSerializeFields())
            {
                EditorGUILayout.BeginHorizontal();
                data.fieldName = EditorGUILayout.TextField(data.fieldName);
               
                data.fieldLevelIndex = EditorGUILayout.Popup(data.fieldLevelIndex, data.fieldLevel);

                data.fieldTypeIndex = EditorGUILayout.Popup(data.fieldTypeIndex, data.Components?.ToArray());

                data.target = EditorGUILayout.ObjectField(data.target, typeof(Object), true);

                if (GUILayout.Button("", "ToggleMixed"))
                {
                    info.RemoveFieldData(data);
                    break;
                }

                EditorGUILayout.EndHorizontal();
            }

            DragObject(rect);

            GUILayout.FlexibleSpace();
            if (info.GetSerializeFields().Count() > 0 && GUILayout.Button("Éú³É´úÂë", GUILayout.Height(25)))
            {               
                GenericCallBack?.Invoke();
            }

            EditorGUILayout.EndVertical();
            EditorGUI.EndDisabledGroup();
        }

        private void DragObject(Rect rect)
        {
            Event e = Event.current;

            if (rect.Contains(e.mousePosition))
            {                             
                DragAndDrop.visualMode = DragAndDropVisualMode.Generic;

                if (e.type == EventType.DragPerform)
                {
                    var assets = DragAndDrop.objectReferences;

                    foreach (var asset in assets)
                    {
                        info.AddFieldData(new SerializeFieldData(asset));
                    }
                    e.Use();
                }
            }
        }
    }
}
