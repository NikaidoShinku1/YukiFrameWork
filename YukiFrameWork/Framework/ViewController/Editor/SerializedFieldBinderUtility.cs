#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System;
using UnityEditor;
using UnityEngine;
using YukiFrameWork.Extension;

namespace YukiFrameWork
{
    public static class SerializedFieldBinderUtility
    {
        public static void BindAllFields(Component target, ISerializedFieldInfo serialized)
        {
            if (target == null || serialized == null || Application.isPlaying) return;

            var fieldInfos = target.GetType().GetRuntimeFields();
            foreach (var fieldInfo in fieldInfos)
            {
                var data = serialized.GetSerializeFields().FirstOrDefault(x => x.fieldName.Equals(fieldInfo.Name));
                if (data == null || data.target == null) continue;
                ApplyFieldValue(target, fieldInfo, data);
            }

            var binds = target.GetComponentsInChildren<YukiBind>(true);
            if (binds == null || binds.Length == 0) return;

            foreach (var fieldInfo in fieldInfos)
            {
                var bind = binds.FirstOrDefault(x => x._fields.fieldName.Equals(fieldInfo.Name));
                if (bind == null) continue;
                var data = bind._fields;
                if (data == null || data.target == null) continue;
                ApplyFieldValue(target, fieldInfo, data);
            }

            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }

        private static void ApplyFieldValue(Component target, FieldInfo fieldInfo, SerializeFieldData data)
        {
            if (!fieldInfo.FieldType.IsSubclassOf(typeof(Component)))
                fieldInfo.SetValue(target, data.target);
            else
                fieldInfo.SetValue(target, data.GetComponent(fieldInfo.FieldType));
        }
    }
}
#endif
