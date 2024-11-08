using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using YukiFrameWork;

namespace XFABManager
{
    public class Unload : YMonoBehaviour
    {

        [LabelText("持续时间")]
        [Tooltip("持续时间")]
        public float duration = 1;

        [LabelText("忽略时间缩放")]
        [Tooltip("忽略时间缩放")]
        public bool ingoreTimeScale;

        private float timer = 0;

        [LabelText("当对象回收时触发")]
        [Tooltip("当对象回收时触发")]
        public UnityEvent onUnLoad;

        private void OnEnable()
        {
            timer = 0;
        }

        private void Update()
        {
            timer += ingoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;

            if (timer >= duration) 
            {
                UnLoadObj();
                timer = 0;
            }
             
        }

        private void UnLoadObj()
        {
            GameObjectLoader.UnLoad(gameObject);
            onUnLoad?.Invoke();
        }


    }

}

