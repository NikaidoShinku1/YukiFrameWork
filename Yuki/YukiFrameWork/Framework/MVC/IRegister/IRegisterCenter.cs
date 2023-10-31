///=====================================================
/// - FileName:      IRegisterCenter.cs
/// - NameSpace:     YukiFrameWork.MVC
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   这是一个框架工具创建的注册中心脚本
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================
using YukiFrameWork.Events;
namespace YukiFrameWork.MVC
{
    public interface IRegisterCenter : IRegisterModelOrView, IUnRegisterModelOrView,IGetRegisterEvent
    {
        /// <summary>
        /// 注册中心初始化方法，该函数通常在Controller手动调用
        /// </summary>
        void Init();
    }
}