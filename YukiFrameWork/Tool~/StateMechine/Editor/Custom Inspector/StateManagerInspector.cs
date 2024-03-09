using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YukiFrameWork.Extension;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
namespace YukiFrameWork.States
{
    [CustomEditor(typeof(StateManager))] 
    public class StateManagerInspector : CustomInspectorEditor
    {
        [MenuItem("GameObject/YukiFrameWork/Create State-Object")]
        private static void Created()
        {           
            GameObject gameObject = Selection.activeGameObject;
            gameObject ??= new GameObject("State Object");
            gameObject.AddComponent<StateManager>();
            Undo.RegisterCreatedObjectUndo(gameObject,"Create");

            EditorUtility.SetDirty(gameObject);
            AssetDatabase.SaveAssets();

        }

        public override void OnInspectorGUI()
        {           
            StateManager manager = (StateManager)target;

            if (manager == null) return;
#if UNITY_2021_1_OR_NEWER
            Texture2D icon = EditorGUIUtility.IconContent("d_TerrainInspector.TerrainToolSettings On").image as Texture2D;
            EditorGUIUtility.SetIconForObject(manager, icon);
#endif
            base.OnInspectorGUI();

            CreateMechine(manager);
        }

        private void CreateMechine(StateManager manager)
        {
            bool IsMechineExist = manager.stateMechine != null;
            if (!IsMechineExist)
            {
                if (GUILayout.Button("创建状态机", GUILayout.Height(40)))
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
                if (GUILayout.Button("打开状态机编辑器",GUILayout.Height(40)))
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
#endif