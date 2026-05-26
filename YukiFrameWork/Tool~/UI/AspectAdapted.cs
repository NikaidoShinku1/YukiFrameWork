///=====================================================
/// - FileName:      AspectHeightAdapted.cs
/// - NameSpace:     YukiFrameWork.Example
/// - Description:   高级定制脚本生成
/// - Creation Time: 2026/2/9 11:31:43
/// -  (C) Copyright 2008 - 2026
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System;
using Sirenix.OdinInspector;

namespace YukiFrameWork.UI
{
    public enum AspectMode
    {
        Width,
        Height,
    } public class AspectAdapted : YMonoBehaviour
    {
        
        public AspectMode aspectMode;

        [LabelText("是否在激活时触发")]
        public bool onEnabled = true;
    
        [LabelText("是否使长宽都跟随等比")]
        public bool onScale = false;

        public float widthScale = 16f;
        public float heightScale = 9f;

        [LabelText("自动反转")]
        [InfoBox("可自动兼容长宽屏幕")]
        public bool autoInversal;

        [LabelText("X原始比例")]
        public float defaultScaleX = 1;
        [LabelText("Y原始比例")]
        public float defaultScaleY = 1;

        private void OnEnable()
        {
            
            if (autoInversal)
            {
                if (Screen.width > Screen.height)
                {
                    aspectMode = AspectMode.Width;
                }
                else aspectMode = AspectMode.Height;
            }

            if(onEnabled)
                Adapted();
        }

        public void Adapted()
        {
            Adapted(aspectMode);
        }

        public void Adapted(AspectMode aspectMode)
        {
            float defaultAspect;
            float aspectRatio = (float)Screen.height / Screen.width;
            switch (aspectMode)
            {
                case AspectMode.Width:
                { 
                    defaultAspect = heightScale / widthScale;
                    float scale = defaultAspect / aspectRatio;
                    transform.SetLocalScaleX(scale * defaultScaleX);
                    if (onScale)
                        transform.SetLocalScaleY(scale * defaultScaleY);
                }
                    break;
                case AspectMode.Height:
                { 
                    defaultAspect = widthScale / heightScale;
                    float scale = defaultAspect / aspectRatio;
                    transform.SetLocalScaleX(scale * defaultScaleX);

                    if (onScale)
                        transform.SetLocalScaleY(scale * defaultScaleY);
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(aspectMode), aspectMode, null);
            }
            
            
            
        }
    }
}
