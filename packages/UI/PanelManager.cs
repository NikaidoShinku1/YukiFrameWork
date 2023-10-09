using System;
using System.Collections.Generic;

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
        public void PushPanel(UIPanelType type,Action action = null)
        {
            if (panelStack.Count > 0)
            {
                BasePanel tempPanel = panelStack.Peek();
                tempPanel.OnPause();
            }
            BasePanel panel = UIManager.GetPanel(type);
            if(!panel.IsInit)
            panel.Init(this, UIManager, new UITools(panel));
            panel.OnEnter(action);
            panelStack.Push(panel);
        }

        /// <summary>
        /// 关闭面板
        /// </summary>
        public void PopPanel()
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

       

       
    }
}
