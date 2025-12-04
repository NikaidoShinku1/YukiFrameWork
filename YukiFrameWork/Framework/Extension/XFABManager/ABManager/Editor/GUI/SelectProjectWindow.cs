using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using XFABManager;

public class SelectProjectWindow : EditorWindow
{
#if UNITY_6000_2_OR_NEWER
	TreeViewState<int> projectState;
#else
    TreeViewState projectState;
#endif

    SelectProjectTreeView projectView;
    MultiColumnHeaderState projectMCHState;

    Rect area = new Rect();

    Rect buttonRect = new Rect();

    private void OnGUI()
    {

        if (projectState == null || projectView == null)
        {
#if UNITY_6000_2_OR_NEWER
            projectState = new TreeViewState<int>();
#else
            projectState = new TreeViewState();
#endif

            var headerState = SelectProjectTreeView.CreateDefaultMultiColumnHeaderState();// multiColumnTreeViewRect.width);
            if (MultiColumnHeaderState.CanOverwriteSerializedFields(projectMCHState, headerState))
                MultiColumnHeaderState.OverwriteSerializedFields(projectMCHState, headerState);
            projectMCHState = headerState;

            projectView = new SelectProjectTreeView(projectState, projectMCHState, this);
            projectView.Reload();
        }



        area.Set(0, 0, position.width, position.height - 30);

        projectView.OnGUI(area);

        buttonRect.Set(0, position.height - 30 + 3, position.width, 20);

        if (GUI.Button(buttonRect, "打包"))
        {

            if (projectView.BuildProjects.Count == 0)
            {
                this.ShowNotification(new GUIContent("未查询需要打包到资源模块,请添加后重试!"));
                return;
            }

            if (EditorUtility.DisplayDialog("一键打包", "确定打包所选项目吗？这个操作可能会比较耗时!", "确定", "取消"))
            {
                foreach (var item in projectView.BuildProjects)
                {
                    ProjectBuild.Build(item, EditorUserBuildSettings.activeBuildTarget, item.revealInFinderOnBuildFinsh);
                }
            }

        }

    }


}
#endif