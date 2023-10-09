using System.Collections.Generic;
using System;
namespace YukiFrameWork.ECS
{
    public class EntityData
    {
        public Dictionary<Type, IComponentData> components = new Dictionary<Type, IComponentData>();

        public T GetComponent<T>() 
        {
            Type type = typeof(T);
            if (components.ContainsKey(type)) return (T)components[type];
            return default;
        }

        public T AddComponent<T>(T component, params object[] args) 
        {
            if (component == null)
                component = (T)Activator.CreateInstance(typeof(T), args);

            components.Add(component.GetType(), (IComponentData)component);
            return component;

        }

        public void RemoveComponent<T>()
        {
            foreach (var component in components.Values)
            {
                if (component.GetType() == typeof(T))
                {
                    components[typeof(T)] = null;
                    components.Remove(component.GetType());
                    break;
                }
            }
        }

        public void Clear()
        {
            components.Clear();
        }
    }
}
