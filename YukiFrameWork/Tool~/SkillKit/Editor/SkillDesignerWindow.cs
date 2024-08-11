///=====================================================
/// - FileName:      SkillDesignerWindow.cs
/// - NameSpace:     YukiFrameWork.Skill
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/6/10 18:52:47
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
using YukiFrameWork.Extension;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;

namespace YukiFrameWork.Skill
{
    public class SkillDesignerWindow : OdinMenuEditorWindow
    {
        private const string SELECTCONFIGINDEX_KEY = nameof(SELECTCONFIGINDEX_KEY);
        [MenuItem("YukiFrameWork/SkillDesigneWindow")]
        internal static void OpenWindow()
        {
            GetWindow<SkillDesignerWindow>().titleContent = new GUIContent("Skill数据配置窗口");
        }
        private SkillDataBase[] dataBases;

        private Dictionary<string, OdinEditor> editors = new Dictionary<string, OdinEditor>();     
        protected override OdinMenuTree BuildMenuTree()
        {
            var tree = new OdinMenuTree();
            Init(tree);
            return tree;
        }

        private string[] names = new string[0];

        private void Init()
        {
            OnEnable();
            BuildMenuTree();
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            selectIndex = PlayerPrefs.GetInt(SELECTCONFIGINDEX_KEY, 0);           
            dataBases = AssetDatabase.FindAssets($"t:{typeof(SkillDataBase)}")
                .Select(guid => AssetDatabase.GUIDToAssetPath(guid))
                .Select(AssetDatabase.LoadAssetAtPath<SkillDataBase>)
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
            PlayerPrefs.SetInt(SELECTCONFIGINDEX_KEY, selectIndex);
        }

        private void OnInspectorUpdate()
        {
            Repaint();
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
            if (SkillData == null) return;           

            OdinMenuItem item = MenuTree.MenuItems.Find(x => x.IsSelected);
            if (item == null) return;
            if (item.Value is UnityEngine.Object obj && obj == editor.target)
            {
                GUI.color = Color.red;
                if (GUILayout.Button("移除技能"))
                {
                    SkillData.DeleteSkillData(MenuTree.MenuItems.FirstOrDefault(x => x.IsSelected).FlatTreeIndex);
                    isDelete = true;
                }
                GUI.color = Color.white;
                editor.DrawDefaultInspector(); 
            }
        }

    
        private void Init(OdinMenuTree MenuTree)
        {            
            if (MenuTree != null && SkillData != null)
            {
                MenuTree.MenuItems.Clear();
                editors.Clear();
                foreach (var item in SkillData.SkillDataConfigs)
                {
                    MenuTree.Add("Skill信息:" + item.GetSkillKey, item, SdfIconType.Wallet);
                    editors[$"{item.GetSkillKey}_{item.GetInstanceID()}"] = (OdinEditor)Editor.CreateEditor(item, typeof(OdinEditor));
                }
                if (MenuTree.MenuItems.Count > 0)
                {
                    //MenuTree.MenuItems.FirstOrDefault().Select();
                    MenuTree.MarkDirty();
                    //MenuTree.MarkLayoutChanged();
                    MenuTree.UpdateMenuTree();
                }
            }
        }

        private bool isDelete;
        protected override void OnImGUI()
        {
            base.OnImGUI();
            if (skillData == null) return;            
            for (int i = 0; i < skillData.SkillDataConfigs.Count; i++)
            {
                var item = skillData.SkillDataConfigs[i];
                item.names.Clear();

                for (int j = 0; j < skillData.SkillDataConfigs.Count; j++)
                {
                    if (skillData.SkillDataConfigs[j].GetSkillName == item.GetSkillName) continue;
                    item.names.Add(skillData.SkillDataConfigs[j].GetSkillName, skillData.SkillDataConfigs[j].GetSkillKey);
                }

            }
        }
        private SkillDataBase skillData;
        private SkillDataBase SkillData
        {
            get => skillData;
            set
            {
                if (value == null || value == skillData) return;
                skillData = value;

                Init(MenuTree);
            }
        }
        private int selectIndex;
        protected override void DrawMenu()
        {
            EditorGUILayout.BeginHorizontal();
            MenuTree?.DrawSearchToolbar();
            if (GUILayout.Button("刷新编辑器", GUILayout.Width(80)))
            {
                Init();
            }
            EditorGUILayout.EndHorizontal();
            selectIndex = EditorGUILayout.Popup(selectIndex, names);

            if (dataBases == null || dataBases.Length == 0)
            {
                EditorGUILayout.HelpBox("没有任何技能配置，请在Assets文件夹下右键Create/YukiFrameWork/BuffDataBase添加配置或点击下面按钮新建!", MessageType.Warning);

                if (GUILayout.Button("新建技能配置", GUILayout.Height(40)))
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
                    LogKit.I(path + @"/" + nameof(SkillDataBase) + ".asset");
                    AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<SkillDataBase>(), targetPath + @"/" + nameof(SkillDataBase) + ".asset");
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
                        SkillData = dataBases.FirstOrDefault(x => name == $"{x.name}_{x.GetInstanceID()}");
                    }
                }
                catch { }
            }
            if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && SkillData != null)
            {
                GenericMenu genericMenu = new GenericMenu();

                var types = AssemblyHelper.GetTypes(x => x.IsSubclassOf(typeof(SkillData)));

                for (int i = 0; i < types.Length; i++)
                {
                    int index = i;
                    genericMenu.AddItem(new GUIContent("创建Skill/" + types[i]), false, () =>
                    {
                        SkillData.CreateSkillData(types[index]);
                        Repaint();
                    });
                }

                genericMenu.ShowAsContext();
            }
            base.DrawMenu();
        }

    
    }
}
#endif