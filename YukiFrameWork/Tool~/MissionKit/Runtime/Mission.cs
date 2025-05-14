///=====================================================
/// - FileName:      Mission.cs
/// - NameSpace:     YukiFrameWork.MissionKit
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/10/12 19:43:41
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using YukiFrameWork.Extension;
namespace YukiFrameWork.Missions
{	
	/// <summary>
	/// 任务基类本身
	/// </summary>
	public class Mission : IDisposable
	{
		private IMissionData missionData;

		public IMissionData MissionData => missionData;
		public MissionGroup Group { get; }
		public MissionStatus Status { get; internal set; }
		
		/// <summary>
		/// 任务基类进行实例注册访问的容器
		/// </summary>
		public Container Container { get; }	
		public Mission(IMissionData missionData,MissionGroup missionGroup)
		{
			this.missionData = missionData;
			startingMissionCondition = new List<IMissionCondition>();
			completedMissionCondition = new List<IMissionCondition>();
			failedMissionCondition = new List<IMissionCondition>();
			Container = new Container();
			Inject_All_Condition();		
			ResetMission();						
			Group = missionGroup;		
        }
	            
		/// <summary>
		/// 检测所有的条件
		/// </summary>
		/// <param name="conditions"></param>
		/// <returns></returns>
		private bool CheckConditions(List<IMissionCondition> conditions)
		{
			if (conditions.Count == 0) return false;

			foreach (var item in conditions)
			{
				if (!item.Condition())
					return false;
			}

			return true;
		}
	
		/// <summary>
		/// 当任务已经开始，更新任务的状态
		/// </summary>
		private void Update_Mission_Status()
		{
			if (Status != MissionStatus.Running) return;
            bool completed = CheckConditions(completedMissionCondition);
            bool failed = CheckConditions(failedMissionCondition);            
            //完成优先级大于失败
            if (completed)
                ChangeStatus(MissionStatus.Completed);
            else if (failed)
                ChangeStatus(MissionStatus.Failed);
        }

		internal void ChangeStatus(MissionStatus status)
		{
			if (Status == status) return;
			Status = status;			
			switch (status)
			{
				case MissionStatus.Idle:
					onMissionIdle.SendEvent(missionData);
					break;
				case MissionStatus.Running:
					onMissionStarting.SendEvent(missionData);
					break;
				case MissionStatus.Completed:
					onMissionCompleted.SendEvent(missionData);
					break;
				case MissionStatus.Failed:
					onMissionFailed.SendEvent(missionData);
					break;
			}
		}

		/// <summary>
		/// 开始任务
		/// </summary>
		public bool StartMission()
		{
			if (Status != MissionStatus.Idle)
			{
				LogKit.W("任务已经处于执行中或者已经完成/失败了");
				return false;
			}
            if (startingMissionCondition.Count != 0 && !CheckConditions(startingMissionCondition))
                return false;
            ChangeStatus(MissionStatus.Running);
			return true;
        }

		/// <summary>
		/// 完成任务的执行方法 任务没开始则无效
		/// </summary>
		public void CompletedMission()
		{
			if (Status != MissionStatus.Running) return;

			ChangeStatus(MissionStatus.Completed);
		}

        /// <summary>
        /// 任务失败的执行方法 任务没开始则无效
        /// </summary>
        public void FailedMission()
		{
            if (Status != MissionStatus.Running) return;
			ChangeStatus(MissionStatus.Failed);
        }

		/// <summary>
		/// 重置任务状态为待机
		/// </summary>
		public void ResetMission()
		{
			ChangeStatus(MissionStatus.Idle);
		}	
		/// <summary>
		/// 当任务调用ResetMission方法重置/第一次加载回待机状态时触发
		/// </summary>
		public readonly EasyEvent<IMissionData> onMissionIdle = new EasyEvent<IMissionData>();
		/// <summary>
		/// 当任务完成时触发
		/// </summary>
		public readonly EasyEvent<IMissionData> onMissionCompleted = new EasyEvent<IMissionData>();
		/// <summary>
		/// 当任务失败时触发
		/// </summary>
		public readonly EasyEvent<IMissionData> onMissionFailed = new EasyEvent<IMissionData>();
		/// <summary>
		/// 当任务开始时触发
		/// </summary>
		public readonly EasyEvent<IMissionData> onMissionStarting = new EasyEvent<IMissionData>();

		/// <summary>
		/// 当任务开始后持续触发的回调(待机/完成/失败均不会触发)
		/// </summary>
		public readonly EasyEvent<IMissionData> onMissionUpdate = new EasyEvent<IMissionData>();

        /// <summary>
        /// 当任务开始后持续触发的回调(待机/完成/失败均不会触发)
        /// </summary>
        public readonly EasyEvent<IMissionData> onMissionFixedUpdate = new EasyEvent<IMissionData>();

        /// <summary>
        /// 当任务开始后持续触发的回调(待机/完成/失败均不会触发)
        /// </summary>
        public readonly EasyEvent<IMissionData> onMissionLateUpdate = new EasyEvent<IMissionData>();

		internal void FixedUpdate()
		{
			if (Status != MissionStatus.Running) return;
			onMissionFixedUpdate.SendEvent(missionData);
		}

		internal void LateUpdate()
		{
			if (Status != MissionStatus.Running) return;
			onMissionLateUpdate.SendEvent(missionData);
		}

		internal void Update()
		{
			if (Status != MissionStatus.Running) return;
			onMissionUpdate.SendEvent(missionData);
			Update_Mission_Status();
		}

        private void Inject_All_Condition()
		{
			Inject_StartingCondition();
			Inject_CompletedCondition();
			Inject_FailedCondition();
		}

		private void Inject_StartingCondition()
		{
			for (int i = 0; i < missionData.StartingCondition.Count; i++)
				Inject_ConditionNonAlloc(missionData.StartingCondition[i], startingMissionCondition);
		}

		private void Inject_CompletedCondition()
		{
			for (int i = 0; i < missionData.CompletedCondition.Count; i++)
				Inject_ConditionNonAlloc(missionData.CompletedCondition[i], completedMissionCondition);
		}

		private void Inject_FailedCondition()
		{
			for (int i = 0; i < missionData.FailedCondition.Count; i++)
				Inject_ConditionNonAlloc(missionData.FailedCondition[i], failedMissionCondition);
		}

		private void Inject_ConditionNonAlloc(string typeFullName, List<IMissionCondition> result)
		{
			if (MissionKit.Missions_runtime_condition_dicts.TryGetValue(typeFullName, out Type type))
			{
				IMissionCondition condition = Activator.CreateInstance(type) as IMissionCondition;
                condition.Mission = this;
                condition.OnInit();				
                result.Add(condition);
			}
		}

        public void Dispose()
        {
			startingMissionCondition.Clear();
			completedMissionCondition.Clear();
			failedMissionCondition.Clear();
			Container.Dispose();
			missionData = null;			

        }

        /// <summary>
        /// 开始任务所有的条件
        /// </summary>
        private List<IMissionCondition> startingMissionCondition;
		/// <summary>
		/// 完成任务所有的条件
		/// </summary>
		private List<IMissionCondition> completedMissionCondition;
		/// <summary>
		/// 任务失败所有的条件
		/// </summary>
		private List<IMissionCondition> failedMissionCondition;		
				

		
    }
}
