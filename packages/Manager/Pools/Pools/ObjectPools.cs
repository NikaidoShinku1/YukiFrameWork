using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork.Pools
{
    /// <summary>
    /// 对象池特供接口，以享受独立生命周期
    /// </summary>
    public interface IInitializeObject
    {
        void Initialize(ObjectPools pools);
        void Close();
    }

    /// <summary>
    /// 对象池类
    /// </summary>
    public class ObjectPools
    {
        //池子队列
        private Queue<Object> poolTeam;

        //池子所依赖的预制体
        private  Object prefab;
           
        //此对象池回收物品后在视图中保存的父物体位置
        private GameObject parentObject;
        
        //是否完成对对象池的初始化
        public bool IsInit { get; private set; }
        /// <summary>
        /// 构造函数初始化
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="prefab">预制体</param>     
        public ObjectPools(GameObject parentObject, Object prefab,int initSize)
        {           
            this.prefab = prefab;            
            this.parentObject = parentObject;
            this.poolTeam = new Queue<Object>(initSize);
            IsInit = true;
        }

        public ObjectPools() { }

        public void RegisterObject(GameObject prefab,GameObject parentObject,int initSize)
        {
            if (IsInit) return;
            this.prefab = prefab;
            this.parentObject = parentObject;
            poolTeam = new Queue<Object>(initSize);
            IsInit = true;
        }

        /// <summary>
        /// 获取对象
        /// </summary>
        /// <param name="target">位置</param>
        /// <param name="rotate">旋转角度</param>
        /// <returns>返回一个指定类型的对象</returns>
        public GameObject GetPoolObject(Vector3 target,Quaternion rotate)
        {
            GameObject obj;
            if (parentObject == null)
            {
                parentObject = new GameObject();
                parentObject.name = prefab.name + "Manager";
            }

            if (poolTeam.Count > 0)
            {
                obj = poolTeam.Dequeue() as GameObject;
                obj.transform.SetParent(null);
                obj.transform.SetPositionAndRotation(target, rotate);               
            }
            else
            {
                obj = Object.Instantiate(prefab,target,rotate) as GameObject;
            }            
            obj.SetActive(true);
            if (obj.GetComponent<IInitializeObject>() == null)
            {
                throw new System.NullReferenceException("添加注册的对象不存在初始化接口请尝试添加再重试！");
            }
            obj.GetComponent<IInitializeObject>().Initialize(this);

            return obj;
        }

        /// <summary>
        /// 判断物体是否处于显示状态
        /// </summary>
        /// <param name="item">物体</param>
        /// <returns>如果相同返回True</returns>
        public bool Contains(GameObject item)
        {
            if(item.activeInHierarchy) return true;
            return false;
        }

        /// <summary>
        /// 回收物体
        /// </summary>
        /// <param name="item">物体</param>
        public void RecyleObject(GameObject item)
        {
            if (Contains(item))
            {
                item.SetActive(false);
                item.transform.SetParent(parentObject.transform);
                item.GetComponent<IInitializeObject>().Close();
                poolTeam.Enqueue(item);
            }
        }
    }
   
}
