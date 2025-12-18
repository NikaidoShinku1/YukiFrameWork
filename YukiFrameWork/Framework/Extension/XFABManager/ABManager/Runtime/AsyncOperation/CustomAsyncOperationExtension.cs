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
using System;
namespace YukiFrameWork
{
	public static class CustomAsyncOperationExtension
	{
        public static YieldTask<ReadyResRequest> GetAwaiter(this ReadyResRequest readyResRequest)
        {
            var awaiter = new YieldTask<ReadyResRequest>();
            YieldTaskExtension.SetRunOnUnityScheduler(awaiter, MonoHelper.Start(NextVoid()));
            IEnumerator NextVoid()
            {
                yield return readyResRequest;
                if (awaiter.token != null)
                    yield return CoroutineTool.WaitUntil(() => awaiter.token.IsRunning);

                awaiter.Complete(null, readyResRequest);
            }
            return awaiter;
        }

        public static YieldTask<CheckResUpdateRequest> GetAwaiter(this CheckResUpdateRequest result)
        {
            var awaiter = new YieldTask<CheckResUpdateRequest>();
            YieldTaskExtension.SetRunOnUnityScheduler(awaiter, MonoHelper.Start(NextVoid()));
            IEnumerator NextVoid()
            {
                yield return result;
                if (awaiter.token != null)
                    yield return CoroutineTool.WaitUntil(() => awaiter.token.IsRunning);

                awaiter.Complete(null, result);
            }
            return awaiter;
        }

        public static YieldTask<CheckResUpdatesRequest> GetAwaiter(this CheckResUpdatesRequest result)
        {
            var awaiter = new YieldTask<CheckResUpdatesRequest>();
            YieldTaskExtension.SetRunOnUnityScheduler(awaiter, MonoHelper.Start(NextVoid()));
            IEnumerator NextVoid()
            {
                yield return result;
                if (awaiter.token != null)
                    yield return CoroutineTool.WaitUntil(() => awaiter.token.IsRunning);

                awaiter.Complete(null, result);
            }
            return awaiter;
        }

        public static YieldTask<ExtractResRequest> GetAwaiter(this ExtractResRequest result)
        {
            var awaiter = new YieldTask<ExtractResRequest>();
            YieldTaskExtension.SetRunOnUnityScheduler(awaiter, MonoHelper.Start(NextVoid()));
            IEnumerator NextVoid()
            {
                yield return result;
                if (awaiter.token != null)
                    yield return CoroutineTool.WaitUntil(() => awaiter.token.IsRunning);

                awaiter.Complete(null, result);
            }
            return awaiter;
        }



        public static YieldTask<UnityEngine.Object> GetAwaiter(this LoadAssetRequest asyncOperation)
		{
            var awaiter = new YieldTask<UnityEngine.Object>();
            YieldTaskExtension.SetRunOnUnityScheduler(awaiter,MonoHelper.Start(NextVoid()));
            IEnumerator NextVoid()
            {
                yield return asyncOperation;
                if (awaiter.token != null)
                    yield return CoroutineTool.WaitUntil(() => awaiter.token.IsRunning);
                
                awaiter.Complete(null, asyncOperation.asset);
            }
            return awaiter;
        }

        public static YieldTask<LoadSceneRequest> GetAwaiter(this LoadSceneRequest asyncOperation)
        {
            var awaiter = new YieldTask<LoadSceneRequest>();
            YieldTaskExtension.SetRunOnUnityScheduler(awaiter, MonoHelper.Start(NextVoid()));
            IEnumerator NextVoid()
            {
                yield return asyncOperation;
                if (awaiter.token != null)
                    yield return CoroutineTool.WaitUntil(() => awaiter.token.IsRunning);

                awaiter.Complete(null, asyncOperation);
            }
            return awaiter;
        }

        [Obsolete]
        public static YieldTask<UnityEngine.Object> CancelWaitGameObjectDestroy<T>(this LoadAssetRequest asyncOperation,T component)
            where T : Component
        {
            return asyncOperation.GetAwaiter().CancelWaitGameObjectDestroy(component);
        }

        public static YieldTask<UnityEngine.Object> Token(this LoadAssetRequest asyncOperation, CoroutineToken token)
        {
            return asyncOperation.GetAwaiter().Token(token);
        }


        public static YieldTask<T> GetAwaiter<T>(this LoadAssetRequest<T> asyncOperation) where T : UnityEngine.Object
        {
            var awaiter = new YieldTask<T>();
            YieldTaskExtension.SetRunOnUnityScheduler(awaiter, MonoHelper.Start(NextVoid()));
            IEnumerator NextVoid()
            {
                yield return asyncOperation;
                if (awaiter.token != null)
                    yield return CoroutineTool.WaitUntil(() => awaiter.token.IsRunning);
                awaiter.Complete(null, asyncOperation.asset);
            }
            return awaiter;
        }
        [Obsolete]
        public static YieldTask<K> CancelWaitGameObjectDestroy<T,K>(this LoadAssetRequest<K> asyncOperation, T component)
            where T : Component where K : UnityEngine.Object
        {
            return asyncOperation.GetAwaiter().CancelWaitGameObjectDestroy(component);
        }

        public static YieldTask<K> Token<K>(this LoadAssetRequest<K> asyncOperation,CoroutineToken token)
            where K : UnityEngine.Object
        {
            return asyncOperation.GetAwaiter().Token(token);
        }

