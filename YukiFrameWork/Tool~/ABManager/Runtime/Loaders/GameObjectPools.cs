///=====================================================
/// - FileName:      GameObjectPools.cs
/// - NameSpace:     YukiFrameWork.Pools
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   Unity游戏对象的对象池
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
using Object = UnityEngine.Object;
namespace YukiFrameWork.Pools
{
    /// <summary>
    /// 适用于Unity游戏对象的对象池，传入(GameObject)预制体即可克隆对应的组件类型,
    /// </summary>
    [Obsolete("该对象池类已弃用,请使用GameObjectLoader来加载!")]
    public class GameObjectPools : AbstarctPools<GameObject>
    {
        public GameObjectPools(GameObject prefab, GameObject parent, System.Action<GameObject> recycleMethod,int maxSize = 200)
        {
            InitPrefab(prefab, parent, recycleMethod,maxSize);
        }

        private bool isPreload = false;

        public bool IsDonDestoryLoad { get; set; } = true;

        /// <summary>
        /// 该对象池管理的父物体
        /// </summary>
        public GameObject Parent { get; private set; }
        /// <summary>
        /// 该对象池的对象本体
        /// </summary>
        public GameObject Prefab { get; private set; }
        public void InitPrefab(GameObject prefab,GameObject parent, System.Action<GameObject> recycleMethod,int maxSize = 200)
        {
            this.maxSize = maxSize;
            this.recycleMethod = recycleMethod;
            this.Prefab = prefab;
            this.Parent = parent;
            if(Parent != null && IsDonDestoryLoad) 
                GameObject.DontDestroyOnLoad(Parent);
            SetFectoryPool(() => 
            {
                var obj = Object.Instantiate(prefab);           
                obj.name = obj.name.Replace("(Clone)", "");                         
                return obj;
            });         
        }

        public GameObjectPools() { }

        /// <summary>
        /// 对象池预加载(每一个对象池生成出来后只能预加载一次)
        /// </summary>
        /// <param name="initSize">预加载的数量</param>
        public void PreLoadingObject(int initSize)
        {
            if (isPreload) return;

            for (int i = 0; i < initSize; i++)
            {
                GameObject obj = fectoryPool.Create();
                if (obj != null)
                    Release(obj);
            }

            isPreload = true;
        }

        public override bool Release(GameObject obj)
        {
            recycleMethod?.Invoke(obj);

            if (tQueue.Count < maxSize)
            {
                if (Parent == null)
                {
                    Parent = new GameObject(Prefab.name + "Manager");
                    if(IsDonDestoryLoad)
                        GameObject.DontDestroyOnLoad(Parent);
                }
                obj.transform.SetParent(Parent.transform);
                tQueue.Enqueue(obj);
                return true;
            }
            return false;
            
        }

        public bool Release(GameObject obj, bool isDestroy)
        {            
            if (!isDestroy)
                Release(obj);
            else
            {
                Object.Destroy(obj);
            }
            return true;
        }
    }
}