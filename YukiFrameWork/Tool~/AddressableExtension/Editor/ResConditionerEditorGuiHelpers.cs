using System.IO;
using UnityEditor;
using UnityEngine;

namespace YukiFramework.AddressableExtension.Editor
{
    internal static class ResConditionerEditorGuiHelpers
    {
        public const float GenerationLabelWidth = 120f;

        public static void DrawGenerationSettings(SerializedObject ruleSetSerialized)
        {
            if (ruleSetSerialized == null)
                return;

            var ruleSet = ruleSetSerialized.targetObject as ResourcesConditionerRuleSet;
            ruleSet?.MigrateGenerationPathsIfNeeded();

            var folderProp = ruleSetSerialized.FindProperty("generatedOutputFolder");
            var classNameProp = ruleSetSerialized.FindProperty("generatedFileClassName");
            var nsProp = ruleSetSerialized.FindProperty("generatedNamespace");

            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = GenerationLabelWidth;

            DrawProjectFolderField(folderProp, classNameProp,
                new GUIContent("输出文件夹", "仅填写 Assets 下文件夹路径，可拖拽 Project 文件夹到此"));
            DrawAlignedTextField(classNameProp,
                new GUIContent("类名", "输出 .cs 文件名（不含扩展名），与文件夹拼接为完整路径"));
            EditorGUILayout.PropertyField(nsProp, new GUIContent("生成命名空间"));

            var preview = ResourcesConditionerRuleSet.CombineOutputPath(
                folderProp.stringValue, classNameProp.stringValue);
            EditorGUILayout.LabelField("完整输出路径", preview, EditorStyles.wordWrappedMiniLabel);

            EditorGUIUtility.labelWidth = oldLabelWidth;
        }

        public static void DrawAlignedTextField(SerializedProperty property, GUIContent label)
        {
            if (property == null)
                return;

            var height = EditorGUIUtility.singleLineHeight;
            var rect = EditorGUILayout.GetControlRect(true, height);
            var labelRect = new Rect(rect.x, rect.y, EditorGUIUtility.labelWidth, height);
            var fieldRect = new Rect(labelRect.xMax, rect.y, rect.width - labelRect.width, height);

            EditorGUI.LabelField(labelRect, label);
            EditorGUI.BeginChangeCheck();
            property.stringValue = EditorGUI.TextField(fieldRect, property.stringValue);
            if (EditorGUI.EndChangeCheck())
                property.serializedObject.ApplyModifiedProperties();
        }

        public static void DrawProjectFolderField(
            SerializedProperty folderProperty,
            SerializedProperty classNameProperty,
            GUIContent label)
        {
            if (folderProperty == null)
                return;

            var height = EditorGUIUtility.singleLineHeight;
            var rect = EditorGUILayout.GetControlRect(true, height);
            var labelRect = new Rect(rect.x, rect.y, EditorGUIUtility.labelWidth, height);
            var fieldRect = new Rect(labelRect.xMax, rect.y, rect.width - labelRect.width, height);

            EditorGUI.LabelField(labelRect, label);
            EditorGUI.BeginChangeCheck();
            folderProperty.stringValue = EditorGUI.TextField(fieldRect, folderProperty.stringValue);
            if (EditorGUI.EndChangeCheck())
                folderProperty.serializedObject.ApplyModifiedProperties();

            HandleProjectFolderDragAndDrop(fieldRect, folderProperty, classNameProperty);
        }

        private static void HandleProjectFolderDragAndDrop(
            Rect rect,
            SerializedProperty folderProperty,
            SerializedProperty classNameProperty)
        {
            var evt = Event.current;
            if (!rect.Contains(evt.mousePosition))
                return;

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!TryGetDraggedFolderAndClassName(out var folder, out var className))
                    {
                        DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                        return;
                    }

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        folderProperty.stringValue = folder;
                        if (classNameProperty != null && !string.IsNullOrEmpty(className))
                            classNameProperty.stringValue = className;
                        folderProperty.serializedObject.ApplyModifiedProperties();
                        GUI.changed = true;
                    }

                    evt.Use();
                    break;
            }
        }

        private static bool TryGetDraggedFolderAndClassName(out string folder, out string className)
        {
            folder = null;
            className = null;

            if (DragAndDrop.objectReferences == null || DragAndDrop.objectReferences.Length == 0)
                return false;

            var path = AssetDatabase.GetAssetPath(DragAndDrop.objectReferences[0]);
            if (string.IsNullOrEmpty(path) || !path.StartsWith("Assets"))
                return false;

            path = path.Replace('\\', '/');

            if (AssetDatabase.IsValidFolder(path))
            {
                folder = path.TrimEnd('/');
                return true;
            }

            if (path.EndsWith(".cs", System.StringComparison.OrdinalIgnoreCase))
            {
                folder = Path.GetDirectoryName(path)?.Replace('\\', '/');
                className = Path.GetFileNameWithoutExtension(path);
                return !string.IsNullOrEmpty(folder);
            }

            folder = Path.GetDirectoryName(path)?.Replace('\\', '/');
            return !string.IsNullOrEmpty(folder);
        }

        public static void DrawRuleClassNameField(SerializedProperty classNameProperty)
        {
            if (classNameProperty == null)
                return;

            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = GenerationLabelWidth;

            EditorGUILayout.BeginVertical(ResourcesConditionerEditorStyles.SectionBox);
            EditorGUILayout.LabelField("Conditioner 类型", EditorStyles.boldLabel);
            DrawAlignedTextField(classNameProperty,
                new GUIContent("类名", "生成到代码文件中的 Conditioner 类型名，需为合法 C# 标识符"));
            EditorGUILayout.EndVertical();

            EditorGUIUtility.labelWidth = oldLabelWidth;
        }
    }
}
