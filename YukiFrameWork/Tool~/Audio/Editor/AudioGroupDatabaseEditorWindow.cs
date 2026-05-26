///=====================================================
/// - FileName:      AudioGroupDatabaseEditorWindow.cs
/// - NameSpace:     YukiFrameWork.Audio
/// - Description:   音频分组可视化编辑器
/// - Creation Time: 2025/5/26
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YukiFrameWork;
using YukiFrameWork.Extension;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;

namespace YukiFrameWork.Audio
{
    public class AudioGroupDatabaseEditorWindow : EditorWindow
    {
        public static AudioGroupDatabaseEditorWindow Instance { get; private set; }

        private const string SELECT_GUID_KEY = "AUDIOGROUP_DATABASE_EDITOR_SELECT_KEY";
        private const string GROUP_PANEL_WIDTH_KEY = "AUDIOGROUP_EDITOR_GROUP_PANEL_WIDTH";
        private const float ToolbarHeight = 32f;
        private const float DefaultGroupPanelWidth = 220f;
        private const float MinGroupPanelWidth = 168f;
        private const float MaxGroupPanelWidth = 520f;
        private const float GroupPanelSplitterWidth = 5f;
        private const float MinGroupCardWidth = 128f;
        private const float EntryCompactHeight = 26f;
        private const float EntryDetailPadding = 8f;
        private const float EntrySectionTitleHeight = 18f;
        private const float FieldLabelWidth = 56f;
        private const float EnumFieldWidth = 148f;
        private const float RowHeight = 20f;
        private const float GroupCardHeight = 54f;
        private const float GroupCardGap = 6f;
        private const float GroupCardPadding = 8f;
        private const float GroupAccentWidth = 4f;
        private const float GroupInnerPad = 8f;
        private const float SplitterHeight = 5f;
        private const float TableHeaderHeight = 20f;
        private const float TableSearchHeight = 20f;
        private const float TableTitleHeight = 22f;
        private const float CodeGenFoldoutHeight = 22f;
        private const float CodeGenExpandedHeight = 132f;
        private const float CodeGenBottomPad = 6f;
        private const float EntryPreviewColWidth = 22f;
        private const float EntryExpandColWidth = 20f;
        private const float EntryIconColWidth = 18f;
        private const float EntryDeleteColWidth = 22f;
        private const float EntryDragHandleWidth = 10f;
        private const float EntryControlSize = 18f;
        private const float EntryColInnerGap = 2f;
        private const float EntrySummaryMinWidth = 64f;
        private const float EntrySummaryMaxWidth = 220f;
        private const float EntryRowPad = 2f;
        private const float EntryColGap = 4f;

        private float entrySummaryColWidth = EntrySummaryMinWidth;
        private float groupPanelWidth = DefaultGroupPanelWidth;
        private string groupEntrySearch = string.Empty;
        private string lastEntrySearchGroupId;

        private readonly struct EntryRowLayout
        {
            public readonly Rect Preview;
            public readonly Rect Expand;
            public readonly Rect Icon;
            public readonly Rect Name;
            public readonly Rect Summary;
            public readonly Rect Delete;

            public EntryRowLayout(Rect preview, Rect expand, Rect icon, Rect name, Rect summary, Rect delete)
            {
                Preview = preview;
                Expand = expand;
                Icon = icon;
                Name = name;
                Summary = summary;
                Delete = delete;
            }
        }

        private static readonly Color PanelBg = new(0.18f, 0.18f, 0.18f, 1f);
        private static readonly Color HeaderBg = new(0.24f, 0.24f, 0.24f, 1f);
        private static readonly Color SelectedBg = new(0.15f, 0.28f, 0.48f, 1f);
        private static readonly Color GroupSelectedTint = new(0.20f, 0.23f, 0.27f, 1f);
        private static readonly Color CardBg = new(0.21f, 0.21f, 0.21f, 1f);
        private static readonly Color CardHoverBg = new(0.25f, 0.25f, 0.25f, 1f);
        private static readonly Color RowAltBg = new(0.21f, 0.21f, 0.21f, 1f);
        private static readonly Color AccentGreen = new(0.35f, 0.75f, 0.45f, 1f);
        private static readonly Color LineColor = new(0.1f, 0.1f, 0.1f, 1f);
        private static readonly Color DividerColor = new(0.08f, 0.08f, 0.08f, 1f);

        private static readonly Color MusicColor = new(0.45f, 0.55f, 0.95f, 1f);
        private static readonly Color VoiceColor = new(0.95f, 0.62f, 0.35f, 1f);
        private static readonly Color SoundColor = new(0.40f, 0.82f, 0.55f, 1f);

        private readonly struct AudioResourceRow
        {
            public readonly AudioClip Clip;
            public readonly string Guid;
            public readonly string Name;
            public readonly string GroupLabel;
            public readonly string AssetPath;
            public readonly AudioGroupData AssignedGroup;
            public readonly AudioEntryData Entry;
            public readonly bool IsAssigned;

            public AudioResourceRow(
                AudioClip clip, string guid, string name,
                string groupLabel, AudioGroupData group, AudioEntryData entry, bool assigned)
            {
                Clip = clip;
                Guid = guid;
                Name = name;
                GroupLabel = groupLabel;
                AssetPath = AssetDatabase.GetAssetPath(clip);
                AssignedGroup = group;
                Entry = entry;
                IsAssigned = assigned;
            }
        }

        private AudioGroupDatabase database;
        private string[] databaseGuids;
        private string selectGuid;
        private int selectedGroupIndex = -1;
        private string resourceSearch = string.Empty;
        private readonly HashSet<string> selectedResourceGuids = new();
        private int lastSelectedResourceIndex = -1;
        private bool showUnassignedOnly;
        private Vector2 groupScroll;
        private Vector2 poolScroll;
        private Vector2 detailScroll;
        private float poolSplitRatio = 0.45f;
        private bool showCodeGenSettings = true;
        private bool showGroupSharedSettings = true;
        private bool isDraggingPoolSplitter;
        private bool isDraggingGroupPanelSplitter;
        private double lastClickTime;
        private string lastClickGuid;

        private List<AudioClip> scannedClips = new();
        private ReorderableList entryReorderList;

        private GUIStyle _rowLabelStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _columnHeaderStyle;
        private GUIStyle _miniBtnStyle;
        private GUIStyle _hintStyle;
        private GUIStyle _groupNameStyle;
        private GUIStyle _groupCodeStyle;
        private GUIStyle _groupBadgeStyle;
        private GUIStyle _groupCountStyle;
        private GUIStyle _sectionTitleStyle;
        private GUIStyle _ruleBadgeStyle;
        private GUIStyle _detailBoxStyle;
        private GUIStyle _previewBtnStyle;
        private GUIStyle _entryHeaderPreviewStyle;
        private GUIStyle _entryHeaderSummaryStyle;
        private GUIStyle _entryHeaderDeleteStyle;
        private GUIStyle _toolbarActionStyle;
        private GUIStyle _toolbarPrimaryStyle;
        private Texture2D _audioIcon;
        private GUIContent _previewPlayIcon;
        private GUIContent _previewStopIcon;

        [MenuItem("YukiFrameWork/Audio分组编辑器", false, -7)]
        public static void ShowWindow()
        {
            var window = GetWindow<AudioGroupDatabaseEditorWindow>();
            window.titleContent = new GUIContent("音频分组");
            window.minSize = new Vector2(780f, 440f);
            window.Show();
        }

        public void SetDatabase(AudioGroupDatabase db)
        {
            if (db == null) return;
            database = db;
            selectGuid = YukiAssetDataBase.InstanceToGUID(db);
            PlayerPrefs.SetString(SELECT_GUID_KEY, selectGuid);
            ScanAudioClips();
            SyncSelection();
            Repaint();
        }

