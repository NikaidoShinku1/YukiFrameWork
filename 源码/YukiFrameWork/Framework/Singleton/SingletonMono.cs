using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace YukiFrameWork
{
    public abstract class SingletonMono<T> : MonoBehaviour, ISingletonKit<T> where T : SingletonMono<T>
    {
        private readonly static object _lock = new object();

        private readonly Stack<Action> OnBack = new Stack<Action>();

        private static T instance;
        [field: SerializeField] public bool IsDonDestroyLoad { get; protected set; } = true;
        public static T Instance
        {
            get
            {
                lock (_lock)
                {
                    if (instance != null) return instance;

                    instance = SingletonFectory.CreateMonoSingleton<T>();

                    return instance;
                }
            }
        }
        public static T I => Instance;

        protected virtual void Awake()
        {
            OnInit();        
        }


        public void OnInit()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this as T;
                if (IsDonDestroyLoad)
                    DontDestroyOnLoad(this);
            }
        }

        public virtual void OnDestroy()
        {
            if(!IsDonDestroyLoad)
            instance = null;
        }

        public static void Show(Action action = null)
        {
            T i = instance;

            i.gameObject.SetActive(true);

            if (action != null)
            {
                i.OnBack.Push(action);
            }
        }

        public static void Hide(bool isBack = true)
        {
            T i = instance;          
            if (isBack && i.OnBack != null && i.OnBack.Count>0)
            {
                i.OnBack.Pop()?.Invoke();
            }
            i.gameObject.SetActive(false);
        }    
    }       
}
