using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace YukiFrameWork.Overlap 
{
    [CustomEditor(typeof(OverlapBox2D))]
    public class OverlapBox2DInspector : OverlapBase2DInpector
    {

        private OverlapBox2D box2D;

        protected SerializedProperty offset;

        protected SerializedProperty size;
         

        protected override void OnEnable()
        {
            base.OnEnable(); 
            box2D = target as OverlapBox2D;

            offset = serializedObject.FindProperty("offset");
            size = serializedObject.FindProperty("size"); 
        }


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (size.vector2Value.x <= 0)
            {
                size.vector2Value = new Vector2(0, size.vector2Value.y);
                size.serializedObject.ApplyModifiedProperties();
            }

            if (size.vector2Value.y <= 0)
            {
                size.vector2Value = new Vector2(size.vector2Value.x, 0);
                size.serializedObject.ApplyModifiedProperties();
            }
        }

        protected virtual void OnSceneGUI() 
        {  
            Vector3 top = box2D.transform.localToWorldMatrix.MultiplyPoint(box2D.Offset + new Vector2(0,box2D.Size.y / 2));
 
            float size = HandleUtility.GetHandleSize(top) * 0.1f;
            Handles.color = Color.green;
            Quaternion rotation = Quaternion.Euler(0, 0, box2D.transform.eulerAngles.z);
            Vector3 current_top = Handles.FreeMoveHandle(top, size, Vector3.one * size, Handles.CubeHandleCap);
             
            SetTop(current_top.y - top.y);

            Vector3 down = box2D.transform.localToWorldMatrix.MultiplyPoint(box2D.Offset - new Vector2(0, box2D.Size.y / 2));
           
            size = HandleUtility.GetHandleSize(down) * 0.1f;
            Handles.color = Color.green;
            rotation = Quaternion.Euler(0, 0, box2D.transform.eulerAngles.z);
            Vector3 current_down = Handles.FreeMoveHandle(down, size, Vector3.one * size, Handles.CubeHandleCap);
              
            SetDown(current_down.y - down.y);

            Vector3 left = box2D.transform.localToWorldMatrix.MultiplyPoint(box2D.Offset - new Vector2(box2D.Size.x / 2, 0));

            size = HandleUtility.GetHandleSize(left) * 0.1f;
            Handles.color = Color.green;
            rotation = Quaternion.Euler(0, 0, box2D.transform.eulerAngles.z);
            Vector3 current_left = Handles.FreeMoveHandle(left, size, Vector3.one * size, Handles.CubeHandleCap);
             
            SetLeft(current_left.x - left.x);


            Vector3 right = box2D.transform.localToWorldMatrix.MultiplyPoint(box2D.Offset + new Vector2(box2D.Size.x / 2, 0));
             
            size = HandleUtility.GetHandleSize(right) * 0.1f;
            Handles.color = Color.green;
            rotation = Quaternion.Euler(0, 0, box2D.transform.eulerAngles.z);
            Vector3 current_right = Handles.FreeMoveHandle(right, size, Vector3.one * size, Handles.CubeHandleCap);
             
            SetRight(current_right.x - right.x); 
        }


        public void SetTop(float detalY) 
        {

            if (box2D.transform.lossyScale.y < 0)
                detalY = -detalY;

            Vector2 offset_v = new Vector2(box2D.Offset.x, box2D.Offset.y + detalY / 2);
            Vector2 size_v = new Vector2(box2D.Size.x, box2D.Size.y + detalY);

            if (!LimitSize(size_v)) return;

            offset.vector2Value = offset_v; 
            size.vector2Value = size_v;

            serializedObject.ApplyModifiedProperties();
        }

        public void SetDown(float detalY)
        {
            if (box2D.transform.lossyScale.y < 0)
                detalY = -detalY;

            Vector2 offset_v = new Vector2(box2D.Offset.x, box2D.Offset.y + detalY / 2);
            Vector2 size_v = new Vector2(box2D.Size.x, box2D.Size.y - detalY);

            if (!LimitSize(size_v)) return;

            offset.vector2Value = offset_v;
            size.vector2Value = size_v;

            serializedObject.ApplyModifiedProperties();
        }

        public void SetLeft(float detalX)
        {
            if (box2D.transform.lossyScale.x < 0)
                detalX = -detalX;

            Vector2 offset_v = new Vector2(box2D.Offset.x + detalX / 2, box2D.Offset.y);
            Vector2 size_v = new Vector2(box2D.Size.x - detalX, box2D.Size.y);

            if (!LimitSize(size_v)) return;

            offset.vector2Value = offset_v;
            size.vector2Value = size_v;

            serializedObject.ApplyModifiedProperties();
        }

        public void SetRight(float detalX)
        {
            if (box2D.transform.lossyScale.x < 0)
                detalX = -detalX;

            Vector2 offset_v = new Vector2(box2D.Offset.x + detalX / 2, box2D.Offset.y);
            Vector2 size_v = new Vector2(box2D.Size.x + detalX, box2D.Size.y);

            if (!LimitSize(size_v)) return;

            offset.vector2Value = offset_v;
            size.vector2Value = size_v;

            serializedObject.ApplyModifiedProperties();
        }


        protected virtual bool LimitSize(Vector2 size) {

            if (size.x <= 0)
                return false;
            if (size.y <= 0)
                return false;

            return true;
        }
        

    }

}

#endif