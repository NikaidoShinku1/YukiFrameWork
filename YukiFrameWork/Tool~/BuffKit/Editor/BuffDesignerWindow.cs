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
using System.Collections.Generic;


#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
namespace YukiFrameWork.Buffer
{
    public class BuffDesignerWindow : Sirenix.OdinInspector.Editor.OdinMenuEditorWindow
    {
        private const string SELECTBUFFCONFIGINDEX_KEY = nameof(SELECTBUFFCONFIGINDEX_KEY);
        [UnityEditor.MenuItem("YukiFrameWork/BuffDesignWindow")]
        public static void OpenWindow()
        {
            GetWindow<BuffDesignerWindow>().titleContent = new GUIContent("Buff数据配置窗口");
        }

        private BuffDataBase[] dataBases;

        private Dictionary<string, OdinEditor> editors = new Dictionary<string, OdinEditor>();
   
        private BuffDataBase buffDataBase;
        private BuffDataBase BuffDataBase
        {
            get => buffDataBase;
            set
            {
                if (value == null || value == buffDataBase) return;
                buffDataBase = value;
                Init(MenuTree);
            }
        }
        private int selectIndex;
        private string[] names = new string[0];
        protected override void OnEnable()
        {
            base.OnEnable();
            names = new string[0];
            selectIndex = PlayerPrefs.GetInt(SELECTBUFFCONFIGINDEX_KEY, 0);
            dataBases = AssetDatabase.FindAssets($"t:{typeof(BuffDataBase)}")
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(AssetDatabase.LoadAssetAtPath<BuffDataBase>)
                .Where(x => x != null)
                .ToArray();

            if (dataBases == null || selectIndex >= dataBases.Length)
                selectIndex = 0;
            if (dataBases == null || dataBases.Length == 0) return;

            names = dataBases.Select(x => $"{x.name}_{x.GetInstanceID()}").ToArray();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            PlayerPrefs.SetInt(SELECTBUFFCONFIGINDEX_KEY, selectIndex);
        }
        private bool isDelete;
        private void Init(OdinMenuTree MenuTree)
        {
            if (MenuTree != null && BuffDataBase != null)
            {
                MenuTree.MenuItems.Clear();
                editors.Clear();
                foreach (var item in buffDataBase.buffConfigs)
                {
                    MenuTree.Add("Buff信息:" + item.GetBuffKey, item, SdfIconType.Wallet);
                    editors[$"{item.GetBuffKey}_{item.GetInstanceID()}"] = (OdinEditor)Editor.CreateEditor(item, typeof(OdinEditor));
                }
                if (MenuTree.MenuItems.Count > 0)
                {
                    //MenuTree.MenuItems.FirstOrDefault().Select();
                    MenuTree.MarkDirty();
                    MenuTree.MarkLayoutChanged();
                    MenuTree.UpdateMenuTree();
                }
            }
        }

        protected override void DrawEditors()
        {
            foreach (var item in editors.Values)
            {
                OnInspectorGUI(item);
            }
        }

        private void OnInspectorGUI(OdinEditor editor)
        {
            if (BuffDataBase == null) return;

            OdinMenuItem item = MenuTree.MenuItems.Find(x => x.IsSelected);
            if (item == null) return;
            if (item.Value is UnityEngine.Object obj && obj == editor.target)
            {
                GUI.color = Color.red;
                if (GUILayout.Button("移除Buff"))
                {
                    BuffDataBase.DeleteBuff(MenuTree.MenuItems.FirstOrDefault(x => x.IsSelected).FlatTreeIndex);
                    isDelete = true;
                }
                GUI.color = Color.white;
                editor.DrawDefaultInspector();
            }
        }

        private void OnInspectorUpdate()
        {
            Repaint();
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

        private void Init()
        {
            OnEnable();
            BuildMenuTree();
        }

        protected override void DrawMenu()
        {
            try
            {
                EditorGUILayout.BeginHorizontal();
                MenuTree?.DrawSearchToolbar();
                if (GUILayout.Button("刷新编辑器",GUILayout.Width(80)))
                {
                    Init();
                }
                EditorGUILayout.EndHorizontal();
                selectIndex = EditorGUILayout.Popup(selectIndex, names);
                if (dataBases == null || dataBases.Length == 0)
                {
                    EditorGUILayout.HelpBox("没有任何Buff配置，请在Assets文件夹下右键Create/YukiFrameWork/BuffDataBase添加配置或点击下面按钮新建!", MessageType.Warning);

                    if (GUILayout.Button("新建Buff配置",GUILayout.Height(40)))
                    {
                        string path = EditorUtility.OpenFolderPanel("选择配置路径", "Assets", "Assets");                      

                        if (path.IsNullOrEmpty()) return;

                        if (!path.Contains("Assets"))
                        {
                            LogKit.E("必须在Assets文件夹下!");
                            return;
                        }
                        bool append = false;

                        string[] values = path.Split('/');
                        string targetPath = string.Empty;
                        for (int i = 0; i < values.Length; i++)
                        {
                            if (values[i].Contains("Assets") || values[i] == "Assets")
                            {
                                append = true;
                            }
                            if (append)
                            {
                                if (i < values.Length - 1)
                                    targetPath += values[i] + "/";
                                else
                                    targetPath += values[i];
                            }
                        }
                        LogKit.I(path + @"/" + nameof(BuffDataBase) + ".asset");
                        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<BuffDataBase>(), targetPath + @"/" + nameof(BuffDataBase) + ".asset");
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        Init();
                    }

                    return;
                }


                if (names != null || names.Length > 0)
                {
                    try
                    {
                        string name = names[selectIndex];

                        if (!name.IsNullOrEmpty())
                        {
                            BuffDataBase = dataBases.FirstOrDefault(x => name == $"{x.name}_{x.GetInstanceID()}");
                        }
                    }
                    catch { }
                }
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
