namespace YukiFrameWork.IOC
{
    public interface IEasyBuilder
    {
        void Register<TInstance>(LifeTime lifeTime = LifeTime.Transient) where TInstance : class;
        void Register<TInterface, TInstance>(LifeTime lifeTime = LifeTime.Transient) where TInterface : class where TInstance : class,TInterface;
        void RegisterScopeInstance<TInterface, TInstance>() where TInterface : class where TInstance : class,TInterface;
        void RegisterScopeInstance<TInstance>(TInstance instance) where TInstance : class;
        void RegisterScopeInstance<TInstance>() where TInstance : class;
        void RegisterInstance<TInstance>() where TInstance : class;
        void RegisterInstance<TInterface,TInstance>() where TInterface : class where TInstance : class, TInterface;
    }
}
