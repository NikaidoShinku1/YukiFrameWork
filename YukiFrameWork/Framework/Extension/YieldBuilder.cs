///=====================================================
/// - FileName:      YieldBuilder.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/28 15:21:22
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Runtime.CompilerServices;
using System.Security;
namespace YukiFrameWork
{
	public struct YieldBuilder
	{
        private YieldTask task;
        public YieldTask Task => task;
            
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static YieldBuilder Create()
        {
            return new YieldBuilder() {task = new YieldTask() };
        }   

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetException(Exception exception)
        {
            task.SetException(exception);
        }
     
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetResult()
        {
            task.SetResult();
        }
     
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {          
            awaiter.OnCompleted(stateMachine.MoveNext);
        }
     
        [SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {          
            awaiter.UnsafeOnCompleted(stateMachine.MoveNext);
        }
      
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }
      
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            //由于性能问题，这里拒绝使用装箱之后的状态机，所以 nothing happened
        }    

    }

    public struct YieldBuilder<T>
    {
        private YieldTask<T> task;
        public YieldTask<T> Task => task;

       
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static YieldBuilder<T> Create()
        {
            return new YieldBuilder<T>() { task = new YieldTask<T>() };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetException(Exception exception)
        {
            task.SetException(exception,default(T));
        }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetResult(T t)
        {
            task.SetResult(t);
        }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AwaitOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : INotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            awaiter.OnCompleted(stateMachine.MoveNext);
        }

        
        [SecuritySafeCritical]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AwaitUnsafeOnCompleted<TAwaiter, TStateMachine>(ref TAwaiter awaiter, ref TStateMachine stateMachine)
            where TAwaiter : ICriticalNotifyCompletion
            where TStateMachine : IAsyncStateMachine
        {
            awaiter.UnsafeOnCompleted(stateMachine.MoveNext);
        }

       
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            stateMachine.MoveNext();
        }

        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetStateMachine(IAsyncStateMachine stateMachine)
        {
            //由于性能问题，这里拒绝使用装箱之后的状态机，所以 nothing happened
        }

    }
}
