using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


namespace YukiFrameWork.Overlap 
{
    /// <summary>
    /// 重叠检测矩形2D
    /// </summary>
    public class OverlapBox2D : OverlapBase2D
    {

        #region 字段
        [SerializeField]
        protected Vector2 offset;
        [SerializeField]
        protected Vector2 size = Vector2.one;

        #endregion


        #region 属性

        /// <summary>
        /// 偏移
        /// </summary>
        public Vector2 Offset {
            get { 
                return offset;
            }
            set { 
                offset = value;
            }
        }

        /// <summary>
        /// 大小
        /// </summary>
        public Vector2 Size {
            get { 
                return size;
            }
            set {
                size = value;
            }
        }

        #endregion


        // Fix编码

        #region 方法

        public override void CheckOverlap()
        {
            Vector3 center = transform.localToWorldMatrix.MultiplyPoint(offset);
  

            ClearResults();

            Vector2 s = size * transform.lossyScale;

            s.Set(Mathf.Abs(s.x), Mathf.Abs(s.y));


            int count = Physics2D.OverlapBoxNonAlloc(center, s, transform.eulerAngles.z, results, layerMask.value); 
            
            for (int i = 0; i < count; i++)
            {
                if (results[i].isTrigger) continue;
                AddResults(results[i],center);
            }

            OverlapEnd();
        
        }

        #endregion

#if UNITY_EDITOR

        protected virtual void OnDrawGizmos()
        {
            Handles.color = Color.green;

            Quaternion rotation = Quaternion.Euler(0, 0, transform.eulerAngles.z); // 例如，绕Y轴旋转45度

            Matrix4x4 oldMat = Handles.matrix;

            Vector3 center = transform.localToWorldMatrix.MultiplyPoint(offset);

            Matrix4x4 gizmoMatrix = Matrix4x4.TRS(center, rotation, oldMat.lossyScale);

            Handles.matrix = gizmoMatrix;
             

            Handles.DrawWireCube(Vector3.zero,size * transform.lossyScale);
             
            Handles.matrix = oldMat;  
        } 
#endif

    }

}
