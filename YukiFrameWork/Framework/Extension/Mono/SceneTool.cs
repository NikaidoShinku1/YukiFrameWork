///=====================================================
/// - FileName:      SceneTool.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   场景管理工具
/// - Creation Time: 2024/3/26 13:13:06
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Collections;
using YukiFrameWork.Extension;
using XFABManager;
namespace YukiFrameWork
{   
	public static class SceneTool
	{	
		public static event Action<float> LoadingScene = null;
        public static event Action LoadSceneSucceed = null;
       
        public static readonly XFABManagerSceneTool XFABManager = new XFABManagerSceneTool();

		public static void LoadScene(string sceneName,LoadSceneMode mode = LoadSceneMode.Single)
			=> SceneManager.LoadScene(sceneName,mode);

		public static void LoadScene(int sceneIndex, LoadSceneMode mode = LoadSceneMode.Single)
			=> SceneManager.LoadScene(sceneIndex,mode);

		public static void LoadScene(string sceneName, LoadSceneParameters sceneParameters)
			=> SceneManager.LoadScene(sceneName, sceneParameters);

		public static void LoadScene(int sceneIndex, LoadSceneParameters sceneParameters)
			=> SceneManager.LoadScene(sceneIndex, sceneParameters);

        public static async void LoadSceneAsyncSafe(string sceneName, Action<float> loadingCallBack = null,Action onFinish = null, LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (SceneManager.GetSceneByName(sceneName).IsValid())
            {              
                onFinish?.Invoke();
                return;
            }
            var operation = SceneManager.LoadSceneAsync(sceneName, mode);
            await MonoHelper.Start(LoadSceneAsync(operation, loadingCallBack));
            onFinish?.Invoke();
        }
        [DisableEnumeratorWarning]
        public static IEnumerator LoadSceneAsync(string sceneName,Action<float> loadingCallBack = null, LoadSceneMode mode = LoadSceneMode.Single)
		{
            var operation = SceneManager.LoadSceneAsync(sceneName, mode);
            return LoadSceneAsync(operation, loadingCallBack);
        }
        [DisableEnumeratorWarning]
        public static IEnumerator LoadSceneAsync(int sceneIndex, Action<float> loadingCallBack = null, LoadSceneMode mode = LoadSceneMode.Single)
        {
            var operation = SceneManager.LoadSceneAsync(sceneIndex, mode);
            return LoadSceneAsync(operation, loadingCallBack);              
        }
        [DisableEnumeratorWarning]
        internal static IEnumerator LoadSceneAsync(AsyncOperation asyncOperation, Action<float> loadingCallBack = null)
        {
            float progress = 0f;

            while (progress < 1)
            {
                progress = asyncOperation.progress;
                loadingCallBack?.Invoke(progress);
                LoadingScene?.Invoke(progress);               
                yield return CoroutineTool.WaitForFrame();
            }          
            LoadSceneSucceed?.Invoke();
        }
        [DisableEnumeratorWarning]
        internal static IEnumerator LoadSceneAsync(LoadSceneRequest asyncOperation, Action<float> loadingCallBack = null)
        {
            float progress = 0f;

            while (progress < 1)
            {
                progress = asyncOperation.progress;
                loadingCallBack?.Invoke(progress);
                LoadingScene?.Invoke(progress);
                yield return CoroutineTool.WaitForFrame();
            }
            LoadSceneSucceed?.Invoke();
        }

        /// <summary>
        /// 异步加载场景但是不立刻跳转，必须要实现回调
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <param name="onCompleted">加载完后的回调</param>
        /// <param name="loadingCallBack">加载的进度</param>
        /// <param name="mode"></param>
        /// <returns></returns>
        [DisableEnumeratorWarning]
        public static IEnumerator LoadSceneAsyncWithAllowSceneActive(string sceneName,Action<AsyncOperation> onCompleted, Action<float> loadingCallBack = null, LoadSceneMode mode = LoadSceneMode.Single)
        {
			var operation = SceneManager.LoadSceneAsync(sceneName, mode);			
            return LoadSceneAsyncWithAllowSceneActive(operation,onCompleted,loadingCallBack);
        }

