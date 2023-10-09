using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

#if UNITY_EDITOR
namespace YukiFrameWork.States
{
    [CustomEditor(typeof(StateManager))]
    public class StateEditor : Editor
    {
        private StateManager stateManager;
        
        private bool isRecomposeScript = false;             

        private void OnEnable()
        {           
            stateManager = (StateManager)target;
            
        }
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            OpenStateGraphEditor();

            Statusdetails();          
        }

        private void OpenStateGraphEditor()
        {
            GUILayout.Space(20);
            if (GUILayout.Button("打开状态机编辑器"))
            {
                StateMechineEditor.StateMechineEditorShow(stateManager);
            }
        }           

        private void Statusdetails()
        {
            GUILayout.Space(30);
            if (stateManager.controllerID != -1 && stateManager.stateMechine != null)
                foreach (var item in stateManager.stateMechine.states)
                {
                    if (item.index == stateManager.controllerID)
                    {
                        GUILayout.Label("状态：" + $"{item.name}");
                        item.name = EditorGUILayout.TextField("状态名称：", item.name);
                        item.index = EditorGUILayout.IntField("状态标识：", item.index);
                        EditorGUILayout.BeginHorizontal();
                        string animType = "";
                        switch (item.type)
                        {
                            case AnimType.None:
                                break;
                            case AnimType.Animation:
                                {
                                    if (item.animation == null)
                                        item.animation = stateManager.GetComponent<Animation>();
                                    if (item.animation == null)
                                    {
                                        item.animation = stateManager.GetComponentInChildren<Animation>();
                                    }
                                    item.animation = (Animation)EditorGUILayout.ObjectField(item.animation, typeof(Animation), true);
                                    animType = "旧版";
                                    item.animator = null;
                                }
                                break;
                            case AnimType.Animator:
                                {
                                    if (item.animator == null)
                                        item.animator = stateManager.GetComponent<Animator>();
                                    if (item.animator == null)
                                    {
                                        item.animator = stateManager.GetComponentInChildren<Animator>();
                                    }
                                    item.animator = (Animator)EditorGUILayout.ObjectField(item.animator, typeof(Animator), true);
                                    animType = "新版";
                                    item.animation = null;
                                }
                                break;
                        }
                        GUILayout.Label($"动画类型：{animType}");

                        item.type = (AnimType)EditorGUILayout.EnumPopup(item.type, GUILayout.Width(180));

                        EditorGUILayout.EndHorizontal();

                        item.isNextState = EditorGUILayout.Toggle("是否自动进入下一个状态", item.isNextState);

                        if (item.isNextState)
                        {
                            item.nextStateID = EditorGUILayout.IntField("下一个状态的标识", item.nextStateID);
                        }
                        else item.nextStateID = -1;

                        GUILayout.Space(20);
                        if (item.behaviours.Count > 0)
                        {
                            for (int i = 0; i < item.behaviours.Count; i++)
                            {
                                EditorGUILayout.Space(10);
                                EditorGUILayout.BeginHorizontal();
                                item.behaviours[i].IsActive = EditorGUILayout.ToggleLeft(item.behaviours[i].name, item.behaviours[i].IsActive);
                                if (GUILayout.Button("删除脚本"))
                                {
                                    item.behaviours.RemoveAt(i);
                                    continue;
                                }
                                EditorGUILayout.EndHorizontal();

                            }
                        }
                        RecomposeBehaviour(item);
                    }

                }
            Repaint();
        }

        /// <summary>
        /// 增删状态
        /// </summary>
        private void RecomposeBehaviour(State state)
        {            
            if (!isRecomposeScript)
            {
                if (GUILayout.Button("添加状态脚本"))
                {
                    isRecomposeScript = true;
                    Repaint();
                    AssetDatabase.Refresh();
                }
            }
            else
            {
                Type type = typeof(StateBehaviour);
                Type[] types = Assembly.GetAssembly(type).GetTypes();

                foreach (var infoType in types)
                {
                    if (infoType.BaseType == type)
                    {
                        if (GUILayout.Button(infoType.Name))
                        {                            
                            StateBehaviour obj = Activator.CreateInstance(infoType) as StateBehaviour;

                            if (state.behaviours.Count > 0)
                            {
                                if (state.behaviours.Find(x => x.GetType() == infoType) != null)
                                {
                                    isRecomposeScript = false;
                                    return;                                   
                                }
                            }
                            
                            obj.name = infoType.ToString();
                            obj.ID = state.index;
                            state.AddBehaviours(obj);
                            isRecomposeScript = false;
                            Repaint();
                            AssetDatabase.Refresh();

                        }
                    }
                }

                if (GUILayout.Button("取消"))
                {
                    isRecomposeScript = false;
                }

            }
           
        }        
       

    }
}
#endif
