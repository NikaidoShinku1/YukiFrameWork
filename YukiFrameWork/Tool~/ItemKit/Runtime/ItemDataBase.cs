///=====================================================
/// - FileName:      ItemDataBase.cs
/// - NameSpace:     YukiFrameWork.Item
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/19 18:27:49
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using YukiFrameWork.Extension;
using System.Reflection;

using UnityEngine.U2D;
using System.Linq;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace YukiFrameWork.Item
{
    public abstract class ItemDataBase : ScriptableObject
    {
        public abstract IItem[] Items { get; set; }
    }
    public abstract class ItemDataBase<Item> : ItemDataBase where Item :class, IItem
    {
        [SerializeField, Searchable, FoldoutGroup("物品管理", -1)]
        private Item[] items = new Item[0];

        public override IItem[] Items
        {
            get => items.Select(x => x as IItem).ToArray();
            set => items = value.Select(x => x as Item).ToArray();
        }

#if UNITY_EDITOR
        public virtual void OnEnable()
        {
            if (string.IsNullOrEmpty(fileName))
            { 
                fileName = this.GetType().Name.Replace("DataBase","s");
                this.Save();
            }
            InitItemTypeByDataBase();
        }
        [SerializeField, LabelText("命名空间"), FoldoutGroup("Code Setting", -2)]
        string NameSpace = "YukiFrameWork.Item.Example";
        [SerializeField, LabelText("生成路径"), FoldoutGroup("Code Setting"), ShowIf(nameof(DoNotNullOrEmpty))]
        string filePath = string.Empty;

        [SerializeField,LabelText("脚本名称/文件名称"), FoldoutGroup("Code Setting")]
        string fileName;
        private bool DoNotNullOrEmpty => !string.IsNullOrEmpty(filePath);
        [Button("生成代码"), FoldoutGroup("Code Setting")]
        void CreateCode()
        {
            if (Items.Length == 0)
            {
                Debug.LogWarning("没有添加物品,无法创建代码");
                return;
            }

            if (string.IsNullOrEmpty(NameSpace))
            {
                Debug.LogWarning("请输入命名空间，为项目保持好习惯");
                return;
            }
            StringBuilder builer = new StringBuilder();
            if (string.IsNullOrEmpty(filePath))
            {
                filePath = UnityEditor.EditorUtility.SaveFolderPanel(fileName, fileName, "");
                string[] cores = filePath.Split('/');
                bool enter = false;
                for (int i = 0; i < cores.Length; i++)
                {
                    if (cores[i].Equals("Assets"))
                        enter = true;

                    if (!enter) continue;

                    builer.Append(cores[i] + (i == cores.Length - 1 ? "" : "/"));
                }

                filePath = builer.ToString();
            }           
            if (string.IsNullOrEmpty(filePath)) return;
            if (string.IsNullOrEmpty(fileName)) return;
            string name = fileName;
            UnityEditor.AssetDatabase.Refresh();
            CodeCore core = new CodeCore();
            CodeWriter writer = new CodeWriter();
            foreach (var item in Items)
            {
                if (string.IsNullOrEmpty(item.GetKey))
                {
                    Debug.LogWarning("存在Key为Null或者没有填写的物品");
                    continue;
                }
                writer.CustomCode($"public static IItem {item.GetKey} => ItemKit.GetItemByKey(\"{item.GetKey}\");");
                writer.CustomCode($"public static string Key_{item.GetKey} = \"{item.GetKey}\";");
            }

            core.Descripton(name, NameSpace, "对项目内所有的ItemConfig收集代码生成类", System.DateTime.Now.ToString())
                .CodeSetting(NameSpace, name, string.Empty, writer, false, false)
                ;

            System.IO.File.WriteAllText(filePath + "/" + name + ".cs", core.builder.ToString());
           
            AssetDatabase.Refresh();

        }
        [LabelText("打开物品类型的自定义功能"), SerializeField, BoxGroup("物品自定义")]
        bool IsOpenCustomType;
        [Button("生成物品类型代码", ButtonHeight = 20), BoxGroup("物品自定义"), ShowIf(nameof(IsOpenCustomType))]
        void CreateItemType()
        {
            CodeCore core = new CodeCore();
            CodeWriter writer = new CodeWriter();
            foreach (var key in mItemTypeDicts.Keys)
            {
                writer.CustomCode($"[LabelText(\"{mItemTypeDicts[key]}\")]")
                    .CustomCode($"{key},");
            }
            core.Using("Sirenix.OdinInspector").Descripton(nameof(ItemType), "YukiFrameWork.Item", "物品的类型枚举", System.DateTime.Now.ToString())
                .CodeSetting("YukiFrameWork.Item", nameof(ItemType), string.Empty, writer, false, false, true)
                .builder.CreateFileStream(ImportSettingWindow.GetData().path + "/ItemKit/Runtime", nameof(ItemType), ".cs");
        }
        [SerializeField, LabelText("配置文件:"), FoldoutGroup("配置表设置")]
        TextAsset textAsset;
        [Button("导入", ButtonHeight = 30), PropertySpace, FoldoutGroup("配置表设置")]
        [InfoBox("Item中对于精灵/精灵图集的路径配置，必须是以Assets为开头的完整包含后缀的路径")]
        void Import()
        {
            if (textAsset == null)
            {
                Debug.LogError("无法导入，当前没有添加Json配置文件");
                return;
            }

            List<Item> items = SerializationTool.DeserializedObject<List<Item>>(textAsset.text);            
            foreach (Item item in items)
            {
                item.GetIcon = string.IsNullOrEmpty(item.SpriteAtlas)
                        ? UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(item.Sprite)
                        : UnityEditor.AssetDatabase.LoadAssetAtPath<SpriteAtlas>(item.SpriteAtlas).GetSprite(item.Sprite);
            }

            Items = items.Select(x => x as IItem).ToArray();
        }
        [LabelText("导出路径"), FoldoutGroup("配置表设置"),SerializeField]
        string reImportPath = "Assets/ItemKitData";
        [LabelText("导出文件名称"), FoldoutGroup("配置表设置"),SerializeField]
        string reImportName = "Item";
        [Button("将现有物品导出配表",ButtonHeight = 30),FoldoutGroup("配置表设置"),PropertySpace(15)]
        void ReImport()
        {
            if (this.Items?.Length == 0)
            {
                return;
            }
            foreach (var item in Items)
            {
                if (item.GetIcon != null)
                {
                    item.Sprite = AssetDatabase.GetAssetPath(item.GetIcon);
                }
            }
            SerializationTool.SerializedObject(Items).CreateFileStream(reImportPath, reImportName, ".json");
        }

        [SerializeField, LabelText("物品的类型收集"), Searchable, DictionaryDrawerSettings(KeyLabel = "类型", ValueLabel = "类型介绍"), BoxGroup("物品自定义"), ShowIf(nameof(IsOpenCustomType))]
        [InfoBox("请勿更改ItemType文件的存放位置，这是由框架设定的", InfoMessageType.Warning)]
        private YDictionary<string, string> mItemTypeDicts = new YDictionary<string, string>();

        void InitItemTypeByDataBase()
        {
            var itemType = typeof(ItemType);

            mItemTypeDicts.Clear();
            for (int i = 0; i < Enum.GetValues(itemType).Length; i++)
            {
                ItemType type = (ItemType)Enum.GetValues(itemType).GetValue(i);

                LabelTextAttribute labelText = itemType.GetField(type.ToString()).GetCustomAttribute<LabelTextAttribute>();

                mItemTypeDicts[type.ToString()] = labelText?.Text;
            }

        }
#endif
    }   
}
