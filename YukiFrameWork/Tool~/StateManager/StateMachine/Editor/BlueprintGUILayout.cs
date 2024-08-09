#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace YukiFrameWork.ActionStates
{
    public class BlueprintGUILayout : Editor
    {
      
        /// <summary>
        /// 结束水平向前移动
        /// </summary>

        public static void EndSpaceHorizontal()
        {
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 开始水平向前移动几个格子
        /// </summary>

        public static void BeginStyleVertical(string name = "系统基础动作组件", string styleName = "ProgressBarBack")
        {
            GUILayout.Button(name, GUI.skin.GetStyle("dragtabdropwindow"));
            if (styleName == "")
                EditorGUILayout.BeginVertical();
            else
                EditorGUILayout.BeginVertical(styleName);
        }

        /// <summary>
        /// 结束水平向前移动
        /// </summary>

        public static void EndStyleVertical()
        {
            EditorGUILayout.EndVertical();
        }
    }
}
#endif