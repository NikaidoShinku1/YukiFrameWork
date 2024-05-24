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
    /// <summary>
    ///  GameObject GetComponent,仅对象派生自框架提供的YMonoBehaviour有效
    /// </summary>
	public sealed class VGetComponent : BaseComponentAttribute
    {
		public VGetComponent()
		{
			
		}	
	}

    /// <summary>
    /// GameObject AddComponent,仅对象派生自框架提供的YMonoBehaviour有效
    /// </summary>
    public sealed class VAddComponent : BaseComponentAttribute
    {
		public VAddComponent()
		{
			
		}
	}

    /// <summary>
    /// GameObject AddComponent Or GetComponent,仅对象派生自框架提供的YMonoBehaviour有效
    /// </summary>
    public sealed class VGetOrAddComponent : BaseComponentAttribute
    {
        public VGetOrAddComponent()
        {
           
        }
    }
   
    /// <summary>
    /// GameObject GetComponentInChildren,仅对象派生自框架提供的YMonoBehaviour有效
    /// </summary>
    public sealed class VGetComponentInChildren : BaseComponentAttribute
    {   
        public bool Include { get; }
        public VGetComponentInChildren(bool include = false)
        {
            this.Include = include;
        }
    }

    /// <summary>
    /// 通过子对象/自身的名称进行组件赋值,仅对象派生自框架提供的YMonoBehaviour有效
    /// </summary>
    public sealed class VFindChildComponentByName : BaseComponentAttribute
    {
        public string name { get; }

        public VFindChildComponentByName(string name)
        {
            this.name = name;
        }
    }

    /// <summary>
    /// 与Transform.Find相同，需要输入完整路径,仅对象派生自框架提供的YMonoBehaviour有效
    /// </summary>
    public sealed class VFindChildComponentByPath : BaseComponentAttribute
    {
        public string path { get; }

        public VFindChildComponentByPath(string path)
        {
            this.path = path;
        }
    }


#if UNITY_2021_1_OR_NEWER
    /// <summary>
    /// GameObject FindObjectOfType,仅对象派生自框架提供的YMonoBehaviour有效
    /// </summary>
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
