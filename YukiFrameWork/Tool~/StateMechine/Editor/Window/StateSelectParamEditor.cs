#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
#endif
using UnityEngine;
#if UNITY_EDITOR
namespace YukiFrameWork.States
{
    public class StateSelectParamEditor : PopupWindowContent
    {
        #region 字段
        private float width;
        private StateConditionData condition;
        private StateMechine stateMechine;

        //搜索框标签
        private Rect labelRect;      

        private SearchField searchField;
        private Rect searchRect;

        //参数列表
        private ParamListTree paramTree;
        private TreeViewState viewState;
        private Rect paramRect;

        private const float searchHeight = 25f;
        private const float labelHeight = 30f;
        #endregion

        public StateSelectParamEditor(float width,StateConditionData condition,StateMechine stateMechine)
        {
            this.width = width;
            this.condition = condition;
            this.stateMechine = stateMechine;
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(this.width, 120);
        }

        public override void OnGUI(Rect rect)
        {  //参数列表

            if (paramTree == null)
            {
                if (viewState == null)
                {
                    viewState = new TreeViewState();
                }
                paramTree = new ParamListTree(viewState, stateMechine, condition);
                paramTree.Reload();
            }
            if (searchField == null)
                searchField = new SearchField();

            searchRect.Set(rect.x + 5, rect.y + 5, rect.width - 10, searchHeight);

            //绘制搜索框
            paramTree.searchString = searchField.OnGUI(searchRect,paramTree.searchString);

            labelRect.Set(rect.x, rect.y + searchHeight, rect.width, labelHeight);
            EditorGUI.LabelField(labelRect,condition.parameterName,GUI.skin.GetStyle("AC BoldHeader"));          
            paramRect.Set(rect.x, rect.y + searchHeight + labelHeight - 5, rect.width, rect.height - searchHeight - labelHeight);

            paramTree.OnGUI(paramRect);
        }

        
    }
}
#endif