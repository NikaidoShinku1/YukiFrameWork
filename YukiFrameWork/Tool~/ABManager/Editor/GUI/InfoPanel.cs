#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace YukiFrameWork.ABManager
{


    public class InfoPanel
    {

        private Vector2 scrollView;
        AssetBundleProjectPanel bundlePanel;

        private EditorWindow window;
        private ABProject project;

        public InfoPanel(EditorWindow window,ABProject project) {
            this.window = window;
            this.project = project;

            //ConfigContent();
        }

        

        #region 方法
 

        #endregion

        #region OnGUI

        public void OnGUI()
        {

            scrollView = GUILayout.BeginScrollView(scrollView);

            if (bundlePanel == null)
            {
                bundlePanel = AssetBundleProjectPanel.CreatePanel(window, project);
            }

            bundlePanel.OnGUI();
            //GUILayout.Space(20);
            //DrawUpdateModel();
            GUILayout.Space(40);


            DrawSaveButton();
            DrawDeleteButton();
            GUILayout.Space(40);

            GUILayout.EndScrollView();


        }

        // 画出创建按钮
        private void DrawSaveButton()
        {
            if (GUILayout.Button("保存"))
            {
                if (bundlePanel.IsAllVerifyPass())
                {
                    bundlePanel.SaveProject();
                    window.titleContent = new GUIContent(bundlePanel.name,"项目名");
                    window.ShowNotification(new GUIContent("保存成功!"));
                }
                else {
                    window.ShowNotification(new GUIContent(bundlePanel.GetErrorMesssage()));
                }
            }
        }

        // 画出删除按钮
        private void DrawDeleteButton() {
            if (GUILayout.Button("删除"))
            {
                if (EditorUtility.DisplayDialog("删除项目", string.Format("确定要删除项目:{0} 吗？",project.name), "确定", "取消")) {

                    if (ABProjectManager.Instance.IsHaveProjectDependence(project.name)) {
                        EditorUtility.DisplayDialog("删除项目", "删除失败!有其他的项目依赖此项目,请删除依赖后重试!", "ok");
                        return;
                    }
                    
                    // 删除
                    AssetDatabase.MoveAssetToTrash( AssetDatabase.GetAssetPath( project ) );
                    this.window.Close();
                    EditorUtility.DisplayDialog("删除项目", "删除成功!", "ok");
                }
            }
        }

        #endregion


    }


}
#endif