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
namespace YukiFrameWork.ActionStates
{
    public class StateBehaviourGenerator : ICodeGenerator
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
            builder.AppendLine("/// - Description:   框架状态类创建");
            builder.AppendLine("/// - Creation Time: " + System.DateTime.Now.ToString());
            builder.AppendLine("/// -  (C) Copyright 2008 - 2024");
            builder.AppendLine("/// -  All Rights Reserved.");
            builder.AppendLine("///=====================================================");

            builder.AppendLine("using YukiFrameWork.ActionStates;");
            builder.AppendLine("using UnityEngine;");
            builder.AppendLine("using YukiFrameWork;");
            builder.AppendLine($"namespace {nameSpace}");
            builder.AppendLine("{");
            if (list.Count != 0)
                builder.AppendLine($"\t[RuntimeInitializeOnArchitecture(typeof({list[selectIndex]}),true)]");
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
            builder.AppendLine("\t\tpublic override void OnStop()");
            builder.AppendLine("\t\t{");
            builder.AppendLine("\t\t\tDebug.Log(\"当停止动作 : 当动作不使用动画循环时, 动画时间到达100%后调用状态\");;");
            builder.AppendLine("\t\t}");
            builder.AppendLine("\t\tpublic override void OnActionExit()");
            builder.AppendLine("\t\t{");
            builder.AppendLine("\t\t\tDebug.Log(\"当动作处于循环模式时, 子动作动画每次结束都会调用一次\");");
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
#endif