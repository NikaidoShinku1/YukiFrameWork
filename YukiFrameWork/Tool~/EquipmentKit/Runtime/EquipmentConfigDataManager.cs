///=====================================================
/// - FileName:      EquipmentDataManager.cs
/// - NameSpace:     RPG
/// - Description:   高级定制脚本生成
/// - Creation Time: 12/12/2025 12:32:33 PM
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
namespace YukiFrameWork.Equips
{
    [CreateAssetMenu(fileName = nameof(EquipmentConfigDataManager),menuName = "YukiFrameWork/装备配置管理器" + nameof(EquipmentConfigDataManager))]
    public class EquipmentConfigDataManager : ScriptableObject
    {

        [ListDrawerSettings(), ReadOnly]
        public List<EquipmentConfigBase> equipmentConfigBases = new List<EquipmentConfigBase>();

        public Action onValidate;

        private void OnValidate()
        {
            onValidate?.Invoke();
        }

#if UNITY_EDITOR

        [UnityEditor.Callbacks.OnOpenAsset(0)]
        private static bool OnOpenAsset(int insId, int line)
        {
            EquipmentConfigDataManager obj = UnityEditor.EditorUtility.InstanceIDToObject(insId) as EquipmentConfigDataManager;
            if (obj != null)
            {
                EquipmentConfigDataEditorWindow.ShowWindow();
            }
            return obj != null;
        }
#endif


    }
}
