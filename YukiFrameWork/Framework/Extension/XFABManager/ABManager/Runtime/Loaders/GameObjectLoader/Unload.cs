using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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

        private void OnEnable()
        {
            timer = 0;
        }

        private void Update()
        {
            timer += ingoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;

            if (timer >= duration) 
            {
                GameObjectLoader.UnLoad(gameObject);
                timer = 0;
            }
             
        }


    }

}