        /// <summary>
        /// 异步加载场景但是不立刻跳转，必须要实现回调
        /// </summary>
        /// <param name="sceneIndex"></param>
        /// <param name="onCompleted">加载完后的回调</param>
        /// <param name="loadingCallBack">加载的进度</param>
        /// <param name="mode"></param>
        /// <returns></returns>
        [DisableEnumeratorWarning]
        public static IEnumerator LoadSceneAsyncWithAllowSceneActive(int sceneIndex, Action<AsyncOperation> onCompleted, Action<float> loadingCallBack = null, LoadSceneMode mode = LoadSceneMode.Single)
        {
            var operation = SceneManager.LoadSceneAsync(sceneIndex);          
            return LoadSceneAsyncWithAllowSceneActive(operation, onCompleted,loadingCallBack);
        }
        [DisableEnumeratorWarning]
        internal static IEnumerator LoadSceneAsyncWithAllowSceneActive(AsyncOperation asyncOperation, Action<AsyncOperation> onCompleted, Action<float> loadingCallBack = null)
        {
            asyncOperation.allowSceneActivation = false;          
            while (asyncOperation.progress < 0.9f)
            {               
                loadingCallBack?.Invoke(asyncOperation.progress);
                LoadingScene?.Invoke(asyncOperation.progress);
                yield return CoroutineTool.WaitForFrame();
            }

            LoadSceneSucceed?.Invoke();
            onCompleted?.Invoke(asyncOperation);
        }   
    }

    public class SceneListener : SingletonMono<SceneListener>
    {
        protected override void Awake()
        {
            IsDonDestroyLoad = false;
            base.Awake();
        }
    }

    public class XFABManagerSceneTool
    {
        private string projectName;
        private bool isInited => !projectName.IsNullOrEmpty();
        [MethodAPI("XFABManager的场景加载封装拓展工具")]
        public void Init(string projectName)
        {         
            this.projectName = projectName;
        }

        public void LoadScene(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (!isInited) return;
            AssetBundleManager.LoadScene(projectName, sceneName, mode);
        }

        public IEnumerator LoadSceneAsync(string sceneName, Action<float> loadingCallBack, LoadSceneMode mode = LoadSceneMode.Single)
            => isInited ? SceneTool.LoadSceneAsync(AssetBundleManager.LoadSceneAsynchrony(projectName, sceneName, mode),loadingCallBack) : throw new Exception("没有完成对SceneTool.XFABManager的初始化，请调用一次Init方法");

        public LoadSceneRequest LoadSceneAsync(string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Single)
        {
            return isInited ? AssetBundleManager.LoadSceneAsynchrony(projectName, sceneName, loadSceneMode) : throw new Exception("没有完成对SceneTool.XFABManager的初始化，请调用一次Init方法");
        }

        /// <summary>
        /// 场景安全加载，如果场景就是当前需要切换的场景，那么会直接执行OnFinish回调，不会执行场景的Load逻辑
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="loadingCallBack"></param>
        /// <param name="onFinish"></param>
        /// <param name="mode"></param>
        public async void LoadSceneAsyncSafe(string sceneName, Action<float> loadingCallBack = null, Action onFinish = null, LoadSceneMode mode = LoadSceneMode.Single)
        {
            if (SceneManager.GetSceneByName(sceneName).IsValid())
            {              
                onFinish?.Invoke();
                return;
            }           
            await MonoHelper.Start(LoadSceneAsync(sceneName, loadingCallBack,mode));
            onFinish?.Invoke();
        }
        /// <summary>
        /// 场景安全加载，如果场景就是当前需要切换的场景，那么会直接执行OnFinish回调，不会执行场景的Load逻辑
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="loadingCallBack"></param>
        /// <param name="onFinish"></param>
        /// <param name="mode"></param>
        public async void LoadSceneAsyncSafe(string sceneName, Action<float> loadingCallBack = null, Action<Scene> onFinish = null, LoadSceneMode mode = LoadSceneMode.Single)
        {
            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (scene.IsValid())
            {
                onFinish?.Invoke(scene);
                return;
            }
            var request = LoadSceneAsync(sceneName, mode);
            await request;
            onFinish?.Invoke(request.Scene);
        }

        [DisableEnumeratorWarning]
        [Obsolete("通过XFABManager进行场景加载时希望提前关闭场景的加载方法已经过时，请使用标准的LoadSceneAsyncWithAllowSceneActive或者使用LoadSceneAsync")]
        public IEnumerator LoadSceneAsyncWithAllowSceneActive(string sceneName, Action<AsyncOperation> onCompleted, Action<float> loadingCallBack = null, LoadSceneMode mode = LoadSceneMode.Single)
            => isInited ? SceneTool.LoadSceneAsyncWithAllowSceneActive(AssetBundleManager.LoadSceneAsync(projectName, sceneName, mode), onCompleted, loadingCallBack) : throw new Exception("没有完成对SceneTool.XFABManager的初始化，请调用一次Init方法");
    }
}
