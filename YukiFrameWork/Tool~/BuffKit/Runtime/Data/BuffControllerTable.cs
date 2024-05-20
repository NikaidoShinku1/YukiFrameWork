///=====================================================
/// - FileName:      BuffControllerTable.cs
/// - NameSpace:     YukiFrameWork.Buffer
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/5/9 12:05:36
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
namespace YukiFrameWork.Buffer
{
    public class BuffControllerTable : TableKit<string, IBuffController>
    {
        private Dictionary<string, List<IBuffController>> mTables = new Dictionary<string, List<IBuffController>>();

        public override IDictionary<string, List<IBuffController>> Table => mTables;       

        protected override void OnDispose()
        {
            
        }
    }
}
