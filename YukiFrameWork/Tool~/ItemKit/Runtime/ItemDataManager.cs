///=====================================================
/// - FileName:      ItemDataManager.cs
/// - NameSpace:     YukiFrameWork.Example
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/21 20:00:13
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System.Collections;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace YukiFrameWork.Item
{
    [CreateAssetMenu(fileName ="ItemDataManager",menuName = "YukiFrameWork/物品配置管理器ItemDataManager")]
    public class ItemDataManager : ScriptableObject
    {
        [ListDrawerSettings,ReadOnly]
        public List<ItemDataBase> itemDataBases = new List<ItemDataBase>();
        public Action onValidate;
        /// <summary>
        /// 多个配置之间的不同类型会被共享
        /// </summary>
        [LabelText("保存所有的物品类型")]
        public List<string> all_ItemTypes = new List<string>()
        {
            "Consumable",
            "Equipment",
            "Material",
            "Weapon"
        };


#if UNITY_EDITOR
        public static IEnumerable AllManager_ItemTypes => YukiAssetDataBase.FindAssets<ItemDataManager>().SelectMany(x => x.all_ItemTypes)
            .Select(x => new ValueDropdownItem() { Text = x, Value = x });
#endif

        private void OnValidate()
        {
            onValidate?.Invoke();
        }

#if UNITY_EDITOR
        [UnityEditor.Callbacks.OnOpenAsset(0)]
        private static bool OnOpenAsset(int insId, int line)
        {
            ItemDataManager obj = EditorUtility.InstanceIDToObject(insId) as ItemDataManager;
            if (obj != null)
            {
                ItemDataBaseEditorWindow.ShowWindow();
            }
            return obj != null;
        }
#endif   
    }
}
