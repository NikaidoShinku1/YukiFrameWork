using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Sirenix.OdinInspector;
namespace YukiFrameWork.Overlap 
{
    /// <summary>
    /// 重叠检测球形
    /// </summary>
    public class OverlapSphere3D : OverlapBase3D
    {

        #region 字段

        [SerializeField]
        [InfoBox("半径")]
        private float radius = 0.5f;

        #endregion

        #region 属性

        /// <summary>
        /// 半径
        /// </summary>
        public float Radius
        {
            get
            { 
                return radius;
            }
            set
            {
                radius = value; 
            }
        }

        #endregion

        // Fix编码
        public override void CheckOverlap()
        {
            Vector3 scale = transform.lossyScale;
            float r = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)) * radius;
            int count = Physics.OverlapSphereNonAlloc(transform.position, r, results, layerMask.value);
            
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
            //Handles.SphereHandleCap(0, transform.position, transform.rotation, radius, EventType.Repaint);

            Handles.color = Color.green;
            Vector3 scale = transform.lossyScale;
            float r = Mathf.Max(Mathf.Abs( scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z)) * radius;

            Handles.DrawWireDisc(transform.position, Vector3.up, r);
            Handles.DrawWireDisc(transform.position, Vector3.forward, r);
            Handles.DrawWireDisc(transform.position, Vector3.right, r);

            Handles.DrawWireDisc(transform.position, Vector3.forward + Vector3.right, r);
            Handles.DrawWireDisc(transform.position, Vector3.forward - Vector3.right, r);
        }

#endif



    }

}

