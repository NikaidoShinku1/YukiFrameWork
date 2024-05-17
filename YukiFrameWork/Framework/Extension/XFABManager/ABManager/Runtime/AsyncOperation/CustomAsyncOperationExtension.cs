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
		public static YieldTask<UnityEngine.Object> GetAwaiter(this LoadAssetRequest asyncOperation)
		{
            var awaiter = new YieldTask<UnityEngine.Object>();
            YieldTaskExtension.SetRunOnUnityScheduler(awaiter,MonoHelper.Start(NextVoid()));
            IEnumerator NextVoid()
            {
                yield return asyncOperation;
                awaiter.Complete(null, asyncOperation.asset);
            }
            return awaiter;
        }   

        public static YieldTask<UnityEngine.Object> CancelWaitGameObjectDestroy<T>(this LoadAssetRequest asyncOperation,T component)
            where T : Component
        {
            return asyncOperation.GetAwaiter().CancelWaitGameObjectDestroy(component);
        }

        public static YieldTask<T> GetAwaiter<T>(this LoadAssetRequest<T> asyncOperation) where T : UnityEngine.Object
        {
            var awaiter = new YieldTask<T>();
            YieldTaskExtension.SetRunOnUnityScheduler(awaiter, MonoHelper.Start(NextVoid()));
            IEnumerator NextVoid()
            {
                yield return asyncOperation;
                awaiter.Complete(null, asyncOperation.asset);
            }
            return awaiter;
        }

        public static YieldTask<K> CancelWaitGameObjectDestroy<T,K>(this LoadAssetRequest<K> asyncOperation, T component)
            where T : Component where K : UnityEngine.Object
        {
            return asyncOperation.GetAwaiter().CancelWaitGameObjectDestroy(component);
        }

        public static YieldTask<UnityEngine.Object> GetAwaiter(this LoadSubAssetRequest asyncOperation)
        {
            var awaiter = new YieldTask<UnityEngine.Object>();
            YieldTaskExtension.SetRunOnUnityScheduler(awaiter, MonoHelper.Start(NextVoid()));
            IEnumerator NextVoid()
            {
                yield return asyncOperation;
                awaiter.Complete(null, asyncOperation.asset);
            }
            return awaiter;
        }

        public static YieldTask<UnityEngine.Object> CancelWaitGameObjectDestroy<T>(this LoadSubAssetRequest asyncOperation, T component)
            where T : Component
        {
            return asyncOperation.GetAwaiter().CancelWaitGameObjectDestroy(component);
        }

        public static YieldTask<UnityEngine.Object[]> GetAwaiter(this LoadAssetsRequest asyncOperation)
        {
            var awaiter = new YieldTask<UnityEngine.Object[]>();
            YieldTaskExtension.SetRunOnUnityScheduler(awaiter, MonoHelper.Start(NextVoid()));
            IEnumerator NextVoid()
            {
                yield return asyncOperation;             
                awaiter.Complete(null, asyncOperation.assets);
            }
            return awaiter;
        }

        public static YieldTask<UnityEngine.Object[]> CancelWaitGameObjectDestroy<T>(this LoadAssetsRequest asyncOperation, T component)
            where T : Component
        {
            return asyncOperation.GetAwaiter().CancelWaitGameObjectDestroy(component);
        }


        public static YieldTask<T[]> GetAwaiter<T>(this LoadAssetsRequest<T> asyncOperation) where T : UnityEngine.Object
        {
            var awaiter = new YieldTask<T[]>();
            YieldTaskExtension.SetRunOnUnityScheduler(awaiter, MonoHelper.Start(NextVoid()));
            IEnumerator NextVoid()
            {
                yield return asyncOperation;
                awaiter.Complete(null, asyncOperation.assets);
            }
            return awaiter;
        }

        public static YieldTask<K[]> CancelWaitGameObjectDestroy<T,K>(this LoadAssetsRequest<K> asyncOperation, T component)
            where T : Component where K : UnityEngine.Object
        {
            return asyncOperation.GetAwaiter().CancelWaitGameObjectDestroy(component);
        }

        public static YieldTask<GameObject> GetAwaiter(this GameObjectLoadRequest asyncOperation)
        {
            var awaiter = new YieldTask<GameObject>();
            YieldTaskExtension.SetRunOnUnityScheduler(awaiter, MonoHelper.Start(NextVoid()));
            IEnumerator NextVoid()
            {
                yield return asyncOperation;
                awaiter.Complete(null, asyncOperation.Obj);
            }
            return awaiter;
        }

        public static YieldTask<UnityEngine.GameObject> CancelWaitGameObjectDestroy<T>(this GameObjectLoadRequest asyncOperation, T component)
            where T : Component
        {
            return asyncOperation.GetAwaiter().CancelWaitGameObjectDestroy(component);
        }

        public static YieldTask<AssetBundle> GetAwaiter(this LoadAssetBundleRequest asyncOperation)
        {
            var awaiter = new YieldTask<AssetBundle>();
            YieldTaskExtension.SetRunOnUnityScheduler(awaiter, MonoHelper.Start(NextVoid()));
            IEnumerator NextVoid()
            {
                yield return asyncOperation;
                awaiter.Complete(null, asyncOperation.assetBundle);
            }
            return awaiter;
        }

        public static YieldTask<UnityEngine.AssetBundle> CancelWaitGameObjectDestroy<T>(this LoadAssetBundleRequest asyncOperation, T component)
            where T : Component
        {
            return asyncOperation.GetAwaiter().CancelWaitGameObjectDestroy(component);
        }
    }
}
