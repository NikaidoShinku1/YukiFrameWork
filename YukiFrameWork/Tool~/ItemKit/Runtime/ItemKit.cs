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

        public static void Init(string projectName)
        {
            loader = new ABManagerItemKitLoader(projectName);
        }

        public static void Init(IItemKitLoader loader)
        {
            ItemKit.loader = loader;          
        }
        [Obsolete("方法已过时，请使用ItemKit.Init进行背包物品的初始化加载!")]
        public static void InitLoader(string projectName)
        {
            loader = new ABManagerItemKitLoader(projectName);
        }
        [Obsolete("方法已过时，请使用ItemKit.Init进行背包物品的初始化加载!")]
        public static void InitLoader(IItemKitLoader loader)
        {
            ItemKit.loader = loader;
        }


        /// <summary>
        /// 将所有的分组物品清空
        /// </summary>
        public static void ClearAllSlogGroup()
        {
            ClearAllSlogGroup(_ => true);
        }

        public static void ClearAllSlogGroup(Func<SlotGroup,bool> condition)
        {
            foreach (var slotGroup in SlotGroupDicts.Values)
            {
                if(condition(slotGroup))
                    slotGroup.ClearItems();
            }
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

        [Obsolete("方法已过时，请调用ItemKit.LoadItemDataManager方法进行物品管理器资源的加载！",true)]
        public static void LoadItemDataBase(string dataBaseName)
        {
            //error
        }
        [Obsolete("方法已过时，请调用ItemKit.LoadItemDataManager方法进行物品管理器资源的加载！",true)]
        public static IEnumerator LoadItemDataBaseAsync(string dataBaseName)
        {
            yield return null;
        }
        [Obsolete("方法已过时，请调用ItemKit.LoadItemDataManager方法进行物品管理器资源的加载！",true)]
        public static void LoadItemDataBase(ItemDataBase itemDataBase)
        {
            //error
        }
        /// <summary>
        /// 加载物品配置管理器
        /// </summary>
        /// <param name="dataBaseName"></param>
        public static void LoadItemDataManager(string name)
        {
            LoadItemDataManager(loader.Load<ItemDataManager>(name));
        }
        /// <summary>
        /// 异步加载物品配置管理器
        /// </summary>
        /// <param name="name"></param>
        public static IEnumerator LoadItemDataManagerAsync(string name)
        {
            bool loadCompleted = false;
            loader.LoadAsync<ItemDataManager>(name, v =>
            {
                LoadItemDataManager(v);
                loadCompleted = true;
            });
            yield return CoroutineTool.WaitUntil(() => loadCompleted);
        }
      
        public static void LoadItemDataManager(ItemDataManager itemDataManager)
        {
            foreach (var manager in itemDataManager.itemDataBases)
            {
                var items = manager.Items;
                foreach (var item in items)
                {
                    AddItem(item);
                }
            }          
            loader?.UnLoad(itemDataManager);
        }

        public static void AddItem(IItem item)
        {
            mItemDicts.Add(item.Key, item);
        }

        public static bool RemoveItem(string Key)
        {
#if UNITY_2021_1_OR_NEWER
            return RemoveItem(Key, out _);
#else
            return mItemDicts.Remove(Key);
#endif

        }

#if UNITY_2021_1_OR_NEWER
        public static bool RemoveItem(string Key,out IItem item)
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
                    itemKey = x.Item != null ? x.Item.Key : null,
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
                slotGroup.ClearSlots();
                slotGroup.CreateSlotsByCount(count,true);
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
