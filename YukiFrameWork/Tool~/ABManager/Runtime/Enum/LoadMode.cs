using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace XFABManager
{
    /// <summary>
    /// 加载模式 资源的加载方式
    /// </summary>
    public enum LoadMode
    {
        /// <summary>
        /// 从AssetBundle加载
        /// </summary>
        AssetBundle,
        /// <summary>
        /// 从本地资源加载，仅限Editor模式，提高开发效率，不用每次修改都重新打包
        /// </summary>
        Assets
    }
}

