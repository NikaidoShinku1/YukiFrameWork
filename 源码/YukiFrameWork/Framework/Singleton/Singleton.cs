using System;
namespace YukiFrameWork
{
    public abstract class Singleton<T> : ISingletonKit , IDisposable where T : class
    {
        protected static T instance;

        private static object _lock = new object();

        public static T Instance
        {
            get
            {
                lock (_lock)
                {
                    instance ??= SingletonFectory.CreateSingleton<T>();
                }
                return instance;
            }
        }   

        public static T I => Instance;

        public virtual void OnInit() { }

        public virtual void OnDestroy()
        {
            instance = null;
        }

        public void Dispose()
        {
            OnDestroy();
        }
    }
}
