using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using YukiFrameWork;

namespace YukiFrameWork.DiaLog
{
    // 对话节点状态枚举值
    public enum NodeState
    {
        Running,
        Waiting,
    }

    [Serializable]
    public class NodeData
    {
        [TextArea] public string dialogueContent;
        [JsonIgnore, PreviewField(50)]
        public Sprite speakerAvatar;
        public string speakerName;
    }

    public abstract class Node : ScriptableObject
    {
        // 对话节点当前状态
        [SerializeField, JsonProperty, LabelText("节点的唯一ID")]
        private int nodeIndex;

        [JsonIgnore]
        public int NodeIndex
        {
            get => nodeIndex;
            set => nodeIndex = value;
        }
        [JsonProperty]
        public NodeState state = NodeState.Waiting;
        [HideInInspector]
        public Language currentLanguage { get; set; }
        [HideInInspector] public string guid;
        [HideInInspector, JsonIgnore] public Vector2 position;
        [TextArea][LabelText("介绍/注释")] public string description;

        [SerializeField,DictionaryDrawerSettings(KeyLabel = "语言",ValueLabel = "数据配置")]
        public YDictionary<Language, NodeData> nodeDatas = new YDictionary<Language, NodeData>();

        public string DialogueContent()
        {
            if (nodeDatas.TryGetValue(currentLanguage, out NodeData data))
            {
                return data.dialogueContent;
            }
            throw new Exception("请查询是否添加了该语言的文本! Language:" + currentLanguage);

        }

        public string SpeakerName()
        {
            if (nodeDatas.TryGetValue(currentLanguage, out NodeData data))
            {
                return data.speakerName;
            }
            throw new Exception("请查询是否添加了该语言的文本名称! Language:" + currentLanguage);
        }

        public Sprite SpeakerAvatar()
        {
            if (nodeDatas.TryGetValue(currentLanguage, out NodeData data))
            {
                return data.speakerAvatar;
            }
            throw new Exception("请查询是否添加了该语言的文本名称! Language:" + currentLanguage);
        }
        public abstract bool MoveToNext(out Node nextNode);

        public abstract void OnStart();
        public abstract void OnStop();
    }
}
