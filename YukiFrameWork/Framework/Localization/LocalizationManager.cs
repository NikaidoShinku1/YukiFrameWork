///=====================================================
/// - FileName:      LocalizationManager.cs
/// - NameSpace:     YukiFrameWork.Example
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/19 18:42:36
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace YukiFrameWork
{
    [CreateAssetMenu(fileName ="LocalizationManager",menuName = "YukiFrameWork/本地化配置管理器LocalizationManager")]
    public class LocalizationManager : ScriptableObject
    {
        [LabelText("多语言本地化配置添加"),ReadOnly]
        public YDictionary<Language, LocalizationConfigBase> localizationConfig_language_dict;

#if UNITY_EDITOR  
        [UnityEditor.Callbacks.OnOpenAsset(0)]
        private static bool OnOpenAsset(int insId, int line)
        {
           LocalizationManager obj = EditorUtility.InstanceIDToObject(insId) as LocalizationManager;
            if (obj != null)
            {
                LocalizationManagerWindow.ShowWindow();
            }
            return obj != null;
        }
#endif        
        private void OnValidate()
        {   
            onValidate?.Invoke();
        }

        public Action onValidate;
    }
}
