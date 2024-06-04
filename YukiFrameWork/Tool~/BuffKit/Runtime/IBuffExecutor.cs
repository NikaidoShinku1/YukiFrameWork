///=====================================================
/// - FileName:      IBuffExecutor.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/6/4 23:04:26
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using YukiFrameWork.Buffer;
namespace YukiFrameWork
{
	public interface IBuffExecutor
	{
		public BuffHandler Handler { get; }

        /// <summary>
        /// 外部Buff添加条件，默认为True，与Controller的添加条件区别在于，该方法控制所有的buff添加判断，如果外部添加条件设置为False,则无法添加任何Buff
        /// </summary>
        /// <returns></returns>
        bool OnAddBuffCondition();       
    }
}
