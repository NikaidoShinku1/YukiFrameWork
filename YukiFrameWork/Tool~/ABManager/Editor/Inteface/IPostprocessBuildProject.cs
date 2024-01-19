
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace YukiFrameWork.ABManager {

    /// <summary>
    /// 某一个资源模块打包完成的事件监听(仅在Editor模式下可用)
    /// </summary>
    public interface IPostprocessBuildProject 
    {
        void OnPostprocess(string projectName, string outputPath, BuildTarget buildTarget);
    }

}

#endif