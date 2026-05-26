///=====================================================
/// - FileName:      AddressableKit.cs
/// - NameSpace:     YukiFrameWork.Addressable
/// - Description:   高级定制脚本生成
/// - Creation Time: 4/14/2026 10:36:27 PM
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using Object = UnityEngine.Object;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;
namespace YukiFrameWork.AddressExtension
{
    
    /// <summary>
    /// Addressable资源拓展套件，提供了基于Addressable的资源加载、实例化、释放等功能的封装，简化了Addressable的使用流程，并且提供了资源条件器的概念来简化资源路径的管理
    /// </summary>
    public class AddressablesKit : Singleton<AddressablesKit>
    {
        private bool isInitialized = false;
        private bool isCompleted = false;
        
        private Dictionary<string,Dictionary<Type,AsyncOperationHandle>> _multiResHandles = new Dictionary<string, Dictionary<Type,AsyncOperationHandle>>();
        private Dictionary<string,AsyncOperationHandle> _singleResHandles = new Dictionary<string, AsyncOperationHandle>();
        private Dictionary<string,List<AsyncOperationHandle>> _instantiateResHandles = new Dictionary<string, List<AsyncOperationHandle>>();
        
        private List<string> preLoadKeys = new List<string>();
        
        private Dictionary<Type,IResourcesConditioner> _resourcesConditioners = new Dictionary<Type, IResourcesConditioner>();
        
        private List<(UniTask,bool)> initializationTasks = new List<(UniTask,bool)>();

        public event Action OnInitialize;

        public event Action OnCompleted;
        /// <summary>
        /// 管理器是否进行过初始化调用
        /// </summary>
        public bool IsInitialized => isInitialized;
        
        /// <summary>
        /// 管理器的完成操作
        /// </summary>
        public UniTask Operation => UniTask.WaitUntil(() => isCompleted);
        
        /// <summary>
        /// 管理器是否完成了所有的初始化操作s
        /// </summary>
        public bool IsCompleted => isCompleted;

        private AddressablesKit()
        {
            isCompleted = false;

            isInitialized = false;
        }
        
        /// <summary>
        /// 资源管理器添加外部初始化任务
        /// </summary>
        /// <param name="task"></param>
        /// <returns></returns>
        public AddressablesKit AddInitializeTask(UniTask task,bool preLoad = true)
        {
            initializationTasks.Add((task,preLoad));
            return this;
        }

        /// <summary>
        /// 资源套件添加以标签为标识的多资源预加载
        /// <para>指定于使用Addressables.LoadAssetsAsync方法加载()</para>>
        /// </summary>
        /// <param name="label"></param>
        /// <param name="preLoad"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public AddressablesKit PreLoadAssetsAsync<T>(string label,bool preLoad = true) where T : Object
        {
            if (!_multiResHandles.TryGetValue(label, out var resHandles))
            {
                resHandles = new Dictionary<Type, AsyncOperationHandle>();
                _multiResHandles.Add(label, resHandles);
            }

            if (resHandles.ContainsKey(typeof(T)))
            {
                Debug.LogError($"相同的句柄已加载!Res Label:{label} Type:{typeof(T)}");
                
            }
            else
            {
                var handle = Addressables.LoadAssetsAsync<T>(label,null);
                resHandles.Add(typeof(T), handle);

                if (preLoad)
                {
                    preLoadKeys.Add(label);
                }
                //  resHandle = new ResourcesHandle<T>()
            }

            return this;
        }

        public AddressablesKit PreLoadAssetAsync<TObject, ResourcesConditioner>(string objName,bool preLoad = true) 
            where TObject : Object
            where ResourcesConditioner : IResourcesConditioner,new()
        {
            
            var conditioner = GetResourcesConditioner<ResourcesConditioner>(); 
            LoadAssetAsyncPrivate<TObject>(objName, conditioner,out var path);
            if(preLoad)
                preLoadKeys.Add(path);
            return this;
        }

        public AddressablesKit PreLoadSceneAsync<TResourcesConditioner>(string sceneName,LoadSceneMode loadSceneMode = LoadSceneMode.Single,bool activeSceneLoad = true,bool preLoad = true) where TResourcesConditioner : IResourcesConditioner,new()
        {
            var conditioner = GetResourcesConditioner<TResourcesConditioner>();
            LoadSceneAsyncPrivate(sceneName, conditioner, loadSceneMode,activeSceneLoad,out var path);
            if(preLoad)
                preLoadKeys.Add(path);
            return this;
        }


