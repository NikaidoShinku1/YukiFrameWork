///=====================================================
/// - FileName:      ActionCondition.cs
/// - NameSpace:     YukiFrameWork.Project
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   这是一个框架工具创建的脚本
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
namespace YukiFrameWork.Events
{
    public interface IActionUpdateCondition
    {
        event Action<IActionUpdateCondition> EnqueueCondition;
        void InitUpdateType(MonoUpdateType type,object data);
        IActionUpdateCondition Where(Func<bool> condition);
        IActionUpdateCondition Delay(float time, Action callBack = null);
        IActionUpdateCondition TakeWhile(Func<bool> condition);
        IActionUpdateCondition First(Func<bool> condition = null);
        IActionUpdate Register(Action<object> callBack);

    }

    public enum MonoUpdateType
    {
        Update,
        FixedUpdate,
        LateUpdate
    }

    public class ActionUpdateCondition : IActionUpdateCondition
    {
        public event Action<IActionUpdateCondition> EnqueueCondition;

        private readonly IActionUpdate actionUpdate = new ActionUpdate();
        private MonoUpdateType updateType;
        
        private float currentTime = 0;
        private Action delayCallBack;

        /// <summary>
        /// 检测条件，于Update循环刷新
        /// </summary>
        private Func<bool> Condition;

        /// <summary>
        /// 检测条件，直到返回False终止Update
        /// </summary>
        private Func<bool> predicate;

        /// <summary>
        /// 检测条件，为True时使Update执行符合条件的操作一次并且终止之后的操作
        /// </summary>      
        private bool isFirst = false;

        /// <summary>
        /// 回调参数：可以为Update附带一个参数
        /// </summary>
        private object data = null;

        /// <summary>
        /// 构造函数初始化Update的类型
        /// </summary>
        /// <param name="updateType">update的类型</param>
        public ActionUpdateCondition(MonoUpdateType updateType,object data)
        {
            this.updateType = updateType;
            this.data = data;
            actionUpdate.UpdateConditionEnqueue += Enqueue;
        }

        /// <summary>
        /// 初始化Update的类型
        /// </summary>
        /// <param name="type">Update类型</param>
        public void InitUpdateType(MonoUpdateType type,object data)
        {
            this.updateType = type;
            this.data = data;
            actionUpdate.UpdateConditionEnqueue += Enqueue;
        }

        /// <summary>
        /// 延迟执行Update
        /// </summary>
        /// <param name="time">时间</param>
        /// <param name="callBack">特殊回调(如需要可使用)</param>       
        public IActionUpdateCondition Delay(float time, Action callBack = null)
        {
            currentTime = time;
            delayCallBack = callBack;
            return this;
        }

        private async UniTask ToDelay(float time)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(time));
            delayCallBack?.Invoke();
        }

        /// <summary>
        /// 发送事件
        /// </summary>
        /// <param name="callBack">事件</param>      
        public IActionUpdate Register(Action<object> events)
        {
            _ = ToRegister(events);
            return actionUpdate;
        }

        private async UniTaskVoid ToRegister(Action<object> callBack)
        {
            await ToDelay(currentTime);
            actionUpdate.Register(callBack);
            CheckUpdateType();

            //初始化         

        }

        /// <summary>
        /// 检查Update的类型
        /// </summary>
        private void CheckUpdateType()
        {
            switch (updateType)
            {
                case MonoUpdateType.Update:
                    {
                        actionUpdate.Update(Condition, predicate,isFirst,data);
                    }
                    break;
                case MonoUpdateType.FixedUpdate:
                    {
                        actionUpdate.FixedUpdate(Condition, predicate,isFirst,data);
                    }
                    break;
                case MonoUpdateType.LateUpdate:
                    {
                        actionUpdate.LateUpdate(Condition, predicate,isFirst,data);
                    }
                    break;             
            }
        }

        private void Enqueue()
        {
            EnqueueCondition?.Invoke(this);
        }

        /// <summary>
        /// 只在其返回True时执行，当condition返回False时终止本生命周期流
        /// </summary>
        /// <param name="condition">条件判断</param>   
        public IActionUpdateCondition TakeWhile(Func<bool> condition)
        {
            this.predicate = condition;
            return this;
        }

        /// <summary>
        /// 条件判断，当为True时执行回调
        /// </summary>
        /// <param name="condition">条件判断</param>       
        public IActionUpdateCondition Where(Func<bool> condition)
        {
            this.Condition = condition;
            return this;
        }

        /// <summary>
        /// 只在第一次条件判定符合时执行(可与Where搭配也可自行添加条件)
        /// </summary>
        /// <param name="condition">条件判定</param>    
        public IActionUpdateCondition First(Func<bool> condition = null)
        {
            if(Condition == null)
            this.Condition = condition;
            isFirst = true;
            return this;
        }       
    }
}