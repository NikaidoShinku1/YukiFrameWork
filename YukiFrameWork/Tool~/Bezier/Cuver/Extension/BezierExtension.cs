using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YukiFrameWork.Extension;


namespace YukiFrameWork
{
    [ClassAPI("贝塞尔曲线API拓展")]   
    public static class BezierExtension 
    {       
        public static IBezier BezierTowards(this Transform transform,BezierVisualTool tool,float currentSpeed)
        {
            transform.position = tool.StartValue;
            return BezierExecute(transform, tool.EndValue, currentSpeed, tool.Mode, tool.paths);
        }
        public static IBezier LocalBezierTowards(this Transform transform, BezierVisualTool tool, float currentSpeed)
        {
            tool.IsLocal = true;
            transform.localPosition = tool.StartValue;
            return BezierExecute(transform, tool.EndValue, currentSpeed, tool.Mode, tool.paths,false,true);
        }
        /// <summary>
        /// Rotated仅对3D有效
        /// </summary>      
        public static IBezier BezierAndRotateTowards(this Transform transform, BezierVisualTool tool, float currentSpeed)
        {
            transform.position = tool.StartValue;
            return BezierExecute(transform, tool.EndValue, currentSpeed, tool.Mode, tool.paths,true);
        }
        /// <summary>
        /// Rotated仅对3D有效
        /// </summary>      
        public static IBezier LocalBezierAndRotateTowards(this Transform transform, BezierVisualTool tool, float currentSpeed)
        {
            tool.IsLocal = true;
            transform.localPosition = tool.StartValue;
            return BezierExecute(transform, tool.EndValue, currentSpeed, tool.Mode, tool.paths,true,true);
        }
        /// <summary>
        /// Rotated仅对3D有效
        /// </summary>       
        public static IBezier LocalBezierAndLocalRotateTowards(this Transform transform, BezierVisualTool tool, float currentSpeed)
        {
            transform.localPosition = tool.StartValue;
            return BezierExecute(transform, tool.EndValue, currentSpeed, tool.Mode, tool.paths,true,true,true);
        }
        /// <summary>
        /// Rotated仅对3D有效
        /// </summary>       
        public static IBezier BezierAndRotateTowards(this Transform transform, Vector3 end, float currentSpeed, BezierRuntimeMode mode = BezierRuntimeMode.OnFixedUpdate, float pointCount = 50)
        {
            return BezierExecute(transform, end, currentSpeed, mode, BezierUtility.GetBezierList(transform.position, end, pointCount),true);
        }
        /// <summary>
        /// Rotated仅对3D有效
        /// </summary>       
        public static IBezier BezierAndRotateTowards(this Transform transform, Vector3 secondOrderControl, Vector3 end, float currentSpeed, BezierRuntimeMode mode = BezierRuntimeMode.OnFixedUpdate, float pointCount = 50)
        {
            return BezierExecute(transform, end, currentSpeed, mode, BezierUtility.GetBezierList(transform.position, secondOrderControl, end, pointCount),true);
        }
        /// <summary>
        /// Rotated仅对3D有效
        /// </summary>       
        public static IBezier BezierAndRotateTowards(this Transform transform, Vector3 secondOrderControl, Vector3 thirdOrderControl, Vector3 end, float currentSpeed, BezierRuntimeMode mode = BezierRuntimeMode.OnFixedUpdate, float pointCount = 50)
        {
            return BezierExecute(transform, end, currentSpeed, mode, BezierUtility.GetBezierList(transform.position, secondOrderControl, thirdOrderControl, end, pointCount), true);
        }
        /// <summary>
        /// Rotated仅对3D有效
        /// </summary>       
        public static IBezier LocalBezierAndRotateTowards(this Transform transform, Vector3 end, float currentSpeed, BezierRuntimeMode mode = BezierRuntimeMode.OnFixedUpdate, float pointCount = 50)
        {
            return BezierExecute(transform, end, currentSpeed, mode, BezierUtility.GetBezierList(transform.position, end, pointCount), true, true);
        }
        /// <summary>
        /// Rotated仅对3D有效
        /// </summary>       
        public static IBezier LocalBezierAndRotateTowards(this Transform transform, Vector3 secondOrderControl, Vector3 end, float currentSpeed, BezierRuntimeMode mode = BezierRuntimeMode.OnFixedUpdate, float pointCount = 50)
        {
            return BezierExecute(transform, end, currentSpeed, mode, BezierUtility.GetBezierList(transform.position, secondOrderControl, end, pointCount), true, true);
        }
        /// <summary>
        /// Rotated仅对3D有效
        /// </summary>       
        public static IBezier LocalBezierAndRotateTowards(this Transform transform, Vector3 secondOrderControl, Vector3 thirdOrderControl, Vector3 end, float currentSpeed, BezierRuntimeMode mode = BezierRuntimeMode.OnFixedUpdate, float pointCount = 50)
        {
            return BezierExecute(transform, end, currentSpeed, mode, BezierUtility.GetBezierList(transform.position, secondOrderControl, thirdOrderControl, end, pointCount), true, true);
        }
        /// <summary>
        /// Rotated仅对3D有效
        /// </summary>       
        public static IBezier LocalBezierAndLocalRotateTowards(this Transform transform, Vector3 end, float currentSpeed, BezierRuntimeMode mode = BezierRuntimeMode.OnFixedUpdate, float pointCount = 50)
        {
            return BezierExecute(transform, end, currentSpeed, mode, BezierUtility.GetBezierList(transform.position, end, pointCount), true, true, true);
        }
        /// <summary>
        /// Rotated仅对3D有效
        /// </summary>       
        public static IBezier LocalBezierAndLocalRotateTowards(this Transform transform, Vector3 secondOrderControl, Vector3 end, float currentSpeed, BezierRuntimeMode mode = BezierRuntimeMode.OnFixedUpdate, float pointCount = 50)
        {
            return BezierExecute(transform, end, currentSpeed, mode, BezierUtility.GetBezierList(transform.position, secondOrderControl, end, pointCount), true, true, true);
        }
        /// <summary>
        /// Rotated仅对3D有效
        /// </summary>       
        public static IBezier LocalBezierAndLocalRotateTowards(this Transform transform, Vector3 secondOrderControl, Vector3 thirdOrderControl, Vector3 end, float currentSpeed, BezierRuntimeMode mode = BezierRuntimeMode.OnFixedUpdate, float pointCount = 50)
        {
            return BezierExecute(transform, end, currentSpeed, mode, BezierUtility.GetBezierList(transform.position, secondOrderControl, thirdOrderControl, end, pointCount), true, true,true);
        }
        public static IBezier BezierTowards(this Transform transform, Vector3 end, float currentSpeed, BezierRuntimeMode mode = BezierRuntimeMode.OnFixedUpdate, float pointCount = 50)
        {         
            return BezierExecute(transform,  end, currentSpeed, mode, BezierUtility.GetBezierList(transform.position, end, pointCount));
        }
        public static IBezier BezierTowards(this Transform transform, Vector3 secondOrderControl, Vector3 end, float currentSpeed, BezierRuntimeMode mode = BezierRuntimeMode.OnFixedUpdate, float pointCount = 50)
        {
            return BezierExecute(transform,  end, currentSpeed, mode, BezierUtility.GetBezierList(transform.position, secondOrderControl, end, pointCount));
        }
        public static IBezier BezierTowards(this Transform transform, Vector3 secondOrderControl, Vector3 thirdOrderControl, Vector3 end, float currentSpeed, BezierRuntimeMode mode = BezierRuntimeMode.OnFixedUpdate, float pointCount = 50)
        {
            return BezierExecute(transform, end, currentSpeed, mode, BezierUtility.GetBezierList(transform.position, secondOrderControl, thirdOrderControl, end, pointCount));
        }
        public static IBezier LocalBezierTowards(this Transform transform, Vector3 end, float currentSpeed, BezierRuntimeMode mode = BezierRuntimeMode.OnFixedUpdate, float pointCount = 50)
        {
            return BezierExecute(transform, end, currentSpeed, mode, BezierUtility.GetBezierList(transform.position, end, pointCount), false, true);
        }
        public static IBezier LocalBezierTowards(this Transform transform, Vector3 secondOrderControl, Vector3 end, float currentSpeed, BezierRuntimeMode mode = BezierRuntimeMode.OnFixedUpdate, float pointCount = 50)
        {
            return BezierExecute(transform, end, currentSpeed, mode, BezierUtility.GetBezierList(transform.position, secondOrderControl, end, pointCount), false, true);
        }
        public static IBezier LocalBezierTowards(this Transform transform, Vector3 secondOrderControl, Vector3 thirdOrderControl, Vector3 end, float currentSpeed, BezierRuntimeMode mode = BezierRuntimeMode.OnFixedUpdate, float pointCount = 50)
        {
            return BezierExecute(transform, end, currentSpeed, mode, BezierUtility.GetBezierList(transform.position, secondOrderControl, thirdOrderControl, end, pointCount),false,true);
        }

