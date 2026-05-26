#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using YukiFrameWork;

namespace YukiFrameWork.UI
{
#if UNITY_6000_2_OR_NEWER
    internal class UIPanelTreeItem : TreeViewItem<int>
#else
    internal class UIPanelTreeItem : TreeViewItem
#endif
    {
        public BasePanel Panel { get; }

        public UIPanelTreeItem(BasePanel panel) : base(panel.GetInstanceID(), 1, panel.name)
        {
            Panel = panel;
        }
    }

#if UNITY_6000_2_OR_NEWER
    internal class UIPanelTreeView : TreeView<int>
#else
    internal class UIPanelTreeView : TreeView
#endif
    {
        private readonly EditorWindow window;
        private readonly MultiColumnHeaderState columnHeaderState;
        private List<BasePanel> panels = new List<BasePanel>();
        private string searchFilter = string.Empty;
        private bool isInitialized;
        private int nextGroupId = 1;

        internal static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
        {
            return new MultiColumnHeaderState(GetColumns());
        }

        private static MultiColumnHeaderState.Column[] GetColumns()
        {
            return new[]
            {
                CreateColumn("Name", "面板名称", 140, 320),
                CreateColumn("Path", "预制体资源路径", 180, 800),
                CreateColumn("Level", "面板层级", 56, 100),
                CreateColumn("Open", "打开模式", 56, 100),
                CreateColumn("Type", "脚本类型", 80, 240),
            };
        }

        private static MultiColumnHeaderState.Column CreateColumn(string title, string tooltip, float width, float maxWidth)
        {
            return new MultiColumnHeaderState.Column
            {
                headerContent = new GUIContent(title, tooltip),
                minWidth = 40,
                width = width,
                maxWidth = maxWidth,
                headerTextAlignment = TextAlignment.Left,
                canSort = true,
                autoResize = true,
            };
        }

#if UNITY_6000_2_OR_NEWER
        public UIPanelTreeView(TreeViewState<int> state, MultiColumnHeaderState headerState, EditorWindow window)
            : base(state, new MultiColumnHeader(headerState))
#else
        public UIPanelTreeView(TreeViewState state, MultiColumnHeaderState headerState, EditorWindow window)
            : base(state, new MultiColumnHeader(headerState))
#endif
        {
            columnHeaderState = headerState;
            this.window = window;
            showBorder = true;
            showAlternatingRowBackgrounds = true;
            rowHeight = 20f;
            Reload();
        }

        public void SetData(IList<BasePanel> source, string filter)
        {
            filter ??= string.Empty;
            var nextPanels = source?.Where(p => p).ToList() ?? new List<BasePanel>();
            if (isInitialized && searchFilter == filter && PanelsEqual(panels, nextPanels))
                return;

            panels = nextPanels;
            searchFilter = filter;
            isInitialized = true;
            Reload();
        }

        private static bool PanelsEqual(IReadOnlyList<BasePanel> left, IReadOnlyList<BasePanel> right)
        {
            if (left.Count != right.Count)
                return false;

            for (int i = 0; i < left.Count; i++)
            {
                if (left[i] != right[i])
                    return false;
            }

            return true;
        }

        public BasePanel GetSelectedPanel()
        {
            var ids = GetSelection();
            if (ids == null || ids.Count == 0)
                return null;

            var item = FindItem(ids[0], rootItem);
            return item is UIPanelTreeItem panelItem ? panelItem.Panel : null;
        }

#if UNITY_6000_2_OR_NEWER
        protected override TreeViewItem<int> BuildRoot()
#else
        protected override TreeViewItem BuildRoot()
#endif
        {
#if UNITY_6000_2_OR_NEWER
            var root = new TreeViewItem<int>(0, -1, "Root")
            {
                children = new List<TreeViewItem<int>>(),
            };
#else
            var root = new TreeViewItem(0, -1, "Root")
            {
                children = new List<TreeViewItem>(),
            };
#endif

            var filtered = FilterPanels(panels, searchFilter);
            var groups = filtered
                .GroupBy(GetGroupKey)
                .OrderBy(g => g.Key, System.StringComparer.OrdinalIgnoreCase);

            nextGroupId = 1;
            foreach (var group in groups)
            {
#if UNITY_6000_2_OR_NEWER
                var groupItem = new TreeViewItem<int>(nextGroupId++, 0, group.Key);
#else
                var groupItem = new TreeViewItem(nextGroupId++, 0, group.Key);
#endif

                foreach (var panel in group.OrderBy(p => p.name, System.StringComparer.OrdinalIgnoreCase))
                    groupItem.AddChild(new UIPanelTreeItem(panel));

                if (groupItem.hasChildren)
                    root.AddChild(groupItem);
            }

            return root;
        }

        private static IEnumerable<BasePanel> FilterPanels(IEnumerable<BasePanel> source, string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return source;

            var keyword = filter.Trim().ToLowerInvariant();
            return source.Where(p =>
            {
                var path = AssetDatabase.GetAssetPath(p);
                return p.name.ToLowerInvariant().Contains(keyword)
                       || path.ToLowerInvariant().Contains(keyword)
                       || p.GetType().Name.ToLowerInvariant().Contains(keyword);
            });
        }

        private static string GetGroupKey(BasePanel panel)
        {
            var path = AssetDatabase.GetAssetPath(panel);
            if (string.IsNullOrEmpty(path))
                return "Unknown";

            var directory = Path.GetDirectoryName(path)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(directory))
                return "Assets";

            const string assetsPrefix = "Assets/";
            return directory.StartsWith(assetsPrefix)
                ? directory.Substring(assetsPrefix.Length)
                : directory;
        }

