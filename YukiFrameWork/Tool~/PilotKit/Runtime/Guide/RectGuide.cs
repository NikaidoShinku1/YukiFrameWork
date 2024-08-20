///=====================================================
/// - FileName:      RectGuide.cs
/// - NameSpace:     YukiFrameWork.Pilot
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/8/14 18:15:46
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.Pilot
{
	public class RectGuide : GuideBase
	{
        private float scaleWidth;
        private float scaleHeight;
        protected override void OnGuide(RectTransform target,GuideInfo info)
		{          
            if(info.isScaling)
            {
                scaleWidth = width * info.scale;
                scaleHeight = height * info.scale;
            }          
            material.SetFloat("_SliderX", info.isScaling ? scaleWidth : width);
			material.SetFloat("_SliderY", info.isScaling ? scaleHeight: height);
        }

        protected override void ScaleExecute(float timer)
        {
            base.ScaleExecute(timer);
            this.material.SetFloat("_SliderX", Mathf.Lerp(scaleWidth, width, timer));
            this.material.SetFloat("_SliderY", Mathf.Lerp(scaleHeight, height, timer));
        }
    }
}
