///=====================================================
/// - FileName:      BackgroundAdaptation.cs
/// - NameSpace:     YukiFrameWork.StateMachine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/8 18:53:39
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.UI
{
    /// <summary>
    /// 背景适配 ( 原理是让当前的游戏物体 与 屏幕大小保持一致 )
    /// </summary>
    public class BackgroundAdaptation : MonoBehaviour
    {
        private float currentScreenWidth;
        private float currentScreenHeight;

        private float width;
        private float height;

        private Canvas canvas;
        private RectTransform canvasRectTransform;

        private void Awake()
        {
            RectTransform rect = GetComponent<RectTransform>();
            if (rect == null) return;
            width = rect.rect.width;
            height = rect.rect.height;

            canvas = GetComponentInParent<Canvas>();
            canvasRectTransform = canvas.GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            Refresh();
            ScreenTool.OnScreenChanged.RegisterEvent(Refresh);
        }

        private void OnDisable()
        {
            ScreenTool.OnScreenChanged.UnRegister(Refresh);
        }


        // 如果能够动态调整分辨率 这里需要监听对应事件刷新( 暂时不考虑 )
        private void Refresh()
        {
            if (currentScreenWidth == canvasRectTransform.rect.width && currentScreenHeight == canvasRectTransform.rect.height)
                return;

            currentScreenWidth = canvasRectTransform.rect.width;
            currentScreenHeight = canvasRectTransform.rect.height;

            float scale_x = currentScreenWidth / width;
            float scale_y = currentScreenHeight / height;
            float s = scale_x > scale_y ? scale_x : scale_y;

            if (s < 1) s = 1; // 不考虑缩小的情况

            transform.localScale = new Vector3(s, s, s);
        }
    }
}
