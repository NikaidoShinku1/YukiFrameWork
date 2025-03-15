///=====================================================
/// - FileName:      StateSelectParamListTree.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/13 19:30:46
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor.IMGUI.Controls;
namespace YukiFrameWork.Machine
{
    public class StateSelectParamListTree : TreeView
    {
        #region 字段

        private RuntimeStateMachineCore runtimeStateMachineCore;
        private StateConditionData condition;

        #endregion




        #region 重写方法
        protected override TreeViewItem BuildRoot()
        {

            TreeViewItem root = new TreeViewItem(-1, -1);

            if (runtimeStateMachineCore != null)
            {
                for (int i = 0; i < runtimeStateMachineCore.all_runtime_parameters.Count; i++)
                {
                    root.AddChild(new TreeViewItem(i, 0, runtimeStateMachineCore.all_runtime_parameters[i].parameterName));
                }
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

            string paramterName = FindItem(id, rootItem).displayName;

            StateParameterData p = runtimeStateMachineCore.GetParameterData(paramterName);

            if (p != null)
            {

                condition.parameterName = paramterName;

                switch (p.parameterType)
                {
                    case ParameterType.Float:
                    case ParameterType.Int:
                        condition.compareType = CompareType.Greater;
                        break;
                    case ParameterType.Bool:
                        condition.compareType = CompareType.Equal;
                        break;
                    case ParameterType.Trigger:
                        condition.compareType = CompareType.Equal;
                        condition.targetValue = 1;
                        break;
                }
            }
            else
            {
                Debug.LogErrorFormat("参数查询失败:{0}", paramterName);
            }

            // 保存
            runtimeStateMachineCore.Save();
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            base.RowGUI(args);

            if (args.label.Equals(condition.parameterName))
            {
                GUI.Label(args.rowRect, "√");
            }

        }

        #endregion

        #region 方法
        public StateSelectParamListTree(TreeViewState state, RuntimeStateMachineCore runtimeStateMachineCore, StateConditionData condition) : base(state)
        {
            this.runtimeStateMachineCore = runtimeStateMachineCore;
            this.condition = condition;

            showBorder = true;
            showAlternatingRowBackgrounds = true;
        }


        #endregion



    }
}
#endif