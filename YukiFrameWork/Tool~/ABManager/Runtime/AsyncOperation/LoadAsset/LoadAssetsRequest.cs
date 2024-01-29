using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YukiFrameWork.XFABManager {

    public class LoadAssetsRequest : CustomAsyncOperation<LoadAssetsRequest>
    {

        public UnityEngine.Object[] assets {
            get; protected set;
        }

        /// <summary>
        /// 异步加载子资源
        /// </summary>
        internal IEnumerator LoadAssetWithSubAssetsAsync(string projectName, string bundleName, string assetName, Type type )
        {
            if (string.IsNullOrEmpty(projectName) || string.IsNullOrEmpty(bundleName) || string.IsNullOrEmpty(assetName))
            {
                yield return new WaitForEndOfFrame();
                Completed(string.Format("项目名 bundle名 或 资源名为空! projectName:{0} bundleName:{1} assetName:{2} ", projectName, bundleName, assetName));
                yield break;
            }
#if UNITY_EDITOR
            if (AssetBundleManager.GetProfile(projectName).loadMode == LoadMode.Assets)
            {
                assets = AssetBundleManager.LoadAssetWithSubAssets(projectName, assetName,type);
                Completed();
                yield break;
            }
#endif
            string bundle_name = string.Format("{0}_{1}", projectName, bundleName);
            LoadAssetBundleRequest requestBundle = AssetBundleManager.LoadAssetBundleAsync(projectName, bundle_name);
            yield return requestBundle;
            if (!string.IsNullOrEmpty(requestBundle.error))
            {
                error = string.Format("加载AssetBundle:{0}/{1} 失败:{2}", projectName, bundle_name, requestBundle.error);
                Completed();
                yield break;
            }

            AssetBundleRequest request = requestBundle.assetBundle.LoadAssetWithSubAssetsAsync(assetName, type);
            yield return request;

            if (request != null && request.allAssets != null)
            {
                assets = request.allAssets;
                AssetBundleManager.AddAssetCache(projectName, bundleName, assets);
            }
            else
            {
                error = string.Format("资源{0}/{1}加载失败!", projectName, bundle_name);
            }
            
            Completed();

        }


        /// <summary>
        /// 异步加载所有资源
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="bundleName"></param>
        internal IEnumerator LoadAllAssetsAsync(string projectName, string bundleName)
        {

            if (string.IsNullOrEmpty(projectName) || string.IsNullOrEmpty(bundleName))
            {
                yield return new WaitForEndOfFrame();
                Completed(string.Format("项目名 bundle名 或 资源名为空! projectName:{0} bundleName:{1} ", projectName, bundleName));
                yield break;
            }

#if UNITY_EDITOR
            if (AssetBundleManager.GetProfile(projectName).loadMode == LoadMode.Assets)
            {
                assets = AssetBundleManager.LoadAllAssets(projectName, bundleName);
                Completed();
                yield break;
            }
#endif
            string bundle_name = string.Format("{0}_{1}", projectName, bundleName);
            LoadAssetBundleRequest requestBundle = AssetBundleManager.LoadAssetBundleAsync(projectName, bundle_name);
            yield return requestBundle;

            if (!string.IsNullOrEmpty(requestBundle.error))
            {
                error = string.Format("加载AssetBundle:{0}/{1} 失败:{2}", projectName, bundle_name, requestBundle.error);
                Completed();
                yield break;
            }

            AssetBundleRequest request = requestBundle.assetBundle.LoadAllAssetsAsync();
            yield return request;
            if (request != null && request.allAssets != null)
            {
                assets = request.allAssets;
                AssetBundleManager.AddAssetCache(projectName,bundleName,assets);
            }
            else {
                error = string.Format("资源{0}/{1}加载失败!",projectName, bundle_name);
            }

            Completed();


        }

        /// <summary>
        /// 异步加载某个类型的所有资源
        /// </summary>
        internal IEnumerator LoadAllAssetsAsync(string projectName, string bundleName, Type type )
        {
            if (string.IsNullOrEmpty(projectName) || string.IsNullOrEmpty(bundleName)  )
            {
                yield return new WaitForEndOfFrame();
                Completed(string.Format("项目名 bundle名 或 资源名为空! projectName:{0} bundleName:{1} ", projectName, bundleName ));
                yield break;
            }
#if UNITY_EDITOR
            if (AssetBundleManager.GetProfile(projectName).loadMode == LoadMode.Assets)
            {
                assets = AssetBundleManager.LoadAllAssets(projectName, bundleName,type);
                Completed();
                yield break;
            }
#endif
            string bundle_name = string.Format("{0}_{1}", projectName , bundleName);
            LoadAssetBundleRequest requestBundle = AssetBundleManager.LoadAssetBundleAsync(projectName, bundle_name);
            yield return requestBundle;
            if (!string.IsNullOrEmpty(requestBundle.error))
            {
                error = string.Format("加载AssetBundle:{0}/{1} 失败:{2}", projectName, bundle_name, requestBundle.error);
                Completed();
                yield break;
            }

            AssetBundleRequest request = requestBundle.assetBundle.LoadAllAssetsAsync(type);

            if (request != null && request.allAssets != null)
            {
                assets = request.allAssets;
                AssetBundleManager.AddAssetCache(projectName,bundleName,assets);
            }
            else
            {
                error = string.Format("资源{0}/{1}加载失败!", projectName, bundle_name);
            }
            Completed();
        }

    }

}


