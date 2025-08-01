﻿using System;
using System.Collections;
using System.Collections.Generic; 
using UnityEngine;
using UnityEngine.SceneManagement;
using YukiFrameWork;
namespace XFABManager {

    /// <summary>
    /// 加载并且实例化GameObject的类，并且对GameObject的加载进行管理和引用计数,当引用为0时会自动卸载资源(推荐使用)
    /// </summary>
    public class GameObjectLoader 
    {  
        /// <summary>
        /// GameObjectLoader实例（单例）
        /// </summary>
        [Obsolete("Instance已过时,请使用静态方法!",true)]
        public static GameObjectLoader Instance
        {
            get { 
                //if (_instance == null)
                //{
                //    GameObject obj = new GameObject(typeof(GameObjectLoader).Name);
                //    obj.transform.position = Vector3.zero;
                //    _instance = obj.AddComponent<GameObjectLoader>();
                //    DontDestroyOnLoad(obj);
                //    _instance.StartCoroutine(_instance.CheckPoolActive());
                //}
                return null;
            }  
        }
         
        static GameObjectLoader() 
        {
            CoroutineStarter.Start(CheckPoolActive());
        }
        /// <summary>
        /// 对象池检测时间间隔(单位：秒),检测内容主要为 判断游戏物体是否需要销毁，对象池是否需要关闭
        /// </summary>
        [Tooltip("对象池检测时间间隔(单位：秒),检测内容主要为 判断游戏物体是否需要销毁，对象池是否需要关闭")]
        private static int detection_interval = 60; // 默认一分钟检测一次

        /// <summary>
        /// 对象池检测时间间隔(单位：秒),检测内容主要为 判断游戏物体是否需要销毁，对象池是否需要关闭
        /// </summary>
        public static int DetectionInterval
        {
            get
            {
                return detection_interval;
            }
            set
            {
                detection_interval = value;
                if (detection_interval < 1)
                    detection_interval = 1;
            }
        }

        /// <summary>
        /// 游戏物体被回收后多长时间会被销毁(单位:秒)
        /// </summary>
        [Tooltip("游戏物体被回收后多长时间会被销毁(单位:秒)")]
        private static int destroy_time = 10 * 60;

        /// <summary>
        /// 游戏物体被回收后多长时间会被销毁(单位:秒)
        /// </summary>
        public static int DestroyTime
        {
            get
            {
                return destroy_time;
            }
            set
            {
                destroy_time = value;
                if (destroy_time < 1)
                    destroy_time = 1;
            }

        }


        /// <summary>
        /// 对象池不再使用后多长时间会被关闭，关闭后会卸载对应的资源(单位:秒)
        /// </summary>
        [Tooltip("对象池不再使用后多长时间会被关闭，关闭后会卸载对应的资源(单位:秒)")]
        private static int close_time = 10 * 60;

        /// <summary>
        /// 对象池不再使用后多长时间会被关闭，关闭后会卸载对应的资源(单位:秒)
        /// </summary>
        public static int CloseTime
        {
            get
            {
                return close_time;
            }
            set
            {
                close_time = value;
                if (close_time < 1)
                    close_time = 1;
            }

        }


        internal static Dictionary<int, GameObjectPool> allPools = new Dictionary<int, GameObjectPool>(); // key : prefab hash code value : pool


        private static Dictionary<int, int> allObjPoolMapping = new Dictionary<int, int>(); // key : obj hash code value : pool prefab hash code


        private static List<GameObjectPool> tempPools = new List<GameObjectPool>(); // 临时存放 GameObjectPool 容器

        private static List<int> tempInts = new List<int>();

        /// <summary>
        /// 加载预制体并进行实例化(注:不在使用时可以直接销毁 或者 通过Unload回收!)通过该方法创建的游戏物体会进行引用计数,当引用数量为0时,会主动卸载资源(推荐使用)
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>

        public static GameObject Load(string projectName, string assetName, Transform parent = null)
        {   
            // 加载预制体
            GameObject prefab = AssetBundleManager.LoadAssetWithoutTips<GameObject>(projectName, assetName); 
            return Load(prefab,parent);
        }

        /// <summary>
        /// 预制体实例化(注:不在使用时可以直接销毁 或者 通过Unload回收!)通过该方法创建的游戏物体会进行引用计数,当引用数量为0时,会主动卸载资源(推荐使用)
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static GameObject Load(GameObject prefab, Transform parent = null)
        {
            if (!prefab) return null;
            GameObjectPool pool = GetOrCreatePool(prefab);
            return pool.Load(parent);
        }
       
