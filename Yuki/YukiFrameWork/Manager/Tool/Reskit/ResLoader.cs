using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using YukiFrameWork.Pools;
using System.IO;
namespace YukiFrameWork.Res
{
    public class ResLoader
    {
        #region Loader引用
        //是否使用编辑器加载
        public bool LoadFromAssetBundle { get; set; } = false;

        private const long MAXLASTTIME = 200000;

        private const int MAXCACHECOUNT = 200;

        public ulong LoaderID { get; set; } = 0;

        //缓存使用的资源列表
        public Dictionary<uint, ResourcesItem> AssetDict { get; set; } = new Dictionary<uint, ResourcesItem>();

        //缓存引用计数为0的资源列表，达到缓存最大的时释放这个列表里最早没用的资源
        protected CMapList<ResourcesItem> nodeResourcesList = new CMapList<ResourcesItem>();
     
        /// <summary>
        /// 正在异步的资源列表
        /// </summary>
        protected List<AsyncLoadResParam>[] loadingAssetList = new List<AsyncLoadResParam>[(int)LoadResPriority.Res_NUM];

        protected List<AsyncLoadMoreResParam>[] loadingMoreAssetList = new List<AsyncLoadMoreResParam>[(int)LoadResPriority.Res_NUM];

        private const string headPath = @"Assets/";

        /// <summary>
        /// 正在异步加载的Dic
        /// </summary>
        protected Dictionary<uint, AsyncLoadResParam> loadingAssetDic = new Dictionary<uint, AsyncLoadResParam>();

        //加载路径下多个资源，所以保存key为我们的路径，
        protected Dictionary<string, AsyncLoadMoreResParam> loadingMoreAssetsDic = new Dictionary<string, AsyncLoadMoreResParam>();

        private readonly static SimpleObjectPools<ResLoader> simpleObjectPools
            = new SimpleObjectPools<ResLoader>(() => new ResLoader(), null, 100);

        private IAsyncExtensionCore assetAsyncCore;
        private IAsyncExtensionCore allAssetAsyncCore;

        public ResLoader(bool loadFrameAssetBundle, uint id)
        {
            Init(loadFrameAssetBundle,id);
        }

        public ResLoader() { }

        public void Init(bool loadFrameAssetBundle, ulong id)
        {
            LoadFromAssetBundle = loadFrameAssetBundle;
            LoaderID = id;          
            for (int i = 0; i < (int)LoadResPriority.Res_NUM; i++)
            {
                loadingAssetList[i] = new List<AsyncLoadResParam>();
            }

            for (int i = 0; i < (int)LoadResPriority.Res_NUM; i++)
            {
                loadingMoreAssetList[i] = new List<AsyncLoadMoreResParam>();
            }        
        }

        public void InitAsync()
        {
            assetAsyncCore = LoadAssetAsync().Start();
            allAssetAsyncCore = LoadAllAssetsAsync().Start();
        }

        public static ResLoader GetLoader(bool loadFrameAssetBundle, ulong id)
        {
            var resloader = simpleObjectPools.Get();
            resloader.Init(loadFrameAssetBundle,id);
            return resloader;
        }

        public void Release()
        {
            Clear();
            assetAsyncCore.Cancel();
            allAssetAsyncCore.Cancel();
            simpleObjectPools.Release(this);
        }
        //异步加载中继类池
        protected SimpleObjectPools<AsyncLoadResParam> asyncPools
            = new SimpleObjectPools<AsyncLoadResParam>(() => new AsyncLoadResParam(), null, 100);

        //异步加载多个物品的中继类池
        protected SimpleObjectPools<AsyncLoadMoreResParam> asyncMorePools
            = new SimpleObjectPools<AsyncLoadMoreResParam>(() => new AsyncLoadMoreResParam(), null, 100);

        //异步加载回调池
        protected SimpleObjectPools<AsyncCallBack<Object>> asyncCallBackPools
           = new SimpleObjectPools<AsyncCallBack<Object>>(() => new AsyncCallBack<Object>(), null, 100);

