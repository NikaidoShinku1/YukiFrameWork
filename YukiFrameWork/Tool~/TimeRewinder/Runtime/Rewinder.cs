///=====================================================
/// - FileName:      Rewinder.cs
/// - NameSpace:     YukiFrameWork.Example
/// - Description:   高级定制脚本生成
/// - Creation Time: 2026/5/23 16:02:46
/// -  (C) Copyright 2008 - 2030
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using YukiFrameWork;

namespace YukiFrameWork.Rewinder
{
    /// <summary>
    /// 倒带器接口
    /// </summary>
    public interface IRewinder
    {
        float RecordTime { get; }
        RewinderMode Mode { get; }
        /// <summary>
        /// 倒带器初始化
        /// </summary>
        /// <param name="recordTime"></param>
        /// <param name="param"></param>
        void Initialize(float recordTime,params object[] param);
        /// <summary>
        /// 完成倒带：停止继续记录，但保留缓冲区中的历史数据以供回放
        /// </summary>
        void Complete();
        /// <summary>
        /// 倒带器回放
        /// </summary>
        /// <param name="seconds"></param>
        void PlayBack(float seconds);
        /// <summary>
        /// 释放倒带器：停止记录并清理资源，之后需重新 Initialize 才能再次使用
        /// </summary>
        void Release();
        /// <summary>
        /// 倒带器强制类型转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T As<T>() where T : IRewinder;
    }

    public abstract class Rewinder: IRewinder
    {
        public float RecordTime { get; private set; }
        public RewinderMode Mode { get; private set; } = RewinderMode.Idle;

        void IRewinder.Initialize(float recordTime,params object[] param)
        {
            if (recordTime < 0)
            {
                throw new Exception("记录时间必须大于0! RecordTime use > 0!");
            }
            this.RecordTime = recordTime;
            Mode = RewinderMode.Update;
            OnInit(recordTime,param);
            MonoHelper.FixedUpdate_RemoveListener(FixedUpdate);
            MonoHelper.FixedUpdate_AddListener(FixedUpdate);
        }

        /// <summary>
        /// 倒带器初始化
        /// </summary>
        /// <param name="recordTime"></param>
        /// <param name="param"></param>
        protected abstract void OnInit(float recordTime,params object[] param);

        /// <summary>
        /// 倒带器持续记录
        /// </summary>
        protected abstract void OnRecord();
        [Obsolete("过时的记录方法,请重写OnRecord方法进行使用!")]
        protected virtual void OnUpdateRewinder()
        {
        }
        
        [Obsolete("过时的回放方法,请重写OnPlayBack方法进行使用!")]
        protected virtual void OnBackFlow(float seconds)
        {
        }
        
        /// <summary>
        /// 倒带器回放
        /// </summary>
        /// <param name="seconds"></param>
        protected abstract void OnPlayBack(float seconds);

        /// <summary>
        /// 完成倒带时回调，此时已停止记录但缓冲区数据仍可用
        /// </summary>
        protected virtual void OnComplete()
        {
        }

        /// <summary>
        /// 释放倒带器时回调，用于清理缓冲区、解绑目标等
        /// </summary>
        protected virtual void OnRelease()
        {
        }

        void IRewinder.Complete()
        {
            if (Mode != RewinderMode.Update)
                return;

            Mode = RewinderMode.Completed;
            MonoHelper.FixedUpdate_RemoveListener(FixedUpdate);
            OnComplete();
        }

        void IRewinder.PlayBack(float seconds)
        {
            OnPlayBack(seconds);
        }

        void IRewinder.Release()
        {
            if (Mode == RewinderMode.Idle)
                return;

            MonoHelper.FixedUpdate_RemoveListener(FixedUpdate);
            Mode = RewinderMode.Idle;
            OnRelease();
        }

        public T As<T>() where T : IRewinder
        {
            IRewinder rewinder = this;
            return (T)rewinder;
        }

        private void FixedUpdate(MonoHelper helper)
        {
            if (Mode != RewinderMode.Update)
                return;

            OnRecord();
        }
    }
}
