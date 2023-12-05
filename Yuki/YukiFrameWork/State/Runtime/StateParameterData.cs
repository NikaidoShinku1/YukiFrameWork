using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork.States
{
    public enum ParameterType
    {
        Float = 0,
        Int,
        Bool
    }

    [System.Serializable]
    public class StateParameterData 
    {
        #region 字段
        public string name;      

        [SerializeField]private float value;

        public ParameterType parameterType;

        public Action onValueChange;
        #endregion

        #region 属性

        public float Value
        {
            get => value;
            set
            {
                if (this.value != value)
                {                 
                    this.value = value;
                    onValueChange?.Invoke();
                }
            }
        }

        #endregion

        #region 方法      
        #endregion
    }
}