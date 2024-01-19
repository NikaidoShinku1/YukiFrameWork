///=====================================================
/// - FileName:      AbstractModel.cs
/// - NameSpace:     YukiFrameWork
/// - Created:       Yuki
/// - Email:         1274672030@qq.com
/// - Description:   数据模型基类,数据类必须继承该接口
/// - Creation Time: 2024/1/4 23:30:57
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
namespace YukiFrameWork
{
    public abstract class AbstractModel : IModel
    {
        public abstract void Init();

        #region 架构本体
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