        public static YieldTask<UnityEngine.Object> GetAwaiter(this LoadSubAssetRequest asyncOperation)
        {
            var awaiter = new YieldTask<UnityEngine.Object>();
            YieldTaskExtension.SetRunOnUnityScheduler(awaiter, MonoHelper.Start(NextVoid()));
            IEnumerator NextVoid()
            {
                yield return asyncOperation;
                if (awaiter.token != null)
                    yield return CoroutineTool.WaitUntil(() => awaiter.token.IsRunning);
                awaiter.Complete(null, asyncOperation.asset);
            }
            return awaiter;
        }
        [Obsolete]
        public static YieldTask<UnityEngine.Object> CancelWaitGameObjectDestroy<T>(this LoadSubAssetRequest asyncOperation, T component)
            where T : Component
        {
            return asyncOperation.GetAwaiter().CancelWaitGameObjectDestroy(component);
        }

        public static YieldTask<UnityEngine.Object> Token(this LoadSubAssetRequest asyncOperation, CoroutineToken token)          
        {
            return asyncOperation.GetAwaiter().Token(token);
        }

        public static YieldTask<UnityEngine.Object[]> GetAwaiter(this LoadAssetsRequest asyncOperation)
        {
            var awaiter = new YieldTask<UnityEngine.Object[]>();
            YieldTaskExtension.SetRunOnUnityScheduler(awaiter, MonoHelper.Start(NextVoid()));
            IEnumerator NextVoid()
            {
                yield return asyncOperation;
                if (awaiter.token != null)
                    yield return CoroutineTool.WaitUntil(() => awaiter.token.IsRunning);
                awaiter.Complete(null, asyncOperation.assets);
            }
            return awaiter;
        }
        [Obsolete]
        public static YieldTask<UnityEngine.Object[]> CancelWaitGameObjectDestroy<T>(this LoadAssetsRequest asyncOperation, T component)
            where T : Component
        {
            return asyncOperation.GetAwaiter().CancelWaitGameObjectDestroy(component);
        }

        public static YieldTask<UnityEngine.Object[]> Token(this LoadAssetsRequest asyncOperation, CoroutineToken token)        
        {
            return asyncOperation.GetAwaiter().Token(token);
        }


        public static YieldTask<T[]> GetAwaiter<T>(this LoadAssetsRequest<T> asyncOperation) where T : UnityEngine.Object
        {
            var awaiter = new YieldTask<T[]>();
            YieldTaskExtension.SetRunOnUnityScheduler(awaiter, MonoHelper.Start(NextVoid()));
            IEnumerator NextVoid()
            {
                yield return asyncOperation;
                if (awaiter.token != null)
                    yield return CoroutineTool.WaitUntil(() => awaiter.token.IsRunning);
                awaiter.Complete(null, asyncOperation.assets);
            }
            return awaiter;
        }
        [Obsolete]
        public static YieldTask<K[]> CancelWaitGameObjectDestroy<T,K>(this LoadAssetsRequest<K> asyncOperation, T component)
            where T : Component where K : UnityEngine.Object
        {
            return asyncOperation.GetAwaiter().CancelWaitGameObjectDestroy(component);
        }

        public static YieldTask<K[]> Token<K>(this LoadAssetsRequest<K> asyncOperation, CoroutineToken token)
            where K : UnityEngine.Object
        {
            return asyncOperation.GetAwaiter().Token(token);
        }

        public static YieldTask<GameObject> GetAwaiter(this GameObjectLoadRequest asyncOperation)
        {
            var awaiter = new YieldTask<GameObject>();
            YieldTaskExtension.SetRunOnUnityScheduler(awaiter, MonoHelper.Start(NextVoid()));
            IEnumerator NextVoid()
            {
                yield return asyncOperation;
                if (awaiter.token != null)
                    yield return CoroutineTool.WaitUntil(() => awaiter.token.IsRunning);
                awaiter.Complete(null, asyncOperation.Obj);
            }
            return awaiter;
        }
        [Obsolete]
        public static YieldTask<UnityEngine.GameObject> CancelWaitGameObjectDestroy<T>(this GameObjectLoadRequest asyncOperation, T component)
            where T : Component
        {
            return asyncOperation.GetAwaiter().CancelWaitGameObjectDestroy(component);
        }

        public static YieldTask<UnityEngine.GameObject> Token(this GameObjectLoadRequest asyncOperation, CoroutineToken token)           
        {
            return asyncOperation.GetAwaiter().Token(token);
        }


        public static YieldTask<AssetBundle> GetAwaiter(this LoadAssetBundleRequest asyncOperation)
        {
            var awaiter = new YieldTask<AssetBundle>();
            YieldTaskExtension.SetRunOnUnityScheduler(awaiter, MonoHelper.Start(NextVoid()));
            IEnumerator NextVoid()
            {
                yield return asyncOperation;
                if (awaiter.token != null)
                    yield return CoroutineTool.WaitUntil(() => awaiter.token.IsRunning);
                awaiter.Complete(null, asyncOperation.assetBundle);
            }
            return awaiter;
        }
        [Obsolete]
        public static YieldTask<UnityEngine.AssetBundle> CancelWaitGameObjectDestroy<T>(this LoadAssetBundleRequest asyncOperation, T component)
            where T : Component
        {
            return asyncOperation.GetAwaiter().CancelWaitGameObjectDestroy(component);
        }

        public static YieldTask<UnityEngine.AssetBundle> Token(this LoadAssetBundleRequest asyncOperation,CoroutineToken token)         
        {
            return asyncOperation.GetAwaiter().Token(token);
        }
    }
}
