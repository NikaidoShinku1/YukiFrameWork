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

namespace YukiFrameWork.Res
{   
    public class ResKit
    {                       
        /// <summary>
        /// 同步加载单个资源(如前置已经异步加载资源使用本api将获取已经加载的资源)
        /// </summary>
        /// <typeparam name="T">物品类型</typeparam>        
        /// <param name="pathName">物品的位置，如果路径内含有resources则从Resources加载，如没有则从ab包进行加载</param>
        /// <param name="objName">物品的名称，当使用ab包进行加载时则需要填写该参数，如未填写则加载对应包下所有的资源</param>
        /// <returns>返回一个资源</returns>
        public static T LoadSync<T>(string pathName,string objName) where T : Object
        {           
            return CheckSyncRes<T>(pathName, objName);
        }
        /// <summary>
        /// 同步加载单个资源(如前置已经异步加载资源使用本api将获取已经加载的资源)
        /// </summary>
        /// <typeparam name="T">物品类型</typeparam>        
        /// <param name="pathName">物品的位置，如果路径内含有resources则从Resources加载，如没有则从ab包进行加载</param>      
        /// <returns>返回一个资源</returns>
        public static T LoadSync<T>(string pathName) where T : Object
        {
            return CheckSyncRes<T>(pathName, string.Empty);
        }

        /// <summary>
        /// 同步加载多个资源 注：路径为Resources根目录时加一条斜杠:Resources/(如前置已经异步加载资源使用本api将获取已经加载的资源)
        /// </summary>
        /// <typeparam name="T">物品类型</typeparam>        
        /// <param name="pathName">物品的位置，如果路径内含有resources则从Resources加载，如没有则从ab包进行加载(</param>      
        /// <returns>返回一个资源</returns>
        public static List<T> LoadAllSync<T>(string pathName) where T : Object
        {
            return CheckAllRes<T>(pathName);
        }

        /// <summary>
        /// 异步加载指定下资源
        /// </summary>
        /// <typeparam name="T">物品类型</typeparam>        
        /// <param name="pathName">物品的位置，如果路径内含有resources则从Resources加载，如没有则从ab包进行加载</param>
        /// <param name="objName">物品的名称，当使用ab包进行加载时则需要填写该参数</param>
        /// <param name="loadAsset">自动回调，如想在等待结束后立刻获得资源则可以使用/param>
        /// <returns>返回路径下所有资源</returns>
        public static async UniTask<T> LoadAsync<T>(string pathName,string objName,System.Action<T> loadAsset = null) where T : Object
        {
            return await CheckAsyncRes(pathName, objName,loadAsset);
        }
        /// <summary>
        /// 异步加载指定下资源
        /// </summary>
        /// <typeparam name="T">物品类型</typeparam>        
        /// <param name="pathName">物品的位置，如果路径内含有resources则从Resources加载，如没有则从ab包进行加载</param>
        /// <param name="loadAsset">自动回调，如想在等待结束后立刻获得资源则可以使用/param>
        /// <returns>返回路径下所有资源</returns>
        public static async UniTask<T> LoadAsync<T>(string pathName, System.Action<T> loadAsset = null) where T : Object
        {
            return await CheckAsyncRes(pathName, string.Empty,loadAsset);
        }

        /// <summary>
        /// 异步加载路径下所有资源,如果路径内含有resources则从Resources加载，如没有则从ab包进行加载</param>
        /// </summary>
        /// <typeparam name="T">物品类型</typeparam>        
        /// <param name="pathName">物品的位置，如果路径内含有Resources则从Resources加载，如没有则从ab包进行加载</param>        
        /// <param name="loadAsset">自动回调，如想在等待结束后立刻获得资源则可以使用/param>
        /// <returns>返回路径下所有资源</returns>
        public static async UniTask<List<T>> LoadAllAsync<T>(string pathName,System.Action<List<T>> loadAsset = null) where T : Object
        {
            return await CheckAllAsyncRes(pathName, loadAsset);
        }

