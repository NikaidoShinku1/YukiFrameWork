using UnityEngine;
using System;
using System.Collections.Generic;
namespace YukiFrameWork.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class BasePanel : MonoBehaviour
    {
        protected CanvasGroup canvasGroup;
        public PanelManager PanelManager { get; private set; }
        public UIManager UIManager { get; private set; }
        public UITools UITools { get; private set; }
        public UIPanelType PanelType { get; private set; }
        public Stack<Action> Actions { get; private set; }     
        
        public bool IsInit { get;private set; } = false;
        public virtual void Init(PanelManager PanelManager,UIManager manager,UITools UITools,UIPanelType type)
        {
            if (IsInit) return;
            this.PanelManager = PanelManager;
            this.UIManager = manager;
            this.UITools = UITools;
            Actions = new Stack<Action>();
            this.PanelType = type;
            IsInit = true;
        }

        #region UI面板生命周期
        /// <summary>
        /// 进入
        /// </summary>
        public virtual void OnEnter(Action action = null)
        {                     
            if(canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
            Actions.Push(action);
            
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public virtual void OnPause()
        {
            canvasGroup.blocksRaycasts = false;
        }

        /// <summary>
        /// 恢复
        /// </summary>
        public virtual void OnResume()
        {
            canvasGroup.blocksRaycasts = true;
        }

        /// <summary>
        /// 退出
        /// </summary>
        public virtual void OnExit(bool isBack = true)
        {          
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            if(isBack) Actions.Pop()?.Invoke();
        }
        #endregion

       
    }
}
