using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
//using UnityEngine.SceneManagement;

namespace XFABManager
{

    internal class SubObjects 
    {
        public Dictionary<string,UnityEngine.Object> objects = new Dictionary<string,UnityEngine.Object>();

        public void Add(UnityEngine.Object[] objs) 
        { 
            foreach (var item in objs)
            {
                if (objects.ContainsKey(item.name))
                    objects[item.name] = item;
                else 
                    objects.Add(item.name, item);
            }
        }

        public void Clear() 
        {
            objects.Clear();
        }
         

        public UnityEngine.Object Get(string name, Type type) 
        {
            if(objects.ContainsKey(name) && objects[name].GetType() == type) 
                return objects[name];
            return null;
        }

        public bool Contains(string name, Type type) 
        {
            if (objects.ContainsKey(name) && objects[name].GetType() == type)
                return true; 
            return false;
        }

    }

    public class AssetBundleManager
    {

        static AssetBundleManager(){
            Initialize();
            CoroutineStarter.Start(AutomaticResourceClearing());
        }

        #region 字段 

        public const int AutomaticResourceClearingTime = 300; // 默认5分钟检测一次

        /// <summary>
        /// 存放所有的 AssetBundle
        /// </summary>
        private static Dictionary<string, Dictionary<string, AssetBundle>> assetBundles = new Dictionary<string, Dictionary<string, AssetBundle>>();

        /// <summary>
        /// 某一个 AssetBundle 被依赖的引用
        /// </summary>
        private static Dictionary<AssetBundle,List<string>> dependence_assetbundles = new Dictionary<AssetBundle,List<string>>();

        /// <summary>
        /// 由于主动加载 被删掉的AssetBundle
        /// </summary>
        private static Dictionary<int, AssetBundle> depnecnce_deleted_bundles = new Dictionary<int, AssetBundle>();
         
        private static Type getProjectVersion;

        private static Dictionary<string, Dictionary<string, string>> AssetBundleNameMapping = new Dictionary<string, Dictionary<string, string>>();

        /// <summary>
        /// 当前加载的bundle 对应加载了哪些资源
        /// </summary>
        private static Dictionary<string, Dictionary<string, Dictionary<int,object>>> bundle_assets = new Dictionary<string, Dictionary<string, Dictionary<int, object>>>();

        /// <summary>
        /// 当前的bundle 对应加载了哪些子资源
        /// </summary>
        internal static Dictionary<string, Dictionary<string, Dictionary<string, SubObjects>>> bundle_sub_assets = new Dictionary<string, Dictionary<string, Dictionary<string, SubObjects>>>();


        internal static IServerFilePath ServerFilePath { get;private set; }

        private static AssetBundle dependenceBundle = null;
        private static string dependenceProjectName = string.Empty;

        /// <summary>
        /// 是否初始化
        /// </summary>
        private static bool isInited { get;set; } = false;

        /// <summary>
        /// 不能重复的异步操作的缓存 
        /// </summary>
        private static Dictionary<string, CustomYieldInstruction> async_cache = new Dictionary<string, CustomYieldInstruction>();

        public delegate CustomYieldInstruction AsyncOperationDelegate();

        /// <summary>
        /// 正在异步加载中的assetbundle
        /// </summary>
        private static Dictionary<string, AssetBundleCreateRequest> loading_assetbundle = new Dictionary<string, AssetBundleCreateRequest>();
         
        internal static Profile[] profiles => XFABManagerSettings.Instances.CurrentProfiles; 
        /// <summary>
        /// 加载出来的资源的hash_code 对应的project_name
        /// </summary>
        internal static Dictionary<int, string> asset_hash_project_name = new Dictionary<int, string>();

#if UNITY_EDITOR
        // 缓存从 Assetdatabase 加载的资源 如果每次都加载会出现卡顿的情况
        private static Dictionary<string,UnityEngine.Object> EditorAssets = new Dictionary<string, UnityEngine.Object>();
        // 缓存 LoadAllAssetsAtPath
        private static Dictionary<string, UnityEngine.Object[]> EditorAllAssets = new Dictionary<string, UnityEngine.Object[]>();
        // 缓存从 Assetdatabase 加载子资源
        private static Dictionary<string, UnityEngine.Object> EditorSubAssets = new Dictionary<string, UnityEngine.Object>();

#endif

        private static Dictionary<string, string> secrets = new Dictionary<string, string>();

        #endregion

        #region 属性

        /// <summary>
        /// 已加载的AssetBundle
        /// </summary>
        public static Dictionary<string, Dictionary<string, AssetBundle>> AssetBundles => assetBundles;

        // 获取依赖的AssetBundle
        internal static AssetBundle DependenceBundle
        {
            get
            {
                return dependenceBundle;
            }
            set
            {
                dependenceBundle = value;
            }
        }

        /// <summary>
        /// 记录某个AssetBundle中加载了哪些资源
        /// </summary>
        public static Dictionary<string, Dictionary<string, Dictionary<int, object>>> BundleLoadedAssets => bundle_assets;

        #endregion


        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        private static void Initialize()
        {
            if (isInited) return;
            // 初始化 获取项目版本接口 
            SetGetProjectVersion<GetProjectVersionDefault>(); 
            // 初始化服务器文件路径接口
            SetServerFilePath(new DefaultServerFilePath());

            // 监听场景切换的事件
            SceneManager.sceneUnloaded += (scene)=> 
            {
                SceneObject s = new SceneObject(scene.name, scene.GetHashCode());
                TimerManager.DelayInvoke(() => 
                {
                    Scene scene1 = SceneManager.GetSceneByName(s.name);
                    if (scene1.IsValid() && scene1.GetHashCode() == s.GetHashCode())
                        return;
                    // 当某一个场景被卸载时触发,当场景被卸载时同时卸载资源
                    UnloadAsset(s);
                }, 1);
            };

            isInited = true; 
        }
 
        #region 更新和下载资源

        /// <summary>
        /// 准备某个模块的资源 
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public static ReadyResRequest ReadyRes(string projectName)
        {
            // 这里不需要同时只有一个执行，因为在准备资源时，如果为释放类型，还会再次调用准备资源
            // 所以如果同时只有一个执行，这个逻辑会卡住
            ReadyResRequest request = new ReadyResRequest();
            CoroutineStarter.Start(request.ReadyRes(projectName));
            return request;
        }

        /// <summary>
        /// 根据检测结果准备资源
        /// </summary>
        /// <param name="result">检测结果</param>
        /// <returns></returns>
        public static ReadyResRequest ReadyRes(CheckUpdateResult result) 
        {
            return ReadyRes(new CheckUpdateResult[] { result });
        }

        /// <summary>
        /// 准备 某一组检测结果的资源
        /// </summary>
        /// <param name="results">检测结果</param>
        /// <returns></returns>
        public static ReadyResRequest ReadyRes(CheckUpdateResult[] results) {
            ReadyResRequest request = new ReadyResRequest();
            CoroutineStarter.Start(request.ReadyRes(results));
            return request;
        }

        /// <summary>
        /// 检测某个项目 及其依赖项目 是否需要更新 下载 或者 释放 等等
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public static CheckResUpdatesRequest CheckResUpdates(string projectName)
        {
            CheckResUpdatesRequest request = new CheckResUpdatesRequest();
            CoroutineStarter.Start(request.CheckResUpdates(projectName));
            return request;
        }

        /// <summary>
        /// 检测某个项目 是否需要更新 下载 或者 释放 等等
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public static CheckResUpdateRequest CheckResUpdate(string projectName)
        {
            CheckResUpdateRequest request = new CheckResUpdateRequest();
            CoroutineStarter.Start(request.CheckResUpdate(projectName));
            return request;
        }


        /// <summary>
        /// 从服务端获取文件 
        /// </summary>
        /// <param name="projectName">项目名</param>
        /// <param name="version">版本</param>
        /// <param name="fileName">文件名 含后缀</param>
        internal static GetFileFromServerRequest GetFileFromServer(string projectName, string version, string fileName)
        {
            string key = string.Format("GetFileFromServer:{0}{1}{2}", projectName, version, fileName);
            return ExecuteOnlyOnceAtATime<GetFileFromServerRequest>(key, () =>
            {
                GetFileFromServerRequest request = new GetFileFromServerRequest(GetProfile(projectName).url);
                CoroutineStarter.Start(request.GetFileFromServer(projectName, version, fileName));
                return request;
            });
        }

        /// <summary>
        /// 更新或下载某个模块资源
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public static UpdateOrDownloadResRequest UpdateOrDownloadRes(string projectName)
        {
            string key = string.Format("UpdateOrDownloadResRequest:{0}", projectName);
            return ExecuteOnlyOnceAtATime<UpdateOrDownloadResRequest>(key, () =>
            {
                UpdateOrDownloadResRequest request = new UpdateOrDownloadResRequest();
                CoroutineStarter.Start(request.UpdateOrDownloadRes(projectName));
                return request;
            });
        }


        /// <summary>
        /// 更新 或 下载资源  
        /// </summary>
        public static UpdateOrDownloadResRequest UpdateOrDownloadRes(CheckUpdateResult result)
        {
            string key = string.Format("UpdateOrDownloadResRequest:{0}", result.projectName);
            return ExecuteOnlyOnceAtATime<UpdateOrDownloadResRequest>(key, () =>
            {
                UpdateOrDownloadResRequest request = new UpdateOrDownloadResRequest();
                CoroutineStarter.Start(request.UpdateOrDownloadRes(result));
                return request;
            });
        }


        /// <summary>
        /// 单独下载某个模块的指定AssetBundle 及其依赖资源
        /// </summary>
        /// <param name="projectName">模块名</param>
        /// <param name="bundleName">bundle名称</param>
        /// <returns></returns>
        //public static DownloadOneAssetBundleRequest DownloadOneAssetBundle(string projectName, string bundleName)
        //{
        //    string key = string.Format("DownloadOneAssetBundle:{0}{1}", projectName,bundleName);
        //    return ExecuteOnlyOnceAtATime<DownloadOneAssetBundleRequest>(key, () =>
        //    {
        //        DownloadOneAssetBundleRequest request = new DownloadOneAssetBundleRequest();
        //        CoroutineStarter.Start(request.DownloadOneAssetBundle(projectName, bundleName));
        //        return request;
        //    });
        //}

        internal static GetProjectVersionRequest GetProjectVersion(string projectName)
        {
            string key = string.Format("GetProjectVersion:{0}", projectName);
            return ExecuteOnlyOnceAtATime<GetProjectVersionRequest>(key, () =>
            {
                GetProjectVersionRequest request = new GetProjectVersionRequest(CreateProjectVersionInstance());
                CoroutineStarter.Start(request.GetProjectVersion(projectName, GetProfile(projectName).updateModel));
                return request;
            });
        }

