using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YukiFrameWork.ABManager
{

    [Serializable]
    public class Profile
    {

        public Profile() { }

        public Profile(string name)
        {
            this.name = name;
        }

        public string name = "Default";

        /// <summary>
        /// 资源更新地址
        /// </summary>
        public string url = string.Empty;

        /// <summary>
        /// 更新模式 
        /// </summary>
        public UpdateMode updateModel = UpdateMode.LOCAL;

        /// <summary>
        /// 加载模式 默认 Assets
        /// </summary>
        public LoadMode loadMode = LoadMode.Assets;

        /// <summary>
        /// 是否使用默认的 获取项目版本
        /// </summary>
        //[Obsolete("已过时")]
        //public bool useDefaultGetProjectVersion = true;

        /// <summary>
        /// 哪个模块的配置
        /// </summary>
        public string ProjectName = string.Empty;


        /// <summary>
        /// 哪个组的配置
        /// </summary>
        public string GroupName = "Default";


    }

    public class ProfileConfig{

        public Profile[] profiles;

        public ProfileConfig() { }

        public ProfileConfig(Profile[] profiles) {
            this.profiles = profiles;
        }

    }

}
