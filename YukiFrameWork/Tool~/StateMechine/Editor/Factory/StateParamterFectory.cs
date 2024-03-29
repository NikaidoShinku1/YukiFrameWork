using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR
namespace YukiFrameWork.States
{
    public static class StateParamterFectory 
    {
        public static void CreateParamter(this StateMechine stateMechine, ParameterType type)
        {
            StateParameterData parameterData = new StateParameterData();

            parameterData.name = GetDefaultName(stateMechine, type);
            parameterData.parameterType = type;
            parameterData.Value = 0;

            stateMechine.parameters.Add(parameterData);
            stateMechine.SaveToMechine();
        }

        public static void RemoveParamter(this StateMechine stateMechine,int index)
        {
            if (Application.isPlaying) return;

            StateParameterData parameterData = stateMechine.parameters[index];

            List<StateTransitionData> StateTransitionDatas = new List<StateTransitionData>();

            foreach (var item in stateMechine.transitions)
            {
                foreach (var condition in item.conditions)
                {
                    if (condition.parameterName != string.Empty && condition.parameterName.Equals(parameterData.name))
                    {
                        StateTransitionDatas.Add(item);
                        break;
                    }
                }
            }

            if (StateTransitionDatas.Count == 0)
            {
                stateMechine.parameters.RemoveAt(index);
            }
            else
            {
                StringBuilder content = new StringBuilder();

                content.Append("确定删除参数：").Append(parameterData.name).Append("吗?").Append("\n");

                content.Append("有以下过渡引用此参数！\n");

                foreach (var item in StateTransitionDatas)
                {
                    content.Append(item.fromStateName).Append("->").Append(item.toStateName);
                }

                if (EditorUtility.DisplayDialog("删除参数", content.ToString(), "确定","取消"))
                {
                    stateMechine.parameters.RemoveAt(index);

                    foreach (var item in stateMechine.transitions)
                    {
                        foreach (var condition in item.conditions)
                        {
                            if (condition.parameterName.Equals(parameterData.name))
                            {
                                condition.parameterName = string.Empty;
                                break;
                            }
                        }
                    }
                }

                stateMechine.SaveToMechine();
            }

            
        }

        public static void RenameParamter(this StateMechine stateMechine, StateParameterData parameter, string newName)
        {
            //判断名称是否为空
            if (string.IsNullOrEmpty(newName))
            {
                Debug.Log("参数名称不能为空！");
                return;
            }
            //新的名称是否已经存在

            if (stateMechine.parameters.Find(x => x.name.Equals(newName)) != null)
            {
                Debug.LogError("参数名称已经存在！name:" + newName);
                return;
            }
            //找到所有引用了此参数的过渡，修改所有的名称

            foreach (var item in stateMechine.transitions)
            {
                foreach (var condition in item.conditions)
                {
                    if (condition.parameterName != string.Empty && condition.parameterName.Equals(parameter.name))
                    {
                        condition.parameterName = newName;
                    }
                }
            }


            //修改参数

            parameter.name = newName;
            stateMechine.SaveToMechine();
        }

        private static string GetDefaultName(StateMechine stateMechine,ParameterType type)
        {
            string name = string.Format("New {0}", type.ToString());

            string targetName = name;

            int i = 1;

            while (stateMechine.parameters.Find(x => x.name.Equals(targetName)) != null)
            {
                targetName = string.Format("{0}{1}", name, i);
                i++;
            }

            return targetName;
        }
       
    }
}
#endif