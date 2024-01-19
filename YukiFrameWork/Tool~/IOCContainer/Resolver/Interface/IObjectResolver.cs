using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace YukiFrameWork
{
    public interface IResolveContainer : IComponentContainer, IDefaultContainer
    {
        
    }

    public class ObjectResolver : IResolveContainer
    {
        private IOCContainer container = null;

        public ObjectResolver(IOCContainer container)
        {
            Init(container);
        }

        private void Init(IOCContainer container) 
        {
            this.container = container;         
        }
        
        public T Resolve<T>() where T : class
        {
            return DynamicResolve(typeof(T),string.Empty) as T;
        }

        public T Resolve<T>(string name) where T : class
        {
            return DynamicResolve(typeof(T),name) as T;
        }

        private object DynamicResolve(Type type,string name)
        {
            ObjectData data = null;
            //先判断这个实例它是不是作为全局实例被我们注册进来了
            if (container.CheckSingletonType(type))
            {
                data = container.GetOrAddSingleton(type);              
                return data?.GetInstance(container,string.Empty);
            }
            //判断我们有没有注册相匹配的容器类型
            data = container.GetContainer(type);
            if (data == null)
            {
                Debug.LogError("实例解析错误！无法获取与解析类型匹配的容器数据！类型：" + type);
                return null;
            }
            //从容器中取出实例
            var instance = data.GetInstance(container,name);

            //如果没有取出实例我们就实例化一个瞬时实例
            if (instance == null)
            {
                Debug.Log("实例匹配但不在注册范围内(或许名称不一致),将解析瞬时实例！实例类型：" + type);
                try
                {
                    if (!name.Equals(string.Empty) || !data.Contains(name))
                    {
                        instance = data.GetTransientInstance(container, string.Empty);
                    }
                    else
                    {
                        instance = data.GetTransientInstance(container, name);
                    }
                }
                catch
                {
                    throw new NullReferenceException("创建瞬时实例失败！请检查实例是否注册或注册类型属于单例！类型：" + type);
                }
            }           
            return instance;
        }
      
        public TComponent ResolveComponent<TComponent>(string objName, string componentName) where TComponent : Component
        {          
            var data = container.GetComponentContainer(objName);
            return data.GetOrAddComponent<TComponent>(componentName,container);
        }

        public TComponent ResolveComponent<TComponent>(string objName) where TComponent : Component
        {            
            var data = container.GetComponentContainer(objName);
            return data.GetOrAddComponent<TComponent>(container);
        }

        public object ResolveComponent(Type type, string objName, string componentName)
        {            
            var data = container.GetComponentContainer(objName);
            return data.GetOrAddComponent(type,componentName,container);
        }

        public object ResolveComponent(Type type, string objName)
        {            
            var data = container.GetComponentContainer(objName);
            return data.GetOrAddComponent(type,container);
        }

        public object Resolve(Type type, string name = "")
        {
            return DynamicResolve(type, name);
        }
    }
}
