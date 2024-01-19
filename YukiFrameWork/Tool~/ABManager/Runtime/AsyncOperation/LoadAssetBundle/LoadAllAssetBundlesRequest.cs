using System.Collections;
using System.Collections.Generic;
using System.IO;


namespace YukiFrameWork.ABManager
{
    public class LoadAllAssetBundlesRequest : CustomAsyncOperation<LoadAllAssetBundlesRequest>
    {
        //private AssetBundleManager bundleManager;

        //public LoadAllAssetBundlesRequest(AssetBundleManager bundleManager) {
        //    this.bundleManager = bundleManager;
        //}

        internal IEnumerator LoadAllAssetBundles(string projectName )
        {
#if UNITY_EDITOR
            if (AssetBundleManager.GetProfile(projectName).loadMode == LoadMode.Assets)
            {
                yield return null;
                // 如果是 编辑器模式 并且从 Assets 加载资源 可以直接完成
                Completed();
                yield break;
            }
#endif

            // 读取这个模块所有的文件
            string project_build_info = ABTools.LocalResPath(projectName, ABConst.project_build_info);
            if (!File.Exists(project_build_info))
            {
                Completed(string.Format("LoadAllAssetBundles 失败!{0}文件不存在", project_build_info));
                yield break;
            }

            string mapping_path = ABTools.LocalResPath(projectName, ABConst.asset_bundle_mapping);
            if (File.Exists(mapping_path))
            {
                List<string> list = new List<string>();
                string[] lines = File.ReadAllLines(mapping_path);
                foreach (string line in lines)
                {
                    if (string.IsNullOrEmpty(line)) continue;
                    string[] names = line.Split('|');
                    if (names == null || names.Length < 2 || string.IsNullOrEmpty(names[1])) continue;
                    if (list.Contains(names[1])) continue;
                    list.Add(names[1]);
                }

                for (int i = 0; i < list.Count; i++)
                {
                    string bundleName = string.Format("{0}_{1}{2}",projectName.ToLower(), list[i],AssetBundleManager.GetAssetBundleSuffix(projectName));
                    if (bundleName.Equals(ABTools.GetCurrentPlatformName())) continue;
                    yield return AssetBundleManager.LoadAssetBundleAsync(projectName, bundleName);
                    progress = (float)i / list.Count;
                }

            }
            else {
                Completed(string.Format("projectName:{0}读取AssetBundle列表失败!",projectName));
                yield break;
            }

            Completed();
        }
    }
}


