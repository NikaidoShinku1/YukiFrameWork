
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