        /// <summary>
        /// 根据标签加载资源句柄
        /// </summary>
        /// <param name="label"></param>
        /// <typeparam name="TObject"></typeparam>
        /// <returns></returns>
        public AsyncOperationHandle<IList<TObject>> LoadAssetsAsync<TObject>(string label) where TObject : Object
        {
            if (_multiResHandles.ContainsKey(label))
            {
                if (!_multiResHandles[label].ContainsKey(typeof(TObject)))
                {
                    _multiResHandles[label].Add(typeof(TObject), Addressables.LoadAssetsAsync<TObject>(label, null));
                }
                return _multiResHandles[label][typeof(TObject)].Convert<IList<TObject>>();
            }

            Debug.LogError("丢失资源的标签名称!请检查是否有误 Label:" + label);
            return default;
        }
        
        /// <summary>
        /// 根据标签和名称加载Label资源(仅可在将包标记了资源标签时使用)
        /// <para>传递的是不需要资源后缀的Object的名称，需要在项目中做到在同一个标签下的资源名称唯一</para>
        /// </summary>
        /// <param name="label"></param>
        /// <param name="objName"></param>
        /// <typeparam name="TObject"></typeparam>
        /// <returns></returns>
        public async UniTask<TObject> LoadAssetAsync<TObject>(string label, string objName)where TObject : Object
        {
            var handle = LoadAssetsAsync<TObject>(label);

           var result =  await handle.Wait(objName);
            
            if(!result)
                Debug.LogError("丢失资源的标签名称或资源名称!请检查是否有误 Label:" + label + " ObjName:" + objName);
            return result;
        }

        public AsyncOperationHandle<SceneInstance> LoadSceneAsync<TResourcesConditioner>(string sceneName,LoadSceneMode loadSceneMode = LoadSceneMode.Single,bool activeSceneLoad = true)
            where TResourcesConditioner : IResourcesConditioner,new()
        {
            var conditioner = GetResourcesConditioner<TResourcesConditioner>();
            return LoadSceneAsyncPrivate(sceneName, conditioner, loadSceneMode,activeSceneLoad,out _);
        }

        /// <summary>
        /// 根据资源条件器加载资源
        /// <para>传递的是不需要资源后缀的Object的名称，会自动与规则器中的路径与后缀拼接完整路径</para>
        /// </summary>
        /// <param name="objName"></param>
        /// <typeparam name="TObject"></typeparam>
        /// <typeparam name="ResourcesConditioner"></typeparam>
        /// <returns></returns>
        public AsyncOperationHandle<TObject> LoadAssetAsync<TObject,ResourcesConditioner>(string objName) 
            where ResourcesConditioner : IResourcesConditioner,new()
            where TObject : Object
        {
            var conditioner = GetResourcesConditioner<ResourcesConditioner>();

            return LoadAssetAsyncPrivate<TObject>(objName, conditioner,out _);
        }
        
        private AsyncOperationHandle<TObject> LoadAssetAsyncPrivate<TObject>(string objName,IResourcesConditioner conditioner,out string path) 
            where TObject : Object
        { 
            path = GetLocalPath(conditioner, objName);
            return LoadHandleCache<TObject>(path);
        }
        private AsyncOperationHandle<SceneInstance> LoadSceneAsyncPrivate(string sceneName,IResourcesConditioner conditioner,LoadSceneMode loadSceneMode,bool activeSceneLoad,out string path)
        {
            path = GetLocalPath(conditioner, sceneName);
            return LoadHandleCache(path, loadSceneMode,activeSceneLoad);
        }

        
        
