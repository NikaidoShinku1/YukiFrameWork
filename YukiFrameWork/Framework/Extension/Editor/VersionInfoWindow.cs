///=====================================================
/// - FileName:      VersionInfoWindow.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   框架版本更新日志编辑与预览
/// - Creation Time: 2025/4/24 14:40:53
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork.Extension;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

#if UNITY_EDITOR
using UnityEditor;
namespace YukiFrameWork
{
    public class VersionInfoWindow : EditorWindow
    {
        const string UpdateInfoRelativePath = "/Framework/Extension/UpdateInfo.md";
        const string PackageJsonRelativePath = "/package.json";

        static readonly Regex VersionEntryRegex = new Regex(
            @"^V([\d.]+)\s+(.*)$",
            RegexOptions.Compiled);

        enum ViewMode
        {
            Edit,
            Preview
        }

        [Serializable]
        class VersionLogEntry
        {
            public string version = "V1.0.0";
            public string description = string.Empty;
        }

        [Serializable]
        class VersionLogData
        {
            public string header = "#### 框架更新日志、";
            public List<VersionLogEntry> entries = new List<VersionLogEntry>();
            public string footer = string.Empty;
        }

        ViewMode viewMode = ViewMode.Preview;
        VersionLogData logData = new VersionLogData();
        string packageVersion = string.Empty;

        Vector2 scrollPosition;
        Vector2 previewScrollPosition;
        bool isDirty;
        int selectedEntryIndex = -1;
        int pendingDeleteIndex = -1;
        bool pendingReload;

        string newVersionInput = string.Empty;
        string newDescriptionInput = string.Empty;

        GUIStyle titleStyle;
        GUIStyle previewHeaderStyle;
        GUIStyle previewVersionStyle;
        GUIStyle previewDescriptionStyle;
        GUIStyle entryBoxStyle;
        GUIStyle actionButtonStyle;

        float ContentWidth => Mathf.Max(240f, position.width - 20f);

        string UpdateInfoPath => ImportSettingWindow.packagePath + UpdateInfoRelativePath;
        string PackageJsonPath => ImportSettingWindow.packagePath + PackageJsonRelativePath;

        [MenuItem("YukiFrameWork/版本更新模块", false, 999)]
        internal static void Open()
        {
            var window = GetWindow<VersionInfoWindow>();
            window.titleContent = new GUIContent("版本更新模块");
            window.minSize = new Vector2(520, 420);
            window.Show();
        }

        void OnEnable()
        {
            InitStyles();
            ReloadFromDisk();
        }

        void OnGUI()
        {
            EnsureData();
            InitStyles();
            ProcessPendingActions();

            DrawToolbar();
            EditorGUILayout.Space(6);

            switch (viewMode)
            {
                case ViewMode.Edit:
                    DrawEditView();
                    break;
                case ViewMode.Preview:
                    DrawPreviewView();
                    break;
            }

            EditorGUILayout.Space(8);
            DrawFooterActions();
        }

        void EnsureData()
        {
            logData ??= new VersionLogData();
            logData.entries ??= new List<VersionLogEntry>();
        }

        void ProcessPendingActions()
        {
            if (pendingReload)
            {
                pendingReload = false;
                ReloadFromDisk();
            }

            if (pendingDeleteIndex >= 0)
            {
                int index = pendingDeleteIndex;
                pendingDeleteIndex = -1;
                if (index >= 0 && index < logData.entries.Count)
                {
                    logData.entries.RemoveAt(index);
                    selectedEntryIndex = Mathf.Clamp(selectedEntryIndex, -1, logData.entries.Count - 1);
                    MarkDirty();
                }
            }
        }

        void InitStyles()
        {
            if (titleStyle != null && previewHeaderStyle != null && previewVersionStyle != null
                && previewDescriptionStyle != null && entryBoxStyle != null && actionButtonStyle != null)
                return;

            titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                clipping = TextClipping.Clip,
                wordWrap = false
            };
            ApplyTextColor(titleStyle, new Color(0f, 0.85f, 0.95f));

            previewHeaderStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 14,
                wordWrap = true
            };
            ApplyTextColor(previewHeaderStyle, new Color(0.75f, 0.75f, 0.75f));

            previewVersionStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 13,
                wordWrap = false
            };
            ApplyTextColor(previewVersionStyle, new Color(0.45f, 0.85f, 1f));

            previewDescriptionStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                wordWrap = true,
                richText = false
            };
            ApplyTextColor(previewDescriptionStyle, EditorStyles.label.normal.textColor);

            entryBoxStyle = new GUIStyle(EditorStyles.helpBox)
            {
                padding = new RectOffset(8, 8, 8, 8)
            };

            actionButtonStyle = new GUIStyle(EditorStyles.miniButton)
            {
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Clip,
                wordWrap = false,
                stretchWidth = true
            };
        }

        static void ApplyTextColor(GUIStyle style, Color color)
        {
            style.normal.textColor = color;
            style.hover.textColor = color;
            style.active.textColor = color;
            style.focused.textColor = color;
        }

        bool DrawActionButton(string label, bool enabled, Action onClick, params GUILayoutOption[] options)
        {
            var content = new GUIContent(label);
            var prevContentColor = GUI.contentColor;
            if (!enabled)
                GUI.contentColor = EditorGUIUtility.isProSkin
                    ? new Color(0.72f, 0.72f, 0.72f)
                    : new Color(0.35f, 0.35f, 0.35f);

            var layoutOptions = new List<GUILayoutOption>
            {
                GUILayout.ExpandWidth(true),
                GUILayout.MaxWidth(ContentWidth),
                GUILayout.Height(24)
            };
            layoutOptions.AddRange(options);

            bool clicked = GUILayout.Button(content, actionButtonStyle ?? EditorStyles.miniButton, layoutOptions.ToArray());

            GUI.contentColor = prevContentColor;

            if (!clicked)
                return false;

            if (enabled)
            {
                onClick?.Invoke();
                return true;
            }

            ShowNotification(new GUIContent("请先填写必填项"));
            return false;
        }

        void BeginContentArea()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(ContentWidth));
        }

        void EndContentArea()
        {
            EditorGUILayout.EndVertical();
        }

        void DrawToolbar()
        {
            BeginContentArea();

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            var newMode = (ViewMode)GUILayout.Toolbar(
                (int)viewMode,
                new[] { "编辑", "预览" },
                EditorStyles.toolbarButton,
                GUILayout.Height(20),
                GUILayout.MinWidth(96));

            viewMode = newMode;

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(new GUIContent("保存"), EditorStyles.toolbarButton, GUILayout.Width(52), GUILayout.Height(20)))
            {
                if (isDirty)
                    SaveToDisk();
            }

            if (GUILayout.Button(new GUIContent("重新加载"), EditorStyles.toolbarButton, GUILayout.Width(64), GUILayout.Height(20)))
            {
                EditorApplication.delayCall += () =>
                {
                    if (!isDirty || EditorUtility.DisplayDialog(
                        "放弃更改",
                        "当前有未保存的修改，重新加载将丢失这些更改。是否继续？",
                        "继续",
                        "取消"))
                    {
                        pendingReload = true;
                        Repaint();
                    }
                };
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (isDirty)
                EditorGUILayout.LabelField("● 有未保存的更改", EditorStyles.miniLabel);
            else
                GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField($"Package: {packageVersion}", EditorStyles.miniLabel, GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            EndContentArea();
        }

        void DrawEditView()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(
                scrollPosition,
                false,
                true,
                GUILayout.ExpandHeight(true));

            BeginContentArea();

            EditorGUILayout.LabelField("日志标题", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            logData.header = EditorGUILayout.TextField(logData.header, GUILayout.MaxWidth(ContentWidth));
            if (EditorGUI.EndChangeCheck())
                MarkDirty();

            EditorGUILayout.Space(10);
            DrawQuickPublishSection();

            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("版本条目", EditorStyles.boldLabel, GUILayout.ExpandWidth(true));
            if (GUILayout.Button("+ 添加条目", EditorStyles.miniButton, GUILayout.Width(88)))
            {
                logData.entries.Insert(0, CreateDefaultEntry());
                selectedEntryIndex = 0;
                MarkDirty();
            }
            EditorGUILayout.EndHorizontal();

            if (logData.entries.Count == 0)
            {
                EditorGUILayout.HelpBox("暂无版本条目，可通过上方「快速发布」或「添加条目」创建。", MessageType.Info);
            }

            for (int i = 0; i < logData.entries.Count; i++)
            {
                DrawEntryEditor(i);
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("页脚说明（可选）", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            logData.footer = EditorGUILayout.TextArea(logData.footer, GUILayout.MinHeight(40), GUILayout.MaxWidth(ContentWidth));
            if (EditorGUI.EndChangeCheck())
                MarkDirty();

            EditorGUILayout.Space(10);
            DrawPackageSyncSection();

            EndContentArea();
            EditorGUILayout.EndScrollView();
        }

        GUIStyle EntryBoxStyle => entryBoxStyle ?? EditorStyles.helpBox;

        void DrawQuickPublishSection()
        {
            EditorGUILayout.BeginVertical(EntryBoxStyle, GUILayout.MaxWidth(ContentWidth));
            EditorGUILayout.LabelField("快速发布新版本", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("版本号", GUILayout.Width(48));
            newVersionInput = EditorGUILayout.TextField(newVersionInput, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Patch +1", EditorStyles.miniButton, GUILayout.ExpandWidth(true), GUILayout.Height(20)))
                newVersionInput = BumpVersion(GetBumpBaseVersion(), 0);
            if (GUILayout.Button("Minor +1", EditorStyles.miniButton, GUILayout.ExpandWidth(true), GUILayout.Height(20)))
                newVersionInput = BumpVersion(GetBumpBaseVersion(), 1);
            if (GUILayout.Button("Major +1", EditorStyles.miniButton, GUILayout.ExpandWidth(true), GUILayout.Height(20)))
                newVersionInput = BumpVersion(GetBumpBaseVersion(), 2);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("更新说明");
            newDescriptionInput = EditorGUILayout.TextArea(
                newDescriptionInput,
                GUILayout.MinHeight(48),
                GUILayout.MaxWidth(ContentWidth - 16f));

            bool canPublish = !string.IsNullOrWhiteSpace(newVersionInput)
                && !string.IsNullOrWhiteSpace(newDescriptionInput);
            bool canAddOnly = !string.IsNullOrWhiteSpace(newVersionInput);

            DrawActionButton(
                "发布并保存",
                canPublish,
                () => PublishNewVersion(NormalizeVersionLabel(newVersionInput), newDescriptionInput.Trim()));

            DrawActionButton(
                "仅添加到列表",
                canAddOnly,
                () =>
                {
                    logData.entries.Insert(0, new VersionLogEntry
                    {
                        version = NormalizeVersionLabel(newVersionInput),
                        description = newDescriptionInput.Trim()
                    });
                    selectedEntryIndex = 0;
                    SaveToDisk();
                });
            EditorGUILayout.EndVertical();
        }

        void DrawEntryEditor(int index)
        {
            var entry = logData.entries[index];
            bool selected = selectedEntryIndex == index;

            EditorGUILayout.BeginVertical(selected ? "SelectionRect" : EntryBoxStyle, GUILayout.MaxWidth(ContentWidth));

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"#{index + 1}", EditorStyles.miniLabel, GUILayout.Width(24));

            if (GUILayout.Button("↑", EditorStyles.miniButtonLeft, GUILayout.Width(22)) && index > 0)
            {
                SwapEntries(index, index - 1);
                selectedEntryIndex = index - 1;
            }
            if (GUILayout.Button("↓", EditorStyles.miniButtonMid, GUILayout.Width(22)) && index < logData.entries.Count - 1)
            {
                SwapEntries(index, index + 1);
                selectedEntryIndex = index + 1;
            }
            if (GUILayout.Button("删", EditorStyles.miniButtonRight, GUILayout.Width(28)))
            {
                int deleteIndex = index;
                string deleteVersion = entry.version;
                EditorApplication.delayCall += () =>
                {
                    if (EditorUtility.DisplayDialog("删除条目", $"确定删除 {deleteVersion} 吗？", "删除", "取消"))
                        pendingDeleteIndex = deleteIndex;
                    Repaint();
                };
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button(selected ? "收起" : "编辑", EditorStyles.miniButton, GUILayout.Width(44)))
                selectedEntryIndex = selected ? -1 : index;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("版本", GUILayout.Width(36));
            EditorGUI.BeginChangeCheck();
            entry.version = EditorGUILayout.TextField(entry.version, GUILayout.ExpandWidth(true));
            if (EditorGUI.EndChangeCheck())
                MarkDirty();
            EditorGUILayout.EndHorizontal();

            if (selected)
            {
                EditorGUILayout.LabelField("说明");
                EditorGUI.BeginChangeCheck();
                entry.description = EditorGUILayout.TextArea(
                    entry.description,
                    GUILayout.MinHeight(56),
                    GUILayout.MaxWidth(ContentWidth - 16f));
                if (EditorGUI.EndChangeCheck())
                    MarkDirty();
            }
            else
            {
                EditorGUILayout.LabelField(
                    string.IsNullOrEmpty(entry.description) ? "(无说明)" : entry.description,
                    EditorStyles.wordWrappedMiniLabel);
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4);
        }

        void DrawPackageSyncSection()
        {
            EditorGUILayout.BeginVertical(EntryBoxStyle, GUILayout.MaxWidth(ContentWidth));
            EditorGUILayout.LabelField("Package.json 同步", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"当前 package.json 版本: {packageVersion}", EditorStyles.miniLabel);

            if (logData.entries.Count > 0)
            {
                string latestVersion = StripVersionPrefix(logData.entries[0].version);
                if (!string.IsNullOrWhiteSpace(latestVersion) && latestVersion != packageVersion)
                {
                    EditorGUILayout.HelpBox(
                        $"日志最新版本 V{latestVersion} 与 package.json ({packageVersion}) 不一致，保存时将自动同步。",
                        MessageType.Warning);
                }
            }

            DrawActionButton(
                "同步到最新条目版本",
                logData.entries.Count > 0,
                () => SyncPackageVersion(StripVersionPrefix(logData.entries[0].version)),
                GUILayout.Height(22));

            DrawActionButton(
                "从 package.json 读取",
                true,
                () =>
                {
                    LoadPackageVersion();
                    newVersionInput = "V" + packageVersion;
                },
                GUILayout.Height(22));
            EditorGUILayout.EndVertical();
        }

        void DrawPreviewView()
        {
            BeginContentArea();
            EditorGUILayout.LabelField("版本更新日志", titleStyle ?? EditorStyles.boldLabel);
            EndContentArea();
            EditorGUILayout.Space(8);

            previewScrollPosition = EditorGUILayout.BeginScrollView(
                previewScrollPosition,
                false,
                true,
                GUILayout.ExpandHeight(true));

            BeginContentArea();

            var headerStyle = previewHeaderStyle ?? EditorStyles.boldLabel;
            var versionStyle = previewVersionStyle ?? EditorStyles.boldLabel;
            var descriptionStyle = previewDescriptionStyle ?? EditorStyles.wordWrappedLabel;

            if (!string.IsNullOrWhiteSpace(logData.header))
            {
                EditorGUILayout.LabelField(logData.header, headerStyle);
                EditorGUILayout.Space(8);
            }

            if (logData.entries.Count == 0)
            {
                EditorGUILayout.HelpBox("暂无版本记录。", MessageType.Info);
            }
            else
            {
                foreach (var entry in logData.entries)
                {
                    if (entry == null)
                        continue;

                    EditorGUILayout.LabelField(entry.version ?? string.Empty, versionStyle);
                    if (!string.IsNullOrWhiteSpace(entry.description))
                        EditorGUILayout.LabelField(entry.description, descriptionStyle);
                    EditorGUILayout.Space(10);
                }
            }

            if (!string.IsNullOrWhiteSpace(logData.footer))
            {
                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField(logData.footer, descriptionStyle);
            }

            EndContentArea();
            EditorGUILayout.EndScrollView();
        }

        void DrawFooterActions()
        {
            BeginContentArea();

            EditorGUILayout.HelpBox(
                "每次框架的更新，如涉及到模块的更新而非本体，则应该打开 ImportSettingWindow 窗口，进行对模块的重新导入操作。",
                MessageType.Info);

            DrawActionButton(
                "快捷打开 ImportSettingWindow",
                true,
                ImportSettingWindow.Open);

            DrawActionButton(
                "保存到 UpdateInfo.md",
                isDirty,
                SaveToDisk);

            EndContentArea();
        }

        void ReloadFromDisk()
        {
            logData = ParseUpdateInfo(ReadUpdateInfoText());
            EnsureData();
            LoadPackageVersion();
            newVersionInput = logData.entries.Count > 0
                ? logData.entries[0].version
                : "V" + packageVersion;
            isDirty = false;
            hasUnsavedChanges = false;
            Repaint();
        }

        public override void SaveChanges()
        {
            SaveToDisk();
            base.SaveChanges();
        }

        public override void DiscardChanges()
        {
            ReloadFromDisk();
            base.DiscardChanges();
        }

        void SaveToDisk()
        {
            try
            {
                string content = SerializeUpdateInfo(logData);
                File.WriteAllText(UpdateInfoPath, content, Encoding.UTF8);

                bool packageSynced = TrySyncPackageVersionFromLatestEntry(false);

                AssetDatabase.Refresh();
                isDirty = false;
                hasUnsavedChanges = false;
                ShowNotification(new GUIContent(packageSynced
                    ? "已保存 UpdateInfo.md，并同步 package.json"
                    : "已保存 UpdateInfo.md"));
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                EditorApplication.delayCall += () =>
                    EditorUtility.DisplayDialog("保存失败", message, "确定");
            }
        }

        void PublishNewVersion(string versionLabel, string description)
        {
            logData.entries.Insert(0, new VersionLogEntry
            {
                version = versionLabel,
                description = description
            });
            selectedEntryIndex = 0;
            newDescriptionInput = string.Empty;
            SaveToDisk();
        }

        bool TrySyncPackageVersionFromLatestEntry(bool showNotification)
        {
            if (logData.entries.Count == 0)
                return false;

            string latestVersion = StripVersionPrefix(logData.entries[0].version);
            if (string.IsNullOrWhiteSpace(latestVersion) || latestVersion == packageVersion)
                return false;

            SyncPackageVersion(latestVersion, showNotification);
            return true;
        }

        void SyncPackageVersion(string semver, bool showNotification = true)
        {
            if (string.IsNullOrWhiteSpace(semver))
                return;

            try
            {
                string jsonText = File.ReadAllText(PackageJsonPath, Encoding.UTF8);
                var root = JObject.Parse(jsonText);
                root["version"] = semver.Trim();
                File.WriteAllText(PackageJsonPath, root.ToString(Newtonsoft.Json.Formatting.Indented), Encoding.UTF8);

                packageVersion = semver.Trim();
                SyncFrameworkConfigVersion(packageVersion);
                AssetDatabase.Refresh();
                if (showNotification)
                    ShowNotification(new GUIContent($"已同步 package.json -> {packageVersion}"));
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                EditorApplication.delayCall += () =>
                    EditorUtility.DisplayDialog("同步 package.json 失败", message, "确定");
            }
        }

        void SyncFrameworkConfigVersion(string version)
        {
            var config = Resources.Load<FrameworkConfigInfo>(nameof(FrameworkConfigInfo));
            if (config == null)
                return;

            if (config.version == version)
                return;

            config.version = version;
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
        }

        void LoadPackageVersion()
        {
            try
            {
                string jsonText = File.ReadAllText(PackageJsonPath, Encoding.UTF8);
                var root = JObject.Parse(jsonText);
                packageVersion = root.Value<string>("version") ?? string.Empty;
            }
            catch
            {
                packageVersion = string.Empty;
            }
        }

        string ReadUpdateInfoText()
        {
            if (!File.Exists(UpdateInfoPath))
                return string.Empty;
            return File.ReadAllText(UpdateInfoPath, Encoding.UTF8);
        }

        static VersionLogData ParseUpdateInfo(string rawText)
        {
            var data = new VersionLogData();
            if (string.IsNullOrWhiteSpace(rawText))
                return data;

            var lines = rawText.Replace("\r\n", "\n").Split('\n');
            var bodyLines = new List<string>();
            bool headerAssigned = false;

            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed))
                    continue;

                var match = VersionEntryRegex.Match(trimmed);
                if (match.Success)
                {
                    data.entries.Add(new VersionLogEntry
                    {
                        version = "V" + match.Groups[1].Value,
                        description = match.Groups[2].Value.Trim()
                    });
                    continue;
                }

                if (!headerAssigned)
                {
                    data.header = trimmed;
                    headerAssigned = true;
                }
                else
                {
                    bodyLines.Add(trimmed);
                }
            }

            if (bodyLines.Count > 0)
                data.footer = string.Join("\n", bodyLines);

            return data;
        }

        static string SerializeUpdateInfo(VersionLogData data)
        {
            var builder = new StringBuilder();
            builder.AppendLine(data.header ?? string.Empty);
            builder.AppendLine();

            for (int i = 0; i < data.entries.Count; i++)
            {
                var entry = data.entries[i];
                builder.Append(entry.version);
                if (!string.IsNullOrWhiteSpace(entry.description))
                {
                    builder.Append(' ');
                    builder.Append(entry.description.Trim());
                }
                builder.AppendLine();
                if (i < data.entries.Count - 1)
                    builder.AppendLine();
            }

            if (!string.IsNullOrWhiteSpace(data.footer))
            {
                if (data.entries.Count > 0)
                    builder.AppendLine();
                builder.Append(data.footer.Trim());
                builder.AppendLine();
            }

            return builder.ToString();
        }

        static VersionLogEntry CreateDefaultEntry()
        {
            return new VersionLogEntry
            {
                version = "V1.0.0",
                description = string.Empty
            };
        }

        static string NormalizeVersionLabel(string version)
        {
            version = version.Trim();
            if (string.IsNullOrEmpty(version))
                return "V1.0.0";
            return version.StartsWith("V", StringComparison.OrdinalIgnoreCase)
                ? "V" + version.Substring(1)
                : "V" + version;
        }

        static string StripVersionPrefix(string version)
        {
            version = version.Trim();
            if (version.StartsWith("V", StringComparison.OrdinalIgnoreCase))
                return version.Substring(1);
            return version;
        }

        string GetBumpBaseVersion()
        {
            if (!string.IsNullOrWhiteSpace(newVersionInput))
                return newVersionInput;
            if (logData.entries.Count > 0)
                return logData.entries[0].version;
            if (!string.IsNullOrWhiteSpace(packageVersion))
                return "V" + packageVersion;
            return "V1.0.0";
        }

        static string BumpVersion(string version, int segmentIndex)
        {
            version = StripVersionPrefix(version);
            var parts = version.Split('.');
            var numbers = new int[3];
            for (int i = 0; i < numbers.Length; i++)
            {
                if (i < parts.Length && int.TryParse(parts[i], out int value))
                    numbers[i] = value;
            }

            numbers[segmentIndex]++;
            for (int i = segmentIndex + 1; i < numbers.Length; i++)
                numbers[i] = 0;

            return $"V{numbers[0]}.{numbers[1]}.{numbers[2]}";
        }

        void SwapEntries(int a, int b)
        {
            (logData.entries[a], logData.entries[b]) = (logData.entries[b], logData.entries[a]);
            MarkDirty();
        }

        void MarkDirty()
        {
            isDirty = true;
            hasUnsavedChanges = true;
            Repaint();
        }

    }
}
#endif
