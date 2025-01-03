///=====================================================
/// - FileName:      BuffDataBaseEditor.cs
/// - NameSpace:     YukiFrameWork.Buffer
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/9/5 21:49:26
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
namespace YukiFrameWork.Buffer
{
    [CustomEditor(typeof(BuffDataBase))]
	public class BuffDataBaseEditor : OdinEditor
	{
        private Dictionary<string, OdinEditor> buff_Info_Dicts = new Dictionary<string, OdinEditor>();
        private BuffDataBase dataBase => target as BuffDataBase;
        private Dictionary<string, string> show_Info_Dicts = new Dictionary<string, string>();
        private string[] names;

        private bool isGeneraicScript;
        private int configCount;

        private Type[] types;
        protected override void OnEnable()
        {
            base.OnEnable();
            configCount = dataBase.buffConfigs.Count;
            buff_Info_Dicts.Clear();
            show_Info_Dicts.Clear();
            names = null;
            foreach (var item in dataBase.buffConfigs)
            {
                if (!item) continue;
                string key = string.Format("{0}_{1}", item.GetBuffKey, item.GetInstanceID());
                buff_Info_Dicts.Add(key, (OdinEditor)OdinEditor.CreateEditor(item, typeof(OdinEditor)));
                show_Info_Dicts.Add(key, item.GetBuffName);
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
            if (configCount != dataBase.buffConfigs.Count)
            {
                OnEnable();
                configCount = dataBase.buffConfigs.Count;
            }
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            dataBase.selectIndex = EditorGUILayout.Popup("选择Buff编辑:", dataBase.selectIndex, names);
            if (GUILayout.Button("刷新编辑选择", GUILayout.Width(100)))
            {
                OnEnable();
            }
            EditorGUILayout.EndHorizontal();


            if (dataBase.buffConfigs == null || dataBase.buffConfigs.Count == 0)
                EditorGUILayout.HelpBox("当前没有任何技能，请选择下方新建Buff!", MessageType.Warning);

            EditorGUILayout.Space(10);
            GUI.color = Color.cyan;
            if (isGeneraicScript == false && GUILayout.Button("创建新的Buff"))
            {
                isGeneraicScript = true;
                types = AssemblyHelper.GetTypes(x => x.IsSubclassOf(typeof(Buff)) && !x.IsAbstract);
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
                            dataBase.CreateBuff(types[index]);
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
                GUILayout.Label("Buff Default Inspector", fontStyle);
                EditorGUILayout.Space(20);
                buff_Info_Dicts[names[dataBase.selectIndex]].DrawDefaultInspector();

            }
            catch
            {
               
            }
            EditorGUILayout.EndVertical();
        }
    }
}
#endif