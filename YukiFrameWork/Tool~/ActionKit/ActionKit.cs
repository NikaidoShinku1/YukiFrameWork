///=====================================================
/// - FileName:      ActionKit.cs
/// - NameSpace:     YukiFrameWork
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   ActionKit动作时序套件
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using System;
using System.Collections;
using YukiFrameWork.Extension;
namespace YukiFrameWork
{           
    [ClassAPI("动作时序管理套件")]
    public static class ActionKit
    {
        [MethodAPI("定时回调")]
        public static IActionNode Delay(float currentTime, Action callBack = null,bool isRealTime = false)
        {
            return YukiFrameWork.Delay.Get(currentTime, callBack,isRealTime);
        }

        [MethodAPI("定时帧回调")]
        public static IActionNode DelayFrame(int frameCount, Action callBack = null)
        {
            return YukiFrameWork.NextFrame.Get(frameCount, callBack);
        }

        [MethodAPI("等待一帧执行")]
        public static IActionNode NextFrame(Action callBack = null)
        {
            return YukiFrameWork.NextFrame.Get(1, callBack);
        }

        [MethodAPI("计时器启动")]
        public static IActionNode StartTimer(float maxTime, Action<float> CallTemp, Action CallBack = null, bool isConstranit = false,bool isRealTime = false)
        {
            return YukiFrameWork.Timer.Get(maxTime,CallTemp,CallBack,isConstranit,isRealTime);
        }

        [MethodAPI("事件判断等待")]
        public static IActionNode ExecuteFrame(Func<bool> predicate, Action CallBack = null)
        {
            return YukiFrameWork.ExecuteFrame.Get(predicate, CallBack);
        }

        [MethodAPI("协程转ActionNode")]
        public static IActionNode Coroutine(IEnumerator enumerator, Action callBack = null)
        {
            return enumerator.ToAction(callBack);
        } 

        [MethodAPI("循环执行,默认值-1，代表无限循环")]
        public static IRepeat Repeat(int count = -1)
        {
            return YukiFrameWork.Repeat.Get(count);
        }

        [MethodAPI("队列执行")]
        public static ISequence Sequence()
        {
            return YukiFrameWork.Sequence.Get();
        }

        [MethodAPI("并行执行")]
        public static IParallel Parallel()
        {
            return YukiFrameWork.Parallel.Get();
        }

        [MethodAPI("OnGUI")]
        public static IMonoActionNode<Action> OnGUI(Action action)
        {
            return YukiFrameWork.MonoAction<Action>.Get(action,IMonoActionNode.Mono.OnGUI);
        }
        [MethodAPI("OnDrawGizmos")]
        public static IMonoActionNode<Action> OnDrawGizmos(Action action)
        {
            return YukiFrameWork.MonoAction<Action>.Get(action, IMonoActionNode.Mono.OnDrawGizmos);
        }
        [MethodAPI("OnApplicationFocus")]
        public static IMonoActionNode<Action<bool>> OnApplicationFocus(Action<bool> action)
        {
            return YukiFrameWork.MonoAction<Action<bool>>.Get(action, IMonoActionNode.Mono.OnApplicationFocus);
        }
        [MethodAPI("OnApplicationPause")]
        public static IMonoActionNode<Action<bool>> OnApplicationPause(Action<bool> action)
        {
            return YukiFrameWork.MonoAction<Action<bool>>.Get(action, IMonoActionNode.Mono.OnApplicationPause);
        }
        [MethodAPI("OnApplicationQuit")]
        public static IMonoActionNode<Action> OnApplicationQuit(Action action)
        {
            return YukiFrameWork.MonoAction<Action>.Get(action, IMonoActionNode.Mono.OnApplicationQuit);
        }

        public static IMonoActionNode<Action> OnCanvasGroupChanged(Action action)
        {
            return YukiFrameWork.MonoAction<Action>.Get(action, IMonoActionNode.Mono.OnCanvasGroupChanged);
        }

        [MethodAPI("增强Update方法")]
        public static IActionUpdateNode OnUpdate()
        {
            return YukiFrameWork.ActionUpdateNode.Get(UpdateStatus.OnUpdate);
        }

        [MethodAPI("增强FixedUpdate方法")]
        public static IActionUpdateNode OnFixedUpdate()
        {
            return YukiFrameWork.ActionUpdateNode.Get(UpdateStatus.OnFixedUpdate);
        }

        [MethodAPI("增强LateUpdate方法")]
        public static IActionUpdateNode OnLateUpdate()
        {
            return YukiFrameWork.ActionUpdateNode.Get(UpdateStatus.OnLateUpdate);
        }      

    }
}
