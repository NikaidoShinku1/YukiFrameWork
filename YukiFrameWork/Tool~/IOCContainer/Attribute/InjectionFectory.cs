using YukiFrameWork;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
namespace YukiFrameWork.Extension
{
    public static class InjectionFectory
    {
        public static void CreateCustomContainer(LifeTimeScope lifeTimeScope)
        {
            IContainerBuilder builder = new ContainerBuilder();            
            foreach (var info in typeof(LifeTimeScope).GetFields
                (BindingFlags.Instance
                | BindingFlags.NonPublic))
            {
                if (info.GetValue(lifeTimeScope) == null)
                {
                    if (info.Name == "containerBuilder")
                        info.SetValue(lifeTimeScope, builder);
                    else if (info.Name == "container")
                    {
                        System.Type type = typeof(LifeTimeScope);

                        foreach (var fieldinfo in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic))
                        {
                            //判断我们是否找到了这个字段
                            bool fieldEnter = false;
                            foreach (var attribute in fieldinfo.GetCustomAttributes(false))
                            {
                                if (attribute is InjectionContainerAttribute injection)
                                {
                                    fieldEnter = true;
                                    IOCContainer container = injection.FieldInfo.GetValue(builder) as IOCContainer;
                                    info.SetValue(lifeTimeScope, new ObjectResolver(container));
                                    break;
                                }
                            }
                            if (fieldEnter) break;
                        }                      
                    }
                }
                
            }
        }

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
            foreach (var info in monoBehaviour.GetType().GetFields(
                        BindingFlags.NonPublic
                        | BindingFlags.Public
                        | BindingFlags.Static
                        | BindingFlags.Instance                       
                        ))
            {
                foreach (var attribute in info.GetCustomAttributes())
                {
                    if (attribute is InjectAttribute inject)
                    {
                        InjectParameterInField(inject, info, monoBehaviour,container);
                    }
                }
            }

            foreach (var info in monoBehaviour.GetType().GetProperties(
                       BindingFlags.NonPublic
                       | BindingFlags.Public
                       | BindingFlags.Static
                       | BindingFlags.Instance
                       ))
            {
                foreach (var attribute in info.GetCustomAttributes())
                {
                    if (attribute is InjectAttribute inject)
                    {
                        InjectParameterInPropertices(inject, info, monoBehaviour,container);
                    }
                }
            }
        }

        private static void InjectParameterInPropertices(InjectAttribute inject,PropertyInfo info,MonoBehaviour monoBehaviour, IResolveContainer container)
        {
            if (info.GetValue(monoBehaviour) == null)
            {
                string pathOrName = inject.Path;
                object obj = null;
                if (info.PropertyType.IsSubclassOf(typeof(Component)))
                {
                    obj = pathOrName !=
                        string.Empty ? monoBehaviour.transform.Find(pathOrName).GetComponent(info.PropertyType)
                        : monoBehaviour.GetComponentInChildren(info.PropertyType, false);
                    if (obj == null && inject.InHierarchy) obj = (Component)UnityEngine.Object.FindObjectOfType(info.PropertyType);                   
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
            if (info.GetValue(monoBehaviour) == null)
            {
                object obj = null;
                string pathOrName = string.Empty;
                if (info.FieldType.IsSubclassOf(typeof(Component)))
                {
                    pathOrName = inject.Path;
                    obj = pathOrName !=
                        string.Empty ? monoBehaviour.transform.Find(pathOrName).GetComponent(info.FieldType)
                        : monoBehaviour.GetComponentInChildren(info.FieldType, false);
                    if (obj == null && inject.InHierarchy) obj = (Component)UnityEngine.Object.FindObjectOfType(info.FieldType);
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
