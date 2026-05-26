///=====================================================
/// - FileName:      RewinderMode.cs
/// - NameSpace:     YukiFrameWork.Example
/// - Description:   高级定制脚本生成
/// - Creation Time: 2026/5/23 15:59:33
/// -  (C) Copyright 2008 - 2030
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.Rewinder
{
    public enum RewinderMode 
    {
        /// <summary>未初始化或已释放</summary>
        Idle,
        /// <summary>正在记录</summary>
        Update,
        /// <summary>已完成倒带，停止记录但保留数据</summary>
        Completed
    }
}
