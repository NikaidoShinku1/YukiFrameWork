using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using XFABManager;

namespace XFABManager
{



    internal enum SortOption
    {
        Asset = 0,
        Size = 2
    }

    public class AssetListTree : TreeView
    {

        #region 字段

        private XFABProject project;
        private string bundle_name;

        private int item_offset_x;

        private List<string> dir_assets_list = new List<string>();

        private GUIContent filterContent = new GUIContent();

        private AssetBundlesPanel bundlesPanel;

        #endregion

        #region 属性
        private XFABAssetBundle Bundle
        {

            get
            {
                return project.GetAssetBundle(bundle_name);
            }
        }

        #endregion

        #region 重写方法

        internal static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
        {
            return new MultiColumnHeaderState(GetColumns());
        }
        private static MultiColumnHeaderState.Column[] GetColumns()
        {
            var retVal = new MultiColumnHeaderState.Column[] {
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                new MultiColumnHeaderState.Column(),
                //new MultiColumnHeaderState.Column()
            };
            retVal[0].headerContent = new GUIContent("Asset", "Short name of asset. For full name select asset and see message below");
            retVal[0].minWidth = 50;
            retVal[0].width = 200;
            retVal[0].maxWidth = 1000;
            retVal[0].headerTextAlignment = TextAlignment.Left;
            retVal[0].canSort = false;
            retVal[0].autoResize = true;

            retVal[1].headerContent = new GUIContent("Bundle", "Bundle name. 'auto' means asset was pulled in due to dependency");
            retVal[1].minWidth = 50;
            retVal[1].width = 200;
            retVal[1].maxWidth = 1000;
            retVal[1].headerTextAlignment = TextAlignment.Left;
            retVal[1].canSort = false;
            retVal[1].autoResize = true;

            //retVal[2].headerContent = new GUIContent("Size", "Size on disk");
            //retVal[2].minWidth = 30;
            //retVal[2].width = 75;
            //retVal[2].maxWidth = 100;
            //retVal[2].headerTextAlignment = TextAlignment.Left;
            //retVal[2].canSort = false;
            //retVal[2].autoResize = true;

            retVal[2].headerContent = new GUIContent("FullPath", "Errors, Warnings, or Info");
            retVal[2].minWidth = 30;
            retVal[2].width = 200;
            retVal[2].maxWidth = 1000;
            retVal[2].headerTextAlignment = TextAlignment.Left;
            retVal[2].canSort = false;
            retVal[2].autoResize = true;

            retVal[3].headerContent = new GUIContent("Filter", "打包该文件夹下指定类型文件,默认全都打包!");
            retVal[3].minWidth = 30;
            retVal[3].width = 150;
            retVal[3].maxWidth = 1000;
            retVal[3].headerTextAlignment = TextAlignment.Left;
            retVal[3].canSort = false;
            retVal[3].autoResize = true;

            return retVal;
        }
        protected override TreeViewItem BuildRoot()
        {
            return CreateView();
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            return base.BuildRows(root);
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i)
                CellGUI(args.GetCellRect(i), args.item as AssetTreeItem, args.GetColumn(i), ref args);
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {

            if (IsValidDragDrop())
            {
                if (args.performDrop)
                {
                    for (int i = 0; i < DragAndDrop.paths.Length; i++)
                    {
                        Debug.Log("添加文件:"+ DragAndDrop.paths[i]);
                        Bundle.AddFile(DragAndDrop.paths[i]);
                    }
                    // 保存
                    project.Save();
                    Reload();
                }

                return DragAndDropVisualMode.Copy;
            }
            return DragAndDropVisualMode.None;
        }


        protected override void SelectionChanged(IList<int> selectedIds)
        {
            if (selectedIds == null)
                return;

            List<Object> selectedObjects = new List<Object>();
            //List<AssetBundleModel.AssetInfo> selectedAssets = new List<AssetBundleModel.AssetInfo>();
            foreach (var id in selectedIds)
            {
                var assetItem = FindItem(id, rootItem) as AssetTreeItem;
                if (assetItem != null)
                {
                    Object o = AssetDatabase.LoadAssetAtPath<Object>(assetItem.FileInfo.AssetPath);
                    selectedObjects.Add(o);
                    Selection.activeObject = o;
                    //selectedAssets.Add(assetItem.asset);
                }
            }
            //m_Controller.SetSelectedItems(selectedAssets);
            Selection.objects = selectedObjects.ToArray();
            this.bundlesPanel.UpdateSelectBundle(selectedObjects);
        }


