﻿///=====================================================
/// - FileName:      SceneTool.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/26 13:13:06
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Collections;
namespace YukiFrameWork
{
	public static class SceneTool
	{	
		public static event Action<float> LoadingScene = null;
        public static event Action LoadSceneSucceed = null;

		public static void LoadScene(string sceneName,LoadSceneMode mode = LoadSceneMode.Single)
			=> SceneManager.LoadScene(sceneName,mode);

		public static void LoadScene(int sceneIndex, LoadSceneMode mode = LoadSceneMode.Single)
			=> SceneManager.LoadScene(sceneIndex,mode);

		public static void LoadScene(string sceneName, LoadSceneParameters sceneParameters)
			=> SceneManager.LoadScene(sceneName, sceneParameters);

		public static void LoadScene(int sceneIndex, LoadSceneParameters sceneParameters)
			=> SceneManager.LoadScene(sceneIndex, sceneParameters);

		public static IYieldExtension LoadSceneAsync(string sceneName,Action<float> loadingCallBack = null, LoadSceneMode mode = LoadSceneMode.Single)
		{
            var operation = SceneManager.LoadSceneAsync(sceneName, mode);
            return LoadSceneAsync(operation, loadingCallBack).Start();
        }

        public static IYieldExtension LoadSceneAsync(int sceneIndex, Action<float> loadingCallBack = null, LoadSceneMode mode = LoadSceneMode.Single)
        {
            var operation = SceneManager.LoadSceneAsync(sceneIndex, mode);
            return LoadSceneAsync(operation, loadingCallBack).Start();              
        }

        private static IEnumerator LoadSceneAsync(AsyncOperation asyncOperation, Action<float> loadingCallBack = null)
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
        public static IYieldExtension LoadSceneAsyncWithAllowSceneActive(string sceneName,Action<AsyncOperation> onCompleted, Action<float> loadingCallBack = null, LoadSceneMode mode = LoadSceneMode.Single)
        {
			var operation = SceneManager.LoadSceneAsync(sceneName, mode);			
            return LoadSceneAsyncWithAllowSceneActive(operation,onCompleted).Start();
        }

        /// <summary>
        /// 异步加载场景但是不立刻跳转，必须要实现回调
        /// </summary>
        /// <param name="sceneIndex"></param>
        /// <param name="onCompleted">加载完后的回调</param>
        /// <param name="loadingCallBack">加载的进度</param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public static IYieldExtension LoadSceneAsyncWithAllowSceneActive(int sceneIndex, Action<AsyncOperation> onCompleted, Action<float> loadingCallBack = null, LoadSceneMode mode = LoadSceneMode.Single)
        {
            var operation = SceneManager.LoadSceneAsync(sceneIndex);          
            return LoadSceneAsyncWithAllowSceneActive(operation, onCompleted).Start();
        }

        private static IEnumerator LoadSceneAsyncWithAllowSceneActive(AsyncOperation asyncOperation, Action<AsyncOperation> onCompleted, Action<float> loadingCallBack = null)
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
}