        /// <summary>
        /// 根据资源条件器实例化GameObject
        /// <para>传递的是不需要资源后缀的GameObject的名称，会自动与规则器中的路径与后缀拼接完整路径</para>
        /// </summary>
        /// <param name="objName"></param>
        /// <typeparam name="ResourcesConditioner"></typeparam>
        /// <returns></returns>
        public AsyncOperationHandle<GameObject> InstantiateAsync<ResourcesConditioner>(string objName,Transform parent = null,bool instantiateWorldSpace = false) where ResourcesConditioner : IResourcesConditioner,new()
        {
            var conditioner = GetResourcesConditioner<ResourcesConditioner>();
            return InstantiateAsyncPrivate(conditioner, objName, parent, instantiateWorldSpace);
        }
        /// <summary>
        /// 根据资源条件器实例化GameObject
        /// <para>传递的是不需要资源后缀的GameObject的名称，会自动与规则器中的路径与后缀拼接完整路径</para>
        /// </summary>
        /// <param name="objName"></param>
        /// <typeparam name="ResourcesConditioner"></typeparam>
        /// <returns></returns>
        public AsyncOperationHandle<GameObject> InstantiateAsync<ResourcesConditioner>(string objName,Vector3 position,Quaternion rotation,Transform parent = null,bool instantiateWorldSpace = false) where ResourcesConditioner : IResourcesConditioner,new()
        {
            var conditioner = GetResourcesConditioner<ResourcesConditioner>();
            return InstantiateAsyncPrivate(conditioner, objName, position, rotation, parent, instantiateWorldSpace);
        }
        
        private AsyncOperationHandle<GameObject> InstantiateAsyncPrivate(IResourcesConditioner conditioner,string objName,Vector3 position,Quaternion rotation,Transform parent = null,bool instantiateWorldSpace = false) 
        {
            var path = GetLocalPath(conditioner, objName);
            return LoadInstantiateHandleCache(path,position, rotation, parent, instantiateWorldSpace);
        }

        private AsyncOperationHandle<GameObject> InstantiateAsyncPrivate(IResourcesConditioner conditioner,string objName,Transform parent = null,bool instantiateWorldSpace = false) 
        {
            var path = GetLocalPath(conditioner, objName);
            return LoadInstantiateHandleCache(path,parent, instantiateWorldSpace);
        }
        
        private ResourcesConditioner GetResourcesConditioner<ResourcesConditioner>() where ResourcesConditioner : IResourcesConditioner,new()
        {
            if (!_resourcesConditioners.ContainsKey(typeof(ResourcesConditioner)))
            {
                _resourcesConditioners.Add(typeof(ResourcesConditioner), new ResourcesConditioner());
            }
            return (ResourcesConditioner)_resourcesConditioners[typeof(ResourcesConditioner)];
        }

        private AsyncOperationHandle<TObject> LoadHandleCache<TObject>(string path) where TObject : Object
        {
            if(!_singleResHandles.ContainsKey(path))
                _singleResHandles.Add(path, Addressables.LoadAssetAsync<TObject>(path));
            return _singleResHandles[path].Convert<TObject>();
        }

        private AsyncOperationHandle<SceneInstance> LoadHandleCache(string path,LoadSceneMode loadSceneMode,bool activeSceneLoad)
        {
            if (!_singleResHandles.ContainsKey(path))
            {
                _singleResHandles.Add(path, Addressables.LoadSceneAsync(path, loadSceneMode,activeSceneLoad));
            }
            return _singleResHandles[path].Convert<SceneInstance>();
        }

        private AsyncOperationHandle<GameObject> LoadInstantiateHandleCache(string path,Transform parent,bool instantiateWorldSpace) 
        {
            if(!_instantiateResHandles.ContainsKey(path))
                _instantiateResHandles.Add(path, new List<AsyncOperationHandle>());
            var result = Addressables.InstantiateAsync(path, parent, instantiateWorldSpace);
            _instantiateResHandles[path].Add(result);
            return result;
        }
        
        private AsyncOperationHandle<GameObject> LoadInstantiateHandleCache(string path,Vector3 position,Quaternion rotation,Transform parent,bool instantiateWorldSpace)
        {
            if(!_instantiateResHandles.ContainsKey(path))
                _instantiateResHandles.Add(path,new List<AsyncOperationHandle>());
            var result = Addressables.InstantiateAsync(path, position, rotation, parent, instantiateWorldSpace);
            _instantiateResHandles[path].Add(result);
            return result;
        }

        
        /// <summary>
        /// 释放某一个标签的资源的句柄
        /// </summary>
        /// <param name="label"></param>
        /// <param name="type"></param>
        public void ReleaseHandle(string label,Type type)
        {
            if (!_multiResHandles.TryGetValue(label, out var resHandles))
            {
                return;
            }
            if (resHandles.TryGetValue(type, out var handle))
            {
                Addressables.Release(handle);
                resHandles.Remove(type);
            }
        }

