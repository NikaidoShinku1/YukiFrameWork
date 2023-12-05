using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace YukiFrameWork.States
{
    [CustomEditor(typeof(StateManager))]
    public class StateManagerInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            StateManager manager = (StateManager)target;

            if (manager == null) return;

            EditorGUI.BeginDisabledGroup(DisabledGroup());
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("状态机初始化方式：");
            manager.initType = (InitType)EditorGUILayout.EnumPopup(manager.initType);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("是否开启调试日志：");
            manager.deBugLog = (DeBugLog)EditorGUILayout.EnumPopup(manager.deBugLog);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            CreateMechine(manager);

            EditorGUI.EndDisabledGroup();
        }

        private void CreateMechine(StateManager manager)
        {
            bool IsMechineExist = manager.stateMechine != null;
            if (!IsMechineExist)
            {
                if (GUILayout.Button("创建状态机"))
                {
                    StateMechine stateMechine = manager.GetComponentInChildren<StateMechine>();

                    if (stateMechine == null)
                    {
                        stateMechine = new GameObject(typeof(StateMechine).Name).AddComponent<StateMechine>();

                        stateMechine.transform.SetParent(manager.transform);                     
                       
                        StateNodeFactory.CreateStateNode(stateMechine, StateConst.entryState, new Rect(0, -100, StateConst.StateWith, StateConst.StateHeight));                       
                    }
                    manager.stateMechine = stateMechine;
                }
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("状态机本体：");

                manager.stateMechine =(StateMechine)EditorGUILayout.ObjectField(manager.stateMechine, typeof(StateMechine), true);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space();
                if (GUILayout.Button("打开状态机编辑器"))
                {
                    StateMechineEditor.OpenWindow();
                }
            }
        }

        private bool DisabledGroup()
        {
            return Application.isPlaying;
        }
    }
}