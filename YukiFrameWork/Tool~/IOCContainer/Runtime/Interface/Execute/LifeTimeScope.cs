using System.Collections.Generic;
using UnityEngine;
using System;
using YukiFrameWork.Pools;
using YukiFrameWork.Extension;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace YukiFrameWork.IOC
{  
    public class LifeTimeScope : MonoBehaviour,IDisposable
    {
        [SerializeField]
        [HelperBox("是否打开列表添加GameObject自动注入")]
        [BoolanPopup("关闭", "打开")]
        protected bool IsAutoInjectObject = true;   
        [ListDrawerSetting]
        [SerializeField]
        [ArrayLabel("需要注入的GameObject")]
        [EnableIf("IsAutoInjectObject")]  
        protected List<GameObject> gameObjects = ListPools<GameObject>.Get();

        [HideInInspector]
        public List<InjectInfo> infos = new List<InjectInfo>();

        private readonly Stack<GameObject> initEnterObjs = new Stack<GameObject>();

        private readonly IContainerBuilder containerBuilder = new ContainerBuilder();   
           
        public IResolveContainer Container => containerBuilder.Container;    
       
        public static LifeTimeScope scope = null;
        protected virtual void Awake()
        {
            scope = this;           
            InitBuilder(containerBuilder);            
            InitializeAllObjects();
            initEnterObjs.Clear();
            scope = null;
        }

        private void InitInjectInfo()
        {
            foreach (var info in infos)
            {
                if (string.IsNullOrEmpty(info.typeName)) continue;

                Type type = AssemblyHelper.GetType(info.typeName);              
                if (type == null || type.IsSubclassOf(typeof(UnityEngine.Object))) continue;
                containerBuilder.Register(type, info.lifeTime);
            }
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
            InjectionFectory.InjectGameObjectInMonoBehaviour(gameObject, (IOCContainer)Container);
            
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
            InitInjectInfo();
        }        
        
        public IResolveContainer GetContainer() => Container;

        protected virtual void OnDestroy()
        {
            Dispose();
        }

        public void Dispose()
        {
            containerBuilder.Dispose();
        }

#if UNITY_EDITOR
        [HideInInspector]
        public int selectIndex = -1;

        public void SaveData()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        public void AddInfo(InjectInfo info)
        {
            infos.Add(info);
            if (infos.Count == 1)
                selectIndex = 0;
        }

        public List<InjectInfo> GetInfos()
        {            
            return infos;
        }

        public void RemoveInfo(InjectInfo info)
        {
            try
            {
                infos.Remove(info);
                if (infos.Count == 0)
                    selectIndex = -1;
                else selectIndex = 0;
            }
            catch { }
        }
#endif
    }
}