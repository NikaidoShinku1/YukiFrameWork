///=====================================================
/// - FileName:      BuffDesignerWindow.cs
/// - NameSpace:     YukiFrameWork.Buffer
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/5/31 14:15:29
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using Object = UnityEngine.Object;
using YukiFrameWork.Extension;
using System.Linq;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
namespace YukiFrameWork.Buffer
{
    public class BuffDesignerWindow : Sirenix.OdinInspector.Editor.OdinMenuEditorWindow
    {
        [UnityEditor.MenuItem("YukiFrameWork/BuffDesignWindow")]
        public static void OpenWindow()
        {
            GetWindow<BuffDesignerWindow>().titleContent = new GUIContent("Buff数据配置窗口");
        }

        public static void OpenWindow(BuffDataBase buffDataBase)
        {
            var window = GetWindow<BuffDesignerWindow>();
            window.titleContent = new GUIContent("Buff数据配置窗口");
            window.BuffDataBase = buffDataBase;
        }
        private BuffDataBase buffDataBase;
        private BuffDataBase BuffDataBase
        {
            get => buffDataBase;
            set
            {
                if (value == null && MenuTree != null)
                {
                    MenuTree.MenuItems.Clear();
                    return;
                }

                if (value == null || value == buffDataBase) return;
                buffDataBase = value;

                Init(MenuTree);
                EnsureEditorsAreReady();
            }
        }
        private bool isDelete;
        private void Init(OdinMenuTree MenuTree)
        {
            if (MenuTree != null && buffDataBase != null)
            {
                MenuTree.MenuItems.Clear();

                foreach (var item in buffDataBase.buffConfigs)
                {
                    MenuTree.Add("Buff信息:" + item.GetBuffKey, item, SdfIconType.Wallet);
                }

                if (MenuTree.MenuItems.Count > 0)
                {
                    MenuTree.MenuItems.FirstOrDefault().Select();
                    MenuTree.MarkLayoutChanged();                 
                }
            }
        }

        protected override void DrawEditor(int index)
        {
            GUI.color = Color.red;
            if (GUILayout.Button("移除Buff"))
            {
                BuffDataBase.DeleteBuff(MenuTree.MenuItems.FirstOrDefault(x => x.IsSelected).FlatTreeIndex);
                isDelete = true;
            }
            GUI.color = Color.white;
            base.DrawEditor(index);
        }

        protected override void OnImGUI()
        {
            base.OnImGUI();
            if (buffDataBase == null) return;
            for (int i = 0; i < MenuTree.MenuItems.Count; i++)
            {
                var item = MenuTree.MenuItems[i];
                if (i >= MenuTree.MenuItems.Count || isDelete) continue;
                item.Name = "Buff信息:" + buffDataBase.buffConfigs[i].GetBuffKey;           
            }

            for (int i = 0; i < buffDataBase.buffConfigs.Count; i++)
            {
                var item = buffDataBase.buffConfigs[i];
                item.names.Clear();

                for (int j = 0; j < buffDataBase.buffConfigs.Count; j++)
                {
                    if (buffDataBase.buffConfigs[j].GetBuffName == item.GetBuffName) continue;
                    item.names.Add(buffDataBase.buffConfigs[j].GetBuffName, buffDataBase.buffConfigs[j].GetBuffKey);
                }

            }
        }

        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree();
            Init(tree);
            return tree;
        }

        protected override void DrawMenu()
        {
            try
            {
                var rect = EditorGUILayout.BeginHorizontal();

                BuffDataBase = (BuffDataBase)EditorGUILayout.ObjectField(GUIContent.none, BuffDataBase, typeof(BuffDataBase), true);

                EditorGUILayout.EndHorizontal();

                if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
                {
                    GenericMenu genericMenu = new GenericMenu();

                    var types = AssemblyHelper.GetTypes(x => x.IsSubclassOf(typeof(Buff)));

                    for (int i = 0; i < types.Length; i++)
                    {
                        int index = i;
                        genericMenu.AddItem(new GUIContent("创建Skill/" + types[i]), false, () =>
                        {
                            BuffDataBase.CreateBuff(types[index]);
                            Repaint();
                        });
                    }

                    genericMenu.ShowAsContext();
                }
            }
            catch { }
            base.DrawMenu();
        }
    } 
    
}
#endif
