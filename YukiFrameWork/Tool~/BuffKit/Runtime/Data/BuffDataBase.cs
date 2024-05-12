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
                .Descripton(fileName, nameSpace, "自动化代码生成的Buff派生类");

            codeCore.CodeSetting(nameSpace, fileName, nameof(Buff), null).Create(fileName, filePath);
        }

        [Button("生成标识代码"), GUIColor("green"),PropertySpace(15), FoldoutGroup("代码设置")]
        [InfoBox("标识代码则为所有配置的标识以及Buff的快捷获取，类名为Buffs,仅在配置完毕且没有标识为空的时候使用")]
        void CreateDefaultCode()
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
               .Descripton("Buffs", nameSpace, "自动化代码生成的Buff标识类");
            CodeWriter writer = new CodeWriter();
            foreach (var item in buffConfigs)
            {
                if (item == null) continue;
                if (string.IsNullOrEmpty(item.GetBuffKey)) continue;

                writer.CustomCode($"public static string {item.GetBuffKey}_Key = \"{item.GetBuffKey}\";");
                writer.CustomCode($"public static IBuff {item.GetBuffKey} => BuffKit.GetBuffByKey(\"{item.GetBuffKey}\");");
            }
            codeCore.CodeSetting(nameSpace, "Buffs", string.Empty, writer).Create("Buffs", filePath);
        }
#endif
        [SerializeField, ValueDropdown(nameof(buffInfoNames)), VerticalGroup("配置"),PropertySpace(10), LabelText("生成类型:"), ShowIf(nameof(isShowInfo))]
        private int buffSelectIndex = 0;

        [SerializeField,LabelText("Buff配置")
            ,ListDrawerSettings(ShowPaging = true          
            ,CustomAddFunction = nameof(CreateBuff)
            ,CustomRemoveIndexFunction = nameof(DeleteBuff)
            ,OnEndListElementGUI = nameof(OnEndDrawBuffGUI)
            ,NumberOfItemsPerPage = 3
            ,DraggableItems = true
            ,ListElementLabelName = "BuffName"             
            ,ShowFoldout = true)]
        [InfoBox("添加Buff配置需要选择Buff的类型，当项目中没有任何Buff类文件时，无法新建Buff配置，且删除对应文件时，会自动消除对应的类",InfoMessageType.Warning)]
        [VerticalGroup("配置")]
        public List<Buff> buffConfigs = new List<Buff>();
     
        private ValueDropdownList<int> buffInfoNames = new ValueDropdownList<int>();
        private string[] nameInfos = new string[0];
        
        private bool isShowInfo => buffInfoNames.Count > 0;

        private bool reimportTable => buffConfigs.Count > 0;

        [ShowIf(nameof(reimportTable)), FoldoutGroup("JsonBuff"),SerializeField]
        private string jsonName = "Buff";
        [ShowIf(nameof(reimportTable)), FoldoutGroup("JsonBuff"),SerializeField]
        private string jsonPath = "Assets/BuffData";
        [ShowIf(nameof(reimportTable)),Button("将现有配置导出Json"),FoldoutGroup("JsonBuff")]
        void CreateTable()
        {
            SerializationTool.SerializedObject(buffConfigs).CreateFileStream(jsonPath,jsonName,".json");
        }
        
        private void OnEnable()
        {
            Type[] types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes())
                .Where(x => x.IsSubclassOf(typeof(Buff))).ToArray();

            buffInfoNames.Clear();
            for(int i = 0;i<types.Length;i++)
            {
                var type = types[i];
                buffInfoNames.Add(type.FullName, i);
            }

            nameInfos = new string[buffInfoNames.Count];

            for (int i = 0; i < buffInfoNames.Count; i++)
            {
                nameInfos[i] = buffInfoNames[i].Text;
            }

            if (buffSelectIndex >= nameInfos.Length)
                buffSelectIndex = 0;
        }
        List<string> buffNames = new List<string>();
        private void OnValidate()
        {          
            for (int i = 0; i < buffConfigs.Count; i++)
            {
                if (buffConfigs[i] == null)
                {
                    buffConfigs.RemoveAt(i);
                    i--;
                    continue;
                }                
                buffNames.Clear();               
                buffConfigs[i].dataBase = this;
            }
#if UNITY_EDITOR
            editors.Clear();
#endif
        }

#if UNITY_EDITOR
        Dictionary<int, OdinEditor> editors = new Dictionary<int, OdinEditor>();
#endif
        void OnEndDrawBuffGUI(int index)
        {
            var buff = buffConfigs[index];
            if (buff == null) return;
#if UNITY_EDITOR

            try
            {
                if (!editors.TryGetValue(index, out var editor))
                {
                    editors.Add(index, Editor.CreateEditor(buff, typeof(OdinEditor)) as OdinEditor);
                }
                else if (!Equals(editors[index].target, buff))
                {
                    editors[index] = Editor.CreateEditor(buff, typeof(OdinEditor)) as OdinEditor;
                }
                else
                {                   
                    editors[index].DrawDefaultInspector();                   
                }
            }
            catch { }
#endif
        }
        void CreateBuff()
        {
            if (buffInfoNames.Count == 0 || nameInfos.Length == 0)
            {
                throw new Exception("当前没有任何类继承Buff基类，无法创建新的Buff配置!");              
            }

            Type buffType = AssemblyHelper.GetType(buffInfoNames[buffSelectIndex].Text) 
                ?? throw new Exception("该BuffType没有找到，请重试：Type:" + buffInfoNames[buffSelectIndex].Text);

            Buff buff = Buff.CreateInstance(buffType.Name, buffType);
           
            buff.dataBase = this;
            buffConfigs.Add(buff);

#if UNITY_EDITOR
            AssetDatabase.AddObjectToAsset(buff,this);
            this.Save();
#endif
        }

        void DeleteBuff(int index)
        {
            var buff = buffConfigs[index];

            buffConfigs.RemoveAt(index);

#if UNITY_EDITOR
            AssetDatabase.RemoveObjectFromAsset(buff);
            OnValidate();
            this.Save();
#endif
        }
    }
}
