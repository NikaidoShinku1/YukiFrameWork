using System;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using YukiFrameWork.Pools;
namespace YukiFrameWork
{
    public class ContainerBuilder : IContainerBuilder,IDisposable
    {
        [InjectionBuilder]
        private IOCContainer container = new IOCContainer();

        public void SetParentTypeName(Type parentType)
        {
            container.SetParentType(parentType);
        }

        private const string warningText = "Mono脚本以及Unity组件不可用该api请改用RegisterComponent等API以获取依赖！";
        #region 注册实例
        public void Register<TInstance>(LifeTime lifeTime, params object[] args) where TInstance : class
        {                     
            Register(typeof(TInstance), lifeTime, args);
        }

        public void Register<TInterface, TInstance>(LifeTime lifeTime = LifeTime.Transient, params object[] args)
            where TInterface : class
            where TInstance : class
        {
            Type interfaceType = typeof(TInterface);
            Type type = typeof(TInstance);            

            if (!CheckTypeInheritInterface(interfaceType, type))
            {
                Debug.Log(string.Format("实例类型与接口类型不匹配无法使用接口隔离！请重试,InterfaceType:{0} -> Type:{1}", interfaceType, type));
                return;
            }


            if (CheckTypeOrComponent(type))
            {
                Debug.LogWarning(warningText);
                return;
            }
            ObjectData datas = null;
            if (lifeTime == LifeTime.Singleton)
                datas = container.GetOrAddSingleton(interfaceType, type);
            else
                datas = container.GetOrAddContainer(interfaceType, type);

            SwitchLifeTimeType(lifeTime, datas,string.Empty, args);
        }

        public void Register<TInstance>(string name,LifeTime lifeTime = LifeTime.Transient, params object[] args) where TInstance : class
        {          
            Register(name, typeof(TInstance), lifeTime, args);
        }

        public void Register<TInterface, TInstance>(string name, LifeTime lifeTime = LifeTime.Transient, params object[] args)
            where TInterface : class
            where TInstance : class
        {
            Type interfaceType = typeof(TInterface);
            Type type = typeof(TInstance);
            
            if (!CheckTypeInheritInterface(interfaceType, type))
            {
                Debug.LogError(string.Format("实例类型与接口类型不匹配无法使用接口隔离！请重试,InterfaceType:{0} -> Type:{1}", interfaceType, type));
                return;
            }

            if (CheckTypeOrComponent(type))
            {
                Debug.LogWarning(warningText);
                return;
            }
            ObjectData datas = null;
            if (lifeTime == LifeTime.Singleton)
                datas = container.GetOrAddSingleton(interfaceType, type);
            else
                datas = container.GetOrAddContainer(interfaceType, type);

            SwitchLifeTimeType(lifeTime, datas, name, args);
        }

        public void Register(Type type, LifeTime lifeTime = LifeTime.Transient, params object[] args)
        {
            if (CheckTypeOrComponent(type))
            {
                Debug.LogWarning(warningText);
                return;
            }
            ObjectData datas = null;
            if (lifeTime == LifeTime.Singleton)
                datas = container.GetOrAddSingleton(type);
            else
                datas = container.GetOrAddContainer(type);

            SwitchLifeTimeType(lifeTime, datas, string.Empty, args);
        }

        public void Register(string name, Type type, LifeTime lifeTime = LifeTime.Transient, params object[] args)
        {
            if (CheckTypeOrComponent(type))
            {
                Debug.LogWarning(warningText);
                return;
            }
            ObjectData datas = null;
            if (lifeTime == LifeTime.Singleton)
            {
                datas = container.GetOrAddSingleton(type);
                name = string.Empty;
            }
            else
                datas = container.GetOrAddContainer(type);

            SwitchLifeTimeType(lifeTime, datas, name, args);
        }

        public void RegisterGameObject(MonoBehaviour monoBehaviour,bool includeInactive)       
           => container.AddComponentContainer(monoBehaviour,includeInactive);
        
        #endregion
        #region 注册全局实例       
        public void RegisterInstance<TInstance>(params object[] args) where TInstance : class
        {
            Register<TInstance>(LifeTime.Singleton,args);
        }
        public void RegisterInstance<TInterface, TInstance>(params object[] args)
           where TInterface : class
           where TInstance : class
        {
            Register<TInterface, TInstance>(LifeTime.Singleton,args); 
        }

        public void RegisterInstance<TInstance>(TInstance instance)
        {
            Type type = typeof(TInstance);
            if (CheckTypeOrComponent(type))
            {
                Debug.LogWarning("Mono脚本以及Unity组件不可用该api请改用RegisterGameObject以获取依赖！");
                return;
            }
            var datas = container.GetOrAddSingleton(type);

            SwitchLifeTimeType(datas, string.Empty, instance, LifeTime.Singleton);
        }
        #endregion
        #region 注册限制实例
        public void RegisterScopeInstance<TInstance>(TInstance instance) where TInstance : class
        {
            Type type = typeof(TInstance);
            if (CheckTypeOrComponent(type))
            {
                Debug.LogWarning("Mono脚本以及Unity组件不可用该api请改用RegisterGameObject以获取依赖！");
                return;
            }
            var datas = container.GetOrAddContainer(type, instance.GetType());

            SwitchLifeTimeType(datas, string.Empty, instance, LifeTime.Scoped);
        }

