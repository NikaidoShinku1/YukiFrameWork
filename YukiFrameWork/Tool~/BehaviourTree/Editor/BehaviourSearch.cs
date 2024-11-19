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
/// ��������
/// </summary>
public class NodeSearchWindowProvider : ScriptableObject, UnityEditor.Experimental.GraphView.ISearchWindowProvider
{
    private BehaviourTreeGraphWindow window;
    private BackGroundView graphView;
    private BehaviourTreeSO behaviourTree;
    /// <summary>
    /// ��ʼ��
    /// </summary>
    public void Init(BehaviourTreeGraphWindow window,BackGroundView view,BehaviourTreeSO behaviourTree)
    {
        this.window = window;
        graphView = view;
        this.behaviourTree = behaviourTree;    
        
    }

    /// <summary>
    /// ����������
    /// </summary>
    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        var entries = new List<SearchTreeEntry>();

        entries.Add(new SearchTreeGroupEntry(new GUIContent("��Ϊ���ڵ�"), 0));      
        entries.Add(new SearchTreeGroupEntry(new GUIContent("���Ͻڵ�"), 1));
        AddNodeOptions<Composite>(entries);       

        entries.Add(new SearchTreeGroupEntry(new GUIContent("װ�νڵ�"), 1));
        AddNodeOptions<Decorator>(entries);

        entries.Add(new SearchTreeGroupEntry(new GUIContent("�����ڵ�"), 1));

        entries.Add(new SearchTreeGroupEntry(new GUIContent("�����ڵ�"), 1));
        AddNodeOptions<YukiFrameWork.Behaviours.Action>(entries);

        return entries;
    }

    /// <summary>
    /// ��ӽڵ�ѡ��
    /// </summary>
    private void AddNodeOptions<T>(List<SearchTreeEntry> entries)
    {
        var types = TypeCache.GetTypesDerivedFrom<T>().ToList();     
        foreach (Type type in types)
        {          
            if (type == typeof(AIRootBehaviour))
            {
                //�������ڵ�
                continue;
            }        
           
            var guiContent = new GUIContent(type.Name);
            entries.Add(new SearchTreeEntry(guiContent) { level =  2,userData = type});
        }
    }

    public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
    {
        //ȡ��������Ϣ
        Type type = (Type)searchTreeEntry.userData;     
        //�����ڵ� 
        var point = context.screenMousePosition - window.position.position; //�������ڴ��ڵ�λ��
        Vector2 graphMousePosition = graphView.contentViewContainer.WorldToLocal(point); //����ڽڵ�ͼ�µ�λ��
        GraphBehaviourView.Create(type, behaviourTree,graphView, graphMousePosition);            

        return true;
    }
}
#endif