        /// <summary>
        /// 释放某个模块的资源 从内置目录(StreamingAssets) 复制到 数据目录(persistentDataPath)  
        /// 仅释放当前项目资源 不包含其依赖项目
        /// </summary>
        /// <param name="projectName"></param>
        public static ExtractResRequest ExtractRes(string projectName)
        {
            string key = string.Format("ExtractRes:{0}", projectName);
            return ExecuteOnlyOnceAtATime<ExtractResRequest>(key, () =>
            {
                ExtractResRequest request = new ExtractResRequest();
                CoroutineStarter.Start(request.ExtractRes(projectName));
                return request;
            });
        }

        /// <summary>
        /// 释放AssetBunle ( 从 StreamingAssets 到 persistentDataPath ) 
        /// 仅释放当前项目资源 不包含其依赖项目
        /// </summary>
        /// <param name="projectName"></param>
        public static ExtractResRequest ExtractRes(CheckUpdateResult result)
        {

            string key = string.Format("ExtractRes:{0}", result.projectName);
            return ExecuteOnlyOnceAtATime<ExtractResRequest>(key, () =>
            {
                ExtractResRequest request = new ExtractResRequest();
                CoroutineStarter.Start(request.ExtractRes(result));
                return request;
            });


        }
        #endregion

        #region 加载AssetBundle

        /// <summary>
        /// 获取某个模块 AssetBundle 的后缀名
        /// </summary>
        /// <param name="projectName"></param>
        public static string GetAssetBundleSuffix(string projectName)
        {
#if UNITY_EDITOR  
            XFABProject project = XFABProjectManager.Instance.GetProject(projectName);
            if (project != null)
                return project.suffix;
            return string.Empty;
#else
            return LocalAssetBundleInfoManager.Instance.GetSuffix(projectName);
#endif
        }

        internal static AssetBundle LoadAssetBundleFromFilePath(string bundle_path,string projectName)
        {
            if (!File.Exists(bundle_path)) return null;

            AssetBundle bundle = null;

            try
            {
                string key = GetSecret(projectName);
                if (string.IsNullOrEmpty(key) || System.IO.Path.GetExtension(bundle_path).Equals(string.Empty))
                {
                    if (Application.platform == RuntimePlatform.WebGLPlayer)
                    {
                        // WebGL平台通过字节数组加载
                        bundle = AssetBundle.LoadFromMemory(File.ReadAllBytes(bundle_path));
                    }
                    else { 
                        bundle = AssetBundle.LoadFromFile(bundle_path);
                    }

                    if (bundle == null)
                        Debug.LogErrorFormat("AssetBundle {0} 加载失败！", bundle_path);
                }
                else
                {
                    bundle = AssetBundle.LoadFromMemory(EncryptTools.Decrypt(File.ReadAllBytes(bundle_path), key));
                    if (bundle == null)
                        Debug.LogErrorFormat("AssetBundle {0} 加载失败！key:{1}", bundle_path, key);
                }
            }
            catch (Exception)
            {
                if(File.Exists(bundle_path))
                    Debug.LogErrorFormat("AssetBundle加载失败:{0} 请检测文件是否损坏或密钥是否正确!", bundle_path);
            }

            

            return bundle;
        }

        /// <summary>
        /// 加载AssetBundle
        /// </summary>
        /// <param name="projectName">项目名</param>
        /// <param name="bundleName">bundle名 需要加后缀</param>
        internal static AssetBundle LoadAssetBundle(string projectName, string bundleName)
        {

            bundleName = bundleName.ToLower();
            //string bundle_name = string.Format("{0}_{1}", projectName.ToLower(), bundleName.ToLower());

            // 判断是否 已经有这个模块的资源
            if (!assetBundles.ContainsKey(projectName))
            {
                assetBundles.Add(projectName, new Dictionary<string, AssetBundle>());
            }

            string[] dependences = GetAssetBundleDependences(projectName, bundleName);

            // 判断是否已经加载了这个AssetBundle
            if (IsLoadedAssetBundle(projectName, bundleName))
            {
                DeleteDependencesBundles(assetBundles[projectName][bundleName]); // 已经加载过这个bundle 再加载需要删除缓存依赖bundle的key
                // 同时还要添加其依赖的bundle
                for (int i = 0; i < dependences.Length; i++)
                {
                    if (IsLoadedAssetBundle(projectName, dependences[i]))
                        AddDependencesBundles(assetBundles[projectName][dependences[i]], projectName, bundleName, dependences[i]);
                    else 
                        Debug.LogWarningFormat("bundle:{0}:{1} 的依赖bundle:{2}未加载!",projectName,bundleName, dependences[i]);
                }

                return assetBundles[projectName][bundleName];
            }

            // 加载依赖bundle
            for (int i = 0; i < dependences.Length; i++)
            {
                if (IsLoadedAssetBundle(projectName, dependences[i]))
                {
                    // 添加依赖缓存
                    AddDependencesBundles(assetBundles[projectName][dependences[i]], projectName, bundleName, dependences[i]);
                    continue; 
                }

                string bundlePath = GetAssetBundlePath(projectName, dependences[i], GetAssetBundleSuffix(projectName));
                AssetBundle bundle = LoadAssetBundleFromFilePath(bundlePath,projectName);

                if (bundle != null) 
                    // 加载成功
                    assetBundles[projectName].Add(dependences[i], bundle);

                // 添加依赖缓存
                AddDependencesBundles(bundle, projectName, bundleName, dependences[i]);
            }

            // 加载bundle
            string bundle_path = GetAssetBundlePath(projectName, bundleName, GetAssetBundleSuffix(projectName));

#if XFABMANAGER_LOG_OPEN_TESTING
            Debug.LogFormat("LoadAssetBundle projectName:{0} bundleName:{1} path:{2}", projectName, bundleName, bundle_path);
#endif

            AssetBundle ab = LoadAssetBundleFromFilePath(bundle_path,projectName);
             
            if (ab != null)
                assetBundles[projectName].Add(bundleName, ab);

            DeleteDependencesBundles(ab);
            return ab;
        }

        internal static AssetBundleCreateRequest LoadAssetBundleFromFilePathAsync(string bundle_path,string projectName)
        {
            if (!File.Exists(bundle_path)) return null;

            if (loading_assetbundle.ContainsKey(bundle_path)) return loading_assetbundle[bundle_path];

            string key = GetSecret(projectName);

            AssetBundleCreateRequest request = null;

            if (string.IsNullOrEmpty(key) || Path.GetExtension(bundle_path).Equals(string.Empty) )
            {
                if (Application.platform == RuntimePlatform.WebGLPlayer)
                {
                    request = AssetBundle.LoadFromMemoryAsync(File.ReadAllBytes(bundle_path));
                }
                else {
                    request = AssetBundle.LoadFromFileAsync(bundle_path);
                }

                if (request == null) Debug.LogErrorFormat("AssetBundle {0} 加载失败！", bundle_path);
            }
            else {
                request = AssetBundle.LoadFromMemoryAsync(EncryptTools.Decrypt(File.ReadAllBytes( bundle_path), key));
                if (request == null) Debug.LogErrorFormat("AssetBundle {0} 加载失败！key:{1}", bundle_path,key);
            }

            if (request != null) {
                loading_assetbundle.Add(bundle_path, request);
                request.completed += (a) => loading_assetbundle.Remove(bundle_path);
            }
             
            return request;
        }

        internal static LoadAssetBundleRequest LoadAssetBundleAsync(string projectName, string bundleName)
        {
            //Debug.LogFormat("异步加载AssetBundle:{0}/{1}", projectName, bundleName);
            string key = string.Format("LoadAssetBundleAsync:{0}{1}", projectName, bundleName);
            //string bundle_name = string.Format("{0}_{1}",projectName.ToLower(),bundleName.ToLower());
            return ExecuteOnlyOnceAtATime<LoadAssetBundleRequest>(key, () =>
            {
                LoadAssetBundleRequest request = new LoadAssetBundleRequest();
                CoroutineStarter.Start(request.LoadAssetBundle(projectName, bundleName));
                return request;
            });
        }

        /// <summary>
        /// 判断是否已经加载某个AssetBundle
        /// 如果是编辑器模式并且从Assets加载资源一直返回True
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        public static bool IsLoadedAssetBundle(string projectName, string bundleName)
        {
#if UNITY_EDITOR
            if (AssetBundleManager.GetProfile(projectName).loadMode == LoadMode.Assets)
            {
                // 如果是 编辑器模式 并且从 Assets 加载资源 一直返回True
                return true;
            }
#endif
            return assetBundles.ContainsKey(projectName) && assetBundles[projectName].ContainsKey(bundleName);
        }

        /// <summary>
        /// 异步加载某个模块所有的AssetBundle
        /// </summary>
        /// <param name="projectName">项目名</param>
        /// <param name="onProgressChange">进度改变的事件</param>
        /// <param name="onFinsh">加载完成的事件</param>
        public static LoadAllAssetBundlesRequest LoadAllAssetBundles(string projectName)
        {
            string key = string.Format("LoadAllAssetBundles:{0}", projectName);

            return ExecuteOnlyOnceAtATime<LoadAllAssetBundlesRequest>(key, () =>
            {
                LoadAllAssetBundlesRequest request = new LoadAllAssetBundlesRequest();
                CoroutineStarter.Start(request.LoadAllAssetBundles(projectName));
                return request;
            });
        }

        /// <summary>
        /// 卸载某个AssetBundle( 内部,bundleName是真实的, 已经拼接过模块名的 )
        /// </summary>
        /// <param name="projectName">项目名</param>
        /// <param name="bundleName">AssetBundle名 </param>
        internal static void UnLoadAssetBundleInternal(string projectName, string bundleName)
        {
            if (!IsLoadedAssetBundle(projectName, bundleName))
                return; 

            if (!assetBundles.ContainsKey(projectName) || !assetBundles[projectName].ContainsKey(bundleName))
                return; 

            if (depnecnce_deleted_bundles.ContainsKey(assetBundles[projectName][bundleName].GetHashCode()))
                depnecnce_deleted_bundles.Remove(assetBundles[projectName][bundleName].GetHashCode());

            assetBundles[projectName][bundleName].Unload(true);
            assetBundles[projectName].Remove(bundleName);


            // 清空子资源 
            if (bundle_sub_assets.ContainsKey(projectName) && bundle_sub_assets[projectName].ContainsKey(bundleName)) 
            {
                foreach (var item in bundle_sub_assets[projectName][bundleName].Values)
                    item.Clear();
                bundle_sub_assets[projectName][bundleName].Clear();
            }

            // 查询到依赖的AssetBundle
            string[] dependences = GetAssetBundleDependences(projectName, bundleName);
            foreach (string dependen in dependences)
            {
                if (!IsLoadedAssetBundle(projectName, dependen)) continue;
                RemoveDependencesBundles(assetBundles[projectName][dependen], projectName, bundleName, dependen);
            } 
        }

