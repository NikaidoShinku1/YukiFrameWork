///=====================================================
/// - FileName:      StateMachineConst.cs
/// - NameSpace:     YukiFrameWork.StateMachine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/7 14:23:07
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.Machine
{
    public class StateMachineConst 
    {
        public const string anyState = "Any State";

        public const string entryState = "Entry";
        public const string up = "up"; // 上一层 
        public const int StateNodeWith = 200;
        public const int StateNodeHeight = 40;

        public const string BaseLayer = "Base Layer";

        public const string initializeDisplay = "状态机初始化时机";
        public const string initializeEventDisplay = "当状态机初始化完成后执行的回调";
        public const string resetDisableDisplay = "(是否在对象失活(Disable)重置状态机)";
        public const string onChangeStateEventDisplay = "当状态机切换某一个状态时触发的回调,会传递所属状态机集合与其实际执行切换的状态机与切换前后的状态名称参数";

        public const string StateMachineKeyDisplay = "状态机指定标识";

        public const string StateMachineValueDisplay = "状态机配置";

        public const string RuntimeStateMachineDisplay = "运行时状态机配置添加";

        public const string RuntimeStateMachineWarning = "为每个不同的状态机指定唯一的访问标识，当状态机只有一个配置时，可忽略标识设置,在运行时会自动赋值标识为Default Machine";
    }
}
