
using UnityEngine;
using UnityEngine.Events;

namespace XFABManager
{
    public class Unload : MonoBehaviour
    {

        [Header("持续时间")]
        [Tooltip("持续时间")]
        public float duration = 1;

        [Header("忽略时间缩放")]
        [Tooltip("忽略时间缩放")]
        public bool ingoreTimeScale;

        private float timer = 0;

        [Tooltip("被回收时触发")]
        [Header("被回收时触发")]
        public UnityEvent onUnload;

        private void OnEnable()
        {
            timer = 0;
        }

        private void Update()
        {
            timer += ingoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;

            if (timer >= duration) 
            {
                Close();
                timer = 0;
            } 
        }

        private void Close() 
        {
            GameObjectLoader.UnLoad(gameObject);
            onUnload?.Invoke();
        }

    }

}

