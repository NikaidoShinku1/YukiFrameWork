using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace XFABManager {
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


                    try
                    {
                        ProjectBuildInfo buildInfo = JsonConvert.DeserializeObject<ProjectBuildInfo>(project_build_info_content);
                        dependon_list.AddRange(buildInfo.dependenceProject); 
                        Completed();
                    }
                    catch (Exception e)
                    {
                        Completed(string.Format("json 解析失败:{0} error:{1}", project_build_info_content, e.ToString()));
                        yield break;
                    }
                }
                else
                {
                    Completed(string.Format("获取{0}依赖出错:{1} url:{2} ", projectName, requestDepend.error, requestDepend.request_url));
                    yield break;
                }
            }
            else
            {

                // 读取内置文件的信息
                ReadBuildInProjectBuildInfoRequest request_build_in_info = ReadBuildInProjectBuildInfoRequest.BuildInProjectBuildInfo(projectName);
                yield return request_build_in_info;
                if (!string.IsNullOrEmpty(request_build_in_info.error))
                {
                    Completed(request_build_in_info.error);
                    yield break;
                }
                ProjectBuildInfo buildInfo = request_build_in_info.ProjectBuildInfo;

                dependon_list.AddRange(buildInfo.dependenceProject);

                Completed();
            }
        } 
    }
}


