///=====================================================
/// - FileName:      AbstractSystem.cs
/// - NameSpace:     YukiFrameWork.Project
/// - Created:       Yuki
/// - Email:         1274672030@qq.com
/// - Description:   系统基类，系统类必须继承该类
/// - Creation Time: 2024/1/5 21:13:57
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
namespace YukiFrameWork
{
    public abstract class AbstractSystem : ISystem
    {
        public abstract void Init();

        public virtual void Destroy() { }
        #region 架构本体
        [NonSerialized]
        private IArchitecture architecture;

        IArchitecture IGetArchitecture.GetArchitecture()
        {
            return architecture;
        }

        void ISetArchitecture.SetArchitecture(IArchitecture architecture)
        {
            this.architecture = architecture;
        }
        #endregion
    }
}