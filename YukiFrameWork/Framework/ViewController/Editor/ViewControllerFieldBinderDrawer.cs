#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using YukiFrameWork.Extension;
using Object = UnityEngine.Object;

namespace YukiFrameWork
{
    /// <summary>
    /// ViewController 字段绑定可视化编辑器 — 级别 / 对象 / 组件 从左到右声明。
    /// </summary>
    public static class ViewControllerFieldBinderDrawer
    {
        private const float IndexWidth = 22f;
        private const float LevelWidth = 78f;
        private const float FieldNameWidth = 118f;
        private const float ColumnGap = 4f;
        private const float RemoveButtonWidth = 26f;
        private const float RowSidePadding = 6f;
        private const float RowHeight = 30f;
        private const float HeaderHeight = 26f;
        private const float ModeBarHeight = 24f;
        private const float MinPickerColumnWidth = 72f;

        private static readonly GUIContent RemoveFieldContent = new GUIContent("×", "移除字段绑定");
        private static readonly GUIContent AddIcon = EditorGUIUtility.IconContent("d_Toolbar Plus");
        private static readonly GUIContent CodeIcon = EditorGUIUtility.IconContent("d_cs Script Icon");

        public static void Draw(
            ISerializedFieldInfo info,
            Component target,
            Action onGenerateCode,
            GenericDataBase bindData = null)
        {
            if (info == null || target == null) return;

            ViewControllerFieldBinderStyles.Ensure();
            EditorGUI.BeginDisabledGroup(Application.isPlaying);

            var fields = info.GetSerializeFields().ToList();
            var yukiBinds = target.GetComponentsInChildren<YukiBind>(true);
            var foldout = PlayerPrefs.GetInt("BindFoldOut", 1) == 1;
            var pickMode = ViewControllerFieldBinderPicker.GetPickMode();

            EditorGUILayout.Space(8);
            DrawFoldoutHeader(ref foldout, fields.Count);
            PlayerPrefs.SetInt("BindFoldOut", foldout ? 1 : 0);

            if (fields.Count <= 0 && yukiBinds is { Length: > 0 })
            {
                EditorGUILayout.HelpBox(
                    FrameWorkConfigData.IsEN
                        ? "YukiBind components detected. You can generate code after adding bindings."
                        : "检测到 YukiBind 组件，添加绑定后可生成代码。",
                    MessageType.Info);
            }

            if (!foldout)
            {
                EditorGUI.EndDisabledGroup();
                return;
            }

            DrawPanel(info, target, fields, yukiBinds, onGenerateCode, pickMode, bindData);
            EditorGUI.EndDisabledGroup();
        }

        private static void DrawFoldoutHeader(ref bool foldout, int count)
        {
            var lineRect = GUILayoutUtility.GetRect(0, 22, GUILayout.ExpandWidth(true));

            var foldoutRect = new Rect(lineRect.x, lineRect.y, lineRect.width - (count > 0 ? 56 : 0), 22);
            foldout = EditorGUI.Foldout(foldoutRect, foldout, FrameWorkConfigData.BindExtensionInfo, true, ViewControllerFieldBinderStyles.FoldoutTitle);

            if (count > 0)
            {
                var badgeRect = new Rect(lineRect.xMax - 48, lineRect.y + 2, 44, 18);
                if (Event.current.type == EventType.Repaint)
                {
                    ViewControllerFieldBinderStyles.DrawRect(badgeRect, new Color(0.95f, 0.78f, 0.25f, 0.12f));
                    ViewControllerFieldBinderStyles.DrawBorder(badgeRect, new Color(0.95f, 0.78f, 0.25f, 0.3f));
                }

                var badgeText = $"{count} {FrameWorkConfigData.BindingCountLabel}";
                GUI.Label(badgeRect, badgeText, ViewControllerFieldBinderStyles.CountBadge);
            }
        }

