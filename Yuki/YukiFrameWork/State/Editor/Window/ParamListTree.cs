using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;


namespace YukiFrameWork.States
{
    public class ParamListTree : TreeView
    {
        #region 字段
        private StateMechine stateMechine;
        private StateConditionData condition;
        #endregion
        public ParamListTree(TreeViewState state,StateMechine stateMechine,StateConditionData condition) : base(state)
        {
            this.condition = condition;
            this.stateMechine = stateMechine;

            //背景边框
            showBorder = true;
            //背景交替显示
            showAlternatingRowBackgrounds = true;
        }
        #region 重写方法
        protected override TreeViewItem BuildRoot()
        {
            TreeViewItem root = new TreeViewItem(-1, -1);

            if (stateMechine.parameters != null)
            {
                for (int i = 0; i < stateMechine.parameters.Count; i++)
                {
                    root.AddChild(new TreeViewItem(i, 0, stateMechine.parameters[i].name));
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

            string paramName = FindItem(id, rootItem).displayName;

            var paramter = stateMechine.parameters.Where(x => x.name.Equals(paramName)).FirstOrDefault();
            if (paramter != null)
            {
                condition.parameterName = paramName;

                switch (paramter.parameterType)
                {
                    case ParameterType.Float:
                        {

                        }
                        break;
                    case ParameterType.Int:
                        {
                            condition.compareType = CompareType.Greater;
                        }
                        break;
                    case ParameterType.Bool:
                        {
                            condition.compareType = CompareType.Equal;
                        }
                        break;
                }
            }
            else
            {
                Debug.LogErrorFormat("参数查询失败 ：{0}", paramName);
            }

            stateMechine.SaveToMechine();
        }
        /// <summary>
        /// 每一行绘制的回调
        /// </summary>
        /// <param name="args"></param>
        protected override void RowGUI(RowGUIArgs args)
        {
            base.RowGUI(args);

            if (args.label.Equals(condition.parameterName))
            {
                GUI.Label(args.rowRect, "√");
            }

            Repaint();
        }
        #endregion

        #region 方法
        #endregion
    }
}
