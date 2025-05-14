///=====================================================
/// - FileName:      LocalizationConfigData.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/8 22:05:21
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using YukiFrameWork.Extension;
using XFABManager;
using Newtonsoft.Json.Linq;
namespace YukiFrameWork
{
    public enum LocalizationImageLoadType
    {
        [LabelText("本地图片")]
        LocalImage,
        [LabelText("通过ImageLoader AssetBundle加载")]
        ImageLoader,
        [LabelText("通过ImageLoader Network加载")]
        NetWorkImageLoader,
       
    }
    [Serializable]
    public class ImageData
    {
        [LabelText("图片加载方式")]
        [InfoBox("除本地图片外，另外两个选择在通过使用LocalizationComponent组件同步时，所关联的图片组件必须挂载ImageLoader组件,本地化组件会自动同步!")]
        public LocalizationImageLoadType LocalizationImageLoadType;
        [ShowIf(nameof(LocalizationImageLoadType),LocalizationImageLoadType.NetWorkImageLoader)]
        public string Url;
        [ShowIf(nameof(LocalizationImageLoadType), LocalizationImageLoadType.ImageLoader)]
        public string ProjectName;
        [ShowIf(nameof(LocalizationImageLoadType), LocalizationImageLoadType.ImageLoader)]
        public string AssetName;
        [ShowIf(nameof(LocalizationImageLoadType), LocalizationImageLoadType.LocalImage)]
#if UNITY_EDITOR
        [CustomValueDrawer(nameof(DrawPreview))]
#endif
        public Sprite Icon;

#if UNITY_EDITOR
        private void DrawPreview()
        {

            GUILayout.BeginHorizontal();

            GUILayout.Label("图标");
            Icon = (Sprite)UnityEditor.EditorGUILayout.ObjectField(this.Icon, typeof(Sprite), true, GUILayout.Width(50), GUILayout.Height(50));
            GUILayout.EndHorizontal();
        }
#endif
    }
    /// <summary>
    /// 本地数据接口
    /// </summary>
    public interface ILocalizationData
    {
        string Key { get; }
        /// <summary>
        /// 文本内容
        /// </summary>
		string Context { get; }
        /// <summary>
        /// 精灵数据
        /// </summary>
		ImageData Image { get; }     
        
        /// <summary>
        /// 备注
        /// </summary>
        string Notes { get; }
    }

    [Serializable]

    public class LocalizationData : ILocalizationData
    {
        [SerializeField,JsonProperty,LabelText("唯一标识")]
        [InfoBox("对于不同的语言，应保持标识始终唯一")]
        internal string key;
        public string Key => key;
        [TextArea, JsonProperty]
        [SerializeField]
        internal string context;     
        public string Context => context;
        [SerializeField]
        internal ImageData image = new ImageData();     
        public ImageData Image => image;

        [LabelText("备注")]
        internal string notes;
        public string Notes => notes;
    }    
}
