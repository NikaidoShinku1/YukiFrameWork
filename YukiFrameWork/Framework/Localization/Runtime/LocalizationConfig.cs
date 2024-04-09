///=====================================================
/// - FileName:      LocalizationConfig.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
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
using System.IO;
using UnityEditor;
using System.Linq;
namespace YukiFrameWork
{
    public abstract class LocalizationConfigBase : ScriptableObject
    {
        public abstract ILocalizationData GetLocalizationData(Language language,string key);

        public abstract void Init();       
    }
  
    public abstract class LocalizationConfigBase<LocalizationData> : LocalizationConfigBase where LocalizationData : ILocalizationData
    {        
        private const string groupName = "本地设置:";
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

        [Button("Json配置表导入"), PropertySpace(10), ShowIf(nameof(openLoadMode)), BoxGroup(groupName)]
        void Import(TextAsset textAsset)
        {
            if (textAsset == null) return;

            config = AssemblyHelper.DeserializedObject<YDictionary<string, YDictionary<Language, LocalizationData>>>(textAsset.text);
        }
        [PropertySpace(10),Button("将本地数据导出Json配置表"), BoxGroup(groupName)]
        void Re_ImportFileInInspector(string fileName = "LocalizationConfigs",string assetPath = "Assets/Localization")
        {
            if (config.Count == 0)
            {
                Debug.LogError("没有添加配置无法导出!", this);
                return;
            }  
            string json = AssemblyHelper.SerializedObject(config);                       

            if (!Directory.Exists(assetPath))
            {               
                Directory.CreateDirectory(assetPath);
                AssetDatabase.Refresh();
            }           
            File.WriteAllText( $"{assetPath}/{fileName}.json",json);
            AssetDatabase.Refresh();

        }      
#endif
    }
    [CreateAssetMenu(fileName = "LocalizationConfig", menuName = "YukiFrameWork/LocalizationConfig")]
    public class LocalizationConfig : LocalizationConfigBase<LocalizationData>
    {

    }
}
