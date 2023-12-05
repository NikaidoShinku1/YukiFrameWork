using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;
using System;
using YukiFrameWork.Res;
using System.Collections;

namespace YukiFrameWork.UI
{
    public class UIManager : Singleton<UIManager>
    {
        private readonly Dictionary<Type, BasePanel> panelsDict = new Dictionary<Type, BasePanel>();        
        
        private Transform mCanvasTransform => UnityEngine.Object.FindObjectOfType<Canvas>().transform;

        /// <summary>
        /// 保存所有已经弹出来的面板
        /// </summary>
        private readonly Dictionary<Type,BasePanel> activePanelsDict = new Dictionary<Type, BasePanel>();

        public Dictionary<Type,BasePanel> PanelsDict => panelsDict;    
        
        public string PanelPath { get; private set; }

        public string AssetBundleName { get; private set; }      

        private readonly ResLoader resNode = ResKit.GetLoader();

        private LoadMode panelLoadType;

        public bool IsInit { get; private set; } = false;

        public bool IsCompleted { get; private set; } = false;

        public void Init(string panelPath, LoadMode mode)
        {
            if (IsInit) return;
            this.PanelPath = panelPath;
            panelLoadType = mode;
            InitAllPanelPrefab();
            IsInit = true;
        }

        public IEnumerator InitAsync()
        {
            yield return new WaitUntil(() => IsCompleted);
        }

        private void InitAllPanelPrefab()
        {
            switch (panelLoadType)
            {
                case LoadMode.同步:
                    var panels = resNode.LoadAllAssetsFromComponent<BasePanel>(PanelPath);
                    foreach (var panel in panels)
                    {
                        CreateParentPanels(panel.GetType(), panel);
                    }
                    IsCompleted = true;
                    break;
                case LoadMode.异步:
                    resNode.InitAsync();
                    resNode.LoadAllAssetsFromComponentsAsync<BasePanel>(PanelPath,(panels,a,b,c) => 
                    {
                        foreach (var panel in panels)
                        {                            
                            CreateParentPanels(panel.GetType(),panel);
                        }
                        IsCompleted = true;
                    });
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

        public void RemovePanel(Type panelType)
        {
            activePanelsDict.Remove(panelType);
        }

        /// <summary>
        /// 检查并生成面板
        /// </summary>
        /// <param name="type">面板类型</param>
        /// <returns>返回一个面板</returns>
        private BasePanel CheckOrCreatePanel(Type type)
        {
            activePanelsDict.TryGetValue(type, out var panel);
            if (panel != null)
            {
                panel.transform.SetParent(mCanvasTransform);
                return panel;
            }
            if (!panelsDict.TryGetValue(type, out var newPanel))
            {
                Debug.LogError("UIManager 没有储存该面板，请检查！Panel：" + type);
            }
            else
            {
                panel = GameObject.Instantiate(newPanel, mCanvasTransform, false);                
                activePanelsDict.Add(panel.GetType(),panel);
            }               

            return panel;
        }

        public void Clear()
        {
            resNode.Release();
            panelsDict.Clear();
            activePanelsDict.Clear();
        }

    }
}
