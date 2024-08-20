///=====================================================
/// - FileName:      GuideBase.cs
/// - NameSpace:     YukiFrameWork.Pilot
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/8/14 18:14:49
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
namespace YukiFrameWork.Pilot
{
    public abstract class GuideBase
    {
        public Material material { get; internal set; }
        
        public RectTransform target { get; internal set; }

        public Canvas root { get; internal set; }

        protected float width { get; private set; }
        protected float height { get; private set; }

        protected bool isScaling { get; private set; }

        private Vector3 startCenter;       
        protected bool isMoving { get; private set; }

        public bool IsCompleted => !isMoving && !isScaling;

        protected CoroutineTokenSource Source;    
   
        //镂空中心区域
        protected Vector3 center;

        public Vector3 Center => center;

        public static implicit operator bool(GuideBase guideBase)
        {
            return guideBase != null;
        }      
        protected Vector3[] targetCorners = new Vector3[4];      
         
        internal void OnInit(RectTransform parent)
        {
            Source = CoroutineTokenSource.Create(parent);
        }      
    
        internal void Guide(GuideInfo info)
        {           
            //获取世界坐标下的边界点
            target.GetWorldCorners(targetCorners);

            for (int i = 0; i < targetCorners.Length; i++)
            {
                targetCorners[i] = WorldToScreenPoint(targetCorners[i]);
            }
            //计算中心点
            center.x = targetCorners[0].x + (targetCorners[3].x - targetCorners[0].x) / 2;
            center.y = targetCorners[0].y + (targetCorners[1].y - targetCorners[0].y) / 2;
            isMoving = false;
            isScaling = false;
            if (Source.Token.IsRunning)
                Source.Cancel();
            Source.Running();
            //设置中心店
            switch (info.transitionType)
            {
                case TransitionType.Direct:
                    material.SetVector("_Center", center);
                    break;
                case TransitionType.Slow:
                    startCenter = material.GetVector("_Center");
                    isMoving = true;
                   
                    this.EasyTimer(info.transTime, timer =>
                    {
                        material.SetVector("_Center", Vector3.Lerp(startCenter, center, timer));
                    }, () => isMoving = false, isConstraint: true,token:Source.Token);                 
                    break;
                default:
                    break;
            }

            if (info.isScaling)
            {
                isScaling = true;
                this.EasyTimer(info.scaleTime, ScaleExecute, () => isScaling = false, isConstraint: true, token: Source.Token);
            }
            width = (targetCorners[3].x - targetCorners[0].x) / 2;
            height = (targetCorners[1].y - targetCorners[0].y) / 2;
            OnGuide(target,info);         

        }    
        protected virtual void ScaleExecute(float timer) { }
    
        /// <summary>
        /// 默认引导
        /// </summary>
        /// <param name="target"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        protected abstract void OnGuide(RectTransform target,GuideInfo info);    
       
        public virtual Vector2 WorldToScreenPoint(Vector3 world)
        {
           // 世界坐标转屏幕坐标
            Vector2 target = RectTransformUtility.WorldToScreenPoint(root.worldCamera, world);

            //屏幕坐标转局部坐标
            RectTransformUtility.ScreenPointToLocalPointInRectangle(root.GetRectTransform(), target, root.worldCamera, out var localPosition);            

            return localPosition;
        }

    }
}
