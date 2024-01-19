using System;
namespace YukiFrameWork
{
    public abstract class Singleton<T> : ISingletonKit<T>, IDisposable where T : Singleton<T>
    {
        protected static T instance;

        private readonly static object _lock = new object();

        public static T Instance
        {
            get
            {
                lock (_lock)
                {
                    if (instance == null)
                    {
                        instance = SingletonFectory.CreateSingleton<T>();
                        instance.OnInit();
                    }
                    return instance;
                }
            }
        }        

        public static T I => Instance;

        public virtual void OnInit() { }
       
        public virtual void OnDestroy()
        {
            instance = null;            
            SingletonFectory.ReleaseInstance<T>();
        }

        public void Dispose()
        {
            OnDestroy();
        }
    }
}
