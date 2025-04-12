///=====================================================
/// - FileName:      LocalizationKit.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   框架本地化管理器
/// - Creation Time: 2024/4/5 13:10:43
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using XFABManager;
using System.Collections;
namespace YukiFrameWork
{
    public interface ILocalizationLoader : IResLoader<LocalizationManager>
    {

    }

    public class ABManagerLocalizationLoader : ILocalizationLoader
    {
        private readonly string projectName;
        public ABManagerLocalizationLoader(string projectName)
            => this.projectName = projectName;
        public TItem Load<TItem>(string name) where TItem : LocalizationManager
        {
            return AssetBundleManager.LoadAsset<TItem>(projectName, name);
        }

        public async void LoadAsync<TItem>(string name, Action<TItem> onCompleted) where TItem : LocalizationManager
        {
            var item = await AssetBundleManager.LoadAssetAsync<TItem>(projectName, name);
            onCompleted?.Invoke(item);
        }

        public void UnLoad(LocalizationManager item)
        {
            AssetBundleManager.UnloadAsset(item);
        }
    }

    /// <summary>
    /// 语言本地化保存序列化器。该接口可以序列化/反序列化当前的语言。持久化保存
    /// </summary>
    public interface ILocalizationSerializer
    {
        void Serialize(Language language);
        Language DeSerialize();
    }
    public class DefaultLocalizationSerializer : ILocalizationSerializer
    {
        public Language DeSerialize()
        {
            return Enum
                .GetValues(typeof(Language))
                .Cast<Language>()
                .FirstOrDefault(x => (int)x == PlayerPrefs.GetInt(LocalizationKit.DEFAULTLANGUAGEBYYUKIFRAMEWORK_KEY));
        }

        public void Serialize(Language language)
        {
            int value = (int)language;
            PlayerPrefs.SetInt(LocalizationKit.DEFAULTLANGUAGEBYYUKIFRAMEWORK_KEY, value);
        }
    }
    /// <summary>
    /// 框架运行时本地化管理器
    /// </summary>
    public static class LocalizationKit
	{    
        private static ILocalizationLoader loader;
        private static Dictionary<string, Dictionary<Language, ILocalizationData[]>> localizationManagers = new Dictionary<string, Dictionary<Language, ILocalizationData[]>>();
        internal const string DEFAULTLANGUAGEBYYUKIFRAMEWORK_KEY = "DEFAULTLANGUAGEBYYUKIFRAMEWORK_KEY";

        /// <summary>
        /// 本地化套件的序列化器
        /// </summary>
        public static ILocalizationSerializer Serializer { get; set; } = new DefaultLocalizationSerializer();
           
        /// <summary>
        /// 注册语言改变时的事件
        /// </summary>
        private static EasyEvent<Language> Update_Language = new EasyEvent<Language>();

        private static Language languageType;       
        /// <summary>
        /// 框架当前的语言类型设置
        /// </summary>
        public static Language LanguageType
        {
            get => languageType;
            set
            {              
                if (languageType != value)
                {
                    languageType = value;
                    Serializer.Serialize(languageType);
                    OnLanguageValueChanged();
                }
            }
        }

        /// <summary>
        /// 注册本地化套件修改语言时的事件(注册事件可以在初始化之前进行)
        /// </summary>
        /// <param name="action">事件注册</param>
        /// <returns></returns>
        public static IUnRegister RegisterLanguageEvent(Action<Language> action)
        {
            return Update_Language.RegisterEvent(action);
        }

        /// <summary>
        /// 注销本地化套件修改语言时的事件(注销事件可以在初始化之前进行)
        /// </summary>
        /// <param name="action"></param>
        public static void UnRegisterLanguageEvent(Action<Language> action)
        {
            Update_Language.UnRegister(action);
        }

        /// <summary>
        /// 发送修改语言的事件
        /// </summary>
        public static void OnLanguageValueChanged()
        {
            Update_Language.SendEvent(LanguageType);
        }       
        public static void Init(string projectName)
        {
            Init(new ABManagerLocalizationLoader(projectName));
        }

        public static void Init(ILocalizationLoader loader)
        {          
            LocalizationKit.loader = loader;
            languageType = Serializer.DeSerialize();
        }

        internal static void LoadLocalizationManagerConfigInternal(LocalizationManager configManager, bool isLoaderLoad)
        {
            string key = configManager.managerKey;
            if (localizationManagers.ContainsKey(key))
                throw new Exception("该本地配置已存在! Config Key:" + key);
            var datas = configManager.localizationConfig_language_dict;
            localizationManagers[key] = configManager.localizationConfig_language_dict
                .ToDictionary(x => x.Key,x => x.Value.LocalizationDatas);
            
            if(isLoaderLoad)
            loader.UnLoad(configManager);
        }
        

        /// <summary>
        /// 手动添加配置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="config"></param>    
        public static void LoadLocalizationManagerConfig(LocalizationManager configManager)
        {
            LoadLocalizationManagerConfigInternal(configManager, false);
        }
           
        public static void LoadLocalizationManagerConfig(string path)
            => LoadLocalizationManagerConfigInternal(loader.Load<LocalizationManager>(path), true);

        [DisableEnumeratorWarning]     
        public static IEnumerator LoadLocalizationManagerConfigAsync(string path)
        {
            bool completed = false;
            loader.LoadAsync<LocalizationManager>(path, config =>
            {
                LoadLocalizationManagerConfigInternal(config, true);
                completed = true;
            });
            yield return CoroutineTool.WaitUntil(() => completed);
        }

        public static ILocalizationData GetContent(string managerKey,string key, Language language)
        {
            return GetContentByKey(managerKey, key, language);
        }

        public static ILocalizationData GetContent(string managerKey, string key)
        {
            return GetContentByKey(managerKey, key, LanguageType);
        }

        internal static ILocalizationData GetContentByKey(string managerKey,string key, Language language)
        {          
            if (!localizationManagers.TryGetValue(managerKey, out var configManager))
            {
                Debug.LogError("没有添加指定标识的本地化管理器资源，managerKey--- " + managerKey);
                return default;
            }

            ILocalizationData data = null;

            if (configManager.TryGetValue(language, out var config))
            {
                for (int i = 0; i < config.Length; i++)
                {
                    if (config[i].Key == key)
                    {
                        data = config[i];
                        break;
                    }
                }
            }                  

            return data;
        }      
    }
}
