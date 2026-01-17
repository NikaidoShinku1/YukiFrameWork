#if UNITY_EDITOR
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace XFABManager
{

#if UNITY_6000_2_OR_NEWER 
    public class DragableTreeView<T> : TreeView<int> where T : class 
#else
    public class DragableTreeView<T> : TreeView where T : class
#endif 
    {
         
        #region 字段

        private ScriptableObject scriptableObject;

        private IList<T> datas = null;
          
        private float startDragTime = 0;

        //private IList<int> draggedItemIDs;

        private int dragCount = 0;

        private float lastRefreshTime;

        #endregion

        #region 构造函数


#if UNITY_6000_2_OR_NEWER 
        public DragableTreeView(ScriptableObject scriptableObject, IList<T> datas, TreeViewState<int> state) : base(state) 
#else
        public DragableTreeView(ScriptableObject scriptableObject, IList<T> datas, TreeViewState state) : base(state)
#endif 
        {
            this.scriptableObject = scriptableObject;
            this.datas = datas;

            if (datas == null)
                throw new System.Exception("datas 不能为空!");


            //getNewSelectionOverride = GetNewSelectionOverride; 
        }
#if UNITY_6000_2_OR_NEWER
        public DragableTreeView(ScriptableObject scriptableObject, IList<T> datas, TreeViewState<int> state, MultiColumnHeader header) : base(state, header)
#else
        public DragableTreeView(ScriptableObject scriptableObject, IList<T> datas, TreeViewState state, MultiColumnHeader header) : base(state, header)
#endif

        {
            this.scriptableObject = scriptableObject;
            this.datas = datas;

            if (datas == null)
                throw new System.Exception("datas 不能为空!");

            //getNewSelectionOverride = GetNewSelectionOverride;

             
        }

#endregion

        #region 重写方法
         
        public override void OnGUI(Rect rect)
        { 
            base.OnGUI(rect); 
            Refresh(); 
        }

#if UNITY_6000_2_OR_NEWER

        protected override IList<TreeViewItem<int>> BuildRows(TreeViewItem<int> root)
        {
            return base.BuildRows(root);
        }

#else

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            return base.BuildRows(root);
        }

#endif

#if UNITY_6000_2_OR_NEWER


        protected override TreeViewItem<int> BuildRoot()
        {
            TreeViewItem<int> root = new TreeViewItem<int>(0, -1);
            OnBuildRoot(root, datas);
            return root;
        }

#else

        protected override TreeViewItem BuildRoot()
        {
            TreeViewItem root = new TreeViewItem(0, -1);
            OnBuildRoot(root, datas);
            return root;
        }        

#endif


        protected override bool CanStartDrag(CanStartDragArgs args)
        {

            // 只有第一层的节点可以拖拽 因为这个可以拖拽的TreeView中的数据是List, 只有一层结构, 子节点无法参数到排序中
            // 如果子节点也希望参与排序 ，需要使用其他的数据结构 
            if (args.draggedItem.depth != 0)
                return true;

            DragAndDrop.SetGenericData(GetType().FullName, args.draggedItemIDs); 
            
            DragAndDrop.paths = null;
            DragAndDrop.StartDrag(string.Empty);
             
            //draggedItemIDs = args.draggedItemIDs;

            startDragTime = Time.realtimeSinceStartup;
            dragCount = 0;
            return base.CanStartDrag(args);
        }


        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        { 
            if (args.parentItem != rootItem)
                return DragAndDropVisualMode.Rejected;

            if (Event.current != null && Event.current.type == EventType.DragUpdated)
                dragCount++;

            if (Event.current != null && Event.current.type == EventType.DragUpdated && dragCount > 5)
            {
                IList<int> draggedItemIDs = DragAndDrop.GetGenericData(GetType().FullName) as IList<int>; 
                SetSelection(draggedItemIDs);
            }

            if (args.performDrop && args.insertAtIndex != -1 && Time.realtimeSinceStartup - startDragTime > 0.2f)
            {
                IList<int> selections = GetSelection();

                List<int> indexs = new List<int>();

                foreach (var id in selections)
                {
                    int index = GetItemIndex(id);
                    if (index == -1) continue;
                    indexs.Add(index);
                }

                // 需要移动的数据
                List<T> moves = new List<T>();

                foreach (var index in indexs)
                {
                    if (index < 0 || index >= datas.Count)
                        continue;

                    moves.Add(datas[index]);
                }

                // 临时数据
                List<T> temps = new List<T>();

                for (int i = 0; i < datas.Count; i++)
                {
                    if (moves.Contains(datas[i]))
                        temps.Add(null);
                    else
                        temps.Add(datas[i]);
                }

                for (int i = moves.Count - 1; i >= 0; i--)
                {
                    temps.Insert(args.insertAtIndex, moves[i]);
                }

                // 移除空的数据
                for (int i = temps.Count - 1; i >= 0; i--)
                {
                    if (temps[i] == null)
                        temps.RemoveAt(i);
                }

                datas.Clear();
                // 同步到原数据列表
                foreach (var item in temps)
                {
                    if (datas.Contains(item))
                        continue;
                    datas.Add(item);
                }

                Reload();
                if (scriptableObject != null)
                    EditorUtility.SetDirty(scriptableObject);

                DragAndDrop.SetGenericData(GetType().FullName, null); // 拖拽结束 清空数据 
            }

            return DragAndDropVisualMode.Link;
        }

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        { 
            base.SetupDragAndDrop(args);  
        }


#if UNITY_6000_2_OR_NEWER

        protected virtual void OnBuildRoot(TreeViewItem<int> root, IList<T> datas)
        {

        }

#else

        protected virtual void OnBuildRoot(TreeViewItem root, IList<T> datas)
        {

        }

#endif


        #endregion

        #region 方法

        private void Refresh() 
        {
            if (Time.realtimeSinceStartup - lastRefreshTime < 1) 
                return;
            lastRefreshTime = Time.realtimeSinceStartup;
            
            for (int i = 0; i < datas.Count; i++)
            {
                if (datas[i] == null)
                {
                    RefreshDatas();
                    break;
                }
            }

            if (rootItem.children != null)
            {
                if (rootItem.children.Count != datas.Count)
                {
                    // 数据列表 与 显示列表数量不一致 刷新一下View
                    RefreshDatas();
                }
            }
            else if (datas.Count > 0)
            {
                // 数据列表(不为0) 与 显示列表(为0)数量不一致 刷新一下View
                RefreshDatas();
            }
        }
         
        protected virtual void RefreshDatas()
        {
            // 如果全为空 不需要清空
            if (!IsAllEmpty()) 
            {
                // 清空为空的数据
                for (int i = datas.Count - 1; i >= 0; i--)
                {
                    if (datas[i] != null) continue;  
                    datas.RemoveAt(i);
                }
            }

            Reload();

            if (scriptableObject != null)
                EditorUtility.SetDirty(scriptableObject);
        }

        private int GetItemIndex(int id) 
        {

#if UNITY_6000_2_OR_NEWER

            TreeViewItem<int> item = FindItem(id, rootItem);

#else 
            TreeViewItem item = FindItem(id, rootItem);
#endif


            if (item == null) return -1;  
            return rootItem.children.IndexOf(item);
        }


        // 判断当前数据是否全为空
        private bool IsAllEmpty() 
        {
            if (datas == null) return true;

            for (int i = 0; i < datas.Count; i++)
            {
                if (datas[i] != null) 
                    return false;
            }

            return true;
        }
        
#endregion

    }


}

#endif