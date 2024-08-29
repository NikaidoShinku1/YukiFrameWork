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

            ExecuteMultipleAsyncOperation<LoadAssetBundleDependRequest> load_assetbundle_requests = new ExecuteMultipleAsyncOperation<LoadAssetBundleDependRequest>(100);

            Queue<string> need_load_dependences = new Queue<string>(dependences);

            int loaded_count = 0;

            // 加载依赖的 AssetBundle
            while (need_load_dependences.Count > 0 || !load_assetbundle_requests.IsDone())
            {
                // 可以下载
                while (load_assetbundle_requests.CanAdd() && need_load_dependences.Count > 0)
                {
                    string depend = need_load_dependences.Dequeue();

                    LoadAssetBundleDependRequest request = LoadAssetBundleDependRequest.LoadAssetBundle(projectName, bundleName, depend, suffix);

                    request.AddCompleteEvent((r) =>
                    {
                        loaded_count++;
                    });

                    // 保存正在下载的请求 (如果需要中断，需要用到) 
                    load_assetbundle_requests.Add(request);
                }

                float p = loaded_count;

                foreach (var item in load_assetbundle_requests.InExecutionAsyncOperations)
                {
                    p += item.progress;
                }

                progress = p / (dependences.Length + 1);

                yield return null;
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

