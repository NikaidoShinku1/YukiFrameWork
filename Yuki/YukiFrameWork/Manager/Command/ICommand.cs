///=====================================================
/// - FileName:      ICommand.cs
/// - NameSpace:     YukiFrameWork.Command
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   框架命令模块
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================


using System.Collections;
using YukiFrameWork.Events;

namespace YukiFrameWork.Command
{
    /// <summary>
    /// 命令接口
    /// </summary>
    public interface ICommand 
    {
        //执行controller发送的命令，同时接收C对应的容器，方便命令通知
        bool Execute(IObjectContainer Container = null,object data = null);

        //自设迭代器命令执行，可作为异步方法使用
        IEnumerator Async_Execute(IObjectContainer Container = null,object data = null);

    }

}