        private void InitStyles()
        {
            if (_entryHeaderDeleteStyle != null) return;
            _rowLabelStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Clip,
                fontSize = 11,
                padding = new RectOffset(2, 2, 0, 0)
            };
            _headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(6, 4, 0, 0)
            };
            _columnHeaderStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.Bold,
                fontSize = 11,
                clipping = TextClipping.Overflow,
                padding = new RectOffset(2, 2, 0, 0)
            };
            _hintStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight,
                normal = { textColor = new Color(0.65f, 0.65f, 0.65f) }
            };
            _miniBtnStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fixedHeight = 16,
                fixedWidth = 20,
                padding = new RectOffset(0, 0, 0, 0)
            };
            _groupNameStyle = new GUIStyle(EditorStyles.label)
            {
                fontSize = 11,
                fontStyle = FontStyle.Bold,
                clipping = TextClipping.Clip,
                padding = new RectOffset(0, 0, 0, 0),
                normal = { textColor = new Color(0.92f, 0.92f, 0.92f) }
            };
            _groupCodeStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                fontSize = 9,
                clipping = TextClipping.Clip,
                normal = { textColor = new Color(0.50f, 0.68f, 0.82f) }
            };
            _groupBadgeStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 9,
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(3, 3, 0, 0)
            };
            _groupCountStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 9,
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.78f, 0.78f, 0.78f) }
            };
            _sectionTitleStyle = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                fontSize = 10,
                normal = { textColor = new Color(0.72f, 0.82f, 0.95f) }
            };
            _ruleBadgeStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight,
                fontSize = 11,
                clipping = TextClipping.Overflow,
                wordWrap = false,
                padding = new RectOffset(2, 2, 0, 0),
                normal = { textColor = new Color(0.62f, 0.78f, 0.62f) }
            };
            _detailBoxStyle = new GUIStyle("HelpBox")
            {
                padding = new RectOffset(6, 6, 6, 6)
            };
            _audioIcon = EditorGUIUtility.IconContent("AudioClip Icon").image as Texture2D;
            _previewPlayIcon = EditorGUIUtility.IconContent("PlayButton On", "预览当前规则下的播放效果");
            _previewStopIcon = EditorGUIUtility.IconContent("PauseButton On", "停止预览");
            _previewBtnStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fixedHeight = 18,
                fixedWidth = 22,
                padding = new RectOffset(2, 2, 2, 2),
                imagePosition = ImagePosition.ImageOnly
            };
            _entryHeaderPreviewStyle = new GUIStyle(_columnHeaderStyle) { alignment = TextAnchor.MiddleCenter };
            _entryHeaderSummaryStyle = new GUIStyle(_columnHeaderStyle) { alignment = TextAnchor.MiddleRight };
            _entryHeaderDeleteStyle = new GUIStyle(_columnHeaderStyle) { alignment = TextAnchor.MiddleCenter };

            _toolbarActionStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fixedHeight = 22,
                fontSize = 11,
                fontStyle = FontStyle.Normal,
                padding = new RectOffset(10, 10, 2, 2),
                margin = new RectOffset(2, 2, 0, 0)
            };
            _toolbarPrimaryStyle = new GUIStyle(_toolbarActionStyle)
            {
                fontStyle = FontStyle.Bold,
                normal = { textColor = new Color(0.92f, 0.96f, 0.92f) },
                hover = { textColor = Color.white },
                active = { textColor = Color.white }
            };
        }

        private static EntryRowLayout BuildEntryRowLayout(Rect rowRect, float summaryColWidth)
        {
            var y = rowRect.y;
            var h = rowRect.height;
            var x = rowRect.x + EntryRowPad;

            var preview = new Rect(x, y, EntryPreviewColWidth, h);
            x = preview.xMax + EntryColInnerGap;
            var expand = new Rect(x, y, EntryExpandColWidth, h);
            x = expand.xMax + EntryColInnerGap;
            var icon = new Rect(x, y, EntryIconColWidth, h);
            x = icon.xMax + EntryColInnerGap;

            var delete = new Rect(rowRect.xMax - EntryRowPad - EntryDeleteColWidth, y, EntryDeleteColWidth, h);
            var summary = new Rect(delete.x - EntryColGap - summaryColWidth, y, summaryColWidth, h);
            var name = new Rect(x, y, Mathf.Max(0f, summary.x - EntryColGap - x), h);

            return new EntryRowLayout(preview, expand, icon, name, summary, delete);
        }

        private static Rect InsetHeaderRowRect(Rect rect) =>
            new(rect.x + EntryDragHandleWidth, rect.y, rect.width - EntryDragHandleWidth, rect.height);

        private static Rect CenterControlInColumn(Rect column, float controlSize)
        {
            var size = Mathf.Min(controlSize, column.height - 2f);
            return new Rect(
                column.x + (column.width - size) * 0.5f,
                column.y + (column.height - size) * 0.5f,
                size,
                size);
        }

        private static void DrawEntryRowGridLines(Rect rowRect, EntryRowLayout layout)
        {
            if (Event.current.type != EventType.Repaint) return;
            var y = rowRect.y;
            var h = rowRect.height;
            var line = new Color(0.12f, 0.12f, 0.12f, 0.85f);
            DrawVerticalGridLine(layout.Preview.xMax + EntryColInnerGap * 0.5f, y, h, line);
            DrawVerticalGridLine(layout.Expand.xMax + EntryColInnerGap * 0.5f, y, h, line);
            DrawVerticalGridLine(layout.Icon.xMax + EntryColInnerGap * 0.5f, y, h, line);
            DrawVerticalGridLine(layout.Summary.x - EntryColGap * 0.5f, y, h, line);
            DrawVerticalGridLine(layout.Delete.x - EntryColGap * 0.5f, y, h, line);
        }

        private static void DrawVerticalGridLine(float x, float y, float height, Color color) =>
            EditorGUI.DrawRect(new Rect(x, y + 1f, 1f, height - 2f), color);

        private void DrawEntryListHeader(Rect rect, AudioGroupData group)
        {
            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(rect, HeaderBg);

            var contentRect = InsetHeaderRowRect(rect);
            var layout = BuildEntryRowLayout(contentRect, entrySummaryColWidth);
            DrawEntryRowGridLines(contentRect, layout);

            GUI.Label(layout.Preview, "预览", _entryHeaderPreviewStyle);
            GUI.Label(layout.Name, GetEntryListHeaderTitle(group), _columnHeaderStyle);
            GUI.Label(layout.Summary, "规则摘要", _entryHeaderSummaryStyle);
            GUI.Label(layout.Delete, "删除", _entryHeaderDeleteStyle);
        }

        private string GetEntryListHeaderTitle(AudioGroupData group)
        {
            var total = group.entries?.Count ?? 0;
            if (groupEntrySearch.IsNullOrEmpty()) return $"已分配 ({total})";
            var visible = CountMatchingGroupEntries(group);
            return $"已分配 ({visible}/{total})";
        }

        private int CountMatchingGroupEntries(AudioGroupData group)
        {
            if (group?.entries == null) return 0;
            if (groupEntrySearch.IsNullOrEmpty()) return group.entries.Count;
            var count = 0;
            foreach (var entry in group.entries)
            {
                if (entry != null && EntryMatchesSearch(entry, group, groupEntrySearch))
                    count++;
            }
            return count;
        }

        private bool EntryMatchesSearch(AudioEntryData entry, AudioGroupData group, string search)
        {
            if (entry == null || search.IsNullOrEmpty()) return true;
            var isSound = group != null && group.playType == AudioPlayType.Sound;
            return entry.GetDisplayName().IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0
                   || entry.ResolvedAssetName.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0
                   || (!entry.codeKey.IsNullOrEmpty() && entry.codeKey.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                   || BuildRuleSummary(entry, isSound).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void SyncGroupEntrySearchContext(AudioGroupData group)
        {
            if (group == null)
            {
                groupEntrySearch = string.Empty;
                lastEntrySearchGroupId = null;
                return;
            }

            if (lastEntrySearchGroupId != group.id)
            {
                groupEntrySearch = string.Empty;
                lastEntrySearchGroupId = group.id;
            }
        }

        private void DrawGroupEntrySearchBar()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("搜索", GUILayout.Width(32));
            var nextSearch = EditorGUILayout.TextField(groupEntrySearch, EditorStyles.toolbarSearchField);
            if (nextSearch != groupEntrySearch)
            {
                groupEntrySearch = nextSearch;
                RebuildEntryList();
            }
            if (GUILayout.Button("清除", EditorStyles.miniButton, GUILayout.Width(40)))
            {
                if (!groupEntrySearch.IsNullOrEmpty())
                {
                    groupEntrySearch = string.Empty;
                    RebuildEntryList();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private float CalcEntrySummaryColumnWidth(AudioGroupData group, bool isSound)
        {
            InitStyles();
            const float padding = 10f;
            var width = _entryHeaderSummaryStyle.CalcSize(new GUIContent("规则摘要")).x + padding;
            if (group?.entries == null) return Mathf.Max(width, EntrySummaryMinWidth);

            foreach (var entry in group.entries)
            {
                var summary = BuildRuleSummary(entry, isSound);
                width = Mathf.Max(width, _ruleBadgeStyle.CalcSize(new GUIContent(summary)).x + padding);
            }

            return Mathf.Clamp(width, EntrySummaryMinWidth, EntrySummaryMaxWidth);
        }

        private float GetCodeGenPanelHeight() =>
            CodeGenFoldoutHeight + (showCodeGenSettings ? CodeGenExpandedHeight : 0f) + CodeGenBottomPad;

        private static Color GetPlayTypeColor(AudioPlayType type) => type switch
        {
            AudioPlayType.Music => MusicColor,
            AudioPlayType.Voice => VoiceColor,
            AudioPlayType.Sound => SoundColor,
            _ => Color.gray
        };

        private void OnEnable()
        {
            Instance = this;
            groupPanelWidth = Mathf.Clamp(
                PlayerPrefs.GetFloat(GROUP_PANEL_WIDTH_KEY, DefaultGroupPanelWidth),
                MinGroupPanelWidth,
                MaxGroupPanelWidth);
            EditorApplication.update += OnEditorUpdate;
            RefreshDatabaseGuids();
            selectGuid = Selection.activeObject is AudioGroupDatabase selected
                ? YukiAssetDataBase.InstanceToGUID(selected)
                : PlayerPrefs.GetString(SELECT_GUID_KEY, databaseGuids?.FirstOrDefault());

            database = YukiAssetDataBase.GUIDToInstance<AudioGroupDatabase>(selectGuid);
            if (database)
            {
                database.onValidate = OnDatabaseChanged;
                ScanAudioClips();
                SyncSelection();
            }
        }

        private void OnDisable()
        {
            EditorApplication.update -= OnEditorUpdate;
            if (database != null) database.onValidate = null;
            PlayerPrefs.SetString(SELECT_GUID_KEY, selectGuid);
            PlayerPrefs.SetFloat(GROUP_PANEL_WIDTH_KEY, groupPanelWidth);
            Instance = null;
        }

        private void OnEditorUpdate()
        {
            if (AudioEntryPreviewUtility.IsPlaying)
                Repaint();
        }

        private void OnDatabaseChanged()
        {
            RebuildEntryList();
            Repaint();
        }

        private void SyncSelection()
        {
            if (database == null || database.groups.Count == 0)
            {
                selectedGroupIndex = -1;
                return;
            }
            if (selectedGroupIndex < 0 || selectedGroupIndex >= database.groups.Count)
                selectedGroupIndex = 0;
            RebuildEntryList();
        }

        private AudioGroupData GetSelectedGroup()
        {
            if (database == null || selectedGroupIndex < 0 || selectedGroupIndex >= database.groups.Count)
                return null;
            return database.groups[selectedGroupIndex];
        }

        private void RefreshDatabaseGuids()
        {
            databaseGuids = AssetDatabase.FindAssets($"t:{nameof(AudioGroupDatabase)}");
        }

        public void ScanAudioClips()
        {
            scannedClips.Clear();
            if (!database) return;

            var folders = database.scanFolders is { Length: > 0 } ? database.scanFolders : new[] { "Assets" };
            foreach (var guid in AssetDatabase.FindAssets("t:AudioClip", folders))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (database.excludePathKeywords != null &&
                    database.excludePathKeywords.Any(k => !k.IsNullOrEmpty() && path.Contains(k)))
                    continue;
                var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                if (clip) scannedClips.Add(clip);
            }
            scannedClips = scannedClips.OrderBy(c => c.name).ToList();
        }

        private void OnGUI()
        {
            InitStyles();
            DrawToolbar();

            if (!database)
            {
                DrawEmptyState();
                return;
            }

            var area = new Rect(0, ToolbarHeight, position.width, position.height - ToolbarHeight);
            groupPanelWidth = Mathf.Clamp(groupPanelWidth, MinGroupPanelWidth, Mathf.Min(MaxGroupPanelWidth, area.width * 0.55f));

            var leftRect = new Rect(area.x, area.y, groupPanelWidth, area.height);
            var groupPanelSplitterRect = new Rect(leftRect.xMax, area.y, GroupPanelSplitterWidth, area.height);
            var rightRect = new Rect(groupPanelSplitterRect.xMax, area.y,
                area.width - groupPanelWidth - GroupPanelSplitterWidth, area.height);

            DrawPanelBg(leftRect, PanelBg);
            DrawGroupPanelSplitter(groupPanelSplitterRect, area);
            DrawPanelBg(rightRect, PanelBg);

            DrawGroupList(leftRect);
            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(new Rect(groupPanelSplitterRect.x, area.y, 1, area.height), DividerColor);

            var codeGenHeight = GetCodeGenPanelHeight();
            var codeGenRect = new Rect(rightRect.x + 4, rightRect.y + 4, rightRect.width - 8, codeGenHeight);
            DrawCodeGenerationSettingsInRect(codeGenRect);
            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(new Rect(rightRect.x, rightRect.y + codeGenHeight + 4, rightRect.width, 1), DividerColor);

            var mainRight = new Rect(rightRect.x, rightRect.y + codeGenHeight + 6, rightRect.width, rightRect.height - codeGenHeight - 6);
            var poolHeight = mainRight.height * poolSplitRatio;
            var detailHeight = mainRight.height - poolHeight - SplitterHeight;
            var detailRect = new Rect(mainRight.x, mainRight.y, mainRight.width, detailHeight);
            var poolSplitterRect = new Rect(mainRight.x, mainRight.y + detailHeight, mainRight.width, SplitterHeight);
            var poolRect = new Rect(mainRight.x, poolSplitterRect.yMax, mainRight.width, poolHeight);

            DrawPoolSplitter(poolSplitterRect, mainRight.y);
            DrawGroupDetail(detailRect);
            DrawResourceTable(poolRect);

            HandleGlobalDragAndDrop(detailRect);

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
            {
                AssignSelectedResourcesToGroup();
                Event.current.Use();
            }
        }

        private void DrawEmptyState()
        {
            var area = new Rect(0, ToolbarHeight, position.width, position.height - ToolbarHeight);
            if (Event.current.type == EventType.Repaint)
                DrawPanelBg(area, PanelBg);

            GUILayout.BeginArea(area);
            EditorGUILayout.BeginVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical(GUILayout.Width(360f));
            EditorGUILayout.HelpBox("请选择或创建 AudioGroupDatabase 配置。", MessageType.Info);
            if (GUILayout.Button("创建配置", GUILayout.Height(26))) CreateNewDatabase();
            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void DrawToolbar()
        {
            var rect = new Rect(0, 0, position.width, ToolbarHeight);
            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(rect, HeaderBg);
                EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1, rect.width, 1), DividerColor);
            }

            GUILayout.BeginArea(new Rect(rect.x + 8, rect.y + 4, rect.width - 16, rect.height - 8));

            EditorGUILayout.BeginHorizontal();
            var dbLabel = database ? database.name : "选择配置";
            if (EditorGUILayout.DropdownButton(new GUIContent(dbLabel), FocusType.Passive,
                    EditorStyles.toolbarDropDown, GUILayout.Width(148), GUILayout.Height(22)))
                ShowDatabaseMenu();

            GUILayout.Space(8);

            GUI.enabled = database != null;
            var prevBg = GUI.backgroundColor;

            GUI.backgroundColor = new Color(0.42f, 0.48f, 0.54f, 1f);
            if (GUILayout.Button(new GUIContent("  扫描音频", EditorGUIUtility.IconContent("Refresh").image),
                    _toolbarActionStyle, GUILayout.Height(22), GUILayout.MinWidth(88)))
            {
                ScanAudioClips();
                SyncSelection();
            }

            GUI.backgroundColor = AccentGreen * 0.85f;
            if (GUILayout.Button(new GUIContent("  新建分组", EditorGUIUtility.IconContent("CreateAddNew").image),
                    _toolbarPrimaryStyle, GUILayout.Height(22), GUILayout.MinWidth(92)))
            {
                database.AddGroup();
                selectedGroupIndex = database.groups.Count - 1;
                RebuildEntryList();
            }

            GUI.backgroundColor = prevBg;
            GUI.enabled = true;

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        private void DrawCodeGenerationSettingsInRect(Rect rect)
        {
            GUILayout.BeginArea(rect);
            DrawCodeGenerationSettings();
            GUILayout.EndArea();
        }

        private void DrawCodeGenerationSettings()
        {
            showCodeGenSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showCodeGenSettings, "代码生成设置");
            if (showCodeGenSettings)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("输出文件夹", GUILayout.Width(72));
                database.codeFilePath = EditorGUILayout.TextField(database.codeFilePath);
                if (GUILayout.Button("…", GUILayout.Width(24)))
                    PickCodeOutputPath();
                EditorGUILayout.EndHorizontal();

                database.codeClassName = EditorGUILayout.TextField("类名", database.codeClassName);
                database.nameSpace = EditorGUILayout.TextField("生成命名空间", database.nameSpace);

                var fullPath = GetGeneratedScriptAssetPath();
                EditorGUILayout.LabelField("完整输出路径", fullPath, EditorStyles.wordWrappedMiniLabel);

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("恢复默认生成设置", GUILayout.Width(128)))
                    ResetCodeSettingsToFrameworkDefaults();

                GUILayout.FlexibleSpace();

                var prev = GUI.backgroundColor;
                GUI.backgroundColor = AccentGreen;
                if (GUILayout.Button("生成代码", GUILayout.Width(72)))
                    AudioGroupCodeGenerator.Generate(database);
                GUI.backgroundColor = prev;

                if (TryGetGeneratedScript(out var script))
                {
                    if (GUILayout.Button("打开生成脚本", GUILayout.Width(96)))
                        OpenGeneratedScript(script);
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (GUI.changed)
                EditorUtility.SetDirty(database);
        }

        private string GetGeneratedScriptAssetPath()
        {
            if (database == null || database.codeClassName.IsNullOrEmpty())
                return string.Empty;

            var folder = database.codeFilePath.IsNullOrEmpty()
                ? AudioGroupDatabase.FrameworkDefaultPath
                : database.codeFilePath.TrimEnd('/', '\\');
            return $"{folder}/{database.codeClassName}.cs";
        }

        private bool TryGetGeneratedScript(out MonoScript script)
        {
            script = null;
            var path = GetGeneratedScriptAssetPath();
            if (path.IsNullOrEmpty() || !path.StartsWith("Assets/"))
                return false;
            if (!System.IO.File.Exists(path))
                return false;
            script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
            return script;
        }

        private static void OpenGeneratedScript(MonoScript script)
        {
            if (!script) return;
            Selection.activeObject = script;
            EditorGUIUtility.PingObject(script);
            AssetDatabase.OpenAsset(script);
        }

        private void PickCodeOutputPath()
        {
            var picked = EditorUtility.OpenFolderPanel("选择代码输出路径", "Assets", string.Empty);
            if (picked.IsNullOrEmpty()) return;
            var relative = FileUtil.GetProjectRelativePath(picked);
            if (!relative.IsNullOrEmpty())
                database.codeFilePath = relative;
        }

        private static void ResetCodeSettingsToFrameworkDefaults()
        {
            if (Instance?.database == null) return;
            Instance.database.codeFilePath = AudioGroupDatabase.FrameworkDefaultPath;
            Instance.database.nameSpace = AudioGroupDatabase.FrameworkDefaultNamespace;
            Instance.database.codeClassName = "GameAudios";
            EditorUtility.SetDirty(Instance.database);
        }

        private void ShowDatabaseMenu()
        {
            var menu = new GenericMenu();
            if (databaseGuids is { Length: > 0 })
            {
                foreach (var guid in databaseGuids)
                {
                    var item = YukiAssetDataBase.GUIDToInstance<AudioGroupDatabase>(guid);
                    if (item) menu.AddItem(new GUIContent(item.name), selectGuid == guid, () => SetDatabase(item));
                }
            }
            else menu.AddDisabledItem(new GUIContent("无配置"));
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("新建配置"), false, CreateNewDatabase);
            menu.ShowAsContext();
        }

        private void CalcGroupGridLayout(float innerWidth, int count, out int columns, out float cardWidth, out float contentHeight)
        {
            columns = Mathf.Max(1, Mathf.FloorToInt((innerWidth + GroupCardGap) / (MinGroupCardWidth + GroupCardGap)));
            if (count > 0)
                columns = Mathf.Min(columns, count);
            cardWidth = columns > 0
                ? (innerWidth - GroupCardGap * (columns - 1)) / columns
                : innerWidth;
            var rows = count > 0 ? Mathf.CeilToInt(count / (float)columns) : 0;
            contentHeight = rows > 0
                ? GroupCardPadding * 2 + rows * GroupCardHeight + (rows - 1) * GroupCardGap
                : GroupCardPadding * 2;
        }

        private void DrawGroupList(Rect rect)
        {
            var headerRect = new Rect(rect.x, rect.y, rect.width, 28);
            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(headerRect, HeaderBg);
            GUI.Label(headerRect, $"  分组  {database.groups.Count}", _headerStyle);
            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(new Rect(rect.x, headerRect.yMax - 1, rect.width, 1), DividerColor);

            var listRect = new Rect(rect.x, rect.y + 28, rect.width, rect.height - 28);
            var innerWidth = listRect.width - 14 - GroupCardPadding * 2;
            CalcGroupGridLayout(innerWidth, database.groups.Count, out var columns, out var cardWidth, out var contentHeight);

            groupScroll = GUI.BeginScrollView(listRect, groupScroll, new Rect(0, 0, listRect.width - 14, contentHeight));

            for (int i = 0; i < database.groups.Count; i++)
            {
                var col = i % columns;
                var row = i / columns;
                var slotRect = new Rect(
                    GroupCardPadding + col * (cardWidth + GroupCardGap),
                    GroupCardPadding + row * (GroupCardHeight + GroupCardGap),
                    cardWidth,
                    GroupCardHeight);
                DrawGroupCard(slotRect, database.groups[i], i);
            }

            GUI.EndScrollView();
        }

        private void DrawGroupCard(Rect cardRect, AudioGroupData group, int index)
        {
            var selected = index == selectedGroupIndex;
            var hovered = cardRect.Contains(Event.current.mousePosition);
            var typeColor = GetPlayTypeColor(group.playType);

            if (Event.current.type == EventType.Repaint)
            {
                var bg = selected ? GroupSelectedTint : hovered ? CardHoverBg : CardBg;
                EditorGUI.DrawRect(cardRect, bg);
                if (hovered && !selected)
                    EditorGUI.DrawRect(new Rect(cardRect.x + 1, cardRect.y + 1, cardRect.width - 2, cardRect.height - 2),
                        new Color(1f, 1f, 1f, 0.03f));
                EditorGUI.DrawRect(new Rect(cardRect.x, cardRect.y, GroupAccentWidth, cardRect.height),
                    selected ? typeColor : typeColor * 0.55f);
                DrawRectBorder(cardRect, selected ? typeColor * 0.45f : new Color(0.14f, 0.14f, 0.14f, 0.95f));
            }

            var contentX = cardRect.x + GroupAccentWidth + GroupInnerPad;
            var contentW = cardRect.width - GroupAccentWidth - GroupInnerPad * 2f;
            var row1Y = cardRect.y + 8f;
            const float badgeH = 17f;
            var badgeW = Mathf.Min(48f, contentW * 0.38f);
            var countW = 22f;

            var badgeRect = new Rect(contentX, row1Y, badgeW, badgeH);
            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(badgeRect, typeColor * (selected ? 0.38f : 0.22f));
                DrawRectBorder(badgeRect, typeColor * (selected ? 0.65f : 0.45f));
            }
            GUI.Label(badgeRect, group.playType.ToString(), _groupBadgeStyle);

            var tag = group.groupName.IsNullOrEmpty() ? "默认分组" : group.groupName;
            var nameW = Mathf.Max(24f, contentW - badgeW - countW - 12f);
            var nameRect = new Rect(contentX + badgeW + 6f, row1Y, nameW, badgeH);
            GUI.Label(nameRect, tag, _groupNameStyle);

            var countRect = new Rect(cardRect.xMax - GroupInnerPad - countW, row1Y, countW, badgeH);
            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(countRect, new Color(0.08f, 0.08f, 0.08f, 0.9f));
                DrawRectBorder(countRect, selected ? typeColor * 0.35f : new Color(0.30f, 0.30f, 0.30f, 0.9f));
            }
            GUI.Label(countRect, group.entries.Count.ToString(), _groupCountStyle);

            var code = database.GetGroupAccessorExpression(group);
            GUI.Label(new Rect(contentX, row1Y + badgeH + 4f, contentW, 14f), code, _groupCodeStyle);

            if (GUI.Button(cardRect, GUIContent.none, GUIStyle.none))
            {
                selectedGroupIndex = index;
                RebuildEntryList();
                Event.current.Use();
            }
        }

        private static void DrawRectBorder(Rect rect, Color color)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1, rect.width, 1), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, 1, rect.height), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - 1, rect.y, 1, rect.height), color);
        }

        private void DrawGroupDetail(Rect rect)
        {
            DrawSectionHeader(rect, "当前分组");
            var bodyRect = new Rect(rect.x + 6, rect.y + 26, rect.width - 12, rect.height - 32);
            var group = GetSelectedGroup();

            GUILayout.BeginArea(bodyRect);
            detailScroll = EditorGUILayout.BeginScrollView(detailScroll);

            if (group == null)
            {
                EditorGUILayout.LabelField("← 选择左侧分组，或点击「新建分组」", EditorStyles.centeredGreyMiniLabel);
                EditorGUILayout.EndScrollView();
                GUILayout.EndArea();
                return;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("层级", GUILayout.Width(36));
            group.playType = (AudioPlayType)EditorGUILayout.EnumPopup(group.playType, GUILayout.Width(80));
            EditorGUILayout.LabelField("组名", GUILayout.Width(36));
            group.groupName = EditorGUILayout.TextField(group.groupName);
            if (GUILayout.Button("删", GUILayout.Width(28)))
            {
                if (EditorUtility.DisplayDialog("删除", $"删除 [{BuildGroupLabel(group)}]？", "确定", "取消"))
                {
                    database.RemoveGroup(group);
                    selectedGroupIndex = database.groups.Count > 0 ? Mathf.Clamp(selectedGroupIndex, 0, database.groups.Count - 1) : -1;
                    RebuildEntryList();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField(database.GetGroupAccessorExpression(group), EditorStyles.miniLabel);

            DrawGroupSharedSettings(group);

            SyncGroupEntrySearchContext(group);
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("点击左侧 ▼ 展开条目，在「播放规则」中配置循环、调用风格等。", EditorStyles.centeredGreyMiniLabel);
            DrawGroupEntrySearchBar();
            if (entryReorderList == null) RebuildEntryList();
            else
            {
                var isSoundGroup = group.playType == AudioPlayType.Sound;
                entrySummaryColWidth = CalcEntrySummaryColumnWidth(group, isSoundGroup);
            }
            if (entryReorderList != null)
                entryReorderList.DoLayoutList();

            if (!groupEntrySearch.IsNullOrEmpty() && CountMatchingGroupEntries(group) == 0)
                EditorGUILayout.HelpBox($"未找到匹配「{groupEntrySearch}」的音频。", MessageType.Info);

            EditorGUILayout.Space(2);

            var dropRect = GUILayoutUtility.GetRect(0, 26, GUILayout.ExpandWidth(true));
            GUI.Box(dropRect, "拖入 AudioClip 添加到当前分组", EditorStyles.helpBox);
            HandleDragAndDrop(dropRect, group);

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawGroupSharedSettings(AudioGroupData group)
        {
            if (group.sharedRules == null)
                group.sharedRules = new AudioGroupSharedRuleSettings();

            var rules = group.sharedRules;
            var isSound = group.playType == AudioPlayType.Sound;
            var entryCount = group.entries?.Count ?? 0;

            EditorGUILayout.Space(4);
            showGroupSharedSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showGroupSharedSettings, "通用设置");
            if (showGroupSharedSettings)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);

                rules.enabled = EditorGUILayout.ToggleLeft(
                    new GUIContent("启用分组通用设置", "开启后可配置统一播放规则，并通过「应用到全部」同步到组内所有音频"),
                    rules.enabled);

                EditorGUI.BeginDisabledGroup(!rules.enabled);

                EditorGUILayout.LabelField("播放规则", EditorStyles.miniBoldLabel);
                EditorGUILayout.BeginHorizontal();
                rules.loopByDefault = EditorGUILayout.ToggleLeft(
                    new GUIContent("默认循环", "对应 AudioPlayer.Loop()"), rules.loopByDefault, GUILayout.MinWidth(90));
                rules.useRealTime = EditorGUILayout.ToggleLeft(
                    new GUIContent("真实时间", "不受 Time.timeScale 影响"), rules.useRealTime);
                EditorGUILayout.EndHorizontal();

                if (isSound)
                {
                    rules.useInterval = EditorGUILayout.ToggleLeft(
                        new GUIContent("间隔播放", "仅 Sound 层，对应 AudioPlayer.Interval()"), rules.useInterval);
                }

                rules.bindParent = EditorGUILayout.ToggleLeft(
                    new GUIContent("绑定父节点", "生成代码传入 Transform parent"), rules.bindParent);

                var playStyle = rules.GetPlayStyle();
                playStyle = (AudioCodeGenPlayStyle)EditorGUILayout.EnumPopup(
                    new GUIContent("调用风格", "Build/Play 或 PlayAsync 等生成写法"), playStyle);
                rules.SetPlayStyle(playStyle);

                EditorGUILayout.Space(2);
                EditorGUILayout.LabelField("3D 音效", EditorStyles.miniBoldLabel);
                rules.use3DSetting = EditorGUILayout.ToggleLeft(
                    new GUIContent("启用 3D 参数", "对应 AudioPlayer.AudioSourceSoundSetting()"), rules.use3DSetting);

                if (rules.use3DSetting)
                {
                    rules.external3DSetting = DrawSoundSettingSourceToggle(rules.external3DSetting);
                    if (!rules.external3DSetting)
                    {
                        if (rules.soundSetting == null)
                            rules.soundSetting = new AudioSourceSoundSetting();

                        EditorGUI.indentLevel++;
                        var setting = rules.soundSetting;
                        setting.Pitch = EditorGUILayout.Slider("Pitch", setting.Pitch, -3f, 3f);
                        setting.SpatitalBlend = EditorGUILayout.Slider("Spatial Blend", setting.SpatitalBlend, 0f, 1f);
                        setting.StereoPan = EditorGUILayout.Slider("Stereo Pan", setting.StereoPan, -1f, 1f);
                        setting.Priority = EditorGUILayout.IntSlider("Priority", setting.Priority, 0, 256);
                        setting.MinDistance = EditorGUILayout.FloatField("Min Distance", setting.MinDistance);
                        setting.MaxDistance = EditorGUILayout.FloatField("Max Distance", setting.MaxDistance);
                        setting.VolumeRolloff = (AudioRolloffMode)EditorGUILayout.EnumPopup("Rolloff", setting.VolumeRolloff);
                        EditorGUI.indentLevel--;
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("生成代码将接收 AudioSourceSoundSetting soundSetting 参数。", MessageType.None);
                    }
                }

                EditorGUILayout.Space(4);
                EditorGUILayout.BeginHorizontal();
                GUI.enabled = rules.enabled && entryCount > 0;
                var prevBg = GUI.backgroundColor;
                GUI.backgroundColor = AccentGreen;
                if (GUILayout.Button($"应用到全部 ({entryCount})", GUILayout.Height(24)))
                {
                    var applied = group.ApplySharedRulesToAllEntries();
                    EditorUtility.SetDirty(database);
                    RebuildEntryList();
                    Debug.Log($"[AudioGroup] 已将通用设置同步到 {applied} 条音频。");
                }
                GUI.backgroundColor = prevBg;
                GUI.enabled = rules.enabled && entryCount > 0;
                if (GUILayout.Button("从首条读取", GUILayout.Width(88), GUILayout.Height(24)))
                {
                    rules.CopyFrom(group.entries[0]);
                    EditorUtility.SetDirty(database);
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();

                if (!rules.enabled)
                    EditorGUILayout.HelpBox("启用后可编辑通用规则，并一键同步到当前组全部音频。新加入的音频也会自动套用。", MessageType.Info);
                else if (entryCount == 0)
                    EditorGUILayout.HelpBox("当前分组暂无音频，添加后会自动套用通用设置。", MessageType.Info);

                EditorGUI.EndDisabledGroup();
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            if (GUI.changed)
                EditorUtility.SetDirty(database);
        }

        private void DrawResourceTable(Rect rect)
        {
            var rows = GetFilteredResourceRows();
            PruneResourceSelection(rows);
            var unassignedCount = rows.Count(r => !r.IsAssigned);

            // 标题栏
            var titleRect = new Rect(rect.x, rect.y, rect.width, TableTitleHeight);
            EditorGUI.DrawRect(titleRect, HeaderBg);
            GUI.Label(new Rect(titleRect.x + 6, titleRect.y, 120, titleRect.height),
                showUnassignedOnly ? $"未分配  {unassignedCount}" : $"匹配资源  {rows.Count}", _headerStyle);
            GUI.Label(new Rect(titleRect.xMax - 280, titleRect.y, 274, titleRect.height),
                "Ctrl/Shift 多选  |  右键/Enter 添加  |  双击 Ping", _hintStyle);
            var toggleRect = new Rect(titleRect.x + 130, titleRect.y + 2, 90, 18);
            showUnassignedOnly = GUI.Toggle(toggleRect, showUnassignedOnly, "仅未分配", EditorStyles.miniLabel);

            // 搜索栏
            var searchRect = new Rect(rect.x + 4, rect.y + TableTitleHeight + 2, rect.width - 8, TableSearchHeight);
            resourceSearch = EditorGUI.TextField(searchRect, resourceSearch, EditorStyles.toolbarSearchField);

            // 列头
            var headerY = searchRect.yMax + 2;
            var headerRect = new Rect(rect.x, headerY, rect.width, TableHeaderHeight);
            DrawTableColumnHeader(headerRect);

            // 列表
            var listRect = new Rect(rect.x, headerRect.yMax, rect.width, rect.height - (headerRect.yMax - rect.y));
            var contentWidth = listRect.width - 16;
            var contentHeight = rows.Count * RowHeight;
            poolScroll = GUI.BeginScrollView(listRect, poolScroll, new Rect(0, 0, contentWidth, contentHeight));

            var cols = CalcColumnWidths(contentWidth);
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                var rowRect = new Rect(0, i * RowHeight, contentWidth, RowHeight);
                DrawResourceRow(rowRect, row, cols, i, rows);
            }

            GUI.EndScrollView();
        }

        private void DrawTableColumnHeader(Rect rect)
        {
            EditorGUI.DrawRect(rect, new Color(0.22f, 0.22f, 0.22f, 1f));
            var cols = CalcColumnWidths(rect.width);
            float x = rect.x;

            DrawColumnCell(new Rect(x, rect.y, cols.Icon, rect.height), "");
            x += cols.Icon;
            DrawColumnCell(new Rect(x, rect.y, cols.Name, rect.height), "音频名称");
            x += cols.Name;
            DrawColumnCell(new Rect(x, rect.y, cols.Group, rect.height), "Group");
            x += cols.Group;
            DrawColumnCell(new Rect(x, rect.y, cols.Path, rect.height), "Asset Path");
            x += cols.Path;
            DrawColumnCell(new Rect(x, rect.y, cols.Action, rect.height), "");
        }

        private static (float Icon, float Name, float Group, float Path, float Action) CalcColumnWidths(float total)
        {
            const float icon = 22f;
            const float action = 24f;
            var rest = total - icon - action;
            return (icon, rest * 0.28f, rest * 0.14f, rest * 0.58f, action);
        }

        private void DrawResourceRow(Rect rowRect, AudioResourceRow row,
            (float Icon, float Name, float Group, float Path, float Action) cols, int index,
            IReadOnlyList<AudioResourceRow> rows)
        {
            var selected = selectedResourceGuids.Contains(row.Guid);
            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(rowRect, selected ? SelectedBg : index % 2 == 0 ? PanelBg : RowAltBg);

            float x = rowRect.x;
            var iconRect = new Rect(x + 3, rowRect.y + 2, 16, 16);
            if (_audioIcon) GUI.DrawTexture(iconRect, _audioIcon, ScaleMode.ScaleToFit);
            x += cols.Icon;

            var nameRect = new Rect(x, rowRect.y, cols.Name, rowRect.height);
            var groupRect = new Rect(x + cols.Name, rowRect.y, cols.Group, rowRect.height);
            var pathRect = new Rect(x + cols.Name + cols.Group, rowRect.y, cols.Path, rowRect.height);
            var actionRect = new Rect(rowRect.xMax - cols.Action, rowRect.y + 2, 20, 16);

            GUI.Label(nameRect, row.Name, _rowLabelStyle);
            GUI.Label(groupRect, row.GroupLabel, _rowLabelStyle);
            GUI.Label(pathRect, row.AssetPath, _rowLabelStyle);

            if (!row.IsAssigned && GUI.Button(actionRect, "+", _miniBtnStyle))
                AssignClipToSelectedGroup(row.Clip);

            var clickRect = new Rect(rowRect.x, rowRect.y, rowRect.width - cols.Action, rowRect.height);
            HandleRowInteraction(clickRect, row, index, rows);
        }

        private void HandleRowInteraction(Rect clickRect, AudioResourceRow row, int index, IReadOnlyList<AudioResourceRow> rows)
        {
            var evt = Event.current;
            // BeginScrollView 内 mousePosition 已是内容区坐标，勿再叠加 scroll 偏移
            if (!clickRect.Contains(evt.mousePosition)) return;

            if (evt.type == EventType.MouseDown && evt.button == 0)
            {
                SelectResourceRow(row, index, rows, evt.control || evt.command, evt.shift);

                if (evt.clickCount == 2)
                {
                    PingResource(row.Clip);
                    evt.Use();
                    return;
                }

                if (evt.clickCount == 1)
                {
                    var now = EditorApplication.timeSinceStartup;
                    if (lastClickGuid == row.Guid && now - lastClickTime < 0.35)
                        PingResource(row.Clip);
                    lastClickGuid = row.Guid;
                    lastClickTime = now;
                    evt.Use();
                }
            }

            if (evt.type == EventType.ContextClick)
            {
                if (!selectedResourceGuids.Contains(row.Guid))
                    SelectResourceRow(row, index, rows, false, false);

                ShowResourceContextMenu();
                evt.Use();
            }
        }

        private void SelectResourceRow(
            AudioResourceRow row,
            int index,
            IReadOnlyList<AudioResourceRow> rows,
            bool additive,
            bool range)
        {
            if (range && lastSelectedResourceIndex >= 0 && lastSelectedResourceIndex < rows.Count)
            {
                selectedResourceGuids.Clear();
                var from = Mathf.Min(lastSelectedResourceIndex, index);
                var to = Mathf.Max(lastSelectedResourceIndex, index);
                for (int i = from; i <= to; i++)
                    selectedResourceGuids.Add(rows[i].Guid);
            }
            else if (additive)
            {
                if (!selectedResourceGuids.Add(row.Guid) && selectedResourceGuids.Count > 1)
                    selectedResourceGuids.Remove(row.Guid);
                lastSelectedResourceIndex = index;
            }
            else
            {
                selectedResourceGuids.Clear();
                selectedResourceGuids.Add(row.Guid);
                lastSelectedResourceIndex = index;
            }
        }

        private void PruneResourceSelection(IReadOnlyList<AudioResourceRow> visibleRows)
        {
            if (selectedResourceGuids.Count == 0) return;
            var visible = new HashSet<string>(visibleRows.Select(r => r.Guid));
            selectedResourceGuids.RemoveWhere(guid => !visible.Contains(guid));
            if (selectedResourceGuids.Count == 0)
                lastSelectedResourceIndex = -1;
        }

        private List<AudioResourceRow> GetSelectedResourceRows()
        {
            if (selectedResourceGuids.Count == 0) return new List<AudioResourceRow>();
            return GetFilteredResourceRows()
                .Where(r => selectedResourceGuids.Contains(r.Guid))
                .ToList();
        }

        private void ShowResourceContextMenu()
        {
            var selected = GetSelectedResourceRows();
            if (selected.Count == 0) return;

            var menu = new GenericMenu();
            if (selected.Count == 1)
                menu.AddItem(new GUIContent("Ping 资源"), false, () => PingResource(selected[0].Clip));
            else
                menu.AddDisabledItem(new GUIContent($"已选 {selected.Count} 项"));

            var unassigned = selected.Where(r => !r.IsAssigned).ToList();
            if (unassigned.Count > 0)
            {
                var addLabel = unassigned.Count == 1
                    ? "添加到当前分组"
                    : $"添加到当前分组 ({unassigned.Count})";
                menu.AddItem(new GUIContent(addLabel), false, () =>
                    AssignClipsToSelectedGroup(unassigned.Select(r => r.Clip)));
            }

            var assigned = selected.Where(r => r.IsAssigned).ToList();
            if (assigned.Count > 0)
            {
                var removeLabel = assigned.Count == 1
                    ? "从分组移除"
                    : $"从分组移除 ({assigned.Count})";
                menu.AddItem(new GUIContent(removeLabel), false, () =>
                {
                    foreach (var row in assigned)
                        database.RemoveEntry(row.AssignedGroup, row.Entry);
                    RebuildEntryList();
                    Repaint();
                });
            }

            menu.ShowAsContext();
        }

        private void AssignSelectedResourcesToGroup()
        {
            var clips = GetSelectedResourceRows()
                .Where(r => !r.IsAssigned)
                .Select(r => r.Clip)
                .Where(c => c)
                .ToList();
            if (clips.Count == 0) return;
            AssignClipsToSelectedGroup(clips);
        }

        private void AssignClipsToSelectedGroup(IEnumerable<AudioClip> clips)
        {
            var group = GetSelectedGroup();
            if (group == null)
            {
                EnsureOrSelectGroup(AudioPlayType.Sound);
                group = GetSelectedGroup();
            }
            if (group == null) return;

            foreach (var clip in clips)
                AssignClip(clip, group);
        }

        private static void PingResource(AudioClip clip)
        {
            if (!clip) return;
            EditorGUIUtility.PingObject(clip);
            Selection.activeObject = clip;
        }

        private List<AudioResourceRow> GetFilteredResourceRows()
        {
            var rows = BuildAllResourceRows();
            if (showUnassignedOnly)
                rows = rows.Where(r => !r.IsAssigned).ToList();

            if (!resourceSearch.IsNullOrEmpty())
            {
                rows = rows.Where(r =>
                    r.Name.IndexOf(resourceSearch, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    r.GroupLabel.IndexOf(resourceSearch, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    r.AssetPath.IndexOf(resourceSearch, StringComparison.OrdinalIgnoreCase) >= 0
                ).ToList();
            }
            return rows;
        }

        private List<AudioResourceRow> BuildAllResourceRows()
        {
            var rows = new List<AudioResourceRow>(scannedClips.Count);
            foreach (var clip in scannedClips)
            {
                var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(clip));
                var (group, entry) = FindAssignment(clip);
                var assigned = group != null && entry != null;
                var groupLabel = assigned ? BuildGroupLabel(group) : "-";
                rows.Add(new AudioResourceRow(clip, guid, clip.name, groupLabel, group, entry, assigned));
            }
            return rows;
        }

        private (AudioGroupData Group, AudioEntryData Entry) FindAssignment(AudioClip clip)
        {
            if (!clip || database == null) return (null, null);
            var path = AssetDatabase.GetAssetPath(clip);
            var guid = AssetDatabase.AssetPathToGUID(path);
            foreach (var group in database.groups)
            {
                foreach (var entry in group.entries)
                {
                    if (entry.clip == clip) return (group, entry);
                    if (!entry.clipGuid.IsNullOrEmpty() && entry.clipGuid == guid) return (group, entry);
                }
            }
            return (null, null);
        }

        private void DrawColumnCell(Rect rect, string text)
        {
            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(new Rect(rect.xMax - 1, rect.y, 1, rect.height), LineColor);
                if (rect.x > 0)
                    EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1, rect.width, 1), LineColor);
            }
            GUI.Label(rect, text, _columnHeaderStyle);
        }

        private void DrawSectionHeader(Rect rect, string title)
        {
            var headerRect = new Rect(rect.x, rect.y, rect.width, 24);
            EditorGUI.DrawRect(headerRect, HeaderBg);
            GUI.Label(headerRect, "  " + title, _headerStyle);
        }

        private static void DrawPanelBg(Rect rect, Color color)
        {
            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(rect, color);
        }

        private void DrawGroupPanelSplitter(Rect rect, Rect area)
        {
            var hover = rect.Contains(Event.current.mousePosition);
            if (Event.current.type == EventType.Repaint)
            {
                EditorGUI.DrawRect(rect, hover || isDraggingGroupPanelSplitter
                    ? new Color(0.28f, 0.32f, 0.38f, 1f)
                    : new Color(0.14f, 0.14f, 0.14f, 1f));
                if (hover || isDraggingGroupPanelSplitter)
                {
                    var gripX = rect.x + (rect.width - 2f) * 0.5f;
                    EditorGUI.DrawRect(new Rect(gripX, rect.y + rect.height * 0.35f, 2f, rect.height * 0.3f),
                        new Color(0.55f, 0.60f, 0.68f, 0.9f));
                }
            }

            EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeHorizontal);
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                isDraggingGroupPanelSplitter = true;
                Event.current.Use();
            }
            if (isDraggingGroupPanelSplitter && Event.current.type == EventType.MouseDrag)
            {
                groupPanelWidth = Mathf.Clamp(
                    Event.current.mousePosition.x - area.x,
                    MinGroupPanelWidth,
                    Mathf.Min(MaxGroupPanelWidth, area.width * 0.55f));
                Repaint();
            }
            if (Event.current.type == EventType.MouseUp)
                isDraggingGroupPanelSplitter = false;
        }

        private void DrawPoolSplitter(Rect rect, float mainRightTop)
        {
            var hover = rect.Contains(Event.current.mousePosition);
            if (Event.current.type == EventType.Repaint)
                EditorGUI.DrawRect(rect, hover || isDraggingPoolSplitter
                    ? new Color(0.28f, 0.32f, 0.38f, 1f)
                    : new Color(0.12f, 0.12f, 0.12f, 1f));

            EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeVertical);
            if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                isDraggingPoolSplitter = true;
                Event.current.Use();
            }
            if (isDraggingPoolSplitter && Event.current.type == EventType.MouseDrag)
            {
                var mainRightHeight = position.height - ToolbarHeight - GetCodeGenPanelHeight() - 6f;
                poolSplitRatio = Mathf.Clamp(1f - (Event.current.mousePosition.y - mainRightTop) / mainRightHeight, 0.25f, 0.75f);
                Repaint();
            }
            if (Event.current.type == EventType.MouseUp)
                isDraggingPoolSplitter = false;
        }

        private void RebuildEntryList()
        {
            var group = GetSelectedGroup();
            if (group == null) { entryReorderList = null; return; }

            var isSound = group.playType == AudioPlayType.Sound;
            var groupCode = database.GetGroupAccessorExpression(group);
            entrySummaryColWidth = CalcEntrySummaryColumnWidth(group, isSound);

            entryReorderList = new ReorderableList(group.entries, typeof(AudioEntryData), true, true, true, true)
            {
                headerHeight = EntryCompactHeight,
                drawHeaderCallback = rect => DrawEntryListHeader(rect, group),
                elementHeightCallback = index =>
                {
                    if (index < 0 || index >= group.entries.Count) return EntryCompactHeight;
                    var entry = group.entries[index];
                    if (!EntryMatchesSearch(entry, group, groupEntrySearch)) return 0f;
                    return entry.expandedInEditor
                        ? EntryCompactHeight + GetEntryDetailHeight(entry, isSound)
                        : EntryCompactHeight;
                },
                drawElementCallback = (rect, index, _, _) =>
                {
                    if (index >= group.entries.Count) return;
                    var entry = group.entries[index];
                    if (!EntryMatchesSearch(entry, group, groupEntrySearch)) return;
                    entry.SyncFromClip();

                    var rowRect = new Rect(rect.x, rect.y, rect.width, EntryCompactHeight);
                    if (Event.current.type == EventType.Repaint)
                        EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, rect.height), index % 2 == 0 ? PanelBg : RowAltBg);

                    DrawEntryCompactRow(rowRect, entry, group, groupCode, isSound);

                    if (entry.expandedInEditor)
                    {
                        var detailHeight = GetEntryDetailHeight(entry, isSound);
                        var detailRect = new Rect(rect.x + 14, rowRect.yMax, rect.width - 18, detailHeight);
                        DrawEntryDetailPanel(detailRect, entry, group, isSound);
                    }
                },
                onAddCallback = _ =>
                {
                    var menu = new GenericMenu();
                    var hasItem = false;
                    foreach (var row in GetFilteredResourceRows().Where(r => !r.IsAssigned).Take(50))
                    {
                        hasItem = true;
                        menu.AddItem(new GUIContent(row.Name), false, () => AssignClip(row.Clip, group));
                    }
                    if (!hasItem) menu.AddDisabledItem(new GUIContent("无未分配音频"));
                    menu.ShowAsContext();
                },
                onRemoveCallback = list =>
                {
                    if (list.index < 0 || list.index >= group.entries.Count) return;
                    database.RemoveEntry(group, group.entries[list.index]);
                }
            };
        }

        private static float GetEntryDetailHeight(AudioEntryData entry, bool isSound)
        {
            var line = EditorGUIUtility.singleLineHeight;
            var gap = EditorGUIUtility.standardVerticalSpacing;
            var h = EntryDetailPadding * 2f + 12f;

            h += EntrySectionTitleHeight + line + gap;
            h += EntrySectionTitleHeight + line + gap + line + gap;
            if (isSound)
                h += line + gap;
            h += line + gap;
            h += line + gap + 4f;
            h += EntrySectionTitleHeight + line + gap;
            if (entry.use3DSetting)
            {
                h += line + gap;
                if (!entry.external3DSetting)
                    h += (line + gap) * 7f;
                else
                    h += line + gap;
            }
            return h;
        }

        private static string BuildRuleSummary(AudioEntryData entry, bool isSound)
        {
            var parts = new List<string>(5);
            if (entry.loopByDefault) parts.Add("循环");
            if (IsAsyncPlayStyle(GetEntryPlayStyle(entry))) parts.Add("异步");
            if (entry.useRealTime) parts.Add("实时");
            if (isSound && entry.useInterval) parts.Add("间隔");
            if (entry.use3DSetting) parts.Add(entry.external3DSetting ? "3D外" : "3D");
            if (entry.bindParent) parts.Add("父节点");
            return parts.Count > 0 ? string.Join(" · ", parts) : "默认";
        }

        private static bool IsAsyncPlayStyle(AudioCodeGenPlayStyle style) =>
            AudioGroupSharedRuleSettings.IsAsyncPlayStyle(style);

        private static AudioCodeGenPlayStyle GetEntryPlayStyle(AudioEntryData entry) =>
            entry.useAsync ? entry.asyncPlayStyle : entry.playStyle;

        private static void SetEntryPlayStyle(AudioEntryData entry, AudioCodeGenPlayStyle style)
        {
            entry.NormalizePlayStyle();
            if (IsAsyncPlayStyle(style))
            {
                entry.useAsync = true;
                entry.asyncPlayStyle = style;
            }
            else
            {
                entry.useAsync = false;
                entry.playStyle = style;
            }
        }

        private void DrawEntryCompactRow(Rect rowRect, AudioEntryData entry, AudioGroupData group, string groupCode, bool isSound)
        {
            var layout = BuildEntryRowLayout(rowRect, entrySummaryColWidth);
            DrawEntryRowGridLines(rowRect, layout);

            var previewBtnRect = CenterControlInColumn(layout.Preview, EntryControlSize);
            var playing = AudioEntryPreviewUtility.IsPreviewingEntry(entry);
            var previewIcon = playing ? _previewStopIcon : _previewPlayIcon;
            if (GUI.Button(previewBtnRect, previewIcon, _previewBtnStyle))
                AudioEntryPreviewUtility.Toggle(entry, group, groupCode);

            var expandRect = CenterControlInColumn(layout.Expand, EntryControlSize);
            var expandLabel = entry.expandedInEditor ? "▲" : "▼";
            if (GUI.Button(expandRect, new GUIContent(expandLabel, "展开 / 折叠详情"), _miniBtnStyle))
            {
                entry.expandedInEditor = !entry.expandedInEditor;
                EditorUtility.SetDirty(database);
            }

            var iconRect = CenterControlInColumn(layout.Icon, 16f);
            if (_audioIcon) GUI.DrawTexture(iconRect, _audioIcon, ScaleMode.ScaleToFit);

            GUI.Label(layout.Name, entry.GetDisplayName(), _rowLabelStyle);
            GUI.Label(layout.Summary, BuildRuleSummary(entry, isSound), _ruleBadgeStyle);

            var deleteBtnRect = CenterControlInColumn(layout.Delete, EntryControlSize);
            if (GUI.Button(deleteBtnRect, new GUIContent("×", "从当前分组移除此音频"), _miniBtnStyle))
            {
                database.RemoveEntry(group, entry);
                RebuildEntryList();
                EditorUtility.SetDirty(database);
            }
        }

        private void DrawEntryDetailPanel(Rect rect, AudioEntryData entry, AudioGroupData group, bool isSound)
        {
            GUI.BeginGroup(rect);
            var inner = new Rect(0, 0, rect.width, rect.height);
            GUI.Box(inner, GUIContent.none, _detailBoxStyle);

            var content = new Rect(
                inner.x + _detailBoxStyle.padding.left,
                inner.y + _detailBoxStyle.padding.top,
                inner.width - _detailBoxStyle.padding.horizontal,
                inner.height - _detailBoxStyle.padding.vertical);

            var line = EditorGUIUtility.singleLineHeight;
            var gap = EditorGUIUtility.standardVerticalSpacing;
            var y = content.y;
            var changed = false;

            y = DrawDetailSectionTitle(content, y, "命名与代码");
            var half = (content.width - 8f) * 0.5f;
            var nameLabel = new Rect(content.x, y, 44, line);
            var nameField = new Rect(content.x + 46, y, half - 46, line);
            var keyLabel = new Rect(content.x + half + 8, y, 52, line);
            var keyField = new Rect(content.x + half + 62, y, half - 62, line);
            GUI.Label(nameLabel, "加载名");
            entry.assetName = EditorGUI.TextField(nameField, entry.assetName);
            GUI.Label(keyLabel, "代码标识");
            entry.codeKey = EditorGUI.TextField(keyField, entry.codeKey);
            y += line + gap + 4f;

            y = DrawDetailSectionTitle(content, y, "播放规则");
            var colW = (content.width - gap) * 0.5f;
            changed |= DrawRuleToggle(new Rect(content.x, y, colW, line),
                "默认循环", "对应 AudioPlayer.Loop()", ref entry.loopByDefault);
            changed |= DrawRuleToggle(new Rect(content.x + colW + gap, y, colW, line),
                "真实时间", "不受 Time.timeScale 影响", ref entry.useRealTime);
            y += line + gap;
            if (isSound)
            {
                changed |= DrawRuleToggle(new Rect(content.x, y, colW, line),
                    "间隔播放", "仅 Sound 层，对应 AudioPlayer.Interval()", ref entry.useInterval);
                y += line + gap;
            }
            changed |= DrawRuleToggle(new Rect(content.x, y, colW, line),
                "绑定父节点", "生成代码传入 Transform parent，并调用 AudioPlayer.Parent()", ref entry.bindParent);
            y += line + gap;

            var playStyle = GetEntryPlayStyle(entry);
            playStyle = DrawLabeledEnum(content, y, "调用风格", playStyle, "Build/Play 或 PlayAsync 等生成写法");
            SetEntryPlayStyle(entry, playStyle);
            y += line + gap + 4f;

            y = DrawDetailSectionTitle(content, y, "3D 音效");
            changed |= DrawRuleToggle(new Rect(content.x, y, content.width, line),
                "启用 3D 参数", "对应 AudioPlayer.AudioSourceSoundSetting()", ref entry.use3DSetting);
            y += line + gap;

            if (entry.use3DSetting)
            {
                changed |= DrawSoundSettingSourceToggle(new Rect(content.x, y, content.width, line), ref entry.external3DSetting);
                y += line + gap;

                if (!entry.external3DSetting)
                {
                    if (entry.soundSetting == null) entry.soundSetting = new AudioSourceSoundSetting();
                    var setting = entry.soundSetting;
                    var indent = 14f;
                    var fieldW = content.width - indent;

                    setting.Pitch = EditorGUI.Slider(new Rect(content.x + indent, y, fieldW, line), "Pitch", setting.Pitch, -3f, 3f);
                    y += line + gap;
                    setting.SpatitalBlend = EditorGUI.Slider(new Rect(content.x + indent, y, fieldW, line), "Spatial Blend", setting.SpatitalBlend, 0f, 1f);
                    y += line + gap;
                    setting.StereoPan = EditorGUI.Slider(new Rect(content.x + indent, y, fieldW, line), "Stereo Pan", setting.StereoPan, -1f, 1f);
                    y += line + gap;
                    setting.Priority = EditorGUI.IntSlider(new Rect(content.x + indent, y, fieldW, line), "Priority", setting.Priority, 0, 256);
                    y += line + gap;
                    setting.MinDistance = EditorGUI.FloatField(new Rect(content.x + indent, y, fieldW, line), "Min Distance", setting.MinDistance);
                    y += line + gap;
                    setting.MaxDistance = EditorGUI.FloatField(new Rect(content.x + indent, y, fieldW, line), "Max Distance", setting.MaxDistance);
                    y += line + gap;
                    setting.VolumeRolloff = (AudioRolloffMode)EditorGUI.EnumPopup(
                        new Rect(content.x + indent, y, fieldW, line), "Rolloff", setting.VolumeRolloff);
                    y += line + gap;
                }
                else
                {
                    GUI.Label(new Rect(content.x, y, content.width, line),
                        "生成代码将接收 AudioSourceSoundSetting soundSetting 参数。", EditorStyles.miniLabel);
                    y += line + gap;
                }
            }

            GUI.EndGroup();

            if (changed || GUI.changed)
                EditorUtility.SetDirty(database);
        }

        private static T DrawLabeledEnum<T>(Rect content, float y, string label, T value, string tooltip) where T : Enum
        {
            var line = EditorGUIUtility.singleLineHeight;
            GUI.Label(new Rect(content.x, y, FieldLabelWidth, line), new GUIContent(label, tooltip));
            return (T)EditorGUI.EnumPopup(
                new Rect(content.x + FieldLabelWidth, y, EnumFieldWidth, line),
                GUIContent.none, value);
        }

        private float DrawDetailSectionTitle(Rect content, float y, string title)
        {
            GUI.Label(new Rect(content.x, y, content.width, EntrySectionTitleHeight), title, _sectionTitleStyle);
            return y + EntrySectionTitleHeight;
        }

        private static bool DrawRuleToggle(Rect rect, string label, string tooltip, ref bool value)
        {
            var prev = value;
            value = EditorGUI.ToggleLeft(new Rect(rect.x, rect.y, rect.width, rect.height),
                new GUIContent(label, tooltip), value);
            return prev != value;
        }

        private static bool DrawSoundSettingSourceToggle(Rect rect, ref bool external)
        {
            const float labelW = 56f;
            const float btnW = 72f;
            GUI.Label(new Rect(rect.x, rect.y, labelW, rect.height), new GUIContent("参数来源", "本地配置写入生成代码，代码传入由运行时参数提供"));
            var localRect = new Rect(rect.x + labelW, rect.y, btnW, rect.height);
            var externalRect = new Rect(localRect.xMax + 2f, rect.y, btnW, rect.height);
            var prev = external;
            if (GUI.Toggle(localRect, !external, "本地配置", EditorStyles.miniButtonLeft))
                external = false;
            if (GUI.Toggle(externalRect, external, "代码传入", EditorStyles.miniButtonRight))
                external = true;
            return prev != external;
        }

        private static bool DrawSoundSettingSourceToggle(bool external)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("参数来源", GUILayout.Width(56));
            if (GUILayout.Toggle(!external, "本地配置", EditorStyles.miniButtonLeft, GUILayout.Width(72)))
                external = false;
            if (GUILayout.Toggle(external, "代码传入", EditorStyles.miniButtonRight, GUILayout.Width(72)))
                external = true;
            EditorGUILayout.EndHorizontal();
            return external;
        }

        private static string BuildGroupLabel(AudioGroupData group)
        {
            var tag = group.groupName.IsNullOrEmpty() ? "默认" : group.groupName;
            return $"{group.playType}/{tag}";
        }

        private void EnsureOrSelectGroup(AudioPlayType playType)
        {
            var idx = database.groups.FindIndex(g => g.playType == playType && g.groupName.IsNullOrEmpty());
            if (idx >= 0) selectedGroupIndex = idx;
            else { database.AddGroup(playType, string.Empty); selectedGroupIndex = database.groups.Count - 1; }
            RebuildEntryList();
            Repaint();
        }

        private void CreateNewDatabase()
        {
            var path = EditorUtility.SaveFilePanelInProject("新建配置", "AudioGroupDatabase", "asset", "保存位置");
            if (path.IsNullOrEmpty()) return;
            var asset = ScriptableObject.CreateInstance<AudioGroupDatabase>();
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            RefreshDatabaseGuids();
            SetDatabase(asset);
        }

        internal void AssignClip(AudioClip clip, AudioGroupData targetGroup)
        {
            if (database == null || clip == null || targetGroup == null) return;
            if (database.AssignClipToGroup(targetGroup, clip))
            {
                RebuildEntryList();
                Repaint();
            }
        }

        private void AssignClipToSelectedGroup(AudioClip clip)
        {
            if (!clip) return;
            AssignClipsToSelectedGroup(new[] { clip });
        }

        private void HandleDragAndDrop(Rect rect, AudioGroupData targetGroup)
        {
            var evt = Event.current;
            if (!rect.Contains(evt.mousePosition)) return;
            if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform) return;
            if (!DragAndDrop.objectReferences.Any(o => o is AudioClip)) return;

            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                foreach (var obj in DragAndDrop.objectReferences)
                    if (obj is AudioClip clip) AssignClip(clip, targetGroup);
            }
            evt.Use();
        }

        private void HandleGlobalDragAndDrop(Rect detailRect)
        {
            var group = GetSelectedGroup();
            if (group == null) return;
            HandleDragAndDrop(detailRect, group);
        }
    }
}
#endif