        public void RegisterScopeInstance<TInstance>(string name, params object[] args) where TInstance : class
        {
            Type type = typeof(TInstance);
            if (CheckTypeOrComponent(type))
            {
                Debug.LogWarning("Mono脚本以及Unity组件不可用该api请改用RegisterGameObject以获取依赖！");
                return;
            }
            var datas = container.GetOrAddContainer(type);

            SwitchLifeTimeType(LifeTime.Scoped, datas, name, args);
        }
        public void RegisterScopeInstance<TInstance>(params object[] args) where TInstance : class
        {
            Type type = typeof(TInstance);
            if (CheckTypeOrComponent(type))
            {
                Debug.LogWarning("Mono脚本以及Unity组件不可用该api请改用RegisterGameObject以获取依赖！");
                return;
            }
            var datas = container.GetOrAddContainer(type);

            SwitchLifeTimeType(LifeTime.Scoped, datas, string.Empty, args);
        }     

        public void RegisterScopeInstance<TInterface, TInstance>(params object[] args)
            where TInterface : class
            where TInstance : class
        {
            Register<TInterface, TInstance>(LifeTime.Scoped, args);
        }

        public void RegisterScopeInstance<TInstance>(string name, TInstance instance) where TInstance : class
        {
            Type type = typeof(TInstance);
            if (CheckTypeOrComponent(type))
            {
                Debug.LogWarning("Mono脚本以及Unity组件不可用该api请改用RegisterGameObject以获取依赖！");
                return;
            }
            var datas = container.GetOrAddContainer(type, instance.GetType());

            SwitchLifeTimeType( datas, name, instance, LifeTime.Scoped);
        }

        public void RegisterScopeInstance<TInterface, TInstance>(string name, params object[] args)
            where TInterface : class
            where TInstance : class
        {
            Register<TInterface, TInstance>(name,LifeTime.Scoped, args);
        }
        #endregion
        private void SwitchLifeTimeType(LifeTime lifeTime,ObjectData data,string name, params object[] args)
        {
            switch (lifeTime)
            {
                case LifeTime.Transient:
                    data.AddConstructPamameter(name,args);
                    break;
                case LifeTime.Scoped:
                    data.AddInstance(name,container,args);
                    break;
                case LifeTime.Singleton:
                    data.AddInstance(name,container,args);
                    //ToDo
                    break;          
            }
        }

        private void SwitchLifeTimeType(ObjectData data,string name,object instance,LifeTime lifeTime)
        {
            switch (lifeTime)
            {                              
                case LifeTime.Scoped:
                    data.AddInstance(container,name,instance);
                    break;
                case LifeTime.Singleton:
                    data.AddInstance(container,name, instance);
                    //ToDo
                    break;            
            }
        }

        /// <summary>
        /// 检查脚本类型是否是monobehaviour或者是unity的组件，如果是的话返回True
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool CheckTypeOrComponent(Type type)
        {
            return type.IsSubclassOf(typeof(Component));           
        }

        /// <summary>
        /// 检查是否满足接口隔离
        /// </summary>
        /// <returns></returns>
        private bool CheckTypeInheritInterface(Type interfaceType,Type type)
        {
            foreach (var iType in type.GetInterfaces())
            {
                if (iType.Equals(interfaceType))
                    return true;
            }

            return false;
        }

        #region 注册组件
        public void RegisterComponent<T>(T component,bool includeInactive = false) where T : Component
        {
            RegisterComonentExecute(component, includeInactive);
        }

        public void RegisterComponentInScene<T>(bool includeInactive = false) where T : Component
        {
#if UNITY_2020_1_OR_NEWER
            T component = UnityEngine.Object.FindObjectOfType<T>(includeInactive);
#endif
            RegisterComonentExecute(component, includeInactive);
        }

        public void RegisterComponentInNewPrefab<T>(GameObject gameObject, bool includeInactive = false) where T : Component
        {
            T component = gameObject.GetComponent<T>();         
            RegisterComonentExecute(component, includeInactive);
        }

        public void RegisterComponentInNewGameObject<T>(string name, bool includeInactive = false) where T : Component
        {
            GameObject obj = new GameObject(name);
            T component = obj.AddComponent<T>();           
            RegisterComonentExecute(component, includeInactive);
        }

        private void RegisterComonentExecute<T>(T component,bool includeInactive) where T : Component
        {
            container.AddComponentContainer(component, includeInactive);
            var data = container.GetComponentContainer(component.gameObject.name);
            data?.Add(component, includeInactive,container);
            InjectInMethodInMonoBehaviour<T>(component.gameObject.name);
        }
        #endregion
        public void InjectInMethodInMonoBehaviour<TComponent>(string objName) where TComponent : Component
        {
            Type type = typeof(TComponent);
            InjectInMethodInMonoBehaviour(objName,type);
        }      

        public void InjectInMethodInMonoBehaviour(string objName,Type type)
        {                    
            var data = container.GetComponentContainer(objName);

            if (data == null)
            {
                Debug.LogError("容器内没有注册这个GameObject下的组件,或者它被注册为容器单例,请先注册组件后再注入构造函数，GameObject：" + objName);
                return;
            }

            //反射注入
            ReflectMethod(type, data.GetOrAddComponent(type,container));
        }

        private void ReflectMethod(Type type,object target)
        {
            List<object> datas = new List<object>();
            foreach (var method in type.GetMethods(BindingFlags.NonPublic
                | BindingFlags.Public
                | BindingFlags.Instance
                | BindingFlags.InvokeMethod))
            {
                foreach (var attribute in method.GetCustomAttributes())
                {                
                    if (attribute is InjectMethodAttribute)
                    {
                        foreach (var parameter in method.GetParameters())
                        {
                            container.SettingInParameter(type,parameter,out var newData);
                            datas.Add(newData);
                        }

                        method.Invoke(target, datas?.ToArray());                   
                    }
                    datas?.Clear();
                }
            }
        }      

        public void Dispose()
        {
            container.Dispose();
            container = null;
        }

        
    }
}
