///=====================================================
/// - FileName:      EquipmentConfig.cs
/// - NameSpace:     RPG
/// - Description:   高级定制脚本生成
/// - Creation Time: 12/12/2025 12:49:53 PM
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using YukiFrameWork.Extension;
namespace YukiFrameWork.Equips
{

    public abstract class EquipmentConfigBase : ScriptableObject
    {
        [LabelText("编辑器显示名称")]
        [InfoBox("当该值为空，则显示默认名称")]
        public string displayName;
        public abstract IEnumerable<IEquipmentData> Equipments { get; }

    }
    public abstract class EquipmentConfigBase<T> : EquipmentConfigBase, IExcelSyncScriptableObject where T : IEquipmentData
    {
        [SerializeField, LabelText("任务集合"), TableList(NumberOfItemsPerPage = 5, DrawScrollView = true), Searchable(), FoldoutGroup("装备集合")]
        private T[] equips;

        private IEnumerable<IEquipmentData> runtime_equipss;
        public override IEnumerable<IEquipmentData> Equipments
        {
            get
            {
                runtime_equipss ??= equips.Select(x => x as IEquipmentData);
                return runtime_equipss;
            }
        }

        public IList Array => equips;

        public Type ImportType => typeof(T);

        public bool ScriptableObjectConfigImport => true;
        public void Create(int maxLength)
        {
            equips = new T[maxLength];
        }

        public void Import(int index, object userData)
        {
            equips[index] = (T)userData;
        }

        public void Completed()
        {
/*#if UNITY_EDITOR
            if (MissionConfigBaseEditorWindow.Instance)
                MissionConfigBaseEditorWindow.Instance.ForceMenuTreeRebuild();
#endif*/
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

    public class EquipmentConfig : EquipmentConfigBase<EquipmentData>
    {
        
    }
}
