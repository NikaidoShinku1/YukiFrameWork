///=====================================================
/// - FileName:      YieldTaskFor.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/5/16 14:40:52
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Networking;
namespace YukiFrameWork
{
    public partial class YieldTask
	{
		public static YieldTask Run(Func<IEnumerator> task)
		{
            return task.Invoke().GetAwaiter();
		}  

        public static YieldTask Run(Func<YieldInstruction> task)
        {
            return task.Invoke().GetAwaiter();
        }

        public static YieldTask Run(Func<YieldTask> task)
        {
            return task.Invoke();
        }

        public static YieldTask<AsyncOperation> Run(Func<AsyncOperation> task)
        {
            return task.Invoke().GetAwaiter();
        }

        public static YieldTask<UnityEngine.Object> Run(Func<ResourceRequest> task)
        {
            return task.Invoke().GetAwaiter();
        }

        public static YieldTask<UnityWebRequest> Run(Func<UnityWebRequest> task)
        {
            return task.Invoke().GetAwaiter();
        }

        public static YieldTask<AssetBundle> Run(Func<AssetBundleCreateRequest> task)
        {
            return task.Invoke().GetAwaiter();
        }

        public static YieldTask<UnityEngine.Object> Run(Func<AssetBundleRequest> task)
        {
            return task.Invoke().GetAwaiter();
        }

        public static YieldTask<T> Run<T>(Func<IEnumerator<T>> task)
        {
            return task.Invoke().GetAwaiter();
        }

        public static YieldTask<T> Run<K, T>(Func<K> task) where K : IEnumerator
        {
            return task.Invoke().GetAwaiter<K,T>();
        }

        public static YieldTask Run(Action action)
        {
            IEnumerator enumerator()
            {
                yield return CoroutineTool.WaitForFrame();
                action.Invoke();
            }
            return enumerator().GetAwaiter();
        }

        void ICoroutineCompletion.StopAllTask()
        {
            for (int i = 0; i < taskQueue.Count; i++)
            {
                ICoroutineCompletion coroutineCompletion = taskQueue[i];

                MonoHelper.Stop(coroutineCompletion.Coroutine);
            }

            for (int i = 0; i < taskAny.Count; i++)
            {
                ICoroutineCompletion coroutineCompletion = taskAny[i];

                MonoHelper.Stop(coroutineCompletion.Coroutine);
            }
        }

        internal FastList<YieldTask> taskQueue = new FastList<YieldTask>();

        internal FastList<YieldTask> taskAny = new FastList<YieldTask>();

        private FastList<YieldTask> queueResults = new FastList<YieldTask>();
        public static YieldTask<YieldTask[]> WhenAll(params YieldTask[] tasks)
		{
            var run = new YieldTask();
            if (tasks != null && tasks.Length > 0)
            {
                for (int i = 0; i < tasks.Length; i++)
                {
                    run.taskQueue.Add(tasks[i]);
                }
            }           
           
			return run.StartAll();
		}

        public static YieldTask<YieldTask> WhenAny(params YieldTask[] tasks)
        {
            var run = new YieldTask();
            if (tasks != null && tasks.Length > 0)
            {
                for (int i = 0; i < tasks.Length; i++)
                {                 
                    run.taskAny.Add(tasks[i]);
                }
            }
            return run.StartAny();
        }

        public static YieldTask<T[]> WhenAll<T>(params YieldTask<T>[] tasks)
        {
            var run = new YieldTask<T>();
            if (tasks != null && tasks.Length > 0)
            {
                for (int i = 0; i < tasks.Length; i++)
                {
                    run.taskQueue.Add(tasks[i]);
                }
            }        
            return run.StartAll();
        }

        public static YieldTask<T> WhenAny<T>(params YieldTask<T>[] tasks)
        {
            var run = new YieldTask<T>();
            if (tasks != null && tasks.Length > 0)
            {
                for (int i = 0; i < tasks.Length; i++)
                {
                    run.taskAny.Add(tasks[i]);
                }
            }
           
            return run.StartAny();
        }
        private YieldTask<YieldTask[]> StartAll()
        {
            YieldTask<YieldTask[]> task = new YieldTask<YieldTask[]>();
            task.taskDispatch += ((ICoroutineCompletion)this).StopAllTask;
            for (int i = 0; i < taskQueue.Count; i++)
            {
                YieldTaskExtension
               .RunOnUnityScheduler(() => ((ICoroutineCompletion)taskQueue[i]).Coroutine = MonoHelper.Start(Running(taskQueue[i])));
            }
                YieldTaskExtension
                .RunOnUnityScheduler(() => ((ICoroutineCompletion)task).Coroutine = MonoHelper.Start(QueueRunning(() => task.Complete(null, taskQueue.ToArray()))));

            IEnumerator Running(YieldTask task)
            {
                yield return task.ToCoroutine();
                queueResults.Add(task);
            }
            return task;
        } 

