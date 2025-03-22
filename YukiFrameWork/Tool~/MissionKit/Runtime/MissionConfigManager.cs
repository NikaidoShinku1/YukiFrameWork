///=====================================================
/// - FileName:      MissionConfigManager.cs
/// - NameSpace:     YukiFrameWork.Example
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/22 19:01:54
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using Sirenix.OdinInspector;
using System.Linq;
namespace YukiFrameWork.Missions
{
    [CreateAssetMenu(fileName ="MissionConfigManager",menuName = "YukiFrameWork/任务配置管理器MissionConfigManager")]
    public class MissionConfigManager : ScriptableObject
    {
        [ListDrawerSettings(),ReadOnly]
        public List<MissionConfigBase> missionConfigBases = new List<MissionConfigBase>();

        [LabelText("任务参数")]
        [InfoBox("所有的配置管理器中的任务参数均同步共享，a配置有的任务参数在b配置中也可以为参数选择使用!")]
        [DictionaryDrawerSettings(KeyLabel = "参数名称/标识", ValueLabel = "可配置数据项")]
        public YDictionary<string, MissionParam> missionParams_dict = new YDictionary<string, MissionParam>();

        /// <summary>
        /// 任务类型添加
        /// </summary>
        [LabelText("任务类型添加:")]
        [InfoBox("在这里添加的任务类型，Assets下所有的MissionConfigManager共享,在配置任务时可以直接使用")]
        public List<string> missionTypes = new List<string>();

        public Action onValidate;

        private void OnValidate()
        {
            onValidate?.Invoke();
        }

#if UNITY_EDITOR
        public static IEnumerable MissionTypes => YukiAssetDataBase.FindAssets<MissionConfigManager>().SelectMany(x => x.missionTypes)
            .Select(x => new ValueDropdownItem() { Text = x,Value = x});

        [UnityEditor.Callbacks.OnOpenAsset(0)]
        private static bool OnOpenAsset(int insId, int line)
        {
            MissionConfigManager obj = UnityEditor.EditorUtility.InstanceIDToObject(insId) as MissionConfigManager;
            if (obj != null)
            {
                MissionConfigBaseEditorWindow.ShowWindow();
            }
            return obj != null;
        } 
#endif

    }
}

