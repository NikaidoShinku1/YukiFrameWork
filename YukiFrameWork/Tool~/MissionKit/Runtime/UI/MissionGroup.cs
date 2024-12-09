///=====================================================
/// - FileName:      MissionGroup.cs
/// - NameSpace:     YukiFrameWork.MissionKit
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/10/26 18:44:33
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
namespace YukiFrameWork.Missions
{
	public class MissionGroup
	{
		private Dictionary<string,Mission> missions = new Dictionary<string, Mission>();

		public string GroupKey { get; }

		public MissionGroup(string groupKey)
		{
			this.GroupKey = groupKey;
		}

		public IReadOnlyDictionary<string,Mission> Mission_Dicts => missions;

		/// <summary>
		/// 当任务有状态/数据更新时刷新事件
		/// </summary>
		private event Action onMissionRefresh;

		/// <summary>
		/// 当任务UI初始化时触发的事件
		/// </summary>
		internal Action<UIMission> onUIMissionInit;
		public event Func<IMissionData,bool> addMissionCondition = _ => true;

		public MissionGroup RegisterMissionRefresh(Action refresh)
		{
			onMissionRefresh += refresh;
			return this;
		}

		public MissionGroup RegisterUIMissionInit(Action<UIMission> action)
		{
			onUIMissionInit += action;
			return this;
		}

        public  MissionGroup UnRegisterUIMissionInit(Action<UIMission> action)
		{
			onUIMissionInit -= action;
			return this;
		}

        /// <summary>
        /// 为任务升序排序
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="orders"></param>
        /// <returns></returns>
        public MissionGroup OrderBy<TKey>(Func<KeyValuePair<string,Mission>,TKey> orders)
        {
            missions = missions.OrderBy(orders).ToDictionary(x => x.Key,x => x.Value);
			OnRefresh();
            return this;
        }

		/// <summary>
		/// 任务的状态更新是在Update中调用的。当有任意操作调用了刷新UI时，可能会出现在刷新后任务事件的状态没有同步的情况。所以等待一帧再刷新
		/// </summary>
		/// <returns></returns>		
		private async void OnRefresh()
		{
			await CoroutineTool.WaitForFrame();
            onMissionRefresh?.Invoke();
        }

        /// <summary>
        /// 为任务降序排序
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="orders"></param>
        /// <returns></returns>
        public MissionGroup OrderByDescending<TKey>(Func<KeyValuePair<string, Mission>, TKey> orders)
        {
            missions = missions.OrderByDescending(orders).ToDictionary(x => x.Key, x => x.Value);
            OnRefresh();
            return this;
        }

        public MissionGroup UnRegisterMissionRefresh(Action refresh)
        {
            onMissionRefresh -= refresh;
            return this;
        }      

		/// <summary>
		/// 创建任务
		/// </summary>
		/// <param name="missionData">任务数据</param>
		/// <param name="onMissionInit">当任务创建时触发的回调</param>
		/// <returns></returns>
		/// <exception cref="Exception"></exception>
        public MissionGroup CreateMission(IMissionData missionData,Action<Mission> onMissionInit = null)
		{
			if (!addMissionCondition.Invoke(missionData))
				return this;

			if (missions.ContainsKey(missionData.Key))
				throw new Exception("任务已经存在，请检查! Mission Key:" + missionData.Key);

			Mission mission = new Mission(missionData,this);
			missions[missionData.Key] = mission;
			onMissionInit?.Invoke(mission);
            onMissionRefresh?.Invoke();
			return this;
			
		}

		/// <summary>
		/// 根据任务类型获取任务集合
		/// </summary>
		/// <param name="missionType"></param>
		/// <returns></returns>
		public Mission[] GetMissionsByType(string missionType)
			=> missions.Values.Where(x => x.MissionData.MissionType == missionType).ToArray();

		/// <summary>
		/// 获取指定的任务
		/// </summary>
		/// <param name="missionKey"></param>
		/// <returns></returns>
		public Mission GetMission(string missionKey)
		{
			missions.TryGetValue(missionKey, out var mission);
			return mission;								
		}
        /// <summary>
        /// 创建任务
        /// </summary>
        /// <param name="missionData">任务数据</param>
        /// <param name="onMissionInit">当任务创建时触发的回调</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public MissionGroup CreateMission(string missionKey,Action<Mission> onMissionInit = null)
		{
			return CreateMission(MissionKit.GetMissionData(missionKey),onMissionInit);
		}

		/// <summary>
		/// 移除某一个任务
		/// </summary>
		/// <param name="missionKey"></param>
		/// <returns></returns>
		public bool RemoveMission(string missionKey)
		{          
#if UNITY_2021_1_OR_NEWER
            bool completed = missions.Remove(missionKey,out Mission mission);
            onMissionRefresh?.Invoke();
            mission?.Dispose();
			return completed;
#else
			if (!missions.ContainsKey(missionKey))
                return false;
            Mission mission = missions[missionKey];
            missions.Remove(missionKey);
            onMissionRefresh?.Invoke();
			mission.Dispose();
            return true;
#endif
        }
        //集体更新任务ToDo
        internal void Update_Mission()
		{
			foreach (var item in missions.Values)
			{
				item.Update();
			}
		}

		internal void FixedUpdate_Mission()
		{
			foreach (var item in missions.Values)
			{
				item.FixedUpdate();
			}
		}

		internal void LateUpdate_Mission()
		{
			foreach (var item in missions.Values)
			{
				item.LateUpdate();
			}
		}

        public Mission[] GetMissions()
        {
			return missions.Values.ToArray();
        }

		public void ForEach(Action<Mission> each)
		{
			foreach (var item in missions)
			{
				each?.Invoke(item.Value);
			}
		}

        public void ForEach(Action<string,Mission> each)
        {
			foreach (var item in missions)
			{
				each?.Invoke(item.Key, item.Value);
			}
        }

        /// <summary>
        /// 获取所有的任务标识
        /// </summary>
        public string[] AllMissionsKey => missions.Keys.ToArray();
		
    }
}
