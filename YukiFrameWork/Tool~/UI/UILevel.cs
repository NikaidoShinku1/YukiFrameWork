///=====================================================
/// - FileName:      UILevel.cs
/// - NameSpace:     YukiFrameWork.Project
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   这是一个框架工具创建的脚本
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
namespace YukiFrameWork.UI
{
    //UI的层级等级划分
    public enum UILevel
    {
        //背景层
        BG = 0,
        //底层
        Buttom,
        //普通层
        Common,
        //动画层
        Animation,
        //弹出层
        Pop,
        //常驻数据层
        Const,
        //前置层
        Forward,
        //系统层
        System,
        //最顶层
        Top
    }
}