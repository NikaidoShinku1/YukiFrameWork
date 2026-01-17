///=====================================================
/// - FileName:      BehaviourTreeView.cs
/// - NameSpace:     YukiFrameWork.Behaviours
/// - Description:   高级定制脚本生成
/// - Creation Time: 2024/11/14 17:21:36
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
#if UNITY_EDITOR
using UnityEngine;
using System;
using UnityEngine.UIElements;
namespace YukiFrameWork.Missions
{
    public class MissionTreeView : TwoPaneSplitView
    {
        public new class UxmlFactory : UxmlFactory<MissionTreeView, UxmlTraits>
        {
            
        }

        public MissionTreeView() 
        {
            
        }

    }
}
#endif