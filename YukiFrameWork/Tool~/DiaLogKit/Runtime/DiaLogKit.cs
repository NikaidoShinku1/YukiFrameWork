///=====================================================
/// - FileName:      DiaLogKit.cs
/// - NameSpace:     YukiFrameWork.DiaLog
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/12 20:50:57
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
namespace YukiFrameWork.DiaLog
{
	public class DiaLogKit : Singleton<DiaLogKit>
	{
		private List<DiaLogController> controllers = new List<DiaLogController>();       
		public void AddController(DiaLogController controller)
		{
			controllers.Add(controller);	
		}

        private DiaLogKit() { }

        public override void OnInit()
        {
            base.OnInit();
            
        }      

       
    }
}
