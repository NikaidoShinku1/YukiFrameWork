///=====================================================
/// - FileName:      DrawConfigEditorWindowBase.cs
/// - NameSpace:     YukiFrameWork.Example
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/21 12:49:35
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System;
using YukiFrameWork.Extension;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
namespace YukiFrameWork.DrawEditor
{
    public abstract class DrawConfigEditorWindowBase<T> : OdinMenuEditorWindow where T : UnityEngine.Object
    {
        protected T tBase;
        public static DrawConfigEditorWindowBase<T> Instance { get; internal protected set; }

        private string[] t_guids;       
        protected override OdinMenuTree BuildMenuTree()
        {
            OdinMenuTree odinMenuTree = new OdinMenuTree();
            odinMenuTree.DefaultMenuStyle.SetHeight(30);
            if (!tBase)
                return odinMenuTree;

            Update_ConfigBase(odinMenuTree);
            return odinMenuTree;
        }

        protected bool CheckMenuTreeNullOrEmpty()
        {
            return MenuTree == null || MenuTree.MenuItems == null || MenuTree.MenuItems.Count == 0;
        }
        internal protected T GetCurrentBase()
        {
            if (t_guids == null || t_guids.Length == 0)
                return null;

            if (selectGUID.IsNullOrEmpty())            
                return YukiAssetDataBase.GUIDToInstance<T>(t_guids.FirstOrDefault());          
            else return YukiAssetDataBase.GUIDToInstance<T>(t_guids.FirstOrDefault(x => x == selectGUID));
        }

        internal protected GUIContent GetCurrentBaseContent()
        {
            return GetBaseContent(GetCurrentBase());
        }

        internal protected GUIContent GetBaseContent(T item)
        {        
            if (item == null)
                return GUIContent.none;
            return new GUIContent($"{item.name}_{item.GetInstanceID()}");
        }
        private string selectGUID;
        protected override void DrawMenu()
        {
            EditorGUILayout.BeginVertical("OL Box", GUILayout.Width(MenuWidth), GUILayout.Height(40));

            GUIContent nameContentCurrent = GetCurrentBaseContent();
            if (selectGUID.IsNullOrEmpty() == false && tBase)
                EditorGUILayout.BeginHorizontal();
            if (EditorGUILayout.DropdownButton(nameContentCurrent, FocusType.Passive))
            {
                if (selectGUID.IsNullOrEmpty() && (t_guids == null || t_guids.Length == 0))
                    return;
                GenericMenu selectMenu = new GenericMenu();

                foreach (var guid in t_guids)
                {
                    T item = YukiAssetDataBase.GUIDToInstance<T>(guid);
                    bool isOn = selectGUID == guid;
                    selectMenu.AddItem(GetBaseContent(item), isOn, () =>
                    {
                        selectGUID = guid;

                        EditorGUIUtility.PingObject(item);

                        if (!isOn)
                        {
                            tBase = item;
                            ConfigRefresh();
                        }
                        GUIDRefresh();
                    });

                }
                selectMenu.ShowAsContext();
            }
            if (selectGUID.IsNullOrEmpty() == false && tBase)
            {
                GUIContent content = EditorGUIUtility.ObjectContent(tBase, typeof(ScriptableObject));
                Texture2D icon = content.image as Texture2D;
                EditorGUILayout.LabelField(new GUIContent(icon), GUILayout.Height(20), GUILayout.Width(20));
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.BeginHorizontal();
            MenuTree.DrawSearchToolbar();
            if (GUILayout.Button("刷新", GUILayout.Width(35)))
            {
                GUIDRefresh();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
           
            GUILayout.Space(10);
            base.DrawMenu();
            if (Event.current.mousePosition.x < MenuWidth
                && Event.current.type == EventType.MouseDown
                && Event.current.button == 1)
            {
                GenericMenu menu = new GenericMenu();
                Type[] types = AssemblyHelper.GetTypes(x => (x.IsSubclassOf(ConfigItemBaseType) || x == ConfigItemBaseType) && !x.IsAbstract);
                if (types.Length == 0 || !tBase)
                {
                    menu.AddDisabledItem(DisableItem(), false);
                }
                else
                { 
                    for (int i = 0; i < types.Length; i++)
                    {
                        Type type = types[i];
                        OnCreateItem(type,menu);
                    }
                }

                menu.ShowAsContext();
            }
        }

        protected abstract Type ConfigItemBaseType { get; }

        protected abstract void OnCreateItem(Type type,GenericMenu menu);

        protected abstract GUIContent DisableItem();
       
        protected override void OnEnable()
        {
            base.OnEnable();
            Instance = this;
            
            GUIDRefresh();
            if (Selection.activeObject is T t)
            {
                selectGUID = YukiAssetDataBase.InstanceToGUID(t);
                PlayerPrefs.SetString(SELECT_GUID_KEY, selectGUID);
            }
            else
            {
                selectGUID = PlayerPrefs.GetString(SELECT_GUID_KEY, t_guids.FirstOrDefault());
            }
            tBase = YukiAssetDataBase.GUIDToInstance<T>(selectGUID);
            if(tBase)
            ConfigRefresh();
        }

        private void GUIDRefresh()
        {
            t_guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");          

        }
        protected override void DrawEditors()
        {
            var menuTreeItem = MenuTree.MenuItems.FirstOrDefault(x => x.IsSelected);
            EditorGUILayout.BeginHorizontal("OL Box",GUILayout.Height(15));
            GUI.color = Color.red;
            GUILayout.FlexibleSpace();
            if (menuTreeItem != null && GUILayout.Button("Delete",GUILayout.Width(50)))
            {
                if (EditorUtility.DisplayDialog("配表元素删除", "是否删除这个元素?", "确定", "取消"))
                {
                    OnDelete(menuTreeItem);
                }
            }
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();            
            base.DrawEditors();
        }

        protected abstract void OnDelete(OdinMenuItem item);    

        protected virtual void ConfigRefresh()
        {           
            ForceMenuTreeRebuild();
            Repaint();
        }

        protected abstract void Update_ConfigBase(OdinMenuTree odinMenuTree);              
        protected abstract string SELECT_GUID_KEY { get; }
        protected override void OnDisable()
        {
            base.OnDisable();
            Instance = this;
            PlayerPrefs.SetString(SELECT_GUID_KEY, selectGUID);
        }
    }
}
#endif