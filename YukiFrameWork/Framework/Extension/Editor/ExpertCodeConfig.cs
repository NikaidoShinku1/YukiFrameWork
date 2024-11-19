///=====================================================
/// - FileName:      ExpertCodeConfig.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/11/7 19:45:33
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Text;
using YukiFrameWork.Extension;


#if UNITY_EDITOR
using UnityEditor;
#endif
namespace YukiFrameWork
{
	public enum ParentType
	{		
		None,
		Architecture,
		Model,
		System,
		Utility,
        Controller,
        MonoBehaviour,
		YMonoBehaviour,
		ScriptableObject,		
	}

	public enum FieldLevel
	{
		Public,
		Private,
		Protected,
		Internal
	}

	[HideMonoScript]
	public class ExpertCodeConfig : ScriptableObject
	{
#if UNITY_EDITOR
        public static Dictionary<Type,string> typeAliases = new Dictionary<Type, string>
		{
		    { typeof(bool), "bool" },
		    { typeof(byte), "byte" },
		    { typeof(sbyte), "sbyte" },
		    { typeof(char), "char" },
		    { typeof(decimal), "decimal" },
		    { typeof(double), "double" },
		    { typeof(float), "float" },
		    { typeof(int), "int" },
		    { typeof(uint), "uint" },
		    { typeof(long), "long" },
		    { typeof(ulong), "ulong" },
		    { typeof(short), "short" },
		    { typeof(ushort), "ushort" },
		    { typeof(string), "string" },
		    { typeof(object), "object" }
		};
        [InitializeOnLoadMethod]
		static void Init()
		{
			ExpertCodeConfig config = Resources.Load<ExpertCodeConfig>(nameof(ExpertCodeConfig));

			if (!config)
			{
				config = YukiAssetDataBase.CreateScriptableAsset<ExpertCodeConfig>(nameof(ExpertCodeConfig),"Assets/Resources/" + nameof(ExpertCodeConfig) + ".asset");
			}
			Instance = config;
		}

	
		public static ExpertCodeConfig Instance;
		private static IEnumerable allType = null;
        public static IEnumerable AllType
        {
			get
			{
				if (!Instance) return null;

                try
                {
                    if (allType == null)
                    {
                        allType = Instance
                            .AssembliesContasts
                            .Select(x => Assembly.Load(x))
                            .SelectMany(x => x.GetTypes())
                            .Where(x => !Regex.IsMatch(x.ToString(), @"[^a-zA-Z0-9\.]"))
                            .Select(x => new ValueDropdownItem<string>()
                            {
                                Text = x.ToString(),
                                Value = x.ToString()
                            }).Core(x => x.Count());
                    }
                    return allType;


                }
                catch
                {
                    return null;
                }

            }
        }

#endif
        internal FrameworkConfigInfo configInfo => Resources.Load<FrameworkConfigInfo>(nameof(FrameworkConfigInfo));
        private void OnEnable()
        {
			NameSpace = configInfo?.nameSpace;
        }
        public const string tip = "高级脚本设置引用dll仅包含框架与FrameworkConfig中程序集设置添加的访问程序集,注入字段/方法类型仅为基础字段打造——————>不能是泛型/集合";	
		[LabelText("命名空间")]
        [HideInInspector]
        public string NameSpace = "YukiFrameWork.Example";
        [LabelText("文件路径"), FolderPath(AbsolutePath = true)]
        [HideInInspector]
        public string FoldPath = "Assets/Scripts";

        [LabelText("脚本文件名称"), HideInInspector]
        public string Name;

        [LabelText("脚本类型规则"),HideInInspector]
        public ParentType ParentType;

        [LabelText("规则以继承接口"),HideInInspector]
        public bool levelInterface;

		[HideInInspector]
		public bool customInterface;

		[HideInInspector]
		public bool IsScruct;

		[HideInInspector]
		public string architecture;

		[HideInInspector]
		public bool IsOpenExpertCode;

		[HideInInspector]
		public bool FoldExport;

