using UnityEditor;
using UnityEngine;

namespace YukiFrameWork.States
{
    public class BackGroundLayer : GraphLayer
    {
        #region 字段
        private Vector2 mousePosition;
        #endregion
        #region 常量

        private const int Cell_Size_Min = 30;
        private const int Cell_Size_Max = 300;

        #endregion
        #region 方法
        public BackGroundLayer(StateMechineEditor editorWindow) : base(editorWindow)
        {
        }

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);

            EditorGUI.DrawRect(rect, ColorConst.BackGroundColor);         
            if (Event.current.type == EventType.Repaint)
            {
                DrawGrid(Cell_Size_Min, ColorConst.GridColor, rect);
                DrawGrid(Cell_Size_Max, ColorConst.GridColor, rect);
            }        
        }

        public override void ProcessEvents()
        {
            base.ProcessEvents();
            Event e = Event.current;

            switch (e.type)
            {
                case EventType.MouseDrag:
                    {
                        ///处理鼠标中键拖拽
                        if (e.button == 2 && position.Contains(e.mousePosition))
                        {
                            this.Context.DragOffset += e.delta;
                            e.Use();
                        }
                    }
                    break;
                case EventType.ScrollWheel:
                    {
                        if (position.Contains(e.mousePosition))
                        {
                            this.Context.ZoomFactor -= Mathf.Sign(e.delta.y) / 20.0f;
                            this.Context.ZoomFactor = Mathf.Clamp(this.Context.ZoomFactor, 0.2f, 1);
                            e.Use();
                        }
                    }
                    break;
                case EventType.MouseDown:
                    {                       
                        if (e.button == 0 && !IsSelectActive())
                        {
                            this.Context.ClearSelections();
                            this.Context.CancelPreviewTransition();
                            e.Use();
                        }
                    }
                    break;
                case EventType.MouseUp:
                    {
                        if (e.button == 1 && this.Context.StateMechine != null && !Application.isPlaying)
                        {
                            this.Context.CancelPreviewTransition();
                            mousePosition = e.mousePosition;
                            ShowMenu();
                            e.Use();
                        }
                    }
                    break;            
                 
            }           
        }
        /// <summary>
        /// 在某一个区域绘制网格
        /// </summary>
        /// <param name="gridSpacing">间隔</param>
        /// <param name="gridColor">颜色</param>
        /// <param name="rect">区域本体</param>
        private void DrawGrid(float gridSpacing, Color gridColor, Rect rect)
        {
            if (gridSpacing == 0) return;

            if (Mathf.Abs(gridSpacing) > rect.width || Mathf.Abs(gridSpacing) > rect.height) return;

            gridSpacing *= this.Context.ZoomFactor;

            DrawHorLine(rect, gridColor, gridSpacing);
            DrawHorLine(rect, gridColor, -gridSpacing, 1);

            DrawVerLine(rect, gridColor, gridSpacing);
            DrawVerLine(rect, gridColor, -gridSpacing);
        }

        /// <summary>
        /// 绘制网格竖线
        /// </summary>
        /// <param name="rect">区域</param>
        /// <param name="gridColor">网格颜色</param>
        /// <param name="gridSpacing">间隔</param>
        /// <param name="startIndex">开始的下标</param>
        private void DrawVerLine(Rect rect, Color gridColor, float gridSpacing, int startIndex = 0)
        {
            Vector2 center = rect.center + this.Context.DragOffset;

            Vector2 start = Vector2.zero;

            Vector2 end = Vector2.zero;

            int i = startIndex;

            if(center.x < rect.x && gridSpacing > 0)
            {
                i = (int)((rect.x - center.x) / gridSpacing);
                if (rect.x - center.x >= gridSpacing * i)
                    i++;
            }

            if (center.x > (rect.x + rect.width) && gridSpacing < 0)
            {
                i = (int)((center.x - (rect.x + rect.width)) / Mathf.Abs(gridSpacing));
                if (center.x - (rect.x + rect.width) >= Mathf.Abs(gridSpacing) * i)
                    i++;
            }

            do
            {
                start = new Vector2(center.x + gridSpacing * i, rect.center.y - rect.height / 2);
                end = new Vector2(center.x + gridSpacing * i, rect.center.y + rect.height / 2);
                if (rect.Contains((start + end) / 2))
                    DrawLine(start, end, gridColor);
                i++;
            } while (rect.Contains((start + end) / 2));
        }
        /// <summary>
        /// 绘制网格横线
        /// </summary>
        /// <param name="rect">区域</param>
        /// <param name="gridColor">网格颜色</param>
        /// <param name="gridSpacing">间隔</param>
        /// <param name="startIndex">开始的下标</param>
        private void DrawHorLine(Rect rect, Color gridColor, float gridSpacing, int startIndex = 0)
        {
            Vector2 center = rect.center + this.Context.DragOffset;

            Vector2 start = Vector2.zero;

            Vector2 end = Vector2.zero;

            int i = startIndex;

            if (center.y < rect.y && gridSpacing > 0)
            {
                i = (int)((rect.y - center.y) / gridSpacing);

                if (rect.y - center.y >= i * gridSpacing)               
                    i++;                
            }

            if (center.y > (rect.y + rect.height) && gridSpacing < 0)
            {
                i = (int)((center.y - (rect.y + rect.height)) / Mathf.Abs(gridSpacing));
                if (center.y - (rect.y + rect.height) >= i * Mathf.Abs(gridSpacing))
                    i++;
            }

            do
            {
                start = new Vector2(rect.center.x - rect.width / 2, center.y + gridSpacing * i);
                end = new Vector2(rect.center.x + rect.width / 2, center.y + gridSpacing * i);

                if (rect.Contains((start + end) / 2))                
                   DrawLine(start, end, gridColor);
                i++;
            } while (rect.Contains((start + end)/2));
        }

        /// <summary>
        /// 绘制线条
        /// </summary>
        /// <param name="start">起始点</param>
        /// <param name="end">终点</param>
        /// <param name="lineColor">线段颜色</param>
        private void DrawLine(Vector2 start, Vector2 end, Color lineColor)
        {
            GL.Begin(GL.LINES);

            GL.Color(lineColor);

            GL.Vertex(start);
            GL.Vertex(end);

            GL.End();
        }

        private void ShowMenu()
        {
            this.Context.ClearSelections();
            var genericMenu = new GenericMenu();
            genericMenu.AddItem(new GUIContent("Create State"), false, CreateState);

            genericMenu.ShowAsContext();
        }

        private void CreateState()
        {
            Rect rect = new Rect(0, 0, StateConst.StateWith, StateConst.StateHeight);

            rect.center = GetNodePosition(mousePosition);

            StateNodeFactory.CreateStateNode(this.Context.StateMechine, rect, this.Context.StateMechine.states.Count == 1);
        }

        private Vector2 GetNodePosition(Vector2 mousePosition)
        {
            Vector2 center = mousePosition + (mousePosition - this.position.center) * (1 - this.Context.ZoomFactor) / this.Context.ZoomFactor;
            center -= this.Context.DragOffset / this.Context.ZoomFactor;
            return center;
        }
        #endregion
    }
}
