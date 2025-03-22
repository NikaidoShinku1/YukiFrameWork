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
using System.Collections;
namespace YukiFrameWork.Buffer
{
    [CreateAssetMenu(fileName = "BuffDataBase",menuName = "YukiFrameWork/BuffDataBase")]
    public class BuffDataBase : ScriptableObject,IExcelSyncScriptableObject
    {           
        [FoldoutGroup("代码设置"), SerializeField]
        private string fileName;

        [FoldoutGroup("代码设置"), SerializeField, FolderPath]
        private string filePath = "Assets/Scripts/Buff";

        [FoldoutGroup("代码设置"), SerializeField]
        private string nameSpace = "YukiFrameWork.Buffer";

        public Action onValidate;     

#if UNITY_EDITOR
        public static IEnumerable allBuffNames => YukiAssetDataBase.FindAssets<BuffDataBase>().SelectMany(x => x.buffConfigs).Select(x => new ValueDropdownItem() { Text = x.GetBuffName,Value = x.GetBuffKey});
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
        [ReadOnly]
        public List<Buff> buffConfigs = new List<Buff>();
        public IList Array => buffConfigs;

        public Type ImportType => typeof(Buff);

        public bool ScriptableObjectConfigImport => false;
#if UNITY_EDITOR
        bool reimportTable => buffConfigs.Count > 0;     
    
#endif
             
        internal Buff CreateBuff(Type buffType,string name = "")
        {           
            Buff buff = Buff.CreateInstance(buffType.Name, buffType);
            buff.name = name;
            buffConfigs.Add(buff);
           
#if UNITY_EDITOR
            AssetDatabase.AddObjectToAsset(buff,this);
            this.Save();
#endif
            return buff;
        }

        public void DeleteBuff(int index)
        {
            DeleteBuff(buffConfigs[index]);
        }

        public void DeleteBuff(Buff buff)
        {
            buffConfigs.Remove(buff);
#if UNITY_EDITOR
            if(buff)
            AssetDatabase.RemoveObjectFromAsset(buff);
            this.Save();
#endif
        }
#if UNITY_EDITOR
        [UnityEditor.Callbacks.OnOpenAsset(0)]
        private static bool OnOpenAsset(int insId, int line)
        {
           BuffDataBase obj = EditorUtility.InstanceIDToObject(insId) as BuffDataBase;
            if (obj != null)
            {
                BuffDataBaseEditorWindow.ShowWindow();
            }
            return obj != null;
        }
#endif   
        public void Create(int maxLength)
        {
            while (buffConfigs.Count > 0)
                DeleteBuff(buffConfigs.Count - 1);
        }

        public void Import(int index, object userData)
        {
            Buff buff = userData as Buff;
            buffConfigs.Add(buff);
#if UNITY_EDITOR
            AssetDatabase.AddObjectToAsset(buff, this);
            this.Save();
#endif
        }

        public void Completed()
        {
#if UNITY_EDITOR
            if (BuffDataBaseEditorWindow.Instance)
            {
                BuffDataBaseEditorWindow.Instance.ForceMenuTreeRebuild();
            }
#endif
        }

        [Sirenix.OdinInspector.FilePath(Extensions = "xlsx"), PropertySpace(50), LabelText("Excel路径")]
        public string excelPath;
#if UNITY_EDITOR
        [Button("导出Excel"), HorizontalGroup("Excel")]
        void CreateExcel()
        {
            if (excelPath.IsNullOrEmpty() || !System.IO.File.Exists(excelPath))
                throw new NullReferenceException("路径为空或不存在!");
            if (SerializationTool.ScriptableObjectToExcel(this, excelPath, out string error))
                Debug.Log("导出成功");
            else throw new Exception(error);
        }
        [Button("导入Excel"), HorizontalGroup("Excel")]
        void ImportExcel()
        {
            if (excelPath.IsNullOrEmpty() || !System.IO.File.Exists(excelPath))
                throw new NullReferenceException("路径为空或不存在!");
            if (SerializationTool.ExcelToScriptableObject(excelPath, 3, this))
            {
                Debug.Log("导入成功");
            }
        }
#endif


    }
}
