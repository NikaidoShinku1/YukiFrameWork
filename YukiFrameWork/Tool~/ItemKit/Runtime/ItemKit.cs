///=====================================================
/// - FileName:      ItemKit.cs
/// - NameSpace:     YukiFrameWork.Item
/// - Description:   背包管理套件
/// - Creation Time: 2024/4/18 18:23:54
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using YukiFrameWork.Extension;
using System.Collections;
namespace YukiFrameWork.Item
{
	public class ItemKit
	{
        private const string SLOT_GROUP_ = "slot_group_";

        private static IItemKitLoader loader;

        public static UISlot CurrentSlotPointer { get; set; } = null;

        internal static bool UseLocalizationConfig { get;private set; } = false;       

        internal static string LocalizationConfigKey { get; private set; }

        public static EasyEvent<IItem> onLanguageChanged = new EasyEvent<IItem>();       

        /// <summary>
        /// 本地化的分割字符，未启用本地化套件则不需要使用
        /// </summary>
        public static char Spilt { get; private set; }

        public static void InitLoader(string projectName)
        {
            loader = new ABManagerItemKitLoader(projectName);
            Init();
        }

        public static void InitLoader(IItemKitLoader loader)
        {
            ItemKit.loader = loader;
            Init();
        }

        private static void Init()
        {
            MonoHelper.Destroy_AddListener(helper => 
            {
                loader = null;
                foreach (var group in mSlotGroupDicts.Values)
                {
                    group.Clear();
                }

                mSlotGroupDicts.Clear();
                mItemDicts.Clear();
                CurrentSlotPointer = null;
                LocalizationConfigKey = string.Empty;
                onLanguageChanged.UnRegisterAllEvent();
            });
        }

        /// <summary>
        /// 调用一次该方法就可以将ItemKit绑定本地化系统
        /// </summary>
        /// <param name="DependIndex">是否是子配置，如果是的话传递对应配置下标，否则不需要</param>
        /// <param name="spilt">分割字符，用于配置中的Context配置时，区分Name以及Description</param>
        public static void DependLocaizationConfig(string configKey,char spilt = ':')
        {
            if (!UseLocalizationConfig)
                LocalizationKit.RegisterLanguageEvent(UpdateAllItemInfo);
            LocalizationConfigKey = configKey;
            UseLocalizationConfig = true;
            Spilt = spilt;
            void UpdateAllItemInfo(Language language)
            {
                foreach (var item in mItemDicts.Values)
                {
                    _ = item.GetName;
                    _ = item.GetDescription;
                    onLanguageChanged.SendEvent(item);
                }            
            }                
        }

        /// <summary>
        /// 将所有的分组物品清空
        /// </summary>
        public static void ClearAllSlogGroup()
        {
            foreach (var slotGroup in SlotGroupDicts.Values)
            {
                slotGroup.ClearItem();
            }
        }

        internal static ILocalizationData GetLocalization(string Key)
        {
            return LocalizationKit.GetContent(ItemKit.LocalizationConfigKey, Key, LocalizationKit.LanguageType);                                 
        }

        /// <summary>
        /// 是否进行本地默认保存(默认开启(适合不用存档系统时))
        /// </summary>
        public static bool DefaultSaveAndLoader { get; set; } = true;
        
        private static Dictionary<string, SlotGroup> mSlotGroupDicts = new Dictionary<string, SlotGroup>();

        public static IReadOnlyDictionary<string, SlotGroup> SlotGroupDicts => mSlotGroupDicts;

        public static SlotGroup CreateSlotGroup(string key)
        {
            var group = new SlotGroup(key);
            mSlotGroupDicts.Add(key, group);
            return group;
        }      
#if UNITY_2021_1_OR_NEWER
        public static SlotGroup RemoveSlotGroup(string key)
        {
            mSlotGroupDicts.Remove(key, out var group);
            return group;
        }
#endif
        private static Dictionary<string, IItem> mItemDicts = new Dictionary<string, IItem>();

        public static IReadOnlyDictionary<string, IItem> ItemDicts => mItemDicts;

        public static void LoadItemDataBase(string dataBaseName)
        {
            LoadItemDataBase(loader.Load<ItemDataBase>(dataBaseName));           
        }

