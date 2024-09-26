///=====================================================
/// - FileName:      StateMachineSetting.cs
/// - NameSpace:     YukiFrameWork.Farm
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/8/8 19:20:59
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.ActionStates
{
    public static class StateMachineSetting
    {
        public static PluginLanguage Language { get; set; } = PluginLanguage.Chinese;

        public static string ActionNext => Language == PluginLanguage.Chinese ? "Action ->" : "Action ->";
        public static string SelectArchitecture => Language == PluginLanguage.Chinese ? "架构选择" : "Select Architecture";
        public static string StateMechineController => Language == PluginLanguage.Chinese ? "状态机控制器" : "State Machine Controller";
        public static string OpenStateMachineEditor => Language == PluginLanguage.Chinese ? "打开状态机编辑器" : "Open the state machine editor";
        public static string SelectLanguageEditor => Language == PluginLanguage.Chinese ? "选择状态机编辑器语言" : "Select language by machine editor";
        public static string StateAttribute => Language == PluginLanguage.Chinese ? "状态属性" : "State attribute";
        public static string StatusName => Language == PluginLanguage.Chinese ? "状态名称" : "Status name";
        public static string StatusIdentifier => Language == PluginLanguage.Chinese ? "状态标识" : "Status identifier";
        public static string ActionSystem => Language == PluginLanguage.Chinese ? "动作系统" : "Action system";
        public static string AnimationMode => Language == PluginLanguage.Chinese ? "动画模式" : "Animation mode";
        public static string OldAnimation => Language == PluginLanguage.Chinese ? "旧版动画" : "Old animation";
        public static string NewAnimation => Language == PluginLanguage.Chinese ? "新版动画" : "New animation";
        public static string DirectorAnimation => Language == PluginLanguage.Chinese ? "导演动画" : "Director animation";
        public static string ActionExecutionMode => Language == PluginLanguage.Chinese ? "动作执行模式" : "Action execution mode";
        public static string ActionRandomised => Language == PluginLanguage.Chinese ? "动作随机" : "Action randomised";
        public static string ActionSequence => Language == PluginLanguage.Chinese ? "动作顺序" : "Action sequence";
        public static string ActionNone => Language == PluginLanguage.Chinese ? "动作控制" : "Action none";
        public static string AnimationSpeed => Language == PluginLanguage.Chinese ? "动画速度" : "Animation speed";
        public static string AnimationCycle => Language == PluginLanguage.Chinese ? "动画循环？" : "Animation cycle?";
        public static string IsCrossFade => Language == PluginLanguage.Chinese ? "动画过渡" : "IsCrossFade";
        public static string Duration => Language == PluginLanguage.Chinese ? "过渡时间" : "Duration";
        public static string ExitStatusAtEndOfAction => Language == PluginLanguage.Chinese ? "动作结束退出状态" : "Exit status at end of action";
        public static string GetIntoTheState => Language == PluginLanguage.Chinese ? "进入状态" : "Get into the state";
        public static string ActionTree => Language == PluginLanguage.Chinese ? "动作树" : "Action tree";
        public static string ActionTreeSet => Language == PluginLanguage.Chinese ? "动作树集合" : "Action tree set";
        public static string AddAction => Language == PluginLanguage.Chinese ? "添加动作" : "Add action";
        public static string RemoveAction => Language == PluginLanguage.Chinese ? "移除动作" : "Remove action";
        public static string CopyAction => Language == PluginLanguage.Chinese ? "复制动作" : "Copy action";
        public static string PasteNewAction => Language == PluginLanguage.Chinese ? "黏贴新的动作" : "Paste new action";
        public static string PasteActionValue => Language == PluginLanguage.Chinese ? "黏贴动作值" : "Paste action value";
        public static string MovieClips => Language == PluginLanguage.Chinese ? "动画剪辑" : "Movie clips";
        public static string PlaySoundEffects => Language == PluginLanguage.Chinese ? "播放音效" : "Play sound effects";
        public static string SoundPlaybackMode => Language == PluginLanguage.Chinese ? "音效播放模式" : "Sound playback mode";
        public static string PlayIn => Language == PluginLanguage.Chinese ? "进入播放" : "Play in";
        public static string EventPlayback => Language == PluginLanguage.Chinese ? "事件播放" : "Event playback";
        public static string QuitPlayback => Language == PluginLanguage.Chinese ? "退出播放" : "Quit playback";
        public static string SoundObject => Language == PluginLanguage.Chinese ? "音效对象" : "Sound object";
        public static string AnimationTime => Language == PluginLanguage.Chinese ? "动画时间" : "Animation time";
        public static string BlendParameter => Language == PluginLanguage.Chinese ? "混合树参数" : "Blend Parameter";
        public static string BlendParameterY => Language == PluginLanguage.Chinese ? "混合树参数Y" : "Blend ParameterY";
        public static string AnimationLength => Language == PluginLanguage.Chinese ? "动画长度" : "Animation length";
        public static string AnimationLayer => Language == PluginLanguage.Chinese ? "动画层级" : "Animation layer";
        public static string BlendTreeInfo => Language == PluginLanguage.Chinese ? "设置的动作为混合树，将显示混合树参数设置 混合树类型:" : "Set the action to blend tree, will display the blend tree parameter set blend tree type:";
        public static string PlayableAsset => Language == PluginLanguage.Chinese ? "可播放资源" : "Playable asset";
        public static string MeshAnimated => Language == PluginLanguage.Chinese ? "GPU动画" : "Mesh animated";
        public static string SkillObject => Language == PluginLanguage.Chinese ? "技能对象" : "Skill object";
        public static string SkillGeneration => Language == PluginLanguage.Chinese ? "技能生成" : "Skill generation";
        public static string Instantiation => Language == PluginLanguage.Chinese ? "实例化" : "Instantiation";
        public static string ObjectPool => Language == PluginLanguage.Chinese ? "对象池" : "Object pool";
        public static string ObjectPoolManagement => Language == PluginLanguage.Chinese ? "对象池管理" : "Object pool management";
        public static string SkillsSettingMode => Language == PluginLanguage.Chinese ? "技能设置模式" : "Skills setting mode";
        public static string SelfLocation => Language == PluginLanguage.Chinese ? "自身位置" : "Self location";
        public static string MountParentObject => Language == PluginLanguage.Chinese ? "挂载父对象" : "Mount parent object";
        public static string ParentObjectLocation => Language == PluginLanguage.Chinese ? "父对象位置" : "Parent object location";
        public static string ParentObject => Language == PluginLanguage.Chinese ? "父对象" : "Parent object";
        public static string SkillPosition => Language == PluginLanguage.Chinese ? "技能位置" : "Skill position";
        public static string SkillSurvivalTime => Language == PluginLanguage.Chinese ? "技能存活时间" : "Skill survival time";
        public static string RemoveActionScripts => Language == PluginLanguage.Chinese ? "移除动作脚本" : "Remove action scripts";
        public static string CopyActionScripts => Language == PluginLanguage.Chinese ? "复制动作脚本" : "Copy action scripts";
        public static string PasteNewActionScripts => Language == PluginLanguage.Chinese ? "黏贴新的动作脚本" : "Paste new action scripts";
        public static string PasteActionScriptValues => Language == PluginLanguage.Chinese ? "黏贴动作脚本值" : "Paste action script values";
        public static string AddActionScripts => Language == PluginLanguage.Chinese ? "添加动作脚本" : "Add action scripts";
        public static string CreateActionScriptPaths => Language == PluginLanguage.Chinese ? "创建动作脚本路径：" : "Create action script paths:";
        public static string CreateActionScripts => Language == PluginLanguage.Chinese ? "创建动作脚本" : "Create action scripts";
        public static string Cancel => Language == PluginLanguage.Chinese ? "取消" : "Cancel";
        public static string RemoveStatusScripts => Language == PluginLanguage.Chinese ? "移除状态脚本" : "Remove status scripts";
        public static string CopyStatusScript => Language == PluginLanguage.Chinese ? "复制状态脚本" : "Copy status script";
        public static string PasteNewStatusScript => Language == PluginLanguage.Chinese ? "黏贴新的状态脚本" : "Paste new status script";
        public static string PasteStatusScriptValues => Language == PluginLanguage.Chinese ? "黏贴状态脚本值" : "Paste status script values";
        public static string AddingStatusScripts => Language == PluginLanguage.Chinese ? "添加状态脚本" : "Adding status scripts";
        public static string CreateStatusScriptPath => Language == PluginLanguage.Chinese ? "创建状态脚本路径：" : "Create a status script path:";
        public static string CreateStatusScripts => Language == PluginLanguage.Chinese ? "创建状态脚本" : "Create status scripts";
        public static string ConnectionProperties => Language == PluginLanguage.Chinese ? "连接属性" : "Connection properties";
        public static string ConnectionMode => Language == PluginLanguage.Chinese ? "连接模式" : "Connection mode";
        public static string CurrentTime => Language == PluginLanguage.Chinese ? "当前时间" : "Current time";
        public static string EndTime => Language == PluginLanguage.Chinese ? "结束时间" : "End time";
        public static string CurrentTimeAutoEnterNextState => Language == PluginLanguage.Chinese ? "当前时间到底结束时间就会自动进入下一个状态" : "The current time will automatically enter the next state at the end of the time.";
        public static string EnterNextState => Language == PluginLanguage.Chinese ? "进入下一个状态" : "Enter the next state";
        public static string RemoveConnectionScripts => Language == PluginLanguage.Chinese ? "移除连接脚本" : "Remove connection scripts";
        public static string CopyConnectionScripts => Language == PluginLanguage.Chinese ? "复制连接脚本" : "Copy connection scripts";
        public static string PasteNewConnectionScript => Language == PluginLanguage.Chinese ? "黏贴新的连接脚本" : "Paste a new connection script";
        public static string PasteConnectionScriptValues => Language == PluginLanguage.Chinese ? "黏贴连接脚本值" : "Paste connection script values";
        public static string AddConnectionScripts => Language == PluginLanguage.Chinese ? "添加连接脚本" : "Add connection scripts";
        public static string CreateConnectionScriptPath => Language == PluginLanguage.Chinese ? "创建连接脚本路径：" : "Create a connection script path:";
        public static string CreateConnectionScripts => Language == PluginLanguage.Chinese ? "创建连接脚本" : "Create connection scripts";
        public static string ConnectionBehaviorScriptInfo => Language == PluginLanguage.Chinese ? "可以创建连接行为脚本控制状态的进入下一个状态" : "You can create a connection behavior script to control the state to the next state";
        public static string EditActionScripts => Language == PluginLanguage.Chinese ? "编辑动作脚本" : "Edit action scripts";
        public static string EditStatusScript => Language == PluginLanguage.Chinese ? "编辑状态脚本" : "Edit status script";
        public static string EditConnectionScript => Language == PluginLanguage.Chinese ? "编辑连接脚本" : "Edit connection script";
        public static string SelectObjectMode => Language == PluginLanguage.Chinese ? "选择状态模式" : "Select object mode";
        public static string StateHideFlags => Language == PluginLanguage.Chinese ? "状态隐藏标志" : "State hide flags";      
        public static string EditorWindow => Language == PluginLanguage.Chinese ? "游戏设计师编辑器窗口" : "YukiFrameWork editor window";
        public static string Reset => Language == PluginLanguage.Chinese ? "复位" : "Reset";
        public static string CreateStateMachine => Language == PluginLanguage.Chinese ? "创建状态机" : "Create a state machine";
        public static string Tips => Language == PluginLanguage.Chinese ? "提示!" : "Tips!";
        public static string PleaseSelectObjectToCreateStateMachine => Language == PluginLanguage.Chinese ? "请选择物体后在点击创建状态机！" : "Please select the object and click to create the state machine!";
        public static string Yes => Language == PluginLanguage.Chinese ? "是" : "Yes";
        public static string No => Language == PluginLanguage.Chinese ? "否" : "No";
        public static string CreateTransition => Language == PluginLanguage.Chinese ? "创建过渡" : "Create transition";
        public static string DefaultState => Language == PluginLanguage.Chinese ? "默认状态" : "Default state";
        public static string ReplicationState => Language == PluginLanguage.Chinese ? "复制状态" : "Replication state";
        public static string DeletedState => Language == PluginLanguage.Chinese ? "删除状态" : "Deleted state";
        public static string CreateState => Language == PluginLanguage.Chinese ? "创建状态" : "Create state";
        public static string NewState => Language == PluginLanguage.Chinese ? "新的状态" : "New state";
        public static string PasteSelectionStatus => Language == PluginLanguage.Chinese ? "粘贴选择状态" : "Paste selection status";
        public static string DeleteSelectionState => Language == PluginLanguage.Chinese ? "删除选择状态" : "Delete selection state";
        public static string CreateAndReplaceStateMachines => Language == PluginLanguage.Chinese ? "创建并替换状态机" : "Create and replace state machines";
        public static string NewStateMachine => Language == PluginLanguage.Chinese ? "新的状态机" : "New state machine";
        public static string DeleteStateMachine => Language == PluginLanguage.Chinese ? "删除状态机" : "Delete state machine";
        public static string DeleteStateManager => Language == PluginLanguage.Chinese ? "删除状态管理器" : "Delete state manager";
        public static string StateMachineHiddenFlag => Language == PluginLanguage.Chinese ? "状态机隐藏标志" : "State machine hidden flag";
        public static string EffectEulerAngles => Language == PluginLanguage.Chinese ? "技能角度" : "Effect euler angles";
        public static string InitMode => Language == PluginLanguage.Chinese ? "初始化模式" : "Init mode";
        public static string Active => Language == PluginLanguage.Chinese ? "激活模式" : "Active";
        public static string None => Language == PluginLanguage.Chinese ? "空模式" : "None";
    }

}