        private static async UniTask<List<T>> CheckAllAsyncRes<T>(string pathName, System.Action<List<T>> loadAsset) where T : Object
        {
            string resPath = string.Empty;
            if (pathName.Contains("Resources"))
            {
                string[] chat = pathName.Split('/');
                try
                {
                    for (int i = 1; i < chat.Length; i++)
                    {
                        if (i == chat.Length - 1)
                            resPath += chat[i];
                        else resPath += chat[i] + "/";
                    }

                    var item = ResourcesManager.LoadAllAsync<T>(resPath);
                    var obj =  await item;                                      
                    
                    loadAsset?.Invoke(obj);
                    return obj;
                }
                catch
                {
                    Debug.LogError("路径有误请重试");
                    return null;
                }                            
               
            }
            else
            {
                var items = AssetBundleManager.LoadFromAsyncAllAssetBundle<T>(pathName);
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

        private static async UniTask<T> CheckAsyncRes<T>(string pathName, string objName = null, System.Action<T> loadAsset = null) where T : Object
        {
            string resPath = string.Empty;
            if (pathName.Contains("Resources"))
            {
                string[] chat = pathName.Split('/');
                try
                {
                    for (int i = 1; i < chat.Length; i++)
                    {
                        if (i == chat.Length - 1)
                            resPath += chat[i];
                        else resPath += chat[i] + "/";
                    }

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
                catch
                {
                    Debug.LogError("路径有误请重试");
                    await UniTask.Delay(System.TimeSpan.FromSeconds(100000));
                }
            }
            else
            {
                if (objName == string.Empty)
                {
                    Debug.LogError("请输入包内资源名！如果想获取此包所有的资源请选择ResKit.LoadAllAsync");
                    await UniTask.Delay(System.TimeSpan.FromSeconds(100000));
                    return null;
                }
                var item = AssetBundleManager.LoadFromAsyncAssetBundle<T>(pathName, objName);
                await item;
                var obj = item.asset as T;
                if (obj == null)
                {
                    obj = (item.asset as GameObject).GetComponent<T>();
                }
                loadAsset?.Invoke(obj);
                return obj;
            }
            loadAsset?.Invoke(default(T));
            return null;
        }

        /// <summary>
        /// 检查加载的物品
        /// </summary>     
        private static T CheckSyncRes<T>(string pathName,string objName = null) where T : Object
        {          
            string resPath = string.Empty;
            if (pathName.Contains("Resources"))
            {
                string[] chat = pathName.Split('/');
                try
                {
                    for (int i = 1; i < chat.Length; i++)
                    {
                        if (i == chat.Length - 1)
                            resPath += chat[i];
                        else resPath += chat[i] + "/";
                    }                 
                    var obj = ResourcesManager.Load<T>(resPath);
                    return obj;                       
                }
                catch
                {
                    Debug.LogError("路径有误请重试");
                }
            }
            else
            {
                if (objName == string.Empty)
                {
                    Debug.LogError("请写入包名，如果想访问ab包下所有资源请使用Reskit.LoadAllSync!");
                    return null;
                }
                var item = AssetBundleManager.GetFromAssetBundle<T>(pathName,objName);
                return item;
            }
            return null;
           
        }

        private static List<T> CheckAllRes<T>(string pathName) where T : Object
        {
            List<T> item = new List<T>();
            string resPath = string.Empty;
            if (pathName.Contains("Resources"))
            {
                string[] chat = pathName.Split('/');
                try
                {
                    for (int i = 1; i < chat.Length; i++)
                    {
                        if (i == chat.Length - 1)
                            resPath += chat[i];
                        else resPath += chat[i] + "/";
                    }                  
                    item = ResourcesManager.LoadAll<T>(resPath);
                }
                catch
                {
                    item = ResourcesManager.LoadAll<T>("");                   
                }
            }
            else
            {
                item = AssetBundleManager.GetFromAllAssetBundle<T>(pathName);
            }
            return item;
        }

        /// <summary>
        /// 卸载所有加载的资源
        /// </summary>
        /// <param name="isUnLoad">是否连同已经加载的资源一起销毁</param>
        /// <param name="callBack">回调</param>
        public static void UnLoadAll(bool isUnLoad = false,System.Action callBack = null)
        {          
            _ = ResourcesManager.UnLoadResourcesAssets(callBack);
            AssetBundleManager.UnLoadAllBundle(isUnLoad,callBack);
        }
    }
}