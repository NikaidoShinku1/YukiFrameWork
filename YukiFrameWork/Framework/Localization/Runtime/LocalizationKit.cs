﻿///=====================================================
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
namespace YukiFrameWork
{
    /// <summary>
    /// 框架运行时本地化管理器
    /// </summary>
    public class LocalizationKit : Singleton<LocalizationKit>
	{
        private LocalizationKit() { }      
        private static Dictionary<string,LocalizationConfigBase> dependConfigs = new Dictionary<string, LocalizationConfigBase>();
        private const string DEFAULTLANGUAGEBYYUKIFRAMEWORK_KEY = "DEFAULTLANGUAGEBYYUKIFRAMEWORK_KEY";

        /// <summary>
        /// 本地序列化语言器，用于将默认的语言持久化保存,与反序列化器成对,如果自行添加则需要两个一起都实现
        /// </summary>
        public static event Action<Language> OnSerializeLanguage = language =>
        {
            int value = (int)language;
            PlayerPrefs.SetInt(DEFAULTLANGUAGEBYYUKIFRAMEWORK_KEY, value);         
        };

        /// <summary>
        /// 本地反序列化语言器，用于启动时加载默认的语言，与序列化器成对,如果自行添加则需要两个一起都实现
        /// </summary>
        public static event Func<Language> OnDeSerializeLanguage = () =>
        {
            return Enum.GetValues(typeof(Language)).Cast<Language>().FirstOrDefault(x => (int)x == PlayerPrefs.GetInt(DEFAULTLANGUAGEBYYUKIFRAMEWORK_KEY));
        };

        /// <summary>
        /// 得到框架的配置文件
        /// </summary>
        private FrameworkConfigInfo framework;

        private static bool Isinited = false;
        /// <summary>
        /// 注册语言改变时的事件
        /// </summary>
        private EasyEvent<Language> Update_Language = new EasyEvent<Language>();

        private Language languageType;       
        /// <summary>
        /// 框架当前的语言类型设置
        /// </summary>
        public static Language LanguageType
        {
            get => I.languageType;
            set
            {              
                if (I.languageType != value)
                {
#if UNITY_EDITOR
                    I.framework.defaultLanguage = value;
#endif
                    I.languageType = value;
                    OnSerializeLanguage(LanguageType);
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
            return I.Update_Language.RegisterEvent(action);
        }

        /// <summary>
        /// 注销本地化套件修改语言时的事件(注销事件可以在初始化之前进行)
        /// </summary>
        /// <param name="action"></param>
        public static void UnRegisterLanguageEvent(Action<Language> action)
        {
            I.Update_Language.UnRegister(action);
        }

        /// <summary>
        /// 发送修改语言的事件
        /// </summary>
        public static void OnLanguageValueChanged()
        {
            if (!Isinited) return;
            I.Update_Language.SendEvent(LanguageType);
        }       
        public override void OnInit()
        {            
            if (Isinited) return;      
            FrameworkConfigInfo info = Resources.Load<FrameworkConfigInfo>(nameof(FrameworkConfigInfo));

            if (info == null)
            {
                Debug.LogError("框架本体配置丢失!请检查Resources文件夹下是否生成了FrameworkConfigInfo");
                return;
            }        
            framework = info;
            Isinited = true;
            
            dependConfigs = info.dependConfigs.ToDictionary(key => key.Key,value => value.Value);

            foreach (var key in dependConfigs.Keys)
            {
                LocalizationConfigBase depend = dependConfigs[key];
                if (depend == null)
                {
                    Debug.LogWarning("该id对应的配置不存在，请检查! id---- " + key);
                    continue;
                }
                depend.Init();
            }
#if UNITY_EDITOR
             languageType = info.defaultLanguage;
#else
             languageType = OnDeSerializeLanguage();
#endif

        }

        /// <summary>
        /// 手动添加配置
        /// </summary>
        /// <param name="key"></param>
        /// <param name="config"></param>
        public static void AddDependConfig(string key, LocalizationConfigBase config)
        {
            try
            {
                dependConfigs.Add(key, config);
                config.Init();
            }
            catch(Exception ex) 
            {
                LogKit.Exception(new Exception(string.Format("该本地配置已经存在!LocalizationConfig Key -- {0} --- {1}", key, ex)));
            }
        }

        public static ILocalizationData GetContent(string configKey,string key, Language language)
        {
            return I.GetContentByKey(configKey, key, language);
        }

        public static ILocalizationData GetContent(string configKey, string key)
        {
            return I.GetContentByKey(configKey, key, LanguageType);
        }

        internal ILocalizationData GetContentByKey(string configKey,string key, Language language)
        {
            if (!Isinited)
            {
                throw new Exception("没有对LocalizationKit进行初始化!请调用一次LocalizationKit.Init()方法!");
            }

            if (!dependConfigs.TryGetValue(configKey, out var config))
            {
                Debug.LogError("依赖的配置文件不存在请于左上角打开YukiFrameWork/LocalConfiguration查看是否进行添加，configKey--- " + configKey);
                return default;
            }

            ILocalizationData data = config.GetLocalizationData(language, key);

            return data;
        }
         

        public override void OnDestroy()
        {
            base.OnDestroy();
            Resources.UnloadAsset(framework);
            framework = null;
            dependConfigs.Clear();          
        }
    }
}
