///=====================================================
/// - FileName:      Lerp.cs
/// - NameSpace:     Survival
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2025/1/7 17:21:06
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections;
using YukiFrameWork.Pools;
namespace YukiFrameWork
{
    public class Lerp : ActionNode
    {
        private static readonly SimpleObjectPools<Lerp> mPool =

          new SimpleObjectPools<Lerp>(() => new Lerp(), x => x.actions.Clear(), 10);

        private float a;
        private float b;
        private float duration;
        private Action<float> onLerp;
        private Action onLerpFinish;
        private float mCurrentTime = 0;
        private bool isRealTime;

        public Lerp() { }
        public Lerp(float a, float b, float duration, Action<float> onLerp, Action onLerpFinish, bool isRealTime)
        {
            Reset(a, b, duration, onLerp, onLerpFinish, isRealTime);
        }

        public static Lerp Get(float a, float b, float duration, Action<float> onLerp, Action onLerpFinish, bool isRealTime)
        {
            Lerp lerp = mPool.Get();
            lerp.Reset(a, b, duration, onLerp, onLerpFinish, isRealTime);
            return lerp;
        }

        public override bool OnExecute(float delta)
        {
            if (IsPaused) return false;
            mCurrentTime += isRealTime ? Time.unscaledDeltaTime : Time.deltaTime;
            if (mCurrentTime < duration)
            {
                onLerp?.Invoke(Mathf.Lerp(a, b, mCurrentTime / duration));
                return false;
            }
            onLerp?.Invoke(Mathf.Lerp(a, b, 1.0f));
            onLerpFinish?.Invoke();
            return true;
        }

        public void Reset(float a,float b,float duration,Action<float> onLerp,Action onLerpFinish = null,bool isRealTime = false)
        {
            mCurrentTime = 0;
            this.a = a;
            this.b = b;
            this.duration = duration;
            this.onLerp = onLerp;
            this.onLerpFinish = onLerpFinish;
            this.isRealTime = isRealTime;
            AddNode(this);
        }

        public override void OnFinish()
        {
            IsInit = false;
            a = 0;
            b = 0;
            duration = 0;
            mCurrentTime = 0;
            onLerp = null;
            onLerpFinish = null;
            isRealTime = false;
            IsCompleted = true;
            mPool.Release(this);
        }

        public override void OnInit()
        {
            mCurrentTime = 0;
            onLerp?.Invoke(Mathf.Lerp(a, b, 0));
            IsInit = true;
            IsCompleted = false;
        }

        public override IEnumerator ToCoroutine()
        {
            return CoroutineTool.WaitUntil(() => OnExecute(isRealTime ? Time.unscaledDeltaTime : Time.deltaTime));
        }
    }

    public static class LerpExtension
    {
        public static ISequence Lerp(this ISequence self, float a, float b, float duration, Action<float> onLerp = null, Action onLerpFinish = null,bool isRealTime = false)
        {
            return self.AddSequence(YukiFrameWork.Lerp.Get(a, b, duration, onLerp, onLerpFinish,isRealTime));
        }

        public static ISequence Lerp01(this ISequence self, float duration, Action<float> onLerp = null, Action onLerpFinish = null, bool isRealTime = false)
        {
            return self.AddSequence(YukiFrameWork.Lerp.Get(0, 1, duration, onLerp, onLerpFinish, isRealTime));
        }

        public static IParallel Lerp(this IParallel self, float a, float b, float duration, Action<float> onLerp = null, Action onLerpFinish = null, bool isRealTime = false)
        {
            return self.AddParallel(YukiFrameWork.Lerp.Get(a, b, duration, onLerp, onLerpFinish, isRealTime));
        }

        public static IParallel Lerp01(this IParallel self, float duration, Action<float> onLerp = null, Action onLerpFinish = null, bool isRealTime = false)
        {
            return self.AddParallel(YukiFrameWork.Lerp.Get(0, 1, duration, onLerp, onLerpFinish, isRealTime));
        }

        public static IRepeat Lerp(this IRepeat self, float a, float b, float duration, Action<float> onLerp = null, Action onLerpFinish = null, bool isRealTime = false)
        {
            self.ActionNode = ActionKit.Lerp(a, b, duration, onLerp, onLerpFinish, isRealTime);
            return self;
        }

        public static IRepeat Lerp01(this IRepeat self, float duration, Action<float> onLerp = null, Action onLerpFinish = null, bool isRealTime = false)
        {
            self.ActionNode = ActionKit.Lerp01( duration, onLerp, onLerpFinish, isRealTime);
            return self;
        }

    }
}
