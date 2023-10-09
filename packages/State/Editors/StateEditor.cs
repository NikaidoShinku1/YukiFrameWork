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
            if (GUILayout.Button("��״̬���༭��"))
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
                        GUILayout.Label("״̬��" + $"{item.name}");
                        item.name = EditorGUILayout.TextField("״̬���ƣ�", item.name);
                        item.index = EditorGUILayout.IntField("״̬��ʶ��", item.index);
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
                                    animType = "�ɰ�";
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
                                    animType = "�°�";
                                    item.animation = null;
                                }
                                break;
                        }
                        GUILayout.Label($"�������ͣ�{animType}");

                        item.type = (AnimType)EditorGUILayout.EnumPopup(item.type, GUILayout.Width(180));

                        EditorGUILayout.EndHorizontal();

                        item.isNextState = EditorGUILayout.Toggle("�Ƿ��Զ�������һ��״̬", item.isNextState);

                        if (item.isNextState)
                        {
                            item.nextStateID = EditorGUILayout.IntField("��һ��״̬�ı�ʶ", item.nextStateID);
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
                                if (GUILayout.Button("ɾ���ű�"))
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
        /// ��ɾ״̬
        /// </summary>
        private void RecomposeBehaviour(State state)
        {            
            if (!isRecomposeScript)
            {
                if (GUILayout.Button("���״̬�ű�"))
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

                if (GUILayout.Button("ȡ��"))
                {
                    isRecomposeScript = false;
                }

            }
           
        }        
       

    }
}
#endif
