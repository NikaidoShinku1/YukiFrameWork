namespace YukiFrameWork.MVC
{
    public interface IView : IGetModel, IGetRegisterEvent,ISetArchitecture,IGetEventTrigger
    {               
        
        /// <summary>
        /// 视图层初始化函数，提供体系模块参数
        /// </summary>
        /// <param name="Architecture">体系模块</param>
        void Init();
         
        /// <summary>
        /// 统一更新视图(当这个视图只针对一个Model进行数据的视图更新时可在控制器注册使用)
        /// </summary>
        void Unified_UpdateView(IModel model);
    }
}
