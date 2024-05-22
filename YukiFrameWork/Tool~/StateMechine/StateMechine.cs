using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace YukiFrameWork.States
{
    [Serializable]
    public class SubStateData
    {
        public List<StateBase> stateBases = new List<StateBase>();
        public StateBase CurrentState { get; set; }

        public SubStateData(List<StateBase> stateBases)
        {
            this.stateBases = stateBases;           
        }
    }
    [System.Serializable]
    public class StateMechine : MonoBehaviour
    {

        [ReadOnly]
        [BoxGroup("图层")]
        [LabelText("当前置顶的图层")]
        public string layerName => parents[parents.Count - 1];

        [BoxGroup("图层")]
        [LabelText("当前显示的层级"),ReadOnly]        
        public List<string> parents = new List<string>()
        {
            "BaseLayer"
        };
#if UNITY_EDITOR
        [ReadOnly]
        [BoxGroup("图层")]
        [SerializeField]private bool isSubLayer;
         
        public Action onChangeValue;
        public bool IsSubLayer
        {
            get => isSubLayer;
            set
            {
                if (!value)
                    ResetParents();

                if (isSubLayer != value)
                {
                    isSubLayer = value;
                    onChangeValue?.Invoke();
                }
            }
        }      

        public void ResetParents()
        {
            parents = new List<string>()
            {
                "BaseLayer"
            };
        }
#endif
        [BoxGroup("数据"),LabelText("该状态机给状态类标记的名称"),ReadOnly]
        public string architectureName;
        [BoxGroup("数据"), LabelText("该状态机给状态类标记的架构下标"), ReadOnly]
        public int architectureIndex;
        [BoxGroup("数据")]
        [LabelText("默认图层下所有的状态")]      
        public List<StateBase> states = new List<StateBase>();      
        [LabelText("默认的所有参数")]
        [BoxGroup("数据")]
        public List<StateParameterData> parameters = new List<StateParameterData>();
        [BoxGroup("数据")]
        [LabelText("默认的所有条件过渡")]
        public List<StateTransitionData> transitions = new List<StateTransitionData>();
        [BoxGroup("数据")]
        [LabelText("每个图层分别保存的状态机")]
        public YDictionary<string, SubStateData> subStatesPair = new YDictionary<string, SubStateData>();
        [BoxGroup("数据")]
        [LabelText("每个图层分别保存的条件过渡")]
        public YDictionary<string, List<StateTransitionData>> subTransitions = new YDictionary<string, List<StateTransitionData>>();
       
#if UNITY_EDITOR
        public void SaveToMechine()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        public List<StateBase> GetStatesOfBox()
        {
            if (layerName.Equals("BaseLayer"))
                return states;

            subStatesPair.TryGetValue(layerName, out var list);
            return list.stateBases;
        }

        public bool IsSubLayerAndContainsName()
        {
            return IsSubLayer && !layerName.Equals("BaseLayer") && subStatesPair.ContainsKey(layerName);
        }

        [Button("刷新图层回到BaseLayer")]
        void Test() => IsSubLayer = false;
#endif
    }
}
