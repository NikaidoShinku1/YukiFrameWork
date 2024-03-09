using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XFABManager
{

    public class LoadAssetRequest : CustomAsyncOperation<LoadAssetRequest>
    {

        public UnityEngine.Object asset
        {
            get; protected set;
        }


        internal IEnumerator LoadAssetAsync(string projectName, string bundleName, string assetName, Type type )
        {
            // 防空判断
            if (string.IsNullOrEmpty(projectName) || string.IsNullOrEmpty(bundleName) || string.IsNullOrEmpty(assetName))
            {
                yield return new WaitForEndOfFrame();
                Completed(string.Format("项目名 bundle名 或 资源名为空! projectName:{0} bundleName:{1} assetName:{2} ", projectName, bundleName, assetName));
                yield break;
            }

#if UNITY_EDITOR
            if (AssetBundleManager.GetProfile(projectName).loadMode == LoadMode.Assets)
            {
                yield return null;
                asset = AssetBundleManager.LoadAssetWithoutTips(projectName, assetName,type);
                Completed();
                yield break;
            }
#endif
            string bundle_name = string.Format("{0}_{1}", projectName, bundleName);
            LoadAssetBundleRequest requestBundle = AssetBundleManager.LoadAssetBundleAsync(projectName, bundle_name);

            while (!requestBundle.isDone)
            {
                yield return null;
                progress = requestBundle.progress * 0.8f;  // 在加载资源的进度中 加载assetbundle占进度的0.8 加载资源占0.2
            }


            if (!string.IsNullOrEmpty(requestBundle.error))
            {
                Completed(string.Format("加载AssetBundle:{0}/{1}出错:{2}", projectName, bundle_name, requestBundle.error));
                yield break;
            }
            AssetBundleRequest requestAsset = requestBundle.assetBundle.LoadAssetAsync(assetName, type);
            //yield return requestAsset;

            while (!requestAsset.isDone)
            {
                yield return null;
                progress = 0.8f + requestAsset.progress * 0.2f;  // 在加载资源的进度中 加载assetbundle占进度的0.8 加载资源占0.2
            }

            if (requestAsset != null && requestAsset.asset != null)
            {
                asset = requestAsset.asset;
                AssetBundleManager.AddAssetCache(projectName, bundleName, asset);
            }
            else
            {
                error = string.Format("资源{0}/{1}/{2}加载失败!", projectName, bundle_name, assetName);
            }
            Completed();

        }
    }
}

