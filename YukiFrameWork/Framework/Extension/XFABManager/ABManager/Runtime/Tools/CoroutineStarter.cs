using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace XFABManager
{

    /// <summary>
    /// 协程启动类(适用于非继承自MonoBehaviour的脚本启动协程)
    /// </summary>
    public class CoroutineStarter : MonoBehaviour
    {
        
          
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void Init() 
        { 
            GameObject obj = new GameObject("CoroutineStarter");
            Instance = obj.AddComponent<CoroutineStarter>();
        }


        private static CoroutineStarter Instance { get;set; }
  
         
        void Awake() 
        { 
            DontDestroyOnLoad(gameObject); 
        }

        /// <summary>
        /// 启动协程（适用于在非继承自MonoBehaviour的脚本中启动协程）,且通过该方法启动的协程不会因为切换场景而停止
        /// </summary>
        /// <param name="enumerator"></param>
        public static Coroutine Start(IEnumerator enumerator) 
        {
            if (Instance == null || Instance.ToString() == "null") 
                return null; 
            
            return Instance.StartCoroutine(enumerator);
        }

        /// <summary>
        /// 停止由CoroutineStarter启动的协程 
        /// </summary>
        /// <param name="coroutine"></param>
        public static void Stop(Coroutine coroutine) 
        {
            if (Instance == null || Instance.ToString() == "null") 
                return;
            Instance.StopCoroutine(coroutine);
        } 
    }

}

