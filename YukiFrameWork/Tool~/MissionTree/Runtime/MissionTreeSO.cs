///=====================================================
/// - FileName:      RuntimeMissionTree.cs
/// - NameSpace:     YukiFrameWork.Missions
/// - Description:   高级定制脚本生成
/// - Creation Time: 1/12/2026 7:24:23 PM
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using YukiFrameWork.Extension;
using System.Collections;
using System.Linq;




#if UNITY_EDITOR
using UnityEditor;
#endif
namespace YukiFrameWork.Missions
{
    [CreateAssetMenu(fileName ="MissionTreeSO",menuName = "YukiFrameWork/任务树节点MissionTreeSO构造")]
    public class MissionTreeSO : ScriptableObject,IExcelReSyncScriptableObject
    {
        [LabelText("任务树唯一标识")]
        public string missionTreeKey;
        [SerializeField, HideInInspector]
        public Rect ViewportRect;
        [SerializeField, HideInInspector]
        internal List<Mission> AllMissions = new List<Mission>();

        /*[SerializeField, HideInInspector]
        public TreeShowInspectorType treeShowInspectorType;*/
        internal System.Action onValidate;

        public void ForEach(Action<Mission> each)
        {
            for (int i = 0; i < AllMissions.Count; i++)
            {
                each?.Invoke(AllMissions[i]);
            }
        }

        public Mission FindMission(int id)
        {
            foreach (var item in AllMissions)
            {
                if (item.MissionId == id)
                    return item;
            }

            return null;
        }

        public MissionTreeSO Clone()
        {
            MissionTreeSO missionTree = this.Instantiate();

            missionTree.AllMissions.Clear();
            foreach (var item in AllMissions)
            {
                var cloneItem = item.Instantiate();
                missionTree.AllMissions.Add(cloneItem);
            }

            return missionTree;
        }

        public bool ReImport(out string error)
        {
            try
            {
                error = string.Empty;
                foreach (var item in AllMissions)
                    item.missionsChildIds = item.Childs.Select(x => x.MissionId).ToList();

                return true;
            }
            catch (Exception ex)
            {
                error = ex.ToString();
                return false;
            }
        }

        public void Create(int maxLength)
        {
#if UNITY_EDITOR
            while (AllMissions.Count > 0)
            {
                Remove(AllMissions[AllMissions.Count - 1]);
            }
#endif
            AllMissions = new List<Mission>();
        }

        public void Import(int index, object userData)
        {
            Mission mission = userData as Mission;
            mission.missionTreeSO = this;
            AllMissions.Add(mission);

#if UNITY_EDITOR
            AssetDatabase.AddObjectToAsset(mission, this);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif
        }

        public void Completed()
        {
#if UNITY_EDITOR
            foreach (var item in AllMissions)
            {
                item.UpdateAllChilds();
            }
#endif
        }

        public IList Array => AllMissions;

        public Type ImportType => typeof(Mission);

        public bool ScriptableObjectConfigImport => false;


#if UNITY_EDITOR

        [InfoBox("导入完成后如编辑器没刷新则点击右上角刷新脚本")]
        [Sirenix.OdinInspector.FilePath(Extensions = "xlsx"), PropertySpace(50), LabelText("Excel路径")]
        public string excelPath;

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
        private void OnValidate()
        {
            onValidate?.Invoke();
        }

        public Mission Create(Type type)
        {
            Mission mission = ScriptableObject.CreateInstance(type) as Mission;
            mission.name = type.Name;
            int id = (1000) + AllMissions.Count + 1;
            mission.MissionId = id;

            // node.nodeType = type.FullName;
            while (AllMissions.Any(x => x.MissionId == id))
            {
                mission.MissionId = ++id;
            }
            mission.MissionName = "编号:" + id.ToString();
            mission.missionTreeSO = this;
            Undo.RecordObject(this, "Behaviour Tree (CreateBehaviour)");
            AllMissions.Add(mission);
            AssetDatabase.AddObjectToAsset(mission, this);
            Undo.RegisterCreatedObjectUndo(mission, "Behaviour Tree (CreateBehaviour)");
            AssetDatabase.SaveAssets();
            return mission;
        }
        public Mission Remove(Mission behaviour)
        {
            Undo.RecordObject(this, "Behaviour Tree (DeleteBehaviour)");
            AllMissions.Remove(behaviour);         
            Undo.DestroyObjectImmediate(behaviour);
            AssetDatabase.SaveAssets();
            return behaviour;
        }


#endif


    }
}