        protected SimpleObjectPools<AsyncCallBack<List<Object>>> asyncMoreCallBackPools
            = new SimpleObjectPools<AsyncCallBack<List<Object>>>(() => new AsyncCallBack<List<Object>>(), null, 100);

        #endregion

        #region 同步加载资源
        /// <summary>
        /// 同步加载资源，仅加载不需要实例化的资源，例如Texture，clip等，组件(Mono脚本实例)无效
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="path">地址</param>
        /// <returns></returns>
        public T LoadAsset<T>(string path) where T : Object
        {
            if (string.IsNullOrEmpty(path))
                return null;          

            string targetPath = !path.StartsWith(headPath) || !path.Contains(headPath) ? headPath + path : path;
          
            uint crc = CRC32.GetCRC32(targetPath);        
            ResourcesItem item = GetCacheResourcesItem(crc);

            if (item != null)
                return item.Obj as T;

            T obj = null;
#if UNITY_EDITOR
            if (!LoadFromAssetBundle)
            {
                item = AssetBundleManager.Instance.FindResourcesItem(crc);
                if (item != null && item.Obj != null)
                    obj = item.Obj as T;
                else
                    obj = LoadAssetByEditor<T>(targetPath);
            }
#endif
            if (obj == null)
            {
                item = AssetBundleManager.Instance.LoadResourceAssetBundle(crc);
                if (item != null && item.AssetBundle != null)
                {
                    if (item.Obj != null)
                        obj = item.Obj as T;
                    else
                        obj = item.AssetBundle.LoadAsset<T>(item.AssetName);
                }
            }       
            CacheResource(targetPath, item, crc, obj);
            return obj;
        }

