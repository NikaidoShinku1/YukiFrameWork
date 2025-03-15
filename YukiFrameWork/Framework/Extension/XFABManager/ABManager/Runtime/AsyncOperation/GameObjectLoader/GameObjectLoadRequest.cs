using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace XFABManager
{

    public class GameObjectLoadRequest : CustomAsyncOperation<GameObjectLoadRequest>
    {
        /// <summary>
        /// 创建出来的游戏对象
        /// </summary>
        public GameObject Obj { get;private set; }

        public IEnumerator LoadAsync(string projectName, string assetName, Transform parent = null, bool sameScene = false)
        {
            string sceneName = null;

            if (sameScene)
            {
                // 如果为true表示,资源加载前 和 加载后场景必须为同一个, 不是一个，则不会创建游戏物体
                Scene scene = SceneManager.GetActiveScene();
                sceneName = scene.name;
            }
            LoadAssetRequest request = AssetBundleManager.LoadAssetAsyncWithoutTips<GameObject>(projectName, assetName);
            
            while (request != null && !request.isDone)
            {
                yield return null;
                progress = request.progress;
            }
              
            if (request != null && string.IsNullOrEmpty(request.error))
            {
                GameObject prefab = request.asset as GameObject;

                GameObjectPool pool = GameObjectLoader.GetOrCreatePool(prefab);

                if (sameScene)
                {
                    if (SceneManager.GetActiveScene().name != sceneName)
                    {
                        Completed("资源加载前的场景和加载后的场景不一致,中断创建游戏物体!");
                        yield break;
                    }
                }

                Obj = pool.Load(parent);
                Completed();
            }
            else
                Completed(string.Format("资源加载失败,projectName:{0} assetName:{1} error:{2}",projectName,assetName, request?.error));
        }
    }

}