        public static GameObject Load(string projectName, string assetName, Transform parent, bool resetTransform = false)
        {
            GameObject obj = Load(projectName, assetName, parent);
            if (resetTransform) 
            {
                obj.transform.localEulerAngles = Vector3.zero;
                obj.transform.localPosition = Vector3.zero;
            } 
            return obj;
        }

        public static GameObject Load(GameObject prefab, Transform parent, bool resetTransform = false)
        {
            GameObject obj = Load(prefab, parent);

            if (resetTransform)
            {
                obj.transform.localEulerAngles = Vector3.zero;
                obj.transform.localPosition = Vector3.zero;
            }

            return obj;
        }
      
        /// <summary>
        /// 异步加载预制体并进行实例化(注:不在使用时可以直接销毁 或者 通过Unload回收!)通过该方法创建的游戏物体会进行引用计数,当引用数量为0时,会主动卸载资源(推荐使用)
        /// </summary> 
        /// <returns></returns>
        public static GameObjectLoadRequest LoadAsync(string projectName,string assetName, Transform parent = null,bool sameScene = false)
        { 
            GameObjectLoadRequest request = new GameObjectLoadRequest();
            CoroutineStarter.Start(request.LoadAsync(projectName, assetName, parent,sameScene));
            return request;
        }

        [Obsolete("已过时,请使用Unload(GameObject obj) 代替!")]
        public static void UnLoad(GameObject obj, bool parentStays = false) 
        {
            UnLoad(obj);
        }

        /// <summary>
        /// 回收或销毁某一个游戏物体，如果这个游戏物体是通过GameObjectLoader加载的, 则会放到缓存池中回收, 如果不是则会直接销毁
        /// 被回收的游戏物体如果超过五分钟没有使用会被销毁
        /// </summary>
        /// <param name="obj">待回收的游戏物体</param>
        /// <param name="parentStays">是否保持父节点不变</param>
        /// <exception cref="System.Exception"></exception>
        public static void UnLoad(GameObject obj)
        {
#if UNITY_EDITOR 
            if (!EditorApplicationTool.isPlaying)
            {
                // 如果是编辑器非运行模式 直接销毁
                if (obj)
                    GameObject.Destroy(obj); 
                return;
            } 
#endif

            // 如果为空 或者 已经被销毁了，可以不用处理
            if (!obj) return;

            int key = obj.GetHashCode();
            if (allObjPoolMapping.ContainsKey(key))
            { 
                int pool_key = allObjPoolMapping[key];
                if (allPools.ContainsKey(pool_key))
                    allPools[pool_key].UnLoad(obj);
                else
                    // 没有查到对应池子 隐藏 
                    obj.SetActive(false);
            }
            else
                GameObject.Destroy(obj);
            
        }

        /// <summary>
        /// 回收某一个节点下的第一级的子节点游戏物体
        /// </summary>
        /// <param name="parent"></param>
        public static void UnLoad(Transform parent) 
        {
            if (!parent) return;

            foreach (var item in parent)
            {
                Transform child = item as Transform;
                if (!child) continue;
                GameObject obj = child.gameObject;
                if (!obj.activeSelf) continue;

                if (allObjPoolMapping.ContainsKey(obj.GetHashCode()))
                {
                    // 说明该游戏物体是通过GameObjectLoader加载出来的 直接回收即可
                    UnLoad(obj);
                }
                else
                {
                    // 如果该游戏物体不是通过 GameObjectLoader 加载，直接隐藏即可
                    obj.SetActive(false);
                }

            }
        }
        #region 预加载资源

        /// <summary>
        /// 预加载
        /// </summary>
        /// <param name="projectName">模块名</param>
        /// <param name="assetName">资源名</param>
        public static void Preload(string projectName, string assetName)
        {
            Preload(projectName, assetName, true);
        }

        /// <summary>
        /// 预加载
        /// </summary>
        /// <param name="projectName">模块名</param>
        /// <param name="assetName">资源名</param>
        /// <param name="autoUnload">是否自动卸载</param>
        public static void Preload(string projectName, string assetName, bool autoUnload)
        {
            GameObjectPreload.Preload(projectName, assetName, autoUnload);
        }
        internal static void AddObjPoolMapping(int obj_hash_code, int pool_hash_code)
        {
            if (allObjPoolMapping.ContainsKey(obj_hash_code))
                allObjPoolMapping[obj_hash_code] = pool_hash_code;
            else
                allObjPoolMapping.Add(obj_hash_code, pool_hash_code);
        }
        /// <summary>
        /// 如果预加载时选择的是不自动卸载，可通过此方法设置为自动卸载
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="assetName"></param>
        public static void UnPreload(string projectName, string assetName)
        {
            GameObjectPreload.UnPreload(projectName, assetName);
        }

