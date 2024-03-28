using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork.States
{
    public interface IState
    {
        Transform transform { get; }             

        Dictionary<string,StateParameterData> ParametersDicts { get; }

        Dictionary<string, SubStateData> runTimeSubStatePair { get; }

        bool GetBool(string name);

        int GetInt(string name);

        float GetFloat(string name);

        void SetBool(string name, bool v);

        void SetFloat(string name, float v);

        void SetInt(string name, int v);
        void OnChangeState(StateBase state, System.Action callBack = null, bool isBack = true);

        void OnChangeState(int index, System.Action callBack = null, bool isBack = true);

        void OnChangeState(string name,string layerName, System.Action callBack = null, bool isBack = true);
    }
}