using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Sirenix.OdinInspector;
namespace YukiFrameWork
{
    public partial class BezierVisualTool
    {
#if UNITY_EDITOR
        [LabelText("在场景中检视"),SerializeField, FoldoutGroup("编辑器拓展",order:1)]
        private bool isScene;

        public bool IsScene => isScene;
#endif
        [LabelText("设置坐标的方式"),FoldoutGroup("基本设置")]
        public PointType pointType = PointType.Transform;

        [LabelText("设置贝塞尔曲线的阶数"), FoldoutGroup("基本设置")]
        public BezierStage stage = BezierStage.一阶;
        [FoldoutGroup("端点设置",order:0), LabelText("起始点坐标")
            , SerializeField, HideIf("pointType", PointType.Transform)]  
         private Vector3 start;
       
        [SerializeField,FoldoutGroup("端点设置")
            , LabelText("终点坐标")
            , HideIf("pointType", PointType.Transform)   ] 
        private Vector3 end;

        [SerializeField,LabelText("起始点Transform"), FoldoutGroup("端点设置"), HideIf("pointType", PointType.Vector)] 
        private Transform startPos;

        [LabelText("终点Transform"), SerializeField, FoldoutGroup("端点设置"), HideIf("pointType", PointType.Vector)]
         private Transform endPos;

        [LabelText("控制点Transform1"),FoldoutGroup("控制组"), ShowIf(nameof(IsTransformInThirdLoader))]
        [SerializeField]       
        private Transform control1Pos;

        [LabelText("控制点Transform2"), FoldoutGroup("控制组"),ShowIf(nameof(IsTransformInSecondLoader))]
        [SerializeField]          
        private Transform control2Pos;

        [LabelText("控制点1"), FoldoutGroup("控制组"), ShowIf(nameof(IsVertorInThirdLoader))]
        [SerializeField]
        private Vector3 control1;
        [LabelText("控制点2"), FoldoutGroup("控制组"), ShowIf(nameof(IsVertorInSecondLoader))]
        [SerializeField]
        private Vector3 control2;

        private bool checkThirdLoader => (stage == BezierStage.二阶 || stage == BezierStage.三阶);
        private bool checkSecondLoader => stage == BezierStage.三阶;
        private bool IsTransformInThirdLoader => checkThirdLoader && pointType == PointType.Transform;
        private bool IsTransformInSecondLoader => checkSecondLoader && pointType == PointType.Transform;

        private bool IsVertorInThirdLoader => checkThirdLoader && pointType == PointType.Vector;
        private bool IsVertorInSecondLoader => checkSecondLoader && pointType == PointType.Vector;
        [LabelText("设置创建的坐标数量"),GUIColor("yellow"),FoldoutGroup("端点设置")]
        [SerializeField]
        private int count;
        [LabelText("端点坐标集合"),ShowIf("isScene"), FoldoutGroup("编辑器拓展"),InfoBox("编写时贝塞尔曲线移动不需要在Update执行,所以被调用后修改端点坐标不会影响到已经开始移动的物体")]
        public List<Vector3> paths = new List<Vector3>();

        [field:SerializeField,LabelText("设置移动的检测方法"),FoldoutGroup("基本设置")]
        public BezierRuntimeMode Mode { get; set; } = BezierRuntimeMode.OnFixedUpdate;

        public int Count => count;
        [LabelText("颜色的设置"), FoldoutGroup("编辑器拓展")]
        public Color color = Color.black;

#if UNITY_EDITOR
      
        public void OnDrawGizmos()
        {          
            if (!isScene) return;
            try
            {        
                Gizmos.color = color;
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
                for (int i = 1; i < paths.Count; i++)
                {
                    Gizmos.DrawLine(paths[i - 1], paths[i]);            
                }             

            }
            catch { }

        }        
#endif
    }
}
