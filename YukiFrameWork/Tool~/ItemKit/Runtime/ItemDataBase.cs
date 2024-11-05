///=====================================================
/// - FileName:      ItemDataBase.cs
/// - NameSpace:     YukiFrameWork.Item
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/19 18:27:49
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using YukiFrameWork.Extension;
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
#if UNITY_EDITOR
       
        protected static ValueDropdownList<string> allItemTypes;

        internal static ValueDropdownList<string> AllItemTypes
        {
            get
            {
                if (allItemTypes == null)
                {
                    allItemTypes = new ValueDropdownList<string>();
                }

                allItemTypes.Clear();

                string[] guids = AssetDatabase.FindAssets($"t:{typeof(ItemDataBase)}");
                var items = YukiAssetDataBase.FindAssets<ItemDataBase>();
                foreach (var itemDataBase in items) {

                    foreach (var type in itemDataBase.mItemTypeDicts)
                    {
                        var temp = allItemTypes.Find(x => x.Value == type.Key);
                        if (temp.Value == type.Key)
                            continue;
                        allItemTypes.Add(type.Value, type.Key);
                    }

                    if (allItemTypes.Count == 0)
                        itemDataBase.ResetItemType();

                }
                return allItemTypes;
            }
        }
#endif
        public virtual void OnEnable()
        {
            InitItemTypeByDataBase();
        }

        private void Reset()
        {
            InitItemTypeByDataBase();
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            InitItemTypeByDataBase();
            var items = YukiAssetDataBase.FindAssets<ItemDataBase>();
            YDictionary<string, string> mItemTypeDicts = new YDictionary<string, string>();
            foreach (var item in items)
            {
                foreach (var dict in item.mItemTypeDicts)
                {
                    mItemTypeDicts[dict.Key] = dict.Value;
                }
            }

            foreach (var item in items)
            {
                item.mItemTypeDicts = mItemTypeDicts;
            }
        }
#endif
        protected abstract void ResetItemType();

        void InitItemTypeByDataBase()
        {
            if (mItemTypeDicts.Count == 0)
            {
                mItemTypeDicts["Consumable"] = "消耗品";
                mItemTypeDicts["Equipment"] = "装备";
                mItemTypeDicts["Material"] = "材料";
                mItemTypeDicts["Weapon"] = "武器";
            }
           
        }

        [SerializeField, LabelText("物品的类型收集"), Searchable, DictionaryDrawerSettings(KeyLabel = "类型", ValueLabel = "类型介绍"), BoxGroup("物品自定义")]
        [InfoBox("所有的物品配置，都可以单独设置物品的类型，但全局共享，如果有相同的Key/Value，则数据共享，访问Item.ItemType以Key为主", InfoMessageType.Info)]
        private YDictionary<string, string> mItemTypeDicts = new YDictionary<string, string>();
    }
    public abstract class ItemDataBase<Item> : ItemDataBase where Item : class, IItem
    {
        [SerializeField, Searchable, FoldoutGroup("物品管理", -1), TableList]
        internal Item[] items = new Item[0];
        protected override void ResetItemType()
        {
            foreach (var item in items)
            {
                item.ItemType = string.Empty;
            }

        }
        public override IItem[] Items
        {
            get => items.Select(x => x as IItem).ToArray();
            set => items = value.Select(x => x as Item).ToArray();
        }

#if UNITY_EDITOR
        public override void OnEnable()
        {

            if (string.IsNullOrEmpty(fileName))
            {
                fileName = this.GetType().Name.Replace("DataBase", "s");
            }
            base.OnEnable();
        }
        [SerializeField, LabelText("命名空间"), FoldoutGroup("Code Setting", -2)]
        string NameSpace = "YukiFrameWork.Item.Example";
        [SerializeField, LabelText("生成路径"), FoldoutGroup("Code Setting"), ShowIf(nameof(DoNotNullOrEmpty))]
        string filePath = string.Empty;

        [SerializeField, LabelText("脚本名称/文件名称"), FoldoutGroup("Code Setting")]
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
        [LabelText("导出路径"), FoldoutGroup("配置表设置"), SerializeField]
        string reImportPath = "Assets/ItemKitData";
        [LabelText("导出文件名称"), FoldoutGroup("配置表设置"), SerializeField]
        string reImportName = "Item";
        [Button("将现有物品导出配表", ButtonHeight = 30), FoldoutGroup("配置表设置"), PropertySpace(15)]
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
        [Button("打开Item编辑器窗口(Plus)", ButtonHeight = 40)]
        void OpenWindow()
        {
            ItemDesignerWindow.OpenWindow();
        }
#endif
    }
}

