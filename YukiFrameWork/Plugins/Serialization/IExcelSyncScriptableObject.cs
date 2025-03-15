///=====================================================
/// - FileName:      IExcelSyncScriptableObject.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/10 7:35:00
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections;

namespace YukiFrameWork
{
    /// <summary>
    /// Excel转ScriptableObject的同步接口，继承该接口即可使用框架的Excel转ScriptableObject功能
    /// </summary>
    public interface IExcelSyncScriptableObject 
    {
        /// <summary>
        /// 使用导出Excel时会采用的集合
        /// </summary>
        IList Array { get; }

        /// <summary>
        /// 导入Excel指定集合的类型，例如需要同步的数据是int类型的数组 则该Type应该是typeof(int)
        /// </summary>
        Type ImportType { get; }

        /// <summary>
        /// 构建时触发，同步集合大小
        /// </summary>
        /// <param name="maxLength"></param>
        void Create(int maxLength);

        /// <summary>
        /// 导入的过程(根据不同的配置自由定制)
        /// </summary>
        /// <param name="userData"></param>
        void Import(int index,object userData);

        /// <summary>
        /// 导入完成后执行的回调
        /// </summary>
        void Completed();

        /// <summary>
        /// 如果集合类型是ScriptableObject，当该属性为True时，默认会有转换工具控制配置的生成。
        /// </summary>
        bool ScriptableObjectConfigImport { get; }
    }
}
