using System;
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

        private const int DETECTION_INTERVAL = 10; // 默认一分钟检测一次


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

            GameObject obj = pool.Load(parent);
            int key = obj.GetHashCode();
            if (allObjPoolMapping.ContainsKey(key))
                allObjPoolMapping[key] = pool.Prefab.GetHashCode();
            else
                allObjPoolMapping.Add(key, pool.Prefab.GetHashCode());
            return obj;
        }

        /// <summary>
        /// 异步加载预制体并进行实例化(注:不在使用时可以直接销毁 或者 通过Unload回收!)通过该方法创建的游戏物体会进行引用计数,当引用数量为0时,会主动卸载资源(推荐使用)
        /// </summary> 
        /// <returns></returns>
        public static GameObjectLoadRequest LoadAsync(string projectName,string assetName, Transform parent = null)
        { 
            GameObjectLoadRequest request = new GameObjectLoadRequest();
            CoroutineStarter.Start(request.LoadAsync(projectName, assetName, parent));
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
                    throw new System.Exception(string.Format("未查询到池子:{0}",pool_key));
            }
            else
                GameObject.Destroy(obj);
            
        }
  
        internal static GameObjectPool GetOrCreatePool(GameObject prefab) {

            if (allPools.ContainsKey(prefab.GetHashCode())) {
                return allPools[prefab.GetHashCode()];
            }

            GameObjectPool pool = new GameObjectPool(prefab);
            allPools.Add(prefab.GetHashCode(), pool);
            return pool;
        }

        internal static IEnumerator CheckPoolActive(){
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

                foreach (var item in tempInts)
                {
                    int pool_key = allObjPoolMapping[item];
                    if (allPools.ContainsKey(pool_key))
                    {
                        GameObjectPool pool = allPools[pool_key];
                        if (!pool.ContainsObject(item)) { 
                            allObjPoolMapping.Remove(pool_key);
                        }
                    }
                    else { 
                        allObjPoolMapping.Remove(item);
                    }
                }
 
                yield return new WaitForSeconds(DETECTION_INTERVAL);
            }
        }

        #region 预加载
         
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

        public static void UnPreload(string projectName, string assetName)
        {
            GameObjectPreload.UnPreload(projectName, assetName);
        }

        public static bool IsPreloading() 
        {
            return GameObjectPreload.IsPreloading();
        }

        #endregion



    }

    public class GameObjectPool
    {
        private const int DESTROY_TIME_OUT = 1 * 60; // 当游戏物体未被使用时 超过多少时间 会被销毁

        /// <summary>
        /// 存放某一个预制体 创建的所有实例  key : obj hash code 
        /// </summary>
        private Dictionary<int,GameObjectInfo> allObjs = new Dictionary<int, GameObjectInfo>(); 
        private List<GameObjectInfo> tempList = new List<GameObjectInfo>(); // 临时容器

        // 存放无效的Obj
        private List<int> invalidObjs = new List<int>();

        private GameObject prefab;

        private static Transform _GameObjectLoaderParent;

        private static Transform GameObjectLoaderParent
        {
            get 
            {
                if (_GameObjectLoaderParent == null)
                {
                    _GameObjectLoaderParent = new GameObject("GameObjectLoader").transform; 
                    GameObject.DontDestroyOnLoad(_GameObjectLoaderParent.gameObject);
                }

                return _GameObjectLoaderParent;
            }
        }

        private Transform _parent;

        public GameObject Prefab => prefab;

        //private Transform Parent
        //{
        //    get {

        //        if (_parent == null)
        //        {
        //            GameObject obj = new GameObject(string.Format("{0}:{1}",prefab.name,prefab.GetHashCode()));
        //            obj.transform.SetParent(GameObjectLoaderParent, false);
        //            _parent = obj.transform;
        //        }

        //        return _parent;
        //    }

        //}

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

                return IsEmpty && Time.time - EmptyTime > DESTROY_TIME_OUT;
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
        }

        // 创建一个实例
        public GameObject Load(Transform parent) 
        {

            GameObject obj = null;


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
            
            obj.transform.SetParent(parent, false);  
            obj.transform.localEulerAngles = Vector3.zero;
            obj.transform.localPosition = Vector3.zero;
            obj.SetActive(true);

            // 如果父节点不为空 把当前游戏物体设置为最后一个节点
            if(obj.transform.parent)
                obj.transform.SetAsLastSibling();

            // 把游戏物体移动到当前活跃的场景中
            //if(obj.transform.parent == null && obj.scene != SceneManager.GetActiveScene())
            //    SceneManager.MoveGameObjectToScene(obj,SceneManager.GetActiveScene());

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
            allObjs[hash] = info;
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
                if (!item.IsInUse && !item.Obj.activeSelf && Time.time - item.UnloadTime > DESTROY_TIME_OUT) {
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

            if(!_parent.IsDestroy() && !_parent.gameObject.IsDestroy())
                GameObject.Destroy(_parent.gameObject); 

            _parent = null;

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

