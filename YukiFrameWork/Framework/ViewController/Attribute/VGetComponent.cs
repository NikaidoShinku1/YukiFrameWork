///=====================================================
/// - FileName:      VGetComponent.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/5/23 17:26:05
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork
{

	public sealed class VGetComponent : BaseComponentAttribute
    {
		public VGetComponent()
		{
			
		}	
	}
	
	public sealed class VAddComponent : BaseComponentAttribute
    {
		public VAddComponent()
		{
			
		}
	}
    
    public sealed class VGetOrAddComponent : BaseComponentAttribute
    {
        public VGetOrAddComponent()
        {
           
        }
    }
   
    public sealed class VGetComponentInChildren : BaseComponentAttribute
    {   
        public VGetComponentInChildren()
        {
            
        }
    }

#if UNITY_2021_1_OR_NEWER
    public sealed class VFindObjectOfType : BaseComponentAttribute
    {
        internal FindObjectsInactive ObjectsInactive { get; }
        public VFindObjectOfType(FindObjectsInactive findObjectsInactive = FindObjectsInactive.Exclude)
        {
            ObjectsInactive = findObjectsInactive;
        }
    }
#else
    public sealed class VFindObjectOfType : BaseComponentAttribute
    {
        internal bool includeInactive { get; }
        public VFindObjectOfType(bool includeInactive = false)
        {
            this.includeInactive = includeInactive;
        }
    }
#endif
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field,AllowMultiple = false,Inherited = true)]
    public class BaseComponentAttribute : Attribute
    {
        
    }

}
