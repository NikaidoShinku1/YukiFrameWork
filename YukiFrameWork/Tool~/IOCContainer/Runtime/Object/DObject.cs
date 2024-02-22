using System.Collections.Generic;
using System;
using System.Reflection;
using System.Linq;
using UnityEngine;
namespace YukiFrameWork.IOC
{
    public class DObject : IOCObject<object>
    {
        private object value;
        
        private object[] args;
        private Type interfaceType;
        private Stack<object> transient = new Stack<object>();
        public DObject(string name, LifeTime lifeTime, Type type,Type interfaceType, IOCContainer container) : base(name, lifeTime, container,type)
        {                     
            if (lifeTime != LifeTime.Transient)
                value = CreateInstance();

            this.interfaceType = interfaceType;
        }

        public DObject(string name,object value, IOCContainer container,LifeTime lifeTime) 
        {
            this.value = value;
            this.type = value.GetType();
            this.container = container;
            this.name = name;
            this.lifeTime = lifeTime;
            InjectionFectory.Inject(this.value, container, type);
            if (this.value is IStartable start)
                start.Start();

            IOCHelper.AddTick(this.value);
        }

        public override void Dispose()
        {
            if (!IsAutoRelease)
            {
                IsAutoRelease = true;
                container.UnRegister(interfaceType, Name);
                return;
            }
            IOCHelper.ClearTick(value);

            if (value is GameObject obj)
                obj.Destroy();
            else if (value is IReleaseTickable release)
                release.Release();
            value = null;
            type = null;

            ///释放缓存中所有的瞬时实例
            foreach (var t in transient)
            {
                IOCHelper.ClearTick(t);
                if (t is IReleaseTickable releaset)
                    releaset.Release();
            }
            IsDestroy = true;
            transient.Clear();
        }

        public override object Get()
        {
            if (IsDestroy) return null;
            return lifeTime switch
            {
                LifeTime.Transient => CreateInstance(),
                LifeTime.Scoped => value,
                LifeTime.Singleton => value,
                _ => null
            };
        }                  

        private object CreateInstance()
        {
            InjectAttribute inject = null;
            ConstructorInfo constructor = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance).Where(x => 
            {
                inject = x.GetCustomAttribute<InjectAttribute>();
                return inject != null;
            }).FirstOrDefault();

            if (constructor == null)
                args = default;
            else
            {
                InjectionFectory.SettingParameter(inject.Names, constructor.GetParameters(), container, out var list);
                args = list.ToArray();
            }          
            object obj = type.CreateInstance(x => 
            {
                if (x is IInjectContainer container)
                    container.Container = this.container;             
            },args);
            //如果是瞬时的实例缓存起来,方便释放
            if (lifeTime == LifeTime.Transient)
                transient.Push(obj);
           
            InjectionFectory.Inject(obj, this.container, type);
            if (obj is IStartable start)
                start.Start();
            return obj;
        }
    }
}