        private static IBezier BezierExecute(this Transform transform, Vector3 end, float currentSpeed, BezierRuntimeMode mode, List<Vector3> list,bool rotated = false,bool localPosition = false,bool localRotation = false)
        {
            BezierExecution execution = transform.GetComponent<BezierExecution>();
            if (execution == null)
                execution = transform.gameObject.AddComponent<BezierExecution>();
            list.Add(end);
            var core = Bezier.Get(mode,  list);
            core.Condition += () =>
            {
                Vector3 dir = OnUpdate(transform, core, currentSpeed, end,rotated,localPosition,localRotation);              
                return dir == end;
            };

            execution.AddCore(core);
            return core as IBezier;
        }

        private static Vector3 OnUpdate(Transform transform, Bezier core, float currentSpeed, Vector3 end, bool rotated = false, bool localPosition = false,bool localRotation = false)
        {
            float distanceToMove = currentSpeed * Time.fixedDeltaTime;
            bool indexCondition = core.index >= core.paths.Count - 1;

            if (!indexCondition)
            {
                float segmentLength = (core.paths[core.index + 1] - core.paths[core.index]).magnitude;
                Update_TotalLength(core, distanceToMove, segmentLength, indexCondition);             
            }
            Vector3 currentPoint;

            if (core.index < core.paths.Count - 1)
            {
                currentPoint = BezierUtility.BezierIntepolate(core.paths[core.index], core.paths[core.index + 1], core.t);             
                if (localPosition)                                
                    transform.localPosition = currentPoint;               
                else
                    transform.position = currentPoint;

                if (rotated)
                {                  
                    if (localRotation)
                        transform.localRotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(core.paths[core.index + 1] - core.paths[core.index]), Time.fixedDeltaTime * 5); 
                    else
                        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(core.paths[core.index + 1] - core.paths[core.index]), Time.fixedDeltaTime * 5); ;                 
                }
            }      

            if (Vector3.Distance(transform.position, end) < 0.1f)
            {
                transform.position = end;
                return end;
            }

            return transform.position;
        }
        private static void Update_TotalLength(Bezier core, float distanceToMove, float segmentLength, bool condition)
        {
            // 循环，直到移动完这一帧应该移动的距离
            while (true)
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
