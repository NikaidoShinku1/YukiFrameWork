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
        [LabelText("推进的节点标识"), SerializeField, HideInInspector, JsonIgnore,ExcelIgnore]
        internal Node nextNode;

        [SerializeField]
        internal int nextNodeId;

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
        [field:InfoBox("该id为节点的运行唯一标识，不可修改，由数组顺序控制恒定。可与配表相互搭配使用"),ReadOnly,JsonProperty]
        public int nodeId { get; set; }
        [SerializeField,ShowIf(nameof(IsComposite)),ListDrawerSettings(HideAddButton = true)]
        internal List<Option> optionItems = new List<Option>();

        [SerializeField,ShowIf(nameof(IsRandom)), ListDrawerSettings(HideAddButton = true),ExcelIgnore]
        internal List<Node> randomItems = new List<Node>();

        [SerializeField,HideInInspector]
        internal List<int> randomIdItems = new List<int>();

        [SerializeField,HideInInspector]
        internal List<int> optionIdItems = new List<int>();

        [SerializeField,HideInInspector,JsonProperty]
        internal List<int> linkIds  = new List<int>();

        [JsonIgnore,ExcelIgnore]
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

        [JsonIgnore,ExcelIgnore]
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
        [SerializeField,HideIf(nameof(IsComOrRan)),ExcelIgnore]
        internal Node child;
        [SerializeField]
        internal int childNodeId;
        [SerializeField][JsonProperty,TextArea,LabelText("对话文本")] 
        internal string dialogueContext;
        [JsonIgnore, PreviewField(50)]
        [SerializeField,LabelText("对话图标")]internal Sprite icon;
        [JsonProperty,SerializeField,LabelText("对话名称")]internal string dialogueTitle;
#if UNITY_EDITOR       
        [HideInInspector,SerializeField]
        public Position position;
        [Serializable]
        public struct Position
        {
            public float x;
            public float y;
            public Position(float x,float y)
            {
                this.x = x;
                this.y = y;
            }
            public static implicit operator Vector2(Position position)
            {
                return new Vector2(position.x,position.y);
            }

            public static implicit operator Position(Vector2 position)
            {
                return new Position(position.x, position.y);
            }
        }
        [ExcelIgnore]
        internal Action onValidate;

        private void OnValidate()
        {
            onValidate?.Invoke();

            if(randomItems != null)
                randomIdItems = randomItems.Where(x => x).Select(x => x.nodeId).ToList();
            if (optionItems != null)
            {
                optionIdItems = optionItems.Where(x => x.nextNode).Select(x => x.nextNode.nodeId).ToList();
                foreach (var item in optionItems)
                {
                    if (item.nextNode)
                    {
                        item.nextNodeId = item.nextNode.nodeId;
                    }
                }
            }
            if ((IsSingle || IsRoot) && child)
            {
                childNodeId = child.nodeId;
            }
        }

        [JsonIgnore,ExcelIgnore]
        internal Type DiscernType => DiscernAttribute.GetType();

        [JsonIgnore,ExcelIgnore]
        internal int AttributeCount => DiscernAttributes.Count();
#endif      
        [JsonIgnore,ExcelIgnore]
        private DiaLogueNodeAttribute mDiscernAttribute;
        [JsonIgnore,ExcelIgnore]
        internal IEnumerable<DiaLogueNodeAttribute> DiscernAttributes => this.GetType().GetCustomAttributes<DiaLogueNodeAttribute>();
        [JsonIgnore,ExcelIgnore]
        internal DiaLogueNodeAttribute DiscernAttribute
        {
            get
            {
                mDiscernAttribute ??= DiscernAttributes.FirstOrDefault();
                return mDiscernAttribute;
            }
        }
        [JsonIgnore,ExcelIgnore]
        public bool IsSingle => DiscernAttribute is SingleNodeAttribute;
        [JsonIgnore,ExcelIgnore]
        public bool IsComposite => DiscernAttribute is CompositeNodeAttribute;
        [JsonIgnore,ExcelIgnore]
        public bool IsRoot => DiscernAttribute is RootNodeAttribute;
        [JsonIgnore,ExcelIgnore]
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
#endif

#endregion

    }
}
