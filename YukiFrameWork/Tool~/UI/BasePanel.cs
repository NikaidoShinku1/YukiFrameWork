using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using YukiFrameWork.Extension;

namespace YukiFrameWork.UI
{
    public interface IPanel
    {
        void Enter();
        void Resume();
        void Pause();
        void Exit();
          
        bool IsPaused { get; }
        bool IsActive { get; }
        bool IsPanelCache { get; }
        GameObject gameObject { get; }
    }
    [RequireComponent(typeof(CanvasGroup))]
    [ClassAPI("框架UI面板基类")]
    public class BasePanel : MonoBehaviour,ISerializedFieldInfo,IPanel
    {      
        protected CanvasGroup canvasGroup;      
        [HideInInspector]
        public UICustomData Data;

        [Label("面板层级"),SerializeField,RuntimeDisabledGroup]        
        protected UILevel mLevel = UILevel.Common;

        public UILevel Level
        {
            get
            {              
                return mLevel;
            }
            set
            {
                mLevel = value; 
            }
        }
      
        [Label("面板是否缓存"),BoolanPopup("关闭缓存","开启缓存"),SerializeField,RuntimeDisabledGroup,HelperBox("如果关闭则每次开关面板都会进行销毁生成",Message.Warning)]
        protected bool isPanelCache = true;

        public bool IsPanelCache => isPanelCache;

        public bool IsPaused { get; private set; }

        public bool IsActive { get; private set; }

        protected virtual void Awake()
        {            
            canvasGroup = GetComponent<CanvasGroup>();
        }
        #region UI面板生命周期      
        [MethodAPI("面板初始化")]
        public virtual void OnInit()
        {

        }

        [MethodAPI("面板打开(进入)")]
        public virtual void OnEnter()
        {                     
              
        }

        [MethodAPI("面板暂停")]
        public virtual void OnPause()
        {
           
        }

        [MethodAPI("面板恢复")]
        public virtual void OnResume()
        {
            
        }

        [MethodAPI("面板退出")]
        public virtual void OnExit()
        {          
           
        }

        #region IPanel
        void IPanel.Enter()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();

            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
            OnEnter();
            IsActive = true;
            IsPaused = false;
        }

        void IPanel.Resume()
        {
            canvasGroup.blocksRaycasts = true;
            OnResume();
            IsPaused = false;
        }

        void IPanel.Pause()
        {
            canvasGroup.blocksRaycasts = false;
            OnPause();
            IsPaused = true;
        }

        void IPanel.Exit()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            OnExit();
            IsActive = false;
            IsPaused = false;           
        }
        #endregion

        #endregion

#if UNITY_EDITOR
        public void SaveData()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
#endif

        #region ISerializedFieldInfo
        [HideInInspector]
        [SerializeField]
        private List<SerializeFieldData> _fields = new List<SerializeFieldData>();
        void ISerializedFieldInfo.AddFieldData(SerializeFieldData data)
            => _fields.Add(data);

        void ISerializedFieldInfo.RemoveFieldData(SerializeFieldData data)
            => _fields.Remove(data);

        void ISerializedFieldInfo.ClearFieldData()
            => _fields.Clear();

        IEnumerable<SerializeFieldData> ISerializedFieldInfo.GetSerializeFields() => _fields;       
        #endregion
    }
}
