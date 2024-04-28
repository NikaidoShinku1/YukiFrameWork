///=====================================================
/// - FileName:      IItemKitLoader.cs
/// - NameSpace:     YukiFrameWork.Item
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/26 21:06:33
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.Item
{
	public interface IItemKitLoader
	{
		ItemDataBase LoadItemDataBase(string dataBaseName);

        void LoadItemDataBaseAsync(string dataBaseName,Action<ItemDataBase> callBack);
    }
}
