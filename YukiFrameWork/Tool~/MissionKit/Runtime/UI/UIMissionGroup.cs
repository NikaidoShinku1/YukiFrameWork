///=====================================================
/// - FileName:      UIMissionGroup.cs
/// - NameSpace:     YukiFrameWork.Missions
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/10/28 22:53:29
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using XFABManager;
namespace YukiFrameWork.Missions
{
	public class UIMissionGroup : YMonoBehaviour
	{
        public enum UIMissionGenericType
        {
            [LabelText("使用插槽预制体生成")]
            Template,
            [LabelText("使用已经存在的插槽")]
            SlotExist
        }

        [SerializeField, LabelText("MissionGroup分组的标识：")]
        private string mGroupKey;

        public string GroupKey
        {
            get => mGroupKey;
            set
            {
                if (!mGroupKey.Equals(value))
                {
                    mGroupKey = value;
                    Refresh();
                }
            }
        }      

        [LabelText("UI Mission生成的类型:"), PropertySpace()]
        public UIMissionGenericType Type;

        [HideIf(nameof(Type), UIMissionGenericType.SlotExist)]
        [LabelText("UI Mission的预制体:")]
        public UIMission UIMissionPrefab;
        [HideIf(nameof(Type), UIMissionGenericType.SlotExist)]
        [LabelText("UI Mission的根节点:")]
        public RectTransform UIMissionRoot;

        [LabelText("已经存在的分组集合"), HideIf(nameof(Type), UIMissionGenericType.Template)]
        public List<UIMission> existSlots = new List<UIMission>();

        private void Reset()
        {
            UIMissionRoot = transform as RectTransform;
        }

        private void Start()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (GroupKey.IsNullOrEmpty()) return;
            if (Type == UIMissionGenericType.Template)
            {
                UIMissionPrefab.Hide();

                foreach (var item in UIMissionRoot.GetComponentsInChildren<UIMission>(true))
                {
                    if (item == UIMissionPrefab) continue;
                    item.UnLoad();
                }

                foreach (var item in MissionKit.GetMissionGroup(GroupKey)
                    .UnRegisterMissionRefresh(Refresh)
                    .RegisterMissionRefresh(Refresh).Mission_Dicts.Values)
                {
                    GameObjectLoader.Load(UIMissionPrefab.gameObject, UIMissionRoot).GetComponent<UIMission>().InitMission(item);
                }
            }
            else if (Type == UIMissionGenericType.SlotExist)
            {
                var missions = MissionKit
                    .GetMissionGroup(GroupKey)
                    .UnRegisterMissionRefresh(Refresh)
                    .RegisterMissionRefresh(Refresh)
                    .GetMissions();

                for (int i = 0; i < Mathf.Min(existSlots.Count,missions.Length); i++)
                {
                    existSlots[i].InitMission(missions[i]);
                }
            }
        }

        private void OnDestroy()
        {
            MissionKit.GetMissionGroup(GroupKey)
                    ?.UnRegisterMissionRefresh(Refresh);
        }

    }
}
