using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using YukiFrameWork.Pools;

namespace YukiFrameWork
{
    public class MonoComponentData
    {
        private readonly Dictionary<string, Transform> containerDict = DictionaryPools<string, Transform>.Get();
        private readonly Component dependObj = null;
        private readonly ulong id;
        public MonoComponentData(ulong id, Component dependObj, bool includeInactive)
        {
            this.dependObj = dependObj;
            this.id = id;

            InitComponent(includeInactive);
        }

        public void Add<T>(T component, bool includeInactive, IOCContainer container) where T : Component
        {
            containerDict[component.name] = component.transform;
            InitComponentParameter<T>(component, includeInactive, container);
            InitComponentParameterInPropertices<T>(component, includeInactive, container);
        }

        public void Remove<T>(T component) where T : Component
        {
            if (containerDict.ContainsKey(component.name))
                containerDict.Remove(component.name);
        }

        public void Clear()
        {
            containerDict.Clear();
        }

        private void InitComponent(bool includeInactive)
        {
            Transform[] transforms = dependObj.GetComponentsInChildren<Transform>(includeInactive);

            for (int i = 0; i < transforms.Length; i++)
            {
                containerDict.Add(transforms[i].name, transforms[i]);
            }
        }

        public ulong GetDataID => id;

        /// <summary>
        /// 获取或者添加该类型的组件并保存于字典
        /// </summary>
        /// <typeparam name="T">组件类型</typeparam>
        /// <param name="name">对象名称</param>
        /// <param name="includeInactive">检索时是否包含非活动的对象</param>
        /// <returns>返回一个组件</returns>
        public T GetOrAddComponent<T>(string name, IOCContainer container) where T : Component
        {
            if (!containerDict.TryGetValue(name, out var Tcomponent))
            {
                T c = dependObj.GetComponentInChildren<T>();
                Add(c, false, container);
                return c;
            }
            return Tcomponent.GetComponent<T>();
        }

        public object GetOrAddComponent(Type type, string name, IOCContainer container)
        {
            if (!containerDict.TryGetValue(name, out var Tcomponent))
            {
                Component c = dependObj.GetComponentInChildren(type);
                Add(c, false, container);
                return c;
            }
            return Tcomponent.GetComponent(type);
        }

        public object GetOrAddComponent(Type type, IOCContainer container)
        {
            foreach (var component in containerDict.Values)
            {
                object components = component.GetComponent(type);
                if (components != null)
                {
                    return components;
                }
            }
            object c = dependObj.GetComponentInChildren(type);
            Add((Component)c, false, container);
            return c;
        }

        public T GetOrAddComponent<T>(IOCContainer container) where T : Component
        {
            foreach (var component in containerDict.Values)
            {
                if (component.GetComponent<T>() is T TComponent)
                {
                    return TComponent;
                }
            }
            T c = dependObj.GetComponentInChildren<T>();
            Add(c, false, container);

            return c;
        }

        private void InitComponentParameter<T>(T component, bool includeInactive, IOCContainer container) where T : Component
        {
            Type type = typeof(T);
            foreach (var info in type.GetFields(
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
                        if (info.GetValue(component) == null)
                        {
                            string path = inject.Path;
                            InitExecute(path, info.FieldType, component, includeInactive, inject.InHierarchy, container, out var obj);
                            info.SetValue(component, obj);
                        }
                    }
                }
            }
        }

