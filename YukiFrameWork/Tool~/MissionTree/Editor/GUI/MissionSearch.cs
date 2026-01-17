using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using YukiFrameWork.Missions;
/// <summary>
/// 搜索窗口
/// </summary>
public class NodeSearchWindowProvider : ScriptableObject, UnityEditor.Experimental.GraphView.ISearchWindowProvider
{
    private MissionTreeGraphWindow window;
    private BackGroundView graphView;
    private MissionTreeSO missionTree;
    /// <summary>
    /// 初始化
    /// </summary>
    public void Init(MissionTreeGraphWindow window, BackGroundView view, MissionTreeSO missionTree)
    {
        this.window = window;
        graphView = view;
        this.missionTree = missionTree;

    }

    /// <summary>
    /// 创建搜索树
    /// </summary>
    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        var entries = new List<SearchTreeEntry>();

       // entries.Add(new SearchTreeGroupEntry(new GUIContent("任务节点"), 0));
       // entries.Add(new SearchTreeGroupEntry(new GUIContent("顺序任务"), 1));
       // AddNodeOptions<CompositeMission>(entries);
       // entries.Add(new SearchTreeGroupEntry(new GUIContent("前置任务"), 1));
       // AddNodeOptions<PreMission>(entries);
       // entries.Add(new SearchTreeGroupEntry(new GUIContent("自定义任务"), 1));
       // AddNodeOptions<CustomMission>(entries);


        return entries;
    }


    public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
    {
        //取出类型信息
        Type type = (Type)searchTreeEntry.userData;
        //创建节点 
        var point = context.screenMousePosition - window.position.position; //鼠标相对于窗口的位置
        Vector2 graphMousePosition = graphView.contentViewContainer.WorldToLocal(point); //鼠标在节点图下的位置
       // GraphMissionView.Create(type, missionTree, graphView, graphMousePosition);

        return true;
    }
}
#endif