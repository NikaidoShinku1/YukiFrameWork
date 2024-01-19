using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork.ABManager
{

    public class GameObjectLoadRequest : CustomAsyncOperation<GameObjectLoadRequest>
    {
        /// <summary>
        /// 创建出来的游戏对象
        /// </summary>
        public GameObject Obj { get;private set; }

        public IEnumerator LoadAsync(string projectName, string assetName, Transform parent = null)
        { 
            LoadAssetRequest request = AssetBundleManager.LoadAssetAsyncWithoutTips<GameObject>(projectName, assetName);
            yield return request;
            if (request != null && string.IsNullOrEmpty(request.error))
            {
                Obj = GameObjectLoader.Load(request.asset as GameObject, parent);
                Completed();
            }
            else
                Completed(string.Format("资源加载失败,projectName:{0} assetName:{1} error:{2}",projectName,assetName, request?.error));
        }
    }

}


