using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace YukiFrameWork
{
    /// <summary>
    /// 标记给字段或者属性，在容器注册实例后会自动赋值
    /// >>>>当注册进容器的实例类是派生自UnityEngine.Component时,Inject中的参数Path对于组件物体的标识为: InHerarchy ? 标签(tag) : 用于transform.Find查找的路径。由InHerarchy决定这个对象的注入是全局标签注入还是自身物体/子物体。对于非组件的对象Path仅用于标识注册的名称。
    /// >>>>当注册进容器的实例类只是普通类时，Inject如果作用于UnityEngine.Component,默认传递是无效的,至少需要传递Path或者将InHerarchy设置为True,当InHerarchy设置为True时,Path的标识类型为GameObject的名称,如果Path保持空传递则该对象会调用FindObjectOfType注入。
    ///
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
        /// <param name="path">路径(标签)(在非组件标记时是名称)</param>
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
