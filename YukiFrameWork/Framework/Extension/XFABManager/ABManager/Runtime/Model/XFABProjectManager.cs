#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.SearchService;

namespace XFABManager
{

    /// <summary>
    /// 资源模块管理类
    /// </summary>
    public class XFABProjectManager
    {
 

        #region 变量
        // 保存所有的 资源项目
        private List<XFABProject> projects;

        private static XFABProjectManager _instance;
        private Dictionary<string,XFABProject> projects_dic = new Dictionary<string,XFABProject>();
        private double lastRefreshTime;

        private string[] allAssets;

        #endregion

        #region 属性

        /// <summary>
        /// 单例
        /// </summary>
        public static XFABProjectManager Instance
        {

            get
            {
                if (_instance == null)
                {
                    _instance = new XFABProjectManager();
                }

                return _instance;
            }

        }

        /// <summary>
        /// 所有资源模块集合
        /// </summary>
        public List<XFABProject> Projects
        {
            get
            {
                if (projects == null || projects.Count == 0) 
                {
                    if(projects == null)
                        projects = new List<XFABProject>();
                     
                    // 查询 
                    string[] assets = AssetDatabase.FindAssets(string.Format("t:{0}",typeof(XFABProject).FullName));

                    for (int i = 0; i < assets.Length; i++)
                    {
                        XFABProject project = AssetDatabase.LoadAssetAtPath<XFABProject>(AssetDatabase.GUIDToAssetPath(assets[i]));
                        if (project != null)
                        {
                            projects.Add( project); 
                        }
                    }
                }

                return projects; 
            }
        } 
        
        private Dictionary<string, XFABProject> ProjectsDic 
        {
            get 
            {
                 
                if (projects_dic == null || projects_dic.Count == 0)
                {  
                    if (projects_dic == null)
                        projects_dic = new Dictionary<string, XFABProject>();

                    foreach (var item in Projects)
                    {

                        if (projects_dic.ContainsKey(item.name))
                        {
                            Debug.LogError(string.Format("资源模块名称:{0}重复!", item.name));
                            continue;
                        }

                        projects_dic.Add(item.name, item); 
                    }
                }
                 
                return projects_dic;
            }
        }
        #endregion

        #region 方法


        // 私有构造函数
        private XFABProjectManager()
        {

            //Debug.Log("XFABManager Init!");

            InitProjects();

            //EditorApplication.update += Update;

        }

        // 初始化项目
        private void InitProjects()
        {
            projects = new List<XFABProject>();
            // 刷新项目
            RefreshProjects();

        }

        /// <summary>
        /// 刷新项目列表
        /// </summary>
        public void RefreshProjects()
        {
            projects?.Clear();
            projects_dic?.Clear();
            // 读项目配置文件
        }

        /// <summary>
        /// 判断是否包含某个项目
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsContainProject(string name) 
        {  
            return ProjectsDic != null && ProjectsDic.ContainsKey(name);
        }

        /// <summary>
        /// 查询项目
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public XFABProject GetProject(string name) 
        {

            if(ProjectsDic.ContainsKey(name))
                return ProjectsDic[name];

            return null;

        }
#endregion

    }


}

#endif