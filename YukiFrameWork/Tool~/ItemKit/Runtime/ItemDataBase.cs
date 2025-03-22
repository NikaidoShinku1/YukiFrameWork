///=====================================================
/// - FileName:      ItemDataBase.cs
/// - NameSpace:     YukiFrameWork.Item
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/19 18:27:49
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using YukiFrameWork.Extension;
using UnityEngine.U2D;
using System.Linq;
using System.Text;
using System.Collections;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace YukiFrameWork.Item
{
    public abstract class ItemDataBase : ScriptableObject
    {
        [LabelText("编辑器显示名称")]
        [InfoBox("如果该值为空，则显示默认名称"),SerializeField]
        internal string displayName;
        public abstract IItem[] Items { get; }      
    }
    public abstract class ItemDataBase<Item> : ItemDataBase,IExcelSyncScriptableObject where Item : class, IItem
    {
        [SerializeField, Searchable,  ListDrawerSettings(ListElementLabelName = "Name", NumberOfItemsPerPage = 5,ShowIndexLabels = true)]
        internal Item[] items = new Item[0];
       
        public override IItem[] Items
        {
            get => items.Select(x => x as IItem).ToArray();           
        }
        public IList Array => items;

        public Type ImportType => typeof(Item);

        public bool ScriptableObjectConfigImport => true;

        public void Create(int maxLength)
        {
            items = new Item[maxLength];
        }

        public void Import(int index, object userData)
        {
            items[index] = (Item)userData;
        }
        public void Completed() 
        {
#if UNITY_EDITOR
            if (ItemDataBaseEditorWindow.Instance)
                ItemDataBaseEditorWindow.Instance.ForceMenuTreeRebuild();
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

