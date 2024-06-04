using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace YukiFrameWork.Overlap 
{

    /// <summary>
    /// 重叠检测立方体3D
    /// </summary>
    public class OverlapBox3D : OverlapBase3D
    {
        #region 字段
         

        [SerializeField]
        private Vector3 size = Vector3.one;

        #endregion

        #region 属性

        /// <summary>
        /// 大小
        /// </summary>
        private Vector3 Size
        {
            get { 
                return size;
            }
            set { 
                size = value;
            }
        }

        #endregion



        // Fix编码
        public override void CheckOverlap()
        {
            Vector3 lossy = transform.lossyScale; 
            Vector3 s = new Vector3( Mathf.Abs( size.x * lossy.x * 0.5f), Mathf.Abs( size.y * lossy.y * 0.5f),Mathf.Abs(  size.z * lossy.z * 0.5f));

            int count = Physics.OverlapBoxNonAlloc(transform.position, s, results, transform.rotation, layerMask.value);
            ClearResults();

            for (int i = 0; i < count; i++)
            {
                if (results[i].isTrigger) continue;
                AddResults(results[i],transform.position); 
            }

            OverlapEnd();
        }

#if UNITY_EDITOR

        private void OnDrawGizmos() 
        {
            Handles.color = Color.green;

            Matrix4x4 old = Handles.matrix;

            Handles.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);

            Vector3 lossy = transform.lossyScale;

            Vector3 s = new Vector3(size.x * lossy.x, size.y * lossy.y , size.z * lossy.z );
            Handles.DrawWireCube(Vector3.zero,s);
             
            Handles.matrix = old;
        }

#endif

    }

}

