///=====================================================
/// - FileName:      MissionKit.cs
/// - NameSpace:     YukiFrameWork.Missions
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/10/27 0:17:37
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using YukiFrameWork.Extension;
using System.Linq;
using System.Collections;
using System.Runtime.InteropServices;
namespace YukiFrameWork.Missions
{
	public static class MissionKit
	{
        private static IMissionLoader loader;
        public static void Init(string projectName)
        {
            Init(new ABManagerMissionLoader(projectName));
        }

        public static void Init(IMissionLoader loader)
        {
            MissionKit.loader = loader;
        }

        static bool runtime_Initer = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Runtime_Mission_Collections_Init()
        {
            if (runtime_Initer) return;
            runtime_Initer = true;
            MonoHelper.Update_AddListener(Update_All_Missions_Status);
            MonoHelper.FixedUpdate_AddListener(FixedUpdate_AllMissions_Status);
            MonoHelper.LateUpdate_AddListener(LateUpdate_All_Missions_Status);
        }

        static void FixedUpdate_AllMissions_Status(MonoHelper _)
        {
            foreach (var mission in missions_runtime_groups.Values)
            {
                mission.FixedUpdate_Mission();
            }
        }

        static void LateUpdate_All_Missions_Status(MonoHelper _)
        {
            foreach (var mission in missions_runtime_groups.Values)
            {
                mission.LateUpdate_Mission();
            }
        }

        static void Update_All_Missions_Status(MonoHelper _)
        {           
            foreach (var mission in missions_runtime_groups.Values)                         
                mission.Update_Mission();          
        }
     

        static MissionKit()
        {
            missions_runtime_condition_dicts = AssemblyHelper
                .GetTypes(type => typeof(IMissionCondition)
                .IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface)
                .ToDictionary(x => x.FullName, x => x);
        }

        /// <summary>
		/// 手动添加一个新的任务数据
		/// </summary>
		/// <param name="missionData"></param>
		public static void AddMissionData(IMissionData missionData)
        {
            if (missions_runtime_dicts.ContainsKey(missionData.Key))
            {
#if YukiFrameWork_DEBUGFULL
                LogKit.W("试图添加已经存在的任务! Mission Key:" + missionData.Key);
#endif
                return;
            }

            //添加任务
            missions_runtime_dicts[missionData.Key] = missionData;
        }

        /// <summary>
        /// 移除已经创建的某个任务数据
        /// </summary>
        /// <param name="key"></param>
        public static bool RemoveMissionData(string key)
        {           
            return missions_runtime_dicts.Remove(key);
        }
#if UNITY_2021_1_OR_NEWER
        /// <summary>
        /// 移除已经创建的某个任务数据
        /// </summary>
        /// <param name="key"></param>
        public static bool RemoveMissionData(string key,out IMissionData data)
        {
            return missions_runtime_dicts.Remove(key,out data);           
        }
#endif
        public static IMissionData GetMissionData(string key)
        {
            missions_runtime_dicts.TryGetValue(key, out var data);
            return data;
        }

        private readonly static Dictionary<string, IMissionData> missions_runtime_dicts = new Dictionary<string, IMissionData>();
        private readonly static Dictionary<string, MissionGroup> missions_runtime_groups = new Dictionary<string, MissionGroup>();
        private readonly static Dictionary<string, MissionParam> missions_runtime_params = new Dictionary<string, MissionParam>();

        public static IReadOnlyDictionary<string, IMissionData> Missions_runtime_dicts => missions_runtime_dicts;
        public static IReadOnlyDictionary<string, MissionGroup> Missions_runtime_groups => missions_runtime_groups;
        public static IReadOnlyDictionary<string, MissionParam> Missions_runtime_params => missions_runtime_params;

        

        /// <summary>
        /// 创建一个新的任务分组
        /// </summary>
        /// <param name="groupKey"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static MissionGroup CreateMissionGroup(string groupKey)
        {
            if (missions_runtime_groups.ContainsKey(groupKey))
                throw new Exception("该任务分组已经存在:" + groupKey);

            MissionGroup missionGroup = new MissionGroup(groupKey);
            missions_runtime_groups[groupKey] = missionGroup;
            return missionGroup;
        }

        /// <summary>
        /// 获取指定的任务分组
        /// </summary>
        /// <param name="groupKey"></param>
        /// <returns></returns>
        public static MissionGroup GetMissionGroup(string groupKey)
        {
            missions_runtime_groups.TryGetValue(groupKey, out var group);
            return group;
        }

