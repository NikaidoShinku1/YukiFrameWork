using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using XFABManager;

namespace XFABManager
{
    public class LoadSceneRequest : CustomAsyncOperation<LoadSceneRequest>
    {
        
        /// <summary>
        /// 是否正在加载场景
        /// </summary>
        internal static bool LoadingScene 
        {
            get
            { 
                return loadingRequest.Count != 0;
            }
        }

        private static List<LoadSceneRequest> loadingRequest = new List<LoadSceneRequest>();

        public Scene Scene { get; private set; }

        internal string sceneName;

        internal IEnumerator LoadSceneAsyncInternal(string projectName , string sceneName, LoadSceneMode mode)
        {

            while (IsHaveSameNameRequest(sceneName))
            {
                yield return null;
            }

            // 添加
            loadingRequest.Add(this);
            this.sceneName = sceneName;

            string bundleName = AssetBundleManager.GetBundleName(projectName, sceneName, typeof(SceneObject));

            if (string.IsNullOrEmpty(bundleName))
            {
                Completed(string.Format("bundleName查询失败: projectName:{0} sceneName{1}!", projectName , sceneName));
                yield break;
            }

            if (string.IsNullOrEmpty(projectName) || string.IsNullOrEmpty(bundleName) || string.IsNullOrEmpty(sceneName))
            {
                Completed(string.Format("参数错误,projectName:{0} bundleName:{1} sceneName{2}!",projectName,bundleName,sceneName));
                yield break;
            }

            //float target_progress = 0;

#if UNITY_EDITOR
            if (AssetBundleManager.GetProfile(projectName).loadMode == LoadMode.Assets)
            {
                XFABAssetBundle bundle = AssetBundleManager.GetXFABAssetBundle(projectName, bundleName);
                if (bundle == null)
                { 
                    Completed(string.Format("查询bundle失败,projectName:{0} bundleName:{1}",projectName,bundleName));
                    yield break;
                }

                string asset_path = bundle.GetAssetPathByFileName(sceneName, typeof(SceneObject));
                if ( string.IsNullOrEmpty(asset_path))
                {
                    Completed(string.Format("查询资源失败,projectName:{0} bundleName:{1} sceneName:{2}", projectName, bundleName,sceneName));
                    yield break;
                }

                AsyncOperation operation_editor = EditorSceneManager.LoadSceneAsyncInPlayMode(asset_path,new LoadSceneParameters() { loadSceneMode = mode });
                 
                while (!operation_editor.isDone)
                {
                    yield return null; 
                    progress = operation_editor.progress; 
                }
                progress = 1; 

                Scene = SceneManager.GetSceneByName(sceneName);

                if (Scene.IsValid())
                    Completed();
                else
                    Completed(string.Format("场景加载失败:{0}", sceneName));
                yield break;
            }
#endif
            string bundle_name = string.Format("{0}_{1}", projectName, bundleName);
            
            LoadAssetBundleRequest request_bundle = AssetBundleManager.LoadAssetBundleAsync(projectName, bundle_name);

            while (!request_bundle.isDone) {
                yield return null;
                progress = request_bundle.progress * 0.5f;
            }

            if (!string.IsNullOrEmpty(request_bundle.error))
            {
                Completed(request_bundle.error);
                yield break;
            }

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, mode);
            
            while (!operation.isDone)
            {
                yield return null;  
                progress = operation.progress;
            } 

            progress = 1;

            // 添加缓存 
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.IsValid()) continue;
                if (scene.name != sceneName) continue;
                if (AssetBundleManager.asset_hash_project_name.ContainsKey(scene.GetHashCode()))
                    continue;
                Scene = scene;
                SceneObject s = new SceneObject(scene.name, scene.GetHashCode());
                AssetBundleManager.AddAssetCache(projectName, bundleName, s);
            }

            if (Scene.IsValid())
            { 
                Completed();
            }
            else {
                Completed(string.Format("场景加载失败:{0}",sceneName));
            } 
        }

        protected override void OnCompleted()
        {
            base.OnCompleted();
            // 移除
            loadingRequest.Remove(this);
        }


        internal static bool IsHaveSameNameRequest(string sceneName) {
            foreach (var item in loadingRequest)
            {
                if(item.sceneName == sceneName)
                    return true;
            }
            return false;
        }

    }
}

