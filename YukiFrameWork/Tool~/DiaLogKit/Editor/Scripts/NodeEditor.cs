#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;
using YukiFrameWork.Extension;

namespace YukiFrameWork.DiaLog
{
    public class NodeEditor : EditorWindow
    {
        NodeTreeViewer nodeTreeViewer;
        InspectorViewer inspectorViewer;
        //顶部菜单栏层级命名
        [MenuItem("YukiFrameWork/DiaLogWindow")]
        public static void OpenWindow()
        {
            NodeEditor wnd = GetWindow<NodeEditor>();
            wnd.titleContent = new GUIContent("Yuki-对话树窗口");
        }
        // 双击资源文件时自动打开一个编辑器窗口
        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            if (Selection.activeObject is NodeTree)
            {
                OpenWindow();
                return true;
            }
            return false;
        }
        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            wantsMouseEnterLeaveWindow = true;
            wantsMouseMove = true;

            var nodeTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ImportSettingWindow.GetData().path + "/DiaLogKit/Editor/UI/NodeEditor.uxml");
            // 此处不使用visualTree.Instantiate() 为了保证行为树的单例防止重复实例化，以及需要将此root作为传参实时更新编辑器状态
            nodeTree.CloneTree(root);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ImportSettingWindow.GetData().path + "/DiaLogKit/Editor/UI/NodeEditor.uss");
            root.styleSheets.Add(styleSheet);

            // 将节点树视图添加到节点编辑器中
            nodeTreeViewer = root.Q<NodeTreeViewer>();
            // 将节属性面板视图添加到节点编辑器中
            inspectorViewer = root.Q<InspectorViewer>();
            // 关联节点选中方法
            nodeTreeViewer.OnNodeSelected = OnNodeSelectionChanged;
            OnSelectionChange();
        }
        private void OnSelectionChange()
        {
            // 检测该选中对象中是否存在节点树
            NodeTree tree = Selection.activeObject as NodeTree;
            // 判断如果选中对象不为节点树，则获取该对象下的节点树运行器中的节点树
            if (tree == null)
            {
                if (Selection.activeGameObject)
                {
                    DiaLogController runner = Selection.activeGameObject.GetComponent<DiaLogController>();
                    if (runner != null)
                    {
                        tree = runner.NodeTree;
                    }
                }
            }
            if (Application.isPlaying)
            {
                if (tree)
                {
                    if (nodeTreeViewer != null)
                    {
                        nodeTreeViewer.PopulateView(tree);
                    }
                }
            }
            else
            {
#if UNITY_2021_1_OR_NEWER
                if (tree && AssetDatabase.CanOpenAssetInEditor(tree.GetInstanceID()))
                {
#else
                if (tree)
                {
#endif
                    if (nodeTreeViewer != null)
                    {
                        nodeTreeViewer.PopulateView(tree);
                    }
                }
            }
        }
        void OnNodeSelectionChanged(NodeView node)
        {
            inspectorViewer?.UpdateSelection(node);
        }
        private void OnInspectorUpdate()
        {
            nodeTreeViewer?.UpdateNodeStates();

        }
    }
}
#endif