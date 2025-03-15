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
using System.Linq;
using UnityEngine.U2D;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor.Callbacks;
using UnityEditor;
#endif
namespace YukiFrameWork
{
    public abstract class LocalizationConfigBase : ScriptableObject
    {
        protected const string groupName = "本地设置:";       

        public abstract ILocalizationData GetLocalizationData(Language language,string key);

        public abstract void Init();

      
        public abstract string[] ConfigKeys { get; }     
    }
  
    public abstract class LocalizationConfigBase<LocalizationData> : LocalizationConfigBase, IExcelSyncScriptableObject where LocalizationData : ILocalizationData,new()
    {      
        [LabelText("打开配置表导入:"),BoxGroup(groupName)]
        [JsonIgnore]
        public bool openLoadMode;      
       
        //[DictionaryDrawerSettings(KeyLabel = "标识", ValueLabel = "配置信息"), BoxGroup(groupName)]       
        [LabelText("本地数据配置:"),BoxGroup(groupName)]
        [JsonProperty]
        [ShowInInspector]
        [DictionaryDrawerSettings(KeyLabel = "唯一标识",DisplayMode = DictionaryDisplayOptions.CollapsedFoldout),Searchable]       
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

        public override string[] ConfigKeys => config.Keys.ToArray();
        public IList Array => config.ToList();
        public Type ImportType => typeof(KeyValuePair<string, YDictionary<Language, LocalizationData>>);

        public bool ScriptableObjectConfigImport => true;
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
        void Re_ImportFileInInspector(string fileName = "LocalizationConfigs",string assetPath = "Assets/Localization")
        {
            if (config.Count == 0)
            {
                Debug.LogError("没有添加配置无法导出!", this);
                return;
            }

            YDictionary<Language, List<LocalizationModel>> mModels = new YDictionary<Language, List<LocalizationModel>>();

            for (Language i = 0; i <= Language.Vietnamese; i++)
            {
                mModels[i] = new List<LocalizationModel>();
            }

            string[] keys = config.Keys.ToArray();

            for (int i = 0; i < keys.Length; i++)
            {             
                for (Language j = 0; j <= Language.Vietnamese; j++)
                {
                    if (config[keys[i]].TryGetValue(j, out LocalizationData data))
                    {
                        mModels[j].Add(new LocalizationModel()
                        {
                            Key = keys[i],
                            Context = data.Context
                        });
                    }
                }
            }            
            SerializationTool.SerializedObject(mModels).CreateFileStream(assetPath, fileName, ".json");     
        }      
        [LabelText("传入配置文件"), ShowIf(nameof(openLoadMode)),BoxGroup("配置设置")]
        [SerializeField]TextAsset textAsset;
        [LabelText("导入前清空原本的数据"),ShowIf(nameof(openLoadMode)), BoxGroup("配置设置"),SerializeField]
        bool isClearConfig = true;

        [InfoBox("注意:LocalizationModel中具有Sprite以及SpriteAtlas分别代表图集以及精灵的路径属性，在配置表中配置好路径后，导入配置表会自动设置好精灵(路径应从Assets开始,也必须持有后缀例如.asset)")]  
        [Button("Json配置表导入", ButtonHeight = 30), PropertySpace(10), ShowIf(nameof(openLoadMode)), BoxGroup("配置设置")]
        void Import()
        {
            YDictionary<Language, LocalizationModel[]> modelDicts = null;

            if (textAsset == null)
            {
                Debug.LogError("配置文件为空，请拖拽添加! textAsset is null");
                return;
            }
            
            modelDicts = SerializationTool.DeserializedObject<YDictionary<Language, LocalizationModel[]>>(textAsset.text);
           
            if (isClearConfig)
                config.Clear();

            foreach (var model in modelDicts)
            {
                Language language = model.Key;
                LocalizationModel[] ms = model.Value;

                foreach (var m in ms)
                {
                    if (!config.TryGetValue(m.Key, out var dict))
                    {
                        dict = new YDictionary<Language, LocalizationData>();
                        config[m.Key] = dict;
                    }

                    dict[language] = new LocalizationData()
                    {
                        Context = m.Context,                       
                    };               
                    dict[language].Sprite = string.IsNullOrEmpty(m.SpriteAtlas)
                                ? AssetDatabase.LoadAssetAtPath<Sprite>(m.Sprite)
                                : AssetDatabase.LoadAssetAtPath<SpriteAtlas>(m.SpriteAtlas).GetSprite(m.Sprite);
                }
            }

        }
#endif

        public void Create(int maxLength)
        {
            config.Clear();
        }

        public void Import(int index, object userData)
        {
            KeyValuePair<string, YDictionary<Language, LocalizationData>> keyValuePair = (KeyValuePair<string, YDictionary<Language, LocalizationData>>)userData;

            config.Add(keyValuePair);
        }

        public void Completed()
        {

        }
    }
    [CreateAssetMenu(fileName = "LocalizationConfig", menuName = "YukiFrameWork/LocalizationConfig")]
    public class LocalizationConfig : LocalizationConfigBase<LocalizationData>
    {

    }
}
