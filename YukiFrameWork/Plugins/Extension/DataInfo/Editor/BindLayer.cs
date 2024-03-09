
#if UNITY_EDITOR
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
        public event Action EventCenterCallBack;
        private Component infoAsset => info as Component;
        public BindLayer(GenericDataBase data, Type targetType) : base(data, targetType)
        {
            
        }
        private Type targetType;

        public BindLayer(ISerializedFieldInfo info,Type type) 
        {
            this.info = info;     
            this.targetType = type;
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
                string fieldName = EditorGUILayout.TextField(data.fieldName);

                if (data.fieldName != fieldName)
                {
                    Undo.RecordObject(infoAsset, "Change Data");
                    data.fieldName = fieldName;

                    SaveData();                   
                }
               
                int levelIndex = EditorGUILayout.Popup(data.fieldLevelIndex, data.fieldLevel);

                if (data.fieldLevelIndex != levelIndex)
                {
                    Undo.RecordObject(infoAsset, "Change Level");
                    data.fieldLevelIndex = levelIndex;
                    SaveData();
                }

                int typeIndex = EditorGUILayout.Popup(data.fieldTypeIndex, data.Components?.ToArray());

                if (typeIndex != data.fieldTypeIndex)
                {
                    Undo.RecordObject(infoAsset, "Change TypeIndex");
                    data.fieldTypeIndex = typeIndex;
                    SaveData();
                }

                var obj = EditorGUILayout.ObjectField(data.target, typeof(Object), true);

                if (data.target != obj)
                {
                    Undo.RecordObject(infoAsset, "Change Object");
                    data.target = obj;
                    SaveData();
                }

                if (GUILayout.Button("", "ToggleMixed"))
                {
                    Undo.RecordObject(infoAsset, "Remove Data");
                    info.RemoveFieldData(data);

                    SaveData();
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
            if (!infoAsset.GetType().Equals(targetType))
            {
                EventCenterCallBack?.Invoke();
            }
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
                        Undo.RecordObject(infoAsset, "Add Data");
                        info.AddFieldData(new SerializeFieldData(asset));

                        SaveData();
                    }
                    e.Use();
                }
            }
        }

        private void SaveData()
        {
            if (PrefabUtility.IsPartOfPrefabInstance(infoAsset))
                PrefabUtility.RecordPrefabInstancePropertyModifications(infoAsset);

            EditorUtility.SetDirty(infoAsset);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif