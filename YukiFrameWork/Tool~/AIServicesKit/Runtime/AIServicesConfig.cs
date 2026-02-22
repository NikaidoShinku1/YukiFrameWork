using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using System;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using YukiFramework.AI.Editor;
using YukiFrameWork.Extension;
#endif
namespace YukiFrameWork.AI
{
    [CreateAssetMenu(fileName = nameof(AIServicesConfig), menuName = "YukiFrameWork/" + "框架AI服务配置"+nameof(AIServicesConfig), order = 0)]
    public class AIServicesConfig : ScriptableObject,IExcelSyncScriptableObject
    {
        [LabelText("AI分组")] public string groupName;

        [ReadOnly]
        [LabelText("所有的AI服务信息")] public List<AIServicesInfo> aiServicesInfos = new List<AIServicesInfo>();

        internal Action onValidate;

        private void OnValidate()
        {
            onValidate?.Invoke();
        }

        internal void CreateServicesData()
        {
            AIServicesInfo info = ScriptableObject.CreateInstance<AIServicesInfo>();
            info.id = aiServicesInfos.Count;
            aiServicesInfos.Add(info);
#if UNITY_EDITOR
            AssetDatabase.AddObjectToAsset(info, this);
            this.Save();
#endif
        }

        internal void DeleteServicesData(int index)
        {
            var info = aiServicesInfos[index];
            DeleteServicesData(info);
        }

        internal void DeleteServicesData(AIServicesInfo data)
        {        
            aiServicesInfos.Remove(data);

#if UNITY_EDITOR
            if(data)
                AssetDatabase.RemoveObjectFromAsset(data);
            this.Save();
#endif
        }
        
#if UNITY_EDITOR

        [UnityEditor.Callbacks.OnOpenAsset(0)]
        private static bool OnOpenAsset(int insId, int line)
        {
            AIServicesConfig obj = EditorUtility.InstanceIDToObject(insId) as AIServicesConfig;
            if (obj != null)
            {
                AIServicesConfigWindow.ShowWindow();
            }
            return obj != null;
        }
        
#endif

        public IList Array => aiServicesInfos;
        public Type ImportType => typeof(AIServicesInfo);
        public void Create(int maxLength)
        {
            while (aiServicesInfos.Count > 0)
                DeleteServicesData(aiServicesInfos.Count - 1);
        }

        public void Import(int index, object userData)
        {
            var servicesInfo = userData as AIServicesInfo;

            aiServicesInfos.Add(servicesInfo);          
#if UNITY_EDITOR
            AssetDatabase.AddObjectToAsset(servicesInfo, this);
            this.Save();
#endif

        }

        public void Completed()
        {
#if UNITY_EDITOR
            if (AIServicesConfigWindow.Instance)
                AIServicesConfigWindow.Instance.ForceMenuTreeRebuild();
#endif
        }

        public bool ScriptableObjectConfigImport => false;
#if UNITY_EDITOR
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
        
        [Button("同步配置")]
        [GUIColor("green")]
        [PropertySpace(10)]
        [InfoBox("当配置丢失数据时可使用")]
        void SyncAllConfig()
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(this));

            aiServicesInfos.Clear();
            foreach (var item in assets)
            {
                if (item is AIServicesInfo info)
                {
                    Undo.RecordObject(this, "YukiFramework AIServices (SyncNode)");

                    aiServicesInfos.Add(info);
                    onValidate?.Invoke();
                    EditorUtility.SetDirty(this);
                    AssetDatabase.SaveAssets();

                }
            }
        }
#endif
    }
}