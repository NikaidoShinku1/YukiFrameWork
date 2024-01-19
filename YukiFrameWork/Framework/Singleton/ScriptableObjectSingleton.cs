using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork
{
    public class ScriptableObjectSingleton<T> : ScriptableObject,ISingletonKit where T : ScriptableObjectSingleton<T>
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = SingletonFectory.CreateScriptableObjectSingleton<T>();
                    instance.OnInit();
                }
                return instance;
            }
        }

        public void OnDestroy()
        {
            instance = null;
            SingletonFectory.ReleaseInstance<T>();
        }

        public void OnDisable()
        {
            instance = null;
            SingletonFectory.ReleaseInstance<T>();
        }

        public virtual void OnInit()
        {
           
        }
    }
}