        private YieldTask<YieldTask> StartAny()
        {
            YieldTask<YieldTask> task = new YieldTask<YieldTask>();
            task.taskDispatch += ((ICoroutineCompletion)this).StopAllTask;
            Action<YieldTask> callBack = v =>
            {
                task.Complete(null, v);
                for (int i = 0; i < taskAny.Count; i++)
                {
                    ICoroutineCompletion coroutineCompletion = taskAny[i];
                    MonoHelper.Stop(coroutineCompletion.Coroutine);
                }
            };
            for (int i = 0; i < taskAny.Count; i++)
            {
                ICoroutineCompletion coroutineCompletion = taskAny[i];
                int index = i;
                YieldTaskExtension
              .RunOnUnityScheduler(() => coroutineCompletion.Coroutine = MonoHelper.Start(Running(taskAny[i])));
            }

            IEnumerator Running(YieldTask task)
            {
                yield return task.ToCoroutine();
                callBack?.Invoke(task);
            }
            return task;
        }
        internal IEnumerator QueueRunning(Action callBack)
        {
            yield return CoroutineTool.WaitUntil(() => taskQueue.Count == queueResults.Count);
            callBack?.Invoke();
        }
       
    }
    public partial class YieldTask<T>
	{
        public static YieldTask<T> Run(Func<YieldTask<T>> action)
        {		
            return action.Invoke();
        }

        internal FastList<YieldTask<T>> taskQueue = new FastList<YieldTask<T>>();

        internal FastList<YieldTask<T>> taskAny = new FastList<YieldTask<T>>();
      
        private FastList<T> queueResults = new FastList<T>();
        internal IEnumerator QueueRunning(Action callBack)
        {
            yield return CoroutineTool.WaitUntil(() => queueResults.Count == taskQueue.Count);
            callBack?.Invoke();
        }

        internal YieldTask<T[]> StartAll()
        {
            YieldTask<T[]> task = new YieldTask<T[]>();
            task.taskDispatch += ((ICoroutineCompletion)this).StopAllTask;
            for (int i = 0; i < taskQueue.Count; i++)
            {
                YieldTaskExtension
               .RunOnUnityScheduler(() => ((ICoroutineCompletion)taskQueue[i]).Coroutine = MonoHelper.Start(Running(taskQueue[i])));
            }
            YieldTaskExtension
            .RunOnUnityScheduler(() => ((ICoroutineCompletion)task).Coroutine = MonoHelper.Start(QueueRunning(() => task.Complete(null, queueResults.Items))));

            IEnumerator Running(YieldTask<T> task)
            {
                yield return task.ToCoroutine();
                queueResults.Add(task.GetResult());                         
            }
            return task;
        }

        internal YieldTask<T> StartAny()
        {
            YieldTask<T> task = new YieldTask<T>();
            task.taskDispatch += ((ICoroutineCompletion)this).StopAllTask;
            Action<T> callBack = v =>
            {
                task.Complete(null, v);
                for (int i = 0; i < taskAny.Count; i++)
                {
                    ICoroutineCompletion coroutineCompletion = taskAny[i];
                    MonoHelper.Stop(coroutineCompletion.Coroutine);
                }
            };
            for (int i = 0; i < taskAny.Count; i++)
            {
                ICoroutineCompletion coroutineCompletion = taskAny[i];
                int index = i;            
                YieldTaskExtension
              .RunOnUnityScheduler(() => coroutineCompletion.Coroutine = MonoHelper.Start(Running(taskAny[i])));
            }       

            IEnumerator Running(YieldTask<T> task)
            {
                yield return task.ToCoroutine();
                callBack?.Invoke(task.GetResult());
            }
            return task;
        }

        void ICoroutineCompletion.StopAllTask()
        {
            for (int i = 0; i < taskQueue.Count; i++)
            {
                ICoroutineCompletion coroutineCompletion = taskQueue[i];

                MonoHelper.Stop(coroutineCompletion.Coroutine);
            }

            for (int i = 0; i < taskAny.Count; i++)
            {
                ICoroutineCompletion coroutineCompletion = taskAny[i];

                MonoHelper.Stop(coroutineCompletion.Coroutine);
            }
            taskDispatch?.Invoke();
        }

        internal event Action taskDispatch = null;
    }
}