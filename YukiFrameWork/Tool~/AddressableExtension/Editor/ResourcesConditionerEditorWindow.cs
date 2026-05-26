using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;

namespace YukiFramework.AddressableExtension.Editor
{
    public sealed class ResourcesConditionerEditorWindow : EditorWindow
    {
        private const float RuleListMinHeight = 240f;
        private const float EntryListMinHeight = 200f;
        private const float DetailMinHeight = 168f;
        private const float SplitterWidth = 6f;

        // 仅表头需要补偿拖拽列；行 rect 已由 ReorderableList 缩进
        private const float RuleListHeaderDragIndent = 20f;
        private const float RuleColPad = 6f;
        private const float RuleColEnabled = 52f;
        private const float RuleColEnabledToClassGap = 12f;
        private const float RuleColToggleSize = 18f;
        private const float RuleColSuffix = 48f;
        private const float RuleColCount = 44f;
        private const float RuleColClassRatio = 0.34f;

        private ResourcesConditionerRuleSet ruleSet;
        private SerializedObject ruleSetSerialized;
        private SerializedProperty rulesProperty;
        private ReorderableList rulesList;

        private TreeViewState entryTreeState;
        private MultiColumnHeader entryHeader;
        private ResourcesConditionerEntryTreeView entryTreeView;
        private SearchField entrySearchField;

        private readonly ResConditionerMatchIndex matchIndex = new ResConditionerMatchIndex();

        private Vector2 mainScroll;
        private Vector2 ruleScroll;
        private float listPanelHeight = 280f;
        private float entryTreeHeight = 220f;
        private int selectedRuleIndex = -1;
        private bool replaceExistingOnScan = true;
        private bool showGenerationSettings;
        private string previewObjectName = "GameState";
        private List<string> validationMessages = new List<string>();

        private int cachedEnabledCount = -1;
        private int cachedTotalCount = -1;
        private int cachedSelectedMatchCount = -1;
        private bool indexRebuildScheduled;
        private bool validationDirty = true;

        [MenuItem("Window/Asset Management/Addressables/Resources Conditioner Rules", false, 2052)]
        [MenuItem("YukiFrameWork/Addressable/规则器编辑器", false, 100)]
        public static void ShowWindow()
        {
            var window = GetWindow<ResourcesConditionerEditorWindow>();
            window.titleContent = new GUIContent("Res Conditioner",
                EditorGUIUtility.IconContent("d_UnityEditor.Graphs.AnimatorControllerTool").image);
            window.minSize = new Vector2(900f, 560f);
            window.Show();
        }

        [MenuItem("Window/Asset Management/Addressables/Resources Conditioner Rules", true)]
        [MenuItem("YukiFrameWork/Addressable/规则器编辑器", true)]
        private static bool ShowWindowValidate()
        {
            return AddressableAssetSettingsDefaultObject.SettingsExists;
        }

        private void OnEnable()
        {
            entryTreeState = new TreeViewState();
            entryHeader = ResourcesConditionerEntryTreeView.CreateDefaultHeader();
            entryTreeView = new ResourcesConditionerEntryTreeView(entryTreeState, entryHeader);
            entrySearchField = new SearchField();
            entrySearchField.downOrUpArrowKeyPressed += entryTreeView.SetFocusAndEnsureSelectedItem;

            listPanelHeight = Mathf.Clamp(position.height * 0.38f, RuleListMinHeight, 420f);
            entryTreeHeight = EntryListMinHeight;

            LoadOrCreateRuleSet();
            BuildRulesList();
            ScheduleMatchIndexRebuild(immediate: true);
        }

        private void OnDisable()
        {
            EditorApplication.delayCall -= DelayedMatchIndexRebuild;
        }

