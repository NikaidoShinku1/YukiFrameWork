///=====================================================
/// - FileName:      BuffHandler.cs
/// - NameSpace:     YukiFrameWork.Buffer
/// - Description:   Buff控制中枢类
/// - Creation Time: 2024/5/8 15:48:06
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using System;
using Sirenix.OdinInspector;
namespace YukiFrameWork.Buffer
{
    [DisableViewWarning]
    public class BuffHandler : MonoBehaviour
    {
        private BuffControllerTable mTable = new BuffControllerTable();
        private List<BuffController> release = new List<BuffController>();

        /// <summary>
        /// Buff添加时触发的回调,该回调与BuffAwake同属周期，仅首次添加调用，如果是添加多个且不可叠加的buff，则每一个新Buff都触发
        /// </summary>
        [LabelText("Buff添加时会触发的回调")]
        public UnityEngine.Events.UnityEvent<BuffController> onBuffAddCallBack = new UnityEngine.Events.UnityEvent<BuffController>();


        /// <summary>
        /// Buff移除时触发的回调，并且可以拿到Controller
        /// </summary>
        [LabelText("Buff移除时会触发的回调")]
        public UnityEngine.Events.UnityEvent<BuffController> onBuffDestroyCallBack = new UnityEngine.Events.UnityEvent<BuffController>();

        public IDictionary<string, List<BuffController>> AllRuntimeBuffers => mTable.Table;
        [Obsolete("不再需要手写控制器泛型，现在使用Handler.AddBuffer即可不需要传递泛型了，通过在BuffKit绑定控制器或者给Buff标记自动化特性BindBuffController!")]
        public void AddBuffer<T>(IBuff buff, IBuffExecutor player) where T : BuffController, new()
        {
            AddBuffer(buff, player);
        }

        public BuffController AddBuffer(IBuff buff, IBuffExecutor player)
        {
            if (mTable.TryGetValue(buff.GetBuffKey, out var controllers))
            {
                var item = controllers.FirstOrDefault();
                if (buff.AdditionType != BuffRepeatAdditionType.None
                    && CheckBuffCounteractID(buff)
                    && CheckBuffDisableID(buff)
                    && item != null
                    && item.OnAddBuffCondition()
                    && player.OnAddBuffCondition()
                    )
                {
                    switch (buff.AdditionType)
                    {
                        case BuffRepeatAdditionType.Reset:
                            {
                                item.RemainingTime = buff.BuffTimer;
                                return item;
                            }
                            
                        case BuffRepeatAdditionType.Multiple:
                            {
                                if (buff.IsMaxStackableLayer && item.BuffLayer >= buff.MaxStackableLayer && buff.MaxStackableLayer > 0)
                                    return item;
                                item.BuffLayer++;
                                item.OnBuffStart();
                                item.onBuffStart?.Invoke(item.BuffLayer);
                                return item;
                            }                            
                        case BuffRepeatAdditionType.MultipleAndReset:
                            {
                                if (buff.IsMaxStackableLayer && item.BuffLayer >= buff.MaxStackableLayer && buff.MaxStackableLayer > 0)
                                    return item;
                                item.BuffLayer++;
                                item.RemainingTime = buff.BuffTimer;
                                item.OnBuffStart();
                                item.onBuffStart?.Invoke(item.BuffLayer);
                                return item;
                            }
                           
                        case BuffRepeatAdditionType.MultipleCount:
                            {
                                BuffController controller = CreateInstance(buff, player);
                                InitController(controller);
                                controllers.Add(controller);
                                controller.OnBuffStart();
                                controller.onBuffStart?.Invoke(controller.BuffLayer);
                                return controller;
                            }                            
                    }
                }
                return item;
            }
            else
            {
                BuffController controller = CreateInstance(buff, player);

                InitController(controller);

                //如果检查没有会被抵消的Buff以及该Buff没有处于别的运行时Buff内的禁止Buff，则正常添加，否则不添加
                if (CheckBuffCounteractID(buff) && CheckBuffDisableID(buff) && controller.OnAddBuffCondition() && player.OnAddBuffCondition())
                {
                    AddBuffer(controller);
                    return controller;
                }
                else
                {
                    release.Add(controller);
                    return null;//如果是要被回收的控制器，则返回空 
                }

            }
        }

