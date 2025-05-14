using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using YukiFrameWork.Extension;

namespace YukiFrameWork.UI
{
    public interface IPanel
    {
        /// <summary>
        /// 面板预初始化方法。执行在OnInit方法之前。当使用OpenPanel打开面板且是该面板第一次加载时，传递的参数会同步到该方法中!
        /// <para>注意:该方法仅通过UIKit加载流(OpenPanel)等方法调用有效。使用临时面板:UIKit.ShowPanel方法时该方法不会有任何反应</para>
        /// </summary>
        /// <param name="param"></param>
        void OnPreInit(params object[] param);     
        void OnInit();
        void Enter(params object[] param);
        void Resume();
        void Pause();
        void Exit();
          
        bool IsPaused { get; }
        bool IsActive { get; }
        bool IsPanelCache { get; }
        GameObject gameObject { get; }
        IUIAnimation Animation { get; set; }
        UILevel Level { get; }
        PanelOpenType OpenType { get; }

        void SetLevel(UILevel level);     
    }
    [RequireComponent(typeof(CanvasGroup))]
    [ClassAPI("框架UI面板基类")]
    public class BasePanel : YMonoBehaviour,ISerializedFieldInfo,IPanel
    {        
        private CanvasGroup canvasGroup;
        protected new RectTransform transform;
        [HideInInspector]
        public UICustomData Data;

        private bool isPlaying => ApplicationHelper.GetRuntimeOrEditor();

        private bool IsBase => this.GetType() == typeof(BasePanel);

        [DisableIf(nameof(isPlaying)),HideIf(nameof(IsBase))]
        [LabelText("面板层级"),SerializeField]        
        protected UILevel mLevel = UILevel.Common;

        [SerializeField, DisableIf(nameof(isPlaying)),LabelText("面板的生成模式"), HideIf(nameof(IsBase))]
        [InfoBox("决定了面板能否同时打开多个")]
        protected PanelOpenType openType = PanelOpenType.Single;

        public PanelOpenType OpenType => openType;

        public CanvasGroup CanvasGroup
        {
            get
            {
                if (!canvasGroup && this)
                    canvasGroup = GetComponent<CanvasGroup>();
                return canvasGroup;
            }
        }
        public UILevel Level
        {
            get
            {              
                return mLevel;
            }           
        }       
        void IPanel.SetLevel(UILevel level)
        {          
            mLevel = level;
        }
        [DisableIf(nameof(isPlaying)), HideIf(nameof(IsBase))]
        [LabelText("面板是否缓存"),SerializeField,InfoBox("如果关闭则每次开关面板都会进行销毁生成")]
        protected bool isPanelCache = true;

        public bool IsPanelCache => isPanelCache;

        public bool IsPaused { get; private set; }

        public bool IsActive { get; private set; }

        private IUIAnimation mAnimation;

        /// <summary>
        /// 这个UI面板可用的UI动画模式
        /// </summary>
        public IUIAnimation Animation
        {
            get => mAnimation;
            set
            {
                if (value != null && mAnimation != value)
                {
                    mAnimation = value;
                    mAnimation.Panel = this;                   
                    mAnimation.OnInit();
                }
            }
        }

        [SerializeField,LabelText("是否让面板可拖拽"),HideIf(nameof(IsBase))]
        protected bool IsPanelDrag = false;

        private CoroutineTokenSource uiTokenScoure;
   
        protected override void Awake()
        {
            base.Awake();
            uiTokenScoure = CoroutineTokenSource.Create(this);
            transform = base.transform as RectTransform;
            canvasGroup = GetComponent<CanvasGroup>();         
            this.BindDragEvent(DefaultDragEvent).UnRegisterWaitGameObjectDestroy(this);
            this.BindPointerDownEvent(DefaultDragDownEvent).UnRegisterWaitGameObjectDestroy(this);
        }

        private void DefaultDragDownEvent(PointerEventData eventData)
        {
            if (!IsPanelDrag) return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(UIManager.Instance.Canvas.transform as RectTransform, Input.mousePosition, null, out var locaoPosition);

            offest = (Vector3)locaoPosition - transform.localPosition;
           
        }

        private Vector2 offest;
        private void DefaultDragEvent(PointerEventData eventData)
        {
            if (!IsPanelDrag) return;          
            RectTransformUtility.ScreenPointToLocalPointInRectangle(UIManager.Instance.Canvas.transform as RectTransform, Input.mousePosition , null, out Vector2 localPosition);
           
            float width = UIManager.I.transform.rect.width;
            float height = UIManager.I.transform.rect.height;          
            float tempWidth = width / 2 - transform.rect.width / 4;
            float tempHeight = height / 2 - transform.rect.height / 2;
            localPosition -= offest;
            localPosition.x = Mathf.Clamp(localPosition.x, -tempWidth, tempWidth);
            localPosition.y = Mathf.Clamp(localPosition.y, -tempHeight, tempHeight);
          
            transform.localPosition = localPosition;
        }
        #region UI面板生命周期    

        /// <summary>
        /// 面板预初始化方法。执行在OnInit方法之前。当使用OpenPanel打开面板且是该面板第一次加载时，传递的参数会同步到该方法中!
        /// <para>注意:该方法仅通过UIKit加载流(OpenPanel)等方法调用有效。使用临时面板:UIKit.ShowPanel方法时该方法不会有任何反应</para>
        /// </summary>
        /// <param name="param"></param>
        [MethodAPI("有参数的面板初始化")]
        public virtual void OnPreInit(params object[] param)
        {
            
        }

        [MethodAPI("面板初始化")]     
        public virtual void OnInit()
        {
            
        }


        [MethodAPI("面板打开(进入)")]
        public virtual void OnEnter(params object[] param)
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

        /// <summary>
        /// 关闭面板
        /// </summary>
        public void CloseSelf()
            => UIKit.ClosePanel(this);

        #region IPanel

        async void IPanel.Enter(params object[] param)
        {           
            //进入先将Alpha设置为1，在动画播放完成后再设置可交互
            CanvasGroup.alpha = 1;
            if (Animation != null)
            {
                Animation.OnEnter(param);
                await CoroutineTool
                    .WaitUntil(Animation.OnEnterAnimation)
                    .Token(uiTokenScoure.Token);
            }
            CanvasGroup.blocksRaycasts = true;       
            OnEnter(param);
           
            IsActive = true;
            IsPaused = false;
        }

        void IPanel.Resume()
        {
            CanvasGroup.blocksRaycasts = true;
            OnResume();
            IsPaused = false;
        }

        void IPanel.Pause()
        {
            CanvasGroup.blocksRaycasts = false;
            OnPause();
            IsPaused = true;
        }

        async void IPanel.Exit()
        {

            //退出先禁止交互，且等待动画播放后再直接把canvasGroup的alpha设置为0
            if (!transform) return;
            CanvasGroup.blocksRaycasts = false;
            if (Animation != null)
            {
                Animation.OnExit();
                await CoroutineTool
                    .WaitUntil(Animation.OnExitAnimation)
                    .Token(uiTokenScoure.Token);
            }
            CanvasGroup.alpha = 0;
                 
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
#if UNITY_EDITOR || DEBUG || DEBUG
        public virtual bool OnInspectorGUI()
        {
            return false; //默认返回False，如果需要自定义编辑器拓展则在这里写逻辑后返回True即可;
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

        SerializeFieldData ISerializedFieldInfo.Find(Func<SerializeFieldData, bool> func)
        {
            for (int i = 0; i < _fields.Count; i++)
            {
                if (func(_fields[i]))
                {
                    return _fields[i];
                }
            }

            return null;
        }      
        #endregion
    }
}
