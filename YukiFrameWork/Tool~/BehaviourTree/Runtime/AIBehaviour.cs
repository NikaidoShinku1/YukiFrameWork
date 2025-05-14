///=====================================================
/// - FileName:      YBehaviour.cs
/// - NameSpace:     YukiFrameWork.Behaviours
/// - Description:   高级定制脚本生成
/// - Creation Time: 2024/11/13 11:30:50
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Reflection;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
#endif
namespace YukiFrameWork.Behaviours
{
    public enum BehaviourStatus
    {
        [LabelText("行为未激活")]
        InActive,
        [LabelText("行为执行中")]
        Running,
        [LabelText("行为成功时")]
        Success,
        [LabelText("行为失败时")]
        Failed
    }  
    [Serializable]
    public struct AIBehaviourPosition
    {
        public float x;
        public float y;
        public AIBehaviourPosition(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
        public static implicit operator Vector2(AIBehaviourPosition position)
        {
            return new Vector2(position.x, position.y);
        }

        public static implicit operator AIBehaviourPosition(Vector2 position)
        {
            return new AIBehaviourPosition(position.x, position.y);
        }
    }
    [Serializable]
    [HideMonoScript]
    public abstract class AIBehaviour : ScriptableObject
    {
        [NonSerialized, HideInInspector]
        private BindableProperty<BehaviourStatus> status;                 
        [JsonIgnore]
        public BehaviourStatus Status
        {
            get
            {              
                status ??= new BindableProperty<BehaviourStatus>();
                return status.Value;
            }
            internal protected set
            {
                status ??= new BindableProperty<BehaviourStatus>();
                status.Value = value;
            }
        }      
        [field: SerializeField, HideInInspector]
        [JsonIgnore] public AIBehaviour Parent { get; internal set; }

        [JsonProperty, SerializeField, HideInInspector]
        internal List<int> LinksId = new List<int>();

        [JsonProperty]
        internal string behaviourType;

        internal bool Isinited;
        /// <summary>
        /// 这个节点是否是根?
        /// </summary>
        [JsonIgnore]
        public bool IsRoot => this.GetType() == typeof(AIRootBehaviour);
        [ReadOnly]
        [JsonProperty]
        public int ID = -1;
        [TextArea]
        [DisableIf(nameof(IsRoot))]
        [JsonProperty]
        public string Description;

        [SerializeField, HideInInspector,JsonProperty]
        public AIBehaviourPosition position;

        [JsonIgnore] public bool IsRuning => Status == BehaviourStatus.Running;
        [JsonIgnore] public bool IsInActive => Status == BehaviourStatus.InActive;
        [JsonIgnore] public bool IsSuccess => Status == BehaviourStatus.Success;
        [JsonIgnore] public bool IsFailed => Status == BehaviourStatus.Failed;
        [JsonIgnore] public Transform transform { get; internal set; }
        [JsonIgnore] public BehaviourTree BehaviourTree { get; internal set; }
        [JsonIgnore, SerializeField, HideInInspector] internal BehaviourTreeSO behaviourTreeSO;
        internal void Init(BehaviourTree tree)
        {
            transform = tree.transform;
            BehaviourTree = tree;
            Status = BehaviourStatus.InActive;
            Inject_All_Params();
            status.Register(statu =>
            {
                switch (statu)
                {
                    case BehaviourStatus.InActive:
                        break;
                    case BehaviourStatus.Running:
                        break;
                    case BehaviourStatus.Success:
                        OnSuccess();
                        break;
                    case BehaviourStatus.Failed:
                        OnFailed();
                        break;
                }
            });
            OnInit();
            Isinited = true;
        }     
        private void Inject_All_Params()
        {
            Type type = this.GetType();

            foreach (var item in type
                .GetFields(System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.NonPublic
                | System.Reflection.BindingFlags.Instance
                | BindingFlags.Static
                ))
            {
                if (item.HasCustomAttribute(true, out BehaviourParamAttribute attribute))
                {
                    if (!BehaviourTree.paramViews.ContainsKey(ID))
                        continue;
                    string paramName = BehaviourTree.paramViews[ID].FirstOrDefault(x => x.fieldName == item.Name)?.paramName;
                    if (paramName.IsNullOrEmpty()) continue;
                    if (item is FieldInfo fieldInfo)
                    {
                        if (!BehaviourParam.AllParamTypes.ContainsKey(fieldInfo.FieldType))
                            throw new Exception("参数异常!类型不匹配! FieldType:" + fieldInfo.FieldType);
                        else
                        {
                            if (BehaviourTree.Params.TryGetValue(paramName, out var param))
                            {
                                BehaviourParamType paramType = BehaviourParam.AllParamTypes[fieldInfo.FieldType];
                                if (paramType != param.behaviourParamType)
                                {
                                    throw new Exception("参数类型匹配，但字段" + fieldInfo.Name + "的类型与指定参数类型不符合! ParamType:" + paramType);
                                }
                                fieldInfo.SetValue(this, param.Value);
                            }
                        }
                    }                   
                }
            };

        }      
        public virtual void OnInit() { }
       
