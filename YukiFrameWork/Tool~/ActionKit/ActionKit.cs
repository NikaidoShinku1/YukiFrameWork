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
        /// <summary>
        /// 定时回调，延迟触发回调
        /// </summary>
        /// <param name="currentTime">延迟的时间(秒)</param>
        /// <param name="callBack">触发的回调</param>
        /// <param name="isRealTime">是否不受到Time.timeScale影响</param>
        /// <returns></returns>
        [MethodAPI("定时回调")]
        public static IActionNode Delay(float currentTime, Action callBack = null,bool isRealTime = false)
        {
            return YukiFrameWork.Delay.Get(currentTime, callBack,isRealTime);
        }

        /// <summary>
        /// 等待延迟的帧数后触发回调
        /// </summary>
        /// <param name="frameCount">需要延迟的帧数</param>
        /// <param name="callBack">触发的回调</param>
        /// <returns></returns>
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

        /// <summary>
        /// 计时器，与Delay相似，有持续检测回调
        /// </summary>
        /// <param name="maxTime">最大时间</param>
        /// <param name="CallTemp">计时器运行时持续触发</param>
        /// <param name="CallBack">计时器结束后触发</param>
        /// <param name="isConstranit">是否约束0-1</param>
        /// <param name="isRealTime">是否不受到Time.timeScale影响</param>
        /// <returns></returns>
        [MethodAPI("计时器启动")]
        public static IActionNode StartTimer(float maxTime, Action<float> CallTemp, Action CallBack = null, bool isConstranit = false,bool isRealTime = false)
        {
            return YukiFrameWork.Timer.Get(maxTime,CallTemp,CallBack,isConstranit,isRealTime);
        }
        /// <summary>
        /// 插值计算
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="duration"></param>
        /// <param name="onLerp"></param>
        /// <param name="onLerpFinish"></param>
        /// <param name="isRealTime"></param>
        /// <returns></returns>
        [MethodAPI("插值")]
        public static IActionNode Lerp(float a, float b, float duration, Action<float> onLerp, Action onLerpFinish = null, bool isRealTime = false)
        {
            return YukiFrameWork.Lerp.Get(a, b, duration, onLerp, onLerpFinish, isRealTime);
        }

        [MethodAPI("插值")]
        public static IActionNode Lerp01(float duration, Action<float> onLerp, Action onLerpFinish = null, bool isRealTime = false)
        {
            return YukiFrameWork.Lerp.Get(0, 1, duration, onLerp, onLerpFinish, isRealTime);
        }

        /// <summary>
        /// 事件等待判断：当predicate返回True时，结束该任务
        /// </summary>
        /// <param name="predicate">设置的条件事件</param>
        /// <param name="CallBack">触发的回调</param>
        /// <returns></returns>
        [MethodAPI("事件判断等待")]
        public static IActionNode ExecuteFrame(Func<bool> predicate, Action CallBack = null)
        {
            return YukiFrameWork.ExecuteFrame.Get(predicate, CallBack);
        }

        /// <summary>
        /// 将协程转换为以Update来运行的ActionNode
        /// </summary>
        /// <param name="enumerator">迭代器本体</param>
        /// <param name="callBack">协程结束后触发的回调</param>
        /// <returns></returns>
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

        [MethodAPI("OnDrawGizmosSelected")]
        public static IMonoActionNode<Action> OnDrawGizmosSelected(Action action)
        {
            return YukiFrameWork.MonoAction<Action>.Get(action, IMonoActionNode.Mono.OnDrawGizmosSelected);
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
