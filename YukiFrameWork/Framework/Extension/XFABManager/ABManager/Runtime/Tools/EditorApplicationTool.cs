 
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


namespace XFABManager
{ 
    /// <summary>
    /// Application工具
    /// </summary>
    public class ApplicationTool
    {
#pragma warning disable IDE1006 // 命名样式
        /// <summary>
        /// 判断当前编辑器是否处于运行状态(UnityEngine.Application.isPlaying在停止运行编辑器时触发OnDestroy时仍返回true)
        /// </summary>
        public static bool isPlaying
        {
            get
            {
#if UNITY_EDITOR
                return EditorApplication.isPlayingOrWillChangePlaymode;  // 如果是编辑器模式 使用这个数据
#else  
                return true; // 如果是非编辑器模式一定是true
#endif  
            }

        }
#pragma warning restore IDE1006 // 命名样式 
    }
}