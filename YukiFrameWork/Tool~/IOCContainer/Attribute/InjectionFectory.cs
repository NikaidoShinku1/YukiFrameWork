using YukiFrameWork;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System;
namespace YukiFrameWork.Extension
{
    public static class InjectionFectory
    {      
        public static void InjectGameObjectInMonoBehaviour(IResolveContainer container, GameObject gameObject, out List<MonoBehaviour> buffers)
        {           
            MonoBehaviour[] monoBehaviours = gameObject.GetComponents<MonoBehaviour>();

            buffers = new List<MonoBehaviour>(monoBehaviours);          

            foreach (var monobehaviour in buffers)
            {
                SetAllFieldsAndPropertices(monobehaviour,container);                                   
            }                    
        }     

        private static void SetAllFieldsAndPropertices(MonoBehaviour monoBehaviour,IResolveContainer container)
        {
            foreach (var info in monoBehaviour.GetType().GetMembers(
                        BindingFlags.NonPublic
                        | BindingFlags.Public
                        | BindingFlags.Static
                        | BindingFlags.Instance                       
                        ))
            {
                InjectAttribute inject = info.GetCustomAttribute<InjectAttribute>();

                if (inject == null) continue;             
                if (info is FieldInfo fieldInfo)
                    InjectParameterInField(inject, fieldInfo, monoBehaviour, container);
                else if (info is PropertyInfo propertyInfo)
                    InjectParameterInPropertices(inject, propertyInfo, monoBehaviour, container);
            }

            IInjectContainer c = monoBehaviour as IInjectContainer;
            if (c == null) return;

            c.Container = LifeTimeScope.scope.Container;
        }

        private static void InjectParameterInPropertices(InjectAttribute inject,PropertyInfo info,MonoBehaviour monoBehaviour, IResolveContainer container)
        {
            object value = info.GetValue(monoBehaviour);

            if (value == null || value.ToString() == "null")
            {
                string pathOrName = inject.Path;
                object obj = null;
                if (info.PropertyType.IsSubclassOf(typeof(Component)))
                {
                    obj = pathOrName !=
                            string.Empty ? (inject.InHierarchy ? GameObject.FindGameObjectWithTag(pathOrName).GetComponent(info.PropertyType) : monoBehaviour.transform.Find(pathOrName).GetComponent(info.PropertyType))
                            : monoBehaviour.GetComponentInChildren(info.PropertyType, false);

                    if(inject.InHierarchy)
                        obj ??= (Component)UnityEngine.Object.FindObjectOfType(info.PropertyType);
                }
                else
                {
                    obj = container.Resolve(info.PropertyType,pathOrName);
                }
                info.SetValue(monoBehaviour, obj);
            }
        }

        private static void InjectParameterInField(InjectAttribute inject,FieldInfo info,MonoBehaviour monoBehaviour,IResolveContainer container)
        {
            object value = info.GetValue(monoBehaviour);

            if (value == null || value.ToString() == "null")
            {              
                object obj = null;
                string pathOrName = string.Empty;
                if (info.FieldType.IsSubclassOf(typeof(Component)))
                {
                    pathOrName = inject.Path;
                                   
                        obj = pathOrName !=
                            string.Empty ? (inject.InHierarchy ? GameObject.FindGameObjectWithTag(pathOrName).GetComponent(info.FieldType) : monoBehaviour.transform.Find(pathOrName).GetComponent(info.FieldType))
                            : monoBehaviour.GetComponentInChildren(info.FieldType, false);
                    if (inject.InHierarchy)
                        obj ??= (Component)UnityEngine.Object.FindObjectOfType(info.FieldType);
                }
                else
                {
                    pathOrName = inject.Path;
                    obj = container.Resolve(info.FieldType, pathOrName);
                }      
                info.SetValue(monoBehaviour, obj);
            }
        }
    }
}
