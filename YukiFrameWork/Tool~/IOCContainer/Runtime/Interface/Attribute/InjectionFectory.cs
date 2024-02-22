using YukiFrameWork;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System;
using YukiFrameWork.Pools;
namespace YukiFrameWork.IOC
{
    public static class InjectionFectory
    {
        public static void SettingParameter(string[] Names, ParameterInfo[] parameters,IOCContainer container, out List<object> list)
        {
            list = ListPools<object>.Get();
            int j = 0;           
            if (Names != default && Names.Length > 0)
            {
                for (j = 0; j < Mathf.Min(parameters.Length, Names.Length); j++)
                {
                    if (string.IsNullOrEmpty(Names[j])) Names[j] = parameters[j].ParameterType.Name;
                    list.Add(parameters[j].ParameterType != typeof(IEntryPoint) ? container.Resolve(parameters[j].ParameterType, Names[j]) : container.ResolveEntry(parameters[j].ParameterType, Names[j]));
                }
            }

            for (_ = j; j < parameters.Length; j++)
            {             
                list.Add(parameters[j].ParameterType != typeof(IEntryPoint) ? container.Resolve(parameters[j].ParameterType, parameters[j].ParameterType.Name) : container.ResolveEntry(parameters[j].ParameterType, parameters[j].ParameterType.Name));
            }           

        }
        public static void Inject(object obj,IOCContainer container,Type type)
        {
            MemberInfo[] memberInfos = type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Static | BindingFlags.Default);

            for (int i = 0; i < memberInfos.Length;i++)
            {
                MemberInfo info = memberInfos[i];
                InjectAttribute inject = info.GetCustomAttribute<InjectAttribute>();
                if (inject == null) continue;

                if (info is MethodInfo methodInfo)
                {
                    SettingParameter(inject.Names, methodInfo.GetParameters(), container, out var list);
                    methodInfo.Invoke(obj, list.ToArray());
                }
            }

            for (int i = 0; i < memberInfos.Length; i++)
            {
                MemberInfo info = memberInfos[i];

                InjectAttribute inject = info.GetCustomAttribute<InjectAttribute>();

                if (inject == null) continue;

                string name = (inject.Names != default && inject.Names.Length > 0) ? inject.Names[0] : default;

                if (info is FieldInfo fieldInfo)
                {                   
                    name ??= fieldInfo.FieldType.Name;                  
                    fieldInfo.SetValue(obj, container.Resolve(fieldInfo.FieldType,name));
                }
                else if (info is PropertyInfo propertyInfo)
                {
                    name ??= propertyInfo.PropertyType.Name;

                    propertyInfo.SetValue(obj, container.Resolve(propertyInfo.PropertyType, name));
                }              
            }
        }

        public static void InjectGameObjectInMonoBehaviour(GameObject gameObject,IOCContainer container)
        {
            MonoBehaviour[] monos = gameObject.GetComponents<MonoBehaviour>();

            foreach (var mono in monos)
            {
                if (mono is IInjectContainer c)
                    c.Container = container;
                Inject(mono, container, mono.GetType());
            }      
        }
    }
}
