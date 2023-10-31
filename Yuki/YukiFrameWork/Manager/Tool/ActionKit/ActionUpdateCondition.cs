///=====================================================
/// - FileName:      ActionCondition.cs
/// - NameSpace:     YukiFrameWork.Project
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   这是Update控制脚本
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
        /// <summary>
        /// 条件判断，当为True时执行回调
        /// </summary>
        /// <param name="condition">条件判断</param>       
        IActionUpdateCondition Where(Func<bool> condition);

        /// <summary>
        /// 延迟执行Update
        /// </summary>
        /// <param name="time">时间</param>
        /// <param name="callBack">特殊回调(如需要可使用)</param>       
        IActionUpdateCondition Delay(float time, Action callBack = null);

        /// <summary>
        /// 只在其返回True时执行，当condition返回False时终止本生命周期流
        /// </summary>
        /// <param name="condition">条件判断</param>   
        IActionUpdateCondition TakeWhile(Func<bool> condition);

        /// <summary>
        /// 只在第一次条件判定符合时执行(可与Where搭配也可自行添加条件)
        /// </summary>
        /// <param name="condition">条件判定</param>    
        IActionUpdateCondition First(Func<bool> condition = null);

        /// <summary>
        /// 注册事件
        /// </summary>
        /// <param name="callBack">事件</param>
        /// <param name="OnFinish">Update终止后的回调，当没有设置终止时将永远不会调用</param>        
        IActionUpdate Register(Action<object> callBack,Action OnFinish = null);

        /// <summary>
        ///  注册事件
        /// </summary>
        /// <param name="callBack">事件</param>
        /// <param name="OnError">处理callBack异常，当callBack异常则进入OnError，并终止Update</param>
        /// <param name="OnFinish">Update终止后的回调，当没有设置终止时将永远不会调用</param>
        /// <returns></returns>
        IActionUpdate Register(Action<object> callBack, Action OnError,Action OnFinish = null);
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
        private bool isImposeCount = false;

        private Action OnFinish;
        private Action OnError;

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
            isImposeCount = false;
            actionUpdate.UpdateConditionEnqueue += Enqueue;
        }

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
    
        public IActionUpdate Register(Action<object> events)
        {
            this.OnError = null;
            this.OnFinish = null;
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
                        actionUpdate.Update(Condition, predicate,isImposeCount,data,OnError,OnFinish);
                    }
                    break;
                case MonoUpdateType.FixedUpdate:
                    {
                        actionUpdate.FixedUpdate(Condition, predicate,isImposeCount,data,OnError,OnFinish);
                    }
                    break;
                case MonoUpdateType.LateUpdate:
                    {
                        actionUpdate.LateUpdate(Condition, predicate,isImposeCount,data,OnError,OnFinish);
                    }
                    break;             
            }
        }

        private void Enqueue()
        {
            EnqueueCondition?.Invoke(this);
        }      
        public IActionUpdateCondition TakeWhile(Func<bool> condition)
        {
            this.predicate = condition;
            return this;
        }     
        public IActionUpdateCondition Where(Func<bool> condition)
        {
            this.Condition = condition;
            return this;
        }

      
        public IActionUpdateCondition First(Func<bool> condition = null)
        {
            if(Condition == null)
            this.Condition = condition;
            isImposeCount = true;
            return this;
        }

        public IActionUpdate Register(Action<object> callBack, Action OnFinish = null)
        {
            this.OnFinish = OnFinish;
            this.OnError = null;
             _ = ToRegister(callBack);
            return actionUpdate;
        }

        public IActionUpdate Register(Action<object> callBack, Action OnError, Action OnFinish = null)
        {
            this.OnFinish = OnFinish;
            this.OnError = OnError;
            _ = ToRegister(callBack);
            return actionUpdate;
        }
    }
}