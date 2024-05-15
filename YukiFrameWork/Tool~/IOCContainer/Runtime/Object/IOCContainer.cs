using System;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork.IOC
{
    public class IOCContainer : IDisposable, IResolveContainer
    {
        private Dictionary<Type, Dictionary<string,IEntryPoint>> containers = new Dictionary<Type, Dictionary<string, IEntryPoint>>();

        private static IOCContainer localContainer = new IOCContainer();
        public void Dispose()
        {
            Clear();
            localContainer.Clear();
        }

        public void RegisterDObject(Type type, string name, LifeTime lifeTime)
        {
            RegisterDObjectInInterface(type, type, name, lifeTime);
        }
        public void RegisterDObjectInInterface(Type interfaceType,Type type, string name, LifeTime lifeTime)
        {
            if (!localContainer.Contains(interfaceType) && lifeTime == LifeTime.Singleton)               
                LocalAdd(interfaceType, new DObject(name,lifeTime,type,interfaceType,this));
            else if(lifeTime != LifeTime.Singleton)
                Add(name, interfaceType, new DObject(name, lifeTime, type,interfaceType,this));          
        }

        public void RegisterDObject<T>(string name,T instance,bool localPoint = false)
        {
            if (localPoint)
                LocalAdd(typeof(T), new DObject(name, instance, this,LifeTime.Singleton));
            else
                Add(name, typeof(T), new DObject(name, instance,this,LifeTime.Scoped));
        }

        public void RegisterMObject<T>(T component, string name, LifeTime lifeTime) where T : Component
        {
            RegisterMObjectInInferface(typeof(T), component, name, lifeTime);
        }

        public void RegisterMObjectInInferface<T>(Type interfaceType,T component, string name, LifeTime lifeTime) where T : Component
        {
            if(!localContainer.Contains(interfaceType) && lifeTime == LifeTime.Singleton)                        
                LocalAdd(interfaceType, new MObject(name, lifeTime, component, this, interfaceType));
            else if (lifeTime != LifeTime.Singleton)
                Add(name, interfaceType, new MObject(name, lifeTime, component, this, interfaceType));
        }    

        private void Add(string name,Type type, IEntryPoint entryPoint)
        {
            if (containers.ContainsKey(type))
            {
                if (containers[type].ContainsKey(name))
                    containers[type][name] = entryPoint;
                else
                    containers[type].Add(name, entryPoint);
            }
            else
            {
                 containers.Add(type, new Dictionary<string, IEntryPoint> { {name,entryPoint } });
            }
        }

        private void LocalAdd(Type type, IEntryPoint entryPoint)
        {
            localContainer.Add(type.Name, type, entryPoint);
        }

        private bool Contains(Type type, string name)
        {
            if (!containers.ContainsKey(type)) return false;

            return containers[type].ContainsKey(name);
        }

        private bool Contains(Type type)
        {
            return Contains(type, type.Name);
        }

        public object Get(Type type,string name)
        {            
            if (containers.TryGetValue(type, out var dict))
            {
                if (dict.TryGetValue(name, out var point))
                {                  
                    return point.Value;
                }
            }

            return null;      
        }

        public bool UnRegister(Type type)
        {
            bool normal = containers.ContainsKey(type);
            bool local = localContainer.containers.ContainsKey(type);
            if (!normal && !local)
                return false;

            if (local)
            {
                foreach (var point in localContainer.containers[type].Values)
                {                    
                    point.Destroy();
                }
            }
            
            if(normal)
                foreach (var point in containers[type].Values)
                {
                    point.Destroy();
                }

            bool Un = containers.Remove(type);
            bool localUn = localContainer.containers.Remove(type);
            return Un || localUn;
        }

        public bool UnRegister(Type type, string name)
        {
            bool normal = containers.ContainsKey(type);
            bool local = localContainer.containers.ContainsKey(type);
            if (!normal && !local) return false;

            try
            {
                if (local)
                    localContainer.containers[type][name].Destroy();

                if(normal)
                    containers[type][name].Destroy();               
            }
            catch
            {
                throw new Exception("试图释放没有注册过的实例!实例类型:" + type + "注册名称:" + name);
            }
            bool Un = false;
            bool localUn = false;
            if (local)
                localUn = localContainer.containers[type].Remove(name);
            if(normal)
                Un = containers[type].Remove(name);
            return Un || localUn;
        }

        public void Clear()
        {
            foreach (var container in containers.Values)
            {
                foreach (var point in container.Values)
                {
                    point.Destroy();
                }
            }

            containers.Clear();            
        }        

        private object DynamicResolve(Type type, string name)
        {
            var obj = localContainer.Get(type, name);          
            obj ??= Get(type, name);          
            return obj;
        }

        public object Resolve(Type type, string name)
        {
            return DynamicResolve(type, name);
        }

        public object Resolve(Type type)
        {
            return DynamicResolve(type, type.Name);
        }      

        public bool Destroy(Type type, string name)
        {
            return UnRegister(type, name);
        }

        public bool Destroy(Type type)
        {
            return Destroy(type, type.Name);
        }

        public IEntryPoint ResolveEntry(Type type, string name)
        {
            IEntryPoint point = localContainer.GetEntry(type, name);
            if (point == null) point = GetEntry(type, name);
            return point;
        }

        private IEntryPoint GetEntry(Type type, string name)
        {
            IEntryPoint point;           
            if (containers.TryGetValue(type, out var dict))
            {
                if (dict.TryGetValue(name, out point))
                {
                    return point;
                }
            }
            return null;
        }
    }
}
