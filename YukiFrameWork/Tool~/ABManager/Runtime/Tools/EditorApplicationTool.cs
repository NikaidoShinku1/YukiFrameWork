using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


/// <summary>
/// EditorApplication工具
/// </summary>
public class EditorApplicationTool
{
    // Fix编码  
    /// <summary>
    /// 默认是运行中
    /// </summary>
    private static bool _isPlaying = true;

    /// <summary>
    /// 判断当前编辑器是否处于运行状态(UnityEngine.Application.isPlaying在停止运行编辑器时触发OnDestroy时仍返回true)
    /// </summary>
    public static bool isPlaying
    {
        get 
        {
            return _isPlaying;
        } 
    }

    [RuntimeInitializeOnLoadMethod] 
    static void Init() 
    { 
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
#endif
    }
#if UNITY_EDITOR
    private static void EditorApplication_playModeStateChanged(PlayModeStateChange obj)
    {
        switch (obj)
        {
            case PlayModeStateChange.ExitingPlayMode:
                _isPlaying = false;
                break;
            case PlayModeStateChange.ExitingEditMode:
                _isPlaying = true;
                break;
            case PlayModeStateChange.EnteredEditMode:
                _isPlaying = false;
                break;
            case PlayModeStateChange.EnteredPlayMode:
                _isPlaying = true;
                break; 
        }
    }
#endif
}