        /// <summary>
        /// 同步加载多个资源(仅加载不需要实例化的资源，例如Texture，clip等，组件(Mono脚本实例)无效)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public List<T> LoadAllAssets<T>(string path) where T : Object
        {
            if(string.IsNullOrEmpty(path)) return null;

            var files = Directory.GetFiles(headPath + path);
            List<T> assets = new List<T>();
            List<string> newFiles = new List<string>();
            foreach (var s in files)
            {
                if (s.Contains(".meta") || s.EndsWith(".meta"))                                  
                    continue;
                
                newFiles.Add(s);
            }

            for (int i = 0; i < newFiles.Count; i++)
            {
                newFiles[i] = newFiles[i].Replace(@"\", @"/");              
                T obj = LoadAsset<T>(newFiles[i]);    
                assets.Add(obj);
            }

            return assets;
        }
        #endregion

        #region 同步加载Mono脚本或组件资源
        /// <summary>
        /// 同步加载资源(适用于Unity当中的GameObject下附带的组件，Mono脚本等，不适用资源)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public T LoadAssetFromComponent<T>(string path) where T : Component
        {
            if (string.IsNullOrEmpty(path))
                return null;
            string targetPath = !path.StartsWith(headPath) || !path.Contains(headPath) ? headPath + path : path;
            uint crc = CRC32.GetCRC32(targetPath);          
            ResourcesItem item = GetCacheResourcesItem(crc);
            if (item != null)
            {
                return ((GameObject)item.Obj).GetComponent<T>();
            }

            T component = null;

#if UNITY_EDITOR
            if (!LoadFromAssetBundle)
            {
                item = AssetBundleManager.Instance.FindResourcesItem(crc);
                if (item != null && item.Obj != null)
                    component = ((GameObject)item.Obj).GetComponent<T>();
                else
                    component = LoadAssetByEditor<T>(targetPath);
            }
#endif
            if (component == null)
            {
                item = AssetBundleManager.Instance.LoadResourceAssetBundle(crc);

                if (item != null && item.AssetBundle != null)
                {
                    if(item.Obj != null)
                        component = ((GameObject)item.Obj).GetComponent<T>();
                    else
                        component = item.AssetBundle.LoadAsset<GameObject>(item.AssetName).GetComponent<T>();
                }
            }

            CacheResource(targetPath, item,crc,component);

            return component;

        }

        /// <summary>
        /// 同步加载多个资源(适用于GameObject下的组件以及Mono脚本，不适用于资源)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="path"></param>
        /// <returns></returns>
        public List<T> LoadAllAssetsFromComponent<T>(string path) where T : Component
        {
            if (string.IsNullOrEmpty(path)) return null;

            List<T> components = new List<T>();

            string[] files = Directory.GetFiles(headPath + path);

            List<string> newFiles = new List<string>();

            foreach(var s in files)
            {
                if (s.Contains(".meta") || s.EndsWith(".meta"))
                    continue;

                newFiles.Add(s);
            }

            for (int i = 0; i < newFiles.Count; i++)
            {
                newFiles[i] = newFiles[i].Replace(@"\", @"/");
                uint crc = CRC32.GetCRC32(newFiles[i]);

                ResourcesItem item = GetCacheResourcesItem(crc);

                T component = LoadAssetFromComponent<T>(newFiles[i]);
                components.Add(component);
            }

            return components;
        }
        #endregion

        /// <summary>
        /// 检查资源
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="item">资源</param>
        /// <param name="crc">crc</param>
        /// <param name="obj">资源的Obj</param>
        /// <param name="addreCount">计数</param>
        private void CacheResource(string path, ResourcesItem item, uint crc, Object obj, int addreCount = 1)
        {
            WashOut();
            if (item == null)
            {
                Debug.LogError("ResourcesItem is Null,Path: " + path);
            }

            if (obj == null)
            {
                Debug.LogError("ResourceLoad Fail: " + path);
            }

            item.Obj = obj;
            item.UID = obj.GetInstanceID();
            item.RefCount += addreCount;
            item.LastUseTime = Time.realtimeSinceStartup;

            if (AssetDict.TryGetValue(crc, out var oldItem))
            {
                AssetDict[crc] = item;
            }
            else
            {
                AssetDict.Add(crc, item);
            }
        }
#if UNITY_EDITOR
        protected T LoadAssetByEditor<T>(string path) where T : Object
            => AssetDatabase.LoadAssetAtPath<T>(path);
#endif
        #region 释放资源
        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="obj">要被释放的资源</param>
        /// <param name="destroy">是否完全销毁</param>
        /// <returns></returns>
        public bool ReleaseAssets(Object obj, bool destroy = false)
        {
            if (obj == null) return false;

            ResourcesItem item = null;

            foreach (var dicItem in AssetDict.Values)
            {
                if (dicItem.UID == obj.GetInstanceID())
                {
                    item = dicItem;
                    break;
                }
            }

            if (item == null)
            {
                Debug.LogError("AssetDic里不存在该资源：" + obj.name + "  可能释放了多次");
                return false;
            }

            item.RefCount--;

            DestroyResourcesItem(item, destroy);
#if UNITY_EDITOR
            Resources.UnloadUnusedAssets();
            AssetDatabase.Refresh();
#endif
            return true;
        }

        public void Clear()
        {
            while (nodeResourcesList.Size() > 0)
            {
                ResourcesItem item = nodeResourcesList.Back();

                DestroyResourcesItem(item, true);

                nodeResourcesList.Pop();
            }
        }

        /// <summary>
        /// 根据路径卸载不需要实例化的资源
        /// </summary>
        /// <param name="path">资源的地址(用于对应CRC)</param>
        /// <param name="destroy">是否完全删除</param>
        /// <returns>是否释放成功</returns>
        public bool ReleaseAssets(string path, bool destroy = false)
        {
            if (string.IsNullOrEmpty(path)) return false;

            uint crc = CRC32.GetCRC32(headPath + path);
            if (!AssetDict.TryGetValue(crc, out var item) || item == null)
            {
                Debug.LogError("AssetDic里不存在该资源：" + path + "  可能释放了多次");
                return false;
            }


            item.RefCount--;

            DestroyResourcesItem(item, destroy);
#if UNITY_EDITOR
            Resources.UnloadUnusedAssets();
            AssetDatabase.Refresh();
#endif
            return true;
        }

        protected void DestroyResourcesItem(ResourcesItem item, bool destroy = false)
        {
            if (item == null || item.RefCount > 0)
                return;

            if (!AssetDict.Remove(item.Crc))
                return;

            if (!destroy)
            {
                nodeResourcesList.InsertToHead(item);
                return;
            }


            AssetBundleManager.Instance.ReleaseAsset(item);

            if (item.Obj != null) item.Obj = null;
        }
        #endregion  

        private void WashOut()
        {
            while (nodeResourcesList.Size() > MAXCACHECOUNT)
            {
                for (int i = 0; i < MAXCACHECOUNT / 2; i++)
                {
                    ResourcesItem item = nodeResourcesList.Back();
                    DestroyResourcesItem(item,true);
                }
            }
        }

        /// <summary>
        /// 检查并得到一个已有的资源中继类
        /// </summary>
        /// <param name="crc">路径CRC</param>
        /// <param name="addreCount">资源计数</param>
        /// <returns>返回一个中继类</returns>
        private ResourcesItem GetCacheResourcesItem(uint crc, int addreCount = 1)
        {
            if (AssetDict.TryGetValue(crc, out var item) && item != null)
            {
                item.RefCount += addreCount;
                item.LastUseTime = Time.realtimeSinceStartup;
             
            }
            return item;
        }

        #region 异步加载

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="path">地址</param>
        /// <param name="onFinish">委托回调</param>
        /// <param name="resPriority">优先级</param>
        /// <param name="args">额外参数</param>
        public void LoadAssetAsync<T>(string path, System.Action<T, object, object, object> onFinish, LoadResPriority resPriority = LoadResPriority.Res_Height, object arg1 = null, object arg2 = null, object arg3 = null) where T : Object
        {
            string targetPath = headPath + path;
            uint crc = CRC32.GetCRC32(targetPath);

            ResourcesItem item = GetCacheResourcesItem(crc);

            if (item != null)
            {
                onFinish?.Invoke((T)item.Obj, arg1, arg2, arg3);
                return;
            }

            LoadAsyncResParam(typeof(T),targetPath, crc, (a, b, c, d) => onFinish?.Invoke(a as T,b,c,d),resPriority,arg1,arg2,arg3);
        }

        public void LoadAssetFromComponentAsync<T>(string path, System.Action<T, object, object, object> onFinish, LoadResPriority resPriority = LoadResPriority.Res_Height, object arg1 = null, object arg2 = null, object arg3 = null) where T : Component
        {
            string targetPath = headPath + path;
            uint crc = CRC32.GetCRC32(targetPath);

            var item = GetCacheResourcesItem(crc);

            if (item != null)
            {
                onFinish?.Invoke(((GameObject)item.Obj).GetComponent<T>(), arg1, arg2, arg3);
                return;
            }

            LoadAsyncResParam(typeof(T),targetPath, crc, (a, b, c, d) => onFinish?.Invoke(((GameObject)a).GetComponent<T>(), arg1, arg2, arg3), resPriority,arg1, arg2, arg3);
        }

        /// <summary>
        /// 加载异步中继类
        /// </summary>
        /// <param name="type">资源类型</param>
        /// <param name="path">路径</param>
        /// <param name="crc">路径Crc</param>
        /// <param name="onFinish">回调</param>
        /// <param name="resPriority">加载优先级</param>
        /// <param name="arg1">额外参数1</param>
        /// <param name="arg2">额外参数2</param>
        /// <param name="arg3">额外参数3</param>
        private void LoadAsyncResParam(System.Type type,string path,uint crc,System.Action<Object,object,object,object> onFinish,LoadResPriority resPriority, object arg1 = null, object arg2 = null, object arg3 = null)
        {
            if (!loadingAssetDic.TryGetValue(crc, out var param) || param == null)
            {
                param = asyncPools.Get();
                param.Crc = crc;
                param.Path = path;
                param.Priority = resPriority;
                param.type = type;
                loadingAssetDic.Add(crc, param);
                loadingAssetList[(int)resPriority].Add(param);
            }
            AsyncCallBack<Object> asyncCallBack = asyncCallBackPools.Get();
            asyncCallBack.OnFinish += onFinish;
            asyncCallBack.Arg1 = arg1;
            asyncCallBack.Arg2 = arg2;
            asyncCallBack.Arg3 = arg3;
            param.asyncCallBacks.Add(asyncCallBack);
        }
      
        private IEnumerator LoadAssetAsync()
        {
            List<AsyncCallBack<Object>> callBackList = null;
            long lastYieldTime = System.DateTime.Now.Ticks;
            while (true)
            {
                bool haveYield = false;
                for (LoadResPriority i = LoadResPriority.Res_Height; i < LoadResPriority.Res_NUM; i++)
                {
                    List<AsyncLoadResParam> loadingList = loadingAssetList[(int)i];

                    if (loadingList.Count <= 0)
                        continue;

                    AsyncLoadResParam loadingItem = loadingList[0];
                    loadingList.RemoveAt(0);
                    callBackList = loadingItem.asyncCallBacks;

                    Object obj = null;
                    ResourcesItem item = null;
#if UNITY_EDITOR
                    if (!LoadFromAssetBundle)
                    {
                        if (loadingItem.IsSprite)
                            obj = LoadAssetByEditor<Sprite>(loadingItem.Path);
                        else
                            obj = LoadAssetByEditor<Object>(loadingItem.Path);
                        yield return new WaitForSeconds(0.5f);
                        item = AssetBundleManager.Instance.FindResourcesItem(loadingItem.Crc);
                    }
#endif
                    if (obj == null)
                    {
                        item = AssetBundleManager.Instance.LoadResourceAssetBundle(loadingItem.Crc);
                        if (item != null && item.AssetBundle != null)
                        {
                            AssetBundleRequest bundleRequest = null;                           
                            if (loadingItem.IsSprite)
                                bundleRequest = item.AssetBundle.LoadAssetAsync<Sprite>(item.AssetName);
                            else
                                bundleRequest = item.AssetBundle.LoadAssetAsync(item.AssetName);
                            yield return bundleRequest;
                            if (bundleRequest.isDone)
                            {                                
                                obj = bundleRequest.asset;
                            }
                            lastYieldTime = System.DateTime.Now.Ticks;
                        }
                    }

                    CacheResource(loadingItem.Path, item, loadingItem.Crc, obj, callBackList.Count);

                    for (int j = 0; j < callBackList.Count; j++)
                    {
                        var callBack = callBackList[j];
                        if (callBack != null)
                        {                          
                            callBack.Invoke(obj);
                        }

                        callBack.Reset();
                        asyncCallBackPools.Release(callBack);
                    }

                    obj = null;
                    callBackList.Clear();
                    loadingAssetDic.Remove(loadingItem.Crc);
                    loadingItem.Reset();
                    asyncPools.Release(loadingItem);

                    if (System.DateTime.Now.Ticks - lastYieldTime > MAXLASTTIME)
                    {
                        yield return null;
                        lastYieldTime = System.DateTime.Now.Ticks;
                        haveYield = true;
                    }
                }

                if (!haveYield || System.DateTime.Now.Ticks - lastYieldTime > MAXLASTTIME)
                {
                    lastYieldTime = System.DateTime.Now.Ticks;
                    yield return null;
                }
            }
        }

        public void LoadAllAssetsAsync<T>(string path, System.Action<List<T>, object, object, object> onFinish, LoadResPriority resPriority = LoadResPriority.Res_Height, object arg1 = null, object arg2 = null, object arg3 = null) where T : Object
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            string targetPath = !path.StartsWith(headPath) || !path.Contains(headPath) ? headPath + path : path;

            string[] files = Directory.GetFiles(targetPath);

            List<string> newFlies = new List<string>();

            for(int i = 0;i<files.Length;i++)
            {
                if (files[i].EndsWith(".meta") || files[i].Contains(".meta"))
                    continue;

                newFlies.Add(files[i]);
            }
            List<T> assets = new List<T>();
            for (int i = 0; i < newFlies.Count; i++)
            {
                newFlies[i] = newFlies[i].Replace(@"\", @"/");

                uint crc = CRC32.GetCRC32(newFlies[i]);
                ResourcesItem item = GetCacheResourcesItem(crc);
                if (item != null)
                    assets.Add((T)item.Obj);
                else
                {
                    //说明在这个文件夹下还有没成功加载的资源，那么我们重新加载
                    assets.Clear();
                    break;            
                }
            }
            //如果我们已经加载好了资源，则直接可以运行
            if (assets.Count > 0)
            {
                onFinish?.Invoke(assets, arg1, arg2, arg3);
                return;
            }        
            LoadAllAsyncResParam(typeof(T),targetPath,  (a, b, c, d) => 
            {
                List<T> objects = new List<T>();
                foreach(var t in a)
                {
                    objects.Add((T)t);
                }
                onFinish?.Invoke(objects, arg1, arg2, arg3);
            },resPriority,arg1,arg2,arg3);
        }

        public void LoadAllAssetsFromComponentsAsync<T>(string path, System.Action<List<T>, object, object, object> onFinish, LoadResPriority resPriority = LoadResPriority.Res_Height, object arg1 = null, object arg2 = null, object arg3 = null) where T : Component
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            string targetPath = !path.StartsWith(headPath) || !path.Contains(headPath) ? headPath + path : path;

            string[] files = Directory.GetFiles(targetPath);

            List<string> newFlies = new List<string>();

            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].EndsWith(".meta") || files[i].Contains(".meta"))
                    continue;

