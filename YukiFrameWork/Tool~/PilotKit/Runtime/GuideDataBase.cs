///=====================================================
/// - FileName:      GuideDataBase.cs
/// - NameSpace:     YukiFrameWork.Pilot
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/8/19 17:12:53
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using YukiFrameWork.Extension;
using UnityEditor;
namespace YukiFrameWork.Pilot
{
	[CreateAssetMenu(fileName = nameof(GuideDataBase),menuName = "YukiFrameWork/GuideDataBase")]
	public class GuideDataBase : ScriptableObject
	{
		[SerializeField,LabelText("保存所有的引导信息"),ListDrawerSettings(CustomAddFunction = "AddInfos")]
		internal GuideInfo[] infos;

		internal ValueDropdownList<string> typeInfos = new ValueDropdownList<string>();
        private void OnEnable() 
        {
			typeInfos.Clear();

			FrameworkConfigInfo info = Resources.Load<FrameworkConfigInfo>(nameof(FrameworkConfigInfo));

			try
			{
				Load(Assembly.Load(info.assembly).GetTypes());

				for (int i = 0; i < info.assemblies.Length; i++)
				{
					Load(Assembly.Load(info.assemblies[i]).GetTypes());
				}

				Load(Assembly.Load("PilotKit").GetTypes());
			}
			catch
			{ }
        }

		void Load(Type[] types)
		{
			IEnumerable<Type> currents = types.Where(x => x.IsSubclassOf(typeof(GuideBase)) && !x.IsAbstract);
			foreach (var item in currents)
			{
				typeInfos.Add(item.Name,item.ToString());
			}
		}

        GuideInfo AddInfos()
		{
			GuideInfo info = new GuideInfo();
			info.dataBase = this;
			return info;
		}
#if UNITY_EDITOR
		[Button("将配置导出json配表")]
		void CreateFile(string filePath = "Assets/GuideData",string fileName = "guideData")
		{
			SerializationTool
				.SerializedObject(infos, settings: 
				new Newtonsoft.Json.JsonSerializerSettings() { NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore })
				.CreateFileStream(filePath,fileName,".json");
			
		}

		[Button("将配置导入")]
		void ImportFile(TextAsset textAsset) 
		{
			if (textAsset == null) return;

			string text = textAsset.text;

			if (text.IsNullOrEmpty()) return;
			infos = SerializationTool.DeserializedObject<GuideInfo[]>(text);

			if (infos == null) return;

			foreach (var item in infos)
			{
				item.guideBaseType = string.Empty;
				item.dataBase = this;
			}

			OnEnable();
		}
#endif

	}
}