        private static void DrawModeBar(ref ViewControllerBindPickMode pickMode, Component target)
        {
            var rect = GUILayoutUtility.GetRect(0, ModeBarHeight, GUILayout.ExpandWidth(true));
            if (Event.current.type == EventType.Repaint)
                ViewControllerFieldBinderStyles.DrawRect(rect, ViewControllerFieldBinderStyles.ModeBarBg);

            var labels = ViewControllerFieldBinderPicker.GetModeLabels();
            var labelWidth = 56f;
            var toggleWidth = (rect.width - labelWidth - 16) / labels.Length;

            GUI.Label(new Rect(rect.x + 8, rect.y + 4, labelWidth, ModeBarHeight - 4),
                FrameWorkConfigData.ObjectPickModeLabel,
                ViewControllerFieldBinderStyles.ModeBarLabel);

            for (var i = 0; i < labels.Length; i++)
            {
                var toggleRect = new Rect(rect.x + labelWidth + 4 + i * (toggleWidth + 2), rect.y + 3, toggleWidth, ModeBarHeight - 6);
                var mode = (ViewControllerBindPickMode)i;
                var active = pickMode == mode;
                var style = active ? ViewControllerFieldBinderStyles.ModeToggleOn : ViewControllerFieldBinderStyles.ModeToggle;

                if (GUI.Button(toggleRect, labels[i], style) && !active)
                {
                    pickMode = mode;
                    ViewControllerFieldBinderPicker.SetPickMode(mode);
                    SaveTarget(target);
                }
            }
        }

        private static void DrawPanel(
            ISerializedFieldInfo info,
            Component target,
            List<SerializeFieldData> fields,
            YukiBind[] yukiBinds,
            Action onGenerateCode,
            ViewControllerBindPickMode pickMode,
            GenericDataBase bindData)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            var mode = pickMode;
            DrawModeBar(ref mode, target);
            pickMode = mode;

            if (bindData != null)
            {
                EditorGUILayout.Space(4);
                DrawAutoBindSection(info, target, bindData, pickMode);
            }

            EditorGUILayout.Space(2);
            DrawHeaderRow();

            var root = target.transform;
            SerializeFieldData removeTarget = null;

            if (fields.Count == 0)
            {
                EditorGUILayout.LabelField(
                    FrameWorkConfigData.IsEN ? "No bindings yet. Click \"Add Binding\" below." : "暂无绑定，点击下方「添加绑定」开始配置。",
                    ViewControllerFieldBinderStyles.EmptyHint);
            }
            else
            {
                for (var i = 0; i < fields.Count; i++)
                {
                    if (DrawFieldRow(fields[i], target, root, i, pickMode))
                        removeTarget = fields[i];
                }
            }

            if (removeTarget != null)
            {
                Undo.RecordObject(target, "Remove Field Binding");
                info.RemoveFieldData(removeTarget);
                SaveTarget(target);
            }

