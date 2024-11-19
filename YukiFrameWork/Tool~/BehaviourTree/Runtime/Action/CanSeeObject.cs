///=====================================================
/// - FileName:      CanSeeObject.cs
/// - NameSpace:     YukiFrameWork.Behaviours
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/11/19 14:12:39
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace YukiFrameWork.Behaviours
{
	public class CanSeeObject : Action
	{
        public Vector3 offect;

        [Range(1,360)]
        public float angle = 1;
        [Range(1,100)]
        public float radius = 1;

        [BehaviourParam]
        public GameObject target;

        public override BehaviourStatus OnUpdate()
        {
            if (!target) return BehaviourStatus.Failed;

            var targetPosition = target.transform.position;
            Vector3 rangeCenter = transform.position + offect;
          
            Vector3 directionToTarget = targetPosition - rangeCenter;
            float distanceToTarget = directionToTarget.magnitude;

            // 如果目标点的距离大于半径，说明不在范围内
            if (distanceToTarget > radius)
            {               
                return BehaviourStatus.Running;
            }

            // 计算目标点的角度（以transform.forward为基准方向）
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget.normalized);

            // 判断目标点是否在角度范围内
            if (angleToTarget <= angle)
            {
                return BehaviourStatus.Success;
            }
            else
            {
                return BehaviourStatus.Running;
            }
        }
#if UNITY_EDITOR
        public override void DrawGizmos(Transform transform)
        {
            base.DrawGizmos(transform);
            Handles.color = new Color(0.7f, 1, 0.9f, 0.6f);
            Handles.DrawSolidArc(transform.position + offect, transform.up, transform.forward, angle, radius);
            Handles.DrawSolidArc(transform.position + offect, transform.up, transform.forward, -angle, radius);
        }
#endif
    }
}
