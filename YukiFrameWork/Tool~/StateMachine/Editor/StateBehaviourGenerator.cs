///=====================================================
/// - FileName:      StateBehaviourGenerators.cs
/// - NameSpace:     YukiFrameWork.States
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/8/8 20:58:11
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
#if UNITY_EDITOR
using YukiFrameWork;
using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
namespace YukiFrameWork.Machine
{
    public class StateBehaviourGenerator : ICodeGenerator
    {
        public StringBuilder BuildFile(params object[] arg)
        {
            string fileName = "用于替换提示类";
            string nameSpace = "框架配置没有设置命名空间字符串已生成需要自己替换的默认命名空间";
            if (arg == null || arg.Length == 0)
            {
                FrameworkConfigInfo configInfo = Resources.Load<FrameworkConfigInfo>(nameof(FrameworkConfigInfo));             
                if (!configInfo.nameSpace.IsNullOrEmpty())
                {
                    nameSpace = configInfo.nameSpace;
                }
            }
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("///=====================================================");
            builder.AppendLine("/// - FileName:      " + fileName + ".cs");
            builder.AppendLine("/// - NameSpace:     " + nameSpace);
            builder.AppendLine("/// - Description:   YUKI 有限状态机构建状态类");
            builder.AppendLine("/// - Creation Time: " + System.DateTime.Now.ToString());
            builder.AppendLine("/// -  (C) Copyright 2008 - 2025");
            builder.AppendLine("/// -  All Rights Reserved.");
            builder.AppendLine("///=====================================================");

            builder.AppendLine("using YukiFrameWork.Machine;");
            builder.AppendLine("using UnityEngine;");
            builder.AppendLine("using YukiFrameWork;");
            builder.AppendLine($"namespace {nameSpace}");
            builder.AppendLine("{");           
            builder.AppendLine($"\tpublic class {fileName} : StateBehaviour");
            builder.AppendLine("\t{");
            builder.AppendLine("\t\tpublic override void OnInit()");
            builder.AppendLine("\t\t{");
            builder.AppendLine("\t\t\tDebug.Log(\"当状态初始化\");");
            builder.AppendLine("\t\t}");
            builder.AppendLine("\t\tpublic override void OnEnter()");
            builder.AppendLine("\t\t{");
            builder.AppendLine("\t\t\tDebug.Log(\"当进入状态\");");
            builder.AppendLine("\t\t}");
            builder.AppendLine("\t\tpublic override void OnUpdate()");
            builder.AppendLine("\t\t{");
            builder.AppendLine("\t\t\tDebug.Log(\"状态每帧更新\");");
            builder.AppendLine("\t\t}");
            builder.AppendLine("\t\tpublic override void OnExit()");
            builder.AppendLine("\t\t{");
            builder.AppendLine("\t\t\tDebug.Log(\"当退出状态\");");
            builder.AppendLine("\t\t}");            
            builder.AppendLine("");
            builder.AppendLine("\t}");

            builder.AppendLine("}");

            return builder;
        }
    }
}
#endif