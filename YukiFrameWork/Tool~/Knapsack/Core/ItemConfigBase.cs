///=====================================================
/// - FileName:      ItemConfig.cs
/// - NameSpace:     YukiFrameWork.Knapsack
/// - Created:       Yuki
/// - Email:         1274672030@qq.com
/// - Description:   背包配置类
/// - Creation Time: 2023/12/26 16:05:25
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System.Collections.Generic;
using YukiFrameWork.ABManager;
using LitJson;

namespace YukiFrameWork.Knapsack
{
    public class ItemConfigBase
    {
        private readonly List<ItemData> itemDatas = new List<ItemData>();

        public ItemConfigData configData { get; private set; }

        private IItemInfomationParse infomationParse;
      
        public bool IsInited = false;      
     
        public void AddItem(ItemData itemData)
            => itemDatas.Add(itemData);   

        public ItemConfigBase(ItemConfigData configData,IItemInfomationParse infomationParse)
        {
            this.configData = configData;
            this.infomationParse = infomationParse;          
        }

        /// <summary>
        /// Config初始化
        /// </summary>
        public void Init()
        {
            if (IsInited) return;                    
            if (configData == null)
            {
                Debug.LogError("加载配置文件失败,请检查是否使用ItemKit初始化!");
                return;
            }
            TextAsset textAsset = null;

            switch (configData.LoadType)
            {
                case LoadType.ABManager:
                    textAsset = AssetBundleManager.LoadAsset<TextAsset>(configData.ProjectName, configData.ProjectPath);
                    break;
                case LoadType.Resources:
                    textAsset = Resources.Load<TextAsset>(configData.ProjectPath);
                    break;               
            }
            JsonData[] datas = JsonMapper.ToObject<JsonData[]>(textAsset.text);  
            for (int i = 0; i < datas.Length; i++)
            {
                AddItem(infomationParse.ParseJsonToItem(datas[i]));
            }          
            IsInited = true;
        }
           

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
            ItemUI itemUI = null;
            switch (configData.LoadType)
            {
                case LoadType.ABManager:
                    itemUI = GameObjectLoader.Load(configData.HandItemPrefab.gameObject).GetComponent<ItemUI>();
                    break;
                case LoadType.Resources:
                    itemUI = GameObject.Instantiate(configData.HandItemPrefab);
                    break;             
            }
           
            itemUI.transform.SetParent(transform);
            return itemUI;
        }

        public void ReleaseItemUI(ItemUI itemUI)
        {
            switch (configData.LoadType)
            {
                case LoadType.Resources:
                    Object.Destroy(itemUI.gameObject);
                    break;
                case LoadType.ABManager:
                    GameObjectLoader.UnLoad(itemUI.gameObject);
                    break;
            }
            
        }

        public Sprite LoadSprite(string spritePathOrName)
        {
            return configData.LoadType switch
            {
                LoadType.ABManager => AssetBundleManager.LoadAsset<Sprite>(configData.ProjectName, spritePathOrName),
                LoadType.Resources => Resources.Load<Sprite>(spritePathOrName),
                _ => null
            } ;
        }        
    }
}