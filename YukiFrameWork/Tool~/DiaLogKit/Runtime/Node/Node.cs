using Newtonsoft.Json;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using YukiFrameWork.Extension;
using System.Collections;

namespace YukiFrameWork.DiaLogue
{
    public interface INode
    {
        /// <summary>
        /// 节点的唯一Id
        /// </summary>
        int Id { get; set; }
        /// <summary>
        /// 节点是否作为根节点使用
        /// </summary>
        bool IsRoot { get; set; }
        /// <summary>
        /// 节点的名称
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// 节点的文本
        /// </summary>
        string Text { get; set; }
        /// <summary>
        /// 节点的图标
        /// </summary>
        Sprite Icon { get; set; }
        /// <summary>
        /// 节点的类型
        /// </summary>
        string NodeType { get; set; }
        /// <summary>
        /// 节点的连接
        /// </summary>
        List<int> LinkNodes { get; }
#if UNITY_EDITOR
        Node.Position NodePosition { get; set; }       
#endif
    }
        
    public class Node : ScriptableObject, INode
    {
        [SerializeField,LabelText("唯一标识")]
        private int id;
        [SerializeField,LabelText("是否作为根节点使用")]
        [InfoBox("当运行时对话控制器没有传递指定的开始标识时，则以默认节点为主,此时一个对话树至少需要存在一个根节点。"),ReadOnly]
        private bool isRoot;
        [SerializeField,LabelText("节点名称")]
        private string _name;
        [SerializeField,LabelText("节点文本")]
        [TextArea]
        private string text;
        [SerializeField,LabelText("节点类型")]
#if UNITY_EDITOR
        [ValueDropdown(nameof(nodeTypes),SortDropdownItems = true)]
#endif
        [HideIf(nameof(isRoot))]
        private string nodeType;
#if UNITY_EDITOR
        [CustomValueDrawer(nameof(DrawPreview)),SerializeField,LabelText("节点的图标")]
#endif
        private Sprite icon;
        [SerializeField,LabelText("节点连接端口")]
        [ReadOnly]
        private List<int> linkNodes = new List<int>();
        [ExcelIgnore]public int Id { get => id; set => id = value; }
        [ExcelIgnore]public string Name { get => _name; set => _name = value; }
        [ExcelIgnore]public string Text { get => text; set => text = value; }
        [ExcelIgnore]public Sprite Icon { get => icon; set => icon = value; }
        [ExcelIgnore]public List<int> LinkNodes => linkNodes;
        [ExcelIgnore]public bool IsRoot { get => isRoot; set => isRoot = value; }
        [ExcelIgnore] public string NodeType { get => nodeType; set => nodeType = value; }
#if UNITY_EDITOR
        [HideInInspector,SerializeField]
        public Position position;
        [ExcelIgnore]public Node.Position NodePosition
        {
            get => position;
            set => position = value;
        }
        [ExcelIgnore] IEnumerable nodeTypes => NodeTree.AllColorTypes;
#endif
        [Serializable]
        public struct Position
        {
            public float x;
            public float y;
            public Position(float x,float y) 
            {
                this.x = x;
                this.y = y;
            }          
        }
#if UNITY_EDITOR
        private void DrawPreview()
        {
            UnityEditor.EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();

            GUILayout.Label("对话图标");
            icon = (Sprite)UnityEditor.EditorGUILayout.ObjectField(this.icon, typeof(Sprite), true, GUILayout.Width(50), GUILayout.Height(50));
            GUILayout.EndHorizontal();
            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                this.Save();
            }
           
        }
#endif
        [ExcelIgnore]
        internal Action onValidate;       

        private void OnValidate()
        {
            onValidate?.Invoke();

            
        }
 
     

    }
}
