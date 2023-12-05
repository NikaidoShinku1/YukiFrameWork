using Codice.CM.Common;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;


namespace YukiFrameWork.States
{
    [CustomEditor(typeof(TransitionInspectorHelper))]
    public class TransitionInspector : Editor
    {
        private ReorderableList reorderableList;

        private Rect condition_left_rect;
        private Rect condition_right_rect;
        private Rect popRect;

        private GUIContent tempContent = new GUIContent();

        private static Dictionary<ParameterType, ConditionInspector> conditionInspectorDict = new Dictionary<ParameterType, ConditionInspector>();
        private static void InitConditionInspectors()
        {
            if (conditionInspectorDict.Count == 0)
            {
                conditionInspectorDict.Add(ParameterType.Bool, new StateBoolCondition());
                conditionInspectorDict.Add(ParameterType.Int, new StateIntCondition());
                conditionInspectorDict.Add(ParameterType.Float, new StateFloatCondition());
            }
        }
        private void OnEnable()
        {
            TransitionInspectorHelper helper = target as TransitionInspectorHelper;
            if (helper == null) return;

            reorderableList = new ReorderableList(helper.transition.conditions,typeof(StateConditionData),true,true,true,true);
            reorderableList.onAddCallback += OnAddCallBack;
            reorderableList.onRemoveCallback += OnRemoveCallBack;
            reorderableList.drawElementCallback += DrawItem;
        }
        public override void OnInspectorGUI()
        {
            TransitionInspectorHelper helper = target as TransitionInspectorHelper;
            if (helper == null) return;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("过渡条件方式：");
            helper.transition.transitionMode = (TransitionMode)EditorGUILayout.EnumPopup(helper.transition.transitionMode);
            EditorGUILayout.EndHorizontal();

            switch (helper.transition.transitionMode)
            {
                case TransitionMode.动画剪辑模式:
                    EditorGUILayout.Space();
                    GUILayout.Label("该模式下需要当前过渡的初始状态的动画模式不为None。\n并且设置了默认动画，初始状态为：" + helper.transition.fromStateName);
                    break;
                case TransitionMode.定时模式:
                    EditorGUILayout.Space();
                    GUILayout.Label("该模式下状态定时，单位为秒,到达时间后退出该状态(必须要与其他状态拥有过渡，否则会出问题");
                    EditorGUILayout.BeginHorizontal();

                    GUILayout.Label("定时时间");
                    helper.transition.stateLoadTime = EditorGUILayout.FloatField(helper.transition.stateLoadTime);
                    EditorGUILayout.EndHorizontal();
                    break;
                case TransitionMode.有限条件模式:
                    {
                        reorderableList.list = helper.transition.conditions;

                        //绘制类似动画状态机的参数设置
                        reorderableList.DoLayoutList();
                    }
                    break;               
            }
           
        }

        protected override void OnHeaderGUI()
        {
            TransitionInspectorHelper helper = target as TransitionInspectorHelper;
            if (helper == null) return;

            GUILayout.BeginHorizontal();

            GUILayout.Label(EditorGUIUtility.IconContent("icons/processed/unityeditor/animations/animatorstate icon.asset"), GUILayout.Width(30), GUILayout.Height(30));
            GUILayout.Label($"{helper.transition.fromStateName} -> {helper.transition.toStateName}");

            GUILayout.EndHorizontal();

            //画一条分割线

            var rect = EditorGUILayout.BeginHorizontal();

            Handles.color = Color.black;
            Handles.DrawLine(new Vector2(rect.x, rect.y), new Vector2(rect.x + rect.width, rect.y));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();


        }

        private void OnAddCallBack(ReorderableList list)
        {
            TransitionInspectorHelper helper = target as TransitionInspectorHelper;
            if (helper == null) return;

            StateConditionDataFactory.CreateCondition(helper.stateMechine, helper.transition);
        }

        private void OnRemoveCallBack(ReorderableList list)
        {
            TransitionInspectorHelper helper = target as TransitionInspectorHelper;
            if (helper == null) return;

            StateConditionDataFactory.RemoveCondition(helper.stateMechine, helper.transition, list.index);
        }

        private void DrawItem(Rect rect, int index, bool isActive, bool isFocused)
        {
            TransitionInspectorHelper helper = target as TransitionInspectorHelper;
            if (helper == null) return;

            var condition = helper.transition.conditions[index];

            condition_left_rect.Set(rect.x, rect.y, rect.width / 2, rect.height);
            condition_right_rect.Set(rect.x + rect.width / 2, rect.y, rect.width / 2, rect.height);

            if (helper.stateMechine.parameters.Count > 0)
            {
                tempContent.text = condition.parameterName;
                if (EditorGUI.DropdownButton(condition_left_rect, tempContent, FocusType.Keyboard))
                {
                    popRect.Set(rect.x, rect.y + 2, rect.width / 2, rect.height);
                    PopupWindow.Show(popRect,new StateSelectParamEditor(rect.width / 2,condition,helper.stateMechine));
                }
            }

            InitConditionInspectors();

            var parameter = helper.stateMechine.parameters.Where(x => x.name.Equals(condition.parameterName)).FirstOrDefault();
            if (parameter == null)
            {
                EditorGUI.LabelField(condition_right_rect, "缺少参数！");
            }
            else
            {
                //根据不同的参数类型绘制不同的内容

                if (conditionInspectorDict.ContainsKey(parameter.parameterType))
                {
                    conditionInspectorDict[parameter.parameterType].OnGUI(condition_right_rect, condition, helper.stateMechine);
                }
                else
                {
                    Debug.LogError(string.Format("未查询到对应绘制方式！-> {0}",parameter.parameterType));
                }
            }
        }       
    }
}