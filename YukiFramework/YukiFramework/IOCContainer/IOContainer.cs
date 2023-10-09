using System.Collections.Generic;
using UnityEngine;
using System;

namespace YukiFrameWork
{
    public enum LifeTime
    {
        //˲ʱʵ��,ÿ�λ�ȡ�����õ��µ�ʵ��
        Transient,      
        //����ʵ�����ڷ�֧��Ψһ����ͬ��֧��ᴴ���µ�ʵ��
        Scope,
        //ȫ��ʵ����ȫ��Ψһ������ģʽ
        Singleton,
    }

    public interface IContainerBuilder 
    {
        ObjectContainer Container { get;set; }
        bool IsDebugLog { get; set; }
       
        void Register<TInterface,Instance>(LifeTime lifeTime = LifeTime.Transient, params object[] args) where Instance : class;
        void Register<T>(LifeTime lifeTime = LifeTime.Transient, params object[] args) where T : class;   
        void Register(Type interfaceType, Type instanceType, LifeTime lifeTime = LifeTime.Transient, params object[] args);   
        void Register(Type type, LifeTime lifeTime = LifeTime.Transient, params object[] args);

        void RegisterScopeInstance<T>(params object[] args) where T : class;
        void RegisterScopeInstance<T>(T instance) where T : class;
        void RegisterScopeInstance<TInterface, Instance>(params object[] args) where Instance : class;
        void RegisterScopeInstance<TInterface, Instance>(Instance instance) where Instance : class;       
        void RegisterScopeInstance(Type type, params object[] args);
        void RegisterScopeInstance(Type interfaceType, Type instanceType, params object[] args);

        void RegisterInstance<T>(params object[] args) where T : class;
        void RegisterInstance<T>(T instance) where T : class;
        void RegisterInstance<TInterface, Instance>(params object[] args) where Instance : class;
        void RegisterInstance<TInterface, Instance>(Instance instance) where Instance : class;      
        void RegisterInstance(Type type,params object[] args);
        void RegisterInstance(Type interfaceType, Type instanceType, params object[] args);

        void AutoRegisterMono(Type type, MonoBehaviour instance,bool isStatic);
        [Obsolete]
        void RegisterMono<T>(bool isStatic = false) where T : MonoBehaviour;
        [Obsolete]
        void RegisterMono<T>(GameObject obj, bool isStatic = false) where T : MonoBehaviour;
    }

    public interface IObjectContainer
    {
        T Resolve<T>() where T : class;              
        object InstanceInject<T>(object obj);
        bool Equals(Type type, LifeTime life = LifeTime.Transient);
        List<object> GetAllScopeInstance();
        List<object> GetAllSingleton();
    }

    public class IOCContainer : IDisposable
    {        
        //���Ʒ�֧Ψһʵ������
        protected readonly Dictionary<Type, object> instanceDict = new Dictionary<Type, object>();

        protected readonly static Dictionary<Type, object> singletonDict = new Dictionary<Type, object>();

        //˲ʱʵ����������
        protected readonly HashSet<Type> transientDict = new HashSet<Type>();

        protected readonly Dictionary<Type, Type> restrainTransientDict = new Dictionary<Type, Type>();
        
        //��������ע��ʱ���캯�������еĲ���
        public Dictionary<Type,object[]> transientObject = new Dictionary<Type, object[]>();
        protected IOCContainer() { }

        public void AddScopeInstance<T>(Type type,T instance) where T : class
        {
            if (instanceDict.ContainsKey(type))
            {
                Debug.LogWarning($"��ǰʵ���ڵ�ǰ��֧���Ѵ��ڣ�ʵ������Ϊ{instance.GetType()}");
                return;
            }
            instanceDict.Add(type, instance);
        }

        public void AddSingleton<T>(Type type, T instance) where T : class
        {
            if(singletonDict.ContainsKey(type))
            {
                //Debug.LogWarning($"��ǰʵ����ȫ�����Ѵ��ڣ�ʵ������Ϊ{instance.GetType()}");
                return;
            }
            singletonDict.Add(type, instance);
        }

        public void AddTransient(Type type,params object[] args)
        {
            if (transientDict.Contains(type)) return;
            transientDict.Add(type);
           
            StorageConStructObject(type, args);
        }

        public void AddRestrainTransient(Type interfaceType, Type instanceType,params object[] args)
        {
            restrainTransientDict[interfaceType] = instanceType;
            StorageConStructObject(interfaceType, args);
        }

        public object GetScopeInstance(Type type)
        {
            return instanceDict[type];
        }

        public object GetSingleton(Type type)
        {
            return singletonDict[type];
        }

        public object[] GetConStructObjects(Type type)
        {
            return transientObject[type];
        }

        public void StorageConStructObject(Type type,params object[] args)
        {        
            transientObject.Add(type,args);
        }

        /// <summary>
        /// �ͷ������±���Ĺ��캯������
        /// </summary>
        public void RemoveConStructionObject(Type type)
        {
            if (transientObject.ContainsKey(type))
            {
                transientObject[type] = null;
                transientObject.Remove(type);
            }
        }
       
        public void Clear()
        {
            transientDict.Clear();
            transientObject.Clear();
            instanceDict.Clear();       
            singletonDict.Clear();
        }

        public void Dispose()
        {
            Clear();
        }
    }
}
