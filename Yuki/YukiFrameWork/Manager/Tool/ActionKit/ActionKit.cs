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
using System.Data;
using System.Threading.Tasks;

namespace YukiFrameWork
{
    public static class ActionKit
    {
        public static IActionNode Delay(float currentTime, Action callBack = null)
        {
            return YukiFrameWork.Delay.Get(currentTime, callBack);
        }

        public static IActionNode DelayFrame(int frameCount, Action callBack = null)
        {
            return YukiFrameWork.NextFrame.Get(frameCount, callBack);
        }

        public static IActionNode NextFrame(Action callBack = null)
        {
            return YukiFrameWork.NextFrame.Get(1, callBack);
        }

        public static IActionNode StartTimer(float maxTime, Action<float> CallTemp, Action CallBack = null, bool isConstranit = false)
        {
            return YukiFrameWork.Timer.Get(maxTime,CallTemp,CallBack,isConstranit);
        }

        public static IActionNode ExecuteFrame(Func<bool> predicate, Action CallBack = null)
        {
            return YukiFrameWork.ExecuteFrame.Get(predicate, CallBack);
        }

        public static IRepeat Repeat(int count)
        {
            return YukiFrameWork.Repeat.Get(count);
        }

        public static ISequence Sequence()
        {
            return YukiFrameWork.Sequence.Get();
        }

        public static IParallel Parallel()
        {
            return YukiFrameWork.Parallel.Get();
        }

        public static IActionUpdateNode OnUpdate()
        {
            return YukiFrameWork.ActionUpdateNode.Get(UpdateStatus.OnUpdate);
        }

        public static IActionUpdateNode OnFixedUpdate()
        {
            return YukiFrameWork.ActionUpdateNode.Get(UpdateStatus.OnFixedUpdate);
        }

        public static IActionUpdateNode OnLateUpdate()
        {
            return YukiFrameWork.ActionUpdateNode.Get(UpdateStatus.OnLateUpdate);
        }


    }
}
