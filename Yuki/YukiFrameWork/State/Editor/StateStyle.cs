///=====================================================
/// - FileName:      StateStyle.cs
/// - NameSpace:     YukiFrameWork.Project
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   这是一个框架工具创建的脚本
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

#if UNITY_EDITOR
using UnityEngine;
using System;
using System.Collections.Generic;

namespace YukiFrameWork.States
{
    public enum Style
    {
        Normal = 0,
        Blue,
        Mint,
        Green,
        Yellow,
        Orange,
        Red,
        NormalOn,
        BlueOn,
        MintOn, 
        GreenOn,
        YellowOn,
        OrangeOn,
        RedOn,
        SelectionRect,
    }
    public class StateStyle
    {
        private readonly Dictionary<int,GUIStyle> styleDict
            = new Dictionary<int,GUIStyle>();

        public StateStyle()
        {
            for (int i = 0; i <= 6; i++)
            {
                styleDict.Add(i, new GUIStyle(string.Format("flow node {0}", i)));
                styleDict.Add(i + 7, new GUIStyle(string.Format("flow node {0} on", i)));
            }

            styleDict.Add(14, new GUIStyle("SelectionRect"));
        }

        public GUIStyle GetStyle(Style style)
            => styleDict[(int)style];
    }
}
#endif