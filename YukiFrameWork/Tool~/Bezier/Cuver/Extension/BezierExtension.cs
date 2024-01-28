using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YukiFrameWork.Extension;


namespace YukiFrameWork
{
    [ClassAPI("贝塞尔曲线API拓展")]
    public static class BezierExtension 
    {
        public static BezierCore BezierTowards(this Transform transform,BezierVisualTool tool,float currentSpeed)
        {
            transform.position = tool.StartValue;
            return tool.stage switch
            {
                BezierStage.一阶 => BezierTowards(transform, tool.EndValue, currentSpeed, tool.Mode),
                BezierStage.二阶 => BezierTowards(transform, tool.SecondOrderControl, tool.EndValue, currentSpeed, tool.Mode),
                BezierStage.三阶 => BezierTowards(transform, tool.SecondOrderControl, tool.ThirdOrderControl, currentSpeed, tool.Mode),
                _ => null
            } ; 
        }
        public static BezierCore BezierTowards(this Transform transform, Vector3 end, float currentSpeed, BezierRuntimeMode mode = BezierRuntimeMode.OnFixedUpdate, float pointCount = 50)
        {         
            return BezierExecute(transform, transform.position, end, currentSpeed, mode, BezierUtility.GetBezierList(transform.position, end, pointCount));
        }

        public static BezierCore BezierTowards(this Transform transform, Vector3 controlPoint1, Vector3 end, float currentSpeed, BezierRuntimeMode mode = BezierRuntimeMode.OnFixedUpdate, float pointCount = 50)
        {
            return BezierExecute(transform, transform.position, end, currentSpeed, mode, BezierUtility.GetBezierList(transform.position, controlPoint1, end, pointCount));
        }

        public static BezierCore BezierTowards(this Transform transform, Vector3 controlPoint1, Vector3 controlPoint2, Vector3 end, float currentSpeed, BezierRuntimeMode mode = BezierRuntimeMode.OnFixedUpdate, float pointCount = 50)
        {
            return BezierExecute(transform,transform.position, end, currentSpeed, mode, BezierUtility.GetBezierList(transform.position, controlPoint1, controlPoint2, end, pointCount));
        }

        private static BezierCore BezierExecute(this Transform transform,Vector3 start, Vector3 end, float currentSpeed, BezierRuntimeMode mode, List<Vector3> list)
        {
            BezierExecution execution = transform.GetComponent<BezierExecution>();
            if (execution == null)
                execution = transform.gameObject.AddComponent<BezierExecution>();
            var core = BezierCore.Get(mode, transform, list);
            core.OnUpdate += () =>
            {
                Vector3 dir = OnUpdate(transform, core, currentSpeed, end);
                return dir == end;
            };

            execution.AddCore(core);
            return core;
        }

        private static Vector3 OnUpdate(Transform transform, BezierCore core, float currentSpeed, Vector3 end)
        {
            float distanceToMove = currentSpeed * Time.fixedDeltaTime;
            bool indexCondition = core.index >= core.paths.Count - 1;

            if (!indexCondition!)
            {
                float segmentLength = (core.paths[core.index + 1] - core.paths[core.index]).magnitude;
                Update_TotalLength(core, distanceToMove, segmentLength, indexCondition);
            }
            Vector3 currentPoint;

            if (core.index > core.paths.Count - 2)
            {
                core.index = core.paths.Count - 1;
                float length = end.magnitude - core.paths[core.index].magnitude;
                Update_TotalLength(core, distanceToMove, length, Vector3.Distance(transform.position, end) < 0.1f);

                currentPoint = BezierUtility.BezierIntepolate(core.paths[core.index], end, core.t);
            }
            else
            {
                currentPoint = BezierUtility.BezierIntepolate(core.paths[core.index], core.paths[core.index + 1], core.t);
            }

            transform.SetPositionAndRotation(currentPoint, Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(currentPoint - transform.position), Time.fixedDeltaTime * 5));

            if (Vector3.Distance(transform.position, end) < 0.1f)
            {
                transform.position = end;
                return end;
            }

            return transform.position;
        }
        private static void Update_TotalLength(BezierCore core, float distanceToMove, float segmentLength, bool condition)
        {
            // 循环，直到移动完这一帧应该移动的距离
            while (distanceToMove > 0)
            {
                if (condition)
                {
                    core.index = core.paths.Count - 1;
                    break;
                }
                float distanceToNextPoint = segmentLength * (1 - core.t);

                if (distanceToMove < distanceToNextPoint)
                {
                    core.t += distanceToMove / segmentLength;
                    core.currentLength += distanceToMove;
                    break;
                }
                else
                {
                    distanceToMove -= distanceToNextPoint;
                    core.currentLength += distanceToNextPoint;
                    core.index++;
                    core.t = 0;
                }
            }
        }
    }

}
