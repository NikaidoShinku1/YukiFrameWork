///=====================================================
/// - FileName:      UserModel.cs
/// - NameSpace:     YukiFrameWork.ExampleRold
/// - Description:   示例模型，非必要不可修改，仅供参考
/// - Creation Time: 2024/5/10 16:32:23
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.ExampleRule.ExampleFrameWork
{
    public class UserModel : AbstractModel
    {
        private int age;
        private string name;
        public override void Init()
        {

        }
        /// <summary>
        /// 更新Age后会触发一次事件
        /// </summary>
        public int Age
        {
            get => age;
            set
            {
                this.SendEvent<EventArgs>(Example.UserEventKey);
                Debug.Log("age被改变:" + value);
                age = value;
            }
        }

        /// <summary>
        /// 更新Name后会触发一次事件
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                this.SendEvent<EventArgs>(Example.UserEventKey);
                Debug.Log("name被改变:" + value);
                name = value;
            }
        }
    }
}
