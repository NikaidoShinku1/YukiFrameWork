using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;

#endif
using UnityEngine;

namespace YukiFrameWork.Overlap 
{
    public enum OverlapAreaMode { 
        
        Position , 
        Transform
    }

    /// <summary>
    /// 区域检测2D
    /// </summary>
    public class OverlapArea2D : OverlapBase2D
    {

        #region 字段

        [SerializeField]
        [InfoBox("检测坐标方式")]
        private OverlapAreaMode _mode = OverlapAreaMode.Position;

        [SerializeField]
        [InfoBox("检测区域坐标"),ShowIf(nameof(_mode),OverlapAreaMode.Position)]
        protected List<Vector3> positions = new List<Vector3>();

        [SerializeField]
        [InfoBox("检测区域位置"),ShowIf(nameof(_mode), OverlapAreaMode.Transform)]
        protected List<Transform> transforms = new List<Transform>();


        #endregion


        #region 属性

        /// <summary>
        /// 检测坐标模式
        /// </summary>
        public OverlapAreaMode Mode
        {
            get {
                return _mode;
            }

            set { 
                _mode = value;
            }

        }

        /// <summary>
        /// 检测区域坐标
        /// </summary>
        public List<Vector3> Positions => positions;
         
        /// <summary>
        /// 检测区域位置
        /// </summary>
        public List<Transform> Transforms => transforms;

        #endregion


        #region 方法

        public override void CheckOverlap()
        {
            ClearResults();

            switch (Mode)
            {
                case OverlapAreaMode.Position:

                    if (positions == null || positions.Count < 2)
                        return;

                    for (int i = 0; i < positions.Count - 1; i++)
                    {
                        Vector3 start = transform.localToWorldMatrix.MultiplyPoint(positions[i]);
                        Vector3 to = transform.localToWorldMatrix.MultiplyPoint(positions[i + 1]);

                        int count = Physics2D.OverlapAreaNonAlloc(start, to, results, layerMask.value);

                        for (int j = 0; j < count; j++) 
                        {
                            if (results[j].isTrigger) continue;

                            AddResults(results[j], Vector3.Lerp(start, to, 0.5f));
                        }

                    }

                    break;
                case OverlapAreaMode.Transform:


                    if (transforms == null || transforms.Count < 2)
                        return;

                    for (int i = 0; i < transforms.Count - 1; i++)
                    {
                        if (transforms[i] == null || transforms[i + 1] == null)
                            continue;

                        Vector3 start = transforms[i].position;
                        Vector3 to = transforms[i + 1].position;

                        int count = Physics2D.OverlapAreaNonAlloc(start, to, results, layerMask.value);

                        for (int j = 0; j < count; j++)
                        {
                            if (results[j].isTrigger) continue;

                            AddResults(results[j], Vector3.Lerp(start, to, 0.5f));
                        }
                    }

                    break;
            }

            OverlapEnd();
        }


        #endregion


#if UNITY_EDITOR
     
        private void OnDrawGizmos() 
        {

            switch (Mode)
            {
                case OverlapAreaMode.Position:

                    if (positions == null || positions.Count < 2) 
                        return;

                    for (int i = 0; i < positions.Count - 1; i++) 
                    { 
                        Vector3 start = transform.localToWorldMatrix.MultiplyPoint(positions[i]);
                        Vector3 to = transform.localToWorldMatrix.MultiplyPoint(positions[i + 1]);

                        Handles.color = Color.green;
                        Handles.DrawLine(start, to);
                    }

                    break;
                case OverlapAreaMode.Transform:


                    if (transforms == null || transforms.Count < 2)
                        return;

                    for (int i = 0; i < transforms.Count - 1; i++)
                    {
                        if(transforms[i] == null || transforms[i + 1] == null)
                            continue;

                        Vector3 start = transforms[i].position;
                        Vector3 to = transforms[i + 1].position;

                        Handles.color = Color.green;
                        Handles.DrawLine(start, to);
                    }

                    break; 
            }

        }

#endif


    }

}
