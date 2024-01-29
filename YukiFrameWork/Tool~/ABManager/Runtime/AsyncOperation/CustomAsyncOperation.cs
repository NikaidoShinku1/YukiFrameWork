using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace YukiFrameWork.XFABManager
{
    public class CustomAsyncOperation<T> : CustomYieldInstruction where T : CustomAsyncOperation<T>
    {
        #region 字段 

        [Obsolete("已过时,请使用AddCompleteEvent代替!")] 
        public event Action<T> completed = null;

        private Action<T> _completed;
        //public event Action<int> completed;

        private bool _isCompleted = false;

        #endregion

        #region 属性 

        public bool isDone { get { return isCompleted; } }


        protected bool isCompleted {
            get {
                return _isCompleted;
            }

            set {
                _isCompleted = value;
                if (_isCompleted) 
                {
                    completed?.Invoke(this as T);
                    _completed?.Invoke(this as T);
                    OnCompleted();
                }
            }
        }

        public float progress { get; protected set; }

        public string error { get; protected set; }

        #endregion

        public override bool keepWaiting
        {
            get
            {
                return !isDone;
            }
        }

        protected virtual void OnCompleted() { 

        }

        protected void Completed(string error) {
            this.error = error;
            Completed();
        }

        protected void Completed() {
            progress = 1;

            if ( isCompleted ) return;
            isCompleted = true;
        }

        public void AddCompleteEvent(Action<T> action)
        {
            _completed += action;
            // 如果该异步请求已经执行完成,则直接触发完成的回调
            if (isDone)
                action?.Invoke(this as T);
        }

        public void RemoveCompleteEvent(Action<T> action) { 
            _completed -= action;
        }

    }

}
