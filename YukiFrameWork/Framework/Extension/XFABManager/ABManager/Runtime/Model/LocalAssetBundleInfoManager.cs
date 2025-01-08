
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace XFABManager
{
    /// <summary>
    /// 本地的AssetBundle文件信息的管理类(内部类)
    /// </summary>
    internal class LocalAssetBundleInfoManager 
    {

        #region 常量

        /// <summary>
        /// 更新或者释放资源的时候会保存bundle文件的后缀(当然后这个功能已经过时，后面可以删掉)
        /// 现在bundle的后缀的信息是直接下载下来的，跟AssetBundleNameMapping.txt 一样
        /// </summary>
        private const string SAVE_BUNDLE_SUFFIX = "SAVE_BUNDLE_SUFFIX";
        /// <summary>
        /// 存放Bundle文件的md5信息的目录
        /// </summary>
        private const string BUNDLE_FILE_MD5_INFO_DIR = "BUNDLE_FILE_MD5_INFO_DIR";

        #endregion

        #region 字段 
        private Dictionary<string, Dictionary<string, string>> bundle_md5_infos = new Dictionary<string, Dictionary<string, string>>();
        //private List<string> temp_list = new List<string>();
        private Dictionary<string,string> suffixs = new Dictionary<string, string>();
        private Dictionary<string,string> temp_dictionary = new Dictionary<string,string>();
        private Dictionary<string,string> data_paths = new Dictionary<string,string>();
        #endregion

        #region 属性

        internal static LocalAssetBundleInfoManager Instance { get; private set; }

        #endregion

        #region 方法

        [RuntimeInitializeOnLoadMethod]
        private static void Init() 
        { 
            Instance = new LocalAssetBundleInfoManager();
        }


        internal string GetAssetBundleMd5(string projectName, string bundleName) 
        {
            if (string.IsNullOrEmpty(bundleName))
                return string.Empty;
            if (bundle_md5_infos.ContainsKey(projectName) && bundle_md5_infos[projectName].ContainsKey(bundleName))
                return bundle_md5_infos[projectName][bundleName]; 
             
            // 没有读取 去读取文件
            string info_path = GetAssetBundleInfoPath(projectName, bundleName);
            string assetbundle_file_path = XFABTools.LocalResPath(projectName, bundleName); 
            string buildInPath = XFABTools.BuildInDataPath(projectName, bundleName);

            bool fileExist = File.Exists(assetbundle_file_path) || File.Exists(buildInPath);


            // 要确保当前这个assetbundle文件是存在的，如果文件不存在即使md5信息存在也没用
            if (File.Exists(info_path) && fileExist ) 
            { 
                string md5 = File.ReadAllText(info_path);
                if(!bundle_md5_infos.ContainsKey(projectName))
                    bundle_md5_infos.Add(projectName, new Dictionary<string, string>());
                bundle_md5_infos[projectName].Add(bundleName, md5);
                return md5;
            }

            return string.Empty;
        }

        internal void SetAssetBundleMd5(string projectName, string bundleName,string md5) 
        {

            // 如果说已有的md5和现有的md5一致则不需要重复操作
            if (bundle_md5_infos.ContainsKey(projectName) && bundle_md5_infos[projectName].ContainsKey(bundleName) && bundle_md5_infos[projectName][bundleName].Equals(md5))
                return;
             

            if (!bundle_md5_infos.ContainsKey(projectName))
                bundle_md5_infos.Add(projectName,new Dictionary<string, string>());

            if (!bundle_md5_infos[projectName].ContainsKey(bundleName))
                bundle_md5_infos[projectName].Add(bundleName, md5);
            else 
                bundle_md5_infos[projectName][bundleName] = md5;
             
            string info_path = GetAssetBundleInfoPath(projectName, bundleName);

            string directory = Path.GetDirectoryName(info_path);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(info_path, md5);
        }

        internal void DeleteAssetBundleMd5(string projectName,string bundleName) 
        {
            try
            {
                if (bundle_md5_infos.ContainsKey(projectName) && bundle_md5_infos[projectName].ContainsKey(bundleName))
                    bundle_md5_infos[projectName].Remove(bundleName);
                string info_path = GetAssetBundleInfoPath(projectName, bundleName);
                if (File.Exists(info_path)) File.Delete(info_path);
            }
            catch (System.Exception)
            { 
            } 
        }
         
        /// <summary>
        /// 同步文件列表(不涉及创建，只涉及删除)
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="bundleNames"></param>
        internal async void SyncBundleList(string projectName, BundleInfo[] bundleInfos)
        {
            if (bundleInfos == null || bundleInfos.Length == 0) return;
            //temp_list.Clear();
            SetDataPath(projectName);

#if XFABMANAGER_LOG_OPEN_TESTING
            UnityEngine.Debug.LogFormat("Test Application.persistentDataPath : {0}", Application.persistentDataPath);
#endif

            
 
            await Task.Run(() => 
            {
                temp_dictionary.Clear();
                foreach (BundleInfo info in bundleInfos)
                {
                    if (string.IsNullOrEmpty(info.bundleName))
                        continue;

                    if (!temp_dictionary.ContainsKey(info.bundleName))
                        temp_dictionary.Add(info.bundleName, string.Empty);

                    // 保存md5信息
                    SetAssetBundleMd5(projectName, info.bundleName, info.md5);
                }

                // 更新本地md5 如果这个列表中不包含这个文件 可以把这个md5信息删掉 
                string dir = string.Format("{0}/{1}", GetDataPath(projectName), BUNDLE_FILE_MD5_INFO_DIR);
                if (!Directory.Exists(dir)) return;
                string[] files = Directory.GetFiles(dir);

                foreach (var file in files)
                {
                    string name = Path.GetFileName(file);
                    if (!temp_dictionary.ContainsKey(name))
                        DeleteAssetBundleMd5(projectName, name);
                }
            }); 
        }
  
        private string GetAssetBundleInfoPath(string projectName,string bundleName)
        {
            return string.Format("{0}/{1}/{2}",GetDataPath(projectName), BUNDLE_FILE_MD5_INFO_DIR, bundleName);
        }
 
        internal string GetSuffix(string projectName) 
        {
            if (suffixs.ContainsKey(projectName)) 
                return suffixs[projectName];

            string suffix_path = string.Format("{0}/{1}", GetDataPath(projectName),XFABConst.bundles_suffix_info);

            if (!File.Exists(suffix_path))
                suffix_path = XFABTools.BuildInDataPath(projectName, XFABConst.bundles_suffix_info);


            if (File.Exists(suffix_path)) 
            {
                string suffix = File.ReadAllText(suffix_path);
                suffixs.Add(projectName, suffix);

#if XFABMANAGER_LOG_OPEN_TESTING
                UnityEngine.Debug.LogFormat("GetSuffix projectName:{0} suffix:{1} from file:{2}", projectName, suffix, suffix_path);
#endif

                return suffix;
            }

            // 从project_build_info中读取
            try
            {
                string project_build_info_path = XFABTools.LocalResPath(projectName, XFABConst.project_build_info);

                // ios平台，StreamingAsset没有写入权限，但是有读取权限，当内置资源时，数据目录是没有资源的，
                // 所以此时获取后缀信息应该是去内置目录获取
                if (!File.Exists(project_build_info_path))
                    project_build_info_path = XFABTools.BuildInDataPath(projectName, XFABConst.project_build_info);

                string content = File.ReadAllText(project_build_info_path);
                ProjectBuildInfo info = JsonConvert.DeserializeObject<ProjectBuildInfo>(content);
                suffixs.Add(projectName, info.suffix);

#if XFABMANAGER_LOG_OPEN_TESTING
                UnityEngine.Debug.LogFormat("GetSuffix projectName:{0} suffix:{1} from ProjectBuildInfo:{2}", projectName, info.suffix, content);
#endif

                return info.suffix;
            }
            catch (System.Exception)
            { 
            }

            // 为了兼容旧版本的资源
            string save_suffix_path = string.Format("{0}/{1}", GetDataPath(projectName), SAVE_BUNDLE_SUFFIX);
            if (File.Exists(save_suffix_path))
            {
                string suffix = File.ReadAllText(save_suffix_path);
                suffixs.Add(projectName, suffix);

#if XFABMANAGER_LOG_OPEN_TESTING
                UnityEngine.Debug.LogFormat("GetSuffix projectName:{0} suffix:{1} from save file:{2}", projectName, suffix, save_suffix_path);
#endif

                return suffix;
            }

            return string.Empty;
        }

        /// <summary>
        /// 保存AssetBundle后缀(在后面的版本中可以删除，已经过时)
        /// </summary>
        /// <param name="projectName"></param>
        /// <param name="suffix"></param>
        internal void SaveSuffix(string projectName, string suffix)
        {
            if (suffixs.ContainsKey(projectName) && suffixs[projectName].Equals(suffix)) return;

            string suffix_path = string.Format("{0}/{1}", GetDataPath(projectName),SAVE_BUNDLE_SUFFIX);
            if (!Directory.Exists(GetDataPath(projectName)))
                Directory.CreateDirectory(GetDataPath(projectName));
            File.WriteAllText(suffix_path, suffix);

            if (suffixs.ContainsKey(projectName))
                suffixs[projectName] = suffix;
            else
                suffixs.Add(projectName, suffix);
        }

        /// <summary>
        /// XFABTools.DataPath() 该方法不能在子线程中调用，否则会在Android平台会抛异常
        /// 所以加了这个方法来代替，在子线程开启之前调用SetDataPath，然后就可以正常使用了
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        internal string GetDataPath(string projectName)
        { 
            if(data_paths.ContainsKey(projectName))
                return data_paths[projectName];
            string dataPath = XFABTools.DataPath(projectName);
            data_paths.Add(projectName, dataPath);
            return dataPath;
        }

        internal void SetDataPath(string projectName) {
            if (data_paths.ContainsKey(projectName)) return;
            data_paths.Add(projectName,XFABTools.DataPath(projectName));
        }

        #endregion


    }

}

