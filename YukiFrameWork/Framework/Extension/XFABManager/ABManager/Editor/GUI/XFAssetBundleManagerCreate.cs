﻿#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace XFABManager
{

    // 创建项目界面

    public class XFAssetBundleManagerCreate : EditorWindow
    {

        //private GUIStyle buttonStyle;

        private Vector2 scrollView;
 
        XFAssetBundleProjectPanel bundlePanel ;
 

        private void OnGUI()
        {

            scrollView = GUILayout.BeginScrollView(scrollView);

            if (bundlePanel == null)
            {
                bundlePanel = XFAssetBundleProjectPanel.CreatePanel(this);
            }

            bundlePanel.GUI(true);

            GUILayout.Space(40);
            DrawCreateButton();
            GUILayout.Space(40);

            GUILayout.EndScrollView();

            if (btn_create_click)
            {
                CreateProject();
                btn_create_click = false;
            }


        }



        #region OnGUI

        private bool btn_create_click = false;
        // 画出创建按钮
        private void DrawCreateButton() {
            GUILayout.BeginHorizontal();

            btn_create_click = GUILayout.Button("创建");
            GUILayout.EndHorizontal();
        }

        #endregion

        #region 方法

        // 创建项目
        private void CreateProject() {

            //XFABProject project1 = bundlePanel.CreateProject();
            if (bundlePanel.IsAllVerifyPass())
            {
                //创建
                string path = EditorUtility.OpenFolderPanel("请选择保存目录", Application.dataPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    Debug.Log(path);
                    if (!path.StartsWith(Application.dataPath))
                    {
                        this.ShowNotification(new GUIContent("必须保存到Assets目录下!"));
                        return;
                    }
                    path = path.Replace(Application.dataPath, "Assets");
                    Debug.Log(path);
                    XFABProject project = CreateInstance<XFABProject>();

                    //设置数据
                    project.name = bundlePanel.name;
                    project.displayName = bundlePanel.displayName;                   

                    project.suffix = bundlePanel.suffix;
                    project.version = bundlePanel.version;

                    // 创建文件
                    AssetDatabase.CreateAsset(project, string.Format("{0}/{1}.asset", path, project.name));
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    // 关闭窗口
                    this.Close();
                }
            }
            else {

                this.ShowNotification(new GUIContent(bundlePanel.GetErrorMesssage()));
            }

        }

         
        #endregion


    }

}
#endif
