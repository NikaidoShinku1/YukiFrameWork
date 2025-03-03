///=====================================================
/// - FileName:      ReddotKit.cs
/// - NameSpace:     YukiFrameWork.Reddot
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2025/2/19 14:26:51
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using YukiFrameWork.Events;
using YukiFrameWork.Extension;
namespace YukiFrameWork
{
	public struct ChangeReddotArg : IEventArgs
	{
		public string parent;
		public string path;
	}
	/// <summary>
	/// 红点套件
	/// </summary>
	public class ReddotKit
	{
		/// <summary>
		/// 注册红点变更的事件
		/// </summary>
		public static event Action onReddotPathChanged;

		/// <summary>
		/// 保存所有的红点路径
		/// <para>Key:Path Parent</para>
		/// <para>Value:All reddotPath</para>
		/// </summary>
		private static Dictionary<string, IList<string>> runtime_all_reddotPath;

		/// <summary>
		/// 获取某一组父级下所有的红点路径
		/// </summary>
		/// <param name="parent"></param>
		/// <returns></returns>
		public static IList<string> GetReddotPaths(string parent)
			=> runtime_all_reddotPath[parent];

		public static IReadOnlyDictionary<string, IList<string>> ReadOnlyReddotPath => runtime_all_reddotPath;

		static ReddotKit()
		{
			runtime_all_reddotPath = new Dictionary<string, IList<string>>();
		}

		/// <summary>
		/// 添加一个新的红点路径(包含子路径)。
		/// </summary>
		/// <param name="parent">红点系统的父路径</param>
		/// <param name="path">红点系统路径</param>
		public static void AddReddotPath(string parent, string path)
		{
			//父级是不能为空的
			if (parent.IsNullOrEmpty())
				return;

			if (runtime_all_reddotPath.TryGetValue(parent, out IList<string> paths))
			{
				if (paths.Contains(path))
				{
					LogKit.W($"红点路径{path}已经存在!");
					return;
				}				
			}
			else 
			{
				paths = new List<string>();
				runtime_all_reddotPath[parent] = paths;           
            }
            if (!path.IsNullOrEmpty())
				paths.Add(path);

			Refresh(parent,path);
			onReddotPathChanged?.Invoke();
        }

		/// <summary>
		/// 添加一个新的父级红点路径(此时路径为空)
		/// </summary>
		/// <param name="parent"></param>
		[Obsolete("方法已过时，请使用void AddReddotPath(string parent, string path)方法!")]
		public static void AddReddotPath(string parent)
		{
			AddReddotPath(parent, string.Empty);
		}

		/// <summary>
		/// 添加一组新的红点路径
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="paths"></param>
		public static void AddReddotPaths(string parent, IList<string> paths)
		{
			if (paths == null || paths.Count == 0)
			{				
				return;
			}
			foreach (var path in paths)
			{
				AddReddotPath(parent, path);
			}
		}

		private static void Refresh(string parent,string path)
		{
			EventManager.SendEvent<ChangeReddotArg>(new ChangeReddotArg() { parent = parent,path = path});
		}

		/// <summary>
		/// 移除某一个父级下所有的红点路径
		/// </summary>
		/// <param name="path"></param>
		public static void RemoveReddotPath(string parent)
		{
			RemoveReddotPathInternal(parent, string.Empty);
		}

		public static void RemoveReddotPath(string parent, string path)
		{
			RemoveReddotPathInternal(parent, path);
		}

		internal static void RemoveReddotPathInternal(string parent,string path)
		{
			//如果父级为空，则不处理，父级是不能为空的
			if (parent.IsNullOrEmpty()) return;

			//如果没有路径传递，说明移除该父级下所有的路径
			if (path.IsNullOrEmpty())
				runtime_all_reddotPath.Remove(parent);

			if (runtime_all_reddotPath.TryGetValue(parent, out var paths))
			{
				if (!paths.Contains(path))
					return;
				paths.Remove(path);
			}

			Refresh(parent,path);
			onReddotPathChanged?.Invoke();

		}

		/// <summary>
		/// 红点是否显示
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		internal static bool IsReddotActive(string parent,string path)
		{
			if (parent.IsNullOrEmpty()) return false;

			if (!runtime_all_reddotPath.TryGetValue(parent, out var paths))
			{
				return false;
			}
			else
			{
				if (paths.Count == 0)
					return false;
			}

			foreach (var item in paths)
			{				
				if (item == path)
					return true;
			}			
			//在拥有parent且path为空的情况下，默认标记为父级红点，会跟随打开
			return path.IsNullOrEmpty();
		}

		/// <summary>
		/// 红点路径持久化(Json字符串)
		/// </summary>
		public static string ReddotPersistence
		{
			get
			{
				return SerializationTool.SerializedObject(runtime_all_reddotPath);
			}
		}

		/// <summary>
		/// 通过持久化字符串加载红点路径
		/// </summary>
		public static void LoadPersistenceReddotPath(string persistence)
		{
			var reddotPaths = SerializationTool.DeserializedObject<Dictionary<string, IList<string>>>(persistence);
			if (reddotPaths == null)
				throw new NullReferenceException("读取数据为空,请检查字符串是否是正确的Json数据");

			foreach (var item in reddotPaths)
			{
				AddReddotPaths(item.Key,item.Value);
			}
		}
	}
}
