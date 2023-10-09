using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System.IO;
using System;

namespace YukiFrameWork.UI
{
    public class UIManager
    {
        private static readonly Dictionary<UIPanelType, BasePanel> panelsDict = new Dictionary<UIPanelType, BasePanel>();        
        
        private Transform mCanvasTransform => UnityEngine.Object.FindObjectOfType<Canvas>().transform;       

        public Dictionary<UIPanelType,BasePanel> PanelsDict => panelsDict;    
        
        public string PanelPath { get; private set; }

        public UIManager() 
        {
            try
            {
                string jsonPath = Application.streamingAssetsPath + "/UIPanel" + "/UIPath.Json";
                UIPath path = JsonMapper.ToObject<UIPath>(File.ReadAllText(jsonPath));
                PanelPath = path.UIPanelPath;
            }
            catch 
            {
                Debug.LogWarning("未使用UIToolKit进行路径初始化，将保持路径默认并检索所有Resources");
                PanelPath = string.Empty;
            }
        }

        public void AddPanels(UIPanelType type, BasePanel panel)
        {
            panelsDict.Add(type, panel);
        }

        public BasePanel GetPanel(UIPanelType type)
        {
            BasePanel panel = null;
            if (!PanelsDict.ContainsKey(type))
            {

                foreach (var item in Resources.LoadAll<BasePanel>(PanelPath))
                {
                    Enum.TryParse(item.name,out UIPanelType tempType);
                    if (tempType == type)
                    {
                        panel = GameObject.Instantiate(item, mCanvasTransform, false);
                        PanelsDict.Add(type, panel);
                        return panel;
                    }
                }

            }
            else panel = PanelsDict[type];
            return panel;
        }

    }
}
