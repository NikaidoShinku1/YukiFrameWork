using System.Collections.Generic;
using UnityEngine;
using YukiFrameWork.Extension;
using System;
using YukiFrameWork.Pools;
using System.Reflection;
namespace YukiFrameWork
{ 
    [Serializable]
    public class LifeTimeScope : MonoBehaviour,IDisposable
    {
        [SerializeField]
        [Header("是否自动注入列表内的GameObject")]
        private bool IsAutoInjectObject = true;

        [SerializeField]
        [Space]
        private List<GameObject> gameObjects = new List<GameObject>();

        private readonly Stack<GameObject> initEnterObjs = new Stack<GameObject>();

        private readonly IContainerBuilder containerBuilder = new ContainerBuilder();

        [field: SerializeField] public int ParentTypeIndex { get; set; }
        
        [field: SerializeField] public string ParentTypeName { get; set; }
       
        private IResolveContainer container = null;

        public IResolveContainer Container => container;

        private ContainerBuilder builder => (ContainerBuilder)containerBuilder;

        private readonly List<MonoBehaviour> monoBehaviours = ListPools<MonoBehaviour>.Get();
        public static LifeTimeScope scope = null;
        protected virtual void Awake()
        {
            scope = this;
            Inited();
            SetSingletonParent();            
            InitBuilder(containerBuilder);

            InitializeAllObjects();
           
            InjectAllConstructorMethodInMonoBehaviour();
           
            initEnterObjs.Clear();
            scope = null;
        }

        private void Inited()
        {
            container = new ObjectResolver(typeof(ContainerBuilder).GetField("container",BindingFlags.NonPublic | BindingFlags.Instance).GetValue(builder) as IOCContainer);
        }

        private void SetSingletonParent()
        {            
            Type parentType = AssemblyHelper.GetType(ParentTypeName);           
            builder.SetParentTypeName(parentType);
        }

        private void InjectAllConstructorMethodInMonoBehaviour()
        {
            for (int i = monoBehaviours.Count - 1; i >= 0; i--)
            {                
                builder.InjectInMethodInMonoBehaviour(monoBehaviours[i].gameObject.name, monoBehaviours[i].GetType());
            }

            monoBehaviours.Clear();
        }

        private void InitializeAllObjects()
        {
            InJectAutoInGameObject(gameObject);

            if (IsAutoInjectObject)
            {               
                for (int i = 0; i < gameObjects.Count; i++)
                {
                    InJectAutoInGameObject(gameObjects[i]);
                }
            }           
        }

        private void InJectAutoInGameObject(GameObject gameObject)
        {          
            initEnterObjs.Push(gameObject);
            InjectionFectory.InjectGameObjectInMonoBehaviour(container,gameObject, out var buffers);

            if (buffers != null)
            {
                foreach (var monoBehaviour in buffers)
                {
                    builder.RegisterGameObject(monoBehaviour, false);
                    monoBehaviours.Add(monoBehaviour);
                }
            }
            Transform[] objs = gameObject.GetComponentsInChildren<Transform>();
            for (int i = 0; i < objs.Length; i++)
            {
                if (initEnterObjs.Contains(objs[i].gameObject))
                    continue;
                InJectAutoInGameObject(objs[i].gameObject);
            }       
        }

        protected virtual void InitBuilder(IContainerBuilder builder)
        {
            
        }        
        
        public IResolveContainer GetContainer() => Container;

        protected virtual void OnDestroy()
        {
            Dispose();
        }

        public void Dispose()
        {
            builder?.Dispose();

        }
    }
}