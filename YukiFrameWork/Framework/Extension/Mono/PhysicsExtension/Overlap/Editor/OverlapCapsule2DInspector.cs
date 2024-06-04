using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine; 

namespace YukiFrameWork.Overlap 
{
    [CustomEditor(typeof(OverlapCapsule2D))]
    public class OverlapCapsule2DInspector : OverlapBox2DInspector
    {
        private OverlapCapsule2D overlapCapsule;

        protected override void OnEnable()
        {
            base.OnEnable();
            overlapCapsule = target as OverlapCapsule2D;
        }


        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            switch (overlapCapsule.Direction)
            {
                case CapsuleDirection2D.Vertical:
                    if (size.vector2Value.y < size.vector2Value.x) {
                        size.vector2Value = new Vector2(size.vector2Value.x, size.vector2Value.x);
                        size.serializedObject.ApplyModifiedProperties();
                    } 

                    break;
                case CapsuleDirection2D.Horizontal:
                    if (size.vector2Value.x < size.vector2Value.y) {
                        size.vector2Value = new Vector2(size.vector2Value.y, size.vector2Value.y);
                        size.serializedObject.ApplyModifiedProperties();
                    }
                    break;
            }
             

        }


        //protected override bool LimitSize(Vector2 size)
        //{
        //    switch (overlapCapsule.Direction)
        //    {
        //        case CapsuleDirection2D.Vertical:
        //            if (size.y < size.x) 
        //                return false;

        //            break;
        //        case CapsuleDirection2D.Horizontal:
        //            if (size.x < size.y)
        //                return false;
        //            break; 
        //    }


        //    return base.LimitSize(size);
        //}

    }
}
#endif