        private const float FoldoutWidth = 14f;
        private const float FoldoutPadding = 2f;
        private GUIStyle groupHeaderLabelStyle;

        protected override void RowGUI(RowGUIArgs args)
        {
            if (args.item.depth == 0)
                DrawGroupRowBackground(args);

            for (int i = 0; i < args.GetNumVisibleColumns(); i++)
                CellGUI(args.GetCellRect(i), args.item, columnHeaderState.visibleColumns[i], ref args);
        }

        private void DrawGroupRowBackground(RowGUIArgs args)
        {
            if (Event.current.type != EventType.Repaint)
                return;

            var bgColor = args.selected
                ? new Color(0.17f, 0.36f, 0.53f, 1f)
                : new Color(0.24f, 0.24f, 0.24f, 1f);
            EditorGUI.DrawRect(args.rowRect, bgColor);
        }

#if UNITY_6000_2_OR_NEWER
        private void CellGUI(Rect cellRect, TreeViewItem<int> item, int column, ref RowGUIArgs args)
#else
        private void CellGUI(Rect cellRect, TreeViewItem item, int column, ref RowGUIArgs args)
#endif
        {
            if (column == 0)
            {
                if (item.depth == 0)
                {
                    DrawTreeColumnLabel(cellRect, item, item.displayName, args);
                    return;
                }

                if (item is UIPanelTreeItem panelItem && panelItem.Panel != null)
                {
                    DrawNameCell(cellRect, panelItem.Panel, args, item);
                    return;
                }
            }

            CenterRectUsingSingleLineHeight(ref cellRect);

            if (item is not UIPanelTreeItem rowPanelItem || rowPanelItem.Panel == null)
                return;

            var panel = rowPanelItem.Panel;
            var path = AssetDatabase.GetAssetPath(panel);

            switch (column)
            {
                case 1:
                    DefaultGUI.Label(cellRect, path, args.selected, args.focused);
                    break;
                case 2:
                    DefaultGUI.Label(cellRect, panel.Level.ToString(), args.selected, args.focused);
                    break;
                case 3:
                    DefaultGUI.Label(cellRect, panel.OpenType.ToString(), args.selected, args.focused);
                    break;
                case 4:
                    DefaultGUI.Label(cellRect, panel.GetType().Name, args.selected, args.focused);
                    break;
            }
        }

#if UNITY_6000_2_OR_NEWER
        private void DrawTreeColumnLabel(Rect cellRect, TreeViewItem<int> item, string label, RowGUIArgs args)
#else
        private void DrawTreeColumnLabel(Rect cellRect, TreeViewItem item, string label, RowGUIArgs args)
#endif
        {
            var lineHeight = EditorGUIUtility.singleLineHeight;

            GUI.BeginGroup(cellRect);

            if (Event.current.type == EventType.Repaint)
            {
                var bgColor = args.selected
                    ? new Color(0.17f, 0.36f, 0.53f, 1f)
                    : new Color(0.24f, 0.24f, 0.24f, 1f);
                EditorGUI.DrawRect(new Rect(0f, 0f, cellRect.width, cellRect.height), bgColor);
            }

            var foldoutRect = new Rect(FoldoutPadding, (cellRect.height - lineHeight) * 0.5f, FoldoutWidth, lineHeight);

            var expanded = IsExpanded(item.id);
            var newExpanded = EditorGUI.Foldout(foldoutRect, expanded, GUIContent.none, true, EditorStyles.foldout);
            if (newExpanded != expanded)
                SetExpanded(item.id, newExpanded);

            var labelRect = new Rect(foldoutRect.xMax + 4f, foldoutRect.y, cellRect.width - foldoutRect.xMax - 4f, lineHeight);
            EditorGUI.LabelField(labelRect, label, GetGroupHeaderLabelStyle());

            GUI.EndGroup();
        }

