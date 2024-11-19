///=====================================================
/// - FileName:      Action.cs
/// - NameSpace:     YukiFrameWork.Behaviours
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/11/13 12:08:21
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using YukiFrameWork.Extension;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System.Collections;
using System.Linq;
using Newtonsoft.Json;
namespace YukiFrameWork.Behaviours
{
    [ChildModeInfo(ChildMode = ChildMode.None)]
    [ClassAPI("动作AI节点")]
	public abstract class Action : AIBehaviour
	{
        [SerializeField,LabelText("动作组件添加处理")]
        [InfoBox("当有自定义类继承IBehaviourAction接口时，可在此序列化并添加，在节点初始化时，自动处理。")]
        [ValueDropdown(nameof(_actionTypes))]
        [JsonProperty]
        internal string[] behaviourActions;

        [JsonIgnore]
        private IEnumerable _actionTypes => AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(x => typeof(IBehaviourAction).IsAssignableFrom(x) && !x.IsAbstract)
            .Select(x => new ValueDropdownItem() { Text = x.FullName,Value = x.FullName });

        [JsonIgnore]
        private List<IBehaviourAction> actions = new List<IBehaviourAction>();

        public override void OnInit()
        {
            base.OnInit();
            if (behaviourActions != null && behaviourActions.Length != 0)
            {
                actions = behaviourActions
                    .Select(x => AssemblyHelper.GetType(x).CreateInstance() as IBehaviourAction)
                    .ToList();
            }
        }

        public override void OnStart()
        {
            base.OnStart();
            for (int i = 0; i < actions.Count; i++)
            {
                actions[i].OnStart();
            }
        }

        public override void OnFailed()
        {
            base.OnFailed();
            for (int i = 0; i < actions.Count; i++)
            {
                actions[i].OnFailed();
            }
        }

        public override void OnSuccess()
        {
            base.OnSuccess();
            for (int i = 0; i < actions.Count; i++)
            {
                actions[i].OnSuccess();
            }
        }

        public override BehaviourStatus OnUpdate()
        {
            if(Status == BehaviourStatus.Running)
                for (int i = 0; i < actions.Count; i++)
                {
                    actions[i].Update();
                }
            return base.OnUpdate();
        }

        public override void OnFixedUpdate()
        {
            base.OnFixedUpdate();
            for (int i = 0; i < actions.Count; i++)
            {
                actions[i].FixedUpdate();
            }
        }

        public override void OnLateUpdate()
        {
            base.OnLateUpdate();
            for (int i = 0; i < actions.Count; i++)
            {
                actions[i].LateUpdate();
            }
        }
        public sealed override void AddChild(AIBehaviour behaviour)
        {

        }
      
        public sealed override void RemoveChild(AIBehaviour behaviour)
        {

        }
        public sealed override void Clear()
        {

        }
        public sealed override void ForEach(Action<AIBehaviour> each)
        {
            
        }

    }
    /// <summary>
    /// 动作节点可以添加的多个动作处理，该接口不会对生命周期进行处理，Update仅Action为Running时触发。
    /// </summary>
    public interface IBehaviourAction
    {
        public Action Action { get; set; }
        public void OnStart();
        public void Update();
        public void FixedUpdate();
        public void LateUpdate();
        public void OnFailed();
        public void OnSuccess();
    }
}