        private void OnGUI()
        {
            ruleSetSerialized?.Update();
            DrawToolbar();

            EditorGUILayout.Space(6f);

            if (!AddressableAssetSettingsDefaultObject.SettingsExists)
            {
                EditorGUILayout.HelpBox("未找到 Addressable 设置，请先在 Window > Asset Management > Addressables 中创建。", MessageType.Error);
                if (GUILayout.Button("打开 Addressables 窗口", GUILayout.Height(26f)))
                    AddressablesEditorBridge.OpenAddressablesWindow();
                return;
            }

            if (ruleSet == null)
            {
                EditorGUILayout.HelpBox("规则集资产未加载。", MessageType.Warning);
                if (GUILayout.Button("创建规则集", GUILayout.Height(26f)))
                    LoadOrCreateRuleSet();
                return;
            }

            mainScroll = EditorGUILayout.BeginScrollView(mainScroll);
            DrawRuleSetBar();

            EditorGUILayout.Space(4f);
            DrawRulesPanel();
            DrawListSplitter();

            EditorGUILayout.Space(4f);
            DrawSelectedRuleInspector();

            EditorGUILayout.Space(6f);
            DrawEntriesPanel();
            EditorGUILayout.EndScrollView();

            if (ruleSetSerialized != null && ruleSetSerialized.hasModifiedProperties)
                ruleSetSerialized.ApplyModifiedProperties();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button(new GUIContent(" 扫描", EditorGUIUtility.IconContent("d_Refresh").image),
                    EditorStyles.toolbarButton, GUILayout.Width(64f)))
                ScanFromAddressables();

            if (GUILayout.Button(new GUIContent(" 刷新匹配", EditorGUIUtility.IconContent("d_Refresh").image),
                    EditorStyles.toolbarButton, GUILayout.Width(76f)))
                ScheduleMatchIndexRebuild(immediate: true);

            replaceExistingOnScan = GUILayout.Toggle(replaceExistingOnScan, "覆盖同键", EditorStyles.toolbarButton,
                GUILayout.Width(68f));

            ResourcesConditionerEditorStyles.DrawToolbarDivider();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button(new GUIContent(" Addressables", EditorGUIUtility.IconContent("d_FolderOpened Icon").image),
                    EditorStyles.toolbarButton, GUILayout.Width(108f)))
                AddressablesEditorBridge.OpenAddressablesWindow();

            ResourcesConditionerEditorStyles.DrawToolbarDivider();

            if (GUILayout.Button(new GUIContent(" 生成代码", EditorGUIUtility.IconContent("d_ScriptableObject Icon").image),
                    EditorStyles.toolbarButton, GUILayout.Width(84f)))
                GenerateCode();

            if (GUILayout.Button(new GUIContent(" 定位输出", EditorGUIUtility.IconContent("d_ViewToolZoom").image),
                    EditorStyles.toolbarButton, GUILayout.Width(76f)))
            {
                var outputPath = ruleSet != null
                    ? ruleSet.GeneratedFilePath
                    : ResConditionerCodeGenerator.DefaultOutputFilePath;
                var obj = AssetDatabase.LoadAssetAtPath<Object>(outputPath);
                if (obj) EditorGUIUtility.PingObject(obj);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawRuleSetBar()
        {
            EditorGUILayout.BeginVertical(ResourcesConditionerEditorStyles.SectionBox);
            EditorGUILayout.BeginHorizontal();
            var barLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = ResConditionerEditorGuiHelpers.GenerationLabelWidth;
            EditorGUI.BeginChangeCheck();
            var newSet = (ResourcesConditionerRuleSet)EditorGUILayout.ObjectField(
                new GUIContent("规则集资产", "持久化规则配置，生成代码时读取此资产"),
                ruleSet, typeof(ResourcesConditionerRuleSet), false);
            EditorGUIUtility.labelWidth = barLabelWidth;
            if (EditorGUI.EndChangeCheck() && newSet != ruleSet)
            {
                ruleSet = newSet;
                BuildRulesList();
                ScheduleMatchIndexRebuild(immediate: true);
            }

            if (GUILayout.Button(new GUIContent("保存", EditorGUIUtility.IconContent("SaveActive").image),
                    GUILayout.Width(64f), GUILayout.Height(20f)))
            {
                EditorUtility.SetDirty(ruleSet);
                AssetDatabase.SaveAssets();
            }

            EditorGUILayout.EndHorizontal();

            showGenerationSettings = EditorGUILayout.Foldout(showGenerationSettings, "代码生成设置", true);
            if (showGenerationSettings && ruleSetSerialized != null)
            {
                EditorGUILayout.Space(2f);
                ResConditionerEditorGuiHelpers.DrawGenerationSettings(ruleSetSerialized);
                if (GUILayout.Button("恢复默认生成设置", GUILayout.Width(140f)))
                {
                    if (ruleSet != null)
                        ruleSet.ResetGenerationDefaults();
                    ruleSetSerialized.Update();
                }
            }

            EditorGUILayout.EndVertical();
        }

        private static readonly int ListResizeControlHash = "ResConditionerListResize".GetHashCode();

        private void DrawListSplitter()
        {
            var rect = GUILayoutUtility.GetRect(0, SplitterWidth, GUILayout.ExpandWidth(true));
            var controlId = GUIUtility.GetControlID(ListResizeControlHash, FocusType.Passive);

            EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin
                ? new Color(1f, 1f, 1f, 0.08f)
                : new Color(0f, 0f, 0f, 0.1f));
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeVertical);

