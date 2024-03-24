///=====================================================
/// - FileName:      MethodButtonAttribute.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/1 20:06:17
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork
{
    [AttributeUsage(AttributeTargets.Method)]
	public sealed class MethodButtonAttribute : PropertyAttribute
	{
        private string label;
        private float height;    
        public string Label => label;
        public float Height => height;
              
        public MethodButtonAttribute(string label,float height)
        {
            this.label = label;
            this.height = height;         
           
        }
        public MethodButtonAttribute(float height)
        {
            this.label = string.Empty;
            this.height = height;                     
        }

        public MethodButtonAttribute(string label)
        {
            this.label = label;
            this.height = 20;                
        }

        public MethodButtonAttribute()
        {
            this.label = string.Empty;
            this.height = 20;                      
        }
    }
}
