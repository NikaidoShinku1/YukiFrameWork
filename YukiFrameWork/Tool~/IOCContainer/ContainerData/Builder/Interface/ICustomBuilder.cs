using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace YukiFrameWork
{
    public interface ICustomBuilder
    {
        void Register<TInstance>(string name, LifeTime lifeTime = LifeTime.Transient, params object[] args)  where TInstance : class;
        void Register<TInterface, TInstance>(string name, LifeTime lifeTime = LifeTime.Transient, params object[] args) where TInterface : class where TInstance : class;
        void RegisterScopeInstance<TInstance>(string name,TInstance instance) where TInstance : class;
        void RegisterScopeInstance<TInstance>(string name,params object[] args) where TInstance : class;
        void RegisterScopeInstance<TInterface, TInstance>(string name, params object[] args) where TInterface : class where TInstance : class;
        void RegisterInstance<TInstance>(TInstance instance);
        
        void RegisterComponent<T>(T component,bool includeInactive = false) where T : Component;
        void RegisterComponentInScene<T>(bool includeInactive = false) where T : Component;
        void RegisterComponentInNewPrefab<T>(GameObject gameObject, bool includeInactive = false) where T : Component;
        void RegisterComponentInNewGameObject<T>(string name, bool includeInactive = false) where T : Component;
        
    }
}
