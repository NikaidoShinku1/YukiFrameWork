using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XFABManager
{
    [System.Obsolete("请使用IPostprocessBuildProject代替!",true)]
    public interface IOnBuildComplete
    {
        void OnBuildComplete(string projectName,string outputPath, BuildTarget buildTarget);
    }
}


