///=====================================================
/// - FileName:      Example.cs
/// - NameSpace:     YukiFrameWork.ExampleRold
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/5/9 23:31:59
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System.IO;
using Sirenix.OdinInspector;
using YukiFrameWork.Extension;
using YukiFrameWork.ExampleRule.ExampleFrameWork;
using System;
using System.Collections.Generic;



#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
#endif
namespace YukiFrameWork.ExampleRule
{	
	public class Example : ScriptableObject
	{
		internal enum Rule
		{
			Architecture,
			Model,
			System,
            Utility,
            Command,
			Controller
		}
#if UNITY_EDITOR
        private static string exampleTextPath => $"{ImportSettingWindow.packagePath}/Framework/Abstract/Example/Text/";

		private static string userPath => $"{ImportSettingWindow.packagePath}/Framework/Abstract/Example/Rule/User.cs";

		private static string userModelPath => $"{ImportSettingWindow.packagePath}/Framework/Abstract/Example/Rule/UserModel.cs";

        private static string userSystemPath => $"{ImportSettingWindow.packagePath}/Framework/Abstract/Example/Rule/UserSystem.cs";

        private static string userControllerPath => $"{ImportSettingWindow.packagePath}/Framework/Abstract/Example/Rule/UserController.cs";

        private static string userUtilityPath => $"{ImportSettingWindow.packagePath}/Framework/Abstract/Example/Rule/UserUtility.cs";

        private static string userCommandPath => $"{ImportSettingWindow.packagePath}/Framework/Abstract/Example/Rule/UserCommand.cs";
#endif
        internal static string UserEventKey => "UserData";

        [SerializeField,EnumToggleButtons(),LabelText("规则分布")]
		internal Rule rule;
		
		[SerializeField,LabelText("测试玩家："),ReadOnly,DisplayAsString,Required]
		internal UserController user;
      
#if UNITY_EDITOR
		[SerializeField,ReadOnly, ShowIf(nameof(rule), Rule.Architecture)]
		internal bool IsModel;

        [SerializeField, ReadOnly, ShowIf(nameof(rule), Rule.Architecture)]
        internal bool IsSystem;
      
        [Button("注册UserModel"), ShowIf(nameof(rule), Rule.Architecture), HorizontalGroup("UserModelExample"), PropertySpace(10)]
        void RegisterModel()
        {          
            if (IsSystem)
                Instanll("UserAll");
            else
                Instanll("User1");
            IsModel = true;
        }

        [Button("注销UserModel"), ShowIf(nameof(rule), Rule.Architecture), HorizontalGroup("UserModelExample"), PropertySpace(10)]
        void RemoveModel()
        {
            if (IsSystem)
                Instanll("User2");
            else Instanll("User");

            IsModel = false;
        }

        [Button("注册UserSystem"), ShowIf(nameof(rule), Rule.Architecture), HorizontalGroup("UserSystemExample"), PropertySpace(10)]
        void RegisterSystem()
        {
            if (IsModel)
                Instanll("UserAll");
            else Instanll("User2");
            IsSystem = true;
        }

        void Instanll(string name)
        {
            TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(exampleTextPath + name + ".txt");
            File.WriteAllText(userPath, textAsset.text);
            AssetDatabase.Refresh();
        }
    
        [Button("注销UserSystem"), ShowIf(nameof(rule), Rule.Architecture), HorizontalGroup("UserSystemExample"), PropertySpace(10)]
        void RemoveSystem()
        {
            if (IsModel)
                Instanll("User1");
            else Instanll("User");

            IsSystem = false;
        }

        [InitializeOnLoadMethod]
		static void InitExample()
		{			
			string path = $"{ImportSettingWindow.packagePath}/Framework/Abstract/Example/Example.asset";
			var example = AssetDatabase.LoadAssetAtPath<Example>($"{ImportSettingWindow.packagePath}/Framework/Abstract/Example/Example.asset");
			if (example == null)
			{
				example = ScriptableObject.CreateInstance<Example>();
                AssetDatabase.CreateAsset(example, path);
			}
			example.user = AssetDatabase.LoadAssetAtPath<UserController>($"{ImportSettingWindow.packagePath}/Framework/Abstract/Example/User.prefab");
        }

        [Button(), ShowIf(nameof(rule), Rule.System)]
        void SendEvent() => user.GetSystem<UserSystem>()?.SendEvent<EventArgs>(UserEventKey);

        [Button(), ShowIf(nameof(rule), Rule.Controller)]
        void SendCommand() => user.SendCommand();
        [Button(), ShowIf(nameof(rule), Rule.Controller)]
        void GetUserModel() => user.GetUserModel();
        [Button(), ShowIf(nameof(rule), Rule.Controller)]
        void GetUserSystem() => user.GetUserSystem();
        [Button(), ShowIf(nameof(rule), Rule.Controller)]
        void GetUserUtility() => user.GetUserUtility();
#endif

#if UNITY_EDITOR
		
		[OnInspectorGUI]
		void OnInspectorGUI()
		{
            GUILayout.Space(10);
            string targetPath = string.Empty;
            switch (rule)
			{
				case Rule.Architecture:
                    targetPath = userPath;
                    EditorGUILayout.HelpBox("框架的核心架构类，用于对层级/项目的初始化以及不同层的注册",MessageType.Info);
					break;
				case Rule.Model:
                    targetPath = userModelPath;
                    EditorGUILayout.HelpBox("框架的Model层，用于处理数据的增删改查，在默认规则下可以进行对事件的发送", MessageType.Info);
                    break;
				case Rule.System:
                    targetPath = userSystemPath;
                    EditorGUILayout.HelpBox("框架的System层,System层本质属于对Controller的压力释放，处理一部分Controller的逻辑/或者实现对系统的编写：任务系统，寻路系统，动作系统等。\n默认规则下可以获得其他System，发送事件，注册事件。获得Model层", MessageType.Info);
                    break;
				case Rule.Controller:
                    targetPath = userControllerPath;
                    EditorGUILayout.HelpBox("框架的控制器类，实际意义上拥有完全控制权，架构的标记，逻辑的处理，属于层级中的最高层。默认规则下，可以获得Model层，System层，发送Command(专属Command层),注册事件", MessageType.Info);
                    break;
                case Rule.Command:
                    EditorGUILayout.HelpBox("框架的命令层，完全遵守设计模式中的命令模式，将逻辑封装成对象，命令仅Controller可作为发送者。命令可以获取除Controller以外所有的层级对象以及事件的处理。", MessageType.Info);
                    targetPath = userCommandPath;
                    break;
                case Rule.Utility:
                    targetPath = userUtilityPath;
                    EditorGUILayout.HelpBox("框架的Utility层，该层默认情况下仅作为API封装，什么都没有，上述层级都可以直接对Utility进行获取", MessageType.Info);
                    break;
			}

			
            GUILayout.Space(20);
            GUILayout.Label("代码预览");

            MonoScript monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(targetPath);
            GUILayout.Space(15);
            var rect = EditorGUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label("Assembly Information");
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("FileName");
            GUILayout.Label(monoScript.GetClass().Assembly.GetName().Name + ".dll");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();
            GUILayout.TextArea(monoScript.text, "FrameBox");
            EditorGUILayout.EndVertical();

        }
#endif
    }

}
