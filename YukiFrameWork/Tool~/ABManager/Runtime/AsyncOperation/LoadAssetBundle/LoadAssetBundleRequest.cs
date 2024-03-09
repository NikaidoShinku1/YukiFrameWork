using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XFABManager
{
    public class LoadAssetBundleRequest : CustomAsyncOperation<LoadAssetBundleRequest>
    {
        //private AssetBundleManager bundleManager;
        //private AssetBundle _assetBundle;
        public AssetBundle assetBundle
        {
            get; protected set;
        }

        //public LoadAssetBundleRequest(AssetBundleManager bundleManager) {
        //    this.bundleManager = bundleManager;
        //}

        internal IEnumerator LoadAssetBundle(string projectName, string bundleName)
        {
            bundleName = bundleName.ToLower();
            // 判断是否 已经有这个模块的资源
            if (!AssetBundleManager.AssetBundles.ContainsKey(projectName))
            {
                AssetBundleManager.AssetBundles.Add(projectName, new Dictionary<string, AssetBundle>());
            }
          
            string[] dependences = AssetBundleManager.GetAssetBundleDependences(projectName, bundleName);
            
            // 判断是否已经加载了这个AssetBundle
            if (AssetBundleManager.IsLoadedAssetBundle(projectName, bundleName))
            {
                assetBundle = AssetBundleManager.AssetBundles[projectName][bundleName];

                AssetBundleManager.DeleteDependencesBundles(assetBundle); // 已经加载过这个bundle 再加载需要删除缓存依赖bundle的key
                // 同时还要添加其依赖的bundle
                for (int i = 0; i < dependences.Length; i++)
                {
                    if (AssetBundleManager.IsLoadedAssetBundle(projectName, dependences[i]))
                        AssetBundleManager.AddDependencesBundles(AssetBundleManager.AssetBundles[projectName][dependences[i]], projectName, bundleName, dependences[i]);
                    else
                        Debug.LogWarningFormat("bundle:{0}:{1} 的依赖bundle:{2}未加载!", projectName, bundleName, dependences[i]);
                }

                Completed();
                yield break;
            }
            string suffix = AssetBundleManager.GetAssetBundleSuffix(projectName);

            //List<string> need_load_bundle = new List<string>(dependences.Length +1);
            //need_load_bundle.Add(bundleName);        // 加载自己
            //need_load_bundle.AddRange(dependences);  // 加载依赖项目

            // 加载依赖的 AssetBundle
            for (int i = 0; i < dependences.Length; i++)
            {

                if (AssetBundleManager.IsLoadedAssetBundle(projectName, dependences[i])) {
                    AssetBundleManager.AddDependencesBundles(AssetBundleManager.AssetBundles[projectName][dependences[i]], projectName, bundleName, dependences[i]);
                    continue;
                }
                string bundlePath = AssetBundleManager.GetAssetBundlePath(projectName, dependences[i], suffix);
                AssetBundleCreateRequest request = AssetBundleManager.LoadAssetBundleFromFilePathAsync(bundlePath,projectName);
                 
                //yield return request;
                while (request != null && !request.isDone) {
                    yield return null;
                    progress = (float)(i + request.progress) / (dependences.Length + 1);  // 更新进度
                }

                if (request != null && request.assetBundle != null)
                {
                    // 加载成功
                    AssetBundleManager.AssetBundles[projectName].Add(dependences[i], request.assetBundle);
                    // 添加依赖信息
                    AssetBundleManager.AddDependencesBundles(request.assetBundle, projectName, bundleName, dependences[i]);
                }
                else {
                    if(File.Exists(bundlePath))
                        Completed(string.Format("AssetBundle:{0}加载失败,请检测文件是否损坏或密钥是否填写正确!", bundlePath));
                    else
                        Completed(string.Format("AssetBundle:{0}加载失败!", bundlePath));
                    yield break;
                }
                
            }

            // 加载Bundle
            string bundle_path = AssetBundleManager.GetAssetBundlePath(projectName, bundleName, suffix);
            AssetBundleCreateRequest request_bundle = AssetBundleManager.LoadAssetBundleFromFilePathAsync(bundle_path,projectName);
            //yield return request_bundle;

            while (request_bundle != null && !request_bundle.isDone)
            {
                yield return null;
                progress = (float)(dependences.Length + request_bundle.progress) / (dependences.Length + 1);  // 更新进度
            }

            if (request_bundle == null || request_bundle.assetBundle == null)
            {

                if (File.Exists(bundle_path))
                    Completed(string.Format("AssetBundle:{0}加载失败,请检测文件是否损坏或密钥是否填写正确!", bundle_path));
                else
                    Completed(string.Format("AssetBundle:{0}加载失败!", bundle_path));

                //Completed(string.Format("AssetBundle:{0}加载失败!", bundle_path));
                yield break;
            }
            AssetBundleManager.DeleteDependencesBundles(request_bundle.assetBundle);
            AssetBundleManager.AssetBundles[projectName].Add(bundleName, request_bundle.assetBundle);
            assetBundle = request_bundle.assetBundle; 
            Completed();
        }
    }

}

