///=====================================================
/// - FileName:      Reskit.cs
/// - NameSpace:     YukiFrameWork.Res
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   ResKit:资源管理套件
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using YukiFrameWork.Pools;

namespace YukiFrameWork.Res
{
    /// <summary>
    /// 资源归属
    /// </summary>
    public enum Attribution
    {
        Resources = 0,
        AssetBundle       
    }

    public class ResNode
    {
        public bool IsUnload { get; set; } = false;

        private readonly AssetBundleManager AssetBundleManager = new AssetBundleManager();
        private readonly ResourcesManager ResourcesManager = new ResourcesManager();      

        /// <summary>
        /// 同步加载单个资源(如前置已经异步加载资源使用本api将获取已经加载的资源)
        /// </summary>
        /// <typeparam name="T">物品类型</typeparam>        
        /// <param name="path">物品的位置，如果路径内含有resources则从Resources加载，如没有则从ab包进行加载</param>
        /// <param name="objName">物品的名称，当使用ab包进行加载时则需要填写该参数，如未填写则加载对应包下所有的资源</param>
        /// <returns>返回一个资源</returns>
        public T LoadSync<T>(Attribution attribution, string path, string objName) where T : Object
        {
            return CheckSyncRes<T>(attribution, path, objName);
        }
        /// <summary>
        /// 同步加载单个资源(如前置已经异步加载资源使用本api将获取已经加载的资源)
        /// </summary>
        /// <typeparam name="T">物品类型</typeparam>        
        /// <param name="path">物品的位置，如果路径内含有resources则从Resources加载，如没有则从ab包进行加载</param>      
        /// <returns>返回一个资源</returns>
        public T LoadSync<T>(Attribution attribution, string path) where T : Object
        {
            return CheckSyncRes<T>(attribution, path, string.Empty);
        }

        /// <summary>
        /// 同步加载多个资源 
        /// </summary>
        /// <typeparam name="T">物品类型</typeparam>        
        /// <param name="path">物品的位置，如果路径内含有resources则从Resources加载，如没有则从ab包进行加载(</param>      
        /// <returns>返回一个资源</returns>
        public List<T> LoadAllSync<T>(Attribution attribution, string path) where T : Object
        {
            return CheckAllRes<T>(attribution, path);
        }

        /// <summary>
        /// 异步加载指定下资源
        /// </summary>
        /// <typeparam name="T">物品类型</typeparam>        
        /// <param name="path">物品的位置，如果路径内含有resources则从Resources加载，如没有则从ab包进行加载</param>
        /// <param name="objName">物品的名称，当使用ab包进行加载时则需要填写该参数</param>
        /// <param name="loadAsset">自动回调，如想在等待结束后立刻获得资源则可以使用/param>
        /// <returns>返回路径下所有资源</returns>
        public async UniTask<T> LoadAsync<T>(Attribution attribution, string path, string objName, System.Action<T> loadAsset = null) where T : Object
        {
            return await CheckAsyncRes(attribution, path, objName, loadAsset);
        }
        /// <summary>
        /// 异步加载指定下资源
        /// </summary>
        /// <typeparam name="T">物品类型</typeparam>        
        /// <param name="path">物品的位置，如果路径内含有resources则从Resources加载，如没有则从ab包进行加载</param>
        /// <param name="loadAsset">自动回调，如想在等待结束后立刻获得资源则可以使用/param>
        /// <returns>返回路径下所有资源</returns>
        public async UniTask<T> LoadAsync<T>(Attribution attribution, string path, System.Action<T> loadAsset = null) where T : Object
        {
            return await CheckAsyncRes(attribution, path, string.Empty, loadAsset);
        }

        /// <summary>
        /// 异步加载路径下所有资源,如果路径内含有resources则从Resources加载，如没有则从ab包进行加载</param>
        /// </summary>
        /// <typeparam name="T">物品类型</typeparam>        
        /// <param name="path">物品的位置，如果路径内含有Resources则从Resources加载，如没有则从ab包进行加载</param>        
        /// <param name="loadAsset">自动回调，如想在等待结束后立刻获得资源则可以使用/param>
        /// <returns>返回路径下所有资源</returns>
        public async UniTask<List<T>> LoadAllAsync<T>(Attribution attribution, string path, System.Action<List<T>> loadAsset = null) where T : Object
        {
            return await CheckAllAsyncRes(attribution, path, loadAsset);
        }

