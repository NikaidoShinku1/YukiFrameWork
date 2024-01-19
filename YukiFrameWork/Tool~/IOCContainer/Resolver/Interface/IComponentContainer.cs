using System;
using UnityEngine;


namespace YukiFrameWork
{
    public interface IComponentContainer 
    {
        TComponent ResolveComponent<TComponent>(string objName,string componentName) where TComponent : Component;
        TComponent ResolveComponent<TComponent>(string objName) where TComponent : Component;

        object ResolveComponent(Type type, string objName, string componentName);
        object ResolveComponent(Type type, string objName);

    }
}  
