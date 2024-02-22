using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace YukiFrameWork.IOC
{
    /// <summary>  
    /// IOC容器注入特性，该特性标记在方法中等同于构造函数，也可以直接在构造函数上标记,如果在多个构造函数上标记特性只返回第一个找到的构造函数
    /// 在MonoBehaviour脚本中等效Awake，通过在容器中注册的实例，可以进行多参传递，如果该特性了标记字段/属性后会自动赋值
    /// </summary>
    [AttributeUsage(AttributeTargets.Property
        | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Constructor
        ,AllowMultiple = true)]
    public class InjectAttribute : Attribute
    {
     
        private string[] names;

        public string[] Names => names;     
        /// <summary>
        /// 传入注册的名称,如果作用字段/属性则只取第一个名称，用于多参构造函数时，可以为每一个参数标记注册的名称，精确获取
        /// </summary>
        /// <param name="names"></param>
        public InjectAttribute(params string[] names)
        {
            this.names = names;
        }
    }     
}
