using Sirenix.OdinInspector;
///=====================================================
/// - FileName:      ItemType.cs
/// - NameSpace:     YukiFrameWork.Item
/// - Description:   物品的类型枚举
/// - Creation Time: 2024/4/20 19:36:30
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
namespace YukiFrameWork.Item
{
	public enum ItemType
	{
		[LabelText("消耗品")]
		Consumable,
		[LabelText("装备")]
		Equipment,
		[LabelText("材料")]
		Material,
		[LabelText("武器")]
		Weapon,		
	}	
}
