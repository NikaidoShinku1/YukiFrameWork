using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using XFABManager;

public class BaseShowProjects 
{
    protected Rect position;
    protected EditorWindow window;

    private static Dictionary<string, EditorWindow> projectWindows = new Dictionary<string, EditorWindow>();

    public virtual void DrawProjects(Rect rect,EditorWindow window) {
        position = rect;
        this.window = window;
    }
    // 打开项目 
    internal static void OpenProject(XFABProject project)
    {  
        if (projectWindows.ContainsKey(project.Title))
        {
            projectWindows[project.Title].Focus();
        }
        else
        {
            XFAssetBundleProjectMain mainWindow = EditorWindow.CreateInstance<XFAssetBundleProjectMain>();
            mainWindow.InitProject(project);
            mainWindow.Show();

            mainWindow.onDestroy += () =>
            {
                projectWindows.Remove(mainWindow.Project.Title);
            };
            projectWindows.Add(project.Title, mainWindow);

        } 
    }


    // 创建项目
    public void CreateProject()
    {
        // 显示创建项目的窗口
        Rect rect = new Rect(0, 0, 600, 700);
        XFAssetBundleManagerCreate window = EditorWindow.GetWindowWithRect<XFAssetBundleManagerCreate>(rect, true, "创建项目");
        window.Show();

    }

}
#endif