using Newtonsoft.Json;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using YukiFrameWork.Extension;

namespace YukiFrameWork.DiaLogue
{
    // 对话节点状态枚举值
    public enum NodeTreeState
    {            
        Running,
        Waiting,
    }   

    [Serializable]
    public class Option
    {
        [SerializeField,LabelText("设置条件的文本"),JsonProperty]
        internal string optionTexts;
        [JsonIgnore]
        public string OptionTexts => optionTexts;
        [LabelText("推进的节点标识"), SerializeField, HideInInspector,JsonIgnore]
        internal Node nextNode;

        [JsonIgnore, PreviewField(50),SerializeField]
        internal Sprite icon;

        [JsonIgnore]
        public Sprite Icon => icon;

        [JsonProperty]
        internal string spritePath;

        [ShowInInspector,LabelText("配对文本:"),JsonIgnore]
        private string ShowName
        {
            get
            {
                try
                {
                    return nextNode.GetContext(); 
                }
                catch { return string.Empty; }
            }
        }
    }
    [HideMonoScript]
    public abstract class Node : ScriptableObject
    {
        // 对话节点GUID标识
        [SerializeField, LabelText("节点的唯一GUID"),ReadOnly] 
        internal string id;
        [field:SerializeField,LabelText("节点的LinkId")]
        [InfoBox("该id为节点的运行唯一标识，不可修改，由数组顺序控制恒定。可与配表相互搭配使用"),ReadOnly,JsonProperty]
        public int nodeId { get; set; }
        [SerializeField,ShowIf(nameof(IsComposite)),ListDrawerSettings(HideAddButton = true)]
        internal List<Option> optionItems = new List<Option>();

        [SerializeField,ShowIf(nameof(IsRandom)), ListDrawerSettings(HideAddButton = true)]
        internal List<Node> randomItems = new List<Node>();

        [SerializeField,HideInInspector,JsonProperty]
        internal List<int> linkIds  = new List<int>();

        [JsonIgnore]
        public List<Option> OptionItems
        {
            get
            {
                if(IsComposite)
                    return optionItems;
#if YukiFrameWork_DEBUGFULL
                LogKit.W("该节点并未标记为分支节点，无法使用条件列表，将返回空值");
#endif
                return null;
            }
        }

        [JsonIgnore]
        public List<Node> RandomItems
        {
            get
            {
                if (IsRandom)
                    return randomItems;
#if YukiFrameWork_DEBUGFULL
                LogKit.W("该节点并未标记为随机节点，无法使用条件列表，将返回空值");
#endif
                return null;
            }
        }

        /// <summary>
        /// 设置节点是否完成
        /// </summary>
        [JsonIgnore]
        public bool IsCompleted { get; set; } = true;

        private bool IsComOrRan => IsComposite || IsRandom;
        [SerializeField,HideIf(nameof(IsComOrRan))]
        internal Node child;              
        [SerializeField][JsonProperty,TextArea,LabelText("对话文本")] 
        internal string dialogueContext;
        [JsonIgnore, PreviewField(50)]
        [SerializeField,LabelText("对话图标")]internal Sprite icon;
        [JsonProperty,SerializeField,LabelText("对话名称")]internal string dialogueTitle;
#if UNITY_EDITOR       
        [SerializeField,HideInInspector,JsonProperty]
        internal string spritePath;
        [SerializeField,HideInInspector,JsonProperty]
        internal string nodeType;
        [HideInInspector,JsonIgnore]
        public Vector2 position;

        internal Action onValidate;

        private void OnValidate()
        {
            onValidate?.Invoke();         
        }

        [JsonIgnore]
        internal Type DiscernType => DiscernAttribute.GetType();

        [JsonIgnore]
        internal int AttributeCount => DiscernAttributes.Count();
#endif      
        [JsonIgnore]
        private DiaLogueNodeAttribute mDiscernAttribute;
        [JsonIgnore]
        internal IEnumerable<DiaLogueNodeAttribute> DiscernAttributes => this.GetType().GetCustomAttributes<DiaLogueNodeAttribute>();
        [JsonIgnore]
        internal DiaLogueNodeAttribute DiscernAttribute
        {
            get
            {
                mDiscernAttribute ??= DiscernAttributes.FirstOrDefault();
                return mDiscernAttribute;
            }
        }
        [JsonIgnore]
        public bool IsSingle => DiscernAttribute is SingleNodeAttribute;
        [JsonIgnore]
        public bool IsComposite => DiscernAttribute is CompositeNodeAttribute;
        [JsonIgnore]
        public bool IsRoot => DiscernAttribute is RootNodeAttribute;
        [JsonIgnore]
        public bool IsRandom => DiscernAttribute is RandomNodeAttribute;      

        public string GetContext()
        {         
            return dialogueContext;            
        }

        public string GetName()
        {
            return dialogueTitle;      
        }

        public Sprite GetIcon()
        {           
            return icon;           
        }
      
        #region 节点的生命周期
     
        public virtual void OnEnter() {  }
        public virtual void OnExit() {  }
        public virtual void OnUpdate() { }
        public virtual void OnFixedUpdate() { }
        public virtual void OnLateUpdate() { }

#if UNITY_EDITOR     
        [Button("导入条件表"),ShowIf(nameof(IsComposite))]
        [InfoBox("导入条件表的操作是将当前已经做好的连线(已经有的条件),按照次序从上到下同步数据，而不是创建新的条件。")]
        void Import(TextAsset textAsset)
        {           
            if (optionItems == null || optionItems.Count == 0) return;      
            List<OptionModel> models = SerializationTool.DeserializedObject<List<OptionModel>>(textAsset.text);
            for (int i = 0; i < Mathf.Min(models.Count,optionItems.Count); i++)
            {
                var model = models[i];
                if(!model.spritePath.IsNullOrEmpty())
                    optionItems[i].icon = AssetDatabase.LoadAssetAtPath<Sprite>(models[i].spritePath);
                optionItems[i].optionTexts = model.optionTexts;
            }
        }

        struct OptionModel
        {
            public string optionTexts;
            public string spritePath;
            public int linkId;
        }

        [Button("导出条件表"),ShowIf(nameof(IsComposite))]
        void ReImport(string assetPath = "Assets/DiaLogData", string assetName = "optionItems")
        {
            if (!IsComposite) return;
            foreach (var option in optionItems)
            {              
                option.spritePath = option.Icon != null ? AssetDatabase.GetAssetPath(option.Icon) : string.Empty;
            }

            string json = SerializationTool.SerializedObject(optionItems, settings: new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            json.CreateFileStream(assetPath, assetName, ".json");
        }
#endif

#endregion

    }
}
