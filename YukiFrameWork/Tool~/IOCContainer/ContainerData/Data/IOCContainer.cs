using System;
using System.Collections.Generic;
using YukiFrameWork.Pools;
using UnityEngine;
using System.Reflection;

namespace YukiFrameWork
{   
    public class IOCContainer : IDisposable
    {
        private readonly Dictionary<Type,ObjectData> dataDicts;
        private readonly Dictionary<string, MonoComponentData> monoDataDicts;

        //保存所有注册的全局单例类型，用于标识容器可访问的类型，如果不存在则要求用户使用标准单例模式
        private readonly static Dictionary<Type, Dictionary<Type, ObjectData>> singletonDicts = new Dictionary<Type, Dictionary<Type, ObjectData>>();

        public IOCContainer() 
        {
            dataDicts = DictionaryPools<Type, ObjectData>.Get();
            monoDataDicts = DictionaryPools<string, MonoComponentData>.Get();           
        }

        private Type parentType;

        public void SetParentType(Type parentType)
        {
            this.parentType = parentType;          
        }

        //对象类引用计数
        private ulong count = 0;
        private ulong componentCount = 0;
        ~IOCContainer()
        {
            Dispose();
        }       

        public void Dispose()
        {
            count = 0;
            foreach (var value in dataDicts.Values)
            {
                value.Clear();
            }
            dataDicts.Clear();
            foreach (var value in monoDataDicts.Values)
            {
                value.Clear();
            }
            monoDataDicts.Clear();            
        }

        public void AddContainer(Type type)
            => CreateContainer(type,type);

        public void AddInterfaceContainer(Type interfaceType,Type type)
            => CreateContainer(interfaceType,type);

        public ObjectData GetContainer(Type type)
            => TryGetContainer(type);

        public ObjectData GetOrAddContainer(Type type)
        {
            if(!dataDicts.ContainsKey(type))
                CreateContainer(type,type);
            return dataDicts[type];
        }

        public void AddComponentContainer(Component component,bool includeInactive)
        {
            if (monoDataDicts.ContainsKey(component.gameObject.name)) return;
            monoDataDicts.Add(component.gameObject.name, new MonoComponentData(componentCount++, component, includeInactive));
            return;
        }

        public MonoComponentData GetComponentContainer(string name)
        {
            monoDataDicts.TryGetValue(name, out var data);
            return data;
        }     

        public ObjectData GetOrAddContainer(Type interfaceType,Type type)
        {
            if (!dataDicts.ContainsKey(interfaceType))
                CreateContainer(interfaceType, type);
            return dataDicts[interfaceType];
        }

        public bool CheckSingletonType(Type type)
        {
            foreach (var key in singletonDicts.Keys)
            {
                if(key.IsSubclassOf(parentType))
                    return true;
            }
            if (!singletonDicts.ContainsKey(parentType))
                return false;

            return singletonDicts[parentType].ContainsKey(type);
        }

        public ObjectData GetOrAddSingleton<TInterface, TInstance>()
        {
            return GetOrAddSingleton(typeof(TInterface), typeof(TInstance));
        }

        public ObjectData GetOrAddSingleton<TInstance>()
        {
            return GetOrAddSingleton(typeof(TInstance));
        }

        public ObjectData GetOrAddSingleton(Type instanceType)
        {
            var dict = GetSingletonDict();           
            if (dict == null)
            {
                Debug.LogError("查找不到父类");
            }

            if (!dict.ContainsKey(instanceType))
            {             
                dict.Add(instanceType, new ObjectData(0, instanceType));
            }
            return dict[instanceType];
        }

        public ObjectData GetOrAddSingleton(Type interfaceType, Type instanceType)
        {
            var dict = GetSingletonDict();

            if (dict == null)
            {
                Debug.LogError("查找不到父类");
            }

            if (!dict.ContainsKey(interfaceType))
                dict.Add(interfaceType, new ObjectData(0, instanceType));
            return dict[interfaceType];
        }

        private Dictionary<Type, ObjectData> GetSingletonDict()
        {
            if (parentType == null) return null;
            if (!singletonDicts.TryGetValue(parentType, out var dict))
            {
                bool created = true;
                foreach (var key in singletonDicts.Keys)
                {
                    if (key.IsSubclassOf(parentType))
                    {
                        created = false;
                        dict = singletonDicts[key];
                        break;
                    }
                }
                if (created)
                {
                    dict = new Dictionary<Type, ObjectData>();
                    singletonDicts.Add(parentType, dict);
                }
            }
            return dict;
        }

        /// <summary>
        /// 检查容器是否存在并获取
        /// </summary>
        /// <param name="interfaceType">接口类型(当不使用接口隔离注册时等于实例类型)</param>
        /// <param name="type">实例类型</param>
        /// <returns></returns>
        private ObjectData TryGetContainer(Type interfaceType)
        {
            dataDicts.TryGetValue(interfaceType, out var data);                                     
            return data;
        }

        private void CreateContainer(Type interfaceType, Type type)
        {
            if (dataDicts.ContainsKey(interfaceType)) return;      
            count++;           
            dataDicts.Add(interfaceType, new ObjectData(count, type));
        }

        public void SettingInParameter(Type parentType,ParameterInfo parameter, out object obj)
        {
            obj = null;
            if (CheckSingletonType(parameter.ParameterType))
            {
                var defaultdata = GetOrAddSingleton(parameter.ParameterType);              
                obj = defaultdata.GetInstance(this);
            }
            else
            {
                if (parameter.ParameterType.IsSubclassOf(typeof(Component)))
                {
                    foreach (var data in monoDataDicts.Values)
                    {
                        obj = data.GetOrAddComponent(parameter.ParameterType, this);
                        if (obj != null)
                            break;
                    }
                }
                else
                {
                    if (obj == null)
                    {
                        var defaultdata = GetContainer(parameter.ParameterType);
                        if (defaultdata != null)
                        {
                            obj = defaultdata.GetInstance(this);
                            if (obj == null)
                            {
                                obj = defaultdata.GetTransientInstance(this);
                            }
                        }
                    }
                }
            }
            if (obj == null)
            {
                Debug.LogWarningFormat("类型:{0} 中的特性构造方法内的参数: {1}没有注册进容器，将保持空对象传递！", parentType.Name, parameter.Name);
            }
        }
    }
}