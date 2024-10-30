///=====================================================
/// - FileName:      YMonoBehaviour.cs
/// - NameSpace:     YukiFrameWork.Tower
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/5/24 17:00:06
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using System.Linq;
using System.Reflection;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
namespace YukiFrameWork
{
	public class YMonoBehaviour : Sirenix.OdinInspector.SerializedMonoBehaviour
	{       
        [SerializeField]
        [LabelText("可视化Awake")]
        [InfoBox("通过在可视化注册的事件可以拿到这个组件的Transform"),FoldoutGroup("(派生YMonoBehaviour类专属功能)")]
        private UnityEvent<Transform> onAwake;       
        protected virtual void Awake()
        {
            onAwake?.Invoke(transform);
           
        }

        
    }
}