        protected override void DoubleClickedItem(int id)
        {
            var assetItem = FindItem(id, rootItem) as AssetTreeItem;
            if (assetItem != null)
            {
                Object o = AssetDatabase.LoadAssetAtPath<Object>(assetItem.FileInfo.AssetPath);
                EditorGUIUtility.PingObject(o);
                Selection.activeObject = o;
            }
        }

        protected override void ContextClickedItem(int id)
        {
            List<AssetTreeItem> selectedNodes = new List< AssetTreeItem>();
            foreach (var nodeID in GetSelection())
            {
                selectedNodes.Add(FindItem(nodeID, rootItem) as AssetTreeItem);
            }

            if (selectedNodes.Count > 0)
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Remove asset(s) from bundle."), false, RemoveAssets, selectedNodes);
                menu.ShowAsContext();
            }

        }

        #endregion


        #region 方法


        public AssetListTree(TreeViewState state, MultiColumnHeaderState mchs, XFABProject project, AssetBundlesPanel bundlesPanel) : base(state, new MultiColumnHeader(mchs))
        {
            showBorder = true;
            showAlternatingRowBackgrounds = true;
            this.project = project;
            multiColumnHeader.sortingChanged += OnSortingChanged;
            this.bundlesPanel = bundlesPanel;
        }

        private void CellGUI(Rect cellRect, AssetTreeItem item, int column, ref RowGUIArgs args)
        {
            Color oldColor = GUI.color;
            CenterRectUsingSingleLineHeight(ref cellRect);
            //if (item.depth == 1)
            //    GUI.color = Color.gray;

            switch (column)
            {
                case 0:
                    {

                        item_offset_x = item.depth * 15;

                        if (item.FileInfo.type == XFBundleFileType.Directory)
                        {
                            item_offset_x += 15;
                        }
                         
                        var iconRect = new Rect(cellRect.x + 1 + item_offset_x, cellRect.y + 1, cellRect.height - 2, cellRect.height - 2);
                        if (item.icon != null)
                            GUI.DrawTexture(iconRect, item.icon, ScaleMode.ScaleToFit);

                        DefaultGUI.Label(
                            new Rect(cellRect.x + iconRect.xMax + 1, cellRect.y, cellRect.width - iconRect.width - item_offset_x, cellRect.height),
                            item.FileInfo.displayName,
                            args.selected,
                            args.focused);
                    }
                    break;
                case 1:
                    DefaultGUI.Label(cellRect, bundle_name, args.selected, args.focused);
                    break;
                //case 2:
                //    DefaultGUI.Label(cellRect, item.FileInfo.SizeString, args.selected, args.focused);
                    //break;
                case 2:
                    DefaultGUI.Label(cellRect, item.FileInfo.AssetPath, args.selected, args.focused);
                    break;
                case 3:

                    if (item.FileInfo.type == XFBundleFileType.Directory && item.depth == 0)
                    { 

                        string filter = string.IsNullOrEmpty(item.FileInfo.filter) ? "All" : item.FileInfo.filter;

                        filterContent.text = filter;
                        
                        if (EditorGUI.DropdownButton(cellRect, filterContent, FocusType.Passive, EditorStyles.toolbarDropDown))
                        {
                            var menu = new GenericMenu();

                            foreach (var type in FileInfo.FilterTypes)
                            {
                                menu.AddItem(new GUIContent(type), type == filter, () =>
                                {
                                    item.FileInfo.filter = type;
                                    // 刷新
                                    Reload();
                                });
                            }
                             
                            menu.DropDown(cellRect);
                        }


                    }
                     
                    break;
            }
            GUI.color = oldColor;
        }

        public void SetSelectBundle(string bundle_name)
        {
            this.bundle_name = bundle_name;
            Reload();
        }

        private TreeViewItem CreateView()
        {

            AssetTreeItem root = new AssetTreeItem();

            if (Bundle != null)
            {
                List<XFABManager.FileInfo> files = Bundle.GetFileInfos();
                for (int i = 0; i < files.Count; i++)
                {
                    AssetTreeItem child = new AssetTreeItem(files[i]);

                    if (files[i].type == XFBundleFileType.Directory)
                    {
                        // 如果是目录要判断目录下有没有内容 如果没有内容 则不需要添加
                        AddDirectory(child, files[i]);
                    }

                    root.AddChild(child);
                }
            }

            return root;
        }

