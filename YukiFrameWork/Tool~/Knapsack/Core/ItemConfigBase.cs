///=====================================================
/// - FileName:      ItemConfig.cs
/// - NameSpace:     YukiFrameWork.Knaspack
/// - Created:       Yuki
/// - Email:         1274672030@qq.com
/// - Description:   背包配置类
/// - Creation Time: 2023/12/26 16:05:25
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
using System.Collections.Generic;
using YukiFrameWork.ABManager;
using LitJson;
using Newtonsoft.Json;
using YukiFrameWork.Pools;

namespace YukiFrameWork.Knaspack
{
    public abstract class ItemConfigBase
    {
        private readonly List<ItemData> itemDatas = new List<ItemData>();        

        public string HandItemPrefabName;

        public bool IsInited = false;
     
        public void AddItem(ItemData itemData)
            => itemDatas.Add(itemData);

        public string ProjectName { get;protected set; }

        private string handItemName;

        /// <summary>
        /// Config初始化
        /// </summary>
        public void Init(string projectName,string handItemName)
        {
            if (IsInited) return;
            ProjectName = projectName;
            this.handItemName = handItemName;
            TextAsset textAsset = null;            
            textAsset = AssetBundleManager.LoadAsset<TextAsset>(projectName,"ItemJson");
            if (textAsset == null)
            {
                Debug.LogError("加载Json配置文件失败!请检查是否使用框架资源管理套件ABManager进行数据的配置！");
                return;
            }
            JsonData[] datas = JsonMapper.ToObject<JsonData[]>(textAsset.text);                   
            for (int i = 0; i < datas.Length; i++)
            {
                ParseItemToJson(datas[i]);
            }          
            IsInited = true;
        }
      
        /// <summary>
        /// 解析Json配置表获取物品参数
        /// </summary>
        /// <param name="jsonData">物品的Json数据</param>
        public abstract void ParseItemToJson(JsonData jsonData);

        public ItemData GetItemByID(int id)
        {
            ItemData data = itemDatas.Find(x => x.ID == id);          
            return data;
        }

        public ItemData GetItemByName(string name)
        {
            ItemData data = itemDatas.Find(x => x.Name.Equals(name));
            return data;
        }

        public virtual void Release()
        {
            itemDatas.Clear();                      
        }

        public ItemUI GetItemUI(Transform transform)
        {
            var obj = GameObjectLoader.Load(ProjectName,handItemName);
            var itemUI = obj.GetComponent<ItemUI>();
            itemUI.transform.SetParent(transform);
            return itemUI;
        }

        public void ReleaseItemUI(ItemUI itemUI)
        {
            GameObjectLoader.UnLoad(itemUI.gameObject);
        }

    }
}