        private void OnValidate()
        {
            onValidate?.Invoke();
        }
        [JsonIgnore]
        public System.Action onValidate;
        internal void Start()
        {
            //如果已经在运行则不需要进入
            if (Status == BehaviourStatus.Running) return;
            Status = BehaviourStatus.Running;
            OnStart();
        }       

        internal void Update()
        {
            if (!Isinited) return;           
            Status = OnUpdate();           
        }      

        /// <summary>
        /// 第一次进入执行状态时触发一次
        /// </summary>
        public virtual void OnStart() { }     

        public virtual BehaviourStatus OnUpdate() => BehaviourStatus.Success; 
        /// <summary>
        /// Set Internal Update
        /// </summary>
        internal virtual void OnInternalUpdate() { }
        public virtual void OnFixedUpdate() { }
        public virtual void OnLateUpdate() { }

        public virtual void OnSuccess() { }

        public virtual void OnFailed() { }

        /// <summary>
        /// 被打断时触发
        /// </summary>
        public virtual void OnInterruption() { }

        internal virtual void ReLoadChild() { }

        public abstract void AddChild(AIBehaviour behaviour);
        public abstract void RemoveChild(AIBehaviour behaviour);
        public abstract void Clear();

        public abstract void ForEach(Action<AIBehaviour> each);       
       
        public void ResetBehaviour() 
        {
            if (Status == BehaviourStatus.InActive) return;
            Status = BehaviourStatus.InActive;
            OnCancel();
            ForEach(child => 
            {
                if(child)
                child.ResetBehaviour();
            });
        }

        public virtual void OnCancel() { }

        public void Dispose()
        {
            ResetBehaviour();
            Clear();
            transform = null;
            BehaviourTree = null;
            status.UnRegisterAllEvent();
        }

        public override string ToString()
        {
            return $"Behaviour Type:{this.GetType()} Status:{Status}";
        }

#if UNITY_EDITOR
        /// <summary>
        /// 自定义编辑器
        /// </summary>
        public virtual void OnInspectorGUI()
        {
           
        }

        /// <summary>
        /// 当配置已经添加到运行对象BehaviourTree组件中，可重写该方法进行场景的绘制，但仅限于UnityEditor，请加入宏
        /// </summary>
        /// <param name="transform"></param>
        public virtual void DrawGizmos(Transform transform)
        {
            
        }
#endif
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(AIBehaviour),true)]
    [CanEditMultipleObjects]
    public class AIBehaviourEditor : OdinEditor 
    {
        public override void OnInspectorGUI()
        {
            AIBehaviour behaviour = target as AIBehaviour;
            base.OnInspectorGUI();
            EditorGUI.BeginChangeCheck();
            if (behaviour)
                behaviour.OnInspectorGUI();
            if (EditorGUI.EndChangeCheck())
                behaviour.Save();
        }
    }
#endif

}
