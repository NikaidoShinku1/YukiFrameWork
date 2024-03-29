using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace YukiFrameWork
{
    public partial class BezierVisualTool
    {
#if UNITY_EDITOR
        [Label("在场景中检视"), BoolanPopup, SerializeField, GUIGroup("编辑器拓展")]
        private bool isScene;

        public bool IsScene => isScene;
#endif
        [Label("设置坐标的方式"),GUIGroup("基本设置")]
        public PointType pointType = PointType.Transform;

        [Label("设置贝塞尔曲线的阶数"), GUIGroup("基本设置")]
        public BezierStage stage = BezierStage.一阶;
        [GUIGroup("端点设置"), Label("起始点坐标")
            , SerializeField, DisableEnumValueIf("pointType", PointType.Transform)]  
         private Vector3 start;
       
        [SerializeField,GUIGroup("端点设置")
            , Label("终点坐标")
            , DisableEnumValueIf("pointType", PointType.Transform)   ] 
        private Vector3 end;

        [SerializeField,Label("起始点Transform"), GUIGroup("端点设置"), DisableEnumValueIf("pointType", PointType.Vector)] 
        private Transform startPos;

        [Label("终点Transform"), SerializeField, GUIGroup("端点设置"), DisableEnumValueIf("pointType", PointType.Vector)]
         private Transform endPos;

        [Label("控制点Transform1"),GUIGroup("控制组"), DisableEnumValueIf("pointType", PointType.Vector), DisableEnumValueIf("stage", BezierStage.一阶)]
        [SerializeField]
        private Transform control1Pos;

        [Label("控制点Transform2"), GUIGroup("控制组"), DisableEnumValueIf("pointType", PointType.Vector), DisableEnumValueIf("stage", BezierStage.一阶), DisableEnumValueIf("stage", BezierStage.二阶)]
        [SerializeField]
        private Transform control2Pos;

        [Label("控制点1"), GUIGroup("控制组"), DisableEnumValueIf("pointType", PointType.Transform), DisableEnumValueIf("stage", BezierStage.一阶)]
        [SerializeField]
        private Vector3 control1;
        [Label("控制点2"), GUIGroup("控制组"), DisableEnumValueIf("stage", BezierStage.一阶), DisableEnumValueIf("stage", BezierStage.二阶), DisableEnumValueIf("pointType", PointType.Transform)]
        [SerializeField]
        private Vector3 control2;

        [Label("设置创建的坐标数量"),GUIColor(ColorType.Yellow),GUIGroup("端点设置")]
        [SerializeField,EnableIf("isScene")]
        private int count;
        [Label("端点坐标集合"),EnableIf("isScene"), GUIGroup("编辑器拓展"),HelperBox("编写时贝塞尔曲线移动不需要在Update执行,所以被调用后修改端点坐标不会影响到已经开始移动的物体")]
        public List<Vector3> paths = new List<Vector3>();

        [field:SerializeField,Label("设置移动的检测方法"),GUIGroup("基本设置")]
        public BezierRuntimeMode Mode { get; set; } = BezierRuntimeMode.OnFixedUpdate;

        public int Count => count;
        [Label("颜色的设置"), GUIGroup("编辑器拓展")]
        public Color color = Color.black;

#if UNITY_EDITOR

        [MethodButton("更新端点数量"),EnableIf(nameof(isScene))]
        public void UpdatePaths()
        {
            if (!isScene) return;
            try
            {
                Vector3 current = StartValue;
                var currentStart = StartValue;
                var currentEnd = EndValue;
                switch (stage)
                {
                    case BezierStage.一阶:
                        paths = BezierUtility.GetBezierList(currentStart, currentEnd, count);
                        break;
                    case BezierStage.二阶:
                        var currentControl1 = SecondOrderControl;
                        paths = BezierUtility.GetBezierList(currentStart, currentControl1, currentEnd, count);
                        break;
                    case BezierStage.三阶:
                        currentControl1 = SecondOrderControl;
                        var currentControl2 = ThirdOrderControl;
                        paths = BezierUtility.GetBezierList(currentStart, currentControl1, currentControl2, currentEnd, count);
                        break;
                }
            }
            catch { }
        }
        public void OnDrawGizmos()
        {          
            if (!isScene) return;
            try
            {        
                Gizmos.color = color;
                Vector3 current = StartValue;
                var currentStart = StartValue;
                var currentEnd = EndValue;                    
                for (int i = 1; i < count; i++)
                {
                    float t = i / (float)count;                  
                    switch (stage)
                    {
                        case BezierStage.一阶:                          
                            paths = BezierUtility.GetBezierList(currentStart, currentEnd,count);
                            break;
                        case BezierStage.二阶:
                            var currentControl1 = SecondOrderControl;
                            paths = BezierUtility.GetBezierList(currentStart, currentControl1, currentEnd, count);                                                
                            break;
                        case BezierStage.三阶:
                            currentControl1 = SecondOrderControl;
                            var currentControl2 = ThirdOrderControl;
                            paths = BezierUtility.GetBezierList(currentStart, currentControl1, currentControl2, currentEnd, count);                           
                            break;
                    }
                    Gizmos.DrawLine(paths[i - 1], paths[i]);            

                }             

            }
            catch { }

        }        
#endif
    }
}