                newFlies.Add(files[i]);
            }
            List<T> assets = new List<T>();
            for (int i = 0; i < newFlies.Count; i++)
            {
                newFlies[i] = newFlies[i].Replace(@"\", @"/");
                uint crc = CRC32.GetCRC32(newFlies[i]);
                ResourcesItem item = GetCacheResourcesItem(crc);
                if (item != null)
                    assets.Add(((GameObject)item.Obj).GetComponent<T>());
                else
                {
                    //说明在这个文件夹下还有没成功加载的资源，那么我们重新加载
                    assets.Clear();
                    break;
                }
            }
            //如果我们已经加载好了资源，则直接可以运行
            if (assets.Count > 0)
            {
                onFinish?.Invoke(assets, arg1, arg2, arg3);
                return;
            }
            LoadAllAsyncResParam(typeof(T), targetPath, (a, b, c, d) =>
            {
                List<T> objects = new List<T>();
                foreach (var t in a)
                {
                    objects.Add(((GameObject)t).GetComponent<T>());
                }
                onFinish?.Invoke(objects, arg1, arg2, arg3);
            }, resPriority, arg1, arg2, arg3);
        }

        private void LoadAllAsyncResParam(System.Type type,string path, System.Action<List<Object>, object, object, object> onFinish, LoadResPriority resPriority, object arg1 = null, object arg2 = null, object arg3 = null)
        {
            if (!loadingMoreAssetsDic.TryGetValue(path, out var param))
            {
                param = asyncMorePools.Get();
                param.Priority = resPriority;
                param.type = type;

                string[] files = Directory.GetFiles(path);

                List<string> newFlies = new List<string>();

                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i].EndsWith(".meta") || files[i].Contains(".meta"))
                        continue;

