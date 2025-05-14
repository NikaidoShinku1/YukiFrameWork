///=====================================================
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
		/// 任务类型(可空)
		/// </summary>
		string MissionType { get; set; }

		
	}
	/// <summary>
	/// 任务数据
	/// </summary>
	[Serializable]
    public class MissionData : IMissionData
    {

		[JsonProperty]
		[SerializeField, LabelText("任务的唯一ID")]
		private string key;
		[JsonIgnore,ExcelIgnore]
		public string Key { get => key; set => key = value; }
		[JsonProperty]
		[SerializeField, LabelText("任务名称")]
		private string name;
        [JsonIgnore, ExcelIgnore]
        public string Name { get => name ; set => name = value; }
		[JsonProperty]
		[SerializeField, LabelText("任务介绍"), TextArea]
		private string description;
        [JsonIgnore, ExcelIgnore]
        public string Description { get => description ; set => description = value; }
		[SerializeField,JsonProperty]
#if UNITY_EDITOR
		[CustomValueDrawer(nameof(DrawPreview))]
#endif
		private Sprite icon;
		public Sprite Icon { get => icon; set => icon = value; }
#if UNITY_EDITOR
        private void DrawPreview()
        {

            GUILayout.BeginHorizontal();

            GUILayout.Label("Mission的图标样式");
            icon = (Sprite)UnityEditor.EditorGUILayout.ObjectField(this.icon, typeof(Sprite), true, GUILayout.Width(50), GUILayout.Height(50));
            GUILayout.EndHorizontal();
        }
#endif
        [JsonProperty(PropertyName = nameof(StartingCondition)), LabelText("开始任务所有的条件"), ValueDropdown(nameof(AllConditionCollection))]
		[InfoBox("对于任务的开始，需要手动调用Mission任务基类的非静态.Start方法。当没有开始条件时，调用Start方法即视为任务启动。",InfoMessageType.Warning)]
        [SerializeField] private List<string> mStartingCondition = new List<string>();

        [JsonProperty(PropertyName = nameof(CompletedCondition)),LabelText("完成任务所有的条件"), ValueDropdown(nameof(AllConditionCollection))]
		[SerializeField]private List<string> mCompletedCondition = new List<string>();

        [JsonProperty(PropertyName = nameof(FailedCondition)), LabelText("任务失败所有的条件"), InfoBox("如果没有失败条件,除非手动调用任务失败，否则任务永远不会失败"), ValueDropdown(nameof(AllConditionCollection))]
        [SerializeField] private List<string> mFailedCondition = new List<string>();

        [JsonIgnore,ExcelIgnore]
		public List<string> CompletedCondition => mCompletedCondition;

		[JsonIgnore,ExcelIgnore]
        public List<string> StartingCondition => mStartingCondition;
		
		[JsonIgnore,ExcelIgnore]
		public List<string> FailedCondition => mFailedCondition;

		[JsonIgnore,ExcelIgnore]
        private IEnumerable mAllConditionCollection;
        [JsonIgnore,ExcelIgnore]
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
		[SerializeField, LabelText("任务类型")]
		[JsonProperty]
#if UNITY_EDITOR
		[ValueDropdown(nameof(Type))]
#endif
		private string missionType;
		[ExcelIgnore,JsonIgnore]
		public string MissionType { get => missionType; set => missionType = value; }
#if UNITY_EDITOR
		[JsonIgnore,ExcelIgnore]
		private IEnumerable Type => MissionConfigManager.MissionTypes;
#endif		
    }
}
