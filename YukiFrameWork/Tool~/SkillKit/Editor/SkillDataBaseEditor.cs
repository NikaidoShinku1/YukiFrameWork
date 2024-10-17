///=====================================================
/// - FileName:      SkillDataBaseEditor.cs
/// - NameSpace:     YukiFrameWork.Skill
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/9/5 21:09:28
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using YukiFrameWork.Extension;
#if UNITY_EDITOR
using UnityEditor;
using Sirenix.OdinInspector.Editor;
namespace YukiFrameWork.Skill
{
	[CustomEditor(typeof(SkillDataBase))]
	public class SkillDataBaseEditor : OdinEditor
	{
        private Dictionary<string, OdinEditor> skill_Info_Dicts = new Dictionary<string, OdinEditor>();
        private SkillDataBase dataBase => target as SkillDataBase;
        private Dictionary<string, string> show_Info_Dicts = new Dictionary<string, string>();
        private string[] names;

        private bool isGeneraicScript;
        private int configCount;

        private Type[] types;
        protected override void OnEnable()
        {
            base.OnEnable();
            configCount = dataBase.SkillDataConfigs.Count;
            skill_Info_Dicts.Clear();
            show_Info_Dicts.Clear();
            names = null;
            foreach (var item in dataBase.SkillDataConfigs)
            {
                string key = string.Format("{0}_{1}", item.SkillKey, item.GetInstanceID());
                skill_Info_Dicts.Add(key, (OdinEditor)OdinEditor.CreateEditor(item,typeof(OdinEditor)));
                show_Info_Dicts.Add(key, item.SkillName);
            }
            names = show_Info_Dicts.Keys.ToArray();
            if (dataBase.selectIndex >= show_Info_Dicts.Count || dataBase.selectIndex < 0)
                dataBase.selectIndex = 0;
            
        }

        private void OnValidate()
        {
            OnEnable();            
        }
        private GUIStyle fontStyle;
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (configCount != dataBase.SkillDataConfigs.Count)
            {
                OnEnable();
                configCount = dataBase.SkillDataConfigs.Count;
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            dataBase.selectIndex = EditorGUILayout.Popup("选择Buff编辑:", dataBase.selectIndex, names);
            if (GUILayout.Button("刷新编辑选择", GUILayout.Width(100)))
            {
                OnEnable();
            }
            EditorGUILayout.EndHorizontal();


            if (dataBase.SkillDataConfigs == null || dataBase.SkillDataConfigs.Count == 0)       
                EditorGUILayout.HelpBox("当前没有任何技能，请选择下方新建技能!", MessageType.Warning);

            EditorGUILayout.Space(10);
            GUI.color = Color.cyan;
            if (isGeneraicScript == false && GUILayout.Button("创建新的技能"))
            {
                isGeneraicScript = true;
                types = AssemblyHelper.GetTypes(x => x.IsSubclassOf(typeof(SkillData)) && !x.IsAbstract);
            }
            GUI.color = Color.white;
            if (isGeneraicScript)
            {
                if (types != null)
                {
                    for (int i = 0; i < types.Length; i++)
                    {
                        int index = i;
                        if (GUILayout.Button(types[i].ToString()))
                        {
                            dataBase.CreateSkillData(types[index]);
                            Repaint();
                            isGeneraicScript = false;
                        };
                    }
                }
            }
            if (isGeneraicScript && GUILayout.Button("取消"))
            {
                isGeneraicScript = false;
            }

            EditorGUILayout.Space(15);
            EditorGUILayout.BeginVertical("OL Box");
            try
            {              
                fontStyle ??= new GUIStyle()
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleCenter,
                };
                fontStyle.normal.textColor = Color.white;
                GUILayout.Label("Skill Default Inspector",fontStyle);
                EditorGUILayout.Space(20);
                skill_Info_Dicts[names[dataBase.selectIndex]].DrawDefaultInspector();
                
            }
            catch 
            {

            }
            EditorGUILayout.EndVertical();
        }
    }
}
#endif