using System;
using System.Linq;

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
            window.titleContent = new GUIContent("对话编辑器窗口");
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
                    Debug.LogWarning("请对节点进行检查，是否标记了多个节点类型特性，如确实标记，请只留下一个特性，一个Node类只能归为一种类型,复数的情况下，往往优先查找到的第一个特性 --- Multiple attributes are not allowed Node Type:" + node.GetType());
                }
            }
        }

        private BackGroundView backGroundView;
        private InspectorView inspectorView;
        private DropdownField dropdownField;
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
            dropdownField = root.Q<DropdownField>("RuntimeNode");

            dropdownField.RegisterValueChangedCallback(OnDropDownFieldChanged);
            RepaintRuntimeTree();
        }

        private void OnDropDownFieldChanged(ChangeEvent<string> evt)
        {
            if (evt.newValue == "None") return;

            if (DiaLogKit.DiaLogs.ContainsKey(evt.newValue))
            {
                nodeTree = DiaLogKit.DiaLogs[evt.newValue].tree;
                objectField.value =  DiaLogKit.DiaLogs[evt.newValue].tree;
                if (backGroundView != null)
                    backGroundView.Pop(nodeTree);
            }
        }

        private void RepaintRuntimeTree()
        {
            if (!Application.isPlaying)
            {
                dropdownField.choices = new System.Collections.Generic.List<string>() { "None" };
                dropdownField.index = 0;
            }
            else
            {
                dropdownField.choices = DiaLogKit.DiaLogs.Keys.ToList();
            }
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