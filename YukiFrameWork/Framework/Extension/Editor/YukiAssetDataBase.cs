///=====================================================
/// - FileName:      EditorExtension.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/10/25 13:58:52
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Linq;
using System.IO;


#if UNITY_EDITOR
using UnityEditor;
namespace YukiFrameWork
{
	public interface IAssetLoader
	{
		Type assetType { get; }

	}
	public static class YukiAssetDataBase
	{
        /// <summary>
        /// 根据GUID查找所有对应的资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T[] FindAssets<T>() where T : UnityEngine.Object
		{			
			string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
			return guids
				.Select(g => AssetDatabase.GUIDToAssetPath(g))
				.Select(AssetDatabase.LoadAssetAtPath<T>).ToArray();
		}

		/// <summary>
		/// 根据GUID查找所有对应的资源
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static UnityEngine.Object[] FindAssets(Type type)
		{		
            string[] guids = AssetDatabase.FindAssets($"t:{type.Name}");
            return guids
                .Select(g => AssetDatabase.GUIDToAssetPath(g))
                .Select(x => AssetDatabase.LoadAssetAtPath(x,type)).ToArray();
        }

		public static T CreateScriptableAsset<T>(string assetName,string assetPath) where T : ScriptableObject
		{
			T asset = ScriptableObject.CreateInstance<T>();

			asset.name = assetName;

			AssetDatabase.CreateAsset(asset, assetPath);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			return asset;
		}

        public static void CreateFolder(string assetName, string assetPath) 
        {
			AssetDatabase.CreateFolder(assetName, assetPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
         
        }

		/// <summary>
		/// 获取资产的文件夹路径，单独的文件名称，完整的路径
		/// </summary>
		/// <param name="asset"></param>
		/// <param name="directoryName"></param>
		/// <param name="assetName"></param>
		/// <param name="allpath"></param>
		public static void GetAssetPath(UnityEngine.Object asset, out string directoryName,out string assetName, out string allpath)
		{
			string path = AssetDatabase.GetAssetPath(asset);
			
			directoryName = Path.GetDirectoryName(path);

			assetName = Path.GetFileNameWithoutExtension(path);
			allpath = path;
		}

		public static T GUIDToInstance<T>(string guid) where T : UnityEngine.Object
        {
			return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
		}

		public static string InstanceToGUID<T>(T item) where T : UnityEngine.Object
		{
			return AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(item));
		}

		/// <summary>
		/// 通过资产名称加载对象，可取代AssetDataBase.LoadAssetAtPath,但注意，资源必须保持名称唯一
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		public static T LoadAssetAtPathName<T>(string name) where T : UnityEngine.Object
		{
			return LoadAssetAtPathName(name,typeof(T)) as T;
		}

        /// <summary>
        /// 通过资产名称加载对象，可取代AssetDataBase.LoadAssetAtPath,但注意，资源必须保持名称唯一
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        public static UnityEngine.Object LoadAssetAtPathName(string name,Type type)
        {
			var assets = FindAssets(type);

			for (int i = 0; i < assets.Length; i++)
			{
				if (assets[i].name == name)
					return assets[i];
			}

			return null;
        }

		public static void Open(this UnityEngine.Object asset)
		{			
			AssetDatabase.OpenAsset(asset);
		}

    }
}
#endif