using System;
using System.Collections; 
using UnityEngine;
using XFABManager;

namespace XFABManager
{
    public class LoadSubAssetRequest : CustomAsyncOperation<LoadSubAssetRequest>
    {
        // Fix编码

        public UnityEngine.Object asset
        {
            get; protected set;
        }


        internal IEnumerator LoadSubAssetAsync(string projectName, string bundleName, string mainAssetName,string subAssetName, Type type)
        {
            // 防空判断
            if (string.IsNullOrEmpty(projectName) || string.IsNullOrEmpty(bundleName) || string.IsNullOrEmpty(mainAssetName))
            {
                yield return new WaitForEndOfFrame();
                Completed(string.Format("项目名 bundle名 或 资源名为空! projectName:{0} bundleName:{1} assetName:{2} ", projectName, bundleName, mainAssetName));
                yield break;
            }

#if UNITY_EDITOR
            if (AssetBundleManager.GetProfile(projectName).loadMode == LoadMode.Assets)
            {
                yield return null;
                asset = AssetBundleManager.LoadSubAsset(projectName, mainAssetName,subAssetName, type);
                Completed();
                yield break;
            }
#endif

            if (AssetBundleManager.bundle_sub_assets.ContainsKey(projectName) &&
                AssetBundleManager.bundle_sub_assets[projectName].ContainsKey(bundleName) &&
                AssetBundleManager.bundle_sub_assets[projectName][bundleName].ContainsKey(mainAssetName) &&
                AssetBundleManager.bundle_sub_assets[projectName][bundleName][mainAssetName].Contains(subAssetName, type))
            {
                asset = AssetBundleManager.bundle_sub_assets[projectName][bundleName][mainAssetName].Get(subAssetName, type);
                Completed();
                yield break;
            }

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

            

            AssetBundleRequest requestAsset = requestBundle.assetBundle.LoadAssetWithSubAssetsAsync(mainAssetName, type);
            //yield return requestAsset;

            while (!requestAsset.isDone)
            {
                yield return null;
                progress = 0.8f + requestAsset.progress * 0.2f;  // 在加载资源的进度中 加载assetbundle占进度的0.8 加载资源占0.2
            }

            if (requestAsset != null && requestAsset.allAssets != null)
            {

                AssetBundleManager.AddSubAssets(projectName, bundleName, mainAssetName, requestAsset.allAssets);

                asset = AssetBundleManager.bundle_sub_assets[projectName][bundleName][mainAssetName].Get(subAssetName, type);
                AssetBundleManager.AddAssetCache(projectName, bundleName, asset);
            }
            else
            {
                error = string.Format("资源{0}/{1}/{2}加载失败!", projectName, bundle_name, mainAssetName);
            }
            Completed();

        }


    } 
}

