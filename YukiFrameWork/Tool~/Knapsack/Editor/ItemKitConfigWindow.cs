
#if UNITY_EDITOR
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using YukiFrameWork.Extension;

namespace YukiFrameWork.Knapsack
{
    public class ItemKitConfigWindow : EditorWindow
    {
        private ItemConfigData configData;

        private const string genericPath = "Assets/Resources";
        private SerializedObject serializedObject;
        private SerializedProperty loadTypeProperty;
        private SerializedProperty projectNameProperty;
        private SerializedProperty projectPathProperty;
        private SerializedProperty handItemProperty;
        private SerializedProperty genericProperty;
        private SerializedProperty genericNameProperty;
        private StringBuilder infoBuilder = new StringBuilder();
        [MenuItem("YukiFrameWork/ItemConfig",false,2)]
        public static void OpenWindow()
        {
            var window = GetWindow<ItemKitConfigWindow>();
            window.Show();
            window.titleContent = new GUIContent("背包配置窗口");
        }

        private bool IsChangeClick = false;
        private bool IsGeneric;
        private List<Type> types = new List<Type>();

        private void OnEnable()
        {
            configData = Resources.Load<ItemConfigData>("ItemConfigData");
            SetProperty(configData);

            types = new List<Type>(AssemblyHelper.GetTypes(typeof(ItemData)).Where(t => Equals(t.BaseType,typeof(ItemData))));
        }

