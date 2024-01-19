using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace YukiFrameWork
{
    /// <summary>
    /// 标记给字段或者属性，在容器注册实例后会自动赋值，如果该字段所在类不派生自组件则赋值操作只对注册的非组件实例生效,当类继承自Mono时则可以检索包括Unity组件在内的字段(属性)
    /// 进行赋值，并且如果自身物体及其子物体没有时可以通过将InHierarchy设置为True而检索一次场景
    /// </summary>
    [AttributeUsage(AttributeTargets.Property
        | AttributeTargets.Field     
        ,AllowMultiple = true)]
    public class InjectAttribute : Attribute
    {
        private string path;
        private bool hierarchy;
     
        public string Path => path;
        public bool InHierarchy => hierarchy;     
       
        /// <summary>
        /// 默认路径(名称)为空查找
        /// </summary>
        /// <param name="InHierarchy">是否通过场景获取(仅组件生效)</param>
        public InjectAttribute(bool InHierarchy = false)
        {
            path = string.Empty;
            this.hierarchy = InHierarchy;
        }      

        /// <summary>
        /// 当字段(属性)为组件时附带参数的地址，如果没有则从全局找，非组件时Path仅为需要获取的实例名称！
        /// </summary>
        /// <param name="path">路径(在非组件标记时是名称)</param>
        /// <param name="InHierarchy">是否通过场景获取(仅组件生效)</param>
        public InjectAttribute(string path, bool InHierarchy = false)
        {
            this.path = path;
            this.hierarchy = InHierarchy;
        }  
    }  

    /// <summary>
    /// 标记该特性后可以自定义构造函数，也可以代替Awake在MonoBehaviour脚本中进行初始化，初始化周期与Awake相同
    /// (注意：在已有多数相同类型实例注册的情况下返回最先注册的实例)
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class InjectMethodAttribute : Attribute
    {      
        public InjectMethodAttribute()
        {
            
        }
    }
}
