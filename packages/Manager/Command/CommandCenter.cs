using Cysharp.Threading.Tasks;

namespace YukiFrameWork.Command
{
    public class CommandCenter
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
        public bool SendCommand<T>(IObjectContainer container = null,object data = null) where T : ICommand, new()
        {
            T command = new T();
            return command.Execute(container,data);
        }

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
        public bool SendCommand<T>(T command, IObjectContainer container = null,object data = null) where T : ICommand
        {
            return command.Execute(container,data);           
        }

        /// <summary>
        /// 异步命令
        /// </summary>
        /// <typeparam name="T">命令类型</typeparam>
        /// <param name="command">命令本体</param>
        /// <param name="container">容器</param>
        /// <param name="data">额外参数，如果有需要可以添加</param>
        /// <returns>返回异步执行结果</returns>
        public async UniTask<bool> Send_AsyncCommand<T>(T command, IObjectContainer container = null,object data = null) where T : ICommand
        {           
            return await command.Async_Execute(container,data);
        }

        /// <summary>
        /// 异步命令
        /// </summary>
        /// <typeparam name="T">命令类型</typeparam>
        /// <param name="container">容器</param>
        /// <param name="data">额外参数，如果有需要可以添加</param>
        /// <returns>返回异步执行结果</returns>
        public async UniTask<bool> Send_AsyneCommand<T>(IObjectContainer container = null, object data = null) where T : ICommand, new()
        {
            T command = new T();
            return await command.Async_Execute(container, data);
        }
    }
}
