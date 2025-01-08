using System;
using System.Collections.Generic; 
using System.Reflection;

namespace XFABManager
{

    /// <summary>
    /// 程序集工具
    /// </summary>
    internal class AssemblyTools
    {

        private static Dictionary<string, Type> typeCaches = new Dictionary<string, Type>();

        /// <summary>
        /// 根据名称来获取类型
        /// </summary>
        /// <param name="classFullName">类名(含命名空间)</param>
        /// <returns></returns>
        internal static Type GetType(string classFullName)
        {  
            if (string.IsNullOrEmpty(classFullName)) return null;

            if (typeCaches.ContainsKey(classFullName) && typeCaches[classFullName] != null) return typeCaches[classFullName];

            typeCaches.Remove(classFullName);

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var item in assemblies)
            {
                Type type = item.GetType(classFullName);
                if (type != null) 
                {
                    try
                    {
                        typeCaches.Add(classFullName, type);
                    }
                    catch (Exception)
                    {
                    }
                    break;
                } 
            }

            if (typeCaches.ContainsKey(classFullName))
                return typeCaches[classFullName];


            return null;
        }

    }

}

