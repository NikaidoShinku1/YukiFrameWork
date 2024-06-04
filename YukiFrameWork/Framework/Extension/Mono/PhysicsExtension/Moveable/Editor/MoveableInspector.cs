using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;

namespace YukiFrameWork.Platform2D
{

    [CustomEditor(typeof(Moveable), true)]
    public class MoveableInspector : OdinEditor
    {            
        private SerializedProperty positions = null;
        private ReorderableList reorderableList = null;

        private bool positionsFoldout = false;

        private Moveable moveable;

        private Vector3 snap = new Vector3(0.1f, 0.1f, 0.01f);

        protected override void OnEnable()
        {
            base.OnEnable();                 
            positions = serializedObject.FindProperty("positions");           
            
            reorderableList = new ReorderableList(serializedObject, positions);
            reorderableList.headerHeight = 1;
            reorderableList.drawElementCallback += OnDrawElementCallback;
            reorderableList.onAddCallback += OnAddCallback;
            reorderableList.onSelectCallback += OnSelect;
            moveable = target as Moveable;
        }

        public override void OnInspectorGUI()
        {

            serializedObject.Update();

            base.OnInspectorGUI();            

            serializedObject.ApplyModifiedProperties();
        }

        private void OnDrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        { 
            EditorGUI.BeginChangeCheck();

            SerializedProperty property = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
            property.vector3Value = EditorGUI.Vector3Field(rect, "", property.vector3Value);

            if (EditorGUI.EndChangeCheck()) 
            {  
                serializedObject.ApplyModifiedProperties();
            }

        }

        private void OnAddCallback(ReorderableList list) 
        {
            Vector3 target = Vector3.zero; 

            int size = list.serializedProperty.arraySize;

            list.serializedProperty.InsertArrayElementAtIndex(size);
            SerializedProperty property = list.serializedProperty.GetArrayElementAtIndex(size);

            if (size == 0)
            {
                target = moveable.transform.position; 
            }
            else if (size == 1)
            {
                target = list.serializedProperty.GetArrayElementAtIndex(0).vector3Value + Vector3.right;
            }
            else 
            {
                SerializedProperty last2 = list.serializedProperty.GetArrayElementAtIndex(size - 2);
                SerializedProperty last1 = list.serializedProperty.GetArrayElementAtIndex(size - 1);
                
                target =(last1.vector3Value - last2.vector3Value).normalized + last1.vector3Value;
            }

            property.vector3Value = target; 
            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {

            if (reorderableList.index < 0) 
                return;
             
            SerializedProperty property = reorderableList.serializedProperty.GetArrayElementAtIndex(reorderableList.index);

            if (property == null)
                return;

            EditorGUI.BeginChangeCheck();


            float size = HandleUtility.GetHandleSize(property.vector3Value) * 0.1f;

            //Handles.color = Color.green; 
            //property.vector3Value = Handles.FreeMoveHandle(property.vector3Value, Quaternion.identity, size, snap, Handles.CubeHandleCap);

            property.vector3Value = Handles.PositionHandle(property.vector3Value, Quaternion.identity);

            if (EditorGUI.EndChangeCheck()) 
            {
                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedProperties();
            }


        }

        private void OnSelect(ReorderableList list) 
        {
            SceneView.lastActiveSceneView.Repaint();
        }

    }

}
#endif