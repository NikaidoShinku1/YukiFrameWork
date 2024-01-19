using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace YukiFrameWork.ABManager
{

    /// <summary>
    /// 协程启动类(ABManager专用)
    /// </summary>
    public class CoroutineStarter : Singleton<CoroutineStarter>
    {      
        private static bool editor_mode = false;

        private CoroutineStarter() { }
#if UNITY_EDITOR 
        private static void EditorApplicationStateChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.ExitingPlayMode:
                    editor_mode = true;
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    editor_mode = false;
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    editor_mode = true;
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    editor_mode = false;
                    break;
            }
        } 
#endif
     
        /// <summary>
        /// 启动协程（适用于在非继承自MonoBehaviour的脚本中启动协程）,且通过该方法启动的协程不会因为切换场景而停止
        /// </summary>
        /// <param name="enumerator"></param>
        public static Coroutine Start(IEnumerator enumerator) {
            if (Instance == null) {
                return null; 
            }
            return AsyncCore.I.StartCoroutine(enumerator);
        }

        /// <summary>
        /// 停止由CoroutineStarter启动的协程 
        /// </summary>
        /// <param name="coroutine"></param>
        public static void Stop(Coroutine coroutine) {
            if (Instance == null) return;
            AsyncCore.I.StopCoroutine(coroutine);
        }
         
        public override void OnDestroy()
        {       
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= EditorApplicationStateChanged;
#endif
        }

    }

}

