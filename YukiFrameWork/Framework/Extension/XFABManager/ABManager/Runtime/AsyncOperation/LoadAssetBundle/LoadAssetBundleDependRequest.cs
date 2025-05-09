﻿
using System.Collections;
using UnityEngine;

namespace XFABManager
{
    public class LoadAssetBundleDependRequest : CustomAsyncOperation<LoadAssetBundleDependRequest>
    {


        public static LoadAssetBundleDependRequest LoadAssetBundle(string projectName, string bundleName, string dependence, string suffix)
        {
            string key = string.Format("LoadAssetBundleDependRequest:{0}{1}{2}{3}", projectName, bundleName,dependence,suffix);
            return AssetBundleManager.ExecuteOnlyOnceAtATime<LoadAssetBundleDependRequest>(key, () =>
            {
                LoadAssetBundleDependRequest request = new LoadAssetBundleDependRequest();
                CoroutineStarter.Start(request.Load(projectName, bundleName,dependence,suffix));
                return request;
            });
        }


        internal IEnumerator Load(string projectName,string bundleName,string dependence,string suffix) 
        {
            if (AssetBundleManager.IsLoadedAssetBundle(projectName, dependence))
            {
                AssetBundleManager.AddDependencesBundles(AssetBundleManager.AssetBundles[projectName][dependence], projectName, bundleName, dependence);
                Completed();
                yield break;
            }

#if XFABMANAGER_LOG_OPEN_TESTING
            float start_load_time = Time.time;
#endif
             
            string bundlePath = AssetBundleManager.GetAssetBundlePath(projectName, dependence, suffix);
            AssetBundleCreateRequest request = AssetBundleManager.LoadAssetBundleFromFilePathAsync(bundlePath, projectName);

            //yield return request;
            while (request != null && !request.isDone)
            {
                yield return null; 
            }

#if XFABMANAGER_LOG_OPEN_TESTING

            if (Time.time - start_load_time > 3) 
            { 
                Debug.LogFormat("AssetBundle:{0}加载耗时:{1},超过3秒,可以尝试优化资源!", dependence, Time.time - start_load_time); 
            }

#endif

            if (request != null && request.assetBundle != null)
            {
                if (!AssetBundleManager.AssetBundles[projectName].ContainsKey(dependence))
                {
                    AssetBundleManager.AssetBundles[projectName].Add(dependence, request.assetBundle);
                    // 添加依赖信息
                    AssetBundleManager.AddDependencesBundles(request.assetBundle, projectName, bundleName, dependence);
                }
                // 完成异步操作
                Completed();
            }
            else
            {
                if (File.Exists(bundlePath))
                    Completed(string.Format("AssetBundle:{0}加载失败,请检测文件是否损坏或密钥是否填写正确!", bundlePath));
                else
                    Completed(string.Format("AssetBundle:{0}加载失败!", bundlePath));
            }
        }


    }

}

