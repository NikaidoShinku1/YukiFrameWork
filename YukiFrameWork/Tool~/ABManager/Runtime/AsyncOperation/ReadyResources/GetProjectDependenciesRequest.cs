using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace YukiFrameWork.XFABManager {
    public class GetProjectDependenciesRequest : CustomAsyncOperation<GetProjectDependenciesRequest>
    {
        //private AssetBundleManager bundleManager;

        private List<string> dependon_list;

        public string[] dependencies
        {
            get {
                return dependon_list.ToArray();
            }
        }

        public GetProjectDependenciesRequest() {
            dependon_list = new List<string>();
        }

        internal IEnumerator GetProjectDependencies(string projectName ) {
            
            if (string.IsNullOrEmpty(projectName))
            {
                yield return new WaitForEndOfFrame();
                Completed(string.Format("项目名不能为空!"));
                yield break;
            } 

            string project_build_info_content = null;

            if (AssetBundleManager.GetProfile(projectName).updateModel == UpdateMode.UPDATE)
            {

                GetProjectVersionRequest requestVersion = null;

                for (int i = 0; i < 3; i++)
                {
                    requestVersion = AssetBundleManager.GetProjectVersion(projectName);
                    // 获取版本
                    yield return requestVersion;
                    if (string.IsNullOrEmpty(requestVersion.error)) break;
                    yield return new WaitForSeconds(0.1f);
                }

                if (!string.IsNullOrEmpty(requestVersion.error))
                {
                    Completed(string.Format("请求{0}版本出错!:{1}", projectName, requestVersion.error));
                    yield break;
                }

                // 获取依赖项目
                GetFileFromServerRequest requestDepend = AssetBundleManager.GetFileFromServer(projectName, requestVersion.version, XFABConst.project_build_info);
                yield return requestDepend;
                if (string.IsNullOrEmpty(requestDepend.error))
                {
                    project_build_info_content = requestDepend.text;
                }
                else
                { 
                    Completed(string.Format("获取{0}依赖出错:{1} url:{2} ", projectName, requestDepend.error, requestDepend.request_url));
                    yield break;
                }
            }
            else
            {
                // 如果是非更新模式，资源只能使用内置的，所以直接从内置目录读就可以了
                string project_build_info = XFABTools.BuildInDataPath(projectName, XFABConst.project_build_info);

#if UNITY_ANDROID && !UNITY_EDITOR
                UnityWebRequest requestBuildInfo = UnityWebRequest.Get(project_build_info);
                yield return requestBuildInfo.SendWebRequest();
                if (string.IsNullOrEmpty(requestBuildInfo.error))
                    project_build_info_content = requestBuildInfo.downloadHandler.text;
                else {
                    Completed(string.Format("文件读取失败:{0} error:{1} 请确认资源是否缺失?", project_build_info,requestBuildInfo.error));
                    yield break;
                }
#else

                try
                {
                    if (File.Exists(project_build_info))
                        project_build_info_content = File.ReadAllText(project_build_info);
                }
                catch (System.Exception e)
                {
                    Completed(string.Format("文件读取失败:{0} error:{1}", project_build_info,e.ToString()));
                    yield break;
                } 
#endif
            }

            if (!string.IsNullOrEmpty(project_build_info_content))
            {
                try
                {
                    ProjectBuildInfo projectBuildInfo = JsonConvert.DeserializeObject<ProjectBuildInfo>(project_build_info_content);
                    dependon_list.AddRange(projectBuildInfo.dependenceProject);
                    Completed();
                }
                catch (System.Exception e)
                {
                    Debug.LogErrorFormat("解析json出错:{0}", project_build_info_content);
                    Debug.LogException(e);
                    Completed(string.Format("解析json出错:{0}", project_build_info_content));
                    yield break;
                }
            }
            else 
            {
                Completed(string.Format("项目信息读取失败:{0}，请检查资源是否缺失?",projectName));
            }
             
        }
        

    }
}


