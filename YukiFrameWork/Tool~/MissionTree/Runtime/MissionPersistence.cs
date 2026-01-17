///=====================================================
/// - FileName:      MissionPersistence.cs
/// - NameSpace:     YukiFrameWork.Missions
/// - Description:   高级定制脚本生成
/// - Creation Time: 1/17/2026 1:14:52 PM
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using YukiFrameWork.Pools;
namespace YukiFrameWork.Missions
{
    [Serializable]
    public class MissionPersistence
    {
        public string missionTreekey;

        public List<MissionPersistenceInfo> missionPersistenceInfos = new List<MissionPersistenceInfo>();

        [Serializable]
        public struct MissionPersistenceInfo
        {
            public int missionId;
            public MissionStatus missionStatus;
            
        }


    }
}
