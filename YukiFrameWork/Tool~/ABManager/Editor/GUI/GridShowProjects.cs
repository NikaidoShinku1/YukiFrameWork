using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using XFABManager;

public class GridShowProjects : BaseShowProjects
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

    private int row_grid_count = 1;     // 每行显示个子的数量 默认是1

    private Vector2 scrollPosition;         // Project Grid 滚动的位置

    private GUIContent buttonContent;
    private GUIStyle buttonStyle;

    private Texture refreshTexture;

    private XFAssetBundleProjectMain mainWindow;

    private GUIContent profileContent;
    private GUIContent toolsContent;
    private GUIContent showContent;

    #endregion
     


    public override void DrawProjects(Rect rect,EditorWindow window)
    {
        base.DrawProjects(rect,window);
        if (buttonContent == null) {
            ConfigStyle();
        }
        //Debug.Log(" Grid DrawProjects ");
        CaculateRowGridCount();
        DrawProjects();

    }

    // 配置按钮样式
    public void ConfigStyle()
    {
        buttonContent = new GUIContent();
        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.richText = true;
        buttonStyle.wordWrap = true;
    }

    // 画出具体某个格子
    private void DrawProjectGrid(int index)
    {


        if (index < XFABProjectManager.Instance.Projects.Count)
        {
            //buttonContent.tooltip = XFABProjectManager.Instance.Projects[index].displayName;
            // 显示具体模块
            buttonContent.text = string.Format("<size=18>{0}</size>\n\n{1}\n\n版本:{2}", XFABProjectManager.Instance.Projects[index].displayName, XFABProjectManager.Instance.Projects[index].name, XFABProjectManager.Instance.Projects[index].version);
        }
        else
        {
            // 显示 添加 按钮
            buttonContent.text = "<size=40>+</size>"; //  <color=#00ffffff>+</color>  <size=40>TestTest</size>
        }

        if (GUILayout.Button(buttonContent, buttonStyle, GUILayout.Width(GRID_WIDTH), GUILayout.Height(GRID_HEIGHT)))
        {
            if (index < XFABProjectManager.Instance.Projects.Count)
            {
                //Debug.Log(" 打开项目: " + projects[index].name);
                OpenProject(XFABProjectManager.Instance.Projects[index]);
            }
            else
            {
                //Debug.Log(" 添加 或 创建项目 ");
                CreateProject();
            }
        }

        if (index <= XFABProjectManager.Instance.Projects.Count - 1)
        {
            GUILayout.Space(SPACE_X);
        }

    }


    // 计算每行格子的数量
    private void CaculateRowGridCount()
    {

        // row_grid_count * width + row_grid_count  * Space_X  = width - m_left + Space_X
        row_grid_count = (int)((position.width - MARGIN_LEFT + SPACE_X) / (GRID_WIDTH + SPACE_X));

        if (row_grid_count < 1)
        {
            row_grid_count = 1;
        }

        if (row_grid_count > XFABProjectManager.Instance.Projects.Count + 1)    // 除了要画出格子之外还要画出一个 + 的按钮
        {
            row_grid_count = XFABProjectManager.Instance.Projects.Count + 1;
        }
    }

    // 画出所有的项目 和 添加按钮
    private void DrawProjects()
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        GUILayout.Space(MARGIN_TOP);
        //内置图标
        for (int i = 0; i < XFABProjectManager.Instance.Projects.Count + 1; i += row_grid_count)
        {

            GUILayout.BeginHorizontal();

            for (int j = 0; j < row_grid_count; j++)
            {
                if (j == 0)
                {
                    GUILayout.Space(MARGIN_LEFT);
                }

                if (i + j < XFABProjectManager.Instance.Projects.Count + 1)
                {
                    DrawProjectGrid(i + j);
                }

            }
            GUILayout.EndHorizontal();
            GUILayout.Space(SPACE_Y);
        }

        GUILayout.EndScrollView();

        GUILayout.Label(row_grid_count.ToString());
    }

}