		[HideInInspector]
		public string soFileName;
		[HideInInspector]
		public string soMenuName;

#if UNITY_EDITOR
		[ValueDropdown(nameof(AllAssemblies))]
#endif
		[InfoBox("程序集构造器，生成类型以存在于此的程序集相关。"),HorizontalGroup]
		public List<string> AssembliesContasts = new List<string>();
#if UNITY_EDITOR
		[ValueDropdown(nameof(AllType),IsUniqueList = true)]
#endif
		[InfoBox("以注入程序为主，添加字段可使用类型"),HorizontalGroup]
		public List<string> TypeContasts = new List<string>();
#if UNITY_EDITOR
		public static IEnumerable Types => Instance?.TypeContasts;
#endif
		public IEnumerable AllAssemblies => AppDomain.CurrentDomain.GetAssemblies().Select(x => new ValueDropdownItem<string>()
		{          
            Text = x.GetName().Name,
			Value = x.GetName().Name,
        });
		[TableList,LabelText("为类添加字段")]
        public List<FieldData> fieldDatas = new List<FieldData>();

		[TableList,LabelText("为类添加方法")]
        public List<MethodData> methodDatas = new List<MethodData>();

#if UNITY_EDITOR
		[FoldoutGroup("Script Code Colors")]
        [ShowInInspector]public static Color BackgroundColor = new Color(0.118f, 0.118f, 0.118f, 1f);
        [FoldoutGroup("Script Code Colors")]
        [ShowInInspector]public static Color TextColor = new Color(0.863f, 0.863f, 0.863f, 1f);
        [FoldoutGroup("Script Code Colors")]
        [ShowInInspector]public static Color KeywordColor = new Color(0.337f, 0.612f, 0.839f, 1f);
        [FoldoutGroup("Script Code Colors")]
        [ShowInInspector]public static Color IdentifierColor = new Color(0.306f, 0.788f, 0.69f, 1f);
        [FoldoutGroup("Script Code Colors")]
        [ShowInInspector]public static Color CommentColor = new Color(0.341f, 0.651f, 0.29f, 1f);
        [FoldoutGroup("Script Code Colors")]
        [ShowInInspector]public static Color LiteralColor = new Color(0.71f, 0.808f, 0.659f, 1f);
        [FoldoutGroup("Script Code Colors")]
        [ShowInInspector]public static Color StringLiteralColor = new Color(0.839f, 0.616f, 0.522f, 1f);      
#endif

    }

	[Serializable]
	public class FieldData
	{    
        public FieldLevel fieldLevel;
#if UNITY_EDITOR
		private IEnumerable e => ExpertCodeConfig.Types;
        [ValueDropdown(nameof(e))]
#endif
		public string fieldType;
        public string fieldName;
		[InfoBox("是否封装属性")]
		public bool WrapperAttribute = false;

