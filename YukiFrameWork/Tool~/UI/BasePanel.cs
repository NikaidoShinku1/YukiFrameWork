using System;
using System.Collections.Generic;
using UnityEngine;
using YukiFrameWork.Extension;

namespace YukiFrameWork.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    [ClassAPI("框架UI面板基类")]
    public class BasePanel : MonoBehaviour,ISerializedFieldInfo
    {      
        protected CanvasGroup canvasGroup;      
        [HideInInspector]
        public UICustomData Data;

        [SerializeField]        
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
            if(canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;                      
        }

        [MethodAPI("面板暂停")]
        public virtual void OnPause()
        {
            canvasGroup.blocksRaycasts = false;
        }

        [MethodAPI("面板恢复")]
        public virtual void OnResume()
        {
            canvasGroup.blocksRaycasts = true;
        }

        [MethodAPI("面板退出")]
        public virtual void OnExit()
        {          
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;        
        }
        #endregion


        #region ISerializedFieldInfo
        [HideInInspector]
        [SerializeField]
        private List<SerializeFieldData> _fields = new List<SerializeFieldData>();
        public void AddFieldData(SerializeFieldData data)
            => _fields.Add(data);

        public void RemoveFieldData(SerializeFieldData data)
            => _fields.Remove(data);

        public void ClearFieldData()
            => _fields.Clear();

        public IEnumerable<SerializeFieldData> GetSerializeFields() => _fields;
        #endregion
    }
}
