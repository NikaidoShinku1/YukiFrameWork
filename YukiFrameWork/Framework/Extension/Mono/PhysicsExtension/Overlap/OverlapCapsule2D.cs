using System.Collections;
using System.Collections.Generic;
using System.Drawing;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace YukiFrameWork.Overlap
{
    /// <summary>
    /// 重叠检测胶囊2D
    /// </summary>
    public class OverlapCapsule2D : OverlapBox2D
    {

        #region 字段
        [SerializeField]
        private CapsuleDirection2D direction;

        #endregion

        #region 属性

        /// <summary>
        /// 方向
        /// </summary>
        public CapsuleDirection2D Direction
        {
            get {
                return direction;
            }
            set {
                direction = value;
            }
        }

        #endregion


        public override void CheckOverlap()
        {
            //base.CheckOverlap(); 
            Vector3 center = transform.localToWorldMatrix.MultiplyPoint(offset);

            ClearResults();

            Vector2 s = size * transform.lossyScale;

            s.Set(Mathf.Abs(s.x), Mathf.Abs(s.y));

            int count = Physics2D.OverlapCapsuleNonAlloc(center, s, direction, transform.eulerAngles.z, results, layerMask.value);

            for (int i = 0; i < count; i++)
            {
                if (results[i].isTrigger) continue;
                AddResults(results[i], center);
            }

            OverlapEnd();
        }

#if UNITY_EDITOR

        protected override void OnDrawGizmos()
        {
            Handles.color = UnityEngine.Color.green;

            Quaternion rotation = Quaternion.Euler(0, 0, transform.eulerAngles.z); // 例如，绕Y轴旋转45度

            Matrix4x4 oldMat = Handles.matrix;

            Vector3 center = transform.localToWorldMatrix.MultiplyPoint(offset);

            Matrix4x4 gizmoMatrix = Matrix4x4.TRS(center, rotation, oldMat.lossyScale);
            Handles.matrix = gizmoMatrix;

            Vector3.zero.DrawCapsule2D(size * transform.lossyScale, direction);

            Handles.matrix = oldMat;
        }

#endif


    }

}
