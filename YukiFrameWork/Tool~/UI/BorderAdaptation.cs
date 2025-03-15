///=====================================================
/// - FileName:      BorderAdaptation.cs
/// - NameSpace:     YukiFrameWork.StateMachine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/8 18:55:15
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.UI
{
    public enum BorderType
    {
        Left,
        Right
    }

    /// <summary>
    /// 边界适配
    /// </summary>
    public class BorderAdaptation : MonoBehaviour
    {
        /// <summary>
        /// 边界类型
        /// </summary>
        [SerializeField]
        private BorderType borderType = BorderType.Right;

        /// <summary>
        /// 最小偏移
        /// </summary>
        [SerializeField]
        private float minOffset = 0;

        /// <summary>
        /// 最大偏移
        /// </summary>
        [SerializeField]
        private float maxOffset = 100;

        // Start is called before the first frame update
        private void OnEnable()
        {
            Refresh_Offset();
            ScreenTool.OnScreenChanged.RegisterEvent(Refresh_Offset);
        }

        private void OnDisable()
        {
            ScreenTool.OnScreenChanged.UnRegister(Refresh_Offset);
        }

        private void Refresh_Offset()
        {
            float value = Mathf.InverseLerp(1.77f, 2.22f, (float)Screen.width / Screen.height);

            switch (borderType)
            {
                case BorderType.Left:
                    transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(Mathf.Lerp(minOffset, maxOffset, value), 0);
                    break;
                case BorderType.Right:
                    transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(-Mathf.Lerp(minOffset, maxOffset, value), 0);
                    break;
            }
        }

    }
}
