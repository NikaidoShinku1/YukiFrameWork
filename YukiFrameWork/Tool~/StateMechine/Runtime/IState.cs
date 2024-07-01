using System;
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

        List<StateBase> currents { get; }

        bool GetBool(string name);

        int GetInt(string name);

        float GetFloat(string name);

        void SetBool(string name, bool v);

        void SetFloat(string name, float v);

        void SetInt(string name, int v);
        void OnChangeState(StateBase state, System.Action callBack = null, bool isBack = true);
        [Obsolete("不建议再使用该方法进行强制切换，请使用参数为名称的OnChangeState")]
        void OnChangeState(int index, System.Action callBack = null, bool isBack = true);

        void OnChangeState(string name,string layerName, System.Action callBack = null, bool isBack = true);

        void SetTrigger(string name);

        void ResetTrigger(string name);
    }
}