using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        private const int DETECTION_INTERVAL = 60; // 默认一分钟检测一次


        private static Dictionary<int, GameObjectPool> allPools = new Dictionary<int, GameObjectPool>(); // key : prefab hash code value : pool


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

            if (!EditorApplicationTool.isPlaying) return null;

            // 加载预制体
            GameObject prefab = AssetBundleManager.LoadAssetWithoutTips<GameObject>(projectName, assetName);
            if (prefab == null) return null;
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
            if (!EditorApplicationTool.isPlaying) return null;
            if (prefab == null) return null;

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
            if (!EditorApplicationTool.isPlaying) return null;
            GameObjectLoadRequest request = new GameObjectLoadRequest();
            CoroutineStarter.Start(request.LoadAsync(projectName, assetName, parent));
            return request;
        }

        /// <summary>
        /// 回收或销毁某一个游戏物体，如果这个游戏物体是通过GameObjectLoader加载的, 则会放到缓存池中回收, 如果不是则会直接销毁
        /// 被回收的游戏物体如果超过五分钟没有使用会被销毁
        /// </summary>
        /// <param name="obj"></param>
        /// <exception cref="System.Exception"></exception>
        public static void UnLoad(GameObject obj)
        {
            if (!EditorApplicationTool.isPlaying) return;
            if (obj == null) return;
            int key = obj.GetHashCode();
            if (allObjPoolMapping.ContainsKey(key)) { 
                int pool_key = allObjPoolMapping[key];
                if (allPools.ContainsKey(pool_key))
                    allPools[pool_key].UnLoad(obj);
                else
                    throw new System.Exception(string.Format("未查询到池子:{0}",pool_key));
            }
            else
                GameObject.Destroy(obj);
            
        }


        private static GameObjectPool GetOrCreatePool(GameObject prefab) {

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

                foreach (var pool in allPools.Values) { 
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

    }

    public class GameObjectPool
    {
        private const int DESTROY_TIME_OUT = 5 * 60; // 当游戏物体未被使用时 超过多少时间 会被销毁

        /// <summary>
        /// 存放某一个预制体 创建的所有实例  key : obj hash code 
        /// </summary>
        private Dictionary<int,GameObjectInfo> allObjs = new Dictionary<int, GameObjectInfo>(); 
        private List<GameObjectInfo> tempList = new List<GameObjectInfo>(); // 临时容器
        
        private static Dictionary<int,GameObject> sceneRoot = new Dictionary<int, GameObject>();

        private static List<GameObject> rootObjects = new List<GameObject>();

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

        private Transform Parent
        {
            get {

                if (_parent == null)
                {
                    GameObject obj = new GameObject(string.Format("{0}:{1}",prefab.name,prefab.GetHashCode()));
                    obj.transform.SetParent(GameObjectLoaderParent, false);
                    _parent = obj.transform;
                }

                return _parent;
            }

        }

        public bool IsEmpty => allObjs.Count == 0;

        public float EmptyTime { get;private set; }

        /// <summary>
        /// 判断池子是否能够关闭
        /// </summary>
        public bool IsCanClose
        {
            get 
            { 
                return IsEmpty && Time.time - EmptyTime > DESTROY_TIME_OUT;
            }
        }

        public GameObjectPool(GameObject prefab) {
            if (prefab == null) throw new System.Exception("prefab is null!");
            this.prefab = prefab;
        }

        // 创建一个实例
        public GameObject Load(Transform parent) 
        {

            GameObject obj = null;
            // 先找一个不在使用中的

            foreach (var item in allObjs.Keys)
            {
                GameObjectInfo info = allObjs[item];
                if (!info.IsDestroy() && !info.IsInUse)
                {
                    obj = info.Obj; 
                    info.IsInUse = true; 
                    allObjs[item] = info;
                    break;
                }
            }
             
            if (obj == null)
            {
                obj = GameObject.Instantiate(prefab, parent);
                GameObjectInfo info = new GameObjectInfo(obj);
                allObjs.Add(info.Hash,info);
            }
            else 
            {

                if (parent == null)
                { 
                    // 找到当前活跃的场景 
                    GameObject root = GetActiveSceneRoot();
                    if (root != null) 
                        obj.transform.SetParent(root.transform);
                    obj.transform.SetParent(null);
                }
                else 
                {
                    obj.transform.SetParent(parent, false);
                }

                obj.transform.localEulerAngles = Vector3.zero;
                obj.transform.localPosition = Vector3.zero;
                obj.SetActive(true);
            } 
            return obj;
        }

        public void UnLoad(GameObject obj) 
        {
            if (obj == null) return;
            int hash = obj.GetHashCode();
            if (!allObjs.ContainsKey(hash)) return;

            GameObjectInfo info = allObjs[hash];

            if (info.IsInUse == false) return; // 说明已经 Unload 了

            info.Obj.transform.SetParent(Parent, false);
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
            GameObject.Destroy(Parent.gameObject); 
        }

        public bool ContainsObject(int obj_hash_code) {
            return allObjs.ContainsKey(obj_hash_code);
        }

        private static GameObject GetActiveSceneRoot() 
        {

            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid()) return null;

            int key = scene.GetHashCode();

            if (sceneRoot.ContainsKey(key) && sceneRoot[key] != null && !sceneRoot[key].IsDestroy())
                return sceneRoot[key];
            
            if(sceneRoot.ContainsKey(key))
                sceneRoot.Remove(key);

            rootObjects.Clear();
            scene.GetRootGameObjects(rootObjects);

            GameObject root = null;

            if (rootObjects.Count == 0)
                root = new GameObject("root");
            else 
                root = rootObjects[0];
             
            sceneRoot.Add(key, root);
            rootObjects.Clear();
            return root;
        }

    }

    public struct GameObjectInfo 
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

