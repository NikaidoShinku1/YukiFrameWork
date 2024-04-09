///=====================================================
/// - FileName:      LocalizationComponent.cs
/// - NameSpace:     YukiFrameWork.Example
/// - Description:   本地化挂载组件
/// - Creation Time: 2024/4/9 15:54:07
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using UnityEngine.Events;
namespace YukiFrameWork
{
    [Serializable]
    public class LocalizationComponentItem
    {       
        [LabelText("标识:")]
        public string key;
        [LabelText("设置的组件:")]
        public Component component;

        [LabelText("事件接收器")]
        public bool eventReceiver;
        [SerializeField,LabelText("文本事件接收"),ShowIf(nameof(eventReceiver))]
        public UnityEvent<string> stringReceiver;
        [SerializeField, LabelText("精灵事件接收"), ShowIf(nameof(eventReceiver))]
        public UnityEvent<Sprite> spriteReceiver;

    }
	public class LocalizationComponent : MonoBehaviour
	{
        [SerializeField] 
        private LocalizationComponentItem[] items;

        /// <summary>
        /// 组件解析器
        /// </summary>
        private Action<Component, ILocalizationData> resolver;
     
        private void OnEnable()
        {            
            LocalizationKit.RegisterLanguageEvent(Update_Component);
            Update_Component(LocalizationKit.LanguageType);
        }

        private void OnDisable()
        {
            LocalizationKit.UnRegisterLanguageEvent(Update_Component);
        }

        private void OnDestroy()
        {
            LocalizationKit.UnRegisterLanguageEvent(Update_Component);
        }

        /// <summary>
        /// 初始化解析器,用于自定义组件的赋值
        /// </summary>
        /// <param name="resolver"></param>
        public void InitResolver(Action<Component, ILocalizationData> resolver)
        {
            this.resolver = resolver;
        }

        public void Update_Component(Language language)
        {
            foreach (var item in items)
            {
                var data = LocalizationKit.GetContent(item.key,language);
                if (data == null)
                    continue;                
                if (resolver != null)
                {
                    resolver.Invoke(item.component, data);
                }
                else
                {
                    if (item.component is Text text)
                    {
                        text.text = data.Content;
                    }
                    else if (item.component is Image image)
                    {
                        image.sprite = data.Sprite;
                    }
                }
                if (item.eventReceiver)
                {
                    item.stringReceiver?.Invoke(data.Content);
                    item.spriteReceiver?.Invoke(data.Sprite);
                }
            }
        }
    }
}
