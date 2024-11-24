using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using YukiFrameWork.Behaviours;
/// <summary>
/// 搜索窗口
/// </summary>
public class NodeSearchWindowProvider : ScriptableObject, UnityEditor.Experimental.GraphView.ISearchWindowProvider
{
    private BehaviourTreeGraphWindow window;
    private BackGroundView graphView;
    private BehaviourTreeSO behaviourTree;
    /// <summary>
    /// 初始化
    /// </summary>
    public void Init(BehaviourTreeGraphWindow window,BackGroundView view,BehaviourTreeSO behaviourTree)
    {
        this.window = window;
        graphView = view;
        this.behaviourTree = behaviourTree;    
        
    }

    /// <summary>
    /// 创建搜索树
    /// </summary>
    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        var entries = new List<SearchTreeEntry>();

        entries.Add(new SearchTreeGroupEntry(new GUIContent("行为树节点"), 0));      
        entries.Add(new SearchTreeGroupEntry(new GUIContent("复合节点"), 1));
        AddNodeOptions<Composite>(entries);       

        entries.Add(new SearchTreeGroupEntry(new GUIContent("装饰节点"), 1));
        AddNodeOptions<Decorator>(entries);

        entries.Add(new SearchTreeGroupEntry(new GUIContent("条件节点"), 1));
        AddNodeOptions<Condition>(entries);
        entries.Add(new SearchTreeGroupEntry(new GUIContent("动作节点"), 1));
        AddNodeOptions<YukiFrameWork.Behaviours.Action>(entries);

        return entries;
    }

    /// <summary>
    /// 添加节点选项
    /// </summary>
    private void AddNodeOptions<T>(List<SearchTreeEntry> entries)
    {
        var types = TypeCache.GetTypesDerivedFrom<T>().ToList();     
        foreach (Type type in types)
        {          
            if (type == typeof(AIRootBehaviour))
            {
                //跳过根节点
                continue;
            }        
           
            var guiContent = new GUIContent(type.Name);
            entries.Add(new SearchTreeEntry(guiContent) { level =  2,userData = type});
        }
    }

    public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
    {
        //取出类型信息
        Type type = (Type)searchTreeEntry.userData;     
        //创建节点 
        var point = context.screenMousePosition - window.position.position; //鼠标相对于窗口的位置
        Vector2 graphMousePosition = graphView.contentViewContainer.WorldToLocal(point); //鼠标在节点图下的位置
        GraphBehaviourView.Create(type, behaviourTree,graphView, graphMousePosition);            

        return true;
    }
}
#endif