        public static IEnumerator LoadItemDataBaseAsync(string dataBaseName)
        {
            bool loadCompleted = false;
            loader.LoadAsync<ItemDataBase>(dataBaseName, v =>
            {
                LoadItemDataBase(v);
                loadCompleted = true;
            });
            yield return CoroutineTool.WaitUntil(() => loadCompleted);
        }

        public static void LoadItemDataBase(ItemDataBase itemDataBase)
        {
            var items = itemDataBase.Items;
            foreach (var item in items)
            {
                AddItemConfig(item);
            }
            
            loader?.UnLoad(itemDataBase);
        }

        public static void AddItemConfig(IItem item)
        {
            mItemDicts.Add(item.GetKey, item);
        }

        public static bool RemoveItemConfig(string Key)
        {
#if UNITY_2021_1_OR_NEWER
            return RemoveItemConfig(Key, out _);
#else
            return mItemDicts.Remove(Key);
#endif

        }

#if UNITY_2021_1_OR_NEWER
        public static bool RemoveItemConfig(string Key,out IItem item)
        {
            return mItemDicts.Remove(Key,out item);
        }
#endif
        public static SlotGroup GetSlotGroup(string key)
        {
            mSlotGroupDicts.TryGetValue(key, out var value);
            return value;
        }

        public static IItem GetItemByKey(string itemKey)
        {
            mItemDicts.TryGetValue(itemKey, out var item);
            return item;
        }     
        [Serializable]
        public class SlotSaveData
        {
            public string itemKey;
            public int itemCount;
        }    
        /// <summary>
        /// 保存所有的背包套件分组数据，得到分组的数据列表转换的Json字符串
        /// </summary>
        /// <returns></returns>
        public static string Save()
        {
            Dictionary<string, List<SlotSaveData>> slotSaveDatas = new Dictionary<string, List<SlotSaveData>>(); 
            foreach (var slotGroup in mSlotGroupDicts.Values)
            {
                if (!slotSaveDatas.TryGetValue(slotGroup.Key, out var value))
                {
                    value = new List<SlotSaveData>();                   
                }
                value = slotGroup.Slots.Select(x => new SlotSaveData()
                {
                    itemKey = x.Item != null ? x.Item.GetKey : null,
                    itemCount = x.ItemCount
                }).ToList();
                slotSaveDatas[slotGroup.Key] = value;
            }
            var json = SerializationTool.SerializedObject(slotSaveDatas);            
            if(DefaultSaveAndLoader)
                PlayerPrefs.SetString(ItemKit.SLOT_GROUP_, json);          
            return json;
        }          
        /// <summary>
        /// 读取信息
        /// </summary>
        /// <param name="info">json信息</param>
        /// <param name="eventTrigger">是否触发更新事件</param>
        public static void Load(string info,bool eventTrigger = true)
        {
            if (string.IsNullOrEmpty(info)) return;
            Dictionary<string, List<SlotSaveData>> saveDatas = SerializationTool.DeserializedObject<Dictionary<string, List<SlotSaveData>>>(info);

            foreach (var key in saveDatas.Keys)
            {
                var slotGroup = ItemKit.GetSlotGroup(key);
                slotGroup ??= CreateSlotGroup(key);
                var data = saveDatas[key];
                uint count = (uint)data.Count;
                slotGroup.ClearItem();
                slotGroup.CreateSlotsByCount(count);
                for (int i = 0; i < count; i++)
                {
                    var slot = data[i];
                    var item = string.IsNullOrEmpty(slot.itemKey) ? null : ItemKit.ItemDicts[slot.itemKey];

                    if (i < slotGroup.Slots.Count)
                    {
                        slotGroup.Slots[i].Item = item;
                        slotGroup.Slots[i].ItemCount = slot.itemCount;
                       // slotGroup.Slots[i].
                        if (eventTrigger)
                            slotGroup.Slots[i].OnItemChanged.SendEvent();
                    }
                    else
                    {
                        slotGroup.CreateSlot(item, slot.itemCount);
                    }
                }
            }
        }

        /// <summary>
        /// 通过内置的存档信息读取(仅支持Json)
        /// </summary>      
        public static void Load(bool eventTrigger = true)
        {
            if (!DefaultSaveAndLoader)
            {
                Debug.LogError("没有开启" + DefaultSaveAndLoader + "本地存档--- 无法内置读取，请使用Load(string info,bool eventTrigger = true)进行外部的Json信息传输!");
                return;
            }
            Load(PlayerPrefs.GetString(SLOT_GROUP_),eventTrigger);
        }
    }
}
