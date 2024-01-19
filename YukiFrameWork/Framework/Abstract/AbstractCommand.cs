///=====================================================
/// - FileName:      AbstractCommand.cs
/// - NameSpace:     YukiFrameWork
/// - Created:       Yuki
/// - Email:         1274672030@qq.com
/// - Description:   命令基类,命令必须继承该抽象类
/// - Creation Time: 2024/1/4 23:17:58
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
using System.Collections;

namespace YukiFrameWork
{
    public abstract class AbstractCommand : ICommand
    {
        #region 架构本体
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

        public abstract void Execute();     
    }

    public abstract class AbstractCommand<TResult> : ICommand<TResult>
    {
        #region 架构本体
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

        public abstract TResult Execute();
      
    }
}