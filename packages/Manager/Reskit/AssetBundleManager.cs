///=====================================================
/// - FileName:      AssetBundleManager.cs
/// - NameSpace:     YukiFrameWork.Res
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   AssetBundleManager:ab包资源管理器
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using System.Collections.Generic;
using UnityEngine;
using System.IO;
using LitJson;

namespace YukiFrameWork.Res
{
    public class AssetBundleManager 
    {
        private static readonly Dictionary<string, AssetBundle> loadedAssetBundles = new Dictionary<string, AssetBundle>();       
        private static string assetBundlePath;
        private static bool IsInit = false;
        public AssetBundleManager()
        {
            Init();                     
        }       

        public static void Init()
        {
            string jsonpath = "AssetBundlesPath.Json";
            string overPath = Path.Combine(Application.streamingAssetsPath, jsonpath);
            string json = File.ReadAllText(overPath);
            JsonData data = JsonMapper.ToObject(json);
            
            assetBundlePath = Application.streamingAssetsPath + "/" + data[0];

            IsInit = true;
        }

        /// <summary>
        /// 异步加载同包下指定资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="assetBundleName">包名</param>
        /// <param name="assetName">物品名</param>
        /// <returns>返回一个AssetBundleRequest</returns>
        public static AssetBundleRequest LoadFromAsyncAssetBundle<T>(string assetBundleName, string assetName) where T :Object
        {if (!IsInit) Init();
            Check(assetBundleName);
            AssetBundleRequest request = loadedAssetBundles[assetBundleName].LoadAssetAsync(assetName);
            return request;
        }

        /// <summary>
        /// 异步加载同包下所有资源
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="assetBundleName">包名</param>
        /// <returns>返回一个AssetBundleRequest</returns>
        public static AssetBundleRequest LoadFromAsyncAllAssetBundle<T>(string assetBundleName) where T : Object
        {
            if (!IsInit) Init();
            Check(assetBundleName);
            AssetBundleRequest request = loadedAssetBundles[assetBundleName].LoadAllAssetsAsync();            
            return request;
        }

        /// <summary>
        /// 获取资源
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="assetBundleName">包名</param>
        /// <param name="assetName">资源名</param>
        public static T GetFromAssetBundle<T>(string assetBundleName, string assetName) where T : Object
        {
            if (!IsInit) Init();
            return CheckAsset<T>(assetBundleName, assetName);
        }
        /// <summary>
        /// 获取所有资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetBundleName"></param>
        /// <returns></returns>
        public static List<T> GetFromAllAssetBundle<T>(string assetBundleName) where T : Object
        {
            if (!IsInit) Init();
            return CheckAllAsset<T>(assetBundleName);
        }

        /// <summary>
        /// 检查物品
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetBundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        private static T CheckAsset<T>(string assetBundleName, string assetName = null) where T : Object
        {
            Check(assetBundleName);
            return LoadAsset<T>(assetBundleName, assetName);
        }
        /// <summary>
        /// 如上对轨
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetBundleName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        private static T LoadAsset<T>(string assetBundleName, string assetName) where T : Object
        {
            if (!loadedAssetBundles.ContainsKey(assetBundleName))
            {
                Debug.LogError("Do Not Get Key!");
                return null;
            }
            T result = loadedAssetBundles[assetBundleName].LoadAsset<T>(assetName);           
            if (result == null)
            {
                result = loadedAssetBundles[assetBundleName].LoadAsset<GameObject>(assetName).GetComponent<T>();       

                if (result == null)
                {
                    Debug.LogError("Resource non existence!");
                    return null;
                }
            }
            return result;          
        }
        /// <summary>
        /// 同步检查包下所有的资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetBundleName"></param>
        /// <returns></returns>
        private static List<T> CheckAllAsset<T>(string assetBundleName) where T : Object
        {
            Check(assetBundleName);
            return LoadAllAsset<T>(assetBundleName);

        }

        private static void Check(string assetBundleName)
        {
            if (!loadedAssetBundles.ContainsKey(assetBundleName))
            {
                string bundlePath = Path.Combine(assetBundlePath, assetBundleName);
                if (!File.Exists(bundlePath))
                {
                    Debug.LogError("ABPackage name not obtained!");
                    return;
                }
                AssetBundle assetBundle = AssetBundle.LoadFromFile(bundlePath);
                loadedAssetBundles.Add(assetBundleName, assetBundle);
            }
        }
        /// <summary>
        /// 如上对轨
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetBundleName"></param>
        /// <returns></returns>
        private static List<T> LoadAllAsset<T>(string assetBundleName) where T : Object
        {
            if (!loadedAssetBundles.ContainsKey(assetBundleName))
            {
                Debug.LogError("Do Not Get Key!");
                return null;
            }

            var result = loadedAssetBundles[assetBundleName].LoadAllAssets<T>();
            
            List<T> pathList = new List<T>();

            if (result != null)
            {              
                foreach (T obj in result)
                {
                    pathList.Add(obj);
                }
            }
            else
            {
                foreach (var item in loadedAssetBundles[assetBundleName].LoadAllAssets<GameObject>())
                {
                    T obj = item.GetComponent<T>();
                    pathList.Add(obj);
                }
            }

            return pathList;          
        }

        /// <summary>
        /// 释放包
        /// </summary>
        public static void UnLoadBundle(string assetBundleName,bool isUnLoad = false)
        {
            if (!loadedAssetBundles.ContainsKey(assetBundleName))
            {
                Debug.Log("Do Not Get Key!");
                return;
            }

            loadedAssetBundles[assetBundleName].Unload(isUnLoad);          
            loadedAssetBundles.Remove(assetBundleName);
        }

        public static void UnLoadAllBundle(bool isUnload = false,System.Action CallBack = null)
        {
            foreach (var ab in loadedAssetBundles.Values)
            {
                ab.Unload(isUnload);
            }
            CallBack?.Invoke();
            loadedAssetBundles.Clear();
        }
    }



}
