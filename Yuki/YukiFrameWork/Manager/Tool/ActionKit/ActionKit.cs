///=====================================================
/// - FileName:      ActionKit.cs
/// - NameSpace:     YukiFrameWork.Events
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   ActionKit:动作时序脚本套件
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================


using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
namespace YukiFrameWork.Events
{
    /// <summary>
    /// 动作时序脚本管理
    /// </summary>
    public class ActionKit : IDisposable
    {
        #region 队列列表合集
        private readonly static Queue<IActionNode> nodes = new Queue<IActionNode>();
        private readonly static Queue<IActionDelay> actionDelays = new Queue<IActionDelay>();
        private readonly static Queue<IActionSequenceCondition> actionSequenceConditions = new Queue<IActionSequenceCondition>();
        private readonly static Queue<IActionDelayFrame> actionDelayFrames = new Queue<IActionDelayFrame>();
        private readonly static Queue<IActionNextFrame> actionNextFrames = new Queue<IActionNextFrame>();
        private readonly static Queue<IActionExcuteFrame> actionExcuteFrames = new Queue<IActionExcuteFrame>();
        private readonly static Queue<IActionUpdateCondition> updateConditions = new Queue<IActionUpdateCondition>();
        private readonly static Queue<IActionRepeat> ActionRepeats = new Queue<IActionRepeat>();
        #endregion

        #region 动作异步时序管理
        /// <summary>
        /// 时序帧检测(直到时间到达)(如不需要持续检测请使用Delay)(注意：当使用ToUpdate持续检测时如没有设置次数限制OnFinish将永远不会调用！)
        /// </summary>
        /// <param name="maxTime">最大时间</param>
        /// <param name="callBack">每帧回调</param>
        /// <param name="isConstraint">是否约束
        /// True:百分比计量最大为1
        /// False:正常时序</param>
        public static IActionExcuteFrame StartTimer(float maxTime, Action<float> callBack,bool isConstraint = false,Action OnFinish = null)
        {
            var excuteItem = CheckExcuteFrame();
            if (excuteItem == null) excuteItem = new ActionExcuteFrame(maxTime, callBack, isConstraint, OnFinish);
            else excuteItem.InitExcuteTimer(maxTime, callBack, isConstraint, OnFinish);
            excuteItem.ExcuteFrameEnquene += Enqueue;
            return excuteItem;
          
        }

        /// <summary>
        /// 启动条件检测
        /// </summary>
        /// <param name="predicate">条件判断</param>
        /// <param name="OnFinish">结束时可触发事件</param>
        public static IActionExcuteFrame ExcuteFrame(Func<bool> predicate,Action OnFinish = null)
        {
            var excuteItem = CheckExcuteFrame();
            if (excuteItem == null) excuteItem = new ActionExcuteFrame(predicate,OnFinish);
            else excuteItem.InitExcutePredicate(predicate, OnFinish);
            excuteItem.ExcuteFrameEnquene += Enqueue;
            return excuteItem;
        }     
        
        /// <summary>
        /// 时序定时回调
        /// </summary>
        /// <param name="time">时间</param>
        /// <param name="callBack">回调</param>
        /// <returns>返回一个ActionNode</returns>
        public static IActionDelay Delay(float time, Action callBack = null)
        {            
            var delayItem = CheckDelays();
            if (delayItem == null) delayItem = new ActionDelay(time, callBack);
            else delayItem.InitDelay(time, callBack);
            delayItem.DelayEnqueue += Enqueue;
            return delayItem;
        }

        /// <summary>
        /// 等待一帧执行
        /// </summary>
        /// <param name="callBack"></param>
        /// <param name="type">等待的执行，默认为Update</param>
        /// <returns></returns>
        public static IActionNextFrame NextFrame(Action callBack = null,MonoUpdateType type = MonoUpdateType.Update)
        {
            var nextFrameItem = CheckNextFrame();
            if (nextFrameItem == null) nextFrameItem = new ActionNextFrame(callBack,type);
            else nextFrameItem.InitNextFrame(callBack,type);
            nextFrameItem.ActionNextEnquene += Enqueue;
            return nextFrameItem;
        }

        public static IActionDelayFrame DelayFrame(int delayFrameCount, Action callBack = null, MonoUpdateType type = MonoUpdateType.Update)
        {
            var delayFrameItem = CheckDelayFrame();
            if (delayFrameItem == null) delayFrameItem = new ActionDelayFrame(callBack, type, delayFrameCount);
            else delayFrameItem.InitDelayFrame(callBack, type, delayFrameCount);
            delayFrameItem.ActionNextEnquene += Enquence;
            return delayFrameItem;
        }

        /// <summary>
        /// 队列时序回调，注意：队列与队列之间是并行的，可以同时启动多个队列，一个队列内所有的回调都是连续的
        /// </summary>
        /// <returns>返回本体</returns>
        public static IActionSequenceCondition Sequence()
        {
            var sequence = CheckSequenceCondition();
            if (sequence == null) sequence = new ActionSequenceCondition();
            else sequence.InitSequenceCondition();
            sequence.EnquenceCondition += Enquence;
            return sequence;
        }

