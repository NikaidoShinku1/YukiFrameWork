///=====================================================
/// - FileName:      CoroutineTokenSource.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/6/20 18:59:48
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using YukiFrameWork.Pools;
using System.Collections.Generic;
using System.Collections;

namespace YukiFrameWork
{
	public class CoroutineTokenSource : IGlobalSign
	{
        internal static HashSet<Action> tasks = new HashSet<Action>();

        internal static void Register(Action action)
        { tasks.Add(action); }

        internal static void Remove(Action action)
            => tasks.Remove(action);
        [RuntimeInitializeOnLoadMethod]
        static void Init()
        {
            MonoHelper.Update_AddListener(_ => 
            {
                foreach (var a in tasks)
                {
                    a?.Invoke();
                }
            });
        }

		private CoroutineToken mToken;
		public CoroutineToken Token => mToken;

        bool IGlobalSign.IsMarkIdle { get; set ; }

		private CoroutineTokenSource() { }

		private OnGameObjectTrigger trigger;

		public static CoroutineTokenSource Create(Component component)
		{           
            if (!component.TryGetComponent<OnGameObjectTrigger>(out var trigger))
            {
                trigger = component.gameObject.AddComponent<OnGameObjectTrigger>();
            }         
            var source = GlobalObjectPools.GlobalAllocation<CoroutineTokenSource>(true);
            trigger.PushFinishEvent(() => 
            {
                source.Cancel();
              
            });          
            source.trigger = trigger;    
            return source;
		}

        /// <summary>
        /// 将Token设置为运行状态
        /// </summary>
        public void Running()
        {
            if (Token.States == TokenStates.Running) return;
            Token.States = TokenStates.Running;

            IsCanceled = false;
            isDelayCanceled = false;
        }

        /// <summary>
        /// 指令已经打断(完成)
        /// </summary>
        public bool IsCoroutineCompleted => Token.States == TokenStates.Cancel;

        public bool IsCanceled { get; private set; }
        private bool isDelayCanceled;

        public void Pause()
        {         
            if (Token.States == TokenStates.Cancel) return;
            Token.States = TokenStates.Paused;
        }    

        public void Cancel()
        {
            IsCanceled = true;          
            mToken.States = TokenStates.Cancel;           
        }

        public void CancelAfter(float time)
        {           
            if (isDelayCanceled) return;
            isDelayCanceled = true;
            IsCanceled = true;
            Delay(time);
        }

        private async void Delay(float time)
        {
            await CoroutineTool.WaitForSeconds(time);
            Cancel();            
        }

        void IGlobalSign.Init()
        {
            mToken = GlobalObjectPools.GlobalAllocation<CoroutineToken>();         
            IsCanceled = false;         
            isDelayCanceled = false;
            Running();
        }

        public void Release()
        {
            trigger = null;

            if(mToken?.GlobalRelease() == true)
                mToken = null;         
        }
 
    }

}