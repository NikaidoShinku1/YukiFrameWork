///=====================================================
/// - FileName:      RandomSelector.cs
/// - NameSpace:     YukiFrameWork.Behaviours
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/11/16 18:21:52
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.Behaviours
{
    /// <summary>
    /// 随机选择器
    /// </summary>
    public class RandomSelector : Selector
    {
        protected sealed override void Next()
        {
            currentIndex = RandomKit.Range(0, childs.Count);
            if (CanExecute())
                currentChild = childs[currentIndex];
            currentChild.Start();
        }      
    }
}
