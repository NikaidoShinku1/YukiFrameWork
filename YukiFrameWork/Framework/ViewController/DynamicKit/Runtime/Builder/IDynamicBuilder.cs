///=====================================================
/// - FileName:      Initializable.cs
/// - NameSpace:     YukiFrameWork.Dynamic
/// - Description:   高级定制脚本生成
/// - Creation Time: 1/6/2026 12:55:06 PM
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork
{
    /// <summary>
    /// 动态注入标记接口，该接口仅作为标记存在,依赖于IDynamicMonoBehaviour
    /// <para>Tips:请注意，使用该接口没有任何默认实现,框架规则上只需要自己手动实现Builder方法并且具备参数即可。即便空参也可以使用。当类型进行注入后，Builder方法会被自动调用,框架提供了最多9个参数的泛型版本。</para>    
    /// <code>public class TestScripts : ViewController,IDynamicBuilder"/>
    /// {
    ///     public void Builder(Transform transform,[DynamicValue]TestScripts testScripts)
    ///     {
    ///         //可进行实现
    ///     }
    /// }
    /// </code>
    /// 默认情况下,参数会通过自身进行查找。以GetComponent的形式获取到对应的参数并赋值，如果需要自定义，可为参数标记DynamicValue特性或DynamicValueFromScene特性以指定获取形式
    /// </summary>
    public interface IDynamicBuilder : IDynamicMonoBehaviour
    {
        
    }

    /// <summary>
    /// 动态注入标记接口,泛型版本,该接口需要实现Builder方法。当类型被注入后，Builder会被自动调用
    /// <code>public class TestScripts : ViewController,<see cref="IDynamicBuilder{T1}"/>
    /// {
    ///     public void Builder(T1 authoring)
    ///     {
    ///         //可进行实现
    ///     }
    /// }
    /// </code>
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    public interface IDynamicBuilder<T1> : IDynamicBuilder
    {
        void Builder(T1 authoring);
    }

    /// <summary>
    /// 动态注入标记接口,泛型版本,该接口需要实现Builder方法。当类型被注入后，Builder会被自动调用
    /// <code>public class TestScripts : ViewController,<see cref="IDynamicBuilder{T1,T2}"/>
    /// {
    ///     public void Builder(T1 authoring1,T2 authoring2)
    ///     {
    ///         //可进行实现
    ///     }
    /// }
    /// </code>
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    public interface IDynamicBuilder<T1, T2> : IDynamicBuilder
    {
        void Builder(T1 authoring1,T2 authoring2);
    }

    /// <summary>
    /// 动态注入标记接口,泛型版本,该接口需要实现Builder方法。当类型被注入后，Builder会被自动调用
    /// <code>public class TestScripts : ViewController,<see cref="IDynamicBuilder{T1,T2,T3}"/>
    /// {
    ///     public void Builder(T1 authoring1,T2 authoring2,T3 authoring3)
    ///     {
    ///         //可进行实现
    ///     }
    /// }
    /// </code>
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    public interface IDynamicBuilder<T1, T2, T3> : IDynamicBuilder
    {
        void Builder(T1 authoring1, T2 authoring2,T3 authoring3);
    }
    /// <summary>
    /// 动态注入标记接口,泛型版本,该接口需要实现Builder方法。当类型被注入后，Builder会被自动调用
    /// <code>public class TestScripts : ViewController,<see cref="IDynamicBuilder{T1,T2,T3,T4}"/>
    /// {
    ///     public void Builder(T1 authoring1,T2 authoring2,T3 authoring3,T4 authoring4)
    ///     {
    ///         //可进行实现
    ///     }
    /// }
    /// </code>
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    public interface IDynamicBuilder<T1, T2, T3, T4> : IDynamicBuilder
    {
        void Builder(T1 authoring1, T2 authoring2,T3 authoring3,T4 authoring4);
    }
    /// <summary>
    /// 动态注入标记接口,泛型版本,该接口需要实现Builder方法。当类型被注入后，Builder会被自动调用
    /// <code>public class TestScripts : ViewController,<see cref="IDynamicBuilder{T1,T2,T3,T4,T5}"/>
    /// {
    ///     public void Builder(T1 authoring1,T2 authoring2,T3 authoring3,T4 authoring4,T5 authoring5)
    ///     {
    ///         //可进行实现
    ///     }
    /// }
    /// </code>
    /// </summary>
    public interface IDynamicBuilder<T1, T2, T3,T4, T5> : IDynamicBuilder
    {
        void Builder(T1 authoring1, T2 authoring2, T3 authoring3, T4 authoring4,T5 authoring5);
    }
    /// <summary>
    /// 动态注入标记接口,泛型版本,该接口需要实现Builder方法。当类型被注入后，Builder会被自动调用
    /// <code>public class TestScripts : ViewController,<see cref="IDynamicBuilder{T1,T2,T3,T4,T5,T6}"/>
    /// {
    ///     public void Builder(T1 authoring1,T2 authoring2,T3 authoring3,T4 authoring4,T5 authoring5,T6 authoring6)
    ///     {
    ///         //可进行实现
    ///     }
    /// }
    /// </code>
    /// </summary>
    public interface IDynamicBuilder<T1, T2, T3, T4, T5,T6> : IDynamicBuilder
    {
        void Builder(T1 authoring1, T2 authoring2, T3 authoring3, T4 authoring4, T5 authoring5,T6 authoring6);
    }
    /// <summary>
    /// 动态注入标记接口,泛型版本,该接口需要实现Builder方法。当类型被注入后，Builder会被自动调用
    /// <code>public class TestScripts : ViewController,<see cref="IDynamicBuilder{T1,T2,T3,T4,T5,T6,T7}"/>
    /// {
    ///     public void Builder(T1 authoring1,T2 authoring2,T3 authoring3,T4 authoring4,T5 authoring5,T6 authoring6,T7 authoring7)
    ///     {
    ///         //可进行实现
    ///     }
    /// }
    /// </code>
    /// </summary>
    public interface IDynamicBuilder<T1, T2, T3, T4, T5,T6,T7> : IDynamicBuilder
    {
        void Builder(T1 authoring1, T2 authoring2, T3 authoring3, T4 authoring4, T5 authoring5,T6 authoring6,T7 authoring7);
    }

    /// <summary>
    /// 动态注入标记接口,泛型版本,该接口需要实现Builder方法。当类型被注入后，Builder会被自动调用
    /// <code>public class TestScripts : ViewController,<see cref="IDynamicBuilder{T1,T2,T3,T4,T5,T6,T7,T8}"/>
    /// {
    ///     public void Builder(T1 authoring1,T2 authoring2,T3 authoring3,T4 authoring4,T5 authoring5,T6 authoring6,T7 authoring7,T8 authoring8)
    ///     {
    ///         //可进行实现
    ///     }
    /// }
    /// </code>
    /// </summary>
    public interface IDynamicBuilder<T1, T2, T3, T4, T5, T6, T7, T8> : IDynamicBuilder
    {
        void Builder(T1 authoring1, T2 authoring2, T3 authoring3, T4 authoring4, T5 authoring5, T6 authoring6, T7 authoring7,T8 authoring8);
    }
    /// <summary>
    /// 动态注入标记接口,泛型版本,该接口需要实现Builder方法。当类型被注入后，Builder会被自动调用
    /// <code>public class TestScripts : ViewController,<see cref="IDynamicBuilder{T1,T2,T3,T4,T5,T6,T7,T8,T9}"/>
    /// {
    ///     public void Builder(T1 authoring1,T2 authoring2,T3 authoring3,T4 authoring4,T5 authoring5,T6 authoring6,T7 authoring7,T8 authoring8,T9 authoring9)
    ///     {
    ///         //可进行实现
    ///     }
    /// }
    /// </code>
    /// </summary>
    public interface IDynamicBuilder<T1, T2, T3, T4, T5, T6, T7, T8, T9> : IDynamicBuilder
    {
        void Builder(T1 authoring1, T2 authoring2, T3 authoring3, T4 authoring4, T5 authoring5, T6 authoring6, T7 authoring7, T8 authoring8,T9 authoring9);
    }

    
}