        private BuffController CreateInstance(IBuff buff, IBuffExecutor player)
        {
            var controller = BuffKit.CreateBuffController(buff.GetBuffKey);
            controller.Buffer = buff;
            controller.Player = player;
            controller.BuffLayer = 0;
            controller.fixedTimer = 0;
            return controller;
        }
        public BuffController AddBuffer(string BuffKey, IBuffExecutor player)
        {
            IBuff buff = BuffKit.GetBuffByKey(BuffKey);
            return AddBuffer(buff, player);
        }

        /// <summary>
        /// 根据ID删除对应的Buff
        /// </summary>
        /// <param name="BuffKey"></param>
        /// <param name="slowly"></param>
        /// <returns></returns>
        public bool RemoveBuffer(string BuffKey)
        {
            if (mTable.TryGetValue(BuffKey, out var controllers))
            {
                var item = controllers.FirstOrDefault();
                if (item == null) return false;

                OnControllerRemove(item);

                //防止重复删除，提前移除回收列表内存在的该控制器
                release.Remove(item);
                return true;
            }

            return false;
        }

        public bool RemoveBuffer(IBuff buff)
            => RemoveBuffer(buff.GetBuffKey);

        /// <summary>
        /// 得到当前正在运行的指定标识的Buff数量
        /// </summary>
        /// <returns></returns>
        public int GetBufferCount(string BuffKey)
        {
            mTable.TryGetValue(BuffKey, out var list);
            return list == null ? 0 : list.Count;
        }

        /// <summary>
        /// 得到当前运行的所有Buff的数量(以Buff类型为基准的数量)
        /// </summary>
        /// <returns></returns>
        public int GetAllBufferCount()
        {
            return mTable.Count;
        }

        /// <summary>
        /// 判断指定的Buff是否有在这个对象上执行
        /// </summary>
        /// <param name="BuffKey"></param>
        /// <returns></returns>
        public bool IsBufferRunning(string BuffKey)
        {
            return GetBufferCount(BuffKey) > 0;
        }

        /// <summary>
        /// 获取指定Buff的控制器，只返回第一个BuffController，如果是可以同时存在不可叠加的Buff，想获取所有的控制器请调用GetRuntimeBuffers方法。
        /// </summary>
        /// <param name="BuffKey"></param>
        /// <returns></returns>
        public BuffController GetRuntimeBuffer(string BuffKey)
        {
            if (!mTable.TryGetValue(BuffKey, out var list))
            {
                LogKit.W("Buff" + BuffKey + "不存在");
                return null;
            }
            return list.FirstOrDefault();
        }

        /// <summary>
        /// 获取指定Buff的所有控制器
        /// </summary>
        /// <param name="BuffKey"></param>
        /// <returns></returns>
        public BuffController[] GetRuntimeBuffers(string BuffKey)
        {
            if (!mTable.TryGetValue(BuffKey, out var list))
            {
                LogKit.W("Buff" + BuffKey + "不存在");
                return null;
            }
            return list.ToArray();
        }

        private void InitController(BuffController controller)
        {
            onBuffAddCallBack?.Invoke(controller);
            controller.OnBuffAwake();
            controller.BuffLayer = 1;
            if (controller.Buffer.SurvivalType == BuffSurvivalType.Timer)
            {
                controller.RemainingTime = controller.Buffer.BuffTimer;
            }
        }

        internal bool CheckBuffCounteractID(IBuff buff)
        {
            bool isAdd = true;
            foreach (var conid in buff.BuffCounteractID)
            {
                if (mTable.TryGetValue(conid, out var cores))
                {
                    isAdd = false;
                    foreach (var item in cores)
                    {
                        release.Add(item);
                    }
                }
            }
            return isAdd;
        }