        /// <summary>
        /// 卸载某一个AssetBundle
        /// </summary>
        /// <param name="projectName">模块名称</param>
        /// <param name="bundleName">bundle名称</param>
        //[System.Obsolete("该方法已过时,将会在未来的版本中移除,请使用AssetBundleManager.UnloadAsset代替!")] // 后面的版本把它设置成内部方法
        internal static void UnLoadAssetBundle(string projectName, string bundleName)
        {
            bundleName = string.Format("{0}_{1}", projectName, bundleName).ToLower();
            UnLoadAssetBundleInternal(projectName, bundleName);
        }

        /// <summary>
        /// 卸载某个资源模块的所有AssetBundle
        /// </summary>
        /// <param name="projectName">模块名</param>
        /// <param name="unloadAllLoadedObjects">是否卸载已加载的资源 默认:true</param>
        public static void UnLoadAllAssetBundles(string projectName, bool unloadAllLoadedObjects = true)
        {
            if (assetBundles.ContainsKey(projectName))
            {
                foreach (var bundleName in assetBundles[projectName].Keys)
                {
                    if(depnecnce_deleted_bundles.ContainsKey(assetBundles[projectName][bundleName].GetHashCode()))
                        depnecnce_deleted_bundles.Remove(assetBundles[projectName][bundleName].GetHashCode());
                    assetBundles[projectName][bundleName].Unload(unloadAllLoadedObjects);

                }
                assetBundles[projectName].Clear();
            }
        }

        internal static void AddDependencesBundles(AssetBundle dependence,string projectName, string bundleName,string dependenceName) {
            if (dependence == null) return;

            // 如果依赖的字典中没有这个包 但是这个包已经加载了 说明这个包被 delete 了,所以不需要加进去了
            //if (!dependence_assetbundles.ContainsKey(dependence) && IsLoadedAssetBundle(projectName, dependenceName)) return;

            if (depnecnce_deleted_bundles.ContainsKey(dependence.GetHashCode())) return;

            //Debug.LogErrorFormat("add_Dependence : {0}",dependence);

            if (!dependence_assetbundles.ContainsKey(dependence))
                dependence_assetbundles.Add(dependence, new List<string>());

            string bundle_info = string.Format("{0}/{1}", projectName, bundleName);

            if (!dependence_assetbundles[dependence].Contains(bundle_info))
                dependence_assetbundles[dependence].Add(bundle_info);
        }

        internal static void RemoveDependencesBundles(AssetBundle dependence, string projectName, string bundleName,string dependenceName) {
            if (dependence == null) return;

            //foreach (var item in dependence_assetbundles.Keys)
            //{
            //    Debug.LogErrorFormat("dependence_assetbundle : {0}", item.name);
            //}

            if (!dependence_assetbundles.ContainsKey(dependence)) return;
            string bundle_info = string.Format("{0}/{1}", projectName, bundleName);
            dependence_assetbundles[dependence].Remove(bundle_info);
            if (dependence_assetbundles[dependence].Count == 0) {
                // 卸载 AssetBundle
                dependence_assetbundles.Remove(dependence);
                // 卸载依赖包
                //Debug.LogFormat("卸载依赖包:{0} {1} {2}", projectName, dependenceName, dependence.name);
                UnLoadAssetBundleInternal(projectName, dependenceName);
            }
        }

        internal static void DeleteDependencesBundles(AssetBundle dependence) {
            if (dependence == null) return;
            if (!dependence_assetbundles.ContainsKey(dependence)) return;
            //Debug.LogFormat("delete:{0}",dependence.name);
            dependence_assetbundles.Remove(dependence);

            if(!depnecnce_deleted_bundles.ContainsKey(dependence.GetHashCode()))
                depnecnce_deleted_bundles.Add(dependence.GetHashCode(), dependence);

            //foreach (var item in dependence_assetbundles.Keys)
            //{
            //    Debug.LogErrorFormat("dependence_assetbundle : {0}",item.name);
            //}

        }

#endregion

        #region 加载资源

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName">没有后缀</param>
        /// <returns></returns>
        [System.Obsolete("该方法已经过时,将会在未来的版本中移除,请使用AssetBundleManager.LoadAsset<T>(string projectName, string assetName) 代替!")]
        public static T LoadAsset<T>(string projectName, string bundleName, string assetName) where T : UnityEngine.Object
        {
            string bundle_name = GetBundleName(projectName, assetName, typeof(T));
            if (string.IsNullOrEmpty(bundle_name)) 
                bundle_name = bundleName;
            return LoadAssetInternal<T>(projectName, bundle_name, assetName);
        }
        // 加载资源
        [System.Obsolete("该方法已经过时,将会在未来的版本中移除,请使用AssetBundleManager.LoadAsset(string projectName,string assetName, Type type) 代替!")]
        public static UnityEngine.Object LoadAsset(string projectName, string bundleName, string assetName, Type type)
        {
            string bundle_name = GetBundleName(projectName, assetName, type);
            if (string.IsNullOrEmpty(bundle_name))
                bundle_name = bundleName;
            
            return LoadAssetInternal(projectName, bundle_name, assetName, type);
        }

        // 异步加载资源 
        [System.Obsolete("该方法已经过时,将会在未来的版本中移除,请使用AssetBundleManager.LoadAssetAsync<T>(string projectName , string assetName) 代替!")]
        public static LoadAssetRequest LoadAssetAsync<T>(string projectName, string bundleName, string assetName) where T : UnityEngine.Object
        {
            string bundle_name = GetBundleName(projectName, assetName, typeof(T));
            if (string.IsNullOrEmpty(bundle_name))
                bundle_name = bundleName;
            return LoadAssetAsyncInternal<T>(projectName, bundle_name, assetName);
        }
        [System.Obsolete("该方法已经过时,将会在未来的版本中移除,请使用AssetBundleManager.LoadAssetAsync(string projectName , string assetName, Type type) 代替!")]
        public static LoadAssetRequest LoadAssetAsync(string projectName, string bundleName, string assetName, Type type)
        {
            string bundle_name = GetBundleName(projectName, assetName, type);
            if (string.IsNullOrEmpty(bundle_name))
                bundle_name = bundleName;
            return LoadAssetAsyncInternal(projectName, bundle_name, assetName,type);
        }

        // 加载子资源 
        [System.Obsolete("该方法已经过时,将会在未来的版本中移除,请使用AssetBundleManager.LoadAssetWithSubAssets<T>(string projectName , string assetName) 代替!")]
        public static T[] LoadAssetWithSubAssets<T>(string projectName, string bundleName, string assetName) where T : UnityEngine.Object
        {
            string bundle_name = GetBundleName(projectName, assetName, typeof(T));
            if (string.IsNullOrEmpty(bundle_name))
                bundle_name = bundleName;
            return LoadAssetWithSubAssetsInternal<T>(projectName, bundle_name, assetName);
        }
        [System.Obsolete("该方法已经过时,将会在未来的版本中移除,请使用AssetBundleManager.LoadAssetWithSubAssets(string projectName , string assetName, Type type) 代替!")]
        public static UnityEngine.Object[] LoadAssetWithSubAssets(string projectName, string bundleName, string assetName, Type type)
        {
            string bundle_name = GetBundleName(projectName, assetName, type);
            if (string.IsNullOrEmpty(bundle_name))
                bundle_name = bundleName;
            return LoadAssetWithSubAssetsInternal(projectName, bundle_name, assetName,type);
        }

        /// <summary>
        /// 异步加载子资源
        /// </summary>
        [System.Obsolete("该方法已经过时,将会在未来的版本中移除,请使用AssetBundleManager.LoadAssetWithSubAssetsAsync<T>(string projectName , string assetName) 代替!")]
        public static LoadAssetsRequest LoadAssetWithSubAssetsAsync<T>(string projectName, string bundleName, string assetName) where T : UnityEngine.Object
        {
            string bundle_name = GetBundleName(projectName, assetName, typeof(T));
            if (string.IsNullOrEmpty(bundle_name))
                bundle_name = bundleName;
            return LoadAssetWithSubAssetsAsyncInternal<T>(projectName, bundle_name, assetName);
        }
        /// <summary>
        /// 异步加载子资源
        /// </summary>
        [System.Obsolete("该方法已经过时,将会在未来的版本中移除,请使用AssetBundleManager.LoadAssetWithSubAssetsAsync(string projectName , string assetName, Type type) 代替!")]
        public static LoadAssetsRequest LoadAssetWithSubAssetsAsync(string projectName, string bundleName, string assetName, Type type)
        {
            string bundle_name = GetBundleName(projectName, assetName,type);
            if (string.IsNullOrEmpty(bundle_name))
                bundle_name = bundleName;
            return LoadAssetWithSubAssetsAsyncInternal(projectName, bundle_name, assetName,type);
        }

        /// <summary>
        /// 加载某个AssetBundle的所有资源
        /// </summary>
        /// <returns></returns>
        public static UnityEngine.Object[] LoadAllAssets(string projectName, string bundleName)
        {
            return LoadAllAssetsInternal(projectName,bundleName);
        }
        /// <summary>
        /// 异步加载某个资源包中所有的资源
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="bundleName"></param>
        public static LoadAssetsRequest LoadAllAssetsAsync(string projectName, string bundleName)
        {
            return LoadAllAssetsAsyncInternal(projectName ,bundleName);
        }

        /// <summary>
        /// 加载某个类型所有资源
        /// </summary>
        /// <returns></returns>
        public static T[] LoadAllAssets<T>(string projectName, string bundleName) where T : UnityEngine.Object
        {
            return LoadAllAssetsInternal<T>(projectName,bundleName);
        }

        /// <summary>
        /// 加载某个类型所有资源
        /// </summary>
        /// <returns></returns>
        public static UnityEngine.Object[] LoadAllAssets(string projectName, string bundleName, Type type)
        { 
            return LoadAllAssetsInternal(projectName,bundleName ,type);
        }

        /// <summary>
        /// 异步加载某个类型的所有资源
        /// </summary>
        public static LoadAssetsRequest LoadAllAssetsAsync<T>(string projectName, string bundleName) where T : UnityEngine.Object
        {
            return LoadAllAssetsAsyncInternal<T>(projectName,bundleName);
        }
        /// <summary>
        /// 异步加载某个类型的所有资源
        /// </summary>
        public static LoadAssetsRequest LoadAllAssetsAsync(string projectName, string bundleName, Type type)
        {
            return LoadAllAssetsAsyncInternal(projectName,bundleName,type);
        }
        [System.Obsolete("该方法已经过时,将会在未来的版本中移除,请使用AssetBundleManager.LoadScene(string projectName , string sceneName, LoadSceneMode mode) 代替!")]
        public static void LoadScene(string projectName, string bundleName, string sceneName, LoadSceneMode mode) {
            string bundle_name = GetBundleName(projectName, sceneName, typeof(SceneObject));
            if (string.IsNullOrEmpty(bundle_name))
                bundle_name = bundleName;
            LoadSceneInternal(projectName, bundle_name, sceneName,mode);
        }

