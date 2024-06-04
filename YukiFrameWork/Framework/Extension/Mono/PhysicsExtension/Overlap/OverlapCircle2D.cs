using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace YukiFrameWork.Overlap 
{
    /// <summary>
    /// 重叠检测圆形2D
    /// </summary>
    public class OverlapCircle2D : OverlapBase2D
    {
        #region 字段

        [SerializeField]
        private float radius = 0.5f;

        #endregion


        #region 属性 
        /// <summary>
        /// 半径
        /// </summary>
        public float Radius
        {
            get { 
                return radius;
            }
            set { 
                radius = value;
            }

        }

        #endregion



        // Fix编码
        public override void CheckOverlap()
        {
            Vector3 s = transform.lossyScale;
            float scale = Mathf.Abs(s.x) > Mathf.Abs(s.y) ? Mathf.Abs(s.x) : Mathf.Abs(s.y);
            int count = Physics2D.OverlapCircleNonAlloc(transform.position, radius * scale, results, layerMask.value);
            ClearResults();
            for (int i = 0; i < count; i++)
            {
                if (results[i].isTrigger) continue;
                AddResults(results[i], transform.position);
            }

            OverlapEnd();
        }

#if UNITY_EDITOR

        private void OnDrawGizmos() 
        {
            Handles.color = Color.green; 
            Vector3 s = transform.lossyScale; 
            float scale = Mathf.Abs(s.x) > Mathf.Abs(s.y) ? Mathf.Abs(s.x) : Mathf.Abs(s.y);
            Handles.DrawWireDisc(transform.position,Vector3.forward,radius * scale);
        }

#endif


    }
}

