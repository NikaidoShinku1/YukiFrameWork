using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork.States
{
    public interface IState
    {
        Transform transform { get; }

        Dictionary<int, StateBase> runTimeStatesDict { get; }

        StateBase CurrentState { get; set; }

        public Dictionary<string,StateParameterData> ParamterDicts { get; }

        bool GetBool(string name);

        int GetInt(string name);

        float GetFloat(string name);

        void SetBool(string name, bool v);

        void SetFloat(string name, float v);

        void SetInt(string name, int v);

        void OnChangeState(StateBase state, System.Action callBack = null, bool isBack = true);

        void OnChangeState(int index, System.Action callBack = null, bool isBack = true);

        void OnChangeState(string name, System.Action callBack = null, bool isBack = true);
    }
}