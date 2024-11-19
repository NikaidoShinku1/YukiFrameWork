#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.Experimental.GraphView;
using UnityEditor.PackageManager.UI;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using YukiFrameWork.Extension;
namespace YukiFrameWork.Behaviours
{
    public class BehaviourTreeGraphWindow : EditorWindow
    {       
        private VisualElement root => rootVisualElement;
        public void CreateGUI()
        {
            
            var viewTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ImportSettingWindow.GetData().path +"/BehaviourTree/Editor/GUI/BehaviourTreeGraphWindow.uxml");
            viewTree.CloneTree(root);
            inspectorView = root.Q<InspectorView>("InspectorView");
            backGroundView = root.Q<BackGroundView>(nameof(BackGroundView));         
            backGroundView.onNodeSelected += v =>
            {
                inspectorView.drawIndex = 0;
                inspectorView.Update_InspectorView(v);
            };
            onValidate += () => 
            {
                backGroundView.Init(this);
                backGroundView.Refresh(behaviourTree);
                AssetDatabase.SaveAssets();
            };
            objectField = root.Q<ObjectField>("behaviourTree");
            objectField.RegisterValueChangedCallback(OnBehaviourChanged);
            objectField.value = behaviourTree;
            objectField.objectType = typeof(BehaviourTreeSO);
           
            Button repaintBtn = root.Q<Button>("repaint");
            Button InspectorBtn = root.Q<Button>(nameof(InspectorBtn));
            Button serializableBtn = root.Q<Button>("SerializaBtn");
            Button debuggerBtn = root.Q<Button>("DebuggerBtn");
            InspectorBtn.clicked += () => 
            {
                if (inspectorView.drawIndex == 0) return;
                inspectorView.drawIndex = 0;
                inspectorView.Clear();
                foreach (var item in backGroundView.nodes) 
                {
                    if (item is GraphBehaviourView view)
                    {
                        if (view.selected)
                            inspectorView.Update_InspectorView(view);
                    }
                }
            };
            serializableBtn.clicked += () =>
            {
                inspectorView.drawIndex = 1;
                inspectorView.Update_ParamView(behaviourTree);
                
            };

            repaintBtn.clicked += () => 
            {               
                onValidate?.Invoke();                
                Repaint();
            };

            debuggerBtn.clicked += () => 
            {
                if (inspectorView.drawIndex == 2) return;
                inspectorView.drawIndex = 2;
                inspectorView.Update_DebuggerView(objectField);

            };
            if (Application.isPlaying)
            {
                if (inspectorView.drawIndex == 2) return;
                inspectorView.drawIndex = 2;
                inspectorView.Update_DebuggerView(objectField);
            }
            //backGroundView.Init(this);

        }      
        private void OnEnable()
        {
            Undo.undoRedoPerformed += () => 
            {
                backGroundView.Refresh(behaviourTree);
            };                     
        }
        private void OnInspectorUpdate()
        {
            Repaint();
        }
        private void OnBehaviourChanged(ChangeEvent<UnityEngine.Object> evt)
        {          
            behaviourTree = evt.newValue as BehaviourTreeSO;
            if (behaviourTree != null)
            {
                onValidate?.Invoke();               
            }
            else
            {
                backGroundView.Init(this);
                backGroundView.Refresh(behaviourTree);
            }
            if(inspectorView.drawIndex != 2)
            inspectorView.Clear();

        }

        public static event System.Action onUpdate;
        public static event System.Action onValidate;
        private void Update()
        {
            onUpdate?.Invoke();

            if (!objectField.value)
            {
                objectField.value = null;
                
            }
        }
        private BackGroundView backGroundView;
        private InspectorView inspectorView;
        internal ObjectField objectField;
        public BehaviourTreeSO behaviourTree;
        public static void ShowExample(BehaviourTreeSO behaviourTree)
        {
            BehaviourTreeGraphWindow window = GetWindow<BehaviourTreeGraphWindow>();
            window.titleContent = new GUIContent("ÐÐÎªÊ÷±à¼­Æ÷´°¿Ú");
            window.behaviourTree = behaviourTree; 
            window.objectField.value = behaviourTree;          
            window.backGroundView.Init(window);
            window.backGroundView.Refresh(behaviourTree); 
        }

        [OnOpenAsset(1)]
        public static bool OnOpenAssets(int id, int line)
        {
            if (EditorUtility.InstanceIDToObject(id) is BehaviourTreeSO tree)
            {
                ShowExample(tree);

                return true;
            }

            return false;
        }

        private void OnDestroy()
        {
            AssetDatabase.SaveAssets();
        }
    }
}
#endif