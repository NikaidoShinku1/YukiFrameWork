using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace YukiFrameWork.States
{
    public class TransitionLayer : GraphLayer
    {

        private string currentStateName;

        private StateManager currentManager;

        private float transitionTimer = 0f;

        #region 方法
        public TransitionLayer(StateMechineEditorWindow editorWindow) : base(editorWindow)
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

            if (this.Context.StateMechine == null) return;
           
            switch (Event.current.type)
            {
                case EventType.MouseDown:
                    {
                        if (Event.current.button == 0)
                            CheckTransitionClick();
                    }
                    break;
                case EventType.KeyDown:
                    if (Event.current.keyCode == KeyCode.Delete)
                    {
                        if (this.Context.selectTransition != null)
                        {                        
                            TransitionFactory.DeleteTransition(this.Context.StateMechine, this.Context.selectTransition);
                            this.Context.selectTransition = null;
                        }
                    }    
                    break;
            }
          
        }

        private void DrawAllTransition()
        {
            if (this.Context.StateMechine == null) return;

            List<StateBase> stateBases = GetLayerStates();         

            StateBase entryState = stateBases.Where(x => x.name.Equals(StateConst.entryState)).FirstOrDefault();
            StateBase defaultState = stateBases.Where(x => x.defaultState).FirstOrDefault();
            //绘制默认过渡
            DrawTransition(entryState, defaultState, Color.yellow,true,1,false);

            foreach (var item in GetLayerTransitions())
            {
                DrawTransition(item.fromStateName, item.toStateName, item == this.Context.selectTransition ? ColorConst.SelectColor : Color.white);
            }

            //绘制预览箭头
            if (this.Context.isPreviewTransition)          
            {
                if (this.Context.hoverState == null                 
                    || this.Context.hoverState.name.Equals(StateConst.entryState))
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

        private void DrawTransition(string fromStateName, string toStateName, Color color, bool isShowArrow = true,float progress = 1f)
        {
           
            var fromState = GetLayerStates()?.Where(x => x.name.Equals(fromStateName)).FirstOrDefault();
            var toState = GetLayerStates()?.Where(x => x.name.Equals(toStateName)).FirstOrDefault();
            DrawTransition(fromState, toState, color, isShowArrow,progress);
        }     

        private void DrawTransition(StateBase startNode, StateBase endNode, Color color, bool isShowArrow = true,float progress = 1f,bool isArrow = true)
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

            float width = GetMinDistanceToLine(Event.current.mousePosition, start, end) <= 5 && isArrow ? 7.5f : 5f;

            float arrowWidth = GetMinDistanceToLine(Event.current.mousePosition, start, end) <= 5 && isArrow ? 12.5f : 10f;
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

        private void CheckTransitionClick()
        {
            foreach (var item in GetLayerTransitions())
            {               
                StateBase fromState = GetLayerStates()?.Where(x => x.name.Equals(item.fromStateName)).FirstOrDefault();
                StateBase toState = GetLayerStates()?.Where(x => x.name.Equals(item.toStateName)).FirstOrDefault();             
                if (fromState == null || toState == null) return;               
                Rect startRect = GetTransformedRect(fromState.rect);
                Rect endRect = GetTransformedRect(toState.rect);

                Vector2 offset = GetTransitionOffset(startRect, endRect);
                Vector2 start = startRect.center + offset;
                Vector2 end = endRect.center + offset;

                float width = Mathf.Clamp(Mathf.Abs(end.x - start.x), 10, Mathf.Abs(end.x - start.x));
                float height = Mathf.Clamp(Mathf.Abs(end.y - start.y), 10, Mathf.Abs(end.y - start.y));

                Rect rect = new Rect(0, 0, width, height);
                rect.center = start + (end - start) * 0.5f;

                if (rect.Contains(Event.current.mousePosition))
                {
                    if (GetMinDistanceToLine(Event.current.mousePosition, start, end) <= 5)
                    {
                        this.Context.selectTransition = item;
                        OnTransitionClick(item);
                        Event.current.Use();
                        break;
                    }
                }

            }
        }

        /// <summary>
        /// 获取一个点到线的最短距离
        /// </summary>        
        private float GetMinDistanceToLine(Vector2 point,Vector2 start,Vector2 end)
        {
            Vector2 vLine = end - start;
            Vector2 wLine = point - start;
            
            Vector2 distance = wLine - Vector2.Dot(vLine, wLine) / vLine.magnitude * vLine.normalized;
            return distance.magnitude;
        }

        private void OnTransitionClick(StateTransitionData transition)
        {
            TransitionInspectorHelper.Instance.Inspect(this.Context.StateMechine, transition);
        }

        private void DrawStateChangeTransition()
        {
            if (!Application.isPlaying) return;

            if (this.Context.StateManager == null) return;

            if (this.Context.StateManager.runTimeSubStatePair[this.Context.StateMechine.layerName].CurrentState == null) return;         

            if (string.IsNullOrEmpty(currentStateName))
            {
                if (!this.Context.StateManager.stateMechine.IsSubLayerAndContainsName())
                    currentStateName = this.Context.StateManager.runTimeSubStatePair[this.Context.StateMechine.layerName].CurrentState.name;
                else if (this.Context.StateManager.runTimeSubStatePair[this.Context.StateMechine.layerName].CurrentState == null) return;

                else currentStateName = this.Context.StateManager.runTimeSubStatePair[this.Context.StateMechine.layerName].CurrentState.name;
                transitionTimer = 0f;
                return;
            }

            //判断状态机是否相同
            if (currentManager != this.Context.StateManager)
            {
                currentStateName = null;
                transitionTimer = 0;
                currentManager = this.Context.StateManager;
                return;
            }

            string currentName = this.Context.StateManager.runTimeSubStatePair[this.Context.StateMechine.layerName]?.CurrentState.name;
            if (!currentName.Equals(currentStateName))
            {
                var ts = this.Context.StateManager.subTransitions[this.Context.StateMechine.layerName];
                string name = currentStateName;
                var info = ts
                 .Find(x
                 => x.FromStateName.Equals(currentStateName)
                 && x.ToStateName.Equals(this.Context.StateManager.runTimeSubStatePair[this.Context.StateMechine.layerName].CurrentState.name));
                if (ts.Find(x => x.FromStateName == StateConst.anyState && x.ToStateName == currentName) != null
                    && info == null)
                {
                    name = StateConst.anyState;
                }             
                ChangeTimer();
                //状态正在切换   

                DrawTransition(name
                    , this.Context.StateManager.runTimeSubStatePair[this.Context.StateMechine.layerName].CurrentState.name
                    , ColorConst.TransitionColor, false, transitionTimer);
            }
            
        }

        private void ChangeTimer()
        {
            transitionTimer += Time.deltaTime;
            if (transitionTimer >= 1)
            {              
                currentStateName = this.Context.StateManager.runTimeSubStatePair[this.Context.StateMechine.layerName].CurrentState.name;
                transitionTimer = 0;
            }
        }
        #endregion
    }
}
#endif