        private void InitComponentParameterInPropertices<T>(T component, bool includeInactive, IOCContainer container) where T : Component
        {
            Type type = typeof(T);
            foreach (var info in type.GetProperties(
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
                        if (info.GetValue(component) == null)
                        {
                            string path = inject.Path;
                            bool InHierarchy = inject.InHierarchy;
                            InitExecute(path, info.PropertyType, component, includeInactive, InHierarchy, container, out var obj);
                            info.SetValue(component, obj);
                        }
                    }
                }
            }
        }

        private void InitExecute(string path, Type type, Component component, bool includeInactive, bool InHierarchy, IOCContainer container, out object obj)
        {
            if (type.IsSubclassOf(typeof(Component)))
            {
                obj = path != string.Empty ? component.transform.Find(path).GetComponent(type) : component.GetComponentInChildren(type, includeInactive);
                if (obj == null && InHierarchy)
                    obj = UnityEngine.Object.FindObjectOfType(type, includeInactive);
            }
            else
            {
                if (container.CheckSingletonType(type))               
                    obj = container.GetOrAddSingleton(type)?.GetInstance(container);               
                else
                    obj = container.GetContainer(type)?.GetInstance(container, path);
            }
        }
    }

    public class ObjectData
    {
        private readonly ulong id = 0;
        private readonly Dictionary<string, object> containerDict = DictionaryPools<string, object>.Get();

        private readonly Dictionary<string, object[]> paramterDict = DictionaryPools<string, object[]>.Get();

        private readonly Type objectType;
        public Type ObjectType => objectType;
        public ObjectData(ulong id, Type objectType)
        {
            this.id = id;
            containerDict.Add(string.Empty, null);
            this.objectType = objectType;
        }

        public ulong GetDataID => id;

        public void Clear()
        {
            containerDict.Clear();
        }

        public object GetInstance(IOCContainer container, string name = "")
        {
            object data = null;        
            containerDict.TryGetValue(name, out data);
            return data;
        }

        public object GetTransientInstance(IOCContainer container, string name = "")
        {
            paramterDict.TryGetValue(name, out object[] args);
            return CreateInstance(container, args);
        }

        public void AddConstructPamameter(string name, params object[] args)
        {
            if (paramterDict.ContainsKey(name))
                paramterDict[name] = args;
            else
                paramterDict.Add(name, args);
        }

        public void AddInstance(IOCContainer container, string name, object instance)
            => CheckContainerDict(container, name, instance);

        public void AddInstance(string name, IOCContainer container, params object[] args)
        {
            var instance = CreateInstance(container, args);
            CheckContainerDict(container, name, instance, false);
        }

        public bool Contains(string name)
            => containerDict.ContainsKey(name);

        private void CheckContainerDict(IOCContainer container, string name, object instance, bool isInit = true)
        {
            if (containerDict.ContainsKey(name))
                containerDict[name] = instance;
            else
                containerDict.Add(name, instance);

            //保存列表最后一个注册的实例
            if (containerDict[string.Empty] == null || !containerDict[string.Empty].Equals(instance))
                containerDict[string.Empty] = instance;
            if (!isInit) return;
            InjectAllField(container, instance);
            InjectAllProperties(container, instance);
            InjectMethodies(container, objectType, instance);
        }

        private object CreateInstance(IOCContainer container, params object[] args)
        {
            var obj = Activator.CreateInstance(objectType, args);
            InjectAllField(container, obj);
            InjectAllProperties(container, obj);
            InjectMethodies(container, objectType, obj);
            return obj;
        }

        private void InjectMethodies(IOCContainer container, Type type, object target)
        {
            List<object> datas = new List<object>();
            foreach (var method in type.GetMethods(BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic
                | BindingFlags.InvokeMethod))
            {
                foreach (var attribute in method.GetCustomAttributes())
                {
                    if (attribute is InjectMethodAttribute)
                    {
                        foreach (var parameter in method.GetParameters())
                        {
                            container.SettingInParameter(type, parameter, out object obj);
                            datas.Add(obj);
                        }

                        method.Invoke(target, datas.ToArray());
                    }
                    datas?.Clear();
                }
            }
        }

        private void InjectAllField(IOCContainer container, object instance)
        {
            object obj = null;
            foreach (var field in objectType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                foreach (var attribute in field.GetCustomAttributes())
                {
                    if (attribute is InjectAttribute inject)
                    {
                        ObjectData data = null;
                        if (container.CheckSingletonType(field.FieldType))
                            data = container.GetOrAddSingleton(field.FieldType);
                        else
                            data = container.GetContainer(field.FieldType);

                        if (data == null) continue;

                        string name = inject.Path;
                        obj = data.GetInstance(container, name);

                        field.SetValue(instance, obj);
                    }
                }
            }
        }

        private void InjectAllProperties(IOCContainer container, object instance)
        {
            object obj = null;
            foreach (var field in objectType.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                foreach (var attribute in field.GetCustomAttributes())
                {
                    if (attribute is InjectAttribute inject)
                    {
                        ObjectData data = null;
                        if(container.CheckSingletonType(field.PropertyType))
                            data = container.GetOrAddSingleton(field.PropertyType);
                        else
                            data = container.GetContainer(field.PropertyType);
                        if (data == null) continue;

                        string name = inject.Path;
                        obj = data.GetInstance(container, name);

                        field.SetValue(instance, obj);
                    }
                }
            }
        }
    }


}
