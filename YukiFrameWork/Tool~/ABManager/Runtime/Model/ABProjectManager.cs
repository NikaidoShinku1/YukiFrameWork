#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
 

namespace YukiFrameWork.ABManager
{

    /// <summary>
    /// 资源模块管理类
    /// </summary>
    public class ABProjectManager
    {
 

        #region 变量
        // 保存所有的 资源项目
        private List<ABProject> projects;

        private static ABProjectManager _instance;

        private double lastRefreshTime;

        private string[] allAssets;

        #endregion

        #region 属性

        /// <summary>
        /// 单例
        /// </summary>
        public static ABProjectManager Instance
        {

            get
            {
                if (_instance == null)
                {
                    _instance = new ABProjectManager();
                }

                return _instance;
            }

        }

        /// <summary>
        /// 所有资源模块集合
        /// </summary>
        public List<ABProject> Projects
        {
            get { return projects; }
        } 
        #endregion

        #region 方法


        // 私有构造函数
        private ABProjectManager()
        {

            //Debug.Log("ABManager Init!");

            InitProjects();

            //EditorApplication.update += Update;

        }

        // 初始化项目
        private void InitProjects()
        {
            projects = new List<ABProject>();
            // 刷新项目
            RefreshProjects();

        }

        /// <summary>
        /// 刷新项目列表
        /// </summary>
        public void RefreshProjects()
        {
            //Debug.Log(" RefreshProjects ");
            if (projects == null)
            {
                return;
            }

            if (EditorApplication.timeSinceStartup - lastRefreshTime < 0.1f) {
                return;
            }


            projects.Clear();
            // 读项目配置文件
            allAssets = AssetDatabase.FindAssets("t:ABProject" );

            for (int i = 0; i < allAssets.Length; i++)
            {
                ABProject project = AssetDatabase.LoadAssetAtPath<ABProject>(AssetDatabase.GUIDToAssetPath(allAssets[i]));
                if (project != null)
                {
                    AddProject(project);
                }
            }

            lastRefreshTime = EditorApplication.timeSinceStartup;
        }

        /// <summary>
        /// 判断是否包含某个项目
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsContainProject(string name) {

            if (projects != null) {

                for (int i = 0; i < projects.Count; i++)
                {
                    if ( projects[i].name == name ) {
                        return true;
                    }
                }
            }

            return false;
        }
 
        // 添加项目
        private void AddProject(ABProject project) {

            if ( project == null ) 
            {
                Debug.LogError("不能添加空项目!");
                return;
            }

            if (!IsContainProject(project.name))
            {
                projects.Add(project);
            }
            else {
                Debug.LogError(string.Format("添加项目{0}失败,已存在!", project.name));
            }
        }


        /// <summary>
        /// 查询项目
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public ABProject GetProject(string name) 
        {

            if (string.IsNullOrEmpty(name)) {
                return null;
            }

            ABProject project = null;

            for (int i = 0; i < projects.Count; i++)
            {
                if ( name.Equals( projects[i].name ) ) {
                    project = projects[i];
                }
            }

            return project;
        }

        /// <summary>
        /// 是否有项目依赖这个项目
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsHaveProjectDependence(string name) {

            for (int i = 0; i < projects.Count; i++)
            {
                if ( !projects[i].name .Equals( name ) ) {
                    if (projects[i].IsDependenceProject(name)) {
                        return true;
                    }
                }
            }

            return false;
        }
 
#endregion

    }


}

#endif