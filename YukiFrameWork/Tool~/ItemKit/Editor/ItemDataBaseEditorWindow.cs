///=====================================================
/// - FileName:      ItemDesignerWindow.cs
/// - NameSpace:     YukiFrameWork.Item
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/5/31 17:14:08
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using YukiFrameWork.DrawEditor;
namespace YukiFrameWork.Item
{
    public class ItemDataBaseEditorWindow : DrawConfigEditorWindowBase<ItemDataManager>
    {
        protected override Type ConfigItemBaseType => typeof(ItemDataBase);

        protected override string SELECT_GUID_KEY => "ITEMDATABASEMANAGER_SELECT_EDITOR_KEY";

        protected override GUIContent DisableItem()
        {
            return new GUIContent("添加新的物品配置集合");
        }

        protected override void OnCreateItem(Type type, GenericMenu menu)
        {
            menu.AddItem(new GUIContent($"添加新的物品配置集合/{type.FullName}"), false, () => 
            {
                ItemDataBase itemDataBase = ScriptableObject.CreateInstance(type) as ItemDataBase;

                AssetDatabase.AddObjectToAsset(itemDataBase,tBase);
                itemDataBase.name = $"{type.Name}_{itemDataBase.GetInstanceID()}";
                tBase.itemDataBases.Add(itemDataBase);
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
        [MenuItem("YukiFrameWork/ItemKit配置窗口", false, -8)]
        internal static void ShowWindow()
        {
            var editorWindow = GetWindow<ItemDataBaseEditorWindow>();

            editorWindow.titleContent = new GUIContent("Yuki-ItemKit配置窗口");

            editorWindow.Show();
        }
        protected override void OnDelete(OdinMenuItem item)
        {
            ItemDataBase itemDataBase = item.Value as ItemDataBase;

            if (!itemDataBase) return;

            tBase.itemDataBases.Remove(itemDataBase);
            AssetDatabase.RemoveObjectFromAsset(itemDataBase);
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
                ItemDataBase itemDataBase = item.Value as ItemDataBase;
                if (!itemDataBase) continue;

                item.Name = itemDataBase.displayName.IsNullOrEmpty() ? $"{itemDataBase.name}" : itemDataBase.displayName;
            }
        }
        protected override void Update_ConfigBase(OdinMenuTree odinMenuTree)
        {
            for (int i = tBase.itemDataBases.Count - 1; i >= 0; i--)
            {
                if (!tBase.itemDataBases[i])
                {
                    tBase.itemDataBases.RemoveAt(i);
                    tBase.onValidate?.Invoke();
                    tBase.Save();
                    AssetDatabase.Refresh();
                }
            }

            foreach (var item in tBase.itemDataBases)
            {
                odinMenuTree.Add($"{item.name}_{item.GetInstanceID()}", item,Sirenix.OdinInspector.SdfIconType.ClipboardData);
            }
        }
    }
}
#endif