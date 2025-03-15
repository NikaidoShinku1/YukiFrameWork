///=====================================================
/// - FileName:      BackGroundGraphView.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/8 20:11:41
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine.UIElements;

namespace YukiFrameWork.Machine
{
    public class BackGroundGraphView : ImmediateModeElement
    {
        private VisualElement m_Container;

        private Vector3 point_left_top;
        private Vector3 point_right_bottom;

        private Rect renderRect;

        private static readonly Color LineColorDark = new Color(0f, 0f, 0f, 0.45f);
        private static readonly Color LineColorLight = new Color(0f, 0f, 0f, 0.15f);

        private float gridSpace = 20;
        private float scale = 1;
        private static Color LineColor
        {
            get
            {
                if (EditorGUIUtility.isProSkin)
                {
                    return LineColorDark;
                }
                return LineColorLight;
            }
        }

        private Rect backgroundRect;

        // Fix编码
        protected override void ImmediateRepaint()
        {

            if (Event.current.type != EventType.Repaint)
                return;

            VisualElement visualElement = base.parent;
            StateGraphView graphView = visualElement as StateGraphView;
            if (graphView == null)
            {
                throw new InvalidOperationException("Background can only be added to a GraphView");
            }

            backgroundRect.Set(0, 0, graphView.layout.width, graphView.layout.height);

            Styles.graphBackground.Draw(backgroundRect, isHover: false, isActive: false, on: false, hasKeyboardFocus: false);

            m_Container = graphView.contentViewContainer;

            Matrix4x4 inverse = m_Container.transform.matrix.inverse;
            Rect clipRect = graphView.layout;

            //Debug.Log(clipRect.ToString());

            point_left_top.Set(clipRect.xMin, 0, 0);
            point_right_bottom.Set(clipRect.xMax, clipRect.yMax, 0);

            point_left_top = inverse.MultiplyPoint(point_left_top);
            point_right_bottom = inverse.MultiplyPoint(point_right_bottom);

            float width = Mathf.Abs(point_right_bottom.x - point_left_top.x);
            float height = Mathf.Abs(point_right_bottom.y - point_left_top.y);

            renderRect.Set(point_left_top.x, point_left_top.y, width, height);

            Color color = Color.white;

            if (graphView.scale < scale)
            {
                color.a = Mathf.InverseLerp(scale * 0.1f, scale, graphView.scale);
            }
            else
            {
                color.a = Mathf.InverseLerp(scale * 10, scale, graphView.scale);
            }

            DrawGrid(gridSpace, LineColor * color, renderRect);
            DrawGrid(gridSpace * 10, LineColor, renderRect);

            //GUI.Label(new Rect(0, 0, 300, 100), graphView.scale.ToString());
            UpdateSpace(graphView.scale);
        }


        public void DrawGrid(float gridSpacing, Color gridColor, Rect rect)
        {

            if (gridSpacing == 0) return;

            if (Mathf.Abs(gridSpacing) > rect.width || Mathf.Abs(gridSpacing) > rect.height) return;

            Vector2 point_start = new Vector2();
            Vector2 point_end = new Vector2();


            float offset_x = rect.x - (int)(rect.x / gridSpacing) * gridSpacing;
            float offset_y = rect.y - (int)(rect.y / gridSpacing) * gridSpacing;


            for (float i = rect.x - offset_x; i < rect.x + rect.width; i += gridSpacing)
            {
                point_start.Set(i, rect.y);
                point_start = m_Container.transform.matrix.MultiplyPoint(point_start);


                point_end.Set(i, rect.y + rect.height);
                point_end = m_Container.transform.matrix.MultiplyPoint(point_end);
                DrawLine(point_start, point_end, gridColor);

                int x = (int)(i / gridSpacing);
            }

            for (float i = rect.y - offset_y; i < rect.y + rect.height; i += gridSpacing)
            {
                point_start.Set(rect.x, i);
                point_start = m_Container.transform.matrix.MultiplyPoint(point_start);

                point_end.Set(rect.x + rect.width, i);
                point_end = m_Container.transform.matrix.MultiplyPoint(point_end);


                DrawLine(point_start, point_end, gridColor);

                int y = (int)(i / gridSpacing);

            }

        }

        private static void DrawLine(Vector2 start, Vector2 end, Color lineColor)
        {
            Handles.color = lineColor;
            Handles.DrawLine(start, end);
        }

        private void UpdateSpace(float currentScale)
        {

            if (currentScale / scale >= 2)
            {
                // 放大了10倍
                gridSpace /= 10;
                scale *= 10;
            }
            else if (currentScale / scale <= 0.2f)
            {
                // 缩小了10倍

                gridSpace *= 10;
                scale *= 0.1f;
            }

        }

    }
}
#endif