            var e = Event.current;
            switch (e.GetTypeForControl(controlId))
            {
                case EventType.MouseDown when rect.Contains(e.mousePosition):
                    GUIUtility.hotControl = controlId;
                    e.Use();
                    break;
                case EventType.MouseDrag when GUIUtility.hotControl == controlId:
                    listPanelHeight = Mathf.Clamp(listPanelHeight + e.delta.y, RuleListMinHeight, 480f);
                    Repaint();
                    e.Use();
                    break;
                case EventType.MouseUp when GUIUtility.hotControl == controlId:
                    GUIUtility.hotControl = 0;
                    e.Use();
                    break;
            }
        }

        private void DrawRulesPanel()
        {
            EditorGUILayout.BeginVertical(ResourcesConditionerEditorStyles.SectionBox, GUILayout.Height(listPanelHeight));

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("规则列表", ResourcesConditionerEditorStyles.SectionTitle);
            GUILayout.FlexibleSpace();
            UpdateRuleCountCache();
            EditorGUILayout.LabelField($"已启用 {cachedEnabledCount} / 共 {cachedTotalCount} 条", EditorStyles.miniLabel);
            if (!matchIndex.IsBuilt)
                EditorGUILayout.LabelField("匹配索引未构建", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            RefreshValidationIfNeeded();
            if (validationMessages.Count > 0)
            {
                var hasError = validationMessages.Any(m => m.StartsWith("错误:"));
                EditorGUILayout.HelpBox(string.Join("\n", validationMessages),
                    hasError ? MessageType.Error : MessageType.Warning);
            }

            ruleScroll = EditorGUILayout.BeginScrollView(ruleScroll, GUILayout.ExpandHeight(true));
            rulesList?.DoLayoutList();
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(4f);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent(" 添加规则", EditorGUIUtility.IconContent("Toolbar Plus").image),
                    GUILayout.Height(22f)))
                rulesList?.onAddCallback?.Invoke(rulesList);

            GUI.enabled = selectedRuleIndex >= 0 && selectedRuleIndex < rulesProperty?.arraySize;
            if (GUILayout.Button(new GUIContent(" 删除选中", EditorGUIUtility.IconContent("Toolbar Minus").image),
                    GUILayout.Height(22f)))
                rulesList?.onRemoveCallback?.Invoke(rulesList);
            GUI.enabled = true;

            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("拖拽左侧手柄可排序", EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }

        private void DrawSelectedRuleInspector()
        {
            EditorGUILayout.BeginVertical(ResourcesConditionerEditorStyles.SectionBox);

            EditorGUILayout.LabelField("规则详情", ResourcesConditionerEditorStyles.SectionTitle);

            if (selectedRuleIndex < 0 || rulesProperty == null || selectedRuleIndex >= rulesProperty.arraySize)
            {
                EditorGUILayout.HelpBox("在上方列表中选择一条规则，可在此编辑详情并预览匹配资源。", MessageType.Info);
                EditorGUILayout.EndVertical();
                return;
            }

            var element = rulesProperty.GetArrayElementAtIndex(selectedRuleIndex);
            var rule = GetSelectedRule();
            var useWideLayout = position.width >= 780f;

            EditorGUI.BeginChangeCheck();

            ResConditionerEditorGuiHelpers.DrawRuleClassNameField(element.FindPropertyRelative("className"));
            EditorGUILayout.Space(4f);

            if (useWideLayout)
                EditorGUILayout.BeginHorizontal();

            // 左侧：路径等字段（类名在上方单独编辑）
            EditorGUILayout.BeginVertical(useWideLayout ? GUILayout.Width(position.width * 0.52f) : GUILayout.ExpandWidth(true));
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = ResourcesConditionerEditorStyles.LabelWidth;

            EditorGUILayout.PropertyField(element.FindPropertyRelative("enabled"), new GUIContent("启用"));
            EditorGUILayout.PropertyField(element.FindPropertyRelative("rulePath"), new GUIContent("Rule Path"));
            EditorGUILayout.PropertyField(element.FindPropertyRelative("suffix"), new GUIContent("后缀"));
            EditorGUILayout.PropertyField(element.FindPropertyRelative("sourceGroup"), new GUIContent("来源 Group"));
            EditorGUILayout.PropertyField(element.FindPropertyRelative("note"), new GUIContent("备注"));
            EditorGUILayout.PropertyField(element.FindPropertyRelative("lockCustomNaming"),
                new GUIContent("锁定自定义", "扫描时不覆盖类名、RulePath、后缀"));

            EditorGUIUtility.labelWidth = oldLabelWidth;
            EditorGUILayout.EndVertical();

            // 右侧：预览
            EditorGUILayout.BeginVertical(ResourcesConditionerEditorStyles.SectionBox);
            EditorGUILayout.LabelField("路径预览", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("对象名", GUILayout.Width(ResourcesConditionerEditorStyles.LabelWidth));
            previewObjectName = EditorGUILayout.TextField(previewObjectName);
            EditorGUILayout.EndHorizontal();

            if (rule != null)
            {
                var preview = rule.PreviewPath(previewObjectName);
                EditorGUILayout.LabelField(preview, ResourcesConditionerEditorStyles.PreviewBox, GUILayout.MinHeight(28f));

                EditorGUILayout.Space(4f);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("匹配资源数", ResourcesConditionerEditorStyles.StatLabel, GUILayout.Width(80f));
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField(GetSelectedMatchCount().ToString(), ResourcesConditionerEditorStyles.StatValue,
                    GUILayout.Width(80f));
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();

            if (useWideLayout)
                EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
                OnRulesChanged(rebuildIndex: true, refreshEntries: true);

            EditorGUILayout.EndVertical();
        }

        private void DrawEntriesPanel()
        {
            EditorGUILayout.BeginVertical(ResourcesConditionerEditorStyles.SectionBox);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("匹配资源", ResourcesConditionerEditorStyles.SectionTitle);
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("双击行 Ping 资源", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(2f);
            if (entryTreeView == null)
            {
                EditorGUILayout.EndVertical();
                return;
            }

            var searchRect = GUILayoutUtility.GetRect(0, 22f, GUILayout.ExpandWidth(true));
            entryTreeView.searchString = entrySearchField.OnGUI(searchRect, entryTreeView.searchString);

            DrawEntryTreeResizeHandle();

            var treeRect = GUILayoutUtility.GetRect(0, entryTreeHeight, GUILayout.ExpandWidth(true));
            entryTreeView.OnGUI(treeRect);

            EditorGUILayout.EndVertical();
        }

        private static readonly int EntryTreeResizeControlHash = "ResConditionerEntryTreeResize".GetHashCode();

        private void DrawEntryTreeResizeHandle()
        {
            var rect = GUILayoutUtility.GetRect(0, SplitterWidth, GUILayout.ExpandWidth(true));
            var controlId = GUIUtility.GetControlID(EntryTreeResizeControlHash, FocusType.Passive);

            EditorGUI.DrawRect(rect, EditorGUIUtility.isProSkin
                ? new Color(1f, 1f, 1f, 0.06f)
                : new Color(0f, 0f, 0f, 0.08f));
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeVertical);

            var e = Event.current;
            switch (e.GetTypeForControl(controlId))
            {
                case EventType.MouseDown when rect.Contains(e.mousePosition):
                    GUIUtility.hotControl = controlId;
                    e.Use();
                    break;
                case EventType.MouseDrag when GUIUtility.hotControl == controlId:
                    entryTreeHeight = Mathf.Clamp(entryTreeHeight + e.delta.y, EntryListMinHeight, 600f);
                    Repaint();
                    e.Use();
                    break;
                case EventType.MouseUp when GUIUtility.hotControl == controlId:
                    GUIUtility.hotControl = 0;
                    e.Use();
                    break;
            }
        }

        private void BuildRulesList()
        {
            if (ruleSet == null)
                return;

            ruleSetSerialized = new SerializedObject(ruleSet);
            rulesProperty = ruleSetSerialized.FindProperty("rules");

            rulesList = new ReorderableList(ruleSetSerialized, rulesProperty, true, true, false, false)
            {
                drawHeaderCallback = rect => DrawRuleListHeader(rect),
                elementHeight = EditorGUIUtility.singleLineHeight + 8f,
                footerHeight = 4f,
                onSelectCallback = list =>
                {
                    selectedRuleIndex = list.index;
                    RefreshEntryPreviewOnly();
                },
                onReorderCallback = list =>
                {
                    selectedRuleIndex = list.index;
                    RefreshEntryPreviewOnly();
                },
                drawElementCallback = (rect, index, active, focused) =>
                {
                    DrawRuleListRow(rect, index, active, focused);
                },
                onAddCallback = list =>
                {
                    list.serializedProperty.arraySize++;
                    var element = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
                    element.FindPropertyRelative("enabled").boolValue = true;
                    element.FindPropertyRelative("className").stringValue = "CustomRule" + list.serializedProperty.arraySize;
                    element.FindPropertyRelative("rulePath").stringValue = string.Empty;
                    element.FindPropertyRelative("suffix").stringValue = string.Empty;
                    element.FindPropertyRelative("sourceGroup").stringValue = string.Empty;
                    element.FindPropertyRelative("note").stringValue = string.Empty;
                    element.FindPropertyRelative("lockCustomNaming").boolValue = false;
                    list.index = list.serializedProperty.arraySize - 1;
                    selectedRuleIndex = list.index;
                    OnRulesChanged(rebuildIndex: true, refreshEntries: true);
                },
                onRemoveCallback = list =>
                {
                    if (list.index < 0 || list.index >= list.serializedProperty.arraySize)
                        return;
                    list.serializedProperty.DeleteArrayElementAtIndex(list.index);
                    selectedRuleIndex = Mathf.Clamp(list.index - 1, -1, list.serializedProperty.arraySize - 1);
                    OnRulesChanged(rebuildIndex: true, refreshEntries: true);
                },
                onChangedCallback = _ => OnRulesChanged(rebuildIndex: true, refreshEntries: true)
            };

            if (selectedRuleIndex < 0 && rulesProperty.arraySize > 0)
                selectedRuleIndex = 0;
            if (selectedRuleIndex >= 0 && selectedRuleIndex < rulesProperty.arraySize)
                rulesList.index = selectedRuleIndex;
        }

        private void DrawRuleListRow(Rect rect, int index, bool active, bool focused)
        {
            if (Event.current.type == EventType.Repaint)
            {
                var bg = (active || focused)
                    ? ResourcesConditionerEditorStyles.SelectedRowColor
                    : index % 2 == 1 ? ResourcesConditionerEditorStyles.ZebraColor : Color.clear;
                if (bg.a > 0.001f)
                    EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, rect.height), bg);
            }

            rect.y += 4f;
            rect.height = EditorGUIUtility.singleLineHeight;
            CalcRuleListColumns(rect, isHeader: false, out var colEnabled, out var colClass, out var colPath, out var colSuffix, out var colCount);

            var element = rulesProperty.GetArrayElementAtIndex(index);
            var enabledProp = element.FindPropertyRelative("enabled");
            var classProp = element.FindPropertyRelative("className");
            var pathProp = element.FindPropertyRelative("rulePath");
            var suffixProp = element.FindPropertyRelative("suffix");

            enabledProp.boolValue = EditorGUI.Toggle(GetCenteredToggleRect(colEnabled), enabledProp.boolValue);
            EditorGUI.LabelField(colClass, classProp.stringValue, EditorStyles.label);
            pathProp.stringValue = EditorGUI.TextField(colPath, pathProp.stringValue);
            suffixProp.stringValue = EditorGUI.TextField(colSuffix, suffixProp.stringValue);

            var rule = ElementToRule(element);
            var countLabel = matchIndex.IsBuilt && rule != null ? matchIndex.GetCount(rule).ToString() : "-";
            EditorGUI.LabelField(colCount, countLabel, ResourcesConditionerEditorStyles.ListCount);
        }

        private void LoadOrCreateRuleSet()
        {
            ruleSet = AssetDatabase.LoadAssetAtPath<ResourcesConditionerRuleSet>(ResourcesConditionerRuleSet.DefaultAssetPath);
            if (ruleSet != null)
            {
                ruleSet.MigrateGenerationPathsIfNeeded();
                return;
            }

            EnsureAssetFolderExists(ResourcesConditionerRuleSet.DefaultAssetPath);
            ruleSet = CreateInstance<ResourcesConditionerRuleSet>();
            AssetDatabase.CreateAsset(ruleSet, ResourcesConditionerRuleSet.DefaultAssetPath);

            if (AddressableAssetSettingsDefaultObject.SettingsExists)
            {
                var scanned = ResConditionerScanner.ScanRules(AddressableAssetSettingsDefaultObject.Settings);
                ruleSet.SetRules(scanned);
            }

            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(ruleSet);
        }

        private static void EnsureAssetFolderExists(string assetPath)
        {
            var folder = System.IO.Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(folder) || AssetDatabase.IsValidFolder(folder))
                return;

            var parts = folder.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        private void ScanFromAddressables()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("未找到 AddressableAssetSettings。");
                return;
            }

            var scanned = ResConditionerScanner.ScanRules(settings);
            if (scanned.Count == 0)
            {
                Debug.LogWarning("未扫描到可导入的规则，请确认资源已加入 Addressables。");
                return;
            }

            ResConditionerScanner.MergeScannedRules(ruleSet, scanned, replaceExistingOnScan);
            EditorUtility.SetDirty(ruleSet);
            AssetDatabase.SaveAssets();

            BuildRulesList();
            ScheduleMatchIndexRebuild(immediate: true);
            Debug.Log($"扫描完成，导入/更新 {scanned.Count} 条规则。");
        }

        private void GenerateCode()
        {
            ruleSetSerialized?.ApplyModifiedProperties();
            var result = ResConditionerCodeGenerator.Generate(ruleSet);
            foreach (var warning in result.Warnings)
                Debug.LogWarning(warning);

            if (result.Success)
                Debug.Log(result.Message);
            else
                Debug.LogError(result.Message);
        }

        private void OnRulesChanged(bool rebuildIndex, bool refreshEntries)
        {
            ruleSetSerialized?.ApplyModifiedProperties();
            validationDirty = true;
            cachedEnabledCount = -1;
            cachedTotalCount = -1;
            cachedSelectedMatchCount = -1;

            if (rebuildIndex)
                ScheduleMatchIndexRebuild(immediate: false);
            else if (refreshEntries)
                RefreshEntryPreviewOnly();
        }

        private void ScheduleMatchIndexRebuild(bool immediate)
        {
            if (immediate)
            {
                RebuildMatchIndex();
                return;
            }

            if (indexRebuildScheduled)
                return;

            indexRebuildScheduled = true;
            EditorApplication.delayCall -= DelayedMatchIndexRebuild;
            EditorApplication.delayCall += DelayedMatchIndexRebuild;
        }

        private void DelayedMatchIndexRebuild()
        {
            indexRebuildScheduled = false;
            if (this == null)
                return;
            RebuildMatchIndex();
        }

        private void RebuildMatchIndex()
        {
            cachedTotalCount = -1;
            cachedEnabledCount = -1;

            if (!AddressableAssetSettingsDefaultObject.SettingsExists)
            {
                matchIndex.Rebuild(null);
                cachedSelectedMatchCount = 0;
                entryTreeView?.SetEntries(new List<ResConditionerScanner.ScannedEntry>());
                return;
            }

            matchIndex.Rebuild(AddressableAssetSettingsDefaultObject.Settings);
            RefreshValidationIfNeeded();
            RefreshEntryPreviewOnly();
            Repaint();
        }

        private void RefreshValidationIfNeeded()
        {
            if (!validationDirty || ruleSet == null)
                return;

            validationMessages.Clear();
            validationMessages.AddRange(
                ResConditionerCodeGenerator.ValidateRules(ruleSet.Rules.Where(r => r != null && r.enabled).ToList()));
            validationDirty = false;
        }

        private void RefreshEntryPreviewOnly()
        {
            var rule = GetSelectedRule();
            if (rule == null || !matchIndex.IsBuilt)
            {
                cachedSelectedMatchCount = 0;
                entryTreeView?.SetEntries(new List<ResConditionerScanner.ScannedEntry>());
                return;
            }

            cachedSelectedMatchCount = matchIndex.GetCount(rule);
            entryTreeView.SetEntries(matchIndex.GetEntries(rule));
        }

        private void UpdateRuleCountCache()
        {
            if (ruleSet == null)
            {
                cachedEnabledCount = 0;
                cachedTotalCount = 0;
                return;
            }

            if (cachedTotalCount >= 0)
                return;

            cachedTotalCount = ruleSet.Rules.Count;
            cachedEnabledCount = 0;
            for (var i = 0; i < ruleSet.Rules.Count; i++)
            {
                if (ruleSet.Rules[i] != null && ruleSet.Rules[i].enabled)
                    cachedEnabledCount++;
            }
        }

        private int GetSelectedMatchCount()
        {
            if (cachedSelectedMatchCount >= 0)
                return cachedSelectedMatchCount;

            var rule = GetSelectedRule();
            return rule != null && matchIndex.IsBuilt ? matchIndex.GetCount(rule) : 0;
        }

        private ResourcesConditionerRuleData GetSelectedRule()
        {
            if (selectedRuleIndex < 0 || selectedRuleIndex >= ruleSet.Rules.Count)
                return null;
            return ruleSet.Rules[selectedRuleIndex];
        }

        private static ResourcesConditionerRuleData ElementToRule(SerializedProperty element)
        {
            return new ResourcesConditionerRuleData
            {
                enabled = element.FindPropertyRelative("enabled").boolValue,
                className = element.FindPropertyRelative("className").stringValue,
                rulePath = element.FindPropertyRelative("rulePath").stringValue,
                suffix = element.FindPropertyRelative("suffix").stringValue,
                sourceGroup = element.FindPropertyRelative("sourceGroup").stringValue,
                note = element.FindPropertyRelative("note").stringValue,
                lockCustomNaming = element.FindPropertyRelative("lockCustomNaming").boolValue
            };
        }

        private static void CalcRuleListColumns(
            Rect row,
            bool isHeader,
            out Rect colEnabled,
            out Rect colClass,
            out Rect colPath,
            out Rect colSuffix,
            out Rect colCount)
        {
            var y = row.y;
            var h = row.height;
            var left = row.x + (isHeader ? RuleListHeaderDragIndent : 0f);
            var right = row.xMax;

            colCount = new Rect(right - RuleColCount, y, RuleColCount, h);
            colSuffix = new Rect(colCount.x - RuleColPad - RuleColSuffix, y, RuleColSuffix, h);
            right = colSuffix.x - RuleColPad;

            colEnabled = new Rect(left, y, RuleColEnabled, h);
            left = colEnabled.xMax + RuleColEnabledToClassGap;

            var midWidth = right - left;
            var classWidth = Mathf.Max(100f, Mathf.Floor(midWidth * RuleColClassRatio));
            colClass = new Rect(left, y, classWidth, h);
            colPath = new Rect(colClass.xMax + RuleColPad, y, right - colClass.xMax - RuleColPad, h);
        }

        private static Rect GetCenteredToggleRect(Rect column)
        {
            var size = RuleColToggleSize;
            return new Rect(
                column.x + (column.width - size) * 0.5f,
                column.y + (column.height - size) * 0.5f,
                size,
                size);
        }

        private static void DrawRuleListHeader(Rect rect)
        {
            if (Event.current.type == EventType.Repaint)
            {
                var headerBg = EditorGUIUtility.isProSkin
                    ? new Color(0f, 0f, 0f, 0.2f)
                    : new Color(0f, 0f, 0f, 0.06f);
                EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, rect.height), headerBg);
            }

            rect.y += 2f;
            rect.height = EditorGUIUtility.singleLineHeight;
            CalcRuleListColumns(rect, isHeader: true, out var colEnabled, out var colClass, out var colPath, out var colSuffix, out var colCount);

            var style = ResourcesConditionerEditorStyles.ListHeader;
            EditorGUI.LabelField(colEnabled, "启用", ResourcesConditionerEditorStyles.ListHeaderCentered);
            EditorGUI.LabelField(colClass, "类名", style);
            EditorGUI.LabelField(colPath, "RulePath", style);
            EditorGUI.LabelField(colSuffix, "Suffix", style);

            var countStyle = new GUIStyle(style) { alignment = TextAnchor.MiddleRight };
            EditorGUI.LabelField(colCount, "匹配数", countStyle);
        }
    }
}
