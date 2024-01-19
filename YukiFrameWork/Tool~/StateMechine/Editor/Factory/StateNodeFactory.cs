using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace YukiFrameWork.States
{
    public class StateNodeFactory : MonoBehaviour
    {
        private static int createIndex = 0;
        public static StateBase CreateStateNode(StateMechine stateMechine,string name,Rect rect,bool defaultState = false)
        {
            if (stateMechine.states.Where(x => x.name.Equals(name)).FirstOrDefault() != null)
            {
                Debug.LogError("该状态已存在，创建失败，状态： " + name);
                return null;
            }

            StateBase data = new StateBase();

            data.name = name;
            data.rect = rect;
            if (stateMechine.states.Count >= 1) createIndex = stateMechine.states.Count - 1;
            if (name.Equals(StateConst.entryState))
                data.index = -1;
            else 
                data.index = createIndex;
            if (defaultState)
            {
                foreach (var item in stateMechine.states)
                {
                    item.defaultState = false;
                }
            }

            data.defaultState = defaultState;
            stateMechine.states.Add(data);
            EditorUtility.SetDirty(stateMechine);
            AssetDatabase.SaveAssets();

            return data;
        }

        public static StateBase CreateStateNode(StateMechine stateMechine, Rect rect, bool defaultState = false)        
        {
            return CreateStateNode(stateMechine, GetStateName(stateMechine), rect, defaultState);
        }

        public static string GetStateName(StateMechine stateMechine)
        {
            string name = string.Empty;
            int i = 0;
            do
            {
                name = string.Format("New State {0}", i);
                i++;
            } while (stateMechine.states.Where(x => x.name.Equals(name)).FirstOrDefault() != null);           
            return name;
        }

        public static void DeleteState(StateMechine stateMechine,StateBase state)
        {
            //判断删除的是不是Entry，Any，如果不是才可以删除
            if (state.name.Equals(StateConst.entryState))
            {
                Debug.LogWarning("当前状态是不可被删除的，状态为：" + state.name);
                return;
            }

            stateMechine.states.Remove(state);
            EditorUtility.SetDirty(stateMechine);
            //删除相关的过渡 ToDo

            for (int i = 0; i < stateMechine.transitions.Count; i++)
            {
                if (stateMechine.transitions[i].fromStateName.Equals(state.name) || stateMechine.transitions[i].toStateName.Equals(state.name))
                {
                    stateMechine.transitions.RemoveAt(i);
                    i--;
                }
            }

            if (state.defaultState)
            {
                foreach (var item in stateMechine.states)
                {
                    if (item.name.Equals(StateConst.entryState))
                        continue;
                    item.defaultState = true;
                    break;
                }
            }

            //判断状态是不是默认状态，如果是默认，则要转移默认
        }

        public static void Rename(StateMechine stateMechine, StateBase node, string newName)
        {
            if (node.name.Equals(StateConst.entryState))
                return;

            if (stateMechine.states.Where(x => x.name.Equals(newName)).FirstOrDefault() != null)
            {
                return;
            }

            //找到过渡相关的数据也要去更改名称

            foreach (var item in stateMechine.transitions)
            {
                if (item.fromStateName.Equals(node.name))
                {
                    item.fromStateName = newName;
                }

                if (item.toStateName.Equals(node.name))
                {
                    item.toStateName = newName;
                }
            }

            node.name = newName;

            EditorUtility.SetDirty(stateMechine);
            AssetDatabase.SaveAssets();
        }
       
    }
}
#endif