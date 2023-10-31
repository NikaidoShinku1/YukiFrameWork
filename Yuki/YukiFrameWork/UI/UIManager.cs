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
        private static readonly Dictionary<Type, BasePanel> panelsDict = new Dictionary<Type, BasePanel>();        
        
        private Transform mCanvasTransform => UnityEngine.Object.FindObjectOfType<Canvas>().transform;

        private static readonly List<BasePanel> activePanels = new List<BasePanel>();

        public Dictionary<Type,BasePanel> PanelsDict => panelsDict;    
        
        public string PanelPath { get; private set; }

        public string AssetBundleName { get; private set; }

        private Attribution panelLoadType;

        private ResNode resNode = ResKit.Get();

        public UIManager() 
        {
            Debug.Log("lailo");
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
                return;
            }

            InitAllPanelPrefab();
        }

        private void InitAllPanelPrefab()
        {
            switch (panelLoadType)
            {
                case Attribution.Resources:
                    resNode.LoadAllAsync<BasePanel>(Attribution.Resources, PanelPath, panels => 
                    {
                        foreach (var panel in panels)
                        {
                            CreateParentPanels(panel.GetType(), panel);
                        }
                    }).Forget();
                    break;
                case Attribution.AssetBundle:
                    resNode.LoadAllAsync<BasePanel>(Attribution.AssetBundle, AssetBundleName,panels => 
                    {
                        foreach (var panel in panels)
                        {                            
                            CreateParentPanels(panel.GetType(),panel);
                        }
                    }).Forget();
                    break;              
            }           
        }        

        public void CreateParentPanels(Type type, BasePanel panel)
        {
            if(!panelsDict.ContainsKey(type))
            panelsDict.Add(type, panel);
        }

        public BasePanel GetPanel(Type type)
        {
            Debug.Log(type);
            BasePanel panel = CheckOrCreatePanel(type);
            return panel;
           
        }

        public void RemovePanel(BasePanel panel)
        {
            activePanels.Remove(panel);
        }

        /// <summary>
        /// 检查并生成面板
        /// </summary>
        /// <param name="type">面板类型</param>
        /// <returns>返回一个面板</returns>
        private BasePanel CheckOrCreatePanel(Type type)
        {           
            BasePanel panel = null;
            if (activePanels.Count > 0)
            {            
                BasePanel newPanel = activePanels.Find(x => x.GetType() == type);
                panel = GameObject.Instantiate(newPanel, mCanvasTransform, false);
            }
            else if (!panelsDict.TryGetValue(type, out var newPanel))
            {
                switch (panelLoadType)
                {
                    case Attribution.Resources:
                        newPanel = resNode.LoadSync<BasePanel>(Attribution.Resources, panel.name);
                        break;
                    case Attribution.AssetBundle:
                        newPanel = resNode.LoadSync<BasePanel>(Attribution.AssetBundle,AssetBundleName, panel.name);
                        break;
                
                }

                CreateParentPanels(type, newPanel);
                panel = GameObject.Instantiate(newPanel, mCanvasTransform, false);
            }
            else
            {
                panel = GameObject.Instantiate(newPanel, mCanvasTransform, false);
            }
           
            return panel;
        }

        public void Clear()
        {
            panelsDict.Clear();
            activePanels.Clear();
        }

    }
}
