
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace YukiFrameWork
{
    [CustomEditor(typeof(BezierVisualTool))]
    public class BezierVisualInspector : Editor
    {
        private SerializedProperty sceneGUIProperty;     

        private SerializedProperty startProperty;
        private SerializedProperty endProperty;

        private SerializedProperty control1Property;
        private SerializedProperty control2Property;

        private SerializedProperty pathsProperty;

        private SerializedProperty countProperty;    

        private string startName
        {
            get 
            {
                BezierVisualTool tool = target as BezierVisualTool;
                return tool.pointType switch
                {
                    PointType.Vector => "start",
                    PointType.Transform => "startPos",
                    _ => string.Empty,
                };
            }
        }

        private string endName
        {
            get
            {
                BezierVisualTool tool = target as BezierVisualTool;
                return tool.pointType switch
                {
                    PointType.Vector => "end",
                    PointType.Transform => "endPos",
                    _ => string.Empty,
                };
            }
        }

        private string control1Name
        {
            get
            {
                BezierVisualTool tool = (BezierVisualTool)target;               
                return tool.pointType == PointType.Vector ? "control1" : "control1Pos";
            }
        }

        private string control2Name
        {
            get
            {
                BezierVisualTool tool = (BezierVisualTool)target;
                return tool.pointType == PointType.Vector ? "control2" : "control2Pos";
            }
        }

        private void OnEnable()
        {
            sceneGUIProperty = serializedObject.FindProperty("isScene");           
            pathsProperty = serializedObject.FindProperty("paths");
            countProperty = serializedObject.FindProperty("count");            
        }
        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(GUI.skin.box);

            BezierVisualTool tool = (BezierVisualTool)target;

            if (tool == null) return;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("设置端点的方式:", GUILayout.Width(200));
            tool.pointType = (PointType)EditorGUILayout.EnumPopup(tool.pointType);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("阶数:", GUILayout.Width(200));
            tool.stage = (BezierStage)EditorGUILayout.EnumPopup(tool.stage);
            EditorGUILayout.EndHorizontal();
            startProperty = serializedObject.FindProperty(startName);
            EditorGUILayout.PropertyField(startProperty);
            endProperty = serializedObject.FindProperty(endName);
            EditorGUILayout.PropertyField(endProperty);         
            EditorGUILayout.Space();

            switch (tool.stage)
            {
                case BezierStage.二阶:
                    control1Property = serializedObject.FindProperty(control1Name);
                    EditorGUILayout.PropertyField(control1Property);
                    break;
                case BezierStage.三阶:
                    control1Property = serializedObject.FindProperty(control1Name);
                    EditorGUILayout.PropertyField(control1Property);
                    control2Property = serializedObject.FindProperty(control2Name);
                    EditorGUILayout.PropertyField(control2Property);
                    break;
            }          
            EditorGUILayout.PropertyField(countProperty);
            EditorGUILayout.Space(10);
            if (GUILayout.Button("更新路径"))
            {
                if (tool.Count <= 0)
                {
                    "没有设置坐标数量无法创建!".LogInfo(Log.E);
                    return;
                }
                if (tool.pointType == PointType.Transform)
                {
                    Transform startPos = (startProperty.objectReferenceValue as Transform);
                    Transform endPos = (endProperty.objectReferenceValue as Transform);
                    if(startPos == null || endPos == null)
                    {
                        $"使用Transform但是没有正确传入开始或者结束的位置无法创建! Start:{startPos} End:{endPos}".LogInfo(Log.E);
                        return;
                    }
                }
                Vector3 start = tool.StartValue;
                Vector3 end = tool.EndValue;
                switch (tool.stage)
                {
                    case BezierStage.一阶:
                        tool.paths = BezierUtility.GetBezierList(start, end,tool.Count);
                        break;
                    case BezierStage.二阶:

                        Transform control1Pos = control1Property.objectReferenceValue as Transform;
                        if (control1Pos == null)
                        {
                            $"使用Transform但是没有传入控制点1无法创建! Control1:{control1Pos}".LogInfo(Log.E);
                            return;
                        }

                        tool.paths = BezierUtility.GetBezierList(start, tool.SecondOrderControl,end,tool.Count);

                        break;
                    case BezierStage.三阶:

                        control1Pos = control1Property.objectReferenceValue as Transform;
                        if (control1Pos == null)
                        {
                            $"使用Transform但是没有传入控制点1无法创建! Control1:{control1Pos}".LogInfo(Log.E);
                            return;
                        }

                        Transform control2Pos = control2Property.objectReferenceValue as Transform;
                        if (control2Pos == null)
                        {
                            $"使用Transform但是没有传入控制点2无法创建! Control1:{control2Pos}".LogInfo(Log.E);
                            return;
                        }
                        tool.paths = BezierUtility.GetBezierList(start, tool.SecondOrderControl,tool.ThirdOrderControl,  end, tool.Count);                      
                        break;                
                }

                tool.paths.Add(tool.EndValue);
            }
            EditorGUILayout.Space(20);            
            EditorGUILayout.LabelField("场景可视化:仅编辑器下生效");
            tool.color = EditorGUILayout.ColorField("线段颜色",tool.color);                    
            EditorGUILayout.PropertyField(sceneGUIProperty);                 

            EditorGUILayout.Space(10);
            EditorGUILayout.PropertyField(pathsProperty);
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            EditorGUILayout.EndVertical();
        }      
    }
}
#endif