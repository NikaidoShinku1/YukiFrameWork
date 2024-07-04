using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using YukiFrameWork.Pools;

#if UNITY_EDITOR
namespace YukiFrameWork.States
{
    public class StateNodeFactory
    {
        private static int createIndex = 0;
        public static StateBase CreateStateNode(StateMechine stateMechine,string name,Rect rect,bool defaultState = false,bool subState = false,bool isAnyState = false)
        {
            if (stateMechine.states.Where(x => x.name.Equals(name)).FirstOrDefault() != null)
            {
                string message = "该状态已存在，创建失败，状态： " + name;
                Debug.LogError(message);
                StateMechineEditorWindow.OnShowNotification(message);
                return null;
            }

            StateBase data = new StateBase();

            data.name = name;
            data.layerName = stateMechine.layerName;
            data.rect = rect;
            data.IsAnyState = isAnyState;
            data.IsSubingState = subState;
            if (defaultState)
            {
                foreach (var item in stateMechine.states)
                {
                    item.defaultState = false;
                }
            }
            data.defaultState = defaultState;
            if (!stateMechine.IsSubLayerAndContainsName())
            {
                if (stateMechine.states.Count >= 1) createIndex = stateMechine.states.Count - 1;               
                if (name.Equals(StateConst.entryState) || (name.Equals(StateConst.anyState) && isAnyState))
                    data.index = -1;
                else
                    data.index = createIndex;               
                stateMechine.states.Add(data);
            }
            else
            {
                var current = stateMechine.states.Find(x => x.name == stateMechine.layerName);
                Predicate<StateBase> func = x => x.name == stateMechine.layerName;
                current ??= stateMechine.subStatesPair.Values
                    .Where(x => x.stateBases.Find(func) != null)
                    .FirstOrDefault()
                    .stateBases
                    .Find(func);
               
                data.index = current.index;

                if (stateMechine.subStatesPair[stateMechine.layerName].stateBases.Count == 3)
                {
                    data.defaultState = true;
                }
                stateMechine.subStatesPair[stateMechine.layerName].stateBases.Add(data);
            }
            if (data.IsSubingState)
            {
                StateBase sub = new StateBase();
                sub.name = StateConst.entryState;
                sub.rect = new Rect(0, -100, StateConst.StateWith, StateConst.StateHeight);
                sub.IsSubingState = false;
                sub.index = -1;
                StateBase upSub = new StateBase();
                upSub.name = StateConst.upState + stateMechine.layerName;
                upSub.index = -1;
                upSub.rect = new Rect(0, 300, StateConst.StateWith, StateConst.StateHeight);
                upSub.IsSubingState = true;

                StateBase anySub = new StateBase();
                anySub.name = StateConst.anyState;
                anySub.IsAnyState = true;
                anySub.index = -1;
                anySub.rect = new Rect(0, 500, StateConst.StateWith, StateConst.StateHeight);
                anySub.IsSubingState = false;
                var list = new System.Collections.Generic.List<StateBase>
                {
                    sub,
                    upSub,
                    anySub
                };
                if (!stateMechine.subTransitions.ContainsKey(name))
                {
                    stateMechine.subTransitions.Add(name, new List<StateTransitionData>());
                }
                SubStateData subData = new SubStateData(list);
                stateMechine.subStatesPair.Add(name, subData);
            }           
            EditorUtility.SetDirty(stateMechine);
            AssetDatabase.SaveAssets();

            return data;
        }

        public static StateBase CreateStateNode(StateMechine stateMechine, Rect rect, bool defaultState = false,bool subState = false)        
        {
            return CreateStateNode(stateMechine, GetStateName(stateMechine), rect, defaultState,subState);
        }

        public static string GetStateName(StateMechine stateMechine)
        {
            List<StateBase> stateBases = ListPools<StateBase>.Get();

            for (int j = 0; j < stateMechine.states.Count; j++)
            {
                stateBases.Add(stateMechine.states[j]);
            }

            foreach (var list in stateMechine.subStatesPair.Values)
            {
                for (int j = 0; j < list.stateBases.Count; j++)
                {
                    stateBases.Add(list.stateBases[j]);
                }
            }

            string name = string.Empty;
            int i = 0;
            do
            {
                name = string.Format("New State {0}", i);
                i++;
            } while (stateBases.Where(x => x.name.Equals(name)).FirstOrDefault() != null);           
            return name;
        }

