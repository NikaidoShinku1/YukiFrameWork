///=====================================================
/// - FileName:      LocalizationManagerWindow.cs
/// - NameSpace:     YukiFrameWork.Example
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/19 18:55:58
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using System.Collections.Generic;
using System.Linq;
using YukiFrameWork.Extension;


#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using YukiFrameWork.DrawEditor;
using UnityEngine;
using System;

namespace YukiFrameWork
{
    public class LocalizationManagerWindow : DrawConfigEditorWindowBase<LocalizationManager>
    {
        protected override string SELECT_GUID_KEY => "LOCALIZATIONMANAGER_EDITOR_SELECT_KEY";

        protected override Type ConfigItemBaseType => typeof(LocalizationConfigBase);

        protected override void ConfigRefresh()
        {          
            tBase.onValidate = () =>
            {    
                if(Instance)
                    Instance.ForceMenuTreeRebuild();
            };
            base.ConfigRefresh();
        }

        protected override void Update_ConfigBase(OdinMenuTree odinMenuTree)
        {
            for (Language i = Language.SimplifiedChinese; i <= Language.Vietnamese; i++)
            {
                if (!tBase.localizationConfig_language_dict.ContainsKey(i)) continue;

                if (tBase.localizationConfig_language_dict[i]) continue;
                             
                tBase.localizationConfig_language_dict.Remove(i);
                tBase.onValidate?.Invoke();
                tBase.Save();
                AssetDatabase.Refresh();
            }
            foreach (var item in tBase.localizationConfig_language_dict)
            {
                if (!item.Value) continue;
                odinMenuTree.Add(item.Key.ToString(), item.Value,Sirenix.OdinInspector.SdfIconType.ClipboardData);
            }                       
        }

        [MenuItem("YukiFrameWork/本地化配置窗口",false,-10)]
        internal static void ShowWindow()
        {
            var editorWindow = GetWindow<LocalizationManagerWindow>();

            editorWindow.titleContent = new GUIContent("Yuki-本地化配置窗口");
            
            editorWindow.Show();
        }         
        protected override void OnCreateItem(Type type,GenericMenu languageMenu)
        {
            for (Language language = Language.SimplifiedChinese; language <= Language.Vietnamese; language++)
            {
                Language temp = language;

                languageMenu.AddItem(new GUIContent($"添加新的语言配置/{type}/{language}"), false, () =>
                {
                    LocalizationConfigBase localizationConfig = ScriptableObject.CreateInstance(type) as LocalizationConfigBase;

                    AssetDatabase.AddObjectToAsset(localizationConfig, tBase);
                    localizationConfig.name = temp.ToString();
                    tBase.localizationConfig_language_dict.Add(temp, localizationConfig);
                    tBase.onValidate?.Invoke();
                    tBase.Save();
                    AssetDatabase.Refresh();

                });
            }
        }

        protected override GUIContent DisableItem()
        {
            return new GUIContent("添加新的语言配置");
        }

        protected override void OnDelete(OdinMenuItem menuTreeItem)
        {
            var localizationConfig = menuTreeItem.Value as LocalizationConfigBase;
            if (!localizationConfig) return;
            Language language = Language.SimplifiedChinese;
            bool over = false;
            foreach (var item in tBase.localizationConfig_language_dict)
            {
                if (item.Value == localizationConfig)
                {
                    language = item.Key;
                    over = true;
                    break;
                }
            }
            if (over)
            {
                tBase.localizationConfig_language_dict.Remove(language);
                AssetDatabase.RemoveObjectFromAsset(localizationConfig);
                tBase.onValidate?.Invoke();
                tBase.Save();
                AssetDatabase.Refresh();
            }
        }    
    }
}
#endif