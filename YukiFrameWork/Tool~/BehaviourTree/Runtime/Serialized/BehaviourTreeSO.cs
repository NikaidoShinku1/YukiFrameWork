///=====================================================
/// - FileName:      BehaviourTreeSO.cs
/// - NameSpace:     YukiFrameWork.Behaviours
/// - Description:   高级定制脚本生成
/// - Creation Time: 2024/11/14 15:03:25
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using YukiFrameWork.Extension;
using Newtonsoft.Json.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif
namespace YukiFrameWork.Behaviours
{
    public enum TreeShowInspectorType
    {
        Param,
        Json,
    }
    [HideMonoScript]
    [CreateAssetMenu(menuName = "YukiFrameWork/BehaviourTree",fileName = "BehaviourTree")]
    public class BehaviourTreeSO : ScriptableObject
    {
        [SerializeField,HideInInspector]
        public Rect ViewportRect;
        [SerializeField,HideInInspector]
        public List<AIBehaviour> AllNodes = new List<AIBehaviour>();
        [ReadOnly]
        public AIRootBehaviour Root;

        [SerializeField,HideInInspector]
        public TreeShowInspectorType treeShowInspectorType;      
        internal System.Action onValidate;


#if UNITY_EDITOR
        private void OnEnable()
        {
            List<AIBehaviour> releases = new List<AIBehaviour>();
            foreach (var item in AllNodes)
            {
                if (!item)
                {
                    releases.Add(item);
                    continue;
                }
                if (!item.behaviourTreeSO)
                    item.behaviourTreeSO = this;
            }
            foreach (var item in releases)
            {
                Remove(item);
            }
        }
        private void OnValidate()
        {
            onValidate?.Invoke();
        }
        public void ForEach(Action<AIBehaviour> each)
        {
            for (int i = 0; i < AllNodes.Count; i++)
            {
                each?.Invoke(AllNodes[i]);
            }
        }

        public AIBehaviour Create(Type type)
        {
            AIBehaviour behaviour = ScriptableObject.CreateInstance(type) as AIBehaviour;
            behaviour.name = type.Name;
            behaviour.ID = (1000) + AllNodes.Count + 1;

            if (!Root && behaviour is AIRootBehaviour rootBehaviour)
            {
                Root = rootBehaviour;
            }
            behaviour.behaviourTreeSO = this;
            Undo.RecordObject(this, "Behaviour Tree (CreateBehaviour)");
            AllNodes.Add(behaviour);
            AssetDatabase.AddObjectToAsset(behaviour, this);
            Undo.RegisterCreatedObjectUndo(behaviour, "Behaviour Tree (CreateBehaviour)");
            AssetDatabase.SaveAssets();
            return behaviour;
        }      
        public AIBehaviour Remove(AIBehaviour behaviour)
        {
            Undo.RecordObject(this, "Behaviour Tree (DeleteBehaviour)");
            AllNodes.Remove(behaviour);
            if (behaviour == Root)
                Root = null;
            for (int i = 1; i < AllNodes.Count; i++)
            {
                AllNodes[i].ID = (1000) + (i + 1);
            }
            Undo.DestroyObjectImmediate(behaviour);
            AssetDatabase.SaveAssets();
            return behaviour;
        }
        [Button("导出Json配置",ButtonHeight = 50)]
        void CreateFile(string filePath = "Assets/BehaviourTreeData",string fileName = "behaviourDatas")
        {         
            foreach (var item in AllNodes)
            {
                item.ForEach(chil => 
                {
                    if (chil)
                        item.LinksId.Add(chil.ID);
                });
                item.behaviourType = item.GetType().ToString();
            }
            SerializationTool.SerializedObject(AllNodes).CreateFileStream(filePath, fileName, ".json");
        }

        [Button("Json导入", ButtonHeight = 50)]
        void SerializaFile(TextAsset textAsset)
        {
            if (textAsset == null) return;
            List<AIBehaviour> behaviours = new List<AIBehaviour>(AllNodes);
            foreach (var item in behaviours)
            {
                Remove(item);
            }

            var items = SerializationTool.DeserializedObject<JArray>(textAsset.text);
            Debug.Log(items.Count);
            foreach(var obj in items)
            {             
                string typeName = obj["behaviourType"].Value<string>();
                Type type = AssemblyHelper.GetType(typeName);                
                AIBehaviour behaviour = Create(type);              
                behaviour.position = obj["position"].ToObject<AIBehaviourPosition>();
                behaviour.LinksId = obj["LinksId"].ToObject<List<int>>();
                if (behaviour is Composite beCom)
                {
                    beCom.AbortType = (AbortType)Enum.Parse(typeof(AbortType), obj["AbortType"].ToString());
                }
                else if (behaviour is Parallel parallel)
                {
                    parallel.Mode = (ParallelMode)Enum.Parse(typeof(ParallelMode), obj["Mode"].ToString());
                }
                else if (behaviour is Action action)
                {
                    action.behaviourActions = obj["behaviourActions"].ToObject<string[]>();
                }               
            }

            foreach (var item in AllNodes)
            {
                foreach (int id in item.LinksId)
                {
                    item.AddChild(AllNodes.Find(x => x.ID == id));
                }
                item.LinksId.Clear();
            }
        }


#endif


    }

}
