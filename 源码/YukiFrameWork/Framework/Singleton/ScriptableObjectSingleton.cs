using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork
{
    public class ScriptableObjectSingleton<T> : ScriptableObject where T : ScriptableObjectSingleton<T>
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = SingletonFectory.CreateScriptableObjectSinleton<T>();
                }
                return instance;
            }
        }

        public void OnDisable()
        {
            instance = null;
        }

    }
}
