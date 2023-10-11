using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;
using System;
using YukiFrameWork.Res;
using Cysharp.Threading.Tasks;

namespace YukiFrameWork.UI
{
    public class UIManager
    {
        private static readonly Dictionary<UIPanelType, BasePanel> panelsDict = new Dictionary<UIPanelType, BasePanel>();        
        
        private Transform mCanvasTransform => UnityEngine.Object.FindObjectOfType<Canvas>().transform;       

        public Dictionary<UIPanelType,BasePanel> PanelsDict => panelsDict;    
        
        public string PanelPath { get; private set; }

        public string AssetBundleName { get; private set; }

        private Attribution panelLoadType;

        public UIManager() 
        {
            try
            {
                string jsonPath = Application.streamingAssetsPath + "/UIPanel" + "/UIPath.Json";
                UIPath path = JsonMapper.ToObject<UIPath>(File.ReadAllText(jsonPath));
                PanelPath = path.UIPanelPath;
                panelLoadType = path.type;
                AssetBundleName = path.assetBundleName;
            }
            catch 
            {
                Debug.LogWarning("未使用UIToolKit进行路径初始化，将保持路径默认并检索所有Resources");
                PanelPath = string.Empty;
            }

            InitAllPanelPrefab();
        }

        private void InitAllPanelPrefab()
        {
            switch (panelLoadType)
            {
                case Attribution.Resources:
                    ResKit.LoadAllAsync<BasePanel>(Attribution.Resources, PanelPath).Forget();
                    break;
                case Attribution.AssetBundle:
                    ResKit.LoadAllAsync<BasePanel>(Attribution.AssetBundle, AssetBundleName).Forget();
                    break;              
            }           
        }

        public void AddPanels(UIPanelType type, BasePanel panel)
        {
            panelsDict.Add(type, panel);
        }

        public BasePanel GetPanel(UIPanelType type)
        {
            BasePanel panel = CheckOrCreatePanel(type);
            return panel;
           
        }

        /// <summary>
        /// 检查并生成面板
        /// </summary>
        /// <param name="type">面板类型</param>
        /// <returns>返回一个面板</returns>
        private BasePanel CheckOrCreatePanel(UIPanelType type)
        {
            BasePanel panel = null;
            if (!PanelsDict.ContainsKey(type))
            {
                List<BasePanel> panels = null;
                switch (panelLoadType)
                {
                    case Attribution.Resources:
                        panels = ResKit.LoadAllSync<BasePanel>(Attribution.Resources, PanelPath);
                        break;
                    case Attribution.AssetBundle:
                        panels = ResKit.LoadAllSync<BasePanel>(Attribution.AssetBundle, AssetBundleName);
                        break;                 
                }
                foreach (var item in panels)
                {
                    Enum.TryParse(item.name, out UIPanelType tempType);
                    if (tempType == type)
                    {
                        panel = GameObject.Instantiate(item, mCanvasTransform, false);
                        panel.name = item.name;
                        PanelsDict.Add(type, panel);
                        return panel;
                    }
                }

            }
            else panel = PanelsDict[type];
            return panel;
        }

        public void Clear()
        {
            panelsDict.Clear();
        }

    }
}
