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
using System.Collections.Generic;
using XFABManager;
using System.Linq;
namespace YukiFrameWork
{

    public class LocalizationComponent : MonoBehaviour
    {
        [SerializeField]
        [LabelText("标识")]
        [InfoBox("为同步组件选择语言的唯一标识")]
#if UNITY_EDITOR
        [ValueDropdown(nameof(allKeys))]
#endif
        private string key;
#if UNITY_EDITOR
        [ValueDropdown(nameof(maskableGraphics))]
#endif
        [InfoBox("同步组件会检索包括自己在内所有的子物体内以找出符合条件的组件可选")]
        [InfoBox("请注意，默认的LocalizationComponent更新语言仅直接支持Text、Image、RawImage，" +
            "\n但组件的赋值是自由的," +
            "\n如需要拓展，比如TMP，则需要调用该组件的InitResolver方法初始化解析器!" +
            "\n如果同步的是精灵/图片且配置选择不是本地图片。则必须要给选定的对象挂载ImageLoader组件!", InfoMessageType.Warning)]
        [SerializeField]
        [LabelText("同步组件")]
        private MaskableGraphic component;

        public string Key => key;

        public MaskableGraphic Component => component;
        /// <summary>
        /// 组件解析器
        /// </summary>
        private Action<MaskableGraphic, ILocalizationData> resolver;

        private ImageLoader imageLoader;

#if UNITY_EDITOR
        private IEnumerable<string> allKeys => LocalizationManager
            .LocalizationManagers
            .SelectMany(x => x.localizationConfig_language_dict)
            .SelectMany(x => x.Value.localizations)
            .Select(x => x.Key)
            ;

        private IEnumerable<MaskableGraphic> maskableGraphics
            => GetComponentsInChildren<MaskableGraphic>();       
#endif
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
        public void InitResolver(Action<MaskableGraphic, ILocalizationData> resolver,bool refresh = false)
        {
            this.resolver = resolver;
            if(refresh)
                Update_Component(LocalizationKit.LanguageType);
        }

        public void Update_Component(Language language)
        {
            ILocalizationData data = LocalizationKit.GetContent(Key, language);
            if (data == null) return;
            MakeGraph_Reset(data);
        }

        private void MakeGraph_Reset(ILocalizationData data)
        {            
            if (resolver != null)
            {              
                resolver.Invoke(Component, data);
            }
            else
            {
                if (Component is Text text)
                    text.text = data.Context;
                else if (Component is Image image)
                {
                    if (data.Image.LocalizationImageLoadType == LocalizationImageLoadType.LocalImage)
                        image.sprite = data.Image.Icon;
                    else ImageLoaderUpdate(image);
                }
                else if (Component is RawImage rawImage)
                {
                    if (data.Image.LocalizationImageLoadType == LocalizationImageLoadType.LocalImage)
                        rawImage.texture = data.Image.Icon.texture;
                    else ImageLoaderUpdate(rawImage);
                }

            }          

            void ImageLoaderUpdate(Component image)
            {
                if (!imageLoader)
                    imageLoader = image.GetComponent<ImageLoader>();
                if (!imageLoader)
                    throw new NullReferenceException($"同步配置非本地图片，需要使用ImageLoader加载。请为指定的组件对象 {image.name} 挂载ImageLoader!");
                switch (data.Image.LocalizationImageLoadType)
                {                   
                    case LocalizationImageLoadType.ImageLoader:                       
                        imageLoader.Type = ImageLoaderType.AssetBundle;
                        imageLoader.ProjectName = data.Image.ProjectName;
                        imageLoader.AssetName = data.Image.AssetName;
                       
                        break;
                    case LocalizationImageLoadType.NetWorkImageLoader:                      
                        imageLoader.Type = ImageLoaderType.Network;
                        imageLoader.Path = data.Image.Url;
                        break;
                }
            }
        }
    }
}
