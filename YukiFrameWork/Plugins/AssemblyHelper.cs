///=====================================================
/// - FileName:      AssemblyHelper.cs
/// - NameSpace:     YukiFrameWork.Project
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   这是一个框架工具创建的脚本
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;
namespace YukiFrameWork.Extension
{
    public class AssemblyHelper
    {
        private static Dictionary<string, Type> typeDict = new Dictionary<string, Type>();
        public static Type GetType(string typeName)
        {
            if (typeDict.TryGetValue(typeName, out var type))
            {
                return type;
            }
            else
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                for (int j = 0; j < assemblies.Length; j++)
                {
                    Type[] types = assemblies[j].GetTypes();
                    for (int i = 0; i < types.Length; i++)
                    {
                        if (((!string.IsNullOrEmpty(types[i].Namespace) ? (types[i].Namespace + ".") : (string.Empty)) + types[i].Name).Equals(typeName))
                        {
                            typeDict.Add(typeName, types[i]);
                            return types[i];
                        }
                    }
                }
            }
            return null;
        }

        public static Type[] GetTypes(Type type)
        {
            return Assembly.GetAssembly(type).GetTypes();
        }       

        public static Type[] GetTypes(Assembly assembly)
        {
            return assembly.GetTypes();
        }
    }
}