///=====================================================
/// - FileName:      GameObjectPools.cs
/// - NameSpace:     YukiFrameWork.Project
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   这是一个框架工具创建的脚本
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
namespace YukiFrameWork.Pools
{
    public class GameObjectPools : AbstarctPools<GameObject>
    {
        public GameObjectPools(GameObject parentPrefab,System.Action<GameObject> recycleMethod,int initSize,int maxSize = 200)
        {
            InitPrefab(parentPrefab, recycleMethod, initSize, maxSize);
        }

        public void InitPrefab(GameObject parentPrefab, System.Action<GameObject> recycleMethod, int initSize, int maxSize = 200)
        {
            this.maxSize = maxSize;
            this.recycleMethod = recycleMethod;
            SetFectoryPool(() => 
            {
                var obj = Object.Instantiate(parentPrefab);
                obj.name = obj.name.Replace("(Clone)", "");
                return obj;
            });

            for (int i = 0; i < initSize; i++)
            {
                GameObject obj = fectoryPool.Create();   
                if(obj != null)
                    Release(obj);
            }
        }

        public GameObjectPools() { }

        public override bool Release(GameObject obj)
        {
            recycleMethod?.Invoke(obj);

            if (tQueue.Count < maxSize)
            {
                tQueue.Enqueue(obj);
                return true;
            }
            return false;
            
        }

        public bool Release(GameObject obj, bool isDestroy)
        {
            recycleMethod?.Invoke(obj);
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