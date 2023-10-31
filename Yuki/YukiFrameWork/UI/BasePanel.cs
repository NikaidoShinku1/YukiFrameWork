using UnityEngine;
using System;
using System.Collections.Generic;
using YukiFrameWork.MVC;
namespace YukiFrameWork.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class BasePanel : MonoBehaviour, IView
    {
        protected CanvasGroup canvasGroup;
        private IArchitecture architecture;
        protected UITools UITools;
        protected virtual void Awake()
        {
            UITools = new UITools(this);
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public virtual void Init()
        {
            
        }

        #region UI面板生命周期
        /// <summary>
        /// 进入
        /// </summary>
        public virtual void OnEnter()
        {                     
            if(canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;                      
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
        public virtual void OnExit()
        {          
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;        
        }
        #endregion

        #region 面板结合MVC架构
        public virtual void Unified_UpdateView(IModel model)
        {

        }

        public void SetArchitecture(IArchitecture architecture)
        {
            this.architecture = architecture;
        }

        public IArchitecture GetArchitecture()
        {
            return architecture;
        }
        #endregion
      

    }
}