        // UnityEditor.SceneAsset
        [System.Obsolete("该方法已经过时,将会在未来的版本中移除,请使用AssetBundleManager.LoadSceneAsync(string projectName , string sceneName, LoadSceneMode mode) 代替!")]
        public static AsyncOperation LoadSceneAsync(string projectName, string bundleName, string sceneName, LoadSceneMode mode) {
            string bundle_name = GetBundleName(projectName, sceneName, typeof(SceneObject));
            if (string.IsNullOrEmpty(bundle_name))
                bundle_name = bundleName;
            return LoadSceneAsyncInternal(projectName, bundle_name, sceneName,mode);
        }

        #endregion

        #region 加载资源(without bundleName)

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName">没有后缀</param>
        /// <returns></returns>
        //[System.Obsolete("该方法已经过时,请")]
        public static T LoadAsset<T>(string projectName , string assetName) where T : UnityEngine.Object
        {

            if (LoaderTips.AllLoaderTips.ContainsKey(typeof(T)))
            {
                if (LoaderTips.AllLoaderTips[typeof(T)].IsThrowException)
                    throw new Exception(LoaderTips.AllLoaderTips[typeof(T)].tips);
                else
                    Debug.LogWarning(LoaderTips.AllLoaderTips[typeof(T)].tips);
            }

            string bundle_name = GetBundleName(projectName, assetName, typeof(T));

#if XFABMANAGER_LOG_OPEN_TESTING
            Debug.LogFormat("LoadAsset projectName:{0} bundleName:{1} assetName:{2} type:{3}",projectName,bundle_name,assetName,typeof(T));
#endif

            if (string.IsNullOrEmpty(bundle_name)) { 
                //Debug.LogException(new Exception(String.Format("bundleName 查询失败: assetName: {0} type:{1}", assetName, typeof(T).FullName)));
                return null;
            }
            return LoadAssetInternal<T>(projectName, bundle_name, assetName);
        }
        // 加载资源
        public static UnityEngine.Object LoadAsset(string projectName, string assetName, Type type)
        {

            if (LoaderTips.AllLoaderTips.ContainsKey(type))
            {
                if (LoaderTips.AllLoaderTips[type].IsThrowException)
                    throw new Exception(LoaderTips.AllLoaderTips[type].tips);
                else
                    Debug.LogWarning(LoaderTips.AllLoaderTips[type].tips);
            }

            string bundle_name = GetBundleName(projectName, assetName, type);
            if (string.IsNullOrEmpty(bundle_name)) {
                //Debug.LogException(new Exception(String.Format("bundleName 查询失败: assetName: {0} type:{1}", assetName, type.FullName)));
                return null;
            }

            return LoadAssetInternal(projectName, bundle_name, assetName, type);
        }


        internal static T LoadAssetWithoutTips<T>(string projectName, string assetName) where T : UnityEngine.Object
        {

            string bundle_name = GetBundleName(projectName, assetName, typeof(T));
            if (string.IsNullOrEmpty(bundle_name))
            {
                //Debug.LogException(new Exception(String.Format("bundleName 查询失败: assetName: {0} type:{1}", assetName, typeof(T).FullName)));
                return null;
            }
            return LoadAssetInternal<T>(projectName, bundle_name, assetName);
        }
        // 加载资源
        internal static UnityEngine.Object LoadAssetWithoutTips(string projectName, string assetName, Type type)
        {
            
            string bundle_name = GetBundleName(projectName, assetName, type);
            if (string.IsNullOrEmpty(bundle_name))
            {
                //Debug.LogException(new Exception(String.Format("bundleName 查询失败: assetName: {0} type:{1}", assetName, type.FullName)));
                return null;
            }

            return LoadAssetInternal(projectName, bundle_name, assetName, type);
        }


        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="projectName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static LoadAssetRequest LoadAssetAsync<T>(string projectName, string assetName) where T : UnityEngine.Object
        {

            if (LoaderTips.AllLoaderTips.ContainsKey(typeof(T))) {
                if (LoaderTips.AllLoaderTips[typeof(T)].IsThrowException)
                    throw new Exception(LoaderTips.AllLoaderTips[typeof(T)].tips);
                else 
                    Debug.LogWarning(LoaderTips.AllLoaderTips[typeof(T)].tips);
            }

            string bundle_name = GetBundleName(projectName, assetName, typeof(T));
            if (string.IsNullOrEmpty(bundle_name)) {
                //Debug.LogException(new Exception(String.Format("bundleName 查询失败: assetName: {0} type:{1}", assetName, typeof(T).FullName)));
                return null;
            }
            return LoadAssetAsyncInternal<T>(projectName, bundle_name, assetName);
        }
        public static LoadAssetRequest LoadAssetAsync(string projectName, string assetName, Type type)
        {
            if (LoaderTips.AllLoaderTips.ContainsKey(type))
            {
                if (LoaderTips.AllLoaderTips[type].IsThrowException)
                    throw new Exception(LoaderTips.AllLoaderTips[type].tips);
                else
                    Debug.LogWarning(LoaderTips.AllLoaderTips[type].tips);
            }

            string bundle_name = GetBundleName(projectName, assetName, type);
            if (string.IsNullOrEmpty(bundle_name)) { 
                //Debug.LogException( new Exception(String.Format("bundleName 查询失败: assetName: {0} type:{1}", assetName, type.FullName)));
                return null;
            }
            return LoadAssetAsyncInternal(projectName, bundle_name, assetName, type);
        }

        // 异步加载资源 
        internal static LoadAssetRequest LoadAssetAsyncWithoutTips<T>(string projectName, string assetName) where T : UnityEngine.Object
        {
            string bundle_name = GetBundleName(projectName, assetName, typeof(T));
            if (string.IsNullOrEmpty(bundle_name))
            {
                //Debug.LogException(new Exception(String.Format("bundleName 查询失败: assetName: {0} type:{1}", assetName, typeof(T).FullName)));
                return null;
            }
            return LoadAssetAsyncInternal<T>(projectName, bundle_name, assetName);
        }
        internal static LoadAssetRequest LoadAssetAsyncWithoutTips(string projectName, string assetName, Type type)
        {
            string bundle_name = GetBundleName(projectName, assetName, type);
            if (string.IsNullOrEmpty(bundle_name))
            {
                //Debug.LogException( new Exception(String.Format("bundleName 查询失败: assetName: {0} type:{1}", assetName, type.FullName)));
                return null;
            }
            return LoadAssetAsyncInternal(projectName, bundle_name, assetName, type);
        }

        /// <summary>
        /// 加载子资源
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="projectName">资源模块名称</param>
        /// <param name="mainAssetName">主资源名称</param>
        /// <param name="subAssetName">子资源名称</param>
        /// <returns></returns>
        public static T LoadSubAsset<T>(string projectName, string mainAssetName, string subAssetName)where T : UnityEngine.Object
        { 
            return LoadSubAsset(projectName,mainAssetName,subAssetName,typeof(T)) as T;
        }

        /// <summary>
        /// 加载子资源
        /// </summary>
        /// <param name="projectName">资源模块名称</param>
        /// <param name="mainAssetName">主资源名称</param>
        /// <param name="subAssetName">子资源名称</param>
        /// <param name="type">类型</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static UnityEngine.Object LoadSubAsset(string projectName, string mainAssetName, string subAssetName, Type type) {

            if (LoaderTips.AllLoaderTips.ContainsKey(type))
            {
                if (LoaderTips.AllLoaderTips[type].IsThrowException)
                    throw new Exception(LoaderTips.AllLoaderTips[type].tips);
                else
                    Debug.LogWarning(LoaderTips.AllLoaderTips[type].tips);
            }

            string bundle_name = GetBundleName(projectName, mainAssetName, type);
            if (string.IsNullOrEmpty(bundle_name)) 
                return null; 
            return LoadSubAssetInternal(projectName,bundle_name,mainAssetName,subAssetName, type);
        }

        internal static T LoadSubAssetWithoutTip<T>(string projectName, string mainAssetName, string subAssetName) where T : UnityEngine.Object
        {
            return LoadSubAssetWithoutTip(projectName, mainAssetName, subAssetName, typeof(T)) as T;
        }

        internal static UnityEngine.Object LoadSubAssetWithoutTip(string projectName, string mainAssetName, string subAssetName, Type type)
        {
            string bundle_name = GetBundleName(projectName, mainAssetName, type);
            if (string.IsNullOrEmpty(bundle_name))
                return null;
            return LoadSubAssetInternal(projectName, bundle_name, mainAssetName, subAssetName, type);
        }

        /// <summary>
        /// 异步加载子资源
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="projectName">资源模块名称</param>
        /// <param name="mainAssetName">主资源名称</param>
        /// <param name="subAssetName">子资源名称</param>
        /// <returns></returns>
        public static LoadSubAssetRequest LoadSubAssetAsync<T>(string projectName, string mainAssetName, string subAssetName)where T : UnityEngine.Object
        {
            return LoadSubAssetAsync(projectName, mainAssetName, subAssetName, typeof(T));
        }

        /// <summary>
        /// 异步加载子资源
        /// </summary>
        /// <param name="projectName">资源模块名称</param>
        /// <param name="mainAssetName">主资源名称</param>
        /// <param name="subAssetName">子资源名称</param>
        /// <param name="type">类型</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static LoadSubAssetRequest LoadSubAssetAsync(string projectName, string mainAssetName, string subAssetName, Type type) {
            if (LoaderTips.AllLoaderTips.ContainsKey(type))
            {
                if (LoaderTips.AllLoaderTips[type].IsThrowException)
                    throw new Exception(LoaderTips.AllLoaderTips[type].tips);
                else
                    Debug.LogWarning(LoaderTips.AllLoaderTips[type].tips);
            }

