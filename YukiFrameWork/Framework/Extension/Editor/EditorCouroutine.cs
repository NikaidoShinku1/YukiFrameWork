///=====================================================
/// - FileName:      EditorCouroutine.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/1 21:54:00
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork.Pools;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;

namespace YukiFrameWork
{
	public class EditorCoroutine
	{		
		private Action onFinish;
		private IEnumerator enumerator;

		private Stack<IEnumerator> enumerators = new Stack<IEnumerator>();
		
		public EditorCoroutine(Action onFinish,IEnumerator enumerator)
		{			
			this.onFinish = onFinish;
			this.enumerator = enumerator;
			enumerators.Push(enumerator);
		}       

        public EditorCoroutine Start()
		{
            EditorApplication.update += Update;
			return this;
        }

		public void Stop()
		{
			EditorApplication.update -= Update;
		}

        protected virtual void Update()
		{
			if (enumerators.Count == 0)
			{
                EditorCoroutineTool.StopCoroutine(this);
                onFinish?.Invoke();
				return;
            }
			IEnumerator i = enumerators.Peek();
			if (!i.MoveNext())
			{
				EditorCoroutineTool.StopCoroutine(this);
				onFinish?.Invoke();
			}
			else
			{
				object result = i.Current;
				if (result is IEnumerator)				
					enumerators.Push(result as IEnumerator);				
			}
		}

        //public override bool keepWaiting => enumerator.MoveNext();
    }
	public static class EditorCoroutineTool
	{
		private static List<EditorCoroutine> coroutines = new List<EditorCoroutine>();
		public static EditorCoroutine StartCoroutine(this IEnumerator enumerator, Action onFinish = null)
		{
            var coroutine = new EditorCoroutine(onFinish, enumerator);
			coroutines.Add(coroutine);
			coroutine.Start();	
			return coroutine;
        }

		public static void StopCoroutine(this EditorCoroutine coroutine)
		{
			coroutine.Stop();
			coroutines.Remove(coroutine);
		}

		public static void StopAllEditorCoroutine()
		{
			for (int i = 0; i < coroutines.Count; i++)
			{
				coroutines[i].Stop();
			}

			coroutines.Clear();
		}
	}
}
#endif