        /// <summary>
        /// 移除指定的任务分组
        /// </summary>
        /// <param name="groupKey"></param>
        /// <returns></returns>
        public static bool RemoveMissionGroup(string groupKey)
        {
            return missions_runtime_groups.Remove(groupKey);
        }

#if UNITY_2021_1_OR_NEWER
        public static bool RemoveMissionGroup(string groupKey, out MissionGroup missionGroup)
        {
            return missions_runtime_groups.Remove(groupKey, out missionGroup);
        }
#endif
        /// <summary>
        /// 所有运行时条件
        /// </summary>
        private static Dictionary<string, Type> missions_runtime_condition_dicts = new Dictionary<string, Type>();

        public static IReadOnlyDictionary<string, Type> Missions_runtime_condition_dicts => missions_runtime_condition_dicts;    
        

        public static void LoadMissionConfigManager(MissionConfigManager missionConfigManager)
        {
            foreach (var dict in missionConfigManager.missionParams_dict)
                AddParam(dict.Key, dict.Value);
            foreach (var configBase in missionConfigManager.missionConfigBases)
            {
                var missions = configBase.Missions;
                foreach (var item in missions)
                    AddMissionData(item);              
            }
            loader?.UnLoad(missionConfigManager);
        }

        public static void LoadMissionConfigManager(string path)
        {
            LoadMissionConfigManager(loader.Load<MissionConfigManager>(path));
        }

        [DisableEnumeratorWarning]
        public static IEnumerator LoadMissionConfigManagerAsync(string path)
        {
            bool completed = false;
            loader.LoadAsync<MissionConfigManager>(path, item => {
                LoadMissionConfigManager(item);
                completed = true;
            });

            yield return CoroutineTool.WaitUntil(() => completed);
        }


        /// <summary>
        /// 添加新的任务参数。
        /// </summary>
        /// <param name="key"></param>
        /// <param name="param"></param>
        public static void AddParam(string key, MissionParam param)
        {
            missions_runtime_params[key] = param;
        }
        
        [Serializable]
        public class Mission_Runtime_Data
        {
            public MissionStatus missionStatus;
            public string missionKey;          
        }
        public static string SaveMissions()
        {
            Dictionary<string, List<Mission_Runtime_Data>> missionSaveDatas = new Dictionary<string, List<Mission_Runtime_Data>>();

            foreach (var group in missions_runtime_groups)
            {
                if (!missionSaveDatas.TryGetValue(group.Key, out List<Mission_Runtime_Data> data))
                {
                    data = new List<Mission_Runtime_Data>();
                }

                data = group.Value.Mission_Dicts.Select(x => 
                {
                    return new Mission_Runtime_Data()
                    {
                        missionStatus = x.Value.Status,
                        missionKey = x.Value.MissionData.Key
                    };
                }).ToList();

                missionSaveDatas[group.Key] = data;
            }
            string json = SerializationTool.SerializedObject(missionSaveDatas, settings: new Newtonsoft.Json.JsonSerializerSettings()
            {
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Include
            });

            if(DefaultSaveAndLoader)
                PlayerPrefs.SetString(MISSION_SAVING_KEY,json);

            return json;
        }

        private const string MISSION_SAVING_KEY = nameof(MISSION_SAVING_KEY);

        /// <summary>
        /// 通过内置的存档信息读取(仅支持Json)
        /// </summary>
        public static void LoadMissions(bool eventTrigger = true)
        {
            if (!DefaultSaveAndLoader)  
            {
                Debug.LogError("没有开启" + nameof(DefaultSaveAndLoader) + "本地存档--- 无法内置读取，请使用Load(string info,bool eventTrigger = true)进行外部的Json信息传输!");
                return;
            }
            LoadMissions(PlayerPrefs.GetString(MISSION_SAVING_KEY, string.Empty),eventTrigger);
        }

        public static void LoadMissions(string json,bool eventTrigger = true)
        {
            if (json.IsNullOrEmpty()) return;

            Dictionary<string, List<Mission_Runtime_Data>> missionSaveDatas = SerializationTool.DeserializedObject<Dictionary<string, List<Mission_Runtime_Data>>>(json);

            foreach (var groupKey in missionSaveDatas.Keys)
            {
                MissionGroup group = MissionKit.GetMissionGroup(groupKey);
                group ??= CreateMissionGroup(groupKey);

                var datas = missionSaveDatas[groupKey];

                for (int i = 0; i < datas.Count; i++)
                {
                    var data = datas[i];

                    if (data == null) continue;

                    Mission mission = group.GetMission(data.missionKey);
                    if (mission == null)
                    {
                        group.CreateMission(data.missionKey, m => mission = m);
                    }                   
                    if (mission.Status == data.missionStatus)
                        continue;

                    if (eventTrigger)
                        mission.ChangeStatus(data.missionStatus);
                    else
                        mission.Status = data.missionStatus;
                }
            }

        }
        /// <summary>
        /// 是否进行本地默认保存(默认开启(适合不用存档系统时))
        /// </summary>
        public static bool DefaultSaveAndLoader { get; set; } = true;
    }
}
