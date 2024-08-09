///=====================================================
/// - FileName:      ActionGenerator.cs
/// - NameSpace:     YukiFrameWork.States
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/8/8 20:58:22
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
namespace YukiFrameWork.ActionStates
{
    public class ActionGenerator : ICodeGenerator
    {
        public StringBuilder BuildFile(params object[] arg)
        {
            string fileName = (string)arg[0];
            string nameSpace = (string)arg[1];
            int selectIndex = (int)arg[2];
            List<string> list = arg[3] as List<string>;
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("///=====================================================");
            builder.AppendLine("/// - FileName:      " + fileName + ".cs");
            builder.AppendLine("/// - NameSpace:     " + nameSpace);
            builder.AppendLine("/// - Description:   框架状态动作类创建");
            builder.AppendLine("/// - Creation Time: " + System.DateTime.Now.ToString());
            builder.AppendLine("/// -  (C) Copyright 2008 - 2024");
            builder.AppendLine("/// -  All Rights Reserved.");
            builder.AppendLine("///=====================================================");

            builder.AppendLine("using YukiFrameWork.ActionStates;");
            builder.AppendLine("using UnityEngine;");
            builder.AppendLine($"namespace {nameSpace}");
            builder.AppendLine("{");
            if (list.Count != 0)
                builder.AppendLine($"\t[RuntimeInitializeOnArchitecture(typeof({list[selectIndex]}),true)]");
            builder.AppendLine($"\tpublic class {fileName} : ActionBehaviour");
            builder.AppendLine("\t{");
            builder.AppendLine("\t\tpublic override void OnInit()");
            builder.AppendLine("\t\t{");
            builder.AppendLine("\t\t\tbase.OnInit();");
            builder.AppendLine("\t\t}");
            builder.AppendLine("\t\tpublic override void OnEnter(StateAction action)");
            builder.AppendLine("\t\t{");
            builder.AppendLine("\t\t\tbase.OnEnter(action);");
            builder.AppendLine("\t\t}");
            builder.AppendLine("\t\tpublic override void OnUpdate(StateAction action)");
            builder.AppendLine("\t\t{");
            builder.AppendLine("\t\t\tbase.OnUpdate(action);");
            builder.AppendLine("\t\t}");
            builder.AppendLine("\t\tpublic override void OnExit(StateAction action)");
            builder.AppendLine("\t\t{");
            builder.AppendLine("\t\t\tbase.OnExit(action);");
            builder.AppendLine("\t\t}");
            builder.AppendLine("\t\tpublic override void OnStop(StateAction action)");
            builder.AppendLine("\t\t{");
            builder.AppendLine("\t\t\tbase.OnStop(action);");
            builder.AppendLine("\t\t}");
            builder.AppendLine("\t\t#if UNITY_EDITOR || DEBUG || DEBUG");
            builder.AppendLine("\t\tpublic override bool OnInspectorGUI(State state)");
            builder.AppendLine("\t\t{");
            builder.AppendLine("\t\t\treturn false;//返回假: 绘制默认监视面板 | 返回真: 绘制扩展自定义监视面板");
            builder.AppendLine("\t\t}");
            builder.AppendLine("\t\t#endif");
            builder.AppendLine("");
            builder.AppendLine("\t}");

            builder.AppendLine("}");

            return builder;
        }
    }
}
