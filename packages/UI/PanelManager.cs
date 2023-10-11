using System;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork.UI
{
    public class PanelManager
    {        
        private readonly UIManager UIManager;
        private static readonly Stack<BasePanel> panelStack = new Stack<BasePanel>();
        public PanelManager()
        {          
            UIManager = new UIManager();         
        }
        public void AddPanels(UIPanelType type, BasePanel panel)
        {
            UIManager.AddPanels(type, panel);         
        }

        /// <summary>
        /// 打开面板
        /// </summary>
        /// <param name="type">面板类型</param>
        public BasePanel PushPanel(UIPanelType type,Action action = null)
        {
            return OnPushPanel(type, action);
        }

        private BasePanel OnPushPanel(UIPanelType type, Action action = null)
        {
            if (panelStack.Count > 0)
            {
                BasePanel tempPanel = panelStack.Peek();
                tempPanel.OnPause();
            }
            BasePanel panel = UIManager.GetPanel(type);
            panel.Init(this, UIManager, new UITools(panel), type);
            panel.OnEnter(action);
            panelStack.Push(panel);
            return panel;
        }

        /// <summary>
        /// 关闭面板
        /// </summary>
        public void PopPanel(bool isDestroy = false)
        {
            if (!isDestroy) Inactivation();
            else DestroyPanel();
        }

        /// <summary>
        /// 面板隐藏
        /// </summary>
        private void Inactivation()
        {
            if (panelStack.Count > 0)
            {
                BasePanel tempPanel = panelStack.Pop();
                tempPanel.OnExit();
            }

            if (panelStack.Count > 0)
            {
                BasePanel panel = panelStack.Peek();
                panel.OnResume();
            }
        }

        /// <summary>
        /// 销毁面板
        /// </summary>
        private void DestroyPanel()
        {
            if (panelStack.Count > 0)
            {
                BasePanel tempPanel = panelStack.Pop();
                tempPanel.OnExit();
                UnityEngine.Object.Destroy(tempPanel.gameObject);

                UIManager.PanelsDict.Remove(tempPanel.PanelType);
            }

            if (panelStack.Count > 0)
            {
                BasePanel panel = panelStack.Peek();
                panel.OnResume();
            }
        }

        public void ClearPanel()
        {
            panelStack.Clear();
            UIManager.Clear();
        }

       
    }
}
