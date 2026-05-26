using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace YukiFramework.AddressableExtension.Editor
{
    internal sealed class ResourcesConditionerEntryTreeView : TreeView
    {
        private List<ResConditionerScanner.ScannedEntry> entries = new List<ResConditionerScanner.ScannedEntry>();

        public ResourcesConditionerEntryTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader)
            : base(state, multiColumnHeader)
        {
            showBorder = true;
            showAlternatingRowBackgrounds = true;
            rowHeight = 20f;
            Reload();
        }

        public void SetEntries(List<ResConditionerScanner.ScannedEntry> newEntries)
        {
            entries = newEntries ?? new List<ResConditionerScanner.ScannedEntry>();
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };
            var items = new List<TreeViewItem>();
            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];
                items.Add(new TreeViewItem(i + 1, 0, entry.Address)
                {
                    icon = (Texture2D)EditorGUIUtility.IconContent("ScriptableObject Icon").image
                });
            }

            SetupParentsAndChildrenFromDepths(root, items);
            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var entry = entries[args.item.id - 1];
            for (var i = 0; i < args.GetNumVisibleColumns(); i++)
            {
                var cellRect = args.GetCellRect(i);
                CenterRectUsingSingleLineHeight(ref cellRect);
                var column = (EntryColumns)args.GetColumn(i);
                switch (column)
                {
                    case EntryColumns.Address:
                        GUI.Label(cellRect, entry.Address);
                        break;
                    case EntryColumns.ObjectName:
                        GUI.Label(cellRect, entry.ObjectName);
                        break;
                    case EntryColumns.Group:
                        GUI.Label(cellRect, entry.GroupName);
                        break;
                    case EntryColumns.AssetPath:
                        GUI.Label(cellRect, entry.AssetPath);
                        break;
                }
            }
        }

        protected override void DoubleClickedItem(int id)
        {
            if (id <= 0 || id > entries.Count)
                return;

            var assetPath = entries[id - 1].AssetPath;
            if (string.IsNullOrEmpty(assetPath))
                return;

            var obj = AssetDatabase.LoadMainAssetAtPath(assetPath);
            if (obj != null)
                EditorGUIUtility.PingObject(obj);
        }

        public static MultiColumnHeader CreateDefaultHeader()
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Address"),
                    width = 260,
                    minWidth = 120,
                    autoResize = true,
                    canSort = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("加载名"),
                    width = 120,
                    minWidth = 60,
                    autoResize = true,
                    canSort = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Group"),
                    width = 100,
                    minWidth = 60,
                    autoResize = true,
                    canSort = false
                },
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Asset Path"),
                    width = 280,
                    minWidth = 120,
                    autoResize = true,
                    canSort = false
                }
            };

            var headerState = new MultiColumnHeaderState(columns);
            return new MultiColumnHeader(headerState)
            {
                height = 24f,
                canSort = false
            };
        }

        private enum EntryColumns
        {
            Address = 0,
            ObjectName = 1,
            Group = 2,
            AssetPath = 3
        }
    }
}