                    newFlies.Add(files[i]);
                }

                for (int i = 0; i < newFlies.Count; i++)
                {
                    newFlies[i] = newFlies[i].Replace(@"\", @"/");

                    uint crc = CRC32.GetCRC32(newFlies[i]);

                    param.PathDatas.Add(new PathData
                    {
                        crc = crc,
                        path = newFlies[i],
                    });
                }

                loadingMoreAssetsDic.Add(path, param);
                loadingMoreAssetList[(int)resPriority].Add(param);
            }
            var moreCallBacks = asyncMoreCallBackPools.Get();
            moreCallBacks.OnFinish += onFinish;
            param.asyncCallBacks.Add(moreCallBacks);
        }

        private IEnumerator LoadAllAssetsAsync()
        {
            List<AsyncCallBack<List<Object>>> callBackList = null;
            long lastYieldTime = System.DateTime.Now.Ticks;
            while (true)
            {
                bool haveYield = false;
                for (LoadResPriority i = LoadResPriority.Res_Height; i < LoadResPriority.Res_NUM; i++)
                {
                    List<AsyncLoadMoreResParam> loadingList = loadingMoreAssetList[(int)i];

                    if (loadingList.Count <= 0)
                        continue;

                    AsyncLoadMoreResParam loadingItem = loadingList[0];
                    loadingList.RemoveAt(0);
                    callBackList = loadingItem.asyncCallBacks;
                    var dict = loadingItem.PathDatas;
                    List<Object> list = new List<Object>();
                    Debug.Log(dict.Count);
                  
                    for (int j = 0;j < dict.Count;j++)
                    {                      
                        Object obj = null;
                        ResourcesItem item = null;
#if UNITY_EDITOR
                        if (!LoadFromAssetBundle)
                        {
                            if (loadingItem.IsSprite)
                                obj = LoadAssetByEditor<Sprite>(dict[j].path);
                            else
                                obj = LoadAssetByEditor<Object>(dict[j].path);
                            yield return new WaitForSeconds(0.5f);
                            item = AssetBundleManager.Instance.FindResourcesItem(dict[j].crc);
                        }
#endif
                        if (obj == null)
                        {
                            item = AssetBundleManager.Instance.LoadResourceAssetBundle(dict[j].crc);
                            if (item != null && item.AssetBundle != null)
                            {
                                AssetBundleRequest bundleRequest = null;
                                if (loadingItem.IsSprite)
                                    bundleRequest = item.AssetBundle.LoadAssetAsync<Sprite>(item.AssetName);
                                else
                                    bundleRequest = item.AssetBundle.LoadAssetAsync(item.AssetName);
                                yield return bundleRequest;
                                if (bundleRequest.isDone)
                                {
                                    obj = bundleRequest.asset;
                                }
                                lastYieldTime = System.DateTime.Now.Ticks;
                            }
                        }                      
                        list.Add(obj);
                        CacheResource(dict[j].path, item, dict[j].crc, obj, callBackList.Count);                       
                        obj = null;                       
                        loadingAssetDic.Remove(dict[j].crc);                                        
                    }
                    for (int k = 0; k < callBackList.Count; k++)
                    {
                        var callBack = callBackList[k];
                        if (callBack != null)
                        {
                            callBack.Invoke(list);
                        }

                        callBack.Reset();
                        asyncMoreCallBackPools.Release(callBack);
                    }
                    loadingItem.Reset();
                    callBackList.Clear();
                    asyncMorePools.Release(loadingItem);                   

                    if (System.DateTime.Now.Ticks - lastYieldTime > MAXLASTTIME)
                        {
                            yield return null;
                            lastYieldTime = System.DateTime.Now.Ticks;
                            haveYield = true;
                        }

                }

                if (!haveYield || System.DateTime.Now.Ticks - lastYieldTime > MAXLASTTIME)
                {
                    lastYieldTime = System.DateTime.Now.Ticks;
                    yield return null;
                }

            }
        }
        #endregion
    }

    public static class ResLoaderExtension
    {
        public static void Release(this ResLoader resLoader)
            => resLoader.Release();
    }
}