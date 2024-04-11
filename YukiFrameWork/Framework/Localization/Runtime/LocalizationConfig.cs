///=====================================================
/// - FileName:      LocalizationConfig.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   本地化配置
/// - Creation Time: 2024/4/8 13:44:56
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using Newtonsoft.Json;
using YukiFrameWork.Extension;
using System.Collections.Generic;
using System.Reflection;
#if UNITY_EDITOR
#endif
using System.Linq;
namespace YukiFrameWork
{
    public abstract class LocalizationConfigBase : ScriptableObject
    {
        protected const string groupName = "本地设置:";       

        public abstract ILocalizationData GetLocalizationData(Language language,string key);

        public abstract void Init();

#if UNITY_EDITOR
        protected enum LoadMode
        {
            Json = 0,
            Xml,           
        }
#endif
    }
  
    public abstract class LocalizationConfigBase<LocalizationData> : LocalizationConfigBase where LocalizationData : ILocalizationData,new()
    {      
        [LabelText("打开配置表导入:"),BoxGroup(groupName)]
        [JsonIgnore]
        public bool openLoadMode;      
       
        //[DictionaryDrawerSettings(KeyLabel = "标识", ValueLabel = "配置信息"), BoxGroup(groupName)]       
        [LabelText("本地数据配置:"),BoxGroup(groupName)]
        [JsonProperty]
        [ShowInInspector]
        [SerializeField]        
        public YDictionary<string, YDictionary<Language,LocalizationData>> config = new YDictionary<string, YDictionary<Language,LocalizationData>>();
        [JsonIgnore]
        [NonSerialized]
        public Dictionary<string, Dictionary<Language, LocalizationData>> runtimeConfigs = new Dictionary<string, Dictionary<Language, LocalizationData>>();      

        public override ILocalizationData GetLocalizationData(Language language,string key)
        {
            if (runtimeConfigs.TryGetValue(key, out var datas))
            {
                if (datas.TryGetValue(language, out var item))
                {                   
                    return item;
                }
            }
            return null;        
        }        

        public override void Init()
        {           
            runtimeConfigs = config
                .ToDictionary(
                key => key.Key, 
                v => v.Value
                .ToDictionary(k => k.Key
                ,v => v.Value));          
        }
#if UNITY_EDITOR      
        [Button("一键应用精灵修改(传入标识，被修改的精灵，应用选择的精灵)"),BoxGroup(groupName), PropertySpace(10)]
        void SetAllSprite(string key,Language validateLanguage, Language[] languages)
        {
            Sprite sprite = null;

            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("没有传入标识!");
                return;
            }
            if (languages.Length == 0)
            {
                Debug.LogError("没有要应用的数据! languages:Count == 0");
                return;
            }

            if (config.TryGetValue(key, out var datas))
            {
                if (datas.TryGetValue(validateLanguage, out var data))
                {
                    if (data.Sprite != null)
                        sprite = data.Sprite;
                }

                if (sprite == null)
                {
                    Debug.LogError("指定的语言图片是空的!language--- " + validateLanguage);
                    return;
                }

                for (int i = 0; i < languages.Length; i++)
                {
                    if (languages[i] == validateLanguage) continue;
                    if (datas.TryGetValue(languages[i], out var array))
                    {                      
                        array.Sprite = sprite;
                    }
                }
            }
            else
            {
                Debug.LogError("没有这个标识的数据 Key---" + key);
            }
        }       
        [PropertySpace(10),Button("将本地数据导出配置表"), BoxGroup(groupName)]
        void Re_ImportFileInInspector(string fileName = "LocalizationConfigs",string assetPath = "Assets/Localization",LoadMode mode = LoadMode.Json)
        {
            if (config.Count == 0)
            {
                Debug.LogError("没有添加配置无法导出!", this);
                return;
            }

            LocalizationModel[] models = new LocalizationModel[config.Keys.Count];

            string[] keys = config.Keys.ToArray();

            for (int i = 0; i < models.Length; i++)
            {
                models[i] = new LocalizationModel()
                {
                    Key = keys[i]
                };

                for (Language j = 0; j < Language.Vietnamese; j++)
                {
                    if (config[keys[i]].TryGetValue(j, out LocalizationData data))
                    {
                        models[i].SetValue(j.ToString(),data.Context);
                    }
                }
            }

            switch (mode)
            {
                case LoadMode.Json:
                    SerializationTool.SerializedObject(models).CreateFileStream(assetPath,fileName,".json");
                    break;
                case LoadMode.Xml:
                    SerializationTool.XmlSerializedObject(models).CreateFileStream(assetPath, fileName, ".xml");
                    break;             
            }

        }
        [LabelText("配置表类型选择"), ShowIf(nameof(openLoadMode)),BoxGroup("配置设置")]
        [SerializeField]LoadMode mode;        
        [LabelText("传入配置文件"), ShowIf(nameof(openLoadMode)),BoxGroup("配置设置")]
        [SerializeField]TextAsset textAsset;
        [LabelText("导入前清空原本的数据"),ShowIf(nameof(openLoadMode)), BoxGroup("配置设置"),SerializeField]
        bool isClearConfig = true;
        [Button("配置表导入", ButtonHeight = 30), PropertySpace(10), ShowIf(nameof(openLoadMode)), BoxGroup("配置设置")]
        void Import()
        {
            LocalizationModel[] models = null;

            if (textAsset == null)
            {
                Debug.LogError("配置文件为空，请拖拽添加! textAsset is null");
                return;
            }

            if (mode == LoadMode.Json)
                models = SerializationTool.DeserializedObject<LocalizationModel[]>(textAsset.text);

            else
                models = SerializationTool.XmlDeserializedObject<LocalizationModel[]>(textAsset.text);

            if (isClearConfig)
                config.Clear();

            foreach (var model in models)
            {
                if (!config.TryGetValue(model.Key, out var dict))
                {
                    dict = new YDictionary<Language, LocalizationData>();
                    config[model.Key] = dict;
                }
                for (Language i = 0; i < Language.Vietnamese; i++)
                {
                    PropertyInfo info = typeof(LocalizationModel).GetProperty(i.ToString());
                    if (info == null) continue;

                    string context = info.GetValue(model)?.ToString();
                    if (string.IsNullOrEmpty(context))
                        continue;

                    dict[i] = new LocalizationData()
                    {
                        Context = context
                    };
                }
            }

        }       
#endif
    }
    [CreateAssetMenu(fileName = "LocalizationConfig", menuName = "YukiFrameWork/LocalizationConfig")]
    public class LocalizationConfig : LocalizationConfigBase<LocalizationData>
    {

    }
}
