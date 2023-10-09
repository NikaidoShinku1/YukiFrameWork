using Cysharp.Threading.Tasks;
using YukiFrameWork.MVC;
namespace YukiFrameWork.Command
{
    /// <summary>
    /// ����ӿ�
    /// </summary>
    public interface ICommand
    {
        //ִ��controller���͵����ͬʱ����C��Ӧ����������������֪ͨ
        bool Execute(IObjectContainer Container = null,object data = null);

        UniTask<bool> Async_Execute(IObjectContainer Container = null,object data = null);

    }

}