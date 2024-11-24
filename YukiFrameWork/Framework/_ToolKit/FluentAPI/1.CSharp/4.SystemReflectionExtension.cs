/****************************************************************************
 * Copyright (c) 2015 - 2022 liangxiegame UNDER MIT License
 * 
 * http://qframework.cn
 * https://github.com/liangxiegame/QFramework
 * https://gitee.com/liangxiegame/QFramework
 ****************************************************************************/


using System;
using System.Linq;
using System.Reflection;

namespace YukiFrameWork.QF
{
#if UNITY_EDITOR888
    [ClassAPI("01.FluentAPI.CSharp", "System.Reflection", 4)]
    [APIDescriptionCN("针对 System.Reflection 提供的链式扩展")]
    [APIDescriptionEN("Chain extension provided for System.Reflection")]
#endif
    public static class SystemReflectionExtension
    {
        // [UnityEditor.MenuItem("QF/Test")]
        // public static void Test()
        // {
        //         "/abc/e.txt".GetFileExtendName().LogInfo();
        // }

#if UNITY_EDITOR888
        // v1 No.32
        [MethodAPI]
        [APIDescriptionCN("通过反射的方式调用私有方法")]
        [APIDescriptionEN("call private method by reflection")]
        [APIExampleCode(@"
class A
{
    private void Say() { Debug.Log("I'm A!") }
}

new A().ReflectionCallPrivateMethod("Say");
// I'm A!
")]
#endif
        public static object ReflectionCallPrivateMethod<T>(this T self, string methodName, params object[] args)
        {
            var type = typeof(T);
            var methodInfo = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);

            return methodInfo?.Invoke(self, args);
        }

#if UNITY_EDITOR888
        // v1 No.33
        [MethodAPI]
        [APIDescriptionCN("通过反射的方式调用私有方法，有返回值")]
        [APIDescriptionEN("call private method by reflection,return the result")]
        [APIExampleCode(@"
class A
{
    private bool Add(int a,int b) { return a + b; }
}

Debug.Log(new A().ReflectionCallPrivateMethod("Add",1,2));
// 3
")]
#endif
        public static TReturnType ReflectionCallPrivateMethod<T, TReturnType>(this T self, string methodName,
            params object[] args)
        {
            return (TReturnType)self.ReflectionCallPrivateMethod(methodName, args);
        }


#if UNITY_EDITOR888
        // v1 No.34
        [MethodAPI]
        [APIDescriptionCN("检查是否有指定的 Attribute")]
        [APIDescriptionEN("Check whether the specified Attribute exists")]
        [APIExampleCode(@"
[DisplayName("A Class")
class A
{
    [DisplayName("A Number")
    public int Number;

    [DisplayName("Is Complete?")
    private bool Complete => Number > 100;

    [DisplayName("Say complete result?")
    public void SayComplete()
    {
        Debug.Log(Complete);
    }
}

var aType = typeof(A);
//
Debug.Log(aType.HasAttribute(typeof(DisplayNameAttribute));
// true
Debug.Log(aType.HasAttribute<DisplayNameAttribute>());
// true

// also support MethodInfo、PropertyInfo、FieldInfo
// 同时 也支持 MethodInfo、PropertyInfo、FieldInfo
")]
#endif
        public static bool HasAttribute<T>(this Type type, bool inherit = false) where T : Attribute
        {
            return type.GetCustomAttributes(typeof(T), inherit).Any();
        }

#if UNITY_EDITOR888
        [APIDescriptionCN("检查是否有指定的 Attribute")]
        [APIDescriptionEN("Check whether the specified Attribute exists")]
#endif
        public static bool HasAttribute(this Type type, Type attributeType, bool inherit = false)
        {
            return type.GetCustomAttributes(attributeType, inherit).Any();
        }
#if UNITY_EDITOR888
        [APIDescriptionCN("检查是否有指定的 Attribute")]
        [APIDescriptionEN("Check whether the specified Attribute exists")]
#endif
        public static bool HasAttribute<T>(this PropertyInfo prop, bool inherit = false) where T : Attribute
        {
            return prop.GetCustomAttributes(typeof(T), inherit).Any();
        }

#if UNITY_EDITOR888
        [APIDescriptionCN("检查是否有指定的 Attribute")]
        [APIDescriptionEN("Check whether the specified Attribute exists")]
#endif
        public static bool HasAttribute(this PropertyInfo prop, Type attributeType, bool inherit = false)
        {
            return prop.GetCustomAttributes(attributeType, inherit).Any();
        }

#if UNITY_EDITOR888
        [APIDescriptionCN("检查是否有指定的 Attribute")]
        [APIDescriptionEN("Check whether the specified Attribute exists")]
#endif
        public static bool HasAttribute<T>(this FieldInfo field, bool inherit = false) where T : Attribute
        {
            return field.GetCustomAttributes(typeof(T), inherit).Any();
        }

#if UNITY_EDITOR888
        [APIDescriptionCN("检查是否有指定的 Attribute")]
        [APIDescriptionEN("Check whether the specified Attribute exists")]
#endif
        public static bool HasAttribute(this FieldInfo field, Type attributeType, bool inherit)
        {
            return field.GetCustomAttributes(attributeType, inherit).Any();
        }

#if UNITY_EDITOR888
        [APIDescriptionCN("检查是否有指定的 Attribute")]
        [APIDescriptionEN("Check whether the specified Attribute exists")]
#endif
        public static bool HasAttribute<T>(this MethodInfo method, bool inherit = false) where T : Attribute
        {
            return method.GetCustomAttributes(typeof(T), inherit).Any();
        }

#if UNITY_EDITOR888
        [APIDescriptionCN("检查是否有指定的 Attribute")]
        [APIDescriptionEN("Check whether the specified Attribute exists")]
#endif
        public static bool HasAttribute(this MethodInfo method, Type attributeType, bool inherit = false)
        {
            return method.GetCustomAttributes(attributeType, inherit).Any();
        }


#if UNITY_EDITOR888
        // v1 No.35
        [MethodAPI]
        [APIDescriptionCN("获取指定的 Attribute")]
        [APIDescriptionEN("Gets the specified Attribute")]
        [APIExampleCode(@"
[DisplayName("A Class")
class A
{
    [DisplayName("A Number")
    public int Number;

    [DisplayName("Is Complete?")
    private bool Complete => Number > 100;

    [DisplayName("Say complete result?")
    public void SayComplete()
    {
        Debug.Log(Complete);
    }
}

var aType = typeof(A);
//
Debug.Log(aType.GetAttribute(typeof(DisplayNameAttribute));
// DisplayNameAttribute
Debug.Log(aType.GetAttribute<DisplayNameAttribute>());
// DisplayNameAttribute

// also support MethodInfo、PropertyInfo、FieldInfo
// 同时 也支持 MethodInfo、PropertyInfo、FieldInfo
")]
#endif
        public static T GetAttribute<T>(this Type type, bool inherit = false) where T : Attribute
        {
            return type.GetCustomAttributes<T>(inherit).FirstOrDefault();
        }

#if UNITY_EDITOR888
        [APIDescriptionCN("获取指定的 Attribute")]
        [APIDescriptionEN("Gets the specified Attribute")]
#endif
        public static object GetAttribute(this Type type, Type attributeType, bool inherit = false)
        {
            return type.GetCustomAttributes(attributeType, inherit).FirstOrDefault();
        }

#if UNITY_EDITOR888
        [APIDescriptionCN("获取指定的 Attribute")]
        [APIDescriptionEN("Gets the specified Attribute")]
#endif
        public static T GetAttribute<T>(this MethodInfo method, bool inherit = false) where T : Attribute
        {
            return method.GetCustomAttributes<T>(inherit).FirstOrDefault();
        }

#if UNITY_EDITOR888
        [APIDescriptionCN("获取指定的 Attribute")]
        [APIDescriptionEN("Gets the specified Attribute")]
#endif
        public static object GetAttribute(this MethodInfo method, Type attributeType, bool inherit = false)
        {
            return method.GetCustomAttributes(attributeType, inherit).FirstOrDefault();
        }

#if UNITY_EDITOR888
        [APIDescriptionCN("获取指定的 Attribute")]
        [APIDescriptionEN("Gets the specified Attribute")]
#endif
        public static T GetAttribute<T>(this FieldInfo field, bool inherit = false) where T : Attribute
        {
            return field.GetCustomAttributes<T>(inherit).FirstOrDefault();
        }

#if UNITY_EDITOR888
        [APIDescriptionCN("获取指定的 Attribute")]
        [APIDescriptionEN("Gets the specified Attribute")]
#endif
        public static object GetAttribute(this FieldInfo field, Type attributeType, bool inherit = false)
        {
            return field.GetCustomAttributes(attributeType, inherit).FirstOrDefault();
        }

#if UNITY_EDITOR888
        [APIDescriptionCN("获取指定的 Attribute")]
        [APIDescriptionEN("Gets the specified Attribute")]
#endif
        public static T GetAttribute<T>(this PropertyInfo prop, bool inherit = false) where T : Attribute
        {
            return prop.GetCustomAttributes<T>(inherit).FirstOrDefault();
        }

#if UNITY_EDITOR888
        [APIDescriptionCN("获取指定的 Attribute")]
        [APIDescriptionEN("Gets the specified Attribute")]
#endif
        public static object GetAttribute(this PropertyInfo prop, Type attributeType, bool inherit = false)
        {
            return prop.GetCustomAttributes(attributeType, inherit).FirstOrDefault();
        }
    }
}