using System;
namespace YukiFrameWork
{
    public abstract class Singleton<T> where T : Singleton<T>
    {
        protected readonly static T instance = (T)Activator.CreateInstance(typeof(T), true);

        public static T Instance
        {
            get
            {
                return instance;
            }
        }

        public virtual void Init() { }

        public virtual void Destroy() { }
    }
}
