using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.PackageManager.UI;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using YukiFrameWork.Extension;
namespace YukiFrameWork.DiaLogue
{
    public class DiaLogGraphWindow : EditorWindow
    {
        //[MenuItem("YukiFrameWork/DiaLogGraphWindow")]
        public static void ShowExample(NodeTree tree)
        {
            DiaLogGraphWindow window = GetWindow<DiaLogGraphWindow>();
            window.titleContent = new GUIContent("�Ի��༭������");
            window.nodeTree = tree;
            window.objectField.value = tree;
            //Label titleLab = window.rootVisualElement.Q<Label>("Tre")

            // window.graph
            window.backGroundView.Pop(tree);
        }



        [OnOpenAsset(1)]
        public static bool OnOpenAssets(int id, int line)
        {
            if (EditorUtility.InstanceIDToObject(id) is NodeTree tree)
            {
                ShowExample(tree);
              
                return true;
            }

            return false;
        }

        public static event Action onUpdate = null;


        private void OnInspectorUpdate()
        {
            Repaint();
        }
        public static void CloseWindow() 
        {
            DiaLogGraphWindow window = GetWindow<DiaLogGraphWindow>();
            window.Close();
        }

        private void Update()
        {
            onUpdate?.Invoke();
        }

        private void OnEnable()
        {
            if (nodeTree == null) return;

            foreach (var node in nodeTree.nodes)
            {
                if (node.AttributeCount > 1)
                {
                    Debug.LogWarning("��Խڵ���м�飬�Ƿ����˶���ڵ��������ԣ���ȷʵ��ǣ���ֻ����һ�����ԣ�һ��Node��ֻ�ܹ�Ϊһ������,����������£��������Ȳ��ҵ��ĵ�һ������ --- Multiple attributes are not allowed Node Type:" + node.GetType());
                }
            }
        }

        private BackGroundView backGroundView;
        private InspectorView inspectorView;
        private ObjectField objectField;
        private VisualElement root => rootVisualElement;

        internal static event Action OnValidate = null;
        /*internal NodeTree nodeTree
        {
            get
            {
                try
                {
                    return objectField.value as NodeTree;
                }
                catch 
                {
                    return null;
                }
            }
        }*/

        private NodeTree nodeTree;

        public void CreateGUI()
        {
            var viewTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ImportSettingWindow.GetData().path + "/DiaLogKit/Editor/DiaLogGraphWindow.uxml");

            viewTree.CloneTree(root);
            inspectorView = root.Q<InspectorView>(nameof(InspectorView));
            backGroundView = root.Q<BackGroundView>(nameof(BackGroundView));            
            backGroundView.onNodeSelected += delegate (GraphNodeView node) { inspectorView.Update_InspectorView(node); };          

            Button saveBtn = root.Q<Button>("SaveButton");

            saveBtn.clicked += delegate 
            {
                OnValidate?.Invoke();
                Repaint();
                AssetDatabase.SaveAssets(); 
            };

            objectField = root.Q<ObjectField>("NodeTreeField");
            objectField.objectType = typeof(NodeTree);
            objectField.value = nodeTree;
            backGroundView.Pop(nodeTree);
            objectField.RegisterValueChangedCallback(OnNodeTreeChanged);         
          
        }        
        private void OnNodeTreeChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            nodeTree = evt.newValue as NodeTree;
            if (nodeTree != null)
                OnValidate?.Invoke();
        }

        private void OnDestroy()
        {
            AssetDatabase.SaveAssets();
        }
    }
}
#endif