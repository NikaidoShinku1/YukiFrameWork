using System.Collections.Generic;
using UnityEngine;
using System;
namespace YukiFrameWork
{ 
    public partial class LifeTimeScope
    {
        private readonly static List<LifeTimeScope> lifeTimeScopes = new List<LifeTimeScope>();

        //编辑器可视化添加的物品自动创建
        [SerializeField]public bool IsAutoRun = true;

        //依赖mono创建实例
        [SerializeField]public List<GameObject> AutoRunToObject = new List<GameObject>();

        private static void EnqueneAwake(LifeTimeScope life)
        {
            if(!lifeTimeScopes.Contains(life))
            lifeTimeScopes.Add(life);
        }

        private static void CancelAwake(LifeTimeScope life)
        {
            lifeTimeScopes.Remove(life);
        }
    }

    public partial class LifeTimeScope : MonoBehaviour, IDisposable
    {
        [field:Header("调试打印")]
        [field:SerializeField]
        public bool IsDebugLog { get; set; } = true;
        private IContainerBuilder containerBuilder = new ContainerBuilder();

        public IObjectContainer Container => containerBuilder.Container;

        protected virtual void Awake()
        {
            containerBuilder.IsDebugLog = IsDebugLog;                     
            EnqueneAwake(this);
            InitBuilder(containerBuilder);
          
        }

        public IObjectContainer GetContainer()
        {
            return containerBuilder.Container;
        }

        /// <summary>
        /// 父类初始化方法
        /// </summary>
        /// <param name="container">容器</param>
        protected virtual void InitBuilder(IContainerBuilder builder)
        {
            if (IsAutoRun)
            {
                if (AutoRunToObject != null)
                    foreach (var obj in AutoRunToObject)
                    {
                        if (obj != null)
                        {
                            AutoRegisterAll(builder, obj);
                        }
                    }
            }
        }

        private void AutoRegisterAll(IContainerBuilder container,GameObject obj)
        {
            if (obj != null)
            {
                (container as ContainerBuilder).AutoRegisterGameObject(obj);
            }
        }

        protected virtual void OnDestroy()
        {         
            DisposeCore();
        }

        /// <summary>
        /// 资源释放
        /// </summary>
        public void DisposeCore()
        {
            containerBuilder.Container?.Dispose();
            containerBuilder = null;       
            lifeTimeScopes.Clear();
            AutoRunToObject.Clear();
            CancelAwake(this);
        }

        public void Dispose()
        {
            DisposeCore();
            if (this != null)
            {
                Destroy(gameObject);
            }
        }
    }
}