        private void SetProperty(ItemConfigData configData)
        {
            if (configData == null) return;
            serializedObject = new SerializedObject(configData);

            loadTypeProperty = serializedObject.FindProperty("loadType");
            projectNameProperty = serializedObject.FindProperty("projectName");
            projectPathProperty = serializedObject.FindProperty("projectPath");
            handItemProperty = serializedObject.FindProperty("handItemPrefab");
            genericProperty = serializedObject.FindProperty("genericPath");
            genericNameProperty = serializedObject.FindProperty("genericName");
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
            serializedObject.Update();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("请选择加载配置文件的方式:",GUILayout.Width(150));
            EditorGUILayout.PropertyField(loadTypeProperty,new GUIContent());        
            EditorGUILayout.EndHorizontal();           
            switch (loadTypeProperty.enumValueIndex)
            {
                case 0:
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("请输入模块名称:",GUILayout.Width(150));
                    EditorGUILayout.PropertyField(projectNameProperty,new GUIContent());
                    EditorGUILayout.EndHorizontal();
                    break;
                case 1:
                    projectNameProperty.stringValue = string.Empty;
                    break;
            }
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(loadTypeProperty.enumValueIndex == (int)LoadType.Resources ? "请输入配置文件的路径:" : "请输入配置文件的名称:", GUILayout.Width(150));
            EditorGUILayout.PropertyField(projectPathProperty,new GUIContent());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            GUILayout.Label("物品的UI预制体:\n(可在Assets文件夹下右键创建一键式面板,从中单独拿出ItemUI作为预制体保存):", GUILayout.Width(position.width));
            EditorGUILayout.PropertyField(handItemProperty);

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("打开物品品质类(可用于标识物品的稀有度,可自定义)",GUILayout.Height(25)))
            {
                string info = AssetDatabase.LoadAssetAtPath<TextAsset>(ImportSettingWindow.importPath).text;                
                ImportSettingWindow.Data data = AssemblyHelper.DeserializeObject<ImportSettingWindow.Data>(info);
                MonoScript mono = AssetDatabase.LoadAssetAtPath<MonoScript>(data.path + "/Knapsack/Core/ItemQuality/ItemQuality.cs");
                AssetDatabase.OpenAsset(mono);
            }     
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(GUI.skin.box);
            IsGeneric = EditorGUILayout.Toggle("打开代码生成",IsGeneric);
            if (IsGeneric)
            {
                
                EditorGUILayout.PropertyField(genericProperty, new GUIContent("生成ItemData代码路径:"));
                EditorGUILayout.PropertyField(genericNameProperty, new GUIContent("脚本名称:"));

                if (GUILayout.Button("生成代码"))
                {
                    string filePath = genericProperty.stringValue + @"/" + genericNameProperty.stringValue + ".cs";
                    if (!Directory.Exists(genericProperty.stringValue))
                    {
                        Directory.CreateDirectory(genericProperty.stringValue);
                        AssetDatabase.Refresh();
                    }

                    if (string.IsNullOrEmpty(genericNameProperty.stringValue))
                    {
                        Debug.LogError("脚本名称不能为空!");
                        return;
                    }

                    if (File.Exists(filePath))
                    {
                        Debug.LogError("这个路径已经存在这个脚本了");
                        return;
                    }

                    StringBuilder builder = new StringBuilder();
                    builder.AppendLine("///=====================================================");
                    builder.AppendLine("/// - FileName:      " + genericNameProperty.stringValue + ".cs");
                    builder.AppendLine("/// - NameSpace:     YukiFrameWork.Knapsack.Example" );
                    builder.AppendLine("/// - Description:   自动生成的物品派生类");
                    builder.AppendLine("/// - Creation Time: " + System.DateTime.Now.ToString());
                    builder.AppendLine("/// -  (C) Copyright 2008 - 2024");
                    builder.AppendLine("/// -  All Rights Reserved.");
                    builder.AppendLine("///=====================================================");

                    builder.AppendLine("using YukiFrameWork;");
                    builder.AppendLine("using UnityEngine;");
                    builder.AppendLine("using System;");
                    builder.AppendLine($"namespace YukiFrameWork.Knapsack.Example");
                    builder.AppendLine("{");
                    builder.AppendLine($"\tpublic class {genericNameProperty.stringValue} : ItemData");
                    builder.AppendLine("\t{");
                    builder.AppendLine($"\t\tpublic {genericNameProperty.stringValue}(int id, string name, string description, int capacity,string sprites,ItemQuality quality) : base(id, name, description, capacity, sprites, quality)");
                    builder.AppendLine("\t\t{ }");
                    builder.AppendLine("\t}");
                    builder.AppendLine("}");
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        StreamWriter streamWriter = new StreamWriter(fileStream);
                        streamWriter.Write(builder);

                        streamWriter.Close();
                        fileStream.Close();
                        AssetDatabase.Refresh();
                    }
                    EditorApplication.delayCall = () =>
                    {
                        MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(filePath);
                        AssetDatabase.OpenAsset(monoScript);
                    };
                    
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();           
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("配置文件展示(可以赋值后新建配置文件直接粘贴):");
            if (GUILayout.Button("Copy"))
            {
                TextEditor text = new TextEditor();
                text.text = infoBuilder.ToString();
                text.OnFocus();
                text.Copy();
            }          
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            EditorGUILayout.BeginVertical("PreferencesSectionBox",GUILayout.Width(position.width / 1.2f));

            infoBuilder.Clear();
            for (int i = 0; i < configData.itemDatas.Count; i++)
            {
                if (i == 0)
                    infoBuilder.AppendLine("[").AppendLine($"{configData.itemDatas[i]}");
                else if (i == configData.itemDatas.Count - 1)
                    infoBuilder.AppendLine($"{configData.itemDatas[i]}").AppendLine("]");
                else infoBuilder.AppendLine($"{configData.itemDatas[i]}");
            }
            if (configData.itemDatas.Count == 1)
                infoBuilder.AppendLine("]");
         

            GUILayout.Label(infoBuilder.ToString());

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
            GUILayout.FlexibleSpace();
            if (IsChangeClick)
            {
                EditorGUILayout.BeginVertical();
                foreach (var type in types)
                {
                    if (GUILayout.Button(type.Name))
                    {
                        var item = AssemblyHelper.DeserializeObject("{ }", type) as ItemData;
                        item.ID = configData.itemDatas.Count;
                        item.Name = $"第{configData.itemDatas.Count}个物品";
                        item.Description = $"这是第{configData.itemDatas.Count}个物品的介绍";
                        item.Capacity = 10;
                        item.Sprites = "Items/Hp";                       
                        item.ItemQuality = ItemQuality.Epic;
                        item.TypeName = type.FullName;
                        configData.itemDatas.Add(AssemblyHelper.SerializedObject(item));
                        configData.Save();
                    }
                }
                if (GUILayout.Button("取消"))
                {
                    IsChangeClick = false;
                }
                EditorGUILayout.EndVertical();
            }
            else
            {
                if (GUILayout.Button("Add"))
                {
                    IsChangeClick = true;                   
                }
            }
            if (GUILayout.Button("Delete"))
            {
                if (configData.itemDatas.Count > 0)
                    configData.itemDatas.RemoveAt(configData.itemDatas.Count - 1);
                else "当前物品为空".LogInfo();
                configData.Save();
            }

            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();

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

                SetProperty(configData);
            }           
        }
    }
}
#endif