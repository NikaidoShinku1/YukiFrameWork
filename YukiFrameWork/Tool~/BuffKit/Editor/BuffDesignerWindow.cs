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
        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree();
            System.Type buffType = typeof(BuffDataBase);

            if (buffType != null)
            {
                Object[] objects = Resources.FindObjectsOfTypeAll(buffType);
                for (int i = 0; i < objects.Length; i++)
                {
                    tree.Add($"BuffKit配置窗口集合/{objects[i].name}_{objects[i].GetInstanceID()}", BuffDataWindow.Create(objects[i] as BuffDataBase), SdfIconType.BookmarkStar);
                }
            }
            return tree;
        }

        protected override void DrawMenu()
        {
            try
            {
                if (MenuTree != null)
                {
                    MenuTree?.DrawSearchToolbar();
                }
            }
            catch { }
            base.DrawMenu();
        }
    }

    public class BuffDataWindow : Sirenix.OdinInspector.Editor.OdinMenuEditorWindow
    {
        private BuffDataBase buffData;
        public static BuffDataWindow Create(BuffDataBase dataBase)
        {
            BuffDataWindow window = OdinMenuEditorWindow.CreateInstance<BuffDataWindow>();          
            window.buffData = dataBase;
            return window;
        }
        internal string[] bufDataNames => buffData.buffConfigs.Select(x => x.GetBuffName).ToArray();
        protected override OdinMenuTree BuildMenuTree()
        {
            OdinMenuTree odinMenuTree = new OdinMenuTree();          
            for (int i = 0; i < buffData.buffConfigs.Count; i++)
            {
                odinMenuTree.Add("Buff信息" + i + buffData.buffConfigs[i].GetBuffKey, buffData.buffConfigs[i], SdfIconType.Wallet);
            }           
            return odinMenuTree;
        }              
        private void OnInspectorUpdate()
        {
            Repaint();
            ForceMenuTreeRebuild();           
        }

        protected override void DrawMenu()
        {
            try
            {
                if (MenuTree != null)
                {
                    MenuTree?.DrawSearchToolbar();
                }
            }
            catch { }
            base.DrawMenu();
        }     
        [OnInspectorGUI]
        protected override void OnImGUI()
        {          
            base.OnImGUI();
            for(int i = 0;i < MenuTree.MenuItems.Count;i++)
            {
                var item = MenuTree.MenuItems[i];
                item.Name ="Buff信息:" + buffData.buffConfigs[i].GetBuffKey;
            }

            for (int i = 0; i < buffData.buffConfigs.Count; i++)
            {
                var item = buffData.buffConfigs[i];
                item.names.Clear();

                for (int j = 0; j < bufDataNames.Length; j++)
                {
                    if (bufDataNames[j] == buffData.buffConfigs[i].GetBuffName) continue;
                    item.names.Add(bufDataNames[j], buffData.buffConfigs[j].GetBuffKey);
                }
                
            }
            if (Event.current.type == EventType.MouseDown && Event.current.button == 1)
            {              
                GenericMenu genericMenu = new GenericMenu();

                var types = AssemblyHelper.GetTypes(x => x.IsSubclassOf(typeof(Buff)));

                for (int i = 0; i < types.Length; i++)
                {
                    int index = i;
                    genericMenu.AddItem(new GUIContent("创建Buff/" + types[i]),false, () => 
                    {
                        buffData.CreateBuff(types[index]);
                        Repaint();
                    });
                }

                genericMenu.ShowAsContext();
            }
        }
    }
}
#endif
