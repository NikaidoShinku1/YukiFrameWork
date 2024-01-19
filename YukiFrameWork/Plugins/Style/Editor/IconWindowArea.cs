#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
///=====================================================
/// - FileName:      IconArea.cs
/// - NameSpace:     YukiFrameWork
/// - Created:       Yuki
/// - Email:         1274672030@qq.com
/// - Description:   这是一个框架工具创建的脚本
/// - Creation Time: 2024/1/16 18:46:15
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================
namespace YukiFrameWork.Extension
{
    public abstract class IconWindowArea
    {
        protected float borderWidth = 1;
        public bool showBorder = false;
        public Color borderColor = new Color(0.6f, 0.6f, 0.6f, 1.333f);
        protected Vector2 areaSize { get; set; }
        protected Rect areaRect { get; private set; }

        public IconWindowArea() => OnInit();

        protected virtual void OnInit()
        {
        }

        public virtual void OnGUI(Rect rect)
        {
            if (showBorder) areaRect = DoBorder(rect);
            else areaRect = rect;
            GUILayout.BeginArea(areaRect);

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            OnTitleGUI();
            EditorGUILayout.EndHorizontal();
            OnBodyGUI();

            GUILayout.EndArea();
            if (Event.current.type == EventType.Repaint) areaSize = rect.size;
        }

        /// <summary>
        /// 绘制标题区域
        /// </summary>
        protected abstract void OnTitleGUI();

        /// <summary>
        /// 绘制主体区域
        /// </summary>
        protected abstract void OnBodyGUI();

        public Rect DoBorder(Rect rect)
        {
            MEditorUtility.DrawOutline(rect, this.borderWidth, borderColor);
            return new Rect(rect.x + this.borderWidth, rect.y + this.borderWidth, rect.width - 2f * borderWidth, rect.height - 2f * borderWidth);
        }
    }

    public class MEditorUtility
    {
        /// <summary>
        /// 绘制方框
        /// </summary>
        /// <param name="rect">区域</param>
        /// <param name="lineWidth">线宽</param>
        /// <param name="color">颜色</param>
        public static void DrawOutline(Rect rect, float lineWidth, Color color)
        {
            if (Event.current.type != EventType.Repaint) return;
            Color color1 = GUI.color;
            GUI.color *= color;
            Rect upRect = new Rect(rect.x, rect.y, rect.width, lineWidth);
            Rect downRect = new Rect(rect.x, rect.y + rect.height - lineWidth, rect.width, lineWidth);
            Rect leftRect = new Rect(rect.x, rect.y + lineWidth, lineWidth, rect.height - 2f * lineWidth);
            Rect rightRect = new Rect(rect.width - lineWidth, rect.y + lineWidth, lineWidth, rect.height - 2f * lineWidth);

            GUI.DrawTexture(upRect, EditorGUIUtility.whiteTexture);
            GUI.DrawTexture(downRect, EditorGUIUtility.whiteTexture);
            GUI.DrawTexture(leftRect, EditorGUIUtility.whiteTexture);
            GUI.DrawTexture(rightRect, EditorGUIUtility.whiteTexture);
            GUI.color = color1;
        }

        /// <summary>
        /// 拷贝字符串到剪贴板
        /// </summary>
        public static void CopyString(string value)
        {
            TextEditor textEditor = new TextEditor();
            textEditor.text = value;
            textEditor.OnFocus();
            textEditor.Copy();
        }
    }
}
#endif