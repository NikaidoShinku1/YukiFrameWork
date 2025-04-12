///=====================================================
/// - FileName:      MissionDataBase.cs
/// - NameSpace:     YukiFrameWork.MissionKit
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/10/13 21:51:24
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System.Linq;
using YukiFrameWork.Extension;
using System.Collections;


#if UNITY_EDITOR
using UnityEditor;
#endif
namespace YukiFrameWork.Missions
{
	public abstract class MissionConfigBase : ScriptableObject
	{
		[LabelText("编辑器显示名称")]
		[InfoBox("当该值为空，则显示默认名称")]
		public string displayName;
		public abstract IEnumerable<IMissionData> Missions { get; }		      
   
	}
    public abstract class MissionConfigBase<T> : MissionConfigBase, IExcelSyncScriptableObject where T : IMissionData
    {
		[SerializeField,LabelText("任务集合"),TableList(NumberOfItemsPerPage = 5,DrawScrollView = true),Searchable(), FoldoutGroup("任务集合")]
		private T[] missions;
	
		private IEnumerable<IMissionData> runtime_missions;
		public override IEnumerable<IMissionData> Missions 
		{
			get
			{
				runtime_missions ??= missions.Select(x => x as IMissionData);				
				return runtime_missions;
			}
		}

        public IList Array => missions;

        public Type ImportType => typeof(T);

        public bool ScriptableObjectConfigImport => true;      
        public void Create(int maxLength)
        {
            missions = new T[maxLength];
        }

        public void Import(int index, object userData)
        {
            missions[index] = (T)userData;
        }

        public void Completed()
        {
#if UNITY_EDITOR
            if (MissionConfigBaseEditorWindow.Instance)
                MissionConfigBaseEditorWindow.Instance.ForceMenuTreeRebuild();
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
