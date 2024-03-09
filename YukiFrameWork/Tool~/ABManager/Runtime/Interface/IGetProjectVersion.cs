using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XFABManager
{

    /// <summary>
    /// 获取项目版本信息的接口
    /// </summary>
    public interface IGetProjectVersion 
    {
        /// <summary>
        /// 获取项目版本信息
        /// </summary>
        /// <param name="projectName">项目名</param>
        void GetProjectVersion(string projectName );

        /// <summary>
        /// 判断是否获取完成
        /// </summary>
        /// <returns></returns>
        bool isDone();
        /// <summary>
        /// 获取结果 要在完成之后获取
        /// </summary>
        /// <returns></returns>
        string Result();
        /// <summary>
        /// 判断是否报错 如果没报错返回 Empy
        /// </summary>
        /// <returns></returns>
        string Error();

    }

}
