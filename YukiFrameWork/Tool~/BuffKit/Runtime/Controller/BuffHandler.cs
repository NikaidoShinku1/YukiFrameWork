﻿///=====================================================
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
namespace YukiFrameWork.Buffer
{	
	public class BuffHandler : MonoBehaviour
	{      
		private BuffControllerTable mTable = new BuffControllerTable();
		private List<BuffController> release = new List<BuffController>();
		private UIBuffHandlerGroup handlerGroup;

		/// <summary>
		/// Buff添加时触发的回调，只要调用AddBuffer没有添加失败而改变了Buff的状态，则统一会调用该方法。并且可以拿到Controller
		/// </summary>
		public readonly EasyEvent<BuffController> onBuffAddCallBack = new EasyEvent<BuffController>();


        /// <summary>
        /// Buff移除时触发的回调，并且可以拿到Controller
        /// </summary>
        public readonly EasyEvent<BuffController> onBuffRemoveCallBack = new EasyEvent<BuffController>();

		public void SetUIBuffHandlerGroup(UIBuffHandlerGroup handlerGroup)
		{
			this.handlerGroup = handlerGroup;
		}
		public void AddBuffer<T>(IBuff buff,GameObject player) where T : BuffController,new()
		{						
			if (mTable.TryGetValue(buff.BuffKey, out var controllers))
			{				
                var item = controllers.FirstOrDefault();
				if (buff.AdditionType != BuffRepeatAdditionType.None 
					&& CheckBuffCounteractID(buff) 
					&& CheckBuffDisableID(buff) 
					&& item != null 
					&& item.OnAddBuffCondition()
					)
				{
                    switch (buff.AdditionType)
                    {                    
                        case BuffRepeatAdditionType.Reset:
                            {
                                item.RemainingTime = buff.BuffTimer;
                                if (item.UIBuffer != null)
                                    item.UIBuffer.OnBuffStart(buff, buff.BuffKey, item.BuffLayer);
                                onBuffAddCallBack.SendEvent(item);
                            }
                            break;
                        case BuffRepeatAdditionType.Multiple:
                            {
								if (buff.IsMaxStackableLayer && item.BuffLayer >= buff.MaxStackableLayer && buff.MaxStackableLayer > 0)
									return;							
                                item.BuffLayer++;
                                item.OnBuffStart();
								if (item.UIBuffer != null)
									item.UIBuffer.OnBuffStart(buff,buff.BuffKey,item.BuffLayer);
                                onBuffAddCallBack.SendEvent(item);
                            }
                            break;
                        case BuffRepeatAdditionType.MultipleAndReset:
                            {
                                if (buff.IsMaxStackableLayer && item.BuffLayer >= buff.MaxStackableLayer && buff.MaxStackableLayer > 0)
                                    return;
                                item.BuffLayer++;
                                item.RemainingTime = buff.BuffTimer;
                                item.OnBuffStart();
                                if (item.UIBuffer != null)
                                    item.UIBuffer.OnBuffStart(buff, buff.BuffKey,item.BuffLayer);
                                onBuffAddCallBack.SendEvent(item);

                            }
                            break;
                        case BuffRepeatAdditionType.MultipleCount:
                            {
                                T controller = BuffController.CreateInstance<T>(buff, player);
                                InitController(controller);
                                controllers.Add(controller);
                                controller.OnBuffStart();
								if (handlerGroup != null)
								{
									controller.UIBuffer = handlerGroup.CreateBuffer();
									controller.UIBuffer.OnBuffStart(buff,buff.BuffKey,item.BuffLayer);
                                    onBuffAddCallBack.SendEvent(controller);
                                }                               
                            }
                            break;
                    }
                }              
			}
			else
			{				
				T controller = BuffController.CreateInstance<T>(buff, player);
				InitController(controller);

				//如果检查没有会被抵消的Buff以及该Buff没有处于别的运行时Buff内的禁止Buff，则正常添加，否则不添加
				if (CheckBuffCounteractID(buff) && CheckBuffDisableID(buff) && controller.OnAddBuffCondition())
					AddBuffer(controller);
				else 
					release.Add(controller);
			}
		}

        public void AddBuffer<T>(string BuffKey, GameObject player) where T : BuffController, new()
        {
            IBuff buff = BuffKit.GetBuffByKey(BuffKey);
            AddBuffer<T>(buff, player);
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
			=> RemoveBuffer(buff.BuffKey);

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
		/// 得到当前运行的所有Buff的数量
		/// </summary>
		/// <returns></returns>
		public int GetAllBufferCount()
		{
			return mTable.Count;
		}

        private void InitController(BuffController controller)
		{
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
			string BuffKey = buff.BuffKey;	
			foreach (var item in mTable.Values)
			{
				for(int i = 0;i < item.Count;i++)
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
			if (handlerGroup != null)
			{
				var uiBuffer = handlerGroup.CreateBuffer();
				uiBuffer.OnBuffStart(buffController.Buffer, buffController.BuffKey,buffController.BuffLayer);
				buffController.UIBuffer = uiBuffer;
			}
            onBuffAddCallBack.SendEvent(buffController);
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
				}
			}
        }

        private void UpdateSetting(BuffController controller)
        {
            controller.OnBuffUpdate();
			if (controller.UIBuffer != null)
			{
				controller.UIBuffer.OnBuffUpdate(controller.RemainingTime);
			}
            controller.RemainingTime -= Time.deltaTime;
            if ((controller.OnRemoveBuffCondition()) || controller.RemainingTime <= 0)
            {			
                release.Add(controller);
            }            
        }

		private void OnControllerRemove<T>(T controller) where T : BuffController
		{
			if(controller.Buffer.IsBuffRemoveBySlowly)
				controller.BuffLayer--;
			if (controller.Buffer.SurvivalType == BuffSurvivalType.Timer)
			{				               
                controller.RemainingTime = controller.Buffer.BuffTimer;
			}
			controller.OnBuffRemove();
			if (controller.UIBuffer != null)
				controller.UIBuffer.OnBuffRemove(controller.BuffLayer);
            if (controller.BuffLayer <= 0 || !controller.Buffer.IsBuffRemoveBySlowly)
            {			
                controller.OnBuffDestroy();
				if (controller.UIBuffer != null)
				{
					controller.UIBuffer.OnBuffDestroy();
					if (handlerGroup != null)
						handlerGroup.ReleaseBuffer(controller.UIBuffer);
				}
                mTable.Remove(controller.BuffKey,controller);
                if (mTable.IsNullOrEmpty(controller.BuffKey))
                    mTable.Remove(controller.BuffKey);

				Debug.Log("回收的控制器类型:"+ controller.GetType());
				BuffController.Release(controller, controller.GetType());
            }
        }

		private void FixedUpdateSetting(BuffController controller)
		{
            controller.OnBuffFixedUpdate();
        }

		/// <summary>
		/// 终止运行中的所有Buff，调用所有控制器的OnBuffDestroy方法，该方法不会触发BuffController的OnBuffRemove方法
		/// </summary>
		public void StopAllBuffController()
		{
			foreach (var item in mTable.Values)
			{
				for (int i = 0; i < item.Count; i++)
				{					
					item[i].OnBuffDestroy();
					if (item[i].UIBuffer != null)
						item[i].UIBuffer.OnBuffDestroy();
				}
			}
			release.Clear();
            mTable.Clear();
        }

        private void OnDestroy()
        {
			release.Clear();
			mTable.Dispose();
			mTable.Clear();
        }
    }
}