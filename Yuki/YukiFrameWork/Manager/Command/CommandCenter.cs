///=====================================================
/// - FileName:      CommandCenter.cs
/// - NameSpace:     YukiFrameWork.Command
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   框架命令中心
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================
using Cysharp.Threading.Tasks;
using System.Collections;
namespace YukiFrameWork.Command
{
    public interface ICommandCenter
    {
        /// <summary>
        /// 发送命令
        /// </summary>
        /// <typeparam name="T">命令类型</typeparam>
        /// <param name="container">容器</param>
        /// <param name="data">额外参数，如果有需要可以添加</param>
        /// <returns>
        /// 返回True:
        /// 命令执行成功
        /// 返回False:
        /// 命令执行失败(未完全完成)</returns>
        bool SendCommand<T>(IObjectContainer container = null, object data = null) where T : ICommand, new();

        /// <summary>
        /// 发送命令
        /// </summary>
        /// <typeparam name="T">命令类型</typeparam>
        /// <param name="command">命令本体</param>
        /// <param name="container">容器</param>
        /// <param name="data">额外参数，如果有需要可以添加</param>
        /// <returns>
        /// 返回True:
        /// 命令执行成功
        /// 返回False:
        /// 命令执行失败(未完全完成)</returns>
        bool SendCommand<T>(T command, IObjectContainer container = null, object data = null) where T : ICommand;

        /// <summary>
        /// 异步命令
        /// </summary>
        /// <typeparam name="T">命令类型</typeparam>
        /// <param name="command">命令本体</param>
        /// <param name="container">容器</param>
        /// <param name="data">额外参数，如果有需要可以添加</param>
        /// <returns>返回异步执行结果</returns>
        IEnumerator Send_AsyncCommand<T>(T command, IObjectContainer container = null, object data = null) where T : ICommand;

        /// <summary>
        /// 异步命令
        /// </summary>
        /// <typeparam name="T">命令类型</typeparam>
        /// <param name="container">容器</param>
        /// <param name="data">额外参数，如果有需要可以添加</param>
        /// <returns>返回异步执行结果</returns>
        IEnumerator Send_AsyncCommand<T>(IObjectContainer container = null, object data = null) where T : ICommand, new();

    }
    public class CommandCenter : ICommandCenter
    {      
        public bool SendCommand<T>(IObjectContainer container = null,object data = null) where T : ICommand, new()
        {
            T command = new T();
            return command.Execute(container,data);
        }
       
        public bool SendCommand<T>(T command, IObjectContainer container = null,object data = null) where T : ICommand
        {
            return command.Execute(container,data);           
        }
      
        public IEnumerator Send_AsyncCommand<T>(T command, IObjectContainer container = null,object data = null) where T : ICommand
        {           
            yield return command.Async_Execute(container,data);
        }
       
        public IEnumerator Send_AsyncCommand<T>(IObjectContainer container = null, object data = null) where T : ICommand, new()
        {
            T command = new T();
            yield return command.Async_Execute(container, data);
        }
    }
}
