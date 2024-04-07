
using UnityEngine;


#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
namespace YukiFrameWork.States
{
    [CustomEditor(typeof(StateManager))] 
    public class StateManagerInspector : OdinEditor
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

        public static StateBase CreateStateNode(StateMechine stateMechine)
        {
            return StateNodeFactory.CreateStateNode(stateMechine, StateConst.entryState, new Rect(0, -100, StateConst.StateWith, StateConst.StateHeight));
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

                        CreateStateNode(stateMechine);
                    }
                    manager.stateMechine = stateMechine;
                }
            }
            else
            {                              
                if (GUILayout.Button("打开状态机编辑器",GUILayout.Height(40)))
                {
                    StateMechineEditorWindow.OpenWindow();
                }
            }
        }

        private bool DisabledGroup()
        {
            return Application.isPlaying;
        }

        protected override void OnHeaderGUI()
        {
            GUILayout.Label("StateManager");
        }
    }
}
#endif