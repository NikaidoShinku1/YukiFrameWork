///=====================================================
/// - FileName:      EquipmentData.cs
/// - NameSpace:     RPG
/// - Description:   高级定制脚本生成
/// - Creation Time: 12/12/2025 12:33:59 PM
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using Newtonsoft.Json;
using UnityEditor;
using YukiFrameWork.Extension;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
namespace YukiFrameWork.Equips
{  
    public interface IEquipmentData
    {
        /// <summary>
        /// 唯一标识
        /// </summary>
        string Key { get; set; }

        /// <summary>
        /// 装备名称
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 介绍
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// 精灵
        /// </summary>
        Sprite Icon { get; set; }

        /// <summary>
        /// 装备脚本类型
        /// </summary>
        string EquipmentType { get; set; }           
        /// <summary>
        /// 装备的参数
        /// </summary>
        IReadOnlyList<EquipParam> EquipParams { get; }
    }
    [Serializable]
    public class EquipmentData : IEquipmentData
    {
        [LabelText("唯一标识"),SerializeField,JsonProperty]
        private string key;
        [LabelText("装备名称"),SerializeField,JsonProperty]
        private string name;
        [LabelText("介绍"),SerializeField,JsonProperty]
        [TextArea]
        private string description;
#if UNITY_EDITOR
        [CustomValueDrawer(nameof(DrawPreview))]
#endif
        [SerializeField,JsonProperty]
        private Sprite icon;
        [SerializeField,JsonProperty,LabelText("装备参数")]
        private List<EquipParam> equipParams = new List<EquipParam>();
        [SerializeField,JsonProperty,LabelText("装备脚本类型"),ValueDropdown(nameof(AllEquipmentsType))]
        private string equipmentType;

        [JsonIgnore,ExcelIgnore]public string Name { get => name; set => name = value; }
        [JsonIgnore,ExcelIgnore]public string Key { get => key; set => key = value; }
        [JsonIgnore,ExcelIgnore]public string Description { get => description; set => description = value; }
        [JsonIgnore,ExcelIgnore]public Sprite Icon { get => icon; set => icon = value; }
        [JsonIgnore,ExcelIgnore]public string EquipmentType { get => equipmentType; set => equipmentType = value; }
        [JsonIgnore,ExcelIgnore] public IReadOnlyList<EquipParam> EquipParams => equipParams;
        [ExcelIgnore,JsonIgnore]
        private IEnumerable AllEquipmentsType => AssemblyHelper.GetTypes(type => typeof(IEquipment).IsAssignableFrom(type))
            .Where(x => x.IsAbstract == false && x.IsInterface == false && x.IsClass)
           .Select(x => new ValueDropdownItem() { Text = x.ToString(), Value = x.ToString() });
#if UNITY_EDITOR
        private void DrawPreview()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Equipment的图标样式");
            icon = (Sprite)UnityEditor.EditorGUILayout.ObjectField(this.icon, typeof(Sprite), true, GUILayout.Width(50), GUILayout.Height(50));
            GUILayout.EndHorizontal();
        }

 /*       private bool IsControllerOrNull => AssemblyHelper.GetType(equipmentType) != null;
        [Button("打开控制器脚本", ButtonHeight = 30), PropertySpace(20), ShowIf(nameof(IsControllerOrNull))]
        void OpenControllerScript()
        {
            Type type = AssemblyHelper.GetType(equipmentType);
            AssetDatabase.OpenAsset(AssetDatabase.FindAssets("t:monoScript").Select(x => AssetDatabase.GUIDToAssetPath(x))
               .Select(x => AssetDatabase.LoadAssetAtPath<MonoScript>(x))
               .FirstOrDefault(x => x?.GetClass() == type));
        }*/
#endif
    }
}
