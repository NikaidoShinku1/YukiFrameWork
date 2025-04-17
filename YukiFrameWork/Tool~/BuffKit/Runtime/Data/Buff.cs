///=====================================================
/// - FileName:      Buff.cs
/// - NameSpace:     YukiFrameWork.Buffer
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/5/5 21:00:44
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using Newtonsoft.Json;
using System.Linq;
using System.Collections;
using YukiFrameWork.Extension;
namespace YukiFrameWork.Buffer
{
    public abstract class Buff : ScriptableObject, IBuff
    {
        [SerializeField, LabelText("Buff的唯一标识:"), InfoBox("该标识应唯一，不允许出现多个buff同标识的情况"), JsonProperty]
        private string BuffKey;
        [JsonIgnore]
        public string GetBuffKey
        {
            get => BuffKey;
            set => BuffKey = value;
        }

        public static Buff CreateInstance(string buffName, Type type)
        {
            var buff = ScriptableObject.CreateInstance(type) as Buff;
            buff.BuffName = buffName;
            return buff;
        }

        public Buff Clone() => GameObject.Instantiate(this);

        [SerializeField, LabelText("Buff名称:"), JsonProperty]
        private string BuffName;
        [JsonIgnore]
        public string GetBuffName { get => BuffName; set => BuffName = value; }

        [SerializeField, LabelText("Buff的介绍:"), JsonProperty, TextArea]
        private string Description;
        [JsonIgnore]
        public string GetDescription { get => Description; set => Description = value; }
       
        [SerializeField, LabelText("Buff重复添加的类型:"),JsonProperty]
        private BuffRepeatAdditionType additionType;

        [JsonIgnore, ExcelIgnore]
        public BuffRepeatAdditionType AdditionType
        {
            get => additionType;
            set => additionType = value;
        }
        [JsonIgnore, ExcelIgnore]
        private bool IsAddition => AdditionType == BuffRepeatAdditionType.Multiple || AdditionType == BuffRepeatAdditionType.MultipleAndReset;

        [SerializeField, LabelText("是否缓慢移除"), ShowIf(nameof(IsAddition)), InfoBox("如果Buff是可叠加的，当该属性为True时，Buff是一层一层减少，False则一次性消失")]
        private bool isBuffRemoveBySlowly;
        [JsonIgnore,ExcelIgnore]
        public bool IsBuffRemoveBySlowly
        {
            get => isBuffRemoveBySlowly;
            set => isBuffRemoveBySlowly = value;
        }

        [SerializeField, LabelText("是否存在层数上限"), ShowIf(nameof(IsAddition))]
        private bool isMaxStackableLayer;
        [JsonIgnore,ExcelIgnore]
        public bool IsMaxStackableLayer { get => isMaxStackableLayer; set => isMaxStackableLayer = value; }

        [SerializeField, LabelText("上限层数"), ShowIf(nameof(isMax)), MinValue(1)]
        private int maxStackableLayer;
        [JsonIgnore,ExcelIgnore]
        public int MaxStackableLayer { get => maxStackableLayer; set => maxStackableLayer = value; }

        [JsonIgnore,ExcelIgnore]
        private bool isMax => IsAddition && IsMaxStackableLayer;

        [SerializeField,LabelText("Buff的周期类型:")]
        private BuffSurvivalType survivalType;
        [JsonIgnore,ExcelIgnore]
        public BuffSurvivalType SurvivalType
        {
            get => survivalType;
            set => survivalType = value;
        }       

        [SerializeField,LabelText("Buff的持续时间(单位秒)"),ShowIf(nameof(survivalType),BuffSurvivalType.Timer)]
        private float buffTimer = 1;
       

        [JsonIgnore]
        public float BuffTimer
        {
            get => buffTimer;
            set
            {
                value = Mathf.Clamp(value, 0f, float.MaxValue);
                buffTimer = value;
            }
        }

        [SerializeField, JsonProperty, LabelText("该Buff会与指定相互抵消的BuffID")]
#if UNITY_EDITOR
        [ValueDropdown(nameof(names))]
#endif
        [InfoBox("Buff没有设置标识时无法添加")]
        private string[] buffCounteractID = new string[0];
       

        [JsonIgnore,ExcelIgnore]
        public string[] BuffCounteractID { get => buffCounteractID; set => buffCounteractID = value; }

        [SerializeField, JsonProperty, LabelText("该Buff运作时禁止添加的BuffID")]
#if UNITY_EDITOR
        [ValueDropdown(nameof(names))]
#endif
        [InfoBox("Buff没有设置标识时无法添加")]
        private string[] buffDisableID = new string[0]; 
      

        [JsonIgnore,ExcelIgnore]
        public string[] BuffDisableID { get => buffDisableID; set => buffDisableID = value; }
        [SerializeField, LabelText("Buff的图标样式")]
#if UNITY_EDITOR
        [CustomValueDrawer(nameof(DrawPreview))]
#endif
        private Sprite buffIcon;
        [JsonIgnore,ExcelIgnore]
        public Sprite BuffIcon { get => buffIcon; set => buffIcon = value; }

        [Button("打开脚本", ButtonHeight = 20), PropertySpace(20)]
        void OpenScript()
        {
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.OpenAsset(UnityEditor.AssetDatabase.FindAssets("t:monoScript").Select(x => UnityEditor.AssetDatabase.GUIDToAssetPath(x))
                .Select(x => UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEditor.MonoScript>(x))
                .FirstOrDefault(x => x?.GetClass() == this.GetType()));
#endif
        }       

        #region Buff自带依赖ID转换于编辑器显示
#if UNITY_EDITOR
        [Searchable]
        [JsonIgnore]
        [ExcelIgnore]
        internal IEnumerable names => BuffDataBase.allBuffNames;
        private void DrawPreview()
        {
           
            GUILayout.BeginHorizontal();
            
            GUILayout.Label("Buff的图标样式");
            buffIcon = (Sprite)UnityEditor.EditorGUILayout.ObjectField(this.buffIcon,typeof(Sprite),true,GUILayout.Width(50), GUILayout.Height(50));           
            GUILayout.EndHorizontal();
        }
#endif
        #endregion
    }
}
