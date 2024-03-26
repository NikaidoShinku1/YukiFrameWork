///=====================================================
/// - FileName:      AbstractQuery.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/26 20:36:19
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork
{
    public abstract class AbstractQuery<TResult> : IQuery<TResult>
    {
        #region 架构本体
        [NonSerialized]
        private IArchitecture mArchitecture;

        IArchitecture IGetArchitecture.GetArchitecture()
        {
            return mArchitecture;
        }

        void ISetArchitecture.SetArchitecture(IArchitecture architecture)
        {
            this.mArchitecture = architecture;
        }
        #endregion

        public abstract TResult Seek();
    }
}
