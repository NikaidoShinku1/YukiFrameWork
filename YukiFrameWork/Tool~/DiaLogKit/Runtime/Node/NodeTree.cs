using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Collections;
using System;
using YukiFrameWork.Extension;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif
using Sirenix.OdinInspector;

namespace YukiFrameWork.DiaLogue
{
  
    public abstract class NodeTreeBase : ScriptableObject
    {
        [SerializeField,LabelText("对话树的唯一标识")]
        [InfoBox("根据该标识通过DiaLogKit加载")]
        private string key;

        public string Key => key;
        public abstract IEnumerable<INode> Nodes { get; }
        internal abstract Type NodeType { get; }

        [LabelText("对话控制器绑定类型"),SerializeField]
        [InfoBox("选择继承DiaLogController的类")]
        [ValueDropdown(nameof(allControllerTypes))]
        private string controllerType;

        private Type runtime_ControllerType;

        /// <summary>
        /// 运行时控制器类型
        /// </summary>
        public Type RuntimeControllerType
        {
            get
            {
                if (runtime_ControllerType == null)
                    runtime_ControllerType = AssemblyHelper.GetType(ControllerType);
                return runtime_ControllerType;
            }
        }

        #region EditorNodeColor
       
        [SerializeField,LabelText("设置根节点的编辑器颜色")]
        internal Color rootColorTip = Color.red;

        [SerializeField, LabelText("不同类型节点的编辑器颜色")]
        [InfoBox("该配置全局共享,所有的NodeTree配置都共享该设置,根节点以根节点颜色设置为优先")]
        [TableList]
        internal List<ColorTip> allNodeColorTypeTips = new List<ColorTip>()
        {
            new ColorTip()
            {
                key = "SingleNode",
                color = Color.black
                
            },
            new ColorTip()
            { 
                key = "SelectNode",
                color = Color.cyan,
            },
            new ColorTip()
            {
                key = "RandomNode",
                color = Color.grey
            }

        };
        [Serializable]
        public class ColorTip
        {
            [LabelText("节点类型")]
            public string key;
            [LabelText("节点颜色")]
            public Color color;
        }

        [SerializeField,LabelText("性能模式")]
        [InfoBox("开启后变更颜色与文本同步不会实时更新，需要手动点击编辑器的刷新脚本")]
        internal bool IsPerformance;
        
        #endregion


        #region Controller

        /// <summary>
        /// 控制器类型
        /// </summary>
        public string ControllerType => controllerType;

        IEnumerable allControllerTypes => AssemblyHelper
            .GetTypes(x => x.IsSubclassOf(typeof(DiaLogController)) && !x.IsAbstract && !x.IsInterface)
            .Select(x => new ValueDropdownItem() { Text = x.ToString(), Value = x.ToString()});
        #endregion

        public abstract void ForEach(Action<INode> each);
#if UNITY_EDITOR
        internal static IEnumerable AllColorTypes => YukiAssetDataBase.FindAssets<NodeTreeBase>()
            .Select(x => x.allNodeColorTypeTips)
            .SelectMany(x => x)
            .Select(x => new ValueDropdownItem() { Text = x.key, Value = x.key });
        public abstract INode DeleteNode(INode node);
        public abstract INode CreateNode(System.Type type);
#endif
    }

    public abstract class NodeTreeBase<T> : NodeTreeBase, IExcelSyncScriptableObject where T :ScriptableObject,INode
    { 
        // 所有对话内容的存储列表
        [LabelText("所有对话内容的存储列表"), SerializeField, HideInInspector]
        private List<T> nodes = new List<T>();
        internal override Type NodeType => typeof(T);
        private IEnumerable<INode> runtimeNodes;
        public override IEnumerable<INode> Nodes
        {
            get
            {
                if (runtimeNodes == null)
                    runtimeNodes = nodes.Select(x => x as INode);
                return runtimeNodes;
            }
        }
        public IList Array => nodes;

        public override void ForEach(Action<INode> each)
        {
            foreach (var item in nodes)
            {
                each?.Invoke(item as INode);
            }
        }

        public Type ImportType => typeof(T);

        public bool ScriptableObjectConfigImport => false;

        public void Completed()
        {
            
        }

        public void Create(int maxLength)
        {
#if UNITY_EDITOR
            while (nodes.Count > 0)            
                DeleteNode(nodes[nodes.Count - 1]);
#endif
            nodes.Clear();
        }

        public void Import(int index, object userData)
        {
            T item = (T)userData;
            nodes.Add(item);
#if UNITY_EDITOR
            AssetDatabase.AddObjectToAsset(item, this);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif
        }

#if UNITY_EDITOR


        [Sirenix.OdinInspector.FilePath(Extensions = "xlsx"), PropertySpace(50), LabelText("Excel路径")]
        public string excelPath;
        [Button("导出Excel"), HorizontalGroup("Excel")]
        void CreateExcel()
        {
            if (excelPath.IsNullOrEmpty() || !System.IO.File.Exists(excelPath))
                throw new NullReferenceException("路径为空或不存在!");
            if (SerializationTool.ScriptableObjectToExcel(this, excelPath, out string error))
                Debug.Log("导出成功");
            else throw new Exception(error);
        }
        [Button("导入Excel"), HorizontalGroup("Excel")]
        void ImportExcel()
        {
            if (excelPath.IsNullOrEmpty() || !System.IO.File.Exists(excelPath))
                throw new NullReferenceException("路径为空或不存在!");
            if (SerializationTool.ExcelToScriptableObject(excelPath, 3, this))
            {
                Debug.Log("导入成功");
            }
        }
        [Button("Edit Graph", ButtonHeight = 30), PropertySpace(20)]
        void OpenWindow()
        {
            DiaLogGraphWindow.ShowExample(this);
        }
        public override INode CreateNode(System.Type type)
        {
            T node = ScriptableObject.CreateInstance(type) as T;
           
            if (nodes.Count == 0)
                node.IsRoot = true;
            //node.GUID = GUID.Generate().ToString();
            int id = (1000) + nodes.Count + 1;
            node.Id = id;
            
            // node.nodeType = type.FullName;
            while (nodes.Any(x => x.Id == id))
            {
                node.Id = ++id;
            }
            node.Name = "编号:" + id.ToString();
            Undo.RecordObject(this, "Node Tree (CreateNode)");

            nodes.Add(node);     
            Undo.RegisterCreatedObjectUndo(node, "Node Tree (CreateNode)");
            AssetDatabase.AddObjectToAsset(node,this);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();

            return node;
        }
        public override INode DeleteNode(INode node)
        {
            Undo.RecordObject(this, "Node Tree (DeleteNode)");
            nodes.Remove(node as T);
          
            Undo.DestroyObjectImmediate(node as T);
            AssetDatabase.SaveAssets();
            return node;
        }
#endif

    }

    [CreateAssetMenu(fileName = "DiaLogNodeTree", menuName = "YukiFrameWork/DiaLogNodeTree")]
    public class NodeTree :  NodeTreeBase<Node>
    {             
             

    }
}
