///=====================================================
/// - FileName:      StateTransitionData.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/9 13:26:26
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using StateConditionGroup = System.Collections.Generic.List<YukiFrameWork.Machine.StateConditionData>;
namespace YukiFrameWork.Machine
{    
    [Serializable]
    public class StateTransitionData 
    {
        public string fromStateName;
        public string toStateName;
        public bool autoSwitch;
        public override string ToString()
        {
            return $"{fromStateName}:{toStateName}";
        }
        /// <summary>
        /// 默认的条件列表
        /// </summary>
        public StateConditionGroup conditions = new StateConditionGroup();

        /// <summary>
        /// 可选的条件列表
        /// </summary>
        public List<StateConditionGroup> conditionGroups = new List<StateConditionGroup>();

        public bool Empty
        {
            get
            {
                if (conditions.Count != 0)
                    return false;

                foreach (var item in conditionGroups)
                {
                    if (item.Count != 0) return false;
                }

                return true;
            }
        }
    }
}
