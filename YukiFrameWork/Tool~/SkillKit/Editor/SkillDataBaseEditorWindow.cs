///=====================================================
/// - FileName:      SkillDataBaseEditorWindow.cs
/// - NameSpace:     YukiFrameWork.Example
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/21 18:24:34
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using YukiFrameWork.DrawEditor;
namespace YukiFrameWork.Skill
{
    public class SkillDataBaseEditorWindow : DrawConfigEditorWindowBase<SkillDataBase>
    {
        protected override Type ConfigItemBaseType => typeof(SkillData);

        protected override string SELECT_GUID_KEY => "SKILLDATABASE_EDITOR_SELECT_KEY";

        protected override GUIContent DisableItem()
        {
            return new GUIContent("添加新的Skill配置");
        }

        protected override void OnCreateItem(Type type, GenericMenu menu)
        {
            menu.AddItem(new GUIContent($"添加新的Skill配置/{type}"), false, () =>
            {
                tBase.CreateSkillData(type);
                tBase.onValidate?.Invoke();
                AssetDatabase.Refresh();
            });
        }

        protected override void OnDelete(OdinMenuItem item)
        {
            SkillData skillData = item.Value as SkillData;
            if (skillData)
            {
                tBase.DeleteSkillData(skillData);
                tBase.onValidate?.Invoke();
                AssetDatabase.Refresh();
         
            }
        }

        protected override void ConfigRefresh()
        {
            tBase.onValidate = () =>
            {
                if(Instance)
                    Instance.ForceMenuTreeRebuild();
            };
            base.ConfigRefresh();
        }

        protected override void OnImGUI()
        {
            base.OnImGUI();
            if (CheckMenuTreeNullOrEmpty()) return;


            foreach (var item in MenuTree.MenuItems)
            {
                SkillData skillData = item.Value as SkillData;
                if (!skillData) continue;

                item.Name = $"{skillData.SkillKey}_{skillData.GetInstanceID()}";

            }
        }   

        protected override void Update_ConfigBase(OdinMenuTree odinMenuTree)
        {
            if (tBase.SkillDataConfigs.Count > 0)
            {
                for (int i = tBase.SkillDataConfigs.Count - 1; i >= 0; i--)
                {
                    if (tBase.SkillDataConfigs[i]) continue;
                    tBase.DeleteSkillData(i);
                }
            }
            foreach (var skill in tBase.SkillDataConfigs)
            {
                if (!skill) continue;
                odinMenuTree.Add($"{skill.SkillKey}_{skill.GetInstanceID()}",skill, Sirenix.OdinInspector.SdfIconType.ClipboardData);
            }          
        }


        [MenuItem("YukiFrameWork/Skill配置窗口", false, -8)]
        internal static void ShowWindow()
        {
            var editorWindow = GetWindow<SkillDataBaseEditorWindow>();

            editorWindow.titleContent = new GUIContent("Yuki-Skill配置窗口");

            editorWindow.Show();
        }
    }
}
#endif