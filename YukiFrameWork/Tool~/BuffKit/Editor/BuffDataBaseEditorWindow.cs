///=====================================================
/// - FileName:      BuffDataBaseEditorWindow.cs
/// - NameSpace:     YukiFrameWork.Example
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/20 19:03:28
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using YukiFrameWork.Extension;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using YukiFrameWork.DrawEditor;
using Sirenix.OdinInspector.Editor;
namespace YukiFrameWork.Buffer
{
    public class BuffDataBaseEditorWindow : DrawConfigEditorWindowBase<BuffDataBase>
    {
        protected override string SELECT_GUID_KEY => "BUFFDATABASE_EDITOR_SELECT_KEY";

        protected override Type ConfigItemBaseType => typeof(Buff);

        [MenuItem("YukiFrameWork/Buff配置窗口",false,-9)]
        internal static void ShowWindow()
        {
            var editorWindow = GetWindow<BuffDataBaseEditorWindow>();

            editorWindow.titleContent = new GUIContent("Yuki-Buff配置窗口");

            editorWindow.Show();
        }

        protected override GUIContent DisableItem()
        {
            return new GUIContent("添加新的Buff配置");
        }

        protected override void OnCreateItem(Type type, GenericMenu menu)
        {
            menu.AddItem(new GUIContent($"添加新的Buff配置/{type}"),false,() => 
            {              
                tBase.CreateBuff(type);
                tBase.onValidate?.Invoke();
                AssetDatabase.Refresh();
            });
        }

        protected override void OnDelete(OdinMenuItem item)
        {
            Buff buff = item.Value as Buff;
            if (buff)
            {
                tBase.DeleteBuff(buff);
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

            if (CheckMenuTreeNullOrEmpty())
                return;

            foreach (var item in MenuTree.MenuItems)
            {
                Buff buff = item.Value as Buff;
                if (!buff) continue;

                item.Name = $"{buff.GetBuffKey}_{buff.GetInstanceID()}";
                
            }
        }

        protected override void Update_ConfigBase(OdinMenuTree odinMenuTree)
        {
            if (tBase.buffConfigs.Count > 0)
            {
                for (int i = tBase.buffConfigs.Count - 1; i >= 0; i--)
                {
                    if (tBase.buffConfigs[i]) continue;
                    tBase.DeleteBuff(i);
                }
            }
            foreach (var item in tBase.buffConfigs)
            {
                if (!item) continue;
                string name = $"{item.GetBuffKey}_{item.GetInstanceID()}";
                odinMenuTree.Add(name, item,Sirenix.OdinInspector.SdfIconType.ClipboardData);
            }
           
        }
    }
}
#endif