        /// <summary>
        /// 循环时间(事件)检测
        /// </summary>
        /// <param name="reapaintCount">循环次数，默认无限循环(值为-1)</param>
        /// <returns>返回本体</returns>
        public static IActionRepeat Repeat(int reapaintCount = -1)
        {
            var repaint = CheckRepaint();
            if (repaint == null) repaint = new ActionRepeat(reapaintCount);
            else repaint.InitRepaint(reapaintCount);
            repaint.EnquenceRepaint += Enquence;
            return repaint;          
        }

        /// <summary>
        /// 循环时间(事件)检测
        /// </summary>
        /// <param name="condition">循环判断，当为False时终止循环</param>
        /// <returns>返回本体</returns>
        public static IActionRepeat Repeat(Func<bool> condition)
        {
            var repaint = CheckRepaint();
            if (repaint == null) repaint = new ActionRepeat(condition);
            else repaint.InitRepaint(condition);
            repaint.EnquenceRepaint += Enquence;
            return repaint;
        }

        /// <summary>
        /// 循环时间(事件)检测
        /// </summary>
        /// <param name="condition">循环判断，当为True时终止循环</param>
        /// <returns>返回本体</returns>
        public static IActionRepeat Repeat(int repaintCount, Func<bool> condition)
        {
            var repaint = CheckRepaint();
            if (repaint == null) repaint = new ActionRepeat(repaintCount,condition);
            else repaint.InitRepaint(repaintCount, condition);
            repaint.EnquenceRepaint += Enquence;
            return repaint;
        }

        private static IActionDelay CheckDelays()
        {
            if (actionDelays.Count > 0)
            {
                return actionDelays.Dequeue();
            }
            return null;
        }

        private static IActionExcuteFrame CheckExcuteFrame()
        {
            if (actionExcuteFrames.Count > 0) return actionExcuteFrames.Dequeue();
            return null;
        }

        private static IActionDelayFrame CheckDelayFrame()
        {
            if(actionDelayFrames.Count > 0)return actionDelayFrames.Dequeue();
            return null;
        }

        private static IActionNextFrame CheckNextFrame()
        {
            if (actionNextFrames.Count > 0) return actionNextFrames.Dequeue();
            return null;
        }

        private static IActionSequenceCondition CheckSequenceCondition()
        {
            if (actionSequenceConditions.Count > 0) return actionSequenceConditions.Dequeue();
            return null;
        }

        private static IActionRepeat CheckRepaint()
        {
            if(ActionRepeats.Count > 0) return ActionRepeats.Dequeue();
            return null;
        }

        private static void Enqueue(IActionDelay actionDelay)
        {
            actionDelays.Enqueue(actionDelay);
        }

        private static void Enqueue(IActionExcuteFrame actionExcuteFrame)
        {
            actionExcuteFrames.Enqueue(actionExcuteFrame);
        }

        private static void Enqueue(IActionNextFrame actionNextFrame)
        {
            actionNextFrames.Enqueue(actionNextFrame);
        }

        private static void Enquence(IActionSequenceCondition actionSequenceCondition)
        {
            actionSequenceConditions.Enqueue(actionSequenceCondition);
        }

        private static void Enquence(IActionDelayFrame actionDelayFrame)
        {
            actionDelayFrames.Enqueue(actionDelayFrame);
        }

        private static void Enquence(IActionRepeat repaint)
        {
            ActionRepeats.Enqueue(repaint);
        }
        #endregion

        #region 仿Mono Update周期
     
        /// <summary>
        /// ActionKit统一API：Update节点
        /// </summary>
        /// <param name="data">可以传一个参数</param>        
        public static IActionUpdateCondition OnUpdate(object data = null)
        {
            var condition = CheckCondition();
            if (condition != null) condition.InitUpdateType(MonoUpdateType.Update,data);
            else condition = new ActionUpdateCondition(MonoUpdateType.Update,data);
            return condition;
        }

        /// <summary>
        /// ActionKit统一API: FixedUpdate节点
        /// </summary>     
        /// <param name="data">可以传一个参数</param> 
        public static IActionUpdateCondition OnFixedUpdate(object data = null)
        {
            var condition = CheckCondition();
            if (condition != null) condition.InitUpdateType(MonoUpdateType.FixedUpdate,data);
            else condition = new ActionUpdateCondition(MonoUpdateType.FixedUpdate,data);
            return condition;
        }


        /// <summary>
        /// ActionKit统一API: LateUpdate节点
        /// </summary>
        /// <param name="data">可以传一个参数</param> 
        public static IActionUpdateCondition OnLateUpdate(object data = null)
        {

            var condition = CheckCondition();
            if (condition != null) condition.InitUpdateType(MonoUpdateType.LateUpdate,data);
            else condition = new ActionUpdateCondition(MonoUpdateType.LateUpdate,data);
            condition.EnqueueCondition += Enqueue;
            return condition;
        }

        private static void Enqueue(IActionUpdateCondition updateCondition)
        {
            updateConditions.Enqueue(updateCondition);
        }

        private static IActionUpdateCondition CheckCondition()
        {
            if (updateConditions.Count > 0) return updateConditions.Dequeue();
            return null;
        }

        #endregion

        public void Dispose()
        {           
            nodes.Clear();
        }
    }
}