        public void AddDirectory(AssetTreeItem root, FileInfo fileInfo)
        {
            string[] files = fileInfo.Files;
            for (int i = 0; i < files.Length; i++)
            {
                if (AssetDatabase.IsValidFolder(files[i]))
                    continue;

                AssetTreeItem child = new AssetTreeItem(new XFABManager.FileInfo(AssetDatabase.AssetPathToGUID(files[i])), root.depth + 1);
                root.AddChild(child);
            }
        }

        void OnSortingChanged(MultiColumnHeader multiColumnHeader)
        {
            //SortIfNeeded(rootItem, GetRows());
            IList<TreeViewItem> rows = GetRows();
            if (rows.Count <= 1) // 不需要排序
                return;

            if (multiColumnHeader.sortedColumnIndex == -1)
                return;

            SortByColumn();

            rows.Clear();
            for (int i = 0; i < rootItem.children.Count; i++)
                rows.Add(rootItem.children[i]);

            Repaint();

        }

        void SortByColumn()
        {
            var sortedColumns = multiColumnHeader.state.sortedColumns;

            if (sortedColumns.Length == 0)
                return;

            List<AssetTreeItem> assetList = new List<AssetTreeItem>();
            foreach (var item in rootItem.children)
            {
                assetList.Add(item as AssetTreeItem);
            }


            var orderedItems = InitialOrder(assetList, sortedColumns);

            rootItem.children = orderedItems.Cast<TreeViewItem>().ToList();
        }

        IOrderedEnumerable<AssetTreeItem> InitialOrder(IEnumerable<AssetTreeItem> myTypes, int[] columnList)
        {
            SortOption sortOption = (SortOption)columnList[0];
            bool ascending = multiColumnHeader.IsSortedAscending(columnList[0]);
            switch (sortOption)
            {
                case SortOption.Asset:
                    if (ascending)
                    {
                        return myTypes.OrderBy(l => l.displayName);
                    }
                    else
                    {
                        return myTypes.OrderByDescending(l => l.displayName);
                    }
                //return myTypes.Order(l => l.displayName, ascending);
                //case SortOption.Size:

                    //if (ascending)
                    //{
                    //    return myTypes.OrderBy(l => l.FileInfo.Size);
                    //}
                    //else
                    //{
                    //    return myTypes.OrderByDescending(l => l.FileInfo.Size);
                    //}
            }
            return null;
        }

        // 删除资源 
        void RemoveAssets(object obj)
        {
            List<AssetTreeItem> assets = (List<AssetTreeItem>)obj;
            for (int i = 0; i < assets.Count; i++)
            {
                Debug.Log(" 移除资源! " + assets[i].FileInfo.AssetPath);
                Bundle.RemoveFile(assets[i].FileInfo.guid);
            }
            project.Save();
            Reload();
            //var selectedNodes = obj as List<AssetBundleModel.AssetTreeItem>;
            //var assets = new List<AssetBundleModel.AssetInfo>();
            ////var bundles = new List<AssetBundleModel.BundleInfo>();
            //foreach (var node in selectedNodes)
            //{
            //    if (!System.String.IsNullOrEmpty(node.asset.bundleName))
            //        assets.Add(node.asset);
            //}
            //AssetBundleModel.Model.MoveAssetToBundle(assets, string.Empty, string.Empty);
            //AssetBundleModel.Model.ExecuteAssetMove();
            //foreach (var bundle in m_SourceBundles)
            //{
            //    bundle.RefreshAssetList();
            //}
            //m_Controller.UpdateSelectedBundles(m_SourceBundles);
            //ReloadAndSelect(new List<int>());
        }

        // 是不是能够拖拽
        protected bool IsValidDragDrop()
        { 
            //can't drag nothing
            if (DragAndDrop.paths == null || DragAndDrop.paths.Length == 0)
                return false;

            // bundle_name 为空的时候 不能拖放
            if (string.IsNullOrEmpty(bundle_name))  return false; 

            if (Bundle == null || Bundle.bundleType == XFBundleType.Group) return false;

            for (int i = 0; i < DragAndDrop.paths.Length; i++)
            {
                // 是无效的AssetBundle文件 并且不是目录
                if (!AssetBundleTools.IsValidAssetBundleFile(DragAndDrop.paths[i]) && AssetDatabase.IsValidFolder(DragAndDrop.paths[i]) == false) { return false; }
            }
             
            return true; 
        }



        #endregion
    }
}