		[LabelText("注释"),TextArea]
		public string description;
    }
	[Serializable]
	public class MethodData
	{		
		public FieldLevel methodLevel;
		public bool Void = true;
		[InfoBox("构建结构体，虚方法不会生效")]
		public bool Virtual = false;
		[HideIf(nameof(Void))]
#if UNITY_EDITOR
		private IEnumerable e => ExpertCodeConfig.Types;
		[ValueDropdown(nameof(e))]
#endif
		[HideIf(nameof(Void))]
		public string ReturnType;
		public string Name;
        [LabelText("注释"),TextArea]
        public string description;
    }


#if UNITY_EDITOR
	public class DrawCode : ICodeGenerator
    {
        StringBuilder builder = new StringBuilder();
        public StringBuilder BuildFile(params object[] arg)
		{			
			builder.Clear();
			ExpertCodeConfig config = arg[0] as ExpertCodeConfig;
			string commentColor = ColorUtility.ToHtmlStringRGBA(ExpertCodeConfig.CommentColor);
			string textColor = ColorUtility.ToHtmlStringRGBA(ExpertCodeConfig.TextColor);
			string keyWordColor = ColorUtility.ToHtmlStringRGBA(ExpertCodeConfig.KeywordColor);
			string IdentiferColor = ColorUtility.ToHtmlStringRGBA(ExpertCodeConfig.IdentifierColor);
			string LiteralColor = ColorUtility.ToHtmlStringRGBA(ExpertCodeConfig.LiteralColor);
			string stringLiteralColor = ColorUtility.ToHtmlStringRGBA(ExpertCodeConfig.StringLiteralColor);
			if (config.Name.IsNullOrEmpty()) return builder;
			builder.AppendLine($"<color=#{commentColor}>///=====================================================");
			builder.AppendLine($"/// - FileName:      " + config.Name + ".cs");
			builder.AppendLine($"/// - NameSpace:     " + config.NameSpace);
			builder.AppendLine($"/// - Description:   高级定制脚本生成");
			builder.AppendLine($"/// - Creation Time: " + System.DateTime.Now.ToString());
			builder.AppendLine($"/// -  (C) Copyright 2008 - 2024");
			builder.AppendLine($"/// -  All Rights Reserved.");
			builder.AppendLine($"///=====================================================</color>");

			builder.AppendLine($"<color=#{keyWordColor}>using</color> YukiFrameWork;");
			builder.AppendLine($"<color=#{keyWordColor}>using</color> UnityEngine;");
			builder.AppendLine($"<color=#{keyWordColor}>using</color> System;");
			if (!config.NameSpace.IsNullOrEmpty())
			{
				builder.AppendLine($"<color=#{keyWordColor}>namespace</color> {config.NameSpace}");
				builder.AppendLine("{");
			}
			string parentName = string.Empty;
			switch (config.ParentType)
			{
				case ParentType.None:
					break;
				case ParentType.Architecture:
					parentName = $"Architecture<{config.Name}>";
					break;
				case ParentType.Model:
					parentName = config.levelInterface ? nameof(IModel) : nameof(AbstractModel);
					break;
				case ParentType.System:
					parentName = config.levelInterface ? nameof(ISystem) : nameof(AbstractSystem);
					break;
				case ParentType.Utility:
					parentName = nameof(IUtility);
					break;
				case ParentType.Controller:
					parentName = config.levelInterface ? nameof(IController) : nameof(AbstractController);
					break;
				case ParentType.MonoBehaviour:
					parentName = nameof(MonoBehaviour);
					break;
				case ParentType.YMonoBehaviour:
					parentName = nameof(YMonoBehaviour);
					break;
				case ParentType.ScriptableObject:
					parentName = nameof(ScriptableObject);
					break;
			}
			bool inter = false;
            string className = config.Name.Substring(0, 1).ToUpper() + config.Name.Substring(1);
            if (config.ParentType == ParentType.Model || config.ParentType == ParentType.System || config.ParentType == ParentType.Utility)
			{
				if (config.customInterface)
				{
                    builder.AppendLine($"    <color=#{keyWordColor}>public interface</color> <color=#{LiteralColor}>I{className}</color> : <color=#{LiteralColor}>I{config.ParentType}</color>");
                    builder.AppendLine("    {");                  
					foreach (var field in config.fieldDatas)
					{
                        if (field.fieldType.IsNullOrEmpty() || field.fieldName.IsNullOrEmpty()) continue;
                        string name = field.fieldName.Substring(0, 1).ToUpper() + field.fieldName.Substring(1);
                        Type fieldType = AssemblyHelper.GetType(field.fieldType);
                        string type = ExpertCodeConfig.typeAliases.ContainsKey(fieldType) ? ExpertCodeConfig.typeAliases[fieldType] : field.fieldType;
                        if (field.WrapperAttribute)
                        {
                            builder.AppendLine($"        <color=#{keyWordColor}>public</color> <color=#{(ExpertCodeConfig.typeAliases.ContainsKey(fieldType) ? keyWordColor : IdentiferColor)}>{type}</color> {name}");
                            builder.AppendLine("        {");
                            builder.AppendLine($"            <color=#{keyWordColor}>get</color> => m{name};");
                            builder.AppendLine($"            <color=#{keyWordColor}>set</color> => m{name} = <color=#{keyWordColor}>value</color>;");
                            builder.AppendLine("        }");
                        }
                    }
                    builder.AppendLine("");
                    foreach (var method in config.methodDatas)
                    {
                        if (method.Name.IsNullOrEmpty() || (!method.Void && method.ReturnType.IsNullOrEmpty())) continue;
						if (method.methodLevel != FieldLevel.Public) continue;
                        string name = method.Name.Substring(0, 1).ToUpper() + method.Name.Substring(1);
                        string level = method.methodLevel.ToString().Substring(0).ToLower();
                        string type = method.ReturnType;
                        if (!method.Void)
                        {
                            Type methodType = AssemblyHelper.GetType(method.ReturnType);
                            type = ExpertCodeConfig.typeAliases.ContainsKey(methodType) ? ExpertCodeConfig.typeAliases[methodType] : method.ReturnType;
                        }
                        if (!method.description.IsNullOrEmpty())
                            builder.AppendLine($"        <color=#{commentColor}>//{method.description}</color>");
                        builder.AppendLine($"        {(method.Void ? $"<color=#{keyWordColor}>void" : $"<color=#{IdentiferColor}>" + type)}</color> {name}();");                      
                    }
                    builder.AppendLine("    }");
                    builder.AppendLine();
                    builder.AppendLine();
                    inter = true;
                }
			}
			
			bool structClass = config.levelInterface && (config.ParentType == ParentType.Model || config.ParentType == ParentType.System || config.ParentType == ParentType.Utility) && config.IsScruct;
            Type architectureType = AssemblyHelper.GetType(config.architecture);
			if (architectureType != null)
			{
				if (config.ParentType == ParentType.Model || config.ParentType == ParentType.System || config.ParentType == ParentType.Utility)
					builder.AppendLine($"    {(inter ? $"[<color=#{IdentiferColor}>Registration</color>(<color=#{keyWordColor}>typeof</color>(<color=#{IdentiferColor}>{architectureType}</color>),<color=#{keyWordColor}>typeof</color>(I<color=#{IdentiferColor}>{config.Name}</color>))]" : $"[<color=#{IdentiferColor}>Registration</color>(<color=#{keyWordColor}>typeof</color>(<color=#{IdentiferColor}>{architectureType}</color>))]")}");
				else if (config.ParentType == ParentType.Controller)
				{
					if (parentName != nameof(IController))
						builder.AppendLine($"    [<color=#{IdentiferColor}>InitController</color>]");
					builder.AppendLine($"    [<color=#{IdentiferColor}>RuntimeInitializeOnArchitecture(typeof({architectureType}),true)</color>]");

				}
			}
			if (config.ParentType == ParentType.ScriptableObject)
			{
				if(!config.soFileName.IsNullOrEmpty() && !config.soMenuName.IsNullOrEmpty())
				{
					Debug.Log(config.soFileName);
                    builder.AppendLine($"    [<color=#{IdentiferColor}>CreateAssetMenu(fileName =<color=#{stringLiteralColor}>\"{config.soFileName}\"</color>,menuName = <color=#{stringLiteralColor}>\"{config.soMenuName}\"</color>)</color>]");
                }
			}
            builder.AppendLine($"    <color=#{keyWordColor}>public {(structClass ? "struct" : "class")}</color> <color=#{IdentiferColor}>{className}</color>{(config.ParentType != ParentType.None ? " :" : "")} <color=#{IdentiferColor}>{parentName}</color>{(inter ? $",<color=#{LiteralColor}>I" + className + "</color>" : "")}");
			builder.AppendLine("    {");
			if (config.IsOpenExpertCode)
			{
				foreach (var field in config.fieldDatas)
				{
					if (field.fieldType.IsNullOrEmpty() || field.fieldName.IsNullOrEmpty()) continue;
					string name = field.fieldName.Substring(0, 1).ToUpper() + field.fieldName.Substring(1);
					string level = field.fieldLevel.ToString().Substring(0).ToLower();
					Type fieldType = AssemblyHelper.GetType(field.fieldType);
					string type = ExpertCodeConfig.typeAliases.ContainsKey(fieldType) ? ExpertCodeConfig.typeAliases[fieldType] : field.fieldType;
					if (!field.description.IsNullOrEmpty())
						builder.AppendLine($"        <color=#{commentColor}>//{field.description}</color>");
					builder.AppendLine($"        <color=#{keyWordColor}>{level} {type}</color> m{name};");
					if (field.WrapperAttribute)
					{
						builder.AppendLine($"        <color=#{keyWordColor}>public</color> <color=#{(ExpertCodeConfig.typeAliases.ContainsKey(fieldType) ? keyWordColor : IdentiferColor)}>{type}</color> {name}");
						builder.AppendLine("        {");
						builder.AppendLine($"            <color=#{keyWordColor}>get</color> => m{name};");
						builder.AppendLine($"            <color=#{keyWordColor}>set</color> => m{name} = <color=#{keyWordColor}>value</color>;");
						builder.AppendLine("        }");
					}
				}
			}
			builder.AppendLine();
			if (config.ParentType == ParentType.Architecture)
			{
				builder.AppendLine($"        <color=#{commentColor}>//可以填写默认进入的场景名称，在架构准备完成后，自动进入</color>");
                builder.AppendLine($"        <color=#{keyWordColor}>public override</color> (string, SceneLoadType) DefaultSceneName => default;");
				builder.AppendLine();
                builder.AppendLine($"        <color=#{keyWordColor}>public override void</color> OnInit()");
                builder.AppendLine("        {");
                builder.AppendLine("");
                builder.AppendLine("        }");
				builder.AppendLine($"        <color=#{commentColor}>//配表构建，通过ArchitectureTable可以在架构中缓存部分需要的资源,例如TextAssets ScriptableObject</color>");
				builder.AppendLine($"        <color=#{keyWordColor}>protected override</color> <color=#{IdentiferColor}>ArchitectureTable</color> BuildArchitectureTable() => default;");
				builder.AppendLine("");

            }
			else if (parentName.Contains(nameof(AbstractModel)) || parentName.Contains(nameof(AbstractSystem)))
			{
				builder.AppendLine($"        <color=#{keyWordColor}>public override void</color> Init()");
				builder.AppendLine("        {");
				builder.AppendLine("");
				builder.AppendLine("        }");
			}
			else if (parentName.Contains(nameof(AbstractController)))
			{
				builder.AppendLine($"        <color=#{keyWordColor}>public override void</color> OnInit()");
				builder.AppendLine("        {");
				builder.AppendLine("");
				builder.AppendLine("        }");
			}
			else if (config.ParentType == ParentType.Model
				|| config.ParentType == ParentType.System
				|| config.ParentType == ParentType.Utility
				|| config.ParentType == ParentType.Controller)
			{
				if (parentName.Contains(nameof(IModel)) || parentName.Contains(nameof(ISystem)))
				{
					builder.AppendLine($"        <color=#{keyWordColor}>public void</color> Init()");
					builder.AppendLine("        {");
					builder.AppendLine("");
					builder.AppendLine("        }");
					builder.AppendLine();
					builder.AppendLine($"        <color=#{keyWordColor}>public void</color> Destroy()");
					builder.AppendLine("        {");
					builder.AppendLine("");
					builder.AppendLine("        }");
				}
				if (config.ParentType != ParentType.Utility && config.levelInterface)
				{
					builder.AppendLine();
					builder.AppendLine($"        <color=#{IdentiferColor}>IArchitecture IGetArchitecture</color>.GetArchitecture()");
					builder.AppendLine("        {");
					builder.AppendLine($"            {(architectureType == null ? $"<color=#{keyWordColor}>return<color> null;" : $"<color=#{keyWordColor}>return</color> <color=#{IdentiferColor}>{architectureType}</color>.Global;")}");
					builder.AppendLine("        }");
					builder.AppendLine();
				}
				if (config.ParentType != ParentType.Controller && config.ParentType != ParentType.Utility && config.levelInterface)
				{
					builder.AppendLine($"        <color=#{keyWordColor}>void</color> <color=#{IdentiferColor}>ISetArchitecture</color>.SetArchitecture(<color=#{LiteralColor}>IArchitecture</color> architecture)");
					builder.AppendLine("        {");
					builder.AppendLine("");
					builder.AppendLine("        }");
				}
			}
			else if (config.ParentType == ParentType.MonoBehaviour || config.ParentType == ParentType.YMonoBehaviour)
			{
				builder.AppendLine($"        <color=#{keyWordColor}>void</color> Start()");
				builder.AppendLine("        {");
				builder.AppendLine("");
				builder.AppendLine("        }");
			}
			builder.AppendLine();
			if (config.IsOpenExpertCode)
			{
				foreach (var method in config.methodDatas)
				{
					if (method.Name.IsNullOrEmpty() || (!method.Void && method.ReturnType.IsNullOrEmpty())) continue;
					string name = method.Name.Substring(0, 1).ToUpper() + method.Name.Substring(1);
					string level = method.methodLevel.ToString().Substring(0).ToLower();
					string type = method.ReturnType;
					if (!method.Void)
					{
						Type methodType = AssemblyHelper.GetType(method.ReturnType);
						type = ExpertCodeConfig.typeAliases.ContainsKey(methodType) ? ExpertCodeConfig.typeAliases[methodType] : method.ReturnType;
					}
					if (!method.description.IsNullOrEmpty())
						builder.AppendLine($"        <color=#{commentColor}>//{method.description}</color>");
					builder.AppendLine($"        <color=#{keyWordColor}>{level}{(method.Virtual && !config.IsScruct ? " virtual" : "")}</color> {(method.Void ? $"<color=#{keyWordColor}>void" : $"<color=#{IdentiferColor}>" + type)}</color> {name}()");
					builder.AppendLine("        {");
					builder.AppendLine($"            {(method.Void ? string.Empty : $"<color=#{keyWordColor}>return default</color>;")}");
					builder.AppendLine("        }");					
				}
			}
            builder.AppendLine();
            builder.AppendLine("    }");

            if (!config.NameSpace.IsNullOrEmpty())
            {              
                builder.AppendLine("}");
            }
            return builder;
        }
    }
#endif
}
