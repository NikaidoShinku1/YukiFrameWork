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
        private readonly BindablePropertyPlayerPrefsByInteger defaultIntger;
        public DefaultLocalizationSerializer()
        {
            this.defaultIntger = new BindablePropertyPlayerPrefsByInteger(LocalizationKit.DEFAULTLANGUAGEBYYUKIFRAMEWORK_KEY, 0); 
        }

        public Language DeSerialize()
        {
            return (Language)defaultIntger.Value;
        }

        public void Serialize(Language language)
        {
            int value = (int)language;
            defaultIntger.Value = value;
        }
    }
    /// <summary>
    /// 框架运行时本地化管理器
    /// </summary>
    public static class LocalizationKit
	{    
        private static ILocalizationLoader loader;
        private static Dictionary<Language,Dictionary<string,ILocalizationData>> all_localizations = new Dictionary<Language, Dictionary<string, ILocalizationData>>();
        private static BindablePropertyPlayerPrefsByBoolan firstPlayMode;
        internal const string DEFAULTLANGUAGEBYYUKIFRAMEWORK_KEY = "DEFAULTLANGUAGEBYYUKIFRAMEWORK_KEY";
        internal const string DEFAULTFIRSTPLAYMODBYLOCALIZATION_KEY = nameof(DEFAULTFIRSTPLAYMODBYLOCALIZATION_KEY);

        /// <summary>
        /// 本地化套件的序列化器
        /// </summary>
        public static ILocalizationSerializer Serializer { get; private set; }
           
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
        /// 单独添加本地语言数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="localizationData"></param>
        /// <param name="language"></param>
        public static void AddLocalizationData<T>(T localizationData,Language language) where T : ILocalizationData
        {
            if (!all_localizations.TryGetValue(language, out var dict))
            {
                dict = new Dictionary<string, ILocalizationData>();
                all_localizations[language] = dict;
            }           
            dict.Add(localizationData.Key, localizationData);
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
        /// <summary>
        /// 初始化本地化套件以赋值配置加载器
        /// </summary>       
        public static void Init(string projectName,Language language = Language.SimplifiedChinese)
        {
            Init(new ABManagerLocalizationLoader(projectName),language);
        }

        /// <summary>
        /// 初始化本地化套件以赋值配置加载器
        /// </summary>
        /// <param name="loader"></param>
        public static void Init(ILocalizationLoader loader, Language language = Language.SimplifiedChinese)
        {
            Init(loader,new DefaultLocalizationSerializer(),language);
        }

        /// <summary>
        /// 初始化本地化套件以赋值配置加载器,可选自定义序列化器
        /// </summary>       
        public static void Init(string projectName,ILocalizationSerializer serializer, Language language = Language.SimplifiedChinese)
        {
            Init(new ABManagerLocalizationLoader(projectName),serializer,language);
        }

        /// <summary>
        /// 初始化本地化套件以赋值配置加载器,可选自定义序列化器
        /// </summary>
        /// <param name="loader"></param>
        public static void Init(ILocalizationLoader loader, ILocalizationSerializer serializer, Language language = Language.SimplifiedChinese)
        {
            Serializer = serializer;           
            LocalizationKit.loader = loader;
            firstPlayMode = new BindablePropertyPlayerPrefsByBoolan(DEFAULTFIRSTPLAYMODBYLOCALIZATION_KEY,false);
            if (!firstPlayMode)
            {
                Debug.Log("第一次进入项目");
                languageType = language;
                firstPlayMode.Value = true;
            }
            else
            {
                languageType = Serializer.DeSerialize();
            }
        }

        internal static void LoadLocalizationManagerConfigInternal(LocalizationManager configManager, bool isLoaderLoad)
        {     
            var datas = configManager.localizationConfig_language_dict;
            foreach (var data in datas)
            {
                foreach (var item in data.Value.localizations)
                {
                    AddLocalizationData(item,data.Key);
                }
            }
            
            if(isLoaderLoad)
            loader.UnLoad(configManager);
        }

        /// <summary>
        /// 判断该语言是否存在(至少在这个语言下有一条LocalizationData)
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public static bool LanguageContains(Language language)
            => all_localizations.ContainsKey(language);

        /// <summary>
        /// 获取目前已经拥有数据的所有语言
        /// </summary>
        /// <returns></returns>
        public static Language[] GetLanguages() => all_localizations.Keys.ToArray();

        /// <summary>
        /// 判断目前的配表在运行中,有没有指定语言的信息
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        public static bool IsContainsLanguage(Language language)
        {
            return all_localizations.ContainsKey(language);
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
        [Obsolete("过时的方法，请使用GetContent(string key, Language language)不再需要传递ManagerKey")]
        public static ILocalizationData GetContent(string managerKey,string key, Language language)
        {
            return GetContentByKey(key, language);
        }
        [Obsolete("过时的方法，请使用GetContent(string key)不再需要传递ManagerKey")]
        public static ILocalizationData GetContent(string managerKey, string key)
        {
            return GetContentByKey(key, LanguageType);
        }
        
        public static ILocalizationData GetContent(string key, Language language)
        {
            return GetContentByKey(key, language);
        }

        public static ILocalizationData GetContent(string key)
        {
            return GetContentByKey(key, LanguageType);
        }


        internal static ILocalizationData GetContentByKey(string key, Language language)
        {          
            if (!all_localizations.TryGetValue(language, out var localizations))
            {                
                return default;
            }

            ILocalizationData data = null;

            if (!localizations.TryGetValue(key, out data))
            {
                Debug.LogError("指定标识不存在，请检查是否有为该语言添加指定的数据: Key:" + key);
                return default;
            }
            return data;
        }      
    }
}
