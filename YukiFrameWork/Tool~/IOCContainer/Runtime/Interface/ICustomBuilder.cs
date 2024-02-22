using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace YukiFrameWork.IOC
{
    public interface ICustomBuilder
    {
        void Register<TInstance>(string name, LifeTime lifeTime = LifeTime.Transient)  where TInstance : class;
        void Register<TInterface, TInstance>(string name, LifeTime lifeTime = LifeTime.Transient) where TInterface : class where TInstance : class, TInterface;
        void RegisterScopeInstance<TInstance>(string name,TInstance instance) where TInstance : class;
        void RegisterScopeInstance<TInstance>(string name) where TInstance : class;
        void RegisterScopeInstance<TInterface, TInstance>(string name) where TInterface : class where TInstance : class, TInterface;
        void RegisterInstance<TInstance>(TInstance instance);
        
        void RegisterComponent<T>(T component) where T : Component;
        void RegisterComponent<T>(string name,T component) where T : Component;
        void RegisterComponentInHierarchy<T>(bool includeInactive = false) where T : Component;     
        void RegisterComponentInHierarchy<T>(string path) where T : Component;       
        void RegisterComponentInScene<T>(bool includeInactive = false) where T : Component;
        void RegisterComponentInScene<T>(string name,bool includeInactive = false) where T : Component;
        void RegisterComponentInNewPrefab<T>(GameObject gameObject, bool findChild = false) where T : Component;
        void RegisterComponentInNewGameObject<T>(string name) where T : Component;
        
    } 
}
