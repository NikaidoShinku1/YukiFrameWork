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
		public abstract IEnumerable<IMissionData> Missions { get; }		

		protected abstract void ResetAllMissionsType();

#if UNITY_EDITOR
        private static ValueDropdownList<string> mMissionsTypes;

        internal static ValueDropdownList<string> MissionsTypes
        {
            get
            {
                if (mMissionsTypes == null)
                    mMissionsTypes = new ValueDropdownList<string>();

                mMissionsTypes.Clear();

                var configs =YukiAssetDataBase.FindAssets<MissionConfigBase>();

                foreach (var config in configs)
                {                  
					foreach (var type in config.mMissionTypes_dict)
					{
						ValueDropdownItem<string> temp = mMissionsTypes.Find(x => x.Value == type.Key);
                   
						if (temp.Value == type.Key)
							continue;
                        
						mMissionsTypes.Add(type.Value.IsNullOrEmpty() ? type.Key : type.Value, type.Key);
					}
                }

				return mMissionsTypes;

            }
        }
        private static ValueDropdownList<string> mMissionsParams;

        internal static ValueDropdownList<string> MissionsParams
        {
            get
            {
                if (mMissionsParams == null)
                    mMissionsParams = new ValueDropdownList<string>();

                mMissionsParams.Clear();

                var configs = YukiAssetDataBase.FindAssets<MissionConfigBase>();

                foreach (var config in configs)
                {
                    foreach (var type in config.mMissionParams_dict)
                    {
                        ValueDropdownItem<string> temp = mMissionsParams.Find(x => x.Text == type.Key);

                        if (temp.Text == type.Key)
                            continue;

                        if (type.Key.IsNullOrEmpty()) continue;

                        mMissionsParams.Add(type.Key);
                    }
                }

                return mMissionsParams;

            }
        }
        [Button("将任务类型导出Json"), FoldoutGroup("任务类型添加")]
        void CreateType(string filePath = "Assets/Mission", string fileName = "missionTypeData")
        {
            SerializationTool.SerializedObject(mMissionTypes_dict).CreateFileStream(filePath,fileName,".json");
        }

        [Button("将任务参数导出Json"), FoldoutGroup("任务参数标识添加")]
        void CreateParam(string filePath = "Assets/Mission", string fileName = "missionParamData")
        {
            SerializationTool.SerializedObject(mMissionParams_dict).CreateFileStream(filePath, fileName, ".json");
        }

        [Button("导入任务类型Json"), FoldoutGroup("任务类型添加"), PropertySpace(10)]
        void LoadType(TextAsset textAsset)
        {
            if (!textAsset) return;
            mMissionTypes_dict = SerializationTool.DeserializedObject<YDictionary<string, string>>(textAsset.text);
        }

        [Button("导入任务参数Json"), FoldoutGroup("任务参数标识添加"),PropertySpace(10)]
        void LoadParam(TextAsset textAsset)
        {
            if (!textAsset) return;
            mMissionParams_dict = SerializationTool.DeserializedObject<YDictionary<string, MissionParam>>(textAsset.text);
        }

        [Button("打开任务配置窗口集合(Plus)",ButtonHeight = 50),PropertySpace(30)]
		static void OpenWindow()
		{
			MissionDesignWindow.OpenWindow();
		}

        private void Reset()
        {
            OnValidate();
        }

        private void OnValidate()
        {
            var items = YukiAssetDataBase.FindAssets<MissionConfigBase>();

            YDictionary<string, string> mMissionTypes = new YDictionary<string, string>();
            YDictionary<string, MissionParam> mMissionParams = new YDictionary<string, MissionParam>();
            foreach (var item in items)
            {
                foreach (var dict in item.mMissionParams_dict)
                {
                    mMissionParams[dict.Key] = dict.Value;
                }
                foreach (var dict in item.mMissionTypes_dict)
                {
                    mMissionTypes[dict.Key] = dict.Value;
                }         
                
            }

            foreach (var item in items)
            {
                item.mMissionTypes_dict = mMissionTypes;
                item.mMissionParams_dict = mMissionParams;
            }
           

        }
#endif
        [SerializeField,LabelText("任务类型")]
		[InfoBox("所有的配置中的任务类型均同步共享，a配置有的任务类型在b配置中也可以为任务选择使用!"),FoldoutGroup("任务类型添加")]
        [DictionaryDrawerSettings(KeyLabel = "任务类型",ValueLabel = "编辑器显示")]
		private YDictionary<string, string> mMissionTypes_dict = new YDictionary<string, string>();

        [SerializeField,LabelText("任务参数")]
        [InfoBox("所有的配置中的任务参数均同步共享，a配置有的任务参数在b配置中也可以为参数选择使用!"), FoldoutGroup("任务参数标识添加")]
        [DictionaryDrawerSettings(KeyLabel = "参数名称/标识",ValueLabel = "可配置数据项")]
        internal YDictionary<string, MissionParam> mMissionParams_dict = new YDictionary<string, MissionParam>();
	}
    public class MissionConfigBase<T> : MissionConfigBase, IExcelSyncScriptableObject where T : IMissionData
    {
		[SerializeField,LabelText("任务集合"),ListDrawerSettings(ListElementLabelName = "Name",NumberOfItemsPerPage = 5),Searchable(), FoldoutGroup("任务集合")]
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

        protected override void ResetAllMissionsType()
        {
			foreach (var item in missions)
			{
				item.MissionType = string.Empty;
			}
        }

        public void Create(int maxLength)
        {
            missions = new T[maxLength];
        }

        public void Import(int index, object userData)
        {
            missions[index] = (T)userData;
        }

        public void Completed() { }
    } 
}
