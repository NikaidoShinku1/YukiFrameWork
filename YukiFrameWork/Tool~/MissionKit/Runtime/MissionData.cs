﻿///=====================================================
/// - FileName:      MissionData.cs
/// - NameSpace:     YukiFrameWork.MissionKit
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/10/11 16:25:15
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System.Collections;
using YukiFrameWork.Extension;
namespace YukiFrameWork.Missions
{
	public enum MissionStatus
	{
		/// <summary>
		/// 任务处于待机状态
		/// </summary>
		Idle,
		/// <summary>
		/// 任务正在进行中
		/// </summary>
		Running,
		/// <summary>
		/// 任务完成
		/// </summary>
		Completed,
		/// <summary>
		/// 任务失败
		/// </summary>
		Failed

	}
	public interface IMissionData
	{
		/// <summary>
		/// 任务的唯一标识(无论有多少个So配置，都必须保证每一个任务的标识唯一)
		/// </summary>
		string Key { get; set; }
		/// <summary>
		/// 任务的名称
		/// </summary>
		string Name { get; set; }
		/// <summary>
		/// 任务的介绍
		/// </summary>
		string Description { get; set; }
		/// <summary>
		/// 任务的图标
		/// </summary>
		Sprite Icon { get; set; }
		/// <summary>
		/// 任务的所有接受/启动条件类型
		/// </summary>
		List<string> StartingCondition { get; }
		/// <summary>
		/// 任务的所有完成条件类型
		/// </summary>
		List<string> CompletedCondition { get; }

		/// <summary>
		/// 任务可能失败的条件
		/// </summary>
		List<string> FailedCondition { get; }

		/// <summary>
		/// 这个任务需要使用到的参数标识配置集合
		/// </summary>
		List<string> MissionParams { get; }
		/// <summary>
		/// 任务类型(可空)
		/// </summary>
		string MissionType { get; set; }

		string Icon_Path { internal get; set; }
	}
	/// <summary>
	/// 任务数据
	/// </summary>
	[Serializable]
    public class MissionData : IMissionData
    {
        [JsonProperty][field:SerializeField,LabelText("任务的唯一ID")]public string Key { get ; set ; }
        [JsonProperty][field:SerializeField, LabelText("任务名称")] public string Name { get ; set ; }
        [JsonProperty][field:SerializeField, LabelText("任务介绍"),TextArea] public string Description { get ; set ; }
        [JsonIgnore][field:SerializeField,LabelText("任务精灵"),PreviewField(50)]public Sprite Icon { get ; set ; }
		
        [JsonProperty(PropertyName = nameof(StartingCondition)), LabelText("开始任务所有的条件"), ValueDropdown(nameof(AllConditionCollection))]
		[InfoBox("对于任务的开始，需要手动调用Mission任务基类的非静态.Start方法。当没有开始条件时，调用Start方法即视为任务启动。",InfoMessageType.Warning)]
        [SerializeField] private List<string> mStartingCondition = new List<string>();

        [JsonProperty(PropertyName = nameof(CompletedCondition)),LabelText("完成任务所有的条件"), ValueDropdown(nameof(AllConditionCollection))]
		[SerializeField]private List<string> mCompletedCondition = new List<string>();

		[JsonIgnore]
		public List<string> CompletedCondition => mCompletedCondition;

		[JsonIgnore]
        public List<string> StartingCondition => mStartingCondition;

		[JsonProperty(PropertyName = nameof(FailedCondition)), LabelText("任务失败所有的条件"),InfoBox("如果没有失败条件，则任务永远不会失败"),ValueDropdown(nameof(AllConditionCollection))]
		[SerializeField]private List<string> mFailedCondition = new List<string>();		
		[JsonIgnore]
		public List<string> FailedCondition => mFailedCondition;

		[JsonIgnore]
        private IEnumerable mAllConditionCollection;
        [JsonIgnore]
        private IEnumerable AllConditionCollection
        {
            get
            {
                if (mAllConditionCollection == null)
                {
                    var dict = MissionKit.Missions_runtime_condition_dicts;
                    ValueDropdownList<string> items = new ValueDropdownList<string>();
                    foreach (var key in dict.Keys)
                        items.Add(key, dict[key].ToString());

                    mAllConditionCollection = items;
                }

                return mAllConditionCollection;
            }
        }

		/// <summary>
		/// 任务的类型(默认是没有的)
		/// </summary>
		[field:SerializeField,LabelText("任务类型")]
		[JsonProperty]
#if UNITY_EDITOR
		[field: ValueDropdown(nameof(Type))]
#endif
		public string MissionType { get; set; }
#if UNITY_EDITOR
		[JsonIgnore]
		private IEnumerable Type => MissionConfigBase.MissionsTypes;

		[JsonIgnore]
		private IEnumerable Params => MissionConfigBase.MissionsParams;
#endif
		[SerializeField, LabelText("可选任务参数"), JsonProperty]
#if UNITY_EDITOR
		[ValueDropdown(nameof(Params), IsUniqueList = true)]
#endif      
        private List<string> missionParams = new List<string>();
		[JsonIgnore]
        public List<string> MissionParams => missionParams;
		[JsonProperty]
        string IMissionData.Icon_Path { get ; set; }
    }
}