        /// <summary>
        /// 是否正在预加载资源
        /// </summary>
        /// <returns></returns>
        public static bool IsPreloading()
        {
            return GameObjectPreload.IsPreloading();
        }


        #endregion
        internal static GameObjectPool GetOrCreatePool(GameObject prefab) {

            if (allPools.ContainsKey(prefab.GetHashCode())) {
                return allPools[prefab.GetHashCode()];
            }

            GameObjectPool pool = new GameObjectPool(prefab);
            allPools.Add(prefab.GetHashCode(), pool);
            return pool;
        }

        internal static IEnumerator CheckPoolActive()
        {
            while (true)
            {

                // 检测池子中的游戏物体是否需要销毁  检测池子是否需要关闭
                tempPools.Clear();

                foreach (var pool in allPools.Values) 
                {
                    // 如果该池子不自动卸载 就不需要检测了
                    if (!pool.IsAutoUnload) continue;
                    tempPools.Add(pool);
                }

                foreach (var item in tempPools)
                {
                    yield return item.CheckActive();
                    if (item.IsCanClose)
                    { 
                        int key = item.Prefab.GetHashCode();
                        allPools.Remove(key);
                        item.Close();
                    }
                }

                // 检测游戏物体的实例 是否需要移除
                tempInts.Clear();
                foreach (var item in allObjPoolMapping.Keys)
                {
                    tempInts.Add(item);
                }

                foreach (var key in tempInts)
                {
                    if (!allObjPoolMapping.ContainsKey(key))
                        continue;

                    int pool_key = allObjPoolMapping[key];
                    if (allPools.ContainsKey(pool_key))
                    {
                        GameObjectPool pool = allPools[pool_key];
                        if (!pool.ContainsObject(key)) 
                        { 
                            allObjPoolMapping.Remove(key);
                        }
                    }
                    else { 
                        allObjPoolMapping.Remove(key);
                    }
                }

                float wait = 0;
                while (wait < DetectionInterval)
                {
                    wait += UnityEngine.Time.unscaledDeltaTime;
                    //Debug.LogFormat("wait:{0}",wait);
                    yield return null;
                }

            }
        }

    }

    public class GameObjectPool
    {
       // private const int DESTROY_TIME_OUT = 10 * 60;  // 当游戏物体未被使用时 超过多少时间 会被销毁

       // private const int CLOSE_TIME_OUT = 10 * 60; // 当池子中没有游戏物体时 多久会关闭当前池子(关闭后会卸载预制体,对应的AB包也可能会被卸载)

        /// <summary>
        /// 存放某一个预制体 创建的所有实例  key : obj hash code 
        /// </summary>
        private Dictionary<int,GameObjectInfo> allObjs = new Dictionary<int, GameObjectInfo>(); 
        private List<GameObjectInfo> tempList = new List<GameObjectInfo>(); // 临时容器

        // 存放无效的Obj
        private List<int> invalidObjs = new List<int>();

        private GameObject prefab;

        private bool isInValidScene = false;

        private bool isHaveParent = false;


        public GameObject Prefab => prefab;

        public bool IsEmpty => allObjs.Count == 0;

        public float EmptyTime { get;private set; }

        /// <summary>
        /// 判断池子是否能够关闭
        /// </summary>
        public bool IsCanClose
        {
            get 
            { 
                // 如果不自动卸载 就不能关闭
                if(!IsAutoUnload) return false;

                return IsEmpty && Time.time - EmptyTime > GameObjectLoader.CloseTime;
            }
        }

        /// <summary>
        /// 是否自动卸载
        /// </summary>
        internal bool IsAutoUnload { get; set; }

        /// <summary>
        /// 模块名
        /// </summary>
        internal string ProjectName { get; set; }

        /// <summary>
        /// 资源名
        /// </summary>
        internal string AssetName { get; set; }

        public GameObjectPool(GameObject prefab) 
        {
            if (!prefab) throw new System.Exception("prefab is null!");
            this.prefab = prefab;
            IsAutoUnload = true;

            isHaveParent = prefab.transform.parent != null;
            isInValidScene = prefab.scene.IsValid();
        }

        // 创建一个实例
        public GameObject Load(Transform parent) 
        {

            GameObject obj = null;

            // 预制体有父节点 并且 在场景中， 需要把这个预制体隐藏
            // 如果没有 说明这个预制体是通过AssetBundle加载出来的，不需要隐藏和处理
            if (isHaveParent && isInValidScene)
                prefab.SetActive(false);

            invalidObjs.Clear();

            // 先找一个不在使用中的

            foreach (var item in allObjs.Keys)
            {
                GameObjectInfo info = allObjs[item];

                if (!info.Obj) 
                {
                    invalidObjs.Add(item);
                    continue;
                }

                if (!info.IsInUse)
                {
                    obj = info.Obj; 
                    info.IsInUse = true; 
                    allObjs[item] = info;
                    break;
                }
            }

            foreach (var item in invalidObjs)
            {
                allObjs.Remove(item);
            }

             
            if (obj == null)
            {
                obj = GameObject.Instantiate(prefab, parent);
                GameObjectInfo info = new GameObjectInfo(obj);
                allObjs.Add(info.Hash,info);
            }
            
            obj.transform.SetParent(parent);

            obj.SetActive(true);

            // 如果父节点不为空 把当前游戏物体设置为最后一个节点
            if(obj.transform.parent)
                obj.transform.SetAsLastSibling();

            // 添加 obj 和 pool 之间的映射
            GameObjectLoader.AddObjPoolMapping(obj.GetHashCode(), Prefab.GetHashCode());
            return obj;
        }

        public void UnLoad(GameObject obj)
        {
            if (!obj) return;
            int hash = obj.GetHashCode();
            if (!allObjs.ContainsKey(hash)) return;

            GameObjectInfo info = allObjs[hash];

            if (info.IsInUse == false) return; // 说明已经 Unload 了
             
            info.Obj.SetActive(false);
            info.IsInUse = false;
            info.UnloadTime = Time.time;
            //allObjs[hash] = info;
        }


        /// <summary>
        /// 检测游戏物体是否活跃 
        /// 如果过游戏物体非活跃 并且超过一定时间 就把这个游戏物体消息 游戏物体的池子中没有游戏物体时 关掉池子 卸载资源
        /// </summary>
        public Coroutine CheckActive() 
        {
            // 如果是空的就没有必要再检测了
            if (IsEmpty)
                return null; 

            return CoroutineStarter.Start(CheckActiveExecute());
        }

        private IEnumerator CheckActiveExecute() {

            tempList.Clear();

            foreach (var item in allObjs.Values)
            {
                if (item.IsDestroy()) {
                    tempList.Add(item);
                    continue;
                }
                if (!item.IsInUse && !item.Obj.activeSelf && Time.time - item.UnloadTime > GameObjectLoader.DestroyTime) {
                    tempList.Add(item);
                }
            }

            foreach (var item in tempList)
            {
                allObjs.Remove(item.Hash);
                if (!item.IsDestroy())
                {
                    GameObject.Destroy(item.Obj); 
                    yield return null;
                } 
            }

            tempList.Clear();

            if (IsEmpty) EmptyTime = Time.time;  

        }

         
        /// <summary>
        /// 关闭池子
        /// </summary>
        public void Close() 
        {
            if (!IsCanClose) return;
            if (!prefab.IsDestroy())
                AssetBundleManager.UnloadAsset(prefab); 
             
#if XFABMANAGER_LOG_OPEN_TESTING
            Debug.LogFormat("GameObjectLoader 卸载资源:{0} asset:{1}", ProjectName, AssetName);
#endif
        }

        public bool ContainsObject(int obj_hash_code) {
            return allObjs.ContainsKey(obj_hash_code);
        }

    }

    public class GameObjectInfo 
    {
        public GameObject Obj { get; set; } // 游戏物体对象
        public float UnloadTime { get; set; } // 回收的事件
        public bool IsInUse { get; set; } // 是不是正在使用中

        public int Hash { get; private set; }

        public GameObjectInfo(GameObject obj) 
        { 
            this.Obj = obj; 
            this.IsInUse = true;
            this.UnloadTime = Time.time;
            Hash = obj.GetHashCode();
        }


        public bool IsDestroy() 
        { 
            return Obj.IsDestroy();
        }




    }


}