            EditorGUILayout.Space(6);
            DrawFooter(info, target, fields, yukiBinds, onGenerateCode);
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4);
        }

        private static float GetPickerColumnWidth(float rowWidth, float pickerAreaStartX, float rowStartX)
        {
            const float reservedRight = FieldNameWidth + ColumnGap + RemoveButtonWidth + ColumnGap;
            var pairTotal = rowWidth - (pickerAreaStartX - rowStartX) - reservedRight;
            return Mathf.Max(MinPickerColumnWidth, (pairTotal - ColumnGap) * 0.5f);
        }

        private static void DrawHeaderRow()
        {
            var rect = GUILayoutUtility.GetRect(0, HeaderHeight, GUILayout.ExpandWidth(true));
            if (Event.current.type == EventType.Repaint)
            {
                ViewControllerFieldBinderStyles.DrawRect(rect, new Color(0.95f, 0.78f, 0.25f, EditorGUIUtility.isProSkin ? 0.12f : 0.18f));
                ViewControllerFieldBinderStyles.DrawRect(new Rect(rect.x, rect.yMax - 1, rect.width, 1),
                    new Color(0.95f, 0.78f, 0.25f, 0.35f));
            }

            var x = rect.x + RowSidePadding;
            DrawHeaderLabel(ref x, rect.y, IndexWidth, "#");
            DrawHeaderLabel(ref x, rect.y, LevelWidth, FrameWorkConfigData.FieldLevelHeader);
            var pickerWidth = GetPickerColumnWidth(rect.width, x, rect.x);
            DrawHeaderLabel(ref x, rect.y, pickerWidth, FrameWorkConfigData.FieldObjectHeader);
            DrawHeaderLabel(ref x, rect.y, pickerWidth, FrameWorkConfigData.FieldComponentHeader);
            DrawHeaderLabel(ref x, rect.y, FieldNameWidth, FrameWorkConfigData.FieldNameHeader);
        }

        private static void DrawHeaderLabel(ref float x, float y, float width, string text)
        {
            GUI.Label(new Rect(x, y, width, HeaderHeight), text, ViewControllerFieldBinderStyles.HeaderLabel);
            x += width + 4;
        }

        private static bool DrawFieldRow(
            SerializeFieldData data,
            Component target,
            Transform root,
            int index,
            ViewControllerBindPickMode pickMode)
        {
            var rect = GUILayoutUtility.GetRect(0, RowHeight, GUILayout.ExpandWidth(true));
            var bg = index % 2 == 0 ? ViewControllerFieldBinderStyles.RowEvenBg : ViewControllerFieldBinderStyles.RowOddBg;

            if (Event.current.type == EventType.Repaint)
                ViewControllerFieldBinderStyles.DrawRect(rect, bg);

            var x = rect.x + RowSidePadding;
            var y = rect.y + 4;
            var controlHeight = RowHeight - 8;

            GUI.Label(new Rect(x, y, IndexWidth, controlHeight), (index + 1).ToString(), ViewControllerFieldBinderStyles.IndexLabel);
            x += IndexWidth + ColumnGap;

            var levelRect = new Rect(x, y, LevelWidth, controlHeight);
            x += LevelWidth + ColumnGap;

            var pickerWidth = GetPickerColumnWidth(rect.width, x, rect.x);
            var objectRect = new Rect(x, y, pickerWidth, controlHeight);
            x += pickerWidth + ColumnGap;

            var componentRect = new Rect(x, y, pickerWidth, controlHeight);
            x += pickerWidth + ColumnGap;

            var fieldRect = new Rect(x, y, FieldNameWidth, controlHeight);
            x += FieldNameWidth + 4;

            var removeRect = new Rect(x, y, 26, controlHeight);

            var levelIndex = EditorGUI.Popup(levelRect, data.fieldLevelIndex, data.fieldLevel);
            if (Event.current.type == EventType.Repaint)
            {
                var accent = ViewControllerFieldBinderStyles.GetLevelColor(levelIndex);
                ViewControllerFieldBinderStyles.DrawRect(new Rect(levelRect.x, levelRect.yMax - 2, levelRect.width, 2), accent);
            }

            var gameObject = ResolveGameObject(data);
            var objectLabel = gameObject != null
                ? ViewControllerFieldBinderPicker.GetDisplayPath(gameObject.transform, root)
                : FrameWorkConfigData.SelectObjectBtn;
            var objectStyle = gameObject != null ? ViewControllerFieldBinderStyles.PickerButton : ViewControllerFieldBinderStyles.PickerButtonEmpty;

            var objectCharLimit = Mathf.Max(8, Mathf.FloorToInt(pickerWidth / 7f));
            if (GUI.Button(objectRect, new GUIContent("  " + TruncateLabel(objectLabel, objectCharLimit), EditorGUIUtility.IconContent("d_GameObject Icon").image), objectStyle))
                ViewControllerFieldBinderPicker.ShowObjectMenu(root, pickMode, go => ApplyObjectSelection(data, go, target));

            var componentLabel = GetSelectedComponentLabel(data);
            var hasComponent = gameObject != null;
            var componentStyle = hasComponent ? ViewControllerFieldBinderStyles.PickerButton : ViewControllerFieldBinderStyles.PickerButtonEmpty;

            var componentCharLimit = Mathf.Max(8, Mathf.FloorToInt(pickerWidth / 7f));
            if (GUI.Button(componentRect, new GUIContent("  " + TruncateLabel(componentLabel, componentCharLimit), EditorGUIUtility.IconContent("d_CustomTool").image), componentStyle))
            {
                if (gameObject == null)
                {
                    var menu = new GenericMenu();
                    menu.AddDisabledItem(new GUIContent(FrameWorkConfigData.SelectObjectFirstForComponent));
                    menu.ShowAsContext();
                }
                else
                {
                    ViewControllerFieldBinderPicker.ShowBoundObjectComponentMenu(
                        gameObject,
                        typeIndex => ApplyComponentSelection(data, gameObject, typeIndex, target));
                }
            }

            var fieldName = EditorGUI.TextField(fieldRect, data.fieldName ?? string.Empty, ViewControllerFieldBinderStyles.FieldInput);
            var remove = GUI.Button(removeRect, RemoveFieldContent, ViewControllerFieldBinderStyles.RemoveButton);

            if (levelIndex != data.fieldLevelIndex || fieldName != data.fieldName)
            {
                Undo.RecordObject(target, "Change Field Binding");
                data.fieldLevelIndex = levelIndex;
                data.fieldName = fieldName;
                SaveTarget(target);
            }

            return remove;
        }

        private static void DrawAutoBindSection(
            ISerializedFieldInfo info,
            Component target,
            GenericDataBase bindData,
            ViewControllerBindPickMode pickMode)
        {
            EditorGUILayout.LabelField(FrameWorkConfigData.AutoBindSectionLabel, EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(FrameWorkConfigData.AutoBindTypeHint, MessageType.None);

            if (pickMode == ViewControllerBindPickMode.Prefab)
                DrawAutoBindPrefabField(target, bindData);

            var scope = CreateTypeScopeContext(target, bindData, pickMode);
            var types = bindData.AutoBindComponentTypes;
            var removeIndex = -1;

            for (var i = 0; i < types.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(FrameWorkConfigData.AutoBindTypeLabel, GUILayout.Width(72));

                var displayName = string.IsNullOrWhiteSpace(types[i])
                    ? FrameWorkConfigData.AutoBindSelectTypeBtn
                    : ViewControllerComponentTypeScope.GetDisplayName(types[i]);
                var pickStyle = string.IsNullOrWhiteSpace(types[i])
                    ? ViewControllerFieldBinderStyles.PickerButtonEmpty
                    : ViewControllerFieldBinderStyles.PickerButton;

                if (GUILayout.Button(displayName, pickStyle))
                {
                    var rowIndex = i;
                    var pickRect = GUILayoutUtility.GetLastRect();
                    OpenComponentTypePopup(pickRect, scope, entry =>
                    {
                        Undo.RecordObject(target, "Change Auto Bind Type");
                        types[rowIndex] = entry.AssemblyQualifiedName;
                        SaveTarget(target);
                    });
                }

                if (GUILayout.Button("×", GUILayout.Width(24)))
                    removeIndex = i;

                EditorGUILayout.EndHorizontal();
            }

            if (removeIndex >= 0)
            {
                Undo.RecordObject(target, "Remove Auto Bind Type");
                types.RemoveAt(removeIndex);
                SaveTarget(target);
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(FrameWorkConfigData.AutoBindAddTypeBtn, ViewControllerFieldBinderStyles.FooterButton))
            {
                var addRect = GUILayoutUtility.GetLastRect();
                OpenComponentTypePopup(addRect, scope, entry =>
                {
                    if (types.Contains(entry.AssemblyQualifiedName)) return;
                    Undo.RecordObject(target, "Add Auto Bind Type");
                    types.Add(entry.AssemblyQualifiedName);
                    SaveTarget(target);
                });
            }

            if (GUILayout.Button(FrameWorkConfigData.AutoBindBuildBtn, ViewControllerFieldBinderStyles.FooterButtonPrimary))
            {
                if (!ValidateAutoBindScope(scope))
                    return;

                if (types.All(string.IsNullOrWhiteSpace))
                {
                    EditorUtility.DisplayDialog(
                        FrameWorkConfigData.AutoBindSectionLabel,
                        FrameWorkConfigData.AutoBindNoTypes,
                        "OK");
                }
                else
                {
                    var added = ViewControllerFieldBinderAutoBuilder.Build(
                        info,
                        target,
                        target.transform,
                        pickMode,
                        types,
                        bindData.AutoBindPrefab);
                    Debug.Log(string.Format(FrameWorkConfigData.AutoBindAddedCount, added));
                    SaveTarget(target);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        private static void DrawAutoBindPrefabField(Component target, GenericDataBase bindData)
        {
            EditorGUILayout.BeginHorizontal();
            var prefab = (GameObject)EditorGUILayout.ObjectField(
                FrameWorkConfigData.PrefabAssetLabel,
                bindData.AutoBindPrefab,
                typeof(GameObject),
                false);

            if (prefab != bindData.AutoBindPrefab)
            {
                Undo.RecordObject(target, "Change Auto Bind Prefab");
                bindData.AutoBindPrefab = prefab;
                SaveTarget(target);
            }

            if (GUILayout.Button("...", GUILayout.Width(28)))
            {
                ViewControllerPrefabPickWindow.Show(go =>
                {
                    if (go == null) return;
                    Undo.RecordObject(target, "Change Auto Bind Prefab");
                    bindData.AutoBindPrefab = go;
                    SaveTarget(target);
                });
            }

            EditorGUILayout.EndHorizontal();
        }

        private static ComponentTypeScopeContext CreateTypeScopeContext(
            Component target,
            GenericDataBase bindData,
            ViewControllerBindPickMode pickMode)
        {
            return new ComponentTypeScopeContext
            {
                HierarchyRoot = target.transform,
                Mode = pickMode,
                PrefabAsset = bindData.AutoBindPrefab
            };
        }

        private static bool ValidateAutoBindScope(ComponentTypeScopeContext scope)
        {
            if (scope.Mode != ViewControllerBindPickMode.Prefab) return true;
            if (scope.PrefabAsset != null) return true;

            EditorUtility.DisplayDialog(
                FrameWorkConfigData.AutoBindSectionLabel,
                FrameWorkConfigData.AutoBindPrefabRequired,
                "OK");
            return false;
        }

        private static void OpenComponentTypePopup(
            Rect activatorRect,
            ComponentTypeScopeContext scope,
            Action<ScopedComponentType> onSelected)
        {
            if (!ValidateAutoBindScope(scope)) return;

            var types = ViewControllerComponentTypeScope.Collect(scope);
            if (types.Count == 0)
            {
                EditorUtility.DisplayDialog(
                    FrameWorkConfigData.AutoBindTypePopupTitle,
                    FrameWorkConfigData.AutoBindNoTypesInScope,
                    "OK");
                return;
            }

            ViewControllerComponentTypePickPopup.Show(activatorRect, scope, onSelected);
        }

        private static void DrawFooter(
            ISerializedFieldInfo info,
            Component target,
            List<SerializeFieldData> fields,
            YukiBind[] yukiBinds,
            Action onGenerateCode)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            var addContent = new GUIContent("  " + FrameWorkConfigData.AddFieldBindingBtn, AddIcon.image);
            if (GUILayout.Button(addContent, ViewControllerFieldBinderStyles.FooterButton, GUILayout.MinWidth(110)))
            {
                Undo.RecordObject(target, "Add Field Binding");
                info.AddFieldData(new SerializeFieldData { fieldName = $"field{fields.Count + 1}" });
                SaveTarget(target);
            }

            if (target is YMonoBehaviour &&
                CodeManager.CheckViewBindder(info, yukiBinds))
            {
                var codeContent = new GUIContent("  " + FrameWorkConfigData.GenerateBindingCodeBtn, CodeIcon.image);
                if (GUILayout.Button(codeContent, ViewControllerFieldBinderStyles.FooterButtonPrimary, GUILayout.MinWidth(110)))
                    onGenerateCode?.Invoke();
            }

            EditorGUILayout.EndHorizontal();
        }

        private static string TruncateLabel(string text, int maxChars)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxChars) return text;
            return text[..(maxChars - 1)] + "…";
        }

        private static string GetSelectedComponentLabel(SerializeFieldData data)
        {
            var names = GetComponentDisplayNames(data);
            if (names.Length == 0) return FrameWorkConfigData.SelectComponentBtn;
            var index = Mathf.Clamp(data.fieldTypeIndex, 0, names.Length - 1);
            return names[index];
        }

        private static void ApplyComponentSelection(SerializeFieldData data, GameObject go, int typeIndex, Component target)
        {
            Undo.RecordObject(target, "Select Binding Component");
            data.target = go;
            data.fieldTypeIndex = typeIndex;
            if (string.IsNullOrWhiteSpace(data.fieldName) || data.fieldName.StartsWith("field"))
            {
                var names = new SerializeFieldData(go).Components;
                var suffix = typeIndex > 0 && typeIndex < names.Count
                    ? GetShortTypeName(names[typeIndex])
                    : go.name;
                data.fieldName = SanitizeFieldName(suffix == "GameObject" ? go.name : suffix);
            }
            SaveTarget(target);
        }

        private static void ApplyObjectSelection(SerializeFieldData data, GameObject go, Component target)
        {
            Undo.RecordObject(target, "Select Binding Object");
            data.target = go;
            data.fieldTypeIndex = 0;
            if (string.IsNullOrWhiteSpace(data.fieldName) || data.fieldName.StartsWith("field"))
                data.fieldName = SanitizeFieldName(go.name);
            SaveTarget(target);
        }

        private static GameObject ResolveGameObject(SerializeFieldData data)
        {
            if (data.target == null) return null;
            return data.target is GameObject go ? go : (data.target as Component)?.gameObject;
        }

        private static string[] GetComponentDisplayNames(SerializeFieldData data)
        {
            var components = data.Components;
            if (components == null || components.Count == 0)
                return Array.Empty<string>();

            return components.Select(GetShortTypeName).ToArray();
        }

        private static string GetShortTypeName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName)) return fullName;
            var lastDot = fullName.LastIndexOf('.');
            return lastDot >= 0 ? fullName[(lastDot + 1)..] : fullName;
        }

        private static string SanitizeFieldName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "field";
            var cleaned = name.Replace(" ", string.Empty);
            if (char.IsDigit(cleaned[0]))
                cleaned = "_" + cleaned;
            return cleaned;
        }

        private static void SaveTarget(Component target)
        {
            if (PrefabUtility.IsPartOfPrefabInstance(target))
                PrefabUtility.RecordPrefabInstancePropertyModifications(target);
            EditorUtility.SetDirty(target);
        }
    }
}
#endif
