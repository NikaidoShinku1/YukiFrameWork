///=====================================================
/// - FileName:      PanelInfo.cs
/// - NameSpace:     YukiFrameWork.UI
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/23 3:31:42
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using YukiFrameWork.Pools;
namespace YukiFrameWork.UI
{
	public struct PanelInfo
    {      
        public string panelName;
        public UILevel level;       
        public IPanel panel;    
        public Type panelType;
        public object[] param;
        public bool levelClear;
    }
}
