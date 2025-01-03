///=====================================================
/// - FileName:      ABManagerUILoader.cs
/// - NameSpace:     YukiFrameWork.UI
/// - Created:       Yuki
/// - Email:         1274672030@qq.com
/// - Description:   这是一个框架工具创建的脚本
/// - Creation Time: 2024/1/15 21:58:54
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
using XFABManager;

namespace YukiFrameWork.UI
{
    public class ABManagerUILoader : IUIConfigLoader
    {     
        private readonly string projectName;

        public ABManagerUILoader(string projectName)
            => this.projectName = projectName;

        public T Load<T>(string name) where T : BasePanel
        {
            return AssetBundleManager.LoadAsset<GameObject>(projectName,name).GetComponent<T>();
        }

        public void LoadAsync<T>(string name, Action<T> onCompleted) where T : BasePanel
        {
            AssetBundleManager.LoadAssetAsync<GameObject>(projectName, name).AddCompleteEvent(request =>
            {
                GameObject panelObj = request.asset as GameObject;

                T panel = panelObj.GetComponent<T>();
                onCompleted?.Invoke(panel);
            });
        }

        public void UnLoad(BasePanel item)
        {
            AssetBundleManager.UnloadAsset(item);
        }
    }
}