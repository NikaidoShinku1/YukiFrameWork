using Newtonsoft.Json;
using Sirenix.OdinInspector;
using System;
using UnityEngine;
using YukiFrameWork;

namespace YukiFrameWork.DiaLog
{
    // 对话节点状态枚举值
    public enum NodeTreeState
    {            
        Running,
        Waiting,
    }

    [Serializable]
    public class NodeData
    {
        [TextArea] public string dialogueContent;
        [JsonIgnore, PreviewField(50)]
        public Sprite icon;
        public string name;
    }

    public abstract class Node : ScriptableObject
    {
        // 对话节点当前状态
        [SerializeField, JsonProperty, LabelText("节点的唯一ID")]
#if UNITY_EDITOR
        [ReadOnly]
#endif
        private int nodeIndex;
       
        [JsonIgnore]
        public int NodeIndex
        {
            get => nodeIndex;
            set => nodeIndex = value;
        }       
        [HideInInspector]
        internal Language currentLanguage { get; set; }
        [HideInInspector] public string guid;      

        /// <summary>
        /// 设置节点是否完成
        /// </summary>
        public bool IsCompleted { get; set; } = true;

#if UNITY_EDITOR
        [HideInInspector]
        public Rect rect;
     
        [Button("修改ID")]
        [InfoBox("注意:任何节点，ID必须是唯一的!",InfoMessageType.Warning)]
        void Change(int newID)
        {
            var instance = DiaLogEditorWindow.instance;
            if (instance == null)
            {
                Debug.LogError("DigLog Window丢失，请检查是否正常开启");
                return;
            }

            if (instance.Context.NodeTree == null)
            {
                Debug.LogError("节点树丢失，请检查是否正确加载了节点树在DigLog Window中");
                return;
            }

            if (instance.Context.NodeTree.nodes.Find(x => x.nodeIndex == newID && x != this))
            {
                Debug.LogError("无法修改ID，该ID已经存在! Node ID:" + newID);
                return;
            }

            this.nodeIndex = newID;
        }
#endif      
        [TextArea][LabelText("介绍/注释")] public string description;

        [SerializeField,DictionaryDrawerSettings(KeyLabel = "语言",ValueLabel = "数据配置")]
        internal YDictionary<Language, NodeData> nodeDatas = new YDictionary<Language, NodeData>();

        public string GetContext()
        {
            if (nodeDatas.TryGetValue(currentLanguage, out NodeData data))
            {
                return data.dialogueContent;
            }
            throw new Exception("请查询是否添加了该语言的文本! Language:" + currentLanguage);

        }

        public string GetName()
        {
            if (nodeDatas.TryGetValue(currentLanguage, out NodeData data))
            {
                return data.name;
            }
            throw new Exception("请查询是否添加了该语言的文本名称! Language:" + currentLanguage);
        }

        public Sprite GetIcon()
        {
            if (nodeDatas.TryGetValue(currentLanguage, out NodeData data))
            {
                return data.icon;
            }
            throw new Exception("请查询是否添加了该语言的文本名称! Language:" + currentLanguage);
        }
        public abstract Node MoveToNext();

        #region 节点的生命周期
     
        public virtual void OnEnter() {  }
        public virtual void OnExit() {  }
        public virtual void OnUpdate() { }
        public virtual void OnFixedUpdate() { }
        public virtual void OnLateUpdate() { }

        /// <summary>
        /// 设置分支是否执行默认推进，当该属性返回False时，对话树推进到该分支时，需要用户自定义跳转或者手动进行，MoveNext方法始终返回False
        /// </summary>
        public abstract bool IsDefaultMoveNext { get; }
        #endregion

    }
}
