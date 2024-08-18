///=====================================================
/// - FileName:      CoroutineToken.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/6/20 18:59:57
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using YukiFrameWork.Pools;
namespace YukiFrameWork
{
	public enum TokenStates
	{				
		Running,
		Paused,
		Cancel
	}
	public class CoroutineToken : IDisposable,IGlobalSign
	{
		internal CoroutineTokenSource mSource;
		internal CoroutineToken(CoroutineTokenSource source)
		{
			this.mSource = source;			
		}

		public readonly EasyEvent<TokenStates> stateChanged = new EasyEvent<TokenStates>();

		private TokenStates states = TokenStates.Cancel;

		public bool IsRunning => states == TokenStates.Running;

		public TokenStates States
		{
			get => states;
			set
			{
				if (states != value)
				{
					switch (value)
					{
						case TokenStates.Running:
							states = TokenStates.Running;
							break;
						case TokenStates.Paused:
							states = TokenStates.Paused;
							break;
						case TokenStates.Cancel:
							states = TokenStates.Cancel;
							Invoke();
							break;						
					}
                    stateChanged.SendEvent(states);
				}
			}
		}

		public CoroutineToken() { }

		private HashSet<Action> actions = new HashSet<Action>();

        bool IGlobalSign.IsMarkIdle { get; set; }

        public void Register(Action callBack)
			=> actions.Add(callBack);

		public void Remove(Action callBack)
			=> actions?.Remove(callBack);
		
        void IDisposable.Dispose()
        {
			actions.Clear();
			states = TokenStates.Cancel;
        }

		internal void Invoke()
		{			
			foreach (var action in actions)
			{
				action?.Invoke();
			}
		}

        void IGlobalSign.Init()
        {
			
        }

        void IGlobalSign.Release()
        {
			((IDisposable)this).Dispose();
        }
    }
}
