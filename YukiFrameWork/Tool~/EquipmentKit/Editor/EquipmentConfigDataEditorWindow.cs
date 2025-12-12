///=====================================================
/// - FileName:      EquipmentConfigDataEditorWindow.cs
/// - NameSpace:     RPG
/// - Description:   高级定制脚本生成
/// - Creation Time: 12/12/2025 12:56:06 PM
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using Sirenix.OdinInspector;
using System.Collections.Generic;


#if UNITY_EDITOR
using YukiFrameWork.DrawEditor;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using UnityEngine;
using System;
namespace YukiFrameWork.Equips
{
    public class EquipmentConfigDataEditorWindow : DrawConfigEditorWindowBase<EquipmentConfigDataManager>
    {

        protected override Type ConfigItemBaseType => typeof(EquipmentConfigBase);

        protected override string SELECT_GUID_KEY => "EQUIPMENTCONFIGBASE_SELECT_EDITOR_KEY";

        protected override GUIContent DisableItem()
        {
            return new GUIContent("添加新的装备置集合");
        }

        protected override void OnCreateItem(Type type, GenericMenu menu)
        {
            menu.AddItem(new GUIContent($"添加新的装备配置集合/{type.FullName}"), false, () =>
            {
                EquipmentConfigBase equipConfigBase = ScriptableObject.CreateInstance(type) as EquipmentConfigBase;

                AssetDatabase.AddObjectToAsset(equipConfigBase, tBase);
                equipConfigBase.name = $"{type.Name}_{equipConfigBase.GetInstanceID()}";
                tBase.equipmentConfigBases.Add(equipConfigBase);
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
            EquipmentConfigBase equipConfigBase = item.Value as EquipmentConfigBase;

            if (!equipConfigBase) return;

            tBase.equipmentConfigBases.Remove(equipConfigBase);
            AssetDatabase.RemoveObjectFromAsset(equipConfigBase);
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
                EquipmentConfigBase equipConfigBase = item.Value as EquipmentConfigBase;
                if (!equipConfigBase) continue;

                item.Name = equipConfigBase.displayName.IsNullOrEmpty() ? $"{equipConfigBase.name}" : equipConfigBase.displayName;
            }
        }
        protected override void Update_ConfigBase(OdinMenuTree odinMenuTree)
        {
            for (int i = tBase.equipmentConfigBases.Count - 1; i >= 0; i--)
            {
                if (!tBase.equipmentConfigBases[i])
                {
                    tBase.equipmentConfigBases.RemoveAt(i);
                    tBase.onValidate?.Invoke();
                    tBase.Save();
                    AssetDatabase.Refresh();
                }
            }

            foreach (var item in tBase.equipmentConfigBases)
            {
                odinMenuTree.Add($"{item.name}_{item.GetInstanceID()}", item, Sirenix.OdinInspector.SdfIconType.ClipboardData);
            }
        }

        [MenuItem("YukiFrameWork/EquipmentKit任务配置窗口", false, -7)]
        internal static void ShowWindow()
        {
            var editorWindow = GetWindow<EquipmentConfigDataEditorWindow>();

            editorWindow.titleContent = new GUIContent("Yuki-EquipmentKit配置窗口");

            editorWindow.Show();
        }
    }
}
#endif