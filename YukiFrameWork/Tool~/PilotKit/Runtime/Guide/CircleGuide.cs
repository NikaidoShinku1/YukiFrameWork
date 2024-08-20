///=====================================================
/// - FileName:      CircleGuide.cs
/// - NameSpace:     YukiFrameWork.Pilot
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/8/14 19:14:03
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.Pilot
{
    public class CircleGuide : GuideBase
    {
        private float radius;
        private float scaleR;              
        protected override void OnGuide(RectTransform target, GuideInfo info)
        {
            radius = Mathf.Sqrt(height * height + width * width);
            if (info.isScaling)
            {
                scaleR = radius * info.scale;
                this.material.SetFloat("_Slider", scaleR);
            }
            else
            {                
                material.SetFloat("_Slider", radius);
            }
        }



        protected override void ScaleExecute(float timer)
        {
            base.ScaleExecute(timer);
            this.material.SetFloat("_Slider", Mathf.Lerp(scaleR, radius, timer));
        }
    }
}
