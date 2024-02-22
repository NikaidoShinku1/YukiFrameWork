using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
namespace YukiFrameWork.IOC
{
    public class MObject : IOCObject<Component>
    {
        private Component component;
        
        public MObject(string name, LifeTime lifeTime,Component component,IOCContainer container,Type type) : base(name, lifeTime,container,type)
        {
            this.component = component;

            if (this.component is IInjectContainer c)
                c.Container = container;
            InjectionFectory.Inject(component, container, type);
        }

        public override void Dispose()
        {
            if (!IsAutoRelease)
            {
                IsAutoRelease = true;              
                container.UnRegister(type, Name);              
                return;
            }
            IOCHelper.ClearTick(component);
            if (component.Destroy())
            {
                component = null;
                IsDestroy = true;
            }                  
        }

        public override Component Get()
        {
            if (IsDestroy) return null;
            return component;
        }
    }
}
