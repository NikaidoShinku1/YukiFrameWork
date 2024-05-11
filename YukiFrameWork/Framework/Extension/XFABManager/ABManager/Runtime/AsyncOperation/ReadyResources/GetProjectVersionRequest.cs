using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace XFABManager{

    public class GetProjectVersionRequest : CustomAsyncOperation<GetProjectVersionRequest>
    {
        //private string _version;
        private IGetProjectVersion getProjectVersion;


        /// <summary>
        /// ProjectVersion
        /// </summary>
        public string version
        {
            get; protected set;
        }



        public GetProjectVersionRequest(IGetProjectVersion  getProjectVersion){
            this.getProjectVersion = getProjectVersion;
            if (getProjectVersion == null)
            {
                throw new Exception(" 接口 IGetProjectVersion is null！请设置后重试! ");
            }
        }

        internal virtual IEnumerator GetProjectVersion(string projectName , UpdateMode updateModel)
        {
            if (updateModel == UpdateMode.LOCAL)
            {
                error = "测试模式下使用内置资源,无需版本信息!";
                isCompleted = true;
                yield break;
            }

            getProjectVersion.GetProjectVersion(projectName);
            while (!getProjectVersion.isDone())
            {
                yield return null;
            }

            if (string.IsNullOrEmpty(getProjectVersion.Error()))
            {
                version = getProjectVersion.Result();
            }
            else
            {
                // 获取失败
                error = getProjectVersion.Error();
            }

            isCompleted = true;

            AssetBundleManager.ReleaseProjectVersionInstance(getProjectVersion);

        }

    }

}

