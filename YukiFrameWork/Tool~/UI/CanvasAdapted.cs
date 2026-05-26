///=====================================================
/// - FileName:      CanvasAdapted.cs
/// - NameSpace:     YukiFrameWork.UI
/// - Description:   高级定制脚本生成
/// - Creation Time: 2026/5/20 16:27:39
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using UnityEngine.UI;
using Sirenix.OdinInspector;
namespace YukiFrameWork.UI
{
     [RequireComponent(typeof(Canvas), typeof(CanvasScaler))]
     public class CanvasAdapted : YMonoBehaviour
     {
         private CanvasScaler canvasScaler;
         
         [LabelText("是否希望持续更新")]
         [InfoBox("")]
         public bool isUpdate;

         private int currentHeight;
         private int currentWidth;
         protected override void Awake()
         {
             base.Awake();
             
             canvasScaler = GetComponent<CanvasScaler>();
             if(!canvasScaler)
                 Debug.LogError("canvasScaler == null");
         }
    
         private void Update()
         {
             if (isUpdate)
             {
                 UpdateScreenAspect();
             }
         }
         
    
         /// <summary>
         /// 更新适配
         /// </summary>
         /// <summary>
         /// 可根据当前分辨率进行更新CanvasScaler画布比例方法
         /// </summary>
         public void UpdateScreenAspect()
         {
             if (Screen.width == currentWidth || Screen.height == currentHeight)
                 return;

             // 计算出比例
             float aspect = (float)Screen.width / Screen.height;
             float inverse_lerp;
             if (IsLandscape())
                 inverse_lerp = Mathf.InverseLerp(1.33f, 1.77f, aspect); // 12:9 ~ 16:9  
             else
                 inverse_lerp = Mathf.InverseLerp(9.0f / 16, 9.0f / 12, aspect); // 

             canvasScaler.matchWidthOrHeight = inverse_lerp;

             currentWidth = Screen.width;
             currentHeight = Screen.height;
         }

         private bool IsLandscape()
         {
             return Screen.width < Screen.height;
         }
         public void UpdateMatchValue(float value)
         {
             canvasScaler.matchWidthOrHeight = value;
         }
     }
}
