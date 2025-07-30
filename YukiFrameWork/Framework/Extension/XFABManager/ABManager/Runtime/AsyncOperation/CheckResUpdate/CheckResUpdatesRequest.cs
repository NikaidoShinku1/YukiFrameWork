using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace XFABManager
{

    /// <summary>
    /// 检测资源 包含依赖项目
    /// </summary>
    public class CheckResUpdatesRequest : CustomAsyncOperation<CheckResUpdatesRequest>
    {
        //private CheckUpdateResult[] _results;

        public CheckUpdateResult[] results
        {
            get; protected set;
        }

        // 检测资源更新
        internal IEnumerator CheckResUpdates(string projectName) {
            
            if (string.IsNullOrEmpty(projectName))
            {
                yield return new WaitForEndOfFrame();
                Completed(string.Format("项目名不能为空!"));
                yield break;
            }

#if XFABMANAGER_LOG_OPEN_TESTING
            Debug.LogFormat("检测及其依赖资源 CheckResUpdates:{0}",projectName);
#endif

            // 如果是编辑器  并且 从 Assets 加载资源 
            // 这种情况下 是 不需要AssetBundle的 
#if UNITY_EDITOR
            if (AssetBundleManager.GetProfile(projectName).loadMode == LoadMode.Assets)
            {
                results = new CheckUpdateResult[0];
                Completed();
                yield break;
            }
#endif
         
            // 需要检测的项目
            List<string> need_check_projects = new List<string>();          
                                                       // 检测的结果 
            results = new CheckUpdateResult[need_check_projects.Count]; // 除了依赖项目还要检测自己

#if XFABMANAGER_LOG_OPEN_TESTING
            Debug.LogFormat("检测及其依赖资源 CheckResUpdates need_check_projects{0}", JsonConvert.SerializeObject(need_check_projects));
#endif

            // 检测
            for (int i = 0; i < need_check_projects.Count; i++)
            {
                CheckResUpdateRequest request = AssetBundleManager.CheckResUpdate(need_check_projects[i]);
                yield return request;
                if (!string.IsNullOrEmpty(request.error) ) 
                { 
                    Completed(string.Format("检测{0}资源出错:{1}", need_check_projects[i], request.error));
                    yield break;
                }
                results[i] = request.result;
            }
            Completed();
        }
    }


    public class CheckUpdateResult
    {
        public UpdateType updateType = UpdateType.DontNeedUpdate;
        /// <summary>
        /// 需要更新的文件总大小 单位 字节
        /// </summary>
        public long updateSize;
        /// <summary>
        /// 更新的内容
        /// </summary>
        public string message = string.Empty;
        /// <summary>
        ///  需要更新的文件列表
        /// </summary>
        [System.Obsolete("该字段已经过时,请使用need_update_bundles代替!",true)]
        public string[] need_update_files; 

        public string projectName;

        /// <summary>
        /// 需要更新的资源包信息
        /// </summary>
        public BundleInfo[] need_update_bundles = new BundleInfo[0];

        /// <summary>
        /// 当前的版本
        /// </summary>
        public string version;

        internal ProjectBuildInfo check_build_info;

        public CheckUpdateResult(string projectName)
        {
            this.projectName = projectName;
        }

        public string updateDate; // 更新日期

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("projectName:").Append(projectName).Append("\n");
            stringBuilder.Append("UpdateType:").Append(updateType).Append("\n");
            stringBuilder.Append("updateSize:").Append((double)updateSize / 1024).Append("kb").Append("\n");
            stringBuilder.Append("message:").Append(message).Append("\n");
            stringBuilder.Append("version:").Append(version).Append("\n");
            stringBuilder.Append("update_date:").Append(updateDate).Append("\n");

            stringBuilder.Append("需要更新的文件列表:").Append("\n");
            if (need_update_bundles != null)
            {

                for (int i = 0; i < need_update_bundles.Length; i++)
                {
                    stringBuilder.Append(need_update_bundles[i].bundleName).Append("\n");
                }
            }

            return stringBuilder.ToString();
        }

    }

}

