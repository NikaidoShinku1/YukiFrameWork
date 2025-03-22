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
using Newtonsoft.Json.Linq;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
namespace YukiFrameWork
{
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
        /// 精灵(图片)
        /// </summary>
		Sprite Sprite { get; }
    }
#if UNITY_EDITOR
    public abstract class CustomLocalizationDataConvert<T> : JsonConverter<T> where T : ILocalizationData
    {
        
    }
    public class SpriteSerializeConvert : CustomLocalizationDataConvert<LocalizationData>
    {

        public override LocalizationData ReadJson(JsonReader reader, Type objectType, LocalizationData existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject obj = JObject.Load(reader);
            LocalizationData localizationData = new LocalizationData();

            string guid = obj["sprite"].ToString();
            localizationData.context = obj["context"].ToString();
            localizationData.key = obj["key"].ToString(); 
            if(!guid.IsNullOrEmpty())
                localizationData.sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(UnityEditor.AssetDatabase.GUIDToAssetPath(guid));

            return localizationData;
        }
       
        public override void WriteJson(JsonWriter writer, LocalizationData value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("key");
            writer.WriteValue(value.key);
            writer.WritePropertyName("context");
            writer.WriteValue(value.context);
            writer.WritePropertyName("sprite");
            writer.WriteValue(UnityEditor.AssetDatabase.AssetPathToGUID(UnityEditor.AssetDatabase.GetAssetPath(value.sprite)));
            writer.WriteEndObject();
        }
    }
#endif
    [Serializable]
#if UNITY_EDITOR
    [JsonConverter(typeof(SpriteSerializeConvert))]
#endif
    public class LocalizationData : ILocalizationData
    {
        [SerializeField,JsonProperty,LabelText("唯一标识")]
        [InfoBox("对于不同的语言，应保持标识始终唯一")]
        protected internal string key;
        [ExcelIgnore,JsonIgnore]

        public string Key => key;
        [TextArea, JsonProperty]
        [SerializeField]
        protected internal string context;

        [JsonIgnore,ExcelIgnore]
        public string Context => context;

#if UNITY_EDITOR
        [CustomValueDrawer(nameof(DrawPreview))]
#endif
        [SerializeField]       
        protected internal Sprite sprite;

        [JsonIgnore,ExcelIgnore]
        public Sprite Sprite => Sprite;
#if UNITY_EDITOR
        private void DrawPreview()
        {

            GUILayout.BeginHorizontal();

            GUILayout.Label("Buff的图标样式");
            sprite = (Sprite)UnityEditor.EditorGUILayout.ObjectField(this.sprite, typeof(Sprite), true, GUILayout.Width(50), GUILayout.Height(50));
            GUILayout.EndHorizontal();
        }
#endif

    }    
}
