using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace YukiFrameWork.ABManager
{

    /// <summary>
    /// 同时执行多个异步请求
    /// </summary>
    public class ExecuteMultipleAsyncOperation<T> where T : CustomAsyncOperation<T>
    {

        /// <summary>
        /// 当某一个异步操作执行完成时触发
        /// </summary>
        public Action<T> onAsyncOperationCompleted;

        private int max_concurrent_execution_count;

        /// <summary>
        /// 执行中的异步操作
        /// </summary>
        public List<T> InExecutionAsyncOperations { get;private set; }

        private List<T> temp_async_operations = null;

        private List<T> error_operations = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="max_concurrent_execution_count">最大同时执行的数量</param>
        public ExecuteMultipleAsyncOperation(int max_concurrent_execution_count) 
        {
            if (max_concurrent_execution_count <= 0) throw new System.Exception("max_concurrent_execution_count 必须大于0!");
            this.max_concurrent_execution_count = max_concurrent_execution_count;
            InExecutionAsyncOperations = new List<T>(max_concurrent_execution_count);
            temp_async_operations = new List<T>(max_concurrent_execution_count);
            error_operations = new List<T>();
        }

        /// <summary>
        /// 加入一个正在执行中的异步请求
        /// </summary>
        /// <param name="instruction"></param>
        public void Add(T instruction) 
        { 
            InExecutionAsyncOperations.Add(instruction);    
        }

        /// <summary>
        /// 更新正在执行中的异步请求的状态(每帧执行)
        /// </summary>
        private void Update() {
            temp_async_operations.Clear();
            //Debug.Log(InExecutionAsyncOperations.Count);
            foreach (var item in InExecutionAsyncOperations)
            {
                if (item.isDone)
                { 
                    onAsyncOperationCompleted?.Invoke(item);
                    temp_async_operations.Add(item);

                    if (!string.IsNullOrEmpty(item.error)) error_operations.Add(item); 
                }
            }
            foreach (var item in temp_async_operations)
            {
                InExecutionAsyncOperations.Remove(item);
            }
        }

        /// <summary>
        /// 判断是否能够执行更多的异步请求，如果返回true则说明可以继续加入
        /// </summary>
        /// <returns></returns>
        public bool CanAdd() {
            Update();
            // 如果出错，不在执行新的异步请求
            if(!string.IsNullOrEmpty(Error())) return false;

            return InExecutionAsyncOperations.Count < max_concurrent_execution_count;
        }

        /// <summary>
        /// 判断异步请求是否全部执行完成
        /// </summary>
        /// <returns></returns>
        public bool IsDone() 
        {
            Update();
            return InExecutionAsyncOperations.Count == 0;
        }
  
        /// <summary>
        /// 请求的错误信息，如果为空则说明没有出错
        /// </summary>
        /// <returns></returns>
        public string Error() {
            if (error_operations.Count == 0) return string.Empty;
            StringBuilder error = new StringBuilder();
            foreach (var item in error_operations) {  
                error.AppendLine(item.error); 
            }
            return error.ToString();
        }

    }
}


