using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;


namespace XFABManager
{
    /// <summary>
    /// 某一个资源模块打包前的事件监听(仅在Editor模式下可用)
    /// </summary>
    public interface IPreprocessBuildProject  
    {
        void OnPreprocess(string projectName, string outputPath, BuildTarget buildTarget);
    }

}
#endif
