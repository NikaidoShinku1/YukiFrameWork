
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YukiFrameWork;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditorInternal;
namespace YukiFrameWork.Overlap 
{

    [CustomEditor(typeof(OverlapArea2D))]
    public class OverlapArea2DInspector : OdinEditor
    {
        // Fix编码

       

        private SerializedProperty mode;
        private SerializedProperty positions;      

        private ReorderableList reorderableList = null;

        private Transform transform = null;

        private bool positions_foldout = false;

        private OverlapArea2D overlapArea2D;

        private bool resultFoldout;

        protected override void OnEnable()
        {
            base.OnEnable();
            overlapArea2D = serializedObject.targetObject as OverlapArea2D;
            transform = overlapArea2D.transform;
                     
            mode = serializedObject.FindProperty("_mode");
            positions = serializedObject.FindProperty("positions");

            reorderableList = new ReorderableList(serializedObject, positions,true,false,true,true);
            reorderableList.headerHeight = 1;
            reorderableList.drawElementCallback += OnDrawElementCallback;
            reorderableList.onSelectCallback += OnSelectCallback;
            reorderableList.onAddCallback += OnAddCallback;
        }

        public override void OnInspectorGUI()
        { 
            serializedObject.Update();

            base.OnInspectorGUI();

            serializedObject.ApplyModifiedProperties();

            OverlapBase2DInpector.DrawResult(ref resultFoldout, overlapArea2D.Results); 
        }
         
        private void OnDrawElementCallback(Rect rect, int index, bool isActive, bool isFocused) 
        {
            SerializedProperty property = positions.GetArrayElementAtIndex(index);
            Vector3 vector3 = property.vector3Value;
            property.vector3Value =  EditorGUI.Vector3Field(rect,string.Empty, vector3);
            property.serializedObject.ApplyModifiedProperties();
        }

        private void OnSelectCallback(ReorderableList list) {
            SceneView.lastActiveSceneView.Repaint();
        }

        private void OnAddCallback(ReorderableList list) 
        {

            int count = list.count;

            list.serializedProperty.InsertArrayElementAtIndex(list.count);

            SerializedProperty current = list.serializedProperty.GetArrayElementAtIndex(count);

            if (count == 1)
            {
                SerializedProperty last = list.serializedProperty.GetArrayElementAtIndex(count - 1);
                current.vector3Value = last.vector3Value + Vector3.right;
            }
            else if (count > 1) 
            {
                SerializedProperty last1 = list.serializedProperty.GetArrayElementAtIndex(count - 2);
                SerializedProperty last2 = list.serializedProperty.GetArrayElementAtIndex(count - 1);
                Vector3 dir = last2.vector3Value - last1.vector3Value; 
                if (dir == Vector3.zero) dir = Vector3.right; 
                current.vector3Value = last2.vector3Value + dir.normalized;
            }

        }


        private void OnSceneGUI() 
        {

            if (mode.enumValueIndex != 0) return;

            for (int i = 0;i < positions.arraySize;i++)
            {
                SerializedProperty property = positions.GetArrayElementAtIndex(i); 
                float size = HandleUtility.GetHandleSize(property.vector3Value) * 0.1f;
                
                Handles.color = reorderableList.index == i ? Color.red : Color.green;

                EditorGUI.BeginChangeCheck();

                Vector3 world = transform.localToWorldMatrix.MultiplyPoint(property.vector3Value);
#if UNITY_2022_1_OR_NEWER
                var fmh_140_55_638531289003172635 = Quaternion.identity; world = Handles.FreeMoveHandle(world, size, Vector3.one * 0.1f, Handles.CubeHandleCap);
#else
                var fmh_140_55_638531289003172635 = Quaternion.identity; world = Handles.FreeMoveHandle(world,fmh_140_55_638531289003172635, size, Vector3.one * 0.1f, Handles.CubeHandleCap);
#endif
                property.vector3Value = transform.worldToLocalMatrix.MultiplyPoint(world);
                property.serializedObject.ApplyModifiedProperties();

                if (EditorGUI.EndChangeCheck()) { 
                    reorderableList.index = i;
                }

            }

        }

         
    }

}
#endif