        private async UniTask<List<T>> CheckAllAsyncRes<T>(Attribution attribution, string path, System.Action<List<T>> loadAsset) where T : Object
        {
            string resPath = path;
            switch (attribution)
            {
                case Attribution.Resources:
                    {
                        var item = ResourcesManager.LoadAllAsync<T>(resPath);
                        var obj = await item;

                        loadAsset?.Invoke(obj);
                        return obj;
                    }
                case Attribution.AssetBundle:
                    {
                        var items = AssetBundleManager.LoadFromAsyncAllAssetBundle<T>(path);
                        await items;
                        var list = new List<T>();
                        foreach (var item in items.allAssets)
                        {
                            T obj = item as T;
                            if (obj == null)
                                obj = (item as GameObject).GetComponent<T>();
                            list.Add(obj);
                        }
                        loadAsset?.Invoke(list);
                        return list;
                    }
            }
            return null;

        }

        private async UniTask<T> CheckAsyncRes<T>(Attribution attribution, string path, string objName = null, System.Action<T> loadAsset = null) where T : Object
        {
            string resPath = path;
            switch (attribution)
            {
                case Attribution.Resources:
                    {
                        var item = ResourcesManager.LoadAsync(resPath);
                        await item;
                        var obj = item.asset as T;
                        if (obj == null)
                        {
                            obj = (item.asset as GameObject).GetComponent<T>();
                        }
                        loadAsset?.Invoke(obj);
                        return obj;
                    }
                case Attribution.AssetBundle:
                    {
                        if (objName == string.Empty)
                        {
                            Debug.LogError("请输入包内资源名！如果想获取此包所有的资源请选择ResKit.LoadAllAsync");
                            await UniTask.Delay(System.TimeSpan.FromSeconds(100000));
                            return null;
                        }
                        var item = AssetBundleManager.LoadFromAsyncAssetBundle<T>(path, objName);
                        await item;
                        var obj = item.asset as T;
                        if (obj == null)
                        {
                            obj = (item.asset as GameObject).GetComponent<T>();
                        }
                        loadAsset?.Invoke(obj);
                        return obj;

                    }
            }
            loadAsset?.Invoke(default(T));
            return null;
        }

        /// <summary>
        /// 检查加载的物品
        /// </summary>     
        private T CheckSyncRes<T>(Attribution attribution, string path, string objName = null) where T : Object
        {
            string resPath = path;

            switch (attribution)
            {
                case Attribution.Resources:
                    {
                        var obj = ResourcesManager.Load<T>(resPath);
                        return obj;
                    }
                case Attribution.AssetBundle:
                    {

                        if (objName == string.Empty)
                        {
                            Debug.LogError("请写入包名，如果想访问ab包下所有资源请使用Reskit.LoadAllSync!");
                            return null;
                        }
                        var item = AssetBundleManager.GetFromAssetBundle<T>(path, objName);
                        return item;

                    }
            }
            return null;

        }

        private List<T> CheckAllRes<T>(Attribution attribution, string path) where T : Object
        {
            List<T> item = new List<T>();
            string resPath = path;

            switch (attribution)
            {
                case Attribution.Resources:
                    {
                        item = ResourcesManager.LoadAll<T>(resPath);
                        return item;
                    }
                case Attribution.AssetBundle:
                    {
                        item = AssetBundleManager.GetFromAllAssetBundle<T>(path);

                        return item;
                    }
            }
            return null;
        }

        /// <summary>
        /// 卸载所有加载的AB包资源
        /// </summary>
        /// <param name="isUnLoad">是否连同已经加载的资源一起销毁</param>
        /// <param name="callBack">回调</param>
        public void UnAssetBundleLoad(bool isUnLoad = false)
        {            
            AssetBundleManager.UnLoadAllBundle(isUnLoad);
        }

        /// <summary>
        /// 卸载所有提前加载的Resources资源(同步加载的资源仍需要手动)
        /// </summary>
        /// <param name="callBack">回调</param>
        public void UnResourcesLoad()
        {
           ResourcesManager.UnLoadResourcesAssets();
        }
    }

    /// <summary>
    /// 资源管理套件
    /// </summary>
    public static class ResKit
    {      
        private readonly static SimpleObjectPools<ResNode> resNodePools
            = new SimpleObjectPools<ResNode>(() => new ResNode(), node => 
            {
                node.UnAssetBundleLoad();
                node.UnResourcesLoad();
            }, 10);

        public static ResNode Get() => resNodePools.Get();

        public static bool Release(ResNode node, bool isUnLoad = false,System.Action callBack = null)
        {
            node.IsUnload = isUnLoad;
            return resNodePools.Release(node);
        }
    }
}