///=====================================================
/// - FileName:      StateListTree.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/13 22:56:30
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
namespace YukiFrameWork.Machine
{
    public class StateTreeViewItem : TreeViewItem
    {
        public MonoScript MonoScript { get; private set; }


        private Type _type = null;

        public Type Type
        {
            get
            {

                if (_type == null)
                    _type = MonoScript.GetClass();
                return _type;
            }
        }

        public StateTreeViewItem(int id, int depth, string displayName, MonoScript script) : base(id, depth, displayName)
        {
            MonoScript = script;
        }




    }


    public class FSMStateListTree : TreeView
    {

        // Fix编码
        #region 字段

        private RuntimeStateMachineCore controller;
        //private FSMConditionData condition;
        private GUIStyle style = new GUIStyle("label");

        private StateNodeData nodeData = null;

        private SelectStateWindow editorWindow = null;

        private List<int> selections = new List<int>();
        #endregion




        #region 重写方法
        protected override TreeViewItem BuildRoot()
        {

            TreeViewItem root = new TreeViewItem(-1, -1);         
            MonoScript[] scripts = YukiAssetDataBase.FindAssets<MonoScript>();
            for (int i = 0; i < scripts.Length; i++)
            {
                Type type = scripts[i].GetClass();

                if (type == null) continue;
                if (!type.IsSubclassOf(typeof(StateBehaviour)) || type.IsAbstract) continue;
                string displayName = type.Name;
                StateTreeViewItem item = new StateTreeViewItem(i, 0, displayName, scripts[i]);
                root.AddChild(item);
            }

            return root;
        }

        protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
        {
            return base.BuildRows(root);
        }

        protected override void SingleClickedItem(int id)
        {
            base.SingleClickedItem(id);

            StateTreeViewItem item = FindItem(id, rootItem) as StateTreeViewItem;

            if (item != null)
            {
                try
                {
                    nodeData.AddStateScript(item.MonoScript);
                }
                catch (Exception e)
                {
                    editorWindow.editorWindow.ShowNotification(new GUIContent(e.Message));
                    return;
                }
            }
            // 保存
            controller.Save();
            editorWindow.Close();
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            //base.RowGUI(args);

            StateTreeViewItem item = args.item as StateTreeViewItem;
            if (item == null) return;
            Type type = item.Type;
            if (type == null) return;

            if (args.rowRect.Contains(Event.current.mousePosition))
            {
                selections.Clear();
                selections.Add(item.id);
                this.SetSelection(selections);
            }


            Rect rect = new Rect();
            rect.Set(args.rowRect.x + 20, args.rowRect.y, args.rowRect.width - 20, args.rowRect.height);

            GUI.Label(new Rect(0, args.rowRect.y - 2, 20, 20), EditorGUIUtility.IconContent("d_cs Script Icon"));

            if (string.IsNullOrEmpty(type.Namespace))
            {
                GUI.Label(rect, type.Name);
            }
            else
            {
                style.richText = true;
                GUI.Label(rect, string.Format("{0}<color=#A4A4A4>({1})</color>", type.Name, type.Namespace), style);
            }

            
        }

        protected override void SelectionChanged(IList<int> selectedIds)
        {
            base.SelectionChanged(selectedIds);
        }

        protected override bool CanMultiSelect(TreeViewItem item)
        {
            return false;
        }

        #endregion

        #region 方法
        public FSMStateListTree(TreeViewState state, RuntimeStateMachineCore controller, StateNodeData nodeData, SelectStateWindow editorWindow) : base(state)
        {
            this.controller = controller;

            showBorder = true;
            showAlternatingRowBackgrounds = true;
            this.nodeData = nodeData;
            this.editorWindow = editorWindow;
        }


        #endregion
    }


}
#endif