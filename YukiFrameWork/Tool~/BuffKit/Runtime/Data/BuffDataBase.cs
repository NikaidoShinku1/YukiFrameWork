///=====================================================
/// - FileName:      BuffBase.cs
/// - NameSpace:     YukiFrameWork.Buffer
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/5/5 16:26:15
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using Sirenix.Utilities.Editor;
#endif
using System.Collections.Generic;
using System;
using System.Linq;
using YukiFrameWork.Extension;
namespace YukiFrameWork.Buffer
{
    [CreateAssetMenu(fileName = "BuffDataBase",menuName = "YukiFrameWork/BuffDataBase")]
    public class BuffDataBase : ScriptableObject
    {           
        [FoldoutGroup("代码设置"), SerializeField]
        private string fileName;

        [FoldoutGroup("代码设置"), SerializeField, FolderPath]
        private string filePath = "Assets/Scripts/Buff";

        [FoldoutGroup("代码设置"), SerializeField]
        private string nameSpace = "YukiFrameWork.Buffer";

#if UNITY_EDITOR
        [HideInInspector, SerializeField]
        public int selectIndex;
        [Button("生成Buff代码"), GUIColor("green"), FoldoutGroup("代码设置")]
        void CreateCode()
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Debug.LogError("请输入脚本文件名");
                return;
            }

            if (string.IsNullOrEmpty(nameSpace))
            {
                Debug.LogError("请输入命名空间，保持良好的命名空间习惯");
                return;
            }
            CodeCore codeCore = new CodeCore();

            codeCore
                .Using("System")
                .Using("UnityEngine")
                .Using("YukiFrameWork.Buffer")
                .Descripton(fileName, nameSpace, "自动化代码生成的Buff派生类");

            codeCore.CodeSetting(nameSpace, fileName, nameof(Buff), null).Create(fileName, filePath);
        }

        [Button("生成标识代码"), GUIColor("green"),PropertySpace(15), FoldoutGroup("代码设置")]
        [InfoBox("标识代码则为所有配置的标识以及Buff的快捷获取，仅在配置完毕且没有标识为空的时候使用")]
        void CreateDefaultCode(string buffInfoScriptNames = "Buffs")
        {          
            if (string.IsNullOrEmpty(nameSpace))
            {
                Debug.LogError("请输入命名空间，保持良好的命名空间习惯");
                return;
            }

            if (buffConfigs.Count == 0)
            {
                Debug.LogError("当前不存在配置");
                return;
            }

            CodeCore codeCore = new CodeCore();
            codeCore
               .Using("System")
               .Using("UnityEngine")
               .Using("YukiFrameWork.Buffer")
               .Descripton(buffInfoScriptNames, nameSpace, "自动化代码生成的Buff标识类");
            CodeWriter writer = new CodeWriter();
            foreach (var item in buffConfigs)
            {
                if (item == null) continue;
                if (string.IsNullOrEmpty(item.GetBuffKey)) continue;

                writer.CustomCode($"public static string {item.GetBuffKey}_Key = \"{item.GetBuffKey}\";");
                writer.CustomCode($"public static IBuff {item.GetBuffKey} => BuffKit.GetBuffByKey(\"{item.GetBuffKey}\");");
            }
            codeCore.CodeSetting(nameSpace, buffInfoScriptNames, string.Empty, writer).Create(buffInfoScriptNames, filePath);
        }
#endif
       
        [SerializeField,LabelText("Buff配置")]      
        [InfoBox("添加Buff配置需要选择Buff的类型，当项目中没有任何Buff类文件时，无法新建Buff配置，且删除对应文件时，会自动消除对应的类",InfoMessageType.Warning)]
        [VerticalGroup("配置")]
        [ListDrawerSettings(HideAddButton = true,CustomRemoveIndexFunction = nameof(DeleteBuff),DraggableItems = false)]
        public List<Buff> buffConfigs = new List<Buff>();
#if UNITY_EDITOR
        bool reimportTable => buffConfigs.Count > 0;
        [ShowIf(nameof(reimportTable)), FoldoutGroup("JsonBuff"),SerializeField]
        private string jsonName = "Buff";
        [ShowIf(nameof(reimportTable)), FoldoutGroup("JsonBuff"),SerializeField]
        private string jsonPath = "Assets/BuffData";
        [ShowIf(nameof(reimportTable)),Button("将现有配置导出Json"),FoldoutGroup("JsonBuff")]
        void CreateTable()
        {
            if (buffConfigs.Count == 0) return;

            foreach (var buff in buffConfigs)
            {
                if (buff.BuffIcon != null)
                    buff.Sprite = AssetDatabase.GetAssetPath(buff.BuffIcon);

                buff.BuffType = buff.GetType().ToString();
            }

            SerializationTool.SerializedObject(buffConfigs, settings: new Newtonsoft.Json.JsonSerializerSettings()
            {
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
            }).CreateFileStream(jsonPath,jsonName,".json");
        }      
#endif
        
        private void OnValidate()
        {          
            for (int i = 0; i < buffConfigs.Count; i++)
            {              
                buffConfigs[i].dataBase = this;
            }
        }
   
        internal void CreateBuff(Type buffType)
        {           
            Buff buff = Buff.CreateInstance(buffType.Name, buffType);
                      
            buffConfigs.Add(buff);
            OnValidate();
#if UNITY_EDITOR
            AssetDatabase.AddObjectToAsset(buff,this);
            this.Save();
#endif
        }

        public void DeleteBuff(int index)
        {
            var buff = buffConfigs[index];

            buffConfigs.RemoveAt(index);

#if UNITY_EDITOR
            AssetDatabase.RemoveObjectFromAsset(buff);           
            this.Save();
#endif
        }
    }
}