        public void ReleaseInstanceHandle(string path)
        {
            if (_instantiateResHandles.ContainsKey(path))
            {
                for (var i = 0; i < _instantiateResHandles[path].Count; i++)
                {
                    var handle = _instantiateResHandles[path][i].Convert<GameObject>();
                    Addressables.ReleaseInstance(handle);
                }
                _instantiateResHandles.Remove(path);
            }
        }

        public void ReleaseInstanceHandle(GameObject obj)
        {
            if (!obj) return;
            foreach (var item in _instantiateResHandles)
            {
                for (var i = 0; i < item.Value.Count; i++)
                {
                    var handle = item.Value[i].Convert<GameObject>();
                    if (handle.Result == obj)
                    {
                        Addressables.ReleaseInstance(handle);
                        item.Value.RemoveAt(i);
                        break;
                    }
                }
       
               
            }
        }

        public void ReleaseInstanceHandle<ResourcesConditioner>(string objName) where ResourcesConditioner : IResourcesConditioner, new()
        {
            var conditioner = GetResourcesConditioner<ResourcesConditioner>();
            var path = GetLocalPath(conditioner, objName);
            ReleaseInstanceHandle(path);
        }

        /// <summary>
        /// 根据资源的名称和资源条件器的类型释放资源的句柄
        /// </summary>
        /// <param name="objName"></param>
        public void ReleaseHandle<ResourcesConditioner>(string objName) where ResourcesConditioner : IResourcesConditioner,new()
        {
            var conditioner = GetResourcesConditioner<ResourcesConditioner>();
            var path = GetLocalPath(conditioner, objName);
            ReleaseHandle(path);
        }

        public void ReleaseHandle(string path) 
        {
            Debug.Log("移除缓存路径:" + path);
            if (_singleResHandles.ContainsKey(path))
            {
                Addressables.Release(_singleResHandles[path]);
                _singleResHandles.Remove(path);
            }
            
        }

        
        private string GetLocalPath<ResourcesConditioner>(ResourcesConditioner conditioner,string objName) where ResourcesConditioner : IResourcesConditioner
        {
            var builder = new StringBuilder();
            builder.Append(conditioner.RulePath)
                .Append(conditioner.RulePath.EndsWith("/") ? "" : "/")
                .Append(objName);

            if (!conditioner.Suffix.IsNullOrEmpty())
            {
                builder.Append(conditioner.Suffix.StartsWith(".") ? "" : ".")
                    .Append(conditioner.Suffix);
            }


            return builder.ToString();
        }
        
        public string GetLocalPath<ResourcesConditioner>(string objName) where ResourcesConditioner : IResourcesConditioner,new()
        {
            return GetLocalPath(GetResourcesConditioner<ResourcesConditioner>(), objName);
        }


        /// <summary>
        /// 资源管理器异步初始化
        /// </summary>
        public async UniTask InitializeAsync()
        {
            try
            {
                if (isInitialized)
                    return;
                isInitialized = true;
                OnInitialize?.Invoke();
                List<UniTask> loadingTasks = new List<UniTask>();
                
                if (initializationTasks.Count > 0)
                {
                    for (var i = 0; i < initializationTasks.Count; i++)
                    {
                        
                        var item = initializationTasks[i];
                        if(item.Item2)
                            loadingTasks.Add(item.Item1);
                    }
                }

                //加载预设的资源
                

                if (preLoadKeys.Count > 0)
                {
                    for (var i = 0; i < preLoadKeys.Count; i++)
                    {
                        var item = preLoadKeys[i];
                        
                        if (_multiResHandles.TryGetValue(item, out var resHandles))
                        {
                            var handles = resHandles.Values.Select(x => x.ToUniTask()).ToArray();
                            loadingTasks.Add(UniTask.WhenAll(handles));
                        }
                        
                        if(_singleResHandles.TryGetValue(item, out var singleHandle))
                        {
                            loadingTasks.Add(singleHandle.ToUniTask());
                        }
                        
                    }
                }
                preLoadKeys.Clear();

                await UniTask.WhenAll(loadingTasks);
                isCompleted = true;
                OnCompleted?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError("资源的初始化异常，请检查问题所在，异常信息：" + ex + "\n异常堆栈:" + ex.StackTrace);
            }
        }

        /// <summary>
        /// 资源套件初始化
        /// </summary>
        public void Initialize()
        {
           _ = InitializeAsync();
        }

    }
    
}
