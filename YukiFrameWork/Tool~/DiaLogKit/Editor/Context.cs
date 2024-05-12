using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;


namespace YukiFrameWork.DiaLogue
{
    public class Context 
    {
        private NodeTree nodeTree;
        public List<Node> SelectNodes = new List<Node>();    
        #region 箭头的预览
        public bool isPreviewTransition = false;
        public Node fromState = null;
        public Node hoverState = null;

        public Node parameterState = null;    
        #endregion

        #region 属性
        public float ZoomFactor { get; set; } = 0.3f;

        public Vector2 DragOffset { get; set; }       
        public NodeTree NodeTree
        {
            get
            {

                Object obj = Selection.activeObject;

                if (obj is NodeTree node)
                {
                    nodeTree = node;
                    PlayerPrefs.SetString("TreePath", AssetDatabase.GetAssetPath(node));
                }
                return nodeTree;                
            }
            set
            {
                nodeTree = value;
            }
        }

        #endregion


        #region 方法
        public void Reset()
        {
            //重置缩放和偏移
            this.ZoomFactor = 0.3f;
            this.DragOffset = Vector2.zero;            
        }
        public void ClearSelections()
        {
            this.SelectNodes.Clear();        
        }

        public void StartPreviewTransition(Node state)
        {
            this.fromState = state;
            this.isPreviewTransition = true;
        }

        public void CancelPreviewTransition()
        {
            this.fromState = null;
            this.isPreviewTransition = false;         
        }
        #endregion
    }
}
#endif