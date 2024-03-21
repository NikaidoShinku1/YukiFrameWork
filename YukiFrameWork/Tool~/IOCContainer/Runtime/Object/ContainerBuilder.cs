using System;
using UnityEngine;

namespace YukiFrameWork.IOC
{   
    public class ContainerBuilder : IContainerBuilder
    {
        private IOCContainer container = new IOCContainer();
        public IResolveContainer Container => container;      
        private bool CheckTypeOrComponent(Type type)
        {
            return type.IsSubclassOf(typeof(UnityEngine.Component));
        }

        public void Register<TInstance>(LifeTime lifeTime = LifeTime.Transient) where TInstance : class
        {
            Register(typeof(TInstance).Name, typeof(TInstance),lifeTime);
        }

        public void Register<TInterface, TInstance>(LifeTime lifeTime = LifeTime.Transient)
            where TInterface : class
            where TInstance : class,TInterface
        {
            Register<TInterface, TInstance>(typeof(TInterface).Name, lifeTime);
        }

        public void Register(Type type, LifeTime lifeTime = LifeTime.Transient)
        {
            Register(type.Name, type, lifeTime);
        }

        public void Register(string name, Type type, LifeTime lifeTime = LifeTime.Transient)
        {
            if (CheckTypeOrComponent(type))
                Debug.LogWarning("对于UnityEngine.Object的对象，应该使用RegisterComponent进行注册!");
            container.RegisterDObject(type, name, lifeTime);
        }

        public void Register<TInstance>(string name, LifeTime lifeTime = LifeTime.Transient) where TInstance : class
        {
            Register(name, typeof(TInstance), lifeTime);
        }

        public void Register<TInterface, TInstance>(string name, LifeTime lifeTime = LifeTime.Transient)
            where TInterface : class
            where TInstance : class, TInterface
        {
            if (CheckTypeOrComponent(typeof(TInstance)))
                Debug.LogWarning("对于UnityEngine.Object的对象，应该使用RegisterComponent进行注册!");
            container.RegisterDObjectInInterface(typeof(TInterface), typeof(TInstance), name, lifeTime);
        }

        public void RegisterComponent<T>(T component) where T : Component
        {
            RegisterComponent(component.gameObject.name, component);
        }      

        public void RegisterComponent<T>(string name, T component) where T : Component
        {
            container.RegisterMObject(component, name, LifeTime.Scoped);
        }

        public void RegisterComponentInNewGameObject<T>(string name) where T : Component
        {
            GameObject obj = new GameObject(name);
            T component = obj.AddComponent<T>();

            RegisterComponent<T>(name,component);
        }

        public void RegisterComponentInNewPrefab<T>(GameObject gameObject, bool findChild = false) where T : Component
        {
            T component = findChild ? gameObject.GetComponentInChildren<T>() : gameObject.GetComponent<T>();

            if (component == null || component.ToString() == "null")
            {
                throw new Exception("无法注册，Prefab中的组件没有查找到!组件类型:" + typeof(T));
            }

            RegisterComponent(gameObject.name, component);
        }

        public void RegisterComponentInHierarchy<T>(bool includeInactive = false) where T : Component
        {
            LifeTimeScope scope = LifeTimeScope.scope;

            if (scope == null)
                throw new NullReferenceException();

            T component = scope.GetComponent<T>();
            component ??= scope.GetComponentInChildren<T>(includeInactive);           
            RegisterComponent(component);
        }

        public void RegisterComponentInHierarchy<T>(string path) where T : Component
        {
            LifeTimeScope scope = LifeTimeScope.scope;

            if (scope == null)
                throw new NullReferenceException();

            T Component = scope.transform.Find(path).GetComponent<T>();
            RegisterComponent(Component);
        }

        public void RegisterComponentInScene<T>(bool includeInactive = false) where T : Component
        {
            T component = UnityEngine.Object.FindObjectOfType<T>(includeInactive);
            container.RegisterMObject(component, component.gameObject.name, LifeTime.Scoped);
        }

        public void RegisterComponentInScene<T>(string name, bool includeInactive = false) where T : Component
        {
            T component = UnityEngine.Object.FindObjectOfType<T>(includeInactive);

            container.RegisterMObject(component, name, LifeTime.Scoped);
        }

        public void RegisterInstance<TInstance>() where TInstance : class
        {
            Register<TInstance>(LifeTime.Singleton);
        }

        public void RegisterInstance<TInterface, TInstance>()
            where TInterface : class
            where TInstance : class, TInterface
        {
            Register<TInterface,TInstance>(LifeTime.Singleton);
        }

        public void RegisterInstance<TInstance>(TInstance instance)
        {
            if (CheckTypeOrComponent(typeof(TInstance)))
                throw LogKit.Exception("对于UnityEngine.Component的注册，请使用对应的组件注册方法RegisterComponent!，注册的类型是:" + typeof(TInstance));
            container.RegisterDObject(typeof(TInstance).Name, instance,true);
        }

        public void RegisterScopeInstance<TInterface, TInstance>()
            where TInterface : class
            where TInstance : class, TInterface
        {
            Register<TInterface, TInstance>(LifeTime.Scoped);
        }

        public void RegisterScopeInstance<TInstance>(TInstance instance) where TInstance : class
        {
            RegisterScopeInstance(typeof(TInstance).Name,instance);
        }

        public void RegisterScopeInstance<TInstance>() where TInstance : class
        {
            Register<TInstance>(LifeTime.Scoped);
        }

        public void RegisterScopeInstance<TInstance>(string name, TInstance instance) where TInstance : class
        {
            if (CheckTypeOrComponent(typeof(TInstance)))
                throw LogKit.Exception("对于UnityEngine.Component的注册，请使用对应的组件注册方法RegisterComponent!，注册的类型是:" + typeof(TInstance));
            container.RegisterDObject(name, instance);
        }

        public void RegisterScopeInstance<TInstance>(string name) where TInstance : class
        {
            Register<TInstance>(name, LifeTime.Scoped);
        }

        public void RegisterScopeInstance<TInterface, TInstance>(string name)
            where TInterface : class
            where TInstance : class, TInterface
        {
            Register<TInterface, TInstance>(name, LifeTime.Scoped);
        }

        public void Dispose()
        {
            container.Dispose();
        }       
    }
}