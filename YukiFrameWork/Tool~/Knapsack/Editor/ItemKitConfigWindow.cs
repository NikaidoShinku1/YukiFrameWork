
#if UNITY_EDITOR
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using YukiFrameWork.Extension;

namespace YukiFrameWork.Knapsack
{
    public class ItemKitConfigWindow : EditorWindow
    {
        private ItemConfigData configData;

        private const string genericPath = "Assets/Resources";

        private List<ItemData> itemDatas = new List<ItemData>();
        [MenuItem("YukiFrameWork/ItemConfig",false,2)]
        public static void OpenWindow()
        {
            var window = GetWindow<ItemKitConfigWindow>();
            window.Show();
            window.titleContent = new GUIContent("背包配置窗口");
        }

        private void OnEnable()
        {
            configData = Resources.Load<ItemConfigData>("ItemConfigData");         
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();                     
            configData = EditorGUILayout.ObjectField(configData, typeof(ItemConfigData), true) as ItemConfigData;
            EditorGUILayout.EndHorizontal();
            if(configData == null)
                GenericConfig();
            else
                ConfigGUI();           
        }

        private void ConfigGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("请选择加载配置文件的方式:",GUILayout.Width(150));
            configData.LoadType = (LoadType)EditorGUILayout.EnumPopup(configData.LoadType);          
            EditorGUILayout.EndHorizontal();           
            switch (configData.LoadType)
            {
                case LoadType.ABManager:
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("请输入模块名称:",GUILayout.Width(150));
                    configData.ProjectName = EditorGUILayout.TextField(configData.ProjectName);
                    EditorGUILayout.EndHorizontal();
                    break;
                case LoadType.Resources:
                    configData.ProjectName = string.Empty;
                    break;
            }
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(configData.LoadType == LoadType.Resources ? "请输入配置文件的路径:" : "请输入配置文件的名称:", GUILayout.Width(150));
            configData.ProjectPath = EditorGUILayout.TextField(configData.ProjectPath);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            GUILayout.Label("物品的UI预制体:\n(可在Assets文件夹下右键创建一键式面板,从中单独拿出ItemUI作为预制体保存):", GUILayout.Width(position.width));
            configData.HandItemPrefab = EditorGUILayout.ObjectField(configData.HandItemPrefab, typeof(ItemUI), true) as ItemUI;

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("打开ItemData物品信息类(可以自由的对这个类进行修改拓展)",GUILayout.Height(25)))
            {
                string info = AssetDatabase.LoadAssetAtPath<TextAsset>(ImportSettingWindow.importPath).text;                
                ImportSettingWindow.Data data = AssemblyHelper.DeserializeObject<ImportSettingWindow.Data>(info);
                MonoScript mono = AssetDatabase.LoadAssetAtPath<MonoScript>(data.path + "/Knapsack/Core/ItemData.cs");
                AssetDatabase.OpenAsset(mono);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            string example = AssemblyHelper.SerializedObject(itemDatas);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("配置文件示例:");
            if (GUILayout.Button("Copy"))
            {
                TextEditor text = new TextEditor();
                text.text = example;
                text.OnFocus();
                text.Copy();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.BeginVertical("PreferencesSectionBox",GUILayout.Width(position.width / 1.2f));
           
            GUILayout.Label(example);
                      
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Add"))
            {
                ItemData exampleData = new ItemData(itemDatas.Count, $"第{itemDatas.Count}个物品", $"这是第{itemDatas.Count}个物品的介绍", 10, "Items/Hp", ItemType.Material, ItemQuality.Epic);
                itemDatas.Add(exampleData);
            }
            if (GUILayout.Button("Delete"))
            {
                if (itemDatas.Count > 0)
                    itemDatas.RemoveAt(itemDatas.Count - 1);
                else "没有添加物品示例".LogInfo();
            }

            EditorGUILayout.EndHorizontal();

        }

        private Vector2 scrollPosition;

        private void GenericConfig()
        {
            EditorGUILayout.Space(position.height / 3);
            if (GUILayout.Button("创建背包配置文件",GUILayout.Height(60)))
            {
                string targetPath = string.Format(@"{0}/ItemConfigData.asset", genericPath);

                configData = Resources.Load<ItemConfigData>("ItemConfigData");

                if (configData != null) return;

                if (!Directory.Exists(genericPath))
                {
                    Directory.CreateDirectory(genericPath);
                    AssetDatabase.Refresh();
                }
                configData = ScriptableObject.CreateInstance<ItemConfigData>();
                AssetDatabase.CreateAsset(configData, targetPath);
            }           
        }
    }
}
#endif