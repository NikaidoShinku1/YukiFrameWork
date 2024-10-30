#if UNITY_EDITOR
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using YukiFrameWork;


namespace XFABManager
{

    public class XFAssetBundleManagerProjects 
    {

        #region 常量
        public const float GRID_WIDTH = 150;        // 菜单界面 格子的宽
        public const float GRID_HEIGHT = 180;       // 菜单界面 格子的高
        public const float SPACE_X = 20;           // 菜单界面 格子X的间距
        public const float SPACE_Y = 20;            // 菜单界面 格子Y的间距
        public const float MARGIN_TOP = 20;         // 菜单界面 格子上边的间距
        public const float MARGIN_LEFT = 20;        // 菜单界面 格子左边的间距

        #endregion

        #region 变量

        //private int row_grid_count = 1;     // 每行显示个子的数量 默认是1

        private Vector2 scrollPosition;         // Project Grid 滚动的位置

        private GUIContent buttonContent;
        private GUIStyle buttonStyle;

        private Texture refreshTexture;

        private XFAssetBundleProjectMain mainWindow;

        private GUIContent profileContent;
        private GUIContent toolsContent;
        private GUIContent showContent;

        private Rect projectsRect;
        private Dictionary<ProjectsShowMode, BaseShowProjects> showProjects = new Dictionary<ProjectsShowMode, BaseShowProjects>();

        #endregion


        #region 生命周期

        void Awake()
        {
            refreshTexture = EditorGUIUtility.FindTexture("Refresh");
            InitShowProjects();
        }

        void InitShowProjects() {


            showProjects.Add( ProjectsShowMode.Grid,new GridShowProjects() );
            showProjects.Add(ProjectsShowMode.List, new ListShowProjects());
        }

        [OnInspectorGUI]
        void OnGUI()
        {         
            if (buttonStyle == null)
            {
                ConfigStyle();
            }
 
            GUILayout.BeginHorizontal();

            if (this.profileContent == null) {
                this.profileContent = new GUIContent(string.Format("Profile:{0}", XFABManagerSettings.Instances.CurrentGroup));
            }

            profileContent.text = string.Format("Profile:{0}", XFABManagerSettings.Instances.CurrentGroup);          
            Rect r = GUILayoutUtility.GetRect(profileContent, EditorStyles.toolbarDropDown,GUILayout.Width(150));

            if ( EditorGUI.DropdownButton(r,profileContent, FocusType.Passive, EditorStyles.toolbarDropDown ) ) {
                GenericMenu menu = new GenericMenu(); 
                List<string> groups = XFABManagerSettings.Instances.Groups;
                for (int i = 0; i < groups.Count; i++)
                {
                    menu.AddItem(new GUIContent(groups[i]), i == XFABManagerSettings.Instances.SelectIndex,(index)=> 
                    {
                        XFABManagerSettings.Instances.SetTargetGroup((int)index);
                    }, i);
                }

                menu.AddSeparator(string.Empty);       
             
                menu.DropDown(r);
            }

            if (this.toolsContent == null) {
                this.toolsContent = new GUIContent("Tools");
            }

            //toolsContent = new GUIContent("Tools");
            Rect rMode = GUILayoutUtility.GetRect(toolsContent, EditorStyles.toolbarDropDown,GUILayout.Width(80));
            if (EditorGUI.DropdownButton(rMode, toolsContent, FocusType.Passive, EditorStyles.toolbarDropDown))
            {
                var menu = new GenericMenu();

                menu.AddItem(new GUIContent("Refresh"), false, () => {
                    XFABProjectManager.Instance.RefreshProjects();
                    FrameWorkDisignWindow.Instance.ShowNotification(new GUIContent("刷新成功!"));
                });
                menu.AddItem(new GUIContent("Package All"), false, () => {
                    BuildAll();
                });
                //menu.AddItem(new GUIContent("整理资源"), false, () => {
                //    Debug.Log("整理资源!");
                //    //EditorWindow.CreateInstance<OrganizeResourcesPanel>();
                //    EditorWindow.GetWindow<OrganizeResourcesPanel>().Show();
                //});

                
                menu.DropDown(rMode);
            }

            if (this.showContent == null) {
                this.showContent = new GUIContent("显示模式");
            }
            Rect showMode = GUILayoutUtility.GetRect(showContent, EditorStyles.toolbarDropDown, GUILayout.Width(100));
            if (EditorGUI.DropdownButton(showMode, showContent, FocusType.Passive, EditorStyles.toolbarDropDown))
            {
                var menu = new GenericMenu();

                menu.AddItem(new GUIContent("格子"), XFABManagerSettings.Instances.ShowMode == ProjectsShowMode.Grid , () => {
                    //XFABProjectManager.Instance.RefreshProjects();
                    //this.ShowNotification(new GUIContent("刷新成功!"));
                    XFABManagerSettings.Instances.ShowMode = ProjectsShowMode.Grid;
                    XFABManagerSettings.Instances.Save();
                });
                menu.AddItem(new GUIContent("列表"), XFABManagerSettings.Instances.ShowMode == ProjectsShowMode.List, () => {
                    //BuildAll();
                    XFABManagerSettings.Instances.ShowMode = ProjectsShowMode.List;
                    XFABManagerSettings.Instances.Save();
                });
                menu.DropDown(showMode);
            }

            GUILayout.EndHorizontal();

            if ( showProjects.Count == 0 ) {
                InitShowProjects();
            }

            if (showProjects.ContainsKey( XFABManagerSettings.Instances.ShowMode)) {
                projectsRect.Set(0, showMode.height, FrameWorkDisignWindow.Instance.position.width, FrameWorkDisignWindow.Instance.position.height - showMode.height);
                showProjects[XFABManagerSettings.Instances.ShowMode].DrawProjects( projectsRect, FrameWorkDisignWindow.Instance);
            }
        }

       /* // 每秒10帧更新
        void OnInspectorUpdate()
        {
            //开启窗口的重绘，不然窗口信息不会刷新
            FrameWorkDisignWindow.Instance.Repaint();
        }*/

        #endregion
         
        #region 方法

        // 配置按钮样式
        public void ConfigStyle()
        {
            buttonContent = new GUIContent();
            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.richText = true;
            buttonStyle.wordWrap = true;
        }

        

        public void BuildAll() {

            SelectProjectWindow selectProject = EditorWindow.GetWindow<SelectProjectWindow>("打包列表");
            selectProject.Show(); 
        }
        private void OnFocus()
        {
            //Debug.Log("focus");
            profileContent = null;
            toolsContent = null;
            showContent = null;
        }
        #endregion


    }


}
#endif