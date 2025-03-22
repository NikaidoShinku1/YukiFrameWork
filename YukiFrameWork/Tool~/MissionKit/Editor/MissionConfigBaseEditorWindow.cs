///=====================================================
/// - FileName:      MissionConfigBaseEditorWindow.cs
/// - NameSpace:     YukiFrameWork.Example
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/22 20:19:14
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
#if UNITY_EDITOR
using YukiFrameWork.DrawEditor;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
namespace YukiFrameWork.Missions
{
    public class MissionConfigBaseEditorWindow : DrawConfigEditorWindowBase<MissionConfigManager>
    {
        protected override Type ConfigItemBaseType => typeof(MissionConfigBase);

        protected override string SELECT_GUID_KEY => "MISSIONCONFIGBASE_SELECT_EDITOR_KEY";

        protected override GUIContent DisableItem()
        {
            return new GUIContent("添加新的任务配置集合");
        }

        protected override void OnCreateItem(Type type, GenericMenu menu)
        {
            menu.AddItem(new GUIContent($"添加新的任务配置集合/{type.FullName}"), false, () =>
            {
                MissionConfigBase missionConfigBase = ScriptableObject.CreateInstance(type) as MissionConfigBase;

                AssetDatabase.AddObjectToAsset(missionConfigBase, tBase);
                missionConfigBase.name = $"{type.Name}_{missionConfigBase.GetInstanceID()}";
                tBase.missionConfigBases.Add(missionConfigBase);
                tBase.onValidate?.Invoke();
                tBase.Save();
                AssetDatabase.Refresh();
            });
        }
        protected override void ConfigRefresh()
        {
            tBase.onValidate = () =>
            {
                if (Instance)
                    Instance.ForceMenuTreeRebuild();
            };
            base.ConfigRefresh();
        }
        protected override void OnDelete(OdinMenuItem item)
        {
            MissionConfigBase missionConfigBase = item.Value as MissionConfigBase;

            if (!missionConfigBase) return;

            tBase.missionConfigBases.Remove(missionConfigBase);
            AssetDatabase.RemoveObjectFromAsset(missionConfigBase);
            tBase.onValidate?.Invoke();
            tBase.Save();
            AssetDatabase.Refresh();
        }
        protected override void OnImGUI()
        {
            base.OnImGUI();

            if (CheckMenuTreeNullOrEmpty()) return;

            foreach (var item in MenuTree.MenuItems)
            {
                MissionConfigBase missionConfigBase = item.Value as MissionConfigBase;
                if (!missionConfigBase) continue;

                item.Name = missionConfigBase.displayName.IsNullOrEmpty() ? $"{missionConfigBase.name}" : missionConfigBase.displayName;
            }
        }
        protected override void Update_ConfigBase(OdinMenuTree odinMenuTree)
        {
            for (int i = tBase.missionConfigBases.Count - 1; i >= 0; i--)
            {
                if (!tBase.missionConfigBases[i])
                {
                    tBase.missionConfigBases.RemoveAt(i);
                    tBase.onValidate?.Invoke();
                    tBase.Save();
                    AssetDatabase.Refresh();
                }
            }

            foreach (var item in tBase.missionConfigBases)
            {
                odinMenuTree.Add($"{item.name}_{item.GetInstanceID()}", item, Sirenix.OdinInspector.SdfIconType.ClipboardData);
            }
        }

        [MenuItem("YukiFrameWork/MissionKit任务配置窗口", false, -7)]
        internal static void ShowWindow()
        {
            var editorWindow = GetWindow<MissionConfigBaseEditorWindow>();

            editorWindow.titleContent = new GUIContent("Yuki-MissionKit配置窗口");

            editorWindow.Show();
        }
    }
}
#endif