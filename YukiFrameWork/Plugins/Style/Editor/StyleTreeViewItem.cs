///=====================================================
/// - FileName:      StyleTreeViewItem.cs
/// - NameSpace:     YukiFrameWork.UI
/// - Created:       Yuki
/// - Email:         1274672030@qq.com
/// - Description:   这是一个框架工具创建的脚本
/// - Creation Time: 2024/1/16 15:47:42
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

#if UNITY_EDITOR
using UnityEngine;
using System;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace YukiFrameWork.Extension
{
    public class StyleTreeViewItem<T> : TreeViewItem
    {
        public T item { get; protected set; }

        public StyleTreeViewItem(T item, int id, int depth = 0)
        {
            this.item = item;
            this.id = id;
            this.depth = depth;
        }
    }

    public abstract class StyleTreeView<T> : TreeView
    {
        private int nextItemIndex;

        protected TreeViewItem itemRoot { get; private set; }

        protected List<TreeViewItem> items { get; private set; }

        public StyleTreeView(TreeViewState state) : base(state) => OnInit();

        public StyleTreeView(TreeViewState state, MultiColumnHeader mHeader) : base(state, mHeader) => OnInit();

        protected virtual void OnInit()
        {
            itemRoot = new TreeViewItem(-1, -1);
            nextItemIndex = 0;
            items ??= new List<TreeViewItem>();          
            items?.Clear();         
        }

        public void InitTreeViewItems(IEnumerable<StyleTreeViewItem<T>> mItems, bool reload = true)
        {
            items ??= new List<TreeViewItem>();           
            items?.Clear();

            int count = mItems == null ? 0 : mItems.Count();

            if (count > 0)
            {
                IEnumerator enumerator = mItems.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    items.Add(enumerator.Current as StyleTreeViewItem<T>);
                }
            }

            if (reload) Reload();
        }

        public void AddTreeViewItem(StyleTreeViewItem<T> item, bool reload = true)
        {
            items.Add(item);
            if(reload) Reload();
        }

        public StyleTreeViewItem<T> AddTreeViewItem(T t, bool reload = true)
        {
            StyleTreeViewItem<T> item = new StyleTreeViewItem<T>(t, GetIndex());

            items.Add(item);

            if (reload) Reload();
            return item;
        }

        public int ItemCount => items.Count;

        protected override TreeViewItem BuildRoot()
        {          
            SetupParentsAndChildrenFromDepths(itemRoot, items);
            return itemRoot;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item as StyleTreeViewItem<T>;

            if (multiColumnHeader == null)
            {
                CellGUI(args.rowRect, item, 0);
            }
            else
            {
                int visableColumns = args.GetNumVisibleColumns();

                if (visableColumns > 0)
                {
                    for (int i = 0; i < visableColumns; i++)
                    {
                        CellGUI(args.GetCellRect(i), item, i);
                    }
                }
            }

            base.RowGUI(args);
        }

        protected abstract void CellGUI(Rect rowRect, StyleTreeViewItem<T> item, int index);
      
        public int GetIndex()
        {
            int currentIndex = nextItemIndex;
            nextItemIndex++;
            return currentIndex;
        }

        protected StyleTreeViewItem<T> FindStyleTreeViewItem(int index) 
        {
            TreeViewItem item = FindItem(index, rootItem);

            if(item == null)
                return null;
            return item as StyleTreeViewItem<T>;
        }

        protected virtual void OnClickItem(StyleTreeViewItem<T> item) { }

        protected void SerlizationInfo(string value)
        {
            TextEditor textEditor = new TextEditor();

            textEditor.text = value;

            textEditor.OnFocus();

            textEditor.Copy();
        }
    }
}
#endif