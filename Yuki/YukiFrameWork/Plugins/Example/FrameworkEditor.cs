using System.IO;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
namespace YukiFrameWork.Editors
{
    [System.Serializable]
    public class FrameworkEditor : Object
    {
        private static string frameworkScriptExample = "C#FrameScripts.txt";
        private static string scriptExample = "C#Scripts.txt";
        private static string monoScriptExample = "C#MonoScripts.txt";
        private static string uipanelScriptExample = "UIPanelScripts.txt";
        private static string statebehaviourExample = "StateBehaviourScripts.txt";
        private static string examplePath = @"Assets\YukiFrameWork\Plugins\Example\Example\";
       
        //private const string AuthorName = "Yuki";

        //private const string AuthorEmail = "Yuki@qq.com";

        //private const string DataFormat = "yyyy/MM/dd HH:mm:ss";
        [MenuItem("Assets/YukiFrameWork代码工具/C# Scripts", false,-1000)]
        private static void OnCreateScript()
        {
            Create(examplePath + scriptExample);         
        }

        [MenuItem("Assets/YukiFrameWork代码工具/C# Mono Scripts", false, -1000)]
        private static void OnCreateMonoScript()
        {
            Create(examplePath + monoScriptExample);
           
        }

        [MenuItem("Assets/YukiFrameWork代码工具/Inherit Scope Scripts", false, -1000)]
        private static void OnCreateFrameWorkScripts()
        {
            Create(examplePath + frameworkScriptExample);         
        }

        [MenuItem("Assets/YukiFrameWork代码工具/UIPanel Scripts", false, -1000)]
        private static void OnCreateUIPanelScripts()
        {
            Create(examplePath + uipanelScriptExample);
        }      

        [MenuItem("Assets/YukiFrameWork代码工具/StateBehaviour Scripts", false, -1000)]
        private static void OnCreateStateBehaviourScripts()
        {
            Create(examplePath + statebehaviourExample);
        }      
        private static void Create(string targetPath)
        {
            string name = "YukiFrameWorkScript.cs";
            string scriptPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            Debug.Log($"当前将在{scriptPath}下生成代码");
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(targetPath, name);
            AssetDatabase.Refresh();      
        }

       /* private static string AssetsMenu()
        {
            object[] mouseData = Selection.objects;
            if (mouseData.Length > 0)
            {
                var normalPath = AssetDatabase.GetAssetPath(mouseData[0] as Object);
                var spile = normalPath.Split('/');
                foreach (var s in spile)
                {
                    if (s == "Scripts")
                    {
                        string path = AssetDatabase.GetAssetPath(mouseData[0] as Object);
                        Debug.Log($"当前将在{path}下生成代码");
                        return path;
                    }
                }
            }
            Debug.LogError($"当前并没有选定打开Scripts文件夹下的任何路径！");
            return string.Empty;
        }*/

        /// <summary>
        /// 创建信息类(暂不需要)
        /// </summary>
        /// <param name="path"></param>
        /*
        private static void OnCreateAssets(string path)
        {
            string allText = File.ReadAllText(path);

            allText = allText.Replace("#AuthorName#", AuthorName);
            allText = allText.Replace("#AuthorEmail#", AuthorEmail);
            allText = allText.Replace("#CreateTime#", System.DateTime.Now.ToString(DataFormat));
            File.WriteAllText(path, allText);
            //AssetDatabase.Refresh();
        }*/
    }
}
#endif