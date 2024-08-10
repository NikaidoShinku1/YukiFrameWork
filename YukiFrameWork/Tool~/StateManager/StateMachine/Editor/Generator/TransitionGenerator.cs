///=====================================================
/// - FileName:      TransitionGenerator.cs
/// - NameSpace:     YukiFrameWork.States
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/8/8 20:58:32
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
    public class TransitionGenerator : ICodeGenerator
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
            builder.AppendLine("/// - Description:   框架状态连接类创建");
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
            builder.AppendLine($"\tpublic class {fileName} : TransitionBehaviour");
            builder.AppendLine("\t{");
            builder.AppendLine("\t\tpublic override void OnInit()");
            builder.AppendLine("\t\t{");
            builder.AppendLine("\t\t\tbase.OnInit();");
            builder.AppendLine("\t\t}");           
            builder.AppendLine("\t\tpublic override void OnUpdate(ref bool isEnterNextState)");
            builder.AppendLine("\t\t{");
            builder.AppendLine("\t\t\tDebug.Log(\"当连接行为更新\");");
            builder.AppendLine("\t\t}");    
            builder.AppendLine("\t\tpublic override void OnDestroy()");
            builder.AppendLine("\t\t{");
            builder.AppendLine("\t\t\tDebug.Log(\"当移除组件\");");
            builder.AppendLine("\t\t}");
            builder.AppendLine("");
            builder.AppendLine("");
            builder.AppendLine("\t\t#if UNITY_EDITOR || DEBUG || DEBUG");
            builder.AppendLine("\t\tpublic override bool OnInspectorGUI(State state)");
            builder.AppendLine("\t\t{");
            builder.AppendLine("\t\t\treturn false; //默认返回False，如果需要自定义编辑器拓展则在这里写逻辑后返回True即可;");
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