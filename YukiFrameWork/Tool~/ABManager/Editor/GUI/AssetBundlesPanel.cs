using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;


namespace YukiFrameWork.XFABManager
{

    public class AssetBundlesPanel
    {

        const int padding = 2; // 内边距
        const int search_field_height = 20;

        private Rect position;


        private TreeViewState bundleListState;
        private TreeViewState bundleInfoState;
        private TreeViewState assetListState;

        MultiColumnHeaderState m_AssetListMCHState;

        private BundleListTree bundleListTree;      // AssetBundle 列表
        private BundleInfoTree bundleInfoTree;      // AssetBundle 信息
        private AssetListTree assetListTree;        // AssetBundle 内 资源 的列表
        //private AssetInfoList assetInfoTree;        // AssetBundle 内 资源 的信息

        private Rect bundleListRect;
        private Rect bundleInfoRect;
        private Rect assetListRect;
 

        private Rect searchFieldRect;

        private Rect bundleSearchRect;

        private Rect abCountRect;

        private float HorizontalSplitPercent = 0.4f;
        private float VerticalLeftSplitPercent = 0.7f;
 
        private Rect HorizontalSplitRect;       // 水平分割的矩形
        private Rect VerticalLeftSplitRect;     // 水平分割的矩形
 

        private bool isResizingHorizontal;      // 是不是正在调整 水平的比例
        //private bool isResizingVerticalRight;   // 是不是正在调整 竖直右边的比例
        private bool isResizingVerticalLeft;    // 是不是正在调整 竖直左边的比例

        private XFAssetBundleProjectMain window;

        private SearchField searchField;
        private SearchField bundleSerchField;

        public AssetBundlesPanel() {

            searchField = new SearchField();
            bundleSerchField = new SearchField();
        }

        public void OnGUI(Rect position, XFAssetBundleProjectMain window)
        {
            this.window = window;
            this.position = position;

            if (bundleListTree == null)
            {
                // 创建

                if (bundleListState == null)
                {
                    bundleListState = new TreeViewState();
                }

                bundleListTree = new BundleListTree(bundleListState, window,this );
                bundleListTree.Reload();

                if (bundleInfoState == null)
                {
                    bundleInfoState = new TreeViewState();
                }

                bundleInfoTree = new BundleInfoTree(bundleInfoState,window);
                bundleInfoTree.Reload();
                if (assetListState == null)
                {
                    assetListState = new TreeViewState();
                }

                var headerState = AssetListTree.CreateDefaultMultiColumnHeaderState();// multiColumnTreeViewRect.width);
                if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_AssetListMCHState, headerState))
                    MultiColumnHeaderState.OverwriteSerializedFields(m_AssetListMCHState, headerState);
                m_AssetListMCHState = headerState;


                //m_AssetList = new AssetListTree(m_AssetListState, m_AssetListMCHState, this);

                assetListTree = new AssetListTree(assetListState, m_AssetListMCHState, window.Project, this);
                assetListTree.Reload();

            }

            bundleSearchRect.Set(position.x + padding, position.y,
                (position.width * HorizontalSplitPercent - padding) * 0.7f, search_field_height);
            OnGUIBundleSerchFiled(bundleSearchRect);

            abCountRect.Set(position.x + padding + (position.width * HorizontalSplitPercent - padding) * 0.7f + 10, 
                position.y,
                (position.width * HorizontalSplitPercent - padding) * 0.3f - 10, search_field_height);
            //OnGUIBundleSerchFiled(bundleSearchRect);
            GUI.Label(abCountRect, string.Format("数量: {0}", window.Project.assetBundles.Count));

            bundleListRect.Set(position.x + padding,
                position.y + search_field_height,
                position.width * HorizontalSplitPercent - padding,
                position.height * VerticalLeftSplitPercent - search_field_height);
            bundleListTree.OnGUI(bundleListRect);

            


            assetListRect.Set(position.x + position.width * HorizontalSplitPercent + padding,
                position.y + search_field_height,
                position.width * (1 - HorizontalSplitPercent) - padding * 2,
                position.height - padding - search_field_height);

            searchFieldRect.Set(position.x + position.width * HorizontalSplitPercent + padding,
                position.y,
                position.width * (1 - HorizontalSplitPercent) - padding * 2,
               search_field_height);

            OnGUISearchBar(searchFieldRect);
            assetListTree.OnGUI(assetListRect);

            bundleInfoRect.Set(position.x + padding,
                position.y + position.height * VerticalLeftSplitPercent + padding,
                position.width * HorizontalSplitPercent - padding,
                position.height * (1 - VerticalLeftSplitPercent) - padding * 2);
            bundleInfoTree.OnGUI(bundleInfoRect);

            VerticalResize();
            HorizontalResize();

            //GUILayout.Label(Input.mousePosition.ToString());


            if (isResizingHorizontal || isResizingVerticalLeft)
            {
                this.window.Repaint();
            }
        }

        // 调整水平的尺寸
        private void HorizontalResize()
        {
            HorizontalSplitRect.Set(position.width * HorizontalSplitPercent, position.y, padding, position.height);


            EditorGUIUtility.AddCursorRect(HorizontalSplitRect, MouseCursor.ResizeHorizontal);
            if (Event.current.type == EventType.MouseDown && HorizontalSplitRect.Contains(Event.current.mousePosition))
            {
                isResizingHorizontal = true;
            }
            //m_ResizingHorizontalSplitter = true;

            if (isResizingHorizontal)
            {

                HorizontalSplitPercent = Mathf.Clamp(Event.current.mousePosition.x / position.width, 0.1f, 0.9f);
            }

            if (Event.current.type == EventType.MouseUp)
            {
                isResizingHorizontal = false;
            }




        }

        // 调整竖直的尺寸
        private void VerticalResize()
        {



            VerticalLeftSplitRect.Set(position.x, position.y + position.height * VerticalLeftSplitPercent, position.width * HorizontalSplitPercent - padding, padding);

            EditorGUIUtility.AddCursorRect(VerticalLeftSplitRect, MouseCursor.ResizeVertical);
            if (Event.current.type == EventType.MouseDown && VerticalLeftSplitRect.Contains(Event.current.mousePosition))
            {
                isResizingVerticalLeft = true;
            }

            //VerticalRightSplitRect.Set(position.x + position.width * HorizontalSplitPercent + padding,
            //    position.y + position.height * VerticalRightSplitPercent,
            //    position.width * (1 - HorizontalSplitPercent), padding);
            //EditorGUIUtility.AddCursorRect(VerticalRightSplitRect, MouseCursor.ResizeVertical);
            //if (Event.current.type == EventType.MouseDown && VerticalRightSplitRect.Contains(Event.current.mousePosition))
            //{
            //    isResizingVerticalRight = true;
            //}



            //if (isResizingVerticalRight)
            //{
            //    //VerticalRightSplitPercent = Mathf.Clamp((Event.current.mousePosition.y - position.y) / position.height, 0.2f, 0.95f);

            //}
            //else 
            if (isResizingVerticalLeft)
            {
                VerticalLeftSplitPercent = Mathf.Clamp((Event.current.mousePosition.y - position.y) / position.height, 0.25f, 0.9f);

            }


            if (Event.current.type == EventType.MouseUp)
            {
                isResizingVerticalLeft = false;
                //isResizingVerticalRight = false;
            }
        }

        // 更新选择的AssetBundle 
        public void UpdateSelectBundle(string bundle_name)
        {
            assetListTree.SetSelectBundle(bundle_name);
            bundleInfoTree.SetSelectBundle(bundle_name);
        }


        public void UpdateSelectBundle(List<Object> selectedObjects) {
            if (selectedObjects.Count > 1) return;
            bundleInfoTree.SetSelectAssets(AssetDatabase.GetAssetPath(selectedObjects[0]));
        }

        void OnGUISearchBar(Rect rect)
        {
            assetListTree.searchString = searchField.OnGUI(rect, assetListTree.searchString);
            
        }

        void OnGUIBundleSerchFiled(Rect rect) {
            bundleListTree.searchString =  bundleSerchField.OnGUI(rect, bundleListTree.searchString);
        }


    }

}