using UnityEngine;
namespace YukiFrameWork.UI
{
    public class UIDisplay
    {
        public UIManager UIManager { get;private set; }
        public UIDisplay(UIManager uIManager)
        {          
            this.UIManager = uIManager;
        }
        public void AddPanels(UIPanelType @enum, BasePanel panel)
        {
            UIManager.AddPanels(@enum, panel);         
        }

        /// <summary>
        /// 打开面板
        /// </summary>
        /// <param name="enum">面板类型</param>
        public void PushPanel(UIPanelType @enum)
        {
            if (UIManager.PanelStack.Count > 0)
            {
                BasePanel tempPanel = UIManager.PanelStack.Peek();
                tempPanel.OnPause();
            }
            BasePanel panel = GetPanel(@enum);
            panel.OnEnter();
            UIManager.PanelStack.Push(panel);
        }

        /// <summary>
        /// 关闭面板
        /// </summary>
        public void PopPanel()
        {
            if (UIManager.PanelStack.Count > 0)
            {
                BasePanel tempPanel = UIManager.PanelStack.Pop();
                tempPanel.OnExit();
            }

            if (UIManager.PanelStack.Count > 0)
            {
                BasePanel panel = UIManager.PanelStack.Peek();
                panel.OnResume();
            }
        }

       

        private BasePanel GetPanel(UIPanelType @enum)
        {
            BasePanel panel = null;
            if (UIManager.PanelsDict.TryGetValue(@enum, out panel))
            {

            }
            if (panel == null)
            {
                Debug.LogError("当前类型不存在面板请尝试添加");
                return null;
            }
            return panel;
        }
    }
}