        private GUIStyle GetGroupHeaderLabelStyle()
        {
            groupHeaderLabelStyle ??= new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Clip,
            };

            return groupHeaderLabelStyle;
        }

        private static void CenterRectVertically(ref Rect rect, float height)
        {
            rect.y += (rect.height - height) * 0.5f;
            rect.height = height;
        }

        private static float GetNameColumnContentIndent(int depth)
        {
            if (depth <= 0)
                return FoldoutPadding + FoldoutWidth;

            return FoldoutPadding + FoldoutWidth + depth * FoldoutWidth;
        }

#if UNITY_6000_2_OR_NEWER
        private void DrawNameCell(Rect cellRect, BasePanel panel, RowGUIArgs args, TreeViewItem<int> item)
#else
        private void DrawNameCell(Rect cellRect, BasePanel panel, RowGUIArgs args, TreeViewItem item)
#endif
        {
            var lineHeight = EditorGUIUtility.singleLineHeight;
            var indent = GetNameColumnContentIndent(item.depth);

            var contentRect = cellRect;
            contentRect.x += indent;
            contentRect.width -= indent;
            CenterRectVertically(ref contentRect, lineHeight);

            var content = EditorGUIUtility.ObjectContent(panel.gameObject, typeof(GameObject));
            var iconRect = new Rect(contentRect.x, contentRect.y + (contentRect.height - 16f) * 0.5f, 16f, 16f);
            GUI.Label(iconRect, content.image, GUIStyle.none);

            var labelRect = contentRect;
            labelRect.x += 18f;
            labelRect.width -= 18f;
            DefaultGUI.Label(labelRect, panel.name, args.selected, args.focused);
        }

        protected override void DoubleClickedItem(int id)
        {
            var item = FindItem(id, rootItem);
            if (item is UIPanelTreeItem panelItem && panelItem.Panel != null)
                AssetDatabase.OpenAsset(panelItem.Panel);
        }

        protected override void ContextClickedItem(int id)
        {
            var item = FindItem(id, rootItem);
            if (item is not UIPanelTreeItem panelItem || panelItem.Panel == null)
                return;

            var panel = panelItem.Panel;
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Ping"), false, () =>
            {
                Selection.activeObject = panel;
                EditorGUIUtility.PingObject(panel);
            });
            menu.AddItem(new GUIContent("打开预制体"), false, () => AssetDatabase.OpenAsset(panel));
            menu.AddItem(new GUIContent("在资源管理器显示"), false, () =>
                EditorUtility.RevealInFinder(AssetDatabase.GetAssetPath(panel)));
            menu.ShowAsContext();
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);
            window.Repaint();
        }
    }

    public class UIVistualWindow : EditorWindow
    {
        private readonly List<BasePanel> panelPrefabs = new List<BasePanel>();
        private string searchFilter = string.Empty;

#if UNITY_6000_2_OR_NEWER
        private TreeViewState<int> treeViewState;
#else
        private TreeViewState treeViewState;
#endif
        private MultiColumnHeaderState multiColumnHeaderState;
        private UIPanelTreeView treeView;

        [MenuItem("YukiFrameWork/UI检索窗口")]
        private static void ShowWindow()
        {
            var window = GetWindow<UIVistualWindow>("UI 检索");
            window.minSize = new Vector2(520, 320);
            window.Show();
        }

        private void OnEnable()
        {
            EnsureTreeView();
        }

        private void EnsureTreeView()
        {
            if (treeView != null)
                return;

#if UNITY_6000_2_OR_NEWER
            treeViewState ??= new TreeViewState<int>();
#else
            treeViewState ??= new TreeViewState();
#endif

            var headerState = UIPanelTreeView.CreateDefaultMultiColumnHeaderState();
            if (MultiColumnHeaderState.CanOverwriteSerializedFields(multiColumnHeaderState, headerState))
                MultiColumnHeaderState.OverwriteSerializedFields(multiColumnHeaderState, headerState);
            multiColumnHeaderState = headerState;

            treeView = new UIPanelTreeView(treeViewState, multiColumnHeaderState, this);
            treeView.SetData(panelPrefabs, searchFilter);
        }

        private List<BasePanel> GetAllPanelPrefabs()
        {
            var objs = YukiAssetDataBase.FindAssets<GameObject>();
            var panels = new List<BasePanel>(objs.Length);

            try
            {
                for (var i = 0; i < objs.Length; i++)
                {
                    EditorUtility.DisplayProgressBar("扫描预制体", $"正在扫描预制体... ({i}/{objs.Length})", i / (float)objs.Length);
                    var item = objs[i];
                    if (!item)
                        continue;

                    var panel = item.GetComponentInChildren<BasePanel>(true);
                    if (panel)
                        panels.Add(panel);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            return panels;
        }

        private void RefreshList()
        {
            panelPrefabs.Clear();
            panelPrefabs.AddRange(GetAllPanelPrefabs());
            EnsureTreeView();
            treeView.SetData(panelPrefabs, searchFilter);
        }

        private void OnGUI()
        {
            EnsureTreeView();

            DrawToolbar();

            var treeRect = GUILayoutUtility.GetRect(0f, 100000f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            treeView.OnGUI(treeRect);

            DrawFooter();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            var nextFilter = EditorGUILayout.TextField(
                searchFilter,
                EditorStyles.toolbarSearchField,
                GUILayout.MinWidth(240f),
                GUILayout.ExpandWidth(true));
            if (nextFilter != searchFilter)
            {
                searchFilter = nextFilter;
                treeView.SetData(panelPrefabs, searchFilter);
            }

            if (GUILayout.Button("刷新", EditorStyles.toolbarButton, GUILayout.Width(50)))
                RefreshList();

            if (GUILayout.Button("清除", EditorStyles.toolbarButton, GUILayout.Width(40)))
            {
                searchFilter = string.Empty;
                GUI.FocusControl(null);
                treeView.SetData(panelPrefabs, searchFilter);
            }

            EditorGUILayout.LabelField($"{panelPrefabs.Count} 个面板", EditorStyles.miniLabel, GUILayout.Width(72f));

            EditorGUILayout.EndHorizontal();
        }

        private void DrawFooter()
        {
            EditorGUILayout.Space(2f);

            var selected = treeView.GetSelectedPanel();
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            if (panelPrefabs.Count == 0)
            {
                EditorGUILayout.LabelField("点击「刷新」开始检索项目中的 UI 面板预制体。", EditorStyles.centeredGreyMiniLabel);
            }
            else if (selected == null)
            {
                EditorGUILayout.LabelField("选择一行以执行操作，双击可打开预制体。", EditorStyles.centeredGreyMiniLabel);
            }
            else
            {
                EditorGUILayout.LabelField(selected.name, EditorStyles.boldLabel, GUILayout.Width(140f));
                EditorGUILayout.LabelField(AssetDatabase.GetAssetPath(selected), EditorStyles.miniLabel);

                if (GUILayout.Button("Ping", GUILayout.Width(52f), GUILayout.Height(20f)))
                {
                    Selection.activeObject = selected;
                    EditorGUIUtility.PingObject(selected);
                }

                if (GUILayout.Button("打开", GUILayout.Width(52f), GUILayout.Height(20f)))
                    AssetDatabase.OpenAsset(selected);

                if (GUILayout.Button("定位", GUILayout.Width(52f), GUILayout.Height(20f)))
                    EditorUtility.RevealInFinder(AssetDatabase.GetAssetPath(selected));
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif
