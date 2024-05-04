using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

#if UNITY_EDITOR
namespace YukiFrameWork.DiaLog
{
    public class TransitionLayer : DiaLogGraphLayer
    {

        private int currentStateName = -1;

        private NodeTree currentManager;

        private float transitionTimer = 0f;

        #region 方法
        public TransitionLayer(DiaLogEditorWindow editorWindow) : base(editorWindow)
        {
        }

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);

            DrawAllTransition();

            DrawStateChangeTransition();

        }

        public override void ProcessEvents()
        {
            base.ProcessEvents();                           
        }

        private void DrawAllTransition()
        {
            if (this.Context.NodeTree == null) return;
     
            foreach (var item in this.Context.NodeTree.nodes)
            {
                var attribute = item.GetType().GetCustomAttribute<RootNodeAttribute>(true);
                if (attribute != null)
                {
                    var child = item.GetType().GetField("child",BindingFlags.Instance | BindingFlags.NonPublic).GetValue(item) as Node;
                    if(child != null)
                        DrawTransition(item.NodeIndex, child.NodeIndex, this.Context.NodeTree.connectColor);
                }
                else if (item is SingleNode single && single.child != null)
                {              
                    DrawTransition(single.NodeIndex, single.child.NodeIndex, this.Context.NodeTree.connectColor);
                }
                else if (item is CompositeNode composite && composite.options.Count > 0)
                {
                    foreach (var c in composite.options)
                    {
                        var child = this.Context.NodeTree.nodes.Find(x => x.NodeIndex == c.childIndex);
                        if (c == null || child == null) continue;                        
                        DrawTransition(composite.NodeIndex, child.NodeIndex, this.Context.NodeTree.connectColor);
                    }
                }
            }

            //绘制预览箭头
            if (this.Context.isPreviewTransition)
            {
                if (this.Context.hoverState == null
                    )
                {
                    Rect rect = GetTransformedRect(this.Context.fromState.rect);
                    DrawTransition(rect.center, Event.current.mousePosition, Color.white);
                }
                else
                {
                    DrawTransition(this.Context.fromState, this.Context.hoverState, Color.white);
                }
            }
        }

        private void DrawTransition(int fromStateName, int toStateName, Color color, bool isShowArrow = true,float progress = 1f)
        {
            var fromState = this.Context.NodeTree.nodes.FirstOrDefault(x => x.NodeIndex == fromStateName);
            var toState = this.Context.NodeTree.nodes.FirstOrDefault(x => x.NodeIndex == toStateName);
            DrawTransition(fromState, toState, color, isShowArrow,progress);
        }     

        private void DrawTransition(Node startNode, Node endNode, Color color, bool isShowArrow = true,float progress = 1f,bool isArrow = true)
        {
            if (startNode == null || endNode == null) return;

            Rect start = GetTransformedRect(startNode.rect);
            Rect end = GetTransformedRect(endNode.rect);

            Vector2 offect = GetTransitionOffset(start, end);

            if (position.Contains(start.center + offect)
                || position.Contains(end.center + offect)
                || position.Contains(start.center + offect + (end.center - start.center) * 0.5f))
            {
                Vector2 endPoint = Vector2.Lerp(start.center + offect, end.center + offect, progress);
                DrawTransition(start.center + offect, endPoint, color, isShowArrow,isArrow);
            }
        }

        private Vector2 GetTransitionOffset(Rect originRect, Rect targetRect)
        {
            Vector2 direction = targetRect.center - originRect.center;
            Vector2 offect = Vector2.zero;

            if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
            {
                //上下关系
                offect.x = direction.y < 0 ? 20 : -20;
            }
            else 
            {
                offect.y = direction.x > 0 ? 20 : -20;
                //左右关系
            }

            return offect * this.Context.ZoomFactor;
        }

        private void DrawTransition(Vector2 start,Vector2 end,Color color,bool isShowArrow = true,bool isArrow = true)
        {
            Handles.BeginGUI();

            Handles.color = color;

            float width = 5f;

            float arrowWidth = 10f;
            Handles.DrawAAPolyLine(width, start, end);       

            if (isShowArrow)
            {
                Vector2 direction = end - start;
                Vector2 center = start + direction * 0.5f;
                Vector2 crossDir = Vector3.Cross(direction, Vector3.forward);
                Vector3[] targetPos = new Vector3[] 
                {
                    direction.normalized * arrowWidth + center,
                    center + crossDir.normalized * width,
                    center - crossDir.normalized * width
                };
                Handles.DrawAAConvexPolygon(targetPos);
            }

            Handles.EndGUI();
        }      
        private void DrawStateChangeTransition()
        {
            if (!Application.isPlaying) return;

            if (this.Context.NodeTree == null || this.Context.NodeTree.runningNode == null) return;          

            if (currentStateName == -1)
            {
                currentStateName = this.Context.NodeTree.runningNode.NodeIndex;
                transitionTimer = 0f;
                return;
            }

            //判断状态机是否相同
            if (currentManager != this.Context.NodeTree)
            {
                currentStateName = -1;
                transitionTimer = 0;
                currentManager = this.Context.NodeTree;
                return;
            }
            if (this.Context.NodeTree.runningNode.NodeIndex != currentStateName)
            {
                ChangeTimer();
                //状态正在切换
                DrawTransition(currentStateName
                    , this.Context.NodeTree.runningNode.NodeIndex
                    , ColorConst.TransitionColor, false, transitionTimer);

            }
                  
        }

        private void ChangeTimer()
        {
            transitionTimer += Time.deltaTime;
            if (transitionTimer >= 1)
            {
                currentStateName = this.Context.NodeTree.runningNode.NodeIndex;
                transitionTimer = 0;
            }
        }
        #endregion
    }
}
#endif