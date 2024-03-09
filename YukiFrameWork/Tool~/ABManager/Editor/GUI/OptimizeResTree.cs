using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace XFABManager
{
    public class OptimizeResTree : TreeView
    {

        private Dictionary<string, List<string>> data;

        //public OptimizeResTree(TreeViewState state, Dictionary<string, List<string>> data) : base(state)
        //{
        //    showBorder = true;
        //    showAlternatingRowBackgrounds = true;
        //    this.data = data;
        //}

        public OptimizeResTree(TreeViewState state, MultiColumnHeaderState mchs, Dictionary<string, List<string>> data) : base(state, new MultiColumnHeader(mchs))
        {
            showBorder = true;
            showAlternatingRowBackgrounds = true;
            this.data = data;
            //this.project = project;
            //multiColumnHeader.sortingChanged += OnSortingChanged;
        }

        protected override TreeViewItem BuildRoot()
        {
            return CreateItem();
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            return base.BuildRows(root);
        }

        // 名称 引用次数 大小 路径 AssetBundles

        // 创建 item 
        private TreeViewItem CreateItem() {

            TreeViewItem root = new TreeViewItem(0, -1);

            if (data != null)
            {
                
                foreach (var item in data.Keys)
                {
                    root.AddChild( new TreeViewItem(item.GetHashCode(),0,item) );
                }

            }


            return root;
        }

        internal static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
        {
            return new MultiColumnHeaderState(GetColumns());
        }
        private static MultiColumnHeaderState.Column[] GetColumns()
        {
            var retVal = new MultiColumnHeaderState.Column[] {
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column()
            };
            retVal[0].headerContent = new GUIContent("名称", "");
            retVal[0].minWidth = 50;
            retVal[0].width = 150;
            retVal[0].maxWidth = 300;
            retVal[0].headerTextAlignment = TextAlignment.Left;
            retVal[0].canSort = true;
            retVal[0].autoResize = true;

            retVal[1].headerContent = new GUIContent("AssetBundles", "Errors, Warnings, or Info");
            retVal[1].minWidth = 30;
            retVal[1].width = 300;
            retVal[1].maxWidth = 1000;
            retVal[1].headerTextAlignment = TextAlignment.Left;
            retVal[1].canSort = false;
            retVal[1].autoResize = true;

            retVal[2].headerContent = new GUIContent("Size", "文件大小!");
            retVal[2].minWidth = 30;
            retVal[2].width = 100;
            retVal[2].maxWidth = 200;
            retVal[2].headerTextAlignment = TextAlignment.Left;
            retVal[2].canSort = false;
            retVal[2].autoResize = true;

            return retVal;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
                CellGUI(args.GetCellRect(i), args.item , args.GetColumn(i), ref args);
        }

        private void CellGUI(Rect cellRect, TreeViewItem item, int column, ref RowGUIArgs args)
        {
            Color oldColor = GUI.color;
            CenterRectUsingSingleLineHeight(ref cellRect);

            switch (column)
            {
                case 0:
                    {
                        var iconRect = new Rect(cellRect.x + 1 , cellRect.y + 1, cellRect.height - 2, cellRect.height - 2);

                        if ( item.icon == null ) {
                            item.icon = AssetDatabase.GetCachedIcon(item.displayName) as Texture2D;
                        }

                        if (item.icon != null)
                            GUI.DrawTexture(iconRect, item.icon, ScaleMode.ScaleToFit);

                        DefaultGUI.Label(
                            new Rect(cellRect.x + iconRect.xMax + 1, cellRect.y, cellRect.width - iconRect.width, cellRect.height),
                            Path.GetFileName( item.displayName ),
                            args.selected,
                            args.focused);
                    }
                    break;
                case 1:
                     DefaultGUI.Label(cellRect, ListToString( data[item.displayName] ) , args.selected, args.focused);
                    break;
                case 2:
                    System.IO.FileInfo fileInfo = new System.IO.FileInfo(item.displayName);
                    string size = string.Empty;
                    if (fileInfo.Exists)
                    {
                        size = EditorUtility.FormatBytes(fileInfo.Length);
                    }
                    else {
                        size = EditorUtility.FormatBytes(0);
                    }
                    DefaultGUI.Label(cellRect, size, args.selected, args.focused);
                    break;

            }
            
        }

        protected override void DoubleClickedItem(int id)
        {
            var assetItem = FindItem(id, rootItem);
            if (assetItem != null)
            {
                Object o = AssetDatabase.LoadAssetAtPath<Object>(assetItem.displayName);
                EditorGUIUtility.PingObject(o);
                Selection.activeObject = o;
            }
        }

        private string ListToString(List<string> list) {

            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < list.Count; i++)
            {
                stringBuilder.Append(list[i]);
                if ( i != list.Count - 1 ) {
                    stringBuilder.Append(",");
                }
            }

 
            return stringBuilder.ToString();
        }


        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds == null)
                return;

            List<Object> selectedObjects = new List<Object>();
            //List<AssetBundleModel.AssetInfo> selectedAssets = new List<AssetBundleModel.AssetInfo>();
            foreach (var id in selectedIds)
            {
                var assetItem = FindItem(id, rootItem);
                if (assetItem != null)
                {
                    Object o = AssetDatabase.LoadAssetAtPath<Object>(assetItem.displayName);
                    selectedObjects.Add(o);
                    Selection.activeObject = o;
                    //selectedAssets.Add(assetItem.asset);
                }
            }
            //m_Controller.SetSelectedItems(selectedAssets);
            Selection.objects = selectedObjects.ToArray();
        }

    }

}

