using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
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

        [EditorDisabledGroup]
        [RuntimeDisabledGroup]
        [GUIGroup("图层")]
        [Label("当前置顶的图层")]
        public string layerName => parents[parents.Count - 1];

        [GUIGroup("图层")]
        [Label("当前显示的层级")]
        [ListDrawerSetting(true)]
        public List<string> parents = new List<string>()
        {
            "BaseLayer"
        };
#if UNITY_EDITOR
        [EditorDisabledGroup]
        [RuntimeDisabledGroup]
        [GUIGroup("图层")]
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
        [GUIGroup("数据")]
        [Label("默认图层下所有的状态")]      
        public List<StateBase> states = new List<StateBase>();      
        [Label("默认的所有参数")]
        [GUIGroup("数据")]
        public List<StateParameterData> parameters = new List<StateParameterData>();
        [GUIGroup("数据")]
        [Label("默认的所有条件过渡")]
        public List<StateTransitionData> transitions = new List<StateTransitionData>();
        [GUIGroup("数据")]
        [Label("每个图层分别保存的状态机")]
        public YDictionary<string, SubStateData> subStatesPair = new YDictionary<string, SubStateData>();
        [GUIGroup("数据")]
        [Label("每个图层分别保存的条件过渡")]
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

        [MethodButton("刷新图层回到BaseLayer")]
        void Test() => IsSubLayer = false;
#endif
    }
}