            string bundle_name = GetBundleName(projectName, mainAssetName, type);
            if (string.IsNullOrEmpty(bundle_name)) 
                return null; 
            return LoadSubAssetAsyncInternal(projectName, bundle_name,mainAssetName,subAssetName, type);
        }


        public static LoadSubAssetRequest LoadSubAssetAsyncWithoutTip<T>(string projectName, string mainAssetName, string subAssetName) where T : UnityEngine.Object
        {
            return LoadSubAssetAsyncWithoutTip(projectName, mainAssetName, subAssetName, typeof(T));
        }

        public static LoadSubAssetRequest LoadSubAssetAsyncWithoutTip(string projectName, string mainAssetName, string subAssetName, Type type)
        { 
            string bundle_name = GetBundleName(projectName, mainAssetName, type);
            if (string.IsNullOrEmpty(bundle_name))
                return null;
            return LoadSubAssetAsyncInternal(projectName, bundle_name, mainAssetName, subAssetName, type);
        }


        /// <summary>
        /// 加载资源及所有子资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="projectName"></param>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static T[] LoadAssetWithSubAssets<T>(string projectName , string assetName) where T : UnityEngine.Object
        {
            string bundle_name = GetBundleName(projectName, assetName, typeof(T));
            if (string.IsNullOrEmpty(bundle_name)) { 
                //Debug.LogException( new Exception(String.Format("bundleName 查询失败: assetName: {0} type:{1}", assetName, typeof(T).FullName)));
                return null;
            }
            return LoadAssetWithSubAssetsInternal<T>(projectName, bundle_name, assetName);
        }
        
        /// <summary>
        /// 加载资源及所有子资源
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="assetName"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static UnityEngine.Object[] LoadAssetWithSubAssets(string projectName , string assetName, Type type)
        {
            string bundle_name = GetBundleName(projectName, assetName, type);
            if (string.IsNullOrEmpty(bundle_name)) { 
                //Debug.LogException( new Exception(String.Format("bundleName 查询失败: assetName: {0} type:{1}", assetName, type.FullName)));
                return null;
            }
            return LoadAssetWithSubAssetsInternal(projectName, bundle_name, assetName, type);
        }

        /// <summary>
        /// 异步加载资源及所有子资源
        /// </summary>
        public static LoadAssetsRequest LoadAssetWithSubAssetsAsync<T>(string projectName , string assetName) where T : UnityEngine.Object
        {
            string bundle_name = GetBundleName(projectName, assetName, typeof(T));
            if (string.IsNullOrEmpty(bundle_name)) { 
                //Debug.LogException( new Exception(String.Format("bundleName 查询失败: assetName: {0} type:{1}", assetName, typeof(T).FullName)));
                return null;
            }
            return LoadAssetWithSubAssetsAsyncInternal<T>(projectName, bundle_name, assetName);
        }
        /// <summary>
        /// 异步加载资源及所有子资源
        /// </summary>
        public static LoadAssetsRequest LoadAssetWithSubAssetsAsync(string projectName, string assetName, Type type)
        {
            string bundle_name = GetBundleName(projectName, assetName, type);
            if (string.IsNullOrEmpty(bundle_name)) { 
                //Debug.LogException( new Exception(String.Format("bundleName 查询失败: assetName: {0} type:{1}", assetName, type.FullName)));
                return null;
            }
            return LoadAssetWithSubAssetsAsyncInternal(projectName, bundle_name, assetName, type);
        }
          
        /// <summary>
        /// 加载打包在AssetBundle中的场景
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="sceneName"></param>
        /// <param name="mode"></param>
        public static void LoadScene(string projectName , string sceneName, LoadSceneMode mode)
        {
            string bundle_name = GetBundleName(projectName, sceneName, typeof(SceneObject));
            if (string.IsNullOrEmpty(bundle_name)) { 
                //Debug.LogException(new Exception(String.Format("bundleName 查询失败: assetName: {0} type:{1}", sceneName, typeof(Scene).FullName)));
                return;
            }
            LoadSceneInternal(projectName, bundle_name, sceneName, mode);
        }

        // UnityEditor.SceneAsset
        /// <summary>
        /// 异步加载打包在AssetBundle中的场景
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="sceneName"></param>
        /// <param name="mode"></param>
        public static AsyncOperation LoadSceneAsync(string projectName , string sceneName, LoadSceneMode mode)
        {
            string bundle_name = GetBundleName(projectName, sceneName, typeof(SceneObject));
            if (string.IsNullOrEmpty(bundle_name)) { 
                //Debug.LogException( new Exception(String.Format("bundleName 查询失败: assetName: {0} type:{1}", sceneName, typeof(Scene).FullName)));
                return null;
            }
            return LoadSceneAsyncInternal(projectName, bundle_name, sceneName, mode);
        }

        #endregion

        #region 加载资源(内部)
        /// <summary>
        /// 加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName">没有后缀</param>
        /// <returns></returns>
        internal static T LoadAssetInternal<T>(string projectName, string bundleName, string assetName) where T : UnityEngine.Object
        {
            return LoadAssetInternal(  projectName,   bundleName,   assetName,typeof(T)) as T;
        }
        // 加载资源
        internal static UnityEngine.Object LoadAssetInternal(string projectName, string bundleName, string assetName, Type type)
        {

            if (string.IsNullOrEmpty(projectName) || string.IsNullOrEmpty(bundleName) || string.IsNullOrEmpty(assetName))
            {
                return null;
            }

#if UNITY_EDITOR
            if (GetProfile(projectName).loadMode == LoadMode.Assets)
            {
                XFABAssetBundle bundle = GetXFABAssetBundle(projectName, bundleName);
                //string path = bundle.GetAssetPathByFileName(assetName);
                //Debug.LogFormat("path:{0} type:{1}",path,type.Name); 
                return LoadAssetFromEditor(bundle != null ? bundle.GetAssetPathByFileName(assetName,type) : string.Empty, type);
            }
#endif
            string bundle_name = string.Format("{0}_{1}", projectName, bundleName);
            AssetBundle assetBundle = LoadAssetBundle(projectName, bundle_name);

            if (assetBundle != null)
            {
                UnityEngine.Object asset = assetBundle.LoadAsset(assetName, type);
                AddAssetCache(projectName, bundleName, asset);
                return asset;
            }

            return null;
        }

        // 异步加载资源 
        internal static LoadAssetRequest LoadAssetAsyncInternal<T>(string projectName, string bundleName, string assetName) where T : UnityEngine.Object
        {
            LoadAssetRequest request = new LoadAssetRequest();
            //CoroutineStarter.Start(request.LoadAssetAsync<T>(projectName, bundleName, assetName)); 
            CoroutineStarter.Start(request.LoadAssetAsync(projectName, bundleName, assetName, typeof(T))); 
            return request;
        }
        internal static LoadAssetRequest LoadAssetAsyncInternal(string projectName, string bundleName, string assetName, Type type)
        {
            LoadAssetRequest request = new LoadAssetRequest();
            CoroutineStarter.Start(request.LoadAssetAsync(projectName, bundleName, assetName, type));
            return request;
        }

        internal static T LoadSubAssetInternal<T>(string projectName, string bundleName, string mainAssetName, string subAssetName)where T : UnityEngine.Object
        {
            return LoadSubAssetInternal(projectName, bundleName, mainAssetName, subAssetName, typeof(T)) as T;
        }

        internal static UnityEngine.Object LoadSubAssetInternal(string projectName, string bundleName, string mainAssetName, string subAssetName, Type type)
        {

            if (string.IsNullOrEmpty(projectName) || string.IsNullOrEmpty(bundleName) || string.IsNullOrEmpty(mainAssetName))
            {
                return null;
            }

#if UNITY_EDITOR
            if (GetProfile(projectName).loadMode == LoadMode.Assets)
            {
                XFABAssetBundle bundle = GetXFABAssetBundle(projectName, bundleName);
                string asset_path = bundle.GetAssetPathByFileName(mainAssetName, type);

                string key = string.Format("{0}/{1}",asset_path,subAssetName);

                if( EditorSubAssets.ContainsKey(key))
                    return EditorSubAssets[key];

                UnityEngine.Object[] objs = LoadAllAssetFromEditor(asset_path);

                foreach (var item in objs)
                {
                    key = string.Format("{0}/{1}", asset_path, item.name);
                    if (EditorSubAssets.ContainsKey(key)) continue;
                    EditorSubAssets.Add(key, item);
                }

                key = string.Format("{0}/{1}", asset_path, subAssetName); 
                if (EditorSubAssets.ContainsKey(key))
                    return EditorSubAssets[key];

                return null;
            }
#endif
            if (bundle_sub_assets.ContainsKey(projectName) &&
               bundle_sub_assets[projectName].ContainsKey(bundleName) &&
               bundle_sub_assets[projectName][bundleName].ContainsKey(mainAssetName) &&
               bundle_sub_assets[projectName][bundleName][mainAssetName].Contains(subAssetName, type)) 
            {
                return bundle_sub_assets[projectName][bundleName][mainAssetName].Get(subAssetName, type);
            }

            string bundle_name = string.Format("{0}_{1}", projectName, bundleName);
            AssetBundle assetBundle = LoadAssetBundle(projectName, bundle_name);

            if (assetBundle != null)
            {
                UnityEngine.Object[] assets = assetBundle.LoadAssetWithSubAssets(mainAssetName, type);
                AddSubAssets(projectName, bundleName,mainAssetName, assets);

                UnityEngine.Object asset = bundle_sub_assets[projectName][bundleName][mainAssetName].Get(subAssetName, type);
                AddAssetCache(projectName, bundleName, asset);
                return asset;
            }

            return null;
        }

        internal static LoadSubAssetRequest LoadSubAssetAsyncInternal<T>(string projectName, string bundleName, string mainAssetName, string subAssetName )
        {
            return LoadSubAssetAsyncInternal(projectName,bundleName,mainAssetName,subAssetName,typeof(T));
        }

        internal static LoadSubAssetRequest LoadSubAssetAsyncInternal(string projectName, string bundleName, string mainAssetName, string subAssetName,Type type) 
        {
            LoadSubAssetRequest request = new LoadSubAssetRequest();
            CoroutineStarter.Start(request.LoadSubAssetAsync(projectName, bundleName, mainAssetName,subAssetName, type));
            return request; 
        }

        // 加载子资源 
        internal static T[] LoadAssetWithSubAssetsInternal<T>(string projectName, string bundleName, string assetName) where T : UnityEngine.Object
        { 
            UnityEngine.Object[] objs = LoadAssetWithSubAssetsInternal(projectName, bundleName, assetName, typeof(T)); 
            if (objs != null) 
                return ArrayCast<T>(objs);  
            return null;
        }
        internal static UnityEngine.Object[] LoadAssetWithSubAssetsInternal(string projectName, string bundleName, string assetName, Type type)
        {

            if (string.IsNullOrEmpty(projectName) || string.IsNullOrEmpty(bundleName) || string.IsNullOrEmpty(assetName))
            {
                return null;
            }

#if UNITY_EDITOR
            if (GetProfile(projectName).loadMode == LoadMode.Assets)
            {
                XFABAssetBundle bundle = GetXFABAssetBundle(projectName, bundleName);
                string asset_path = bundle.GetAssetPathByFileName(assetName, type);
                return bundle != null ? ArrayCast(LoadAllAssetFromEditor(asset_path), type) : null;
            }
#endif
            string bundle_name = string.Format("{0}_{1}", projectName, bundleName);
            AssetBundle assetBundle = LoadAssetBundle(projectName, bundle_name);

            if (assetBundle != null)
            {
                UnityEngine.Object[] objs = assetBundle.LoadAssetWithSubAssets(assetName,type);
                AddAssetCache(projectName, bundleName, objs);
                return objs;
            }

            return null;
        }

        /// <summary>
        /// 异步加载子资源
        /// </summary>
        internal static LoadAssetsRequest LoadAssetWithSubAssetsAsyncInternal<T>(string projectName, string bundleName, string assetName) where T : UnityEngine.Object
        {
            LoadAssetsRequest request = new LoadAssetsRequest();
            //CoroutineStarter.Start(request.LoadAssetWithSubAssetsAsync<T>(projectName, bundleName, assetName));
            CoroutineStarter.Start(request.LoadAssetWithSubAssetsAsync(projectName, bundleName, assetName, typeof(T)));
            return request;
        }
        /// <summary>
        /// 异步加载子资源
        /// </summary>
        internal static LoadAssetsRequest LoadAssetWithSubAssetsAsyncInternal(string projectName, string bundleName, string assetName, Type type)
        {
            LoadAssetsRequest request = new LoadAssetsRequest();
            CoroutineStarter.Start(request.LoadAssetWithSubAssetsAsync(projectName, bundleName, assetName, type));
            return request;
        }

        /// <summary>
        /// 加载所有资源
        /// </summary>
        /// <returns></returns>
        internal static UnityEngine.Object[] LoadAllAssetsInternal(string projectName, string bundleName)
        {
            if (string.IsNullOrEmpty(projectName) || string.IsNullOrEmpty(bundleName))
            {
                return null;
            }
#if UNITY_EDITOR
            if (GetProfile(projectName).loadMode == LoadMode.Assets)
            {
                XFABAssetBundle bundle = GetXFABAssetBundle(projectName, bundleName);
                return LoadAssetsFromAssets(bundle.GetAllAssetPaths());
            }
#endif
            string bundle_name = string.Format("{0}_{1}", projectName, bundleName);
            AssetBundle assetBundle = LoadAssetBundle(projectName, bundle_name);

            if (assetBundle != null) {
                UnityEngine.Object[] objs = assetBundle.LoadAllAssets();
                AddAssetCache(projectName,bundleName, objs);
                return objs;
            }

            return null;
        }
        /// <summary>
        /// 异步加载所有资源
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="bundleName"></param>
        internal static LoadAssetsRequest LoadAllAssetsAsyncInternal(string projectName, string bundleName)
        {
            LoadAssetsRequest request = new LoadAssetsRequest();
            CoroutineStarter.Start(request.LoadAllAssetsAsync(projectName, bundleName));
            return request;
        }

        /// <summary>
        /// 加载某个类型所有资源
        /// </summary>
        /// <returns></returns>
        internal static T[] LoadAllAssetsInternal<T>(string projectName, string bundleName) where T : UnityEngine.Object
        { 
            UnityEngine.Object[] objs = LoadAllAssetsInternal(  projectName,   bundleName, typeof(T));
            if (objs != null)
                return ArrayCast<T>(objs);
            return null;
        }

        /// <summary>
        /// 加载某个类型所有资源
        /// </summary>
        /// <returns></returns>
        internal static UnityEngine.Object[] LoadAllAssetsInternal(string projectName, string bundleName, Type type)
        {

            if (string.IsNullOrEmpty(projectName) || string.IsNullOrEmpty(bundleName))
            {
                return null;
            }

#if UNITY_EDITOR
            if (GetProfile(projectName).loadMode == LoadMode.Assets)
            {
                XFABAssetBundle bundle = GetXFABAssetBundle(projectName, bundleName);
                return ArrayCast(LoadAssetsFromAssets(bundle.GetAllAssetPaths()), type);
            }
#endif
            string bundle_name = string.Format("{0}_{1}", projectName, bundleName);
            AssetBundle assetBundle = LoadAssetBundle(projectName, bundle_name);

            if (assetBundle != null) {
                UnityEngine.Object[] objs = assetBundle.LoadAllAssets(type);
                AddAssetCache(projectName,bundleName, objs);
                return objs;
            }

            return  null;
        }

        /// <summary>
        /// 异步加载某个类型的所有资源
        /// </summary>
        internal static LoadAssetsRequest LoadAllAssetsAsyncInternal<T>(string projectName, string bundleName) where T : UnityEngine.Object
        {
            LoadAssetsRequest request = new LoadAssetsRequest();
            //CoroutineStarter.Start(request.LoadAllAssetsAsync<T>(projectName, bundleName));
            CoroutineStarter.Start(request.LoadAllAssetsAsync(projectName, bundleName, typeof(T)));
            return request;
        }
        /// <summary>
        /// 异步加载某个类型的所有资源
        /// </summary>
        internal static LoadAssetsRequest LoadAllAssetsAsyncInternal(string projectName, string bundleName, Type type)
        {
            LoadAssetsRequest request = new LoadAssetsRequest();
            CoroutineStarter.Start(request.LoadAllAssetsAsync(projectName, bundleName, type));
            return request;
        }

        internal static void LoadSceneInternal(string projectName, string bundleName, string sceneName, LoadSceneMode mode)
        {

            if (string.IsNullOrEmpty(projectName) || string.IsNullOrEmpty(bundleName))
            {
                return;
            }

#if UNITY_EDITOR
            if (GetProfile(projectName).loadMode == LoadMode.Assets)
            {
                XFABAssetBundle bundle = GetXFABAssetBundle(projectName, bundleName);
                //bundle.GetAssetPathByFileName(assetName);
                EditorSceneManager.LoadSceneInPlayMode(bundle.GetAssetPathByFileName(sceneName,typeof(UnityEngine.SceneManagement.Scene)), new LoadSceneParameters() { loadSceneMode = mode });
                return;
            }
#endif
            string bundle_name = string.Format("{0}_{1}", projectName, bundleName);
            // 加载场景所在 AB
            LoadAssetBundle(projectName, bundle_name);
            // 加载场景
            SceneManager.LoadScene(sceneName, mode);

            Scene scene = SceneManager.GetSceneByName(sceneName);
            if (scene.IsValid()) {
                // 添加缓存
                SceneObject s = new SceneObject(scene.name, scene.GetHashCode());
                AddAssetCache(projectName, bundleName,s);
            }
        }

        // UnityEditor.SceneAsset
        internal static AsyncOperation LoadSceneAsyncInternal(string projectName, string bundleName, string sceneName, LoadSceneMode mode)
        {

            if (string.IsNullOrEmpty(projectName) || string.IsNullOrEmpty(bundleName) || string.IsNullOrEmpty(sceneName))
            {
                return null;
            }

#if UNITY_EDITOR
            if (GetProfile(projectName).loadMode == LoadMode.Assets)
            {
                XFABAssetBundle bundle = GetXFABAssetBundle(projectName, bundleName);
                //bundle.GetAssetPathByFileName(assetName);
                return EditorSceneManager.LoadSceneAsyncInPlayMode(bundle.GetAssetPathByFileName(sceneName, typeof(SceneObject)), new LoadSceneParameters() { loadSceneMode = mode });
            }
#endif
            string bundle_name = string.Format("{0}_{1}", projectName, bundleName);
            LoadAssetBundle(projectName, bundle_name);
 
            AsyncOperation async = SceneManager.LoadSceneAsync(sceneName, mode);
            async.completed += (a) =>
            { 
                Scene scene = SceneManager.GetSceneByName(sceneName);
                if (scene.IsValid())
                {
                    // 添加缓存
                    SceneObject s = new SceneObject(scene.name, scene.GetHashCode());
                    AddAssetCache(projectName, bundleName, s);
                }
            };
            return async;
        }

        #endregion

        #region 卸载资源

        /// <summary>
        /// 保存已经加载的资源
        /// </summary>
        internal static void AddAssetCache(string projectName,string bundleName, object asset) {

            if (asset == null) return; 

            if (!bundle_assets.ContainsKey(projectName))
                bundle_assets.Add(projectName,  new Dictionary<string, Dictionary<int,object>>());

            if (!bundle_assets[projectName].ContainsKey(bundleName))
                bundle_assets[projectName].Add(bundleName, new Dictionary<int, object>());

            if(!bundle_assets[projectName][bundleName].ContainsKey(asset.GetHashCode()))
                bundle_assets[projectName][bundleName].Add(asset.GetHashCode(),asset);

            int asset_hash = asset.GetHashCode();
            if (!asset_hash_project_name.ContainsKey(asset_hash)) 
                asset_hash_project_name.Add(asset_hash, projectName);
        }

        internal static void AddAssetCache(string projectName, string bundleName, UnityEngine.Object[] assets) {
            foreach (var asset in assets)
                AddAssetCache(projectName, bundleName, asset);
        }

        internal static void AddSubAssets(string projectName, string bundleName, string mainAssetName, UnityEngine.Object[] assets) {
            
            if (!bundle_sub_assets.ContainsKey(projectName))
                bundle_sub_assets.Add(projectName, new Dictionary<string, Dictionary<string, SubObjects>>());

            if (!bundle_sub_assets[projectName].ContainsKey(bundleName))
                bundle_sub_assets[projectName].Add(bundleName, new Dictionary<string, SubObjects>());

            if (!bundle_sub_assets[projectName][bundleName].ContainsKey(mainAssetName))
                bundle_sub_assets[projectName][bundleName].Add(mainAssetName,new SubObjects());

            bundle_sub_assets[projectName][bundleName][mainAssetName].Add(assets);
        }

        [Obsolete("该方法已过时,请使用UnloadAsset(object asset)!代替!",true)]
        public static void UnloadAsset(string projectName, object asset) { 
        
        }

        /// <summary>
        /// 卸载资源(如果卸载场景请传入UnityEngine.SceneManagement.Scene)
        /// </summary>
        public static void UnloadAsset(object asset)
        {
            
            if (asset == null) return;

            string assetName = string.Empty;

            if (asset is UnityEngine.Object)
            {
                UnityEngine.Object obj = asset as UnityEngine.Object;
                if (obj != null)
                    assetName = obj.name;
            }
            else 
            {
                try
                {
                    // 通过反射获取
                    System.Reflection.PropertyInfo info = asset.GetType().GetProperty("name");
                    if (info != null && info.GetValue(asset) != null)
                        assetName = info.GetValue(asset).ToString();
                }
                catch (Exception)
                {
                    return;
                } 
            }
             
            if (string.IsNullOrEmpty(assetName)) return;

            int asset_hash_code = asset.GetHashCode();
            if (!asset_hash_project_name.ContainsKey(asset_hash_code)) return; 

            string projectName = asset_hash_project_name[asset_hash_code];

            string bundleName = GetBundleName(projectName, assetName, asset.GetType());

            if (string.IsNullOrEmpty(bundleName))
            { 
                Debug.LogErrorFormat("查询bundleName失败:projectName:{0} asset:{1} type:{2},请确认资源是否通过AssetBundleManager加载?\n<color=red>*注:如果资源是通过AssetBundleManager加载,仍然报错,请确认该资源是否为子资源!子资源不能卸载,请卸载其所在的主资源!</color>", projectName, assetName, asset.GetType().FullName);
                return;
            }

            if (bundle_assets.ContainsKey(projectName) && bundle_assets[projectName].ContainsKey(bundleName) && bundle_assets[projectName][bundleName].ContainsKey(asset.GetHashCode()))
            {
                bundle_assets[projectName][bundleName].Remove(asset_hash_code);
                if(asset_hash_project_name.ContainsKey(asset_hash_code))
                    asset_hash_project_name.Remove(asset_hash_code);

                if (bundle_assets[projectName][bundleName].Count == 0) {
                    if (IsHaveInUseAssetBundleDependence(projectName, bundleName)) return; // 如果这个assetbundle 被别人依赖 则不能卸载
                    bundle_assets[projectName].Remove(bundleName);
                    UnLoadAssetBundle(projectName, bundleName);
                }
            }
        }

        
        /// <summary>
        /// 一个assetbundle 是否被正在使用的assetbundle 依赖
        /// </summary>
        /// <param name="bundleName"></param>
        internal static bool IsHaveInUseAssetBundleDependence(string projectName,string bundleName) {
            string bundle_name = string.Format("{0}_{1}", projectName.ToLower(), bundleName);
            foreach (var item in assetBundles[projectName].Keys)
            {
                if (item == bundle_name) continue;
                if (GetAssetBundleDependences(projectName, item).Contains(bundle_name))
                    return true; // 说明有AssetBundle依赖当前的这个bundle,不能卸载 
            }
            return false;
        }

        /// <summary>
        /// 自动清理资源的协程
        /// </summary>
        private static IEnumerator AutomaticResourceClearing() {

            Dictionary<string, List<string>> need_remove_bundles = new Dictionary<string, List<string>>();
            Dictionary<string, List<string>> not_unload_bundles = new Dictionary<string, List<string>>();  // 保存本应该卸载 但是因为被别人引用没有卸载的bundle

            while (true) {

                need_remove_bundles.Clear();
                not_unload_bundles.Clear();

                // 收集数据
                foreach (var projectName in BundleLoadedAssets.Keys)
                {
                    foreach (var bundleName in BundleLoadedAssets[projectName].Keys)
                    {
                        if (BundleLoadedAssets[projectName][bundleName].Count == 0)
                        {
                            if (!not_unload_bundles.ContainsKey(projectName))
                                not_unload_bundles.Add(projectName, new List<string>());
                            not_unload_bundles[projectName].Add(bundleName);
                        }
                    }
                }

                foreach (var projectName in not_unload_bundles.Keys)
                {
                    yield return null;
                    foreach (var bundleName in not_unload_bundles[projectName])
                    {
                        yield return null;
                        // 说明资源已经卸载 但是bundle被别人引用 导致bundle没有卸载 所以这里判断一下 还有没有人引用 如果没有就卸掉
                        if (!IsHaveInUseAssetBundleDependence(projectName, bundleName))
                        {
                            UnLoadAssetBundle(projectName, bundleName);

                            if (!need_remove_bundles.ContainsKey(projectName))
                                need_remove_bundles.Add(projectName, new List<string>());
                            need_remove_bundles[projectName].Add(bundleName);
                        }
                    }
                }

                foreach (var projectName in need_remove_bundles.Keys)
                {
                    foreach (var bundleName in need_remove_bundles[projectName])
                    {
                        if (BundleLoadedAssets.ContainsKey(projectName) && BundleLoadedAssets[projectName].ContainsKey(bundleName)) {
                            BundleLoadedAssets[projectName].Remove(bundleName);
                        }
                    }
                }

                yield return new WaitForSeconds(AutomaticResourceClearingTime); // 10 分钟检测一次
            }   
        }

        #endregion

        #region 方法

        /// <summary>
        /// 判断某个模块的资源是否内置在安装包中
        /// </summary>
        /// <param name="projectName">资源模块名</param>
        /// <returns></returns>
        public static IsHaveBuiltInResRequest IsHaveBuiltInRes(string projectName )
        {
            IsHaveBuiltInResRequest request = new IsHaveBuiltInResRequest();
            CoroutineStarter.Start(request.IsHaveBuiltInRes(projectName));
            return request;
        }

        /// <summary>
        /// 判断本地是否有某个模块资源
        /// </summary>
        /// <param name="projectName">资源项目名称</param>
        /// <returns></returns>
        public static bool IsHaveResOnLocal(string projectName)
        {
            // 之前是判断有没有 XFABConst.project_build_info 这个文件
            // 后来修改之后XFABConst.project_build_info这个文件在客户端不一定存在，可有可无，所以修改为判断有没有XFABConst.asset_bundle_mapping
            string path = XFABTools.LocalResPath(projectName, XFABConst.asset_bundle_mapping);

            bool isHaveBuildIn = false;

            if (XFABTools.StreamingAssetsReadable()) { 
                string buildin = string.Format("{0}/{1}", XFABTools.BuildInDataPath(projectName), XFABConst.project_build_info);
                isHaveBuildIn = File.Exists(buildin);
            }

            // 数据目录或者内置目录只要一个目录有数据就算本地有数据，
            // 如果内置目录有数据，说明内置一定是可读的，如果不可读会一直返回false

            return File.Exists(path) || isHaveBuildIn;
        }

        /// <summary>
        /// 自定义获取版本的接口 
        /// </summary>
        /// <param name="projectVersion"></param>
        public static void SetGetProjectVersion<T>() where T : IGetProjectVersion
        { 
            getProjectVersion = typeof(T);
        }

        public static void SetGetProjectVersion(Type type) {

            if( XFABTools.IsImpInterface(type,typeof(IGetProjectVersion)) == false)
                throw new Exception("类型必须实现 IGetProjectVersion 接口!");
            
            getProjectVersion = type; 
        }

        [Obsolete("改方法已经过时,将会在未来的版本中移除,请使用方法 SetGetProjectVersion<T>() 或者 SetGetProjectVersion(Type type)代替!", false)]
        public static void SetGetProjectVersion(IGetProjectVersion getProjectVersion) {
            if(getProjectVersion == null)
                throw new Exception("getProjectVersion is null!");
            SetGetProjectVersion(getProjectVersion.GetType());
        }

        /// <summary>
        /// 设置服务端文件路径接口
        /// </summary>
        /// <param name="serverFilePath"></param>
        public static void SetServerFilePath(IServerFilePath serverFilePath) {
            ServerFilePath = serverFilePath;
        }

        /// <summary>
        /// 获取某个项目的依赖项目
        /// </summary>
        /// <param name="projectName">项目名</param>
        public static GetProjectDependenciesRequest GetProjectDependencies(string projectName)
        {
            GetProjectDependenciesRequest request = new GetProjectDependenciesRequest();
            CoroutineStarter.Start(request.GetProjectDependencies(projectName));
            return request;
        }

        /// <summary>
        /// 获取AssetBundle的路径
        /// </summary>
        /// <param name="projectName">项目名</param>
        /// <param name="bundleName">AssetBundle名称 需要加后缀!</param>
        /// <returns></returns>
        internal static string GetAssetBundlePath(string projectName, string bundleName, string suffix = "")
        {
            // 优先从数据拿数据
            string bundle_path = XFABTools.LocalResPath(projectName, string.Format("{0}{1}", bundleName, suffix));

            // 判断是否有文件
            if (!File.Exists(bundle_path)) {
                string build_in_path = string.Format("{0}/{1}/{2}", Application.streamingAssetsPath, projectName, XFABTools.GetCurrentPlatformName());
                // 如果没有这个文件
                bundle_path = string.Format("{0}/{1}", build_in_path, string.Format("{0}{1}", bundleName, suffix)); 
            }

#if XFABMANAGER_LOG_OPEN_TESTING
            Debug.LogFormat("GetAssetBundlePath: projectName:{0} bundleName:{1} suffix:{2} path:{3}",projectName,bundleName,suffix,bundle_path);
#endif

            return bundle_path;
        }
        /// <summary>
        /// 获取AssetBundle的依赖
        /// </summary>
        /// <param name="projectName">项目名</param>
        /// <param name="bundleName">bundle名 不需要加后缀</param>
        /// <returns></returns>
        public static string[] GetAssetBundleDependences(string projectName, string bundleName)
        {
            string dependenceName = XFABTools.GetCurrentPlatformName();

            if (!dependenceProjectName.Equals(projectName) && dependenceBundle != null)
            {
                dependenceBundle.Unload(true);
                dependenceBundle = null;
            }

            if (dependenceBundle == null)
            {
                string path = GetAssetBundlePath(projectName, dependenceName);
                dependenceBundle = LoadAssetBundleFromFilePath(path,projectName);
                dependenceProjectName = projectName;
            }

            // 加载依赖的AssetBundle
            AssetBundleManifest manifest = dependenceBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");

            if (manifest == null) {
                Debug.LogException(new Exception(string.Format("加载AssetBundleManifest失败:{0}",projectName)));
                return new string[0];
            }

            string[] dependences = manifest.GetAllDependencies(string.Format("{0}{1}", bundleName, GetAssetBundleSuffix(projectName)));
            for (int i = 0; i < dependences.Length; i++)
            {
                dependences[i] = Path.GetFileNameWithoutExtension(dependences[i]);
            }
            return dependences;
        }

        /// <summary>
        /// 执行一个异步请求,如果该异步请求已经在执行中,则返回执行中的请求对象,避免重复执行同一请求,该异步请求必须继承CustomYieldInstruction
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="execute"></param>
        /// <returns></returns>
        public static T ExecuteOnlyOnceAtATime<T>(string key, AsyncOperationDelegate execute) where T : CustomYieldInstruction
        {
            bool enable = true;
#if UNITY_EDITOR
            if (!Application.isPlaying) enable = false;
#endif 
            if (enable == false) return null;

            if (async_cache.ContainsKey(key))
            {
                if(async_cache[key].keepWaiting) 
                    return async_cache[key] as T;
                else
                    async_cache.Remove(key);
            }

            CustomYieldInstruction operation = execute?.Invoke();

            async_cache.Add(key, operation);

            CoroutineStarter.Start(WaitCustomYieldInstruction(operation,()=> {
                async_cache.Remove(key);
            }));

            return operation as T;
        }

        private static IEnumerator WaitCustomYieldInstruction(CustomYieldInstruction operation,Action onFinsh) {
            yield return operation;
            onFinsh?.Invoke();
        }

        /// <summary>
        /// 获取某一个模块的所有AssetBundle文件大小 单位:字节
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public static long GetAssetBundleFilesSize(string projectName) {
            return FileTools.GetDirectorySize(XFABTools.DataPath(projectName));
        }

        /// <summary>
        /// 删除某一个模块的所有AssetBundle文件,文件删除后资源将无法加载，若想继续使用请重新准备资源(释放或下载等)
        /// </summary>
        /// <param name="projectName"></param>
        public static void DeleteAssetBundleFiles(string projectName) {

            // 如果当前平台的StreamingAssets目录可以写入说明数据目录和内置的目录是同一个
            // 并且当前的更新模式为读取本地资源，则此时资源是内置的，并且不能更新，所以不能清空资源，如果清掉资源就缺失了
            if (XFABTools.StreamingAssetsWritable() && GetProfile(projectName).updateModel == UpdateMode.LOCAL) 
                return;

            try
            {
                string dir = XFABTools.DataPath(projectName);
                if(Directory.Exists(dir))
                    Directory.Delete(dir, true);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 手动设置某一个项目的更新地址 如果projectName为空,则对所有项目的有效,如果不为空仅对指定项目有效!
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="projectName">项目</param>
        public static void SetProjectUpdateUrl(string url, string projectName = "")
        {  
            SetProfileUrl(url, projectName); 
        }

        private static void SetProfileUrl(string url, string projectName = "") {
            foreach (var item in profiles)
            {
                if (item.ProjectName.Equals(projectName) || string.IsNullOrEmpty(projectName))
                {
                    item.url = url;
                }
            }
        }


#if UNITY_EDITOR
 
        /// <summary>
        /// 获取 XFABAssetBundle( 编辑器数据 )
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        private static XFABAssetBundle GetXFABAssetBundle(string projectName, string bundleName)
        {
            if (string.IsNullOrEmpty(projectName) || string.IsNullOrEmpty(bundleName) ) {
                return null;
            }
            if (GetProfile(projectName).loadMode == LoadMode.Assets)
            {
                XFABProject project = XFABProjectManager.Instance.GetProject(projectName);
                if (project != null)
                {
                    XFABAssetBundle bundle = project.GetAssetBundle(bundleName);
                    if (bundle != null)
                    {
                        return bundle;
                    }
                    else
                    {
                        Debug.LogErrorFormat("LoadAsset error ! XFABAssetBundle:{0} is null!", bundleName);
                    }
                }
                else
                {
                    Debug.LogErrorFormat("LoadAsset error ! 未查询到项目配置 :{0}! 如果是从AssetBundle文件中加载资源,请把 LoadMode 设置为 AssetBundle模式!", projectName);
                }

                return null;
            }
            return null;
        }


        /// <summary>
        /// 从Asset目录加载资源
        /// </summary>
        /// <param name="asset_paths"></param>
        /// <returns></returns>
        private static UnityEngine.Object[] LoadAssetsFromAssets(string[] asset_paths)
        {
            UnityEngine.Object[] objects = new UnityEngine.Object[asset_paths.Length];
            if (asset_paths != null)
            {
                for (int i = 0; i < asset_paths.Length; i++)
                {
                    objects[i] = LoadAssetFromEditor<UnityEngine.Object>(asset_paths[i]);
                }
            }
            return objects;
        }

        private static T LoadAssetFromEditor<T>(string asset_path) where T :UnityEngine.Object {
            if (string.IsNullOrEmpty(asset_path)) return null;
            return LoadAssetFromEditor(asset_path,typeof(T)) as T;
        }

        private static UnityEngine.Object LoadAssetFromEditor(string asset_path,Type type){
            if (string.IsNullOrEmpty(asset_path)) return null;

            string assetName = AssetBundleTools.GetAssetNameWithType(asset_path,type);

            if (EditorAssets.ContainsKey(assetName)) 
                return EditorAssets[assetName];

            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(asset_path,type);
            EditorAssets.Add(assetName, obj);
            return obj;
        }

        private static UnityEngine.Object[] LoadAllAssetFromEditor(string asset_path ) {

            if (string.IsNullOrEmpty(asset_path)) return null;

            if(EditorAllAssets.ContainsKey(asset_path))
                return EditorAllAssets[asset_path];

            UnityEngine.Object[] objs = AssetDatabase.LoadAllAssetsAtPath(asset_path);
            EditorAllAssets.Add(asset_path, objs);
            return objs ;
        }

#endif
        /// <summary>
        /// 数组类型转换
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objects"></param>
        /// <returns></returns>
        private static T[] ArrayCast<T>(UnityEngine.Object[] objects) where T : UnityEngine.Object
        {
            if (objects == null)
            {
                throw new Exception("objects is null!");
            }
            List<T> list = new List<T>(objects.Length);
            for (int i = 0; i < objects.Length; i++)
            {
                T t = objects[i] as T;
                if (t != null)
                {
                    list.Add(t);
                }
            }
            return list.ToArray();
        }

        private static UnityEngine.Object[] ArrayCast(UnityEngine.Object[] objects, Type type)
        {
            if (objects == null)
            {
                throw new Exception("objects is null!");
            }
            List<UnityEngine.Object> list = new List<UnityEngine.Object>(objects.Length);

            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i].GetType() == type)
                {
                    list.Add(objects[i]);
                }
            }

            return list.ToArray();
        }

        /// <summary>
        /// 获取某一个模块的配置信息 (如果没有单独设置则返回默认配置)
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public static Profile GetProfile(string projectName = "") {
            
            //if (!isInited) throw new Exception("AssetBundleManager 没有初始化!请调用 AssetBundleManager.InitializeAsync() 进行初始化!");

            if (string.IsNullOrEmpty(projectName))
            {
                return profiles.Where(x => x.name.Equals("Default")).FirstOrDefault();
            }
            else
            {
                foreach (var item in profiles)
                {
                    if (item.ProjectName.Equals(projectName))
                    {
                        return item;
                    }
                }
            }

            return profiles.Where(x => x.name.Equals("Default")).FirstOrDefault();
        }

        private static IGetProjectVersion CreateProjectVersionInstance()
        {
            if (getProjectVersion == null) return null;

            if (XFABTools.IsBaseByClass(getProjectVersion, typeof(MonoBehaviour)))
            {
                GameObject obj = new GameObject(getProjectVersion.FullName);
                GameObject.DontDestroyOnLoad(obj);
                return obj.AddComponent(getProjectVersion) as IGetProjectVersion;
            }
            else
                return Activator.CreateInstance(getProjectVersion) as IGetProjectVersion;
        }

        internal static void ReleaseProjectVersionInstance(IGetProjectVersion version)
        {
            if (version != null && XFABTools.IsBaseByClass(version.GetType(), typeof(MonoBehaviour)))
            {
                MonoBehaviour mono = version as MonoBehaviour;
                GameObject.Destroy(mono.gameObject);
            }
        }

        internal static string GetBundleName(string projectName,string assetName, Type type) {
#if UNITY_EDITOR
            if (GetProfile(projectName).loadMode == LoadMode.Assets)
            {
                //Debug.Log(projectName+" getfromeditor " + assetName);
                // 从 Asset 查询 BundleName
                XFABProject project = XFABProjectManager.Instance.GetProject(projectName);
                string asset_name_key = AssetBundleTools.GetAssetNameWithType(assetName, type);

                if (project != null && project.AssetBundleNameMappingWithTypeEditorLoad.ContainsKey(asset_name_key))
                    return project.AssetBundleNameMappingWithTypeEditorLoad[asset_name_key];
                else
                    return String.Empty;
            }
#endif
            if ( !AssetBundleNameMapping.ContainsKey(projectName) ) {
                Dictionary<string, string> map = new Dictionary<string, string>();
                string mapping_path = XFABTools.LocalResPath(projectName, XFABConst.asset_bundle_mapping);
                // 如果数据目录没有这个文件 尝试从内置目录获取
                if(!File.Exists(mapping_path))
                    mapping_path = XFABTools.BuildInDataPath(projectName, XFABConst.asset_bundle_mapping);

                if (File.Exists(mapping_path) )
                {
                    string[] datas = File.ReadAllLines(mapping_path);
                    foreach (var item in datas)
                    {
                        if (string.IsNullOrEmpty(item)) continue;
                        string[] names = item.Split('|');
                        map.Add(names[0], names[1]);
                    }
                }
                AssetBundleNameMapping.Add(projectName, map);
            }
            string asset_key = AssetBundleTools.GetAssetNameWithType(assetName, type);

            if (!AssetBundleNameMapping[projectName].ContainsKey(asset_key))
                return string.Empty; 

            return AssetBundleNameMapping[projectName][asset_key];
        }

        /// <summary>
        /// 获取当前已加载的资源信息(可根据此信息查看AssetBundle的释放情况)
        /// *注: 资源加载方式必须为AssetBundle,如果是从Assets加载,数据会一直为空
        /// </summary>
        /// <returns></returns>
        public static string GetAssetBundleInfo() {

            StringBuilder info = new StringBuilder();
            info.Append("已加载的AssetBundle数量:").Append( AssetBundles.Count).Append("\n");
            foreach (var projectName in AssetBundles.Keys)
            {
                info.Append("---").Append("<color=yellow>").Append(projectName).Append("</color>").AppendLine("---");
                foreach (var abName in AssetBundles[projectName].Keys)
                {
                     
                    int count = 0;
                    
                    string realName = abName.Remove(0, projectName.Length + 1);

                    if (BundleLoadedAssets.ContainsKey(projectName) && BundleLoadedAssets[projectName].ContainsKey(realName))
                    {
                         count = BundleLoadedAssets[projectName][realName].Count; 
                    }
                    info.Append("<color=green>").Append("   ab名:").Append("</color>").Append(abName).Append("<color=green>").Append("   已加载的资源数量:").Append("</color>").AppendLine(count.ToString());

                    if (BundleLoadedAssets.ContainsKey(projectName) && BundleLoadedAssets[projectName].ContainsKey(realName)) { 
                        foreach (object asset in BundleLoadedAssets[projectName][realName].Values)
                        {
                            info.Append("<color=#FF00F6>").Append("         asset名:").Append("</color>").AppendLine(asset.ToString());
                        }

                    }

                    AssetBundle bundle = AssetBundles[projectName][abName];
                    if (dependence_assetbundles.ContainsKey(bundle)) 
                    {
                         
                        info.Append("<color=grey>");
                        info.Append("               依赖此bundle的数量:").AppendLine(dependence_assetbundles[bundle].Count.ToString());
                        foreach (var item in dependence_assetbundles[bundle])
                        {
                            info.Append("                   bundle:").AppendLine(item);
                        }
                        info.Append("</color>");
                    }

                }
            }
              
            return info.ToString();
        }

        #endregion

        #region 秘钥

        /// <summary>
        /// 设置某一个模块的资源解密密钥 
        /// *注:请务必保证密钥设置正确, 如果未加密请不要设置 否则会导致资源加载异常
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="secret"></param>
        public static void SetSecret(string projectName, string secret) {
            if (secrets.ContainsKey(projectName))
                secrets[projectName] = secret;
            else
                secrets.Add(projectName, secret);
        }

        /// <summary>
        /// 获取某个模块的资源解密密钥
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public static string GetSecret(string projectName) {
            if (secrets.ContainsKey(projectName))
                return secrets[projectName];
            return string.Empty;
        }

        #endregion

    }

}