        internal bool CheckBuffDisableID(IBuff buff)
        {
            string BuffKey = buff.GetBuffKey;
            foreach (var item in mTable.Values)
            {
                for (int i = 0; i < item.Count; i++)
                {
                    string[] disableID = item[i].Buffer.BuffDisableID;

                    if (disableID == null || disableID.Length == 0) continue;

                    for (int j = 0; j < disableID.Length; j++)
                    {
                        if (BuffKey == disableID[j])
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        internal void AddBuffer(BuffController buffController)
        {
            mTable.Add(buffController.BuffKey, buffController);
            buffController.OnBuffStart();
            buffController.onBuffStart?.Invoke(buffController.BuffLayer);
        }

        private void Update()
        {
            UpdateAllBuffController(UpdateStatus.OnUpdate);
        }

        private void FixedUpdate()
        {
            UpdateAllBuffController(UpdateStatus.OnFixedUpdate);
        }

        private void LateUpdate()
        {
            UpdateAllBuffController(UpdateStatus.OnLateUpdate);

            if (release.Count == 0) return;

            for (int i = 0; i < release.Count; i++)
            {
                OnControllerRemove(release[i]);
            }

            release.Clear();
        }

        private void UpdateAllBuffController(UpdateStatus updateStatus)
        {
            foreach (var item in mTable.Values)
            {
                for (int i = 0; i < item.Count; i++)
                {
                    if (updateStatus == UpdateStatus.OnUpdate)
                        UpdateSetting(item[i]);
                    else if (updateStatus == UpdateStatus.OnFixedUpdate)
                        FixedUpdateSetting(item[i]);
                    else if (updateStatus == UpdateStatus.OnLateUpdate && !release.Contains(item[i]))
                        LateUpdateSetting(item[i]);
                }
            }
        }

        private void UpdateSetting(BuffController controller)
        {
            controller.OnBuffUpdate();
            controller.onBuffReleasing?.Invoke(controller.RemainingProgress);
            controller.RemainingTime -= Time.deltaTime;
            if ((controller.OnRemoveBuffCondition()) || (controller.RemainingTime <= 0 && controller.Buffer.SurvivalType == BuffSurvivalType.Timer))
            {
                release.Add(controller);
            }
        }

        private void OnControllerRemove<T>(T controller) where T : BuffController
        {
            if (controller.Buffer.IsBuffRemoveBySlowly)
                controller.BuffLayer--;
            if (controller.Buffer.SurvivalType == BuffSurvivalType.Timer)
            {
                controller.RemainingTime = controller.Buffer.BuffTimer;
            }
            controller.OnBuffRemove();
            controller.onBuffRemove?.Invoke(controller.Buffer.IsBuffRemoveBySlowly ? controller.BuffLayer : 0);
            if (controller.BuffLayer <= 0 || !controller.Buffer.IsBuffRemoveBySlowly)
            {
                onBuffDestroyCallBack?.Invoke(controller);
                controller.OnBuffDestroy();
                mTable.Remove(controller.BuffKey, controller);
                if (mTable.IsNullOrEmpty(controller.BuffKey))
                    mTable.Remove(controller.BuffKey);
#if YukiFrameWork_DEBUGFULL
                LogKit.I("回收的控制器类型:" + controller.GetType());
#endif
                BuffController.Release(controller);
            }
        }

        private void FixedUpdateSetting(BuffController controller)
        {
            //防止因为卡顿导致FixedUpdate执行周期变长
            if (controller.fixedTimer - controller.RemainingTime > Time.fixedDeltaTime * 2)
                return;

            controller.fixedTimer += Time.fixedDeltaTime;

            controller.OnBuffFixedUpdate();
        }

        private void LateUpdateSetting(BuffController controller)
        {
            controller.OnBuffLateUpdate();
        }

        /// <summary>
        /// 终止运行中的所有Buff，调用所有控制器的OnBuffDestroy方法,该方法不会触发BuffController的OnBuffRemove方法以及Handler的移除回调
        /// </summary>
        public void StopAllBuffController()
        {
            foreach (var item in mTable.Values)
            {
                for (int i = 0; i < item.Count; i++)
                {
                    var controller = item[i];
                    if (controller.BuffLayer > 1)
                    {
                        for (int j = 0; j < controller.BuffLayer; i++)
                        {
                            controller.onBuffRemove?.Invoke(controller.BuffLayer - j);
                            controller.OnBuffRemove();
                        }
                    }

                    onBuffDestroyCallBack?.Invoke(controller);
                    controller.OnBuffDestroy();
                    BuffController.Release(controller);
                }
            }
            release.Clear();
            mTable.Clear();
        }

        private void OnDestroy()
        {
            onBuffDestroyCallBack.RemoveAllListeners();
            onBuffAddCallBack.RemoveAllListeners();
            StopAllBuffController();
            mTable.Dispose();
        }
    }
}
