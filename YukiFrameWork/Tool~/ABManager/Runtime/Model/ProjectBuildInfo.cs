using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XFABManager
{

    [Serializable]
    public struct BundleInfo {

        public static BundleInfo Empty = new BundleInfo(string.Empty,0,string.Empty);

        public string bundleName;
        public long bundleSize;
        public string md5;
         

        public BundleInfo(string bundleName,long bundleSize,string md5) {
            this.bundleName = bundleName;
            this.bundleSize = bundleSize;
            this.md5 = md5;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            BundleInfo other = (BundleInfo)obj; 
            return this.bundleName.Equals(other.bundleName) && this.bundleSize == other.bundleSize && this.md5.Equals(other.md5);
        }

    }

    [Serializable]
    public class ProjectBuildInfo
    {
        /// <summary>
        /// 项目名
        /// </summary>
        public string projectName;
        /// <summary>
        /// 显示名
        /// </summary>
        public string displayName;
        /// <summary>
        /// 依赖的项目 (所有)
        /// </summary>
        public string[] dependenceProject;
        /// <summary>
        /// AssetBundle 的信息
        /// </summary>
        public BundleInfo[] bundleInfos = new BundleInfo[0];
        /// <summary>
        /// 后缀
        /// </summary>
        public string suffix;
        /// <summary>
        /// 版本
        /// </summary>
        public string version;
        /// <summary>
        /// 更新的内容
        /// </summary>
        public string update_message;

        /// <summary>
        /// 更新的日期
        /// </summary>
        public string update_date;


        private Dictionary<string, string> _bundle_md5s = null;
        private Dictionary<string, string> bundle_md5s
        {
            get { 
                if (_bundle_md5s == null) { 
                    _bundle_md5s = new Dictionary<string, string>(); 
                    foreach (var item in bundleInfos)
                    {
                        _bundle_md5s.Add(item.bundleName, item.md5);
                    }
                } 
                return _bundle_md5s;
            }
        }

        // bundle
        private List<BundleInfo> _bundleInfos = null;
        private List<BundleInfo> mBundleInfos
        {
            get {
                if (_bundleInfos == null) {
                    _bundleInfos = new List<BundleInfo>();
                    _bundleInfos.AddRange(bundleInfos);
                }
                return _bundleInfos;
            }
        }


        public ProjectBuildInfo() { }

        public ProjectBuildInfo(string projectName)
        {
            this.projectName = projectName;
        }

        // 对比bundle文件
        public bool EqualBundleFile(string bundleName, string md5) {
            return bundle_md5s.ContainsKey(bundleName) && bundle_md5s[bundleName].Equals(md5);
        }

        public void UpdateBundleFile(string bundleName,long size , string md5) {

            BundleInfo info = BundleInfo.Empty;
            int index = 0;
            foreach (var item in mBundleInfos) {
                if (item.bundleName.Equals(bundleName)) { 
                    info = item;
                    index = mBundleInfos.IndexOf(item);
                    break;
                }
            }

            if (!info.Equals(BundleInfo.Empty)) { 
                info.md5 = md5;
                info.bundleSize = size;
                mBundleInfos[index] = info;
            }
            else
                mBundleInfos.Add(new BundleInfo(bundleName, size, md5));

            bundleInfos = mBundleInfos.ToArray();

            if (bundle_md5s.ContainsKey(bundleName))
                bundle_md5s[bundleName] = md5;
            else 
                bundle_md5s.Add(bundleName,md5);

            // 保存到本地文件
            //Save();
        }

    }
}


