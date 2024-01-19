namespace YukiFrameWork
{
    public interface IEasyBuilder
    {
        void Register<TInstance>(LifeTime lifeTime,params object[] args) where TInstance : class;
        void Register<TInterface, TInstance>(LifeTime lifeTime = LifeTime.Transient, params object[] args) where TInterface : class where TInstance : class;
        void RegisterScopeInstance<TInterface, TInstance>(params object[] args) where TInterface : class where TInstance : class;
        void RegisterScopeInstance<TInstance>(TInstance instance) where TInstance : class;
        void RegisterScopeInstance<TInstance>(params object[] args) where TInstance : class;
        void RegisterInstance<TInstance>(params object[] args) where TInstance : class;
        void RegisterInstance<TInterface,TInstance>(params object[] args) where TInterface : class where TInstance : class;
    }
}