        public static void DeleteState(StateMechine stateMechine,StateBase state)
        {
            //判断删除的是不是Entry，Any，如果不是才可以删除
            if (state.name.Equals(StateConst.entryState) || state.name.StartsWith(StateConst.upState) || state.name.Equals(StateConst.anyState))
            {
                string message = "当前状态是不可被删除的，状态为：" + state.name;
                Debug.LogWarning(message);
                StateMechineEditorWindow.OnShowNotification(message);
                return;
            }

            if (!stateMechine.IsSubLayerAndContainsName())
            {
                stateMechine.states.Remove(state);
                if (stateMechine.subStatesPair.ContainsKey(state.name))
                {
                    foreach (var s in stateMechine.subStatesPair[state.name].stateBases)
                    {
                        if (stateMechine.subStatesPair.ContainsKey(s.name))
                            stateMechine.subStatesPair.Remove(s.name);
                    }
                    stateMechine.subStatesPair.Remove(state.name);
                }
                EditorUtility.SetDirty(stateMechine);              
                if (state.defaultState)
                {
                    foreach (var item in stateMechine.states)
                    {
                        if (item.name.Equals(StateConst.entryState) || item.name.StartsWith(StateConst.upState) || item.name.Equals(StateConst.anyState))
                            continue;
                        item.defaultState = true;
                        break;
                    }
                }
            }
            else
            {
                stateMechine.subStatesPair[stateMechine.layerName].stateBases.Remove(state);
                if (stateMechine.subStatesPair.ContainsKey(state.name))
                {
                    foreach (var s in stateMechine.subStatesPair[state.name].stateBases)
                    {
                        if (stateMechine.subStatesPair.ContainsKey(s.name))
                            stateMechine.subStatesPair.Remove(s.name);
                    }
                    stateMechine.subStatesPair.Remove(state.name);
                }

                EditorUtility.SetDirty(stateMechine);              
                if (state.defaultState)
                {
                    foreach (var item in stateMechine.subStatesPair[stateMechine.layerName].stateBases)
                    {
                        if (item.name.Equals(StateConst.entryState) || item.name.StartsWith(StateConst.upState) || item.name.Equals(StateConst.anyState))
                            continue;
                        item.defaultState = true;
                        break;
                    }
                }
            }

            for (int i = 0; i < stateMechine.transitions.Count; i++)
            {
                if (stateMechine.transitions[i].fromStateName.Equals(state.name) || stateMechine.transitions[i].toStateName.Equals(state.name))
                {
                    stateMechine.transitions.RemoveAt(i);
                    i--;
                }
            }

            foreach (var transitions in stateMechine.subTransitions.Values)
            {
                for (int i = 0; i < transitions.Count; i++)
                {
                    var item = transitions[i];

                    if (item.fromStateName.Equals(state.name) || item.toStateName.Equals(state.name))
                    {
                        transitions.RemoveAt(i);
                        i--;
                    }
                }
            }          

            if (stateMechine.subTransitions.ContainsKey(state.name))
                stateMechine.subTransitions.Remove(state.name);

            if (stateMechine.states.Count == 1)
                stateMechine.subTransitions.Clear();

        }

        public static void Rename(StateMechine stateMechine, StateBase node, string newName)
        {
            string message;
            if (node.name.Equals(StateConst.entryState) || node.name.StartsWith(StateConst.upState) || node.name.Equals(StateConst.anyState))
            {
                message = "无法更改Entry、Any以及子节点根状态名称!";
                StateMechineEditorWindow.OnShowNotification(message);
                return;
            }
            message = $"无法修改名称!该名称已经在这个状态机里拥有了 --- Name:{newName}";
            if (stateMechine.states.Where(x => x.name.Equals(newName)).FirstOrDefault() != null)                
            {
                StateMechineEditorWindow.OnShowNotification(message);
                return;
            }

            foreach (var item in stateMechine.subStatesPair.Values)
                foreach (var state in item.stateBases)
                    if (state.name.Equals(newName))
                    {
                        StateMechineEditorWindow.OnShowNotification(message);
                        return;
                    }                                              
            //找到过渡相关的数据也要去更改名称

            foreach (var item in stateMechine.transitions)
            {
                if (node.IsSubingState)
                    item.layerName = newName;
                if (item.fromStateName.Equals(node.name))
                {
                    item.fromStateName = newName;
                }

                if (item.toStateName.Equals(node.name))
                {
                    item.toStateName = newName;
                }
            }

            foreach (var transitions in stateMechine.subTransitions.Values)
            {
                foreach (var item in transitions)
                {
                    if(node.IsSubingState)
                        item.layerName = newName;
                    if (item.fromStateName.Equals(node.name))
                    {
                        item.fromStateName = newName;
                    }

                    if (item.toStateName.Equals(node.name))
                    {
                        item.toStateName = newName;
                    }
                }
            }

            ///如果是子状态机
            if (node.IsSubingState)
            {               
                stateMechine.subStatesPair.Remove(node.name,out var value);
                stateMechine.subStatesPair[newName] = value;

                stateMechine.subTransitions.Remove(node.name, out var transitionDatas);
                stateMechine.subTransitions[newName] = transitionDatas;
            }

            node.name = newName;
            foreach (var item in node.dataBases)
            {
                item.name = newName;
            }
            EditorUtility.SetDirty(stateMechine);
            AssetDatabase.SaveAssets();
        }
       
    }
}
#endif