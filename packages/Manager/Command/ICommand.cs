using Cysharp.Threading.Tasks;
using YukiFrameWork.MVC;
namespace YukiFrameWork.Command
{
    /// <summary>
    /// 命令接口
    /// </summary>
    public interface ICommand
    {
        //执行controller发送的命令，同时接收C对应的容器，方便命令通知
        bool Execute(IObjectContainer Container = null,object data = null);

        UniTask<bool> Async_Execute(IObjectContainer Container = null,object data = null);

    }

}