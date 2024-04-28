///=====================================================
/// - FileName:      CustomAsyncOperationExtension.cs
/// - NameSpace:     XFABManager
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/27 22:43:57
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using XFABManager;
using System.Collections;
namespace YukiFrameWork
{
	public static class CustomAsyncOperationExtension
	{
		public static YieldAwaitable<UnityEngine.Object> GetAwaiter(this LoadAssetRequest asyncOperation)
		{
            var awaiter = new YieldAwaitable<UnityEngine.Object>();
            YieldAwaitableExtension.SetRunOnUnityScheduler(awaiter,MonoHelper.Start(NextVoid()));
            IEnumerator NextVoid()
            {
                yield return asyncOperation;
                awaiter.Complete(null, asyncOperation.asset);
            }
            return awaiter;
        }

        public static YieldAwaitable<UnityEngine.Object> CancelWaitGameObjectDestroy<T>(this LoadAssetRequest asyncOperation,T component)
            where T : Component
        {
            return asyncOperation.GetAwaiter().CancelWaitGameObjectDestroy(component);
        }

        public static YieldAwaitable<T> GetAwaiter<T>(this LoadAssetRequest<T> asyncOperation) where T : UnityEngine.Object
        {
            var awaiter = new YieldAwaitable<T>();
            YieldAwaitableExtension.SetRunOnUnityScheduler(awaiter, MonoHelper.Start(NextVoid()));
            IEnumerator NextVoid()
            {
                yield return asyncOperation;
                awaiter.Complete(null, asyncOperation.asset);
            }
            return awaiter;
        }

        public static YieldAwaitable<K> CancelWaitGameObjectDestroy<T,K>(this LoadAssetRequest<K> asyncOperation, T component)
            where T : Component where K : UnityEngine.Object
        {
            return asyncOperation.GetAwaiter().CancelWaitGameObjectDestroy(component);
        }

        public static YieldAwaitable<UnityEngine.Object> GetAwaiter(this LoadSubAssetRequest asyncOperation)
        {
            var awaiter = new YieldAwaitable<UnityEngine.Object>();
            YieldAwaitableExtension.SetRunOnUnityScheduler(awaiter, MonoHelper.Start(NextVoid()));
            IEnumerator NextVoid()
            {
                yield return asyncOperation;
                awaiter.Complete(null, asyncOperation.asset);
            }
            return awaiter;
        }

        public static YieldAwaitable<UnityEngine.Object> CancelWaitGameObjectDestroy<T>(this LoadSubAssetRequest asyncOperation, T component)
            where T : Component
        {
            return asyncOperation.GetAwaiter().CancelWaitGameObjectDestroy(component);
        }

        public static YieldAwaitable<UnityEngine.Object[]> GetAwaiter(this LoadAssetsRequest asyncOperation)
        {
            var awaiter = new YieldAwaitable<UnityEngine.Object[]>();
            YieldAwaitableExtension.SetRunOnUnityScheduler(awaiter, MonoHelper.Start(NextVoid()));
            IEnumerator NextVoid()
            {
                yield return asyncOperation;             
                awaiter.Complete(null, asyncOperation.assets);
            }
            return awaiter;
        }

        public static YieldAwaitable<UnityEngine.Object[]> CancelWaitGameObjectDestroy<T>(this LoadAssetsRequest asyncOperation, T component)
            where T : Component
        {
            return asyncOperation.GetAwaiter().CancelWaitGameObjectDestroy(component);
        }


        public static YieldAwaitable<T[]> GetAwaiter<T>(this LoadAssetsRequest<T> asyncOperation) where T : UnityEngine.Object
        {
            var awaiter = new YieldAwaitable<T[]>();
            YieldAwaitableExtension.SetRunOnUnityScheduler(awaiter, MonoHelper.Start(NextVoid()));
            IEnumerator NextVoid()
            {
                yield return asyncOperation;
                awaiter.Complete(null, asyncOperation.assets);
            }
            return awaiter;
        }

        public static YieldAwaitable<K[]> CancelWaitGameObjectDestroy<T,K>(this LoadAssetsRequest<K> asyncOperation, T component)
            where T : Component where K : UnityEngine.Object
        {
            return asyncOperation.GetAwaiter().CancelWaitGameObjectDestroy(component);
        }

        public static YieldAwaitable<GameObject> GetAwaiter(this GameObjectLoadRequest asyncOperation)
        {
            var awaiter = new YieldAwaitable<GameObject>();
            YieldAwaitableExtension.SetRunOnUnityScheduler(awaiter, MonoHelper.Start(NextVoid()));
            IEnumerator NextVoid()
            {
                yield return asyncOperation;
                awaiter.Complete(null, asyncOperation.Obj);
            }
            return awaiter;
        }

        public static YieldAwaitable<UnityEngine.GameObject> CancelWaitGameObjectDestroy<T>(this GameObjectLoadRequest asyncOperation, T component)
            where T : Component
        {
            return asyncOperation.GetAwaiter().CancelWaitGameObjectDestroy(component);
        }

        public static YieldAwaitable<AssetBundle> GetAwaiter(this LoadAssetBundleRequest asyncOperation)
        {
            var awaiter = new YieldAwaitable<AssetBundle>();
            YieldAwaitableExtension.SetRunOnUnityScheduler(awaiter, MonoHelper.Start(NextVoid()));
            IEnumerator NextVoid()
            {
                yield return asyncOperation;
                awaiter.Complete(null, asyncOperation.assetBundle);
            }
            return awaiter;
        }

        public static YieldAwaitable<UnityEngine.AssetBundle> CancelWaitGameObjectDestroy<T>(this LoadAssetBundleRequest asyncOperation, T component)
            where T : Component
        {
            return asyncOperation.GetAwaiter().CancelWaitGameObjectDestroy(component);
        }
    }
}
