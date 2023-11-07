///=====================================================
/// - FileName:      IEnumeratorExtension.cs
/// - NameSpace:     YukiFrameWork
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   迭代器拓展脚本
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using System;
using System.Collections;
using UnityEngine;
using YukiFrameWork.Pools;
namespace YukiFrameWork
{
    public static class IEnumeratorExtension
    {
        public static IYukiTask Start(this IEnumerator enumerator,Action callBack = null)
            => YukiTask.Get(enumerator,callBack);

        public static IYukiTask CancelWaitGameObjectDestory<TComponent>(this IYukiTask yukiEnumerator, TComponent component) where TComponent : Component
        {
            ((YukiTask)yukiEnumerator).Current = component.gameObject;
            return yukiEnumerator;
        }
        
        public static void Cancel(this IYukiTask enumerator)
            => enumerator.Cancel();
    }

    public interface IYukiTask
    {
        bool IsPause { get; set; }
        bool IsRunning { get; }
        object Current { get; }
        void Cancel();
    }

    public class YukiTask : IYukiTask
    {
        private IEnumerator enumerator;
        private Action callBack;
        private static readonly SimpleObjectPools<YukiTask> simpleObjectPools
            = new SimpleObjectPools<YukiTask>(() => new YukiTask(), null, 10);
        public object Current { get; set; }
        public bool IsPause { get; set; }       
        public bool IsRunning { get; private set; }

        public static YukiTask Get(IEnumerator enumerator, Action callBack)
        {
            var yuki = simpleObjectPools.Get();
            yuki.Init(enumerator, callBack);
            return yuki;
        }
        public YukiTask() { }
        public YukiTask(IEnumerator enumerator,Action callBack)
        {
            Init(enumerator, callBack);
        }

        public void Init(IEnumerator enumerator, Action callBack)
        {
            this.enumerator = enumerator;
            this.callBack = callBack;
            IsRunning = true;
            Current = IEnumeratorNode.I;
            IEnumeratorNode.I.StartCoroutine(GetEnumerator());
        }

        public IEnumerator GetEnumerator()
        {
            while (IsRunning)
            {
                if (Current.Equals(null))
                {
                    Debug.Log("绑定的对象被销毁，强制终止协程");
                    IsRunning = false;
                    simpleObjectPools.Release(this);
                    yield break;
                }
                
                if (IsPause)
                    yield return null;
                else
                {
                    if (enumerator.MoveNext())
                    {
                        yield return enumerator.Current;
                    }
                    else
                    {
                        IsRunning = false;
                    }
                    
                }
            }
            callBack?.Invoke();
            simpleObjectPools.Release(this);
        }

        public void Cancel()
        {
            simpleObjectPools.Release(this);
            IEnumeratorNode.I.StopCoroutine(GetEnumerator());
        }
    }

    public sealed class IEnumeratorNode : SingletonMono<IEnumeratorNode>
    {
        protected override void Awake()
        {
            IsDonDestroyLoad = true;
            base.Awake();
        }           
    }  

}