using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using YukiFrameWork.Extension;

namespace YukiFrameWork.Events
{
    //注册标识类型
    public enum RegisterType
    {       
        //字符串
        String,
        //枚举
        Enum
    }
    [Serializable]
    public class EventCenter
    {       
        public string name;
        public Enum mEnum;

        #region EnumInfo
        /// <summary>
        /// 仅作用于mEnum的选择
        /// </summary>
        [HideInInspector]
        public int enumIndex = 0; 

        [HideInInspector]
        public List<string> mEnumInfos = new List<string>();

        [HideInInspector]
        [SerializeField] private Type mEnumType;

#if UNITY_EDITOR
        /// <summary>
        /// 仅对标识设置为枚举时生效
        /// </summary>
        public Type EnumType
        {
            get
            {                           
                return mEnumType;
            }
            set
            {
                mEnumType = value;
            }
        }
#endif
#endregion
        public UnityEvent<EventArgs> mEvent = new UnityEvent<EventArgs>();
    }   
}
