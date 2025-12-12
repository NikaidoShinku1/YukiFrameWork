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
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace YukiFrameWork.Buffer
{
    /// <summary>
    /// 框架提供的默认效果
    /// </summary>
    [Serializable]
    public class NormalEffect : IEffect
    {
        [SerializeField,LabelText("效果的唯一标识")]
        [InfoBox("效果的唯一标识仅针对当前Buff")]
        private string key;
        [SerializeField,LabelText("效果的名称")]
        private string name;
        [SerializeField,LabelText("效果介绍"),TextArea]
        private string description;
        [LabelText("效果的类型"),SerializeField]
        private string type;
        public string Key { get => key; set => key = value; }
        public string Name { get => name; set => name = value; }
        public string Type { get => type; set => type = value; }
        public string Description { get => description ; set => description = value; }
    }
    public abstract class Buff : ScriptableObject, IBuff
    {
        [SerializeField, LabelText("Buff唯一标识")]
        private string key;
        [SerializeField, LabelText("Buff名称")]
        private string _name;
        [SerializeField,TextArea,LabelText("Buff的介绍")]
        private string description;
        [SerializeField,LabelText("Buff的模式")]
        [InfoBox("这个决定了Buff是否可叠加")]
        private BuffMode buffMode;
        [SerializeField,LabelText("持续时间")]
        [InfoBox("当持续时间小于0,则视为该Buff不受时间影响。需自行进行移除")]
        private float duration;
        [SerializeField,LabelText("Buff的图标")]
#if UNITY_EDITOR
        [CustomValueDrawer(nameof(DrawPreview))]
#endif
        private Sprite icon;
        [SerializeField,LabelText("Buff参数")]
        private BuffParam[] buffParams;
        
        [SerializeField,LabelText("Buff默认效果集合")]
        [InfoBox("当EffectDatas被重写时，该字段不可用，请自行序列化"),ShowIf(nameof(IsNormalEffect))]
        private List<NormalEffect> normalEffects = new List<NormalEffect>();

        [ExcelIgnore]internal List<IEffect> runtimeNormalEffects = new List<IEffect>();

        [SerializeField,PropertySpace,LabelText("Buff绑定的控制器类型")]
        [ValueDropdown(nameof(AllControllerType))]
        private string buffControllerType;
        [ExcelIgnore]public BuffParam[] BuffParams => buffParams;
        [ExcelIgnore]public string Key { get => key; set => key = value; }
        [ExcelIgnore]public string Name { get => _name ; set => _name = value; }
        [ExcelIgnore]public string Description { get => description; set => description = value ; }
        [ExcelIgnore]public BuffMode BuffMode { get => buffMode; set => buffMode = value; }
        [ExcelIgnore]public float Duration { get => duration ; set => duration = value; }
        [ExcelIgnore]public Sprite Icon { get => icon; set => icon = value; }
        [ExcelIgnore]public virtual List<IEffect> EffectDatas
        {
            get
            {
                if (runtimeNormalEffects == null || runtimeNormalEffects.Count == 0)
                    runtimeNormalEffects = normalEffects.Select(x => x as IEffect).ToList();
                return runtimeNormalEffects;
            }
        }

        [ExcelIgnore]public string BuffControllerType
        {
            get => buffControllerType;
            set => buffControllerType = value;
        }

        [ExcelIgnore]private bool IsNormalEffect => this.GetType().GetRealProperty(nameof(EffectDatas)).DeclaringType == typeof(Buff);
        [ExcelIgnore]private IEnumerable AllControllerType => AssemblyHelper.GetTypes(type => type.IsSubclassOf(typeof(BuffController)))
            .Where(x => x.IsAbstract == false)
            .Select(x => new ValueDropdownItem() { Text = x.ToString(), Value = x.ToString() });

#if UNITY_EDITOR
        private void DrawPreview()
        {
            UnityEditor.EditorGUI.BeginChangeCheck();
            GUILayout.BeginHorizontal();

            GUILayout.Label("Buff图标");
            icon = (Sprite)UnityEditor.EditorGUILayout.ObjectField(this.icon, typeof(Sprite), true, GUILayout.Width(50), GUILayout.Height(50));
            GUILayout.EndHorizontal();
            if (UnityEditor.EditorGUI.EndChangeCheck())
            {
                this.Save();
            }
        }

        private bool IsControllerOrNull => AssemblyHelper.GetType(buffControllerType) != null;
        [Button("打开控制器脚本", ButtonHeight = 30), PropertySpace(20), ShowIf(nameof(IsControllerOrNull))]
        void OpenControllerScript()
        {
            Type type = AssemblyHelper.GetType(buffControllerType);
            AssetDatabase.OpenAsset(AssetDatabase.FindAssets("t:monoScript").Select(x => AssetDatabase.GUIDToAssetPath(x))
               .Select(x => AssetDatabase.LoadAssetAtPath<MonoScript>(x))
               .FirstOrDefault(x => x?.GetClass() == type));
        }

        [Button("打开脚本", ButtonHeight = 30), PropertySpace(20)]
        void OpenScript()
        {
#if UNITY_EDITOR
            AssetDatabase.OpenAsset(AssetDatabase.FindAssets("t:monoScript").Select(x => AssetDatabase.GUIDToAssetPath(x))
                .Select(x => AssetDatabase.LoadAssetAtPath<MonoScript>(x))
                .FirstOrDefault(x => x?.GetClass() == this.GetType()));
#endif
        }
#endif

    }
}
