///=====================================================
/// - FileName:      MissionTree.cs
/// - NameSpace:     YukiFrameWork.Missions
/// - Description:   高级定制脚本生成
/// - Creation Time: 1/12/2026 10:58:02 PM
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using YukiFrameWork.Pools;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YukiFrameWork.Extension;
namespace YukiFrameWork.Missions
{   
    public class MissionTree : IGlobalSign
    {
        private static IMissionLoader missionLoader = null;
        static MissionTree()
        {
            if (!Application.isPlaying) return;

            GlobalObjectPools.SetGlobalPoolsBySize<MissionTree>(200, new MissionTreeGenerator());
        }
        private static Dictionary<string, MissionTree> runtimeMissionTrees = new Dictionary<string, MissionTree>();

        internal static IEnumerable<MissionTree> runtime_MissionTreeSO => runtimeMissionTrees.Values;

        public static void Init(string projectName)
        {
            Init(new ABManagerMissionLoader(projectName));
        }

        public static void Init(IMissionLoader missionLoader)
        {
            MissionTree.missionLoader = missionLoader;
        }

        /// <summary>
        /// 任务树的持久化数据
        /// <para>该属性返回全部已保存任务树的Json数据。</para>
        /// </summary>
        public static string PersistenceMissionData
        {
            get
            {
                List<MissionPersistence> missionPersistences = new List<MissionPersistence>();
                
                foreach (var item in runtimeMissionTrees)
                {
                    MissionPersistence persistence = new MissionPersistence()
                    {
                        missionTreekey = item.Key,
                        missionPersistenceInfos = item.Value.runtime_Missions_Controllers.Select(x =>
                        {
                            return new MissionPersistence.MissionPersistenceInfo()
                            {
                                missionId = x.Key,
                                missionStatus = x.Value.MissionStatus
                            };
                        }).ToList()
                    };

                    missionPersistences.Add(persistence);
                   
                }
                return SerializationTool.SerializedObject(missionPersistences);
            }
        }

        /// <summary>
        /// 是否处于持久化获取数据中,当该属性为True时,MissionTree任何对任务的解锁启动重置都不会生效
        /// </summary>
        public static bool IsPersistence { get; private set; }

        /// <summary>
        /// 创建指定标识的任务树，如果已经有了则直接返回
        /// </summary>
        /// <param name="key"></param>
        /// <param name="missionTreeSO"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public static MissionTree Create(MissionTreeSO missionTreeSO,params object[] param)
        {
            string key = missionTreeSO.missionTreeKey;
            if (runtimeMissionTrees.TryGetValue(key, out MissionTree missionTree))
                return missionTree;

            missionTree = GlobalObjectPools.GlobalAllocation<MissionTree>();
            runtimeMissionTrees[key] = missionTree;
            missionTree.Key = key;
            missionTree.MissionTreeSO = missionTreeSO.Clone();          
            missionTree.BuildAllController(param);
           
            if (missionLoader != null)
                missionLoader.UnLoad(missionTreeSO);

            return missionTree;
        }

        public static MissionTree Create(string nameOrPath, params object[] param)
        {
            return Create(missionLoader.Load<MissionTreeSO>(nameOrPath), param);
        }

#if UNITY_2021_1_OR_NEWER
        public static async YieldTask<MissionTree> CreateAsync(string nameOrPath, params object[] param)
        {
            bool isCompleted = false;

            MissionTree missionTree = null;

            missionLoader.LoadAsync<MissionTreeSO>(nameOrPath, s => 
            {
                missionTree = Create(s, param);
                isCompleted = true;
            });

            await CoroutineTool.WaitUntil(() => isCompleted);
            return missionTree;

        }


#else
        public static IEnumerator CreateAsync(string nameOrPath, params object[] param)
        {
            bool isCompleted = false;

            missionLoader.LoadAsync<MissionTreeSO>(nameOrPath, s =>
            {
                Create(s, param);
                isCompleted = true;
            });

            yield return CoroutineTool.WaitUntil(() => isCompleted);          
        }
#endif

        /// <summary>
        /// 根据标识获取到指定的任务树
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="NullReferenceException"></exception>
        public static MissionTree GetMissionTree(string key)
        {
            if (runtimeMissionTrees.TryGetValue(key, out MissionTree missionTree))
                return missionTree;

            throw new NullReferenceException("查找不到指定的任务树!请重试:Key:" + key);

        }

        /// <summary>
        /// 根据标识获取到指定的任务树，不会触发异常
        /// </summary>
        /// <param name="key"></param>
        /// <param name="missionTree"></param>
        /// <returns></returns>
        public static bool TryGetMissionTree(string key, out MissionTree missionTree)
        {
            missionTree = null;
            try
            {
                missionTree = GetMissionTree(key);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 加载行为树持久化数据
        /// <para>加载数据的前提是已经构建了数据内包含的指定标识的行为树,才可以同步数据</para>
        /// </summary>
        /// <param name="persistence"></param>
        public static void LoadPersistenceMissionTrees(string persistence)
        {
            IsPersistence = true;
            List<MissionPersistence> missionPersistences = SerializationTool.DeserializedObject<List<MissionPersistence>>(persistence);

            if (missionPersistences == null)
                throw new Exception("读取数据失败!请检查持久化数据是否出错! persistence:" + persistence);

            if (missionPersistences.Count == 0) return;

            for (int i = 0; i < missionPersistences.Count; i++)
            {
                var persistenceData = missionPersistences[i];

                if (TryGetMissionTree(persistenceData.missionTreekey, out var missionTree))
                {
                    for (int j = 0; j < persistenceData.missionPersistenceInfos.Count; j++)
                    {
                        var info = persistenceData.missionPersistenceInfos[j];
                        if (missionTree.TryFindMissionController(info.missionId, out var controller))                        
                            controller.ChangeStatus(info.missionStatus);                      

                    }
                }
            }

            IsPersistence = false;
        }

        /// <summary>
        /// 释放指定标识的任务树
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool ReleaseMissionTree(string key)
        {
            if (runtimeMissionTrees.ContainsKey(key))
                runtimeMissionTrees[key].GlobalRelease();

            return runtimeMissionTrees.Remove(key);
        }

        private MissionTree() { }

        private Dictionary<int, MissionController> runtime_Missions_Controllers = new Dictionary<int, MissionController>();

        /// <summary>
        /// 任务树的唯一标识
        /// </summary>
        public string Key { get; private set; }

        public bool IsMarkIdle { get; set; }
        
        /// <summary>
        /// 该行为树依赖的so配置
        /// <para>Tips:数据为克隆数据，任何修改都不会作用于Editor环境下的本配置</para>
        /// </summary>
        public MissionTreeSO MissionTreeSO { get; private set; }

        /// <summary>
        /// 根据标识查找到对应的任务控制器
        /// </summary>
        /// <param name="missionId"></param>
        /// <returns></returns>
        public MissionController FindMissionController(int missionId)
        {
            if(runtime_Missions_Controllers.TryGetValue(missionId, out var controller))
                 return controller;
            throw new NullReferenceException("任务控制器丢失!请检查 MissionId:" + missionId);
        }

        /// <summary>
        /// 根据标识查找到对应的任务控制器，不会抛出异常
        /// </summary>
        /// <param name="missionId"></param>
        /// <param name="missionController"></param>
        /// <returns></returns>
        public bool TryFindMissionController(int missionId, out MissionController missionController)
        {
            missionController = null;
            try
            {
                missionController = FindMissionController(missionId);
                return true;
            }
            catch 
            {
                return false;
            }
        }

        /// <summary>
        /// 根据标识查找到对应的任务控制器，不会抛出异常
        /// </summary>
        /// <param name="missionId"></param>
        /// <param name="missionController"></param>
        /// <returns></returns>
        public bool TryFindMissionController(Mission mission, out MissionController missionController)
        {
            return TryFindMissionController(mission.MissionId, out missionController);
        }

        /// <summary>
        /// 通过任务数据本身查找到对应的任务控制器
        /// </summary>
        /// <param name="mission"></param>
        /// <returns></returns>
        public MissionController FindMissionController(Mission mission)
        {
            return FindMissionController(mission.MissionId);
        }

        /// <summary>
        /// 遍历所有的任务控制器
        /// </summary>
        /// <param name="each"></param>
        public void ForEach(Action<MissionController> each)
        {
            foreach (var item in runtime_Missions_Controllers)
            {
                each?.Invoke(item.Value);
            }
        }

        /// <summary>
        /// 启动所有任务
        /// </summary>
        public void StartMissions(Func<bool> condition)
        {
            foreach (var item in runtime_Missions_Controllers)
            {
                if (!condition()) continue;
                StartMission(item.Key);
            }
        }
        /// <summary>
        /// 启动所有任务
        /// </summary>
        public void StartMissions()
        {
            StartMissions(() => true);
        }

        /// <summary>
        /// 解锁所有任务
        /// </summary>
        public void UnLockMissions()
        {
            UnLockMissions(() => true);
        }

        /// <summary>
        /// 解锁所有任务
        /// </summary>
        /// <param name="condition"></param>
        public void UnLockMissions(Func<bool> condition)
        {
            foreach (var item in runtime_Missions_Controllers)
            {
                //if (item.Value.IsChild)
                //    continue;

                if (!condition()) continue;
                UnLockMission(item.Key);
            }
        }

        /// <summary>
        /// 重置所有任务
        /// <para>仅能确保任务恢复到待机状态,如需要完全重置，则可调用LockMissions进行对任务的锁定</para>
        /// </summary>
        public void ResetMissions()
        {
            ResetMissions(() => true);
        }

        /// <summary>
        /// 重置所有任务
        /// <para>仅能确保任务恢复到待机状态,如需要完全重置，则可调用LockMission进行对任务的锁定</para>
        /// </summary>
        /// <param name="condition"></param>
        public void ResetMissions(Func<bool> condition)
        {
            foreach (var item in runtime_Missions_Controllers)
            {
               // if (item.Value.IsChild)
               //     continue;

                if (!condition()) continue;
                ResetMission(item.Key);
            }
        }

        /// <summary>
        /// 锁定所有任务
        /// </summary>
        public void LockMissions()
        {
            LockMissions(() => true);
        }

        /// <summary>
        /// 锁定所有任务
        /// </summary>
        /// <param name="condition"></param>
        public void LockMissions(Func<bool> condition)
        {
            foreach (var item in runtime_Missions_Controllers)
            {
                //if (item.Value.IsChild)
                //    continue;

                if (!condition()) continue;
                LockMission(item.Key);
            }
        }
        /// <summary>
        /// 根据标识解锁任务
        /// </summary>
        /// <param name="missionId"></param>
        /// <exception cref="Exception"></exception>
        public bool UnLockMission(int missionId)
        {
            return UnLockMission(missionId, out _);
        }
        /// <summary>
        /// 根据标识解锁任务
        /// </summary>
        /// <param name="missionId"></param>
        /// <exception cref="Exception"></exception>
        public bool UnLockMission(int missionId, out string failedTip)
        {
            failedTip = string.Empty;
            if (IsPersistence)
            {
                failedTip = $"MissionTree处于持久化写入数据中,访问无效 MissionTree Key:{Key}";
                return false;
            }    
          
            if (runtime_Missions_Controllers.TryGetValue(missionId, out var controller))
            {
                if (controller.Mission.MissionStatus != MissionStatus.Lock)
                {
                    failedTip = $"Mission:{missionId}并非锁定状态,无法解锁";
                    return false;
                }

                if (!controller.MissionIsUnLock())
                {
                    failedTip = $"Mission:{missionId}本身不满足可解锁的条件";
                    return false;
                }

                controller.ChangeStatus(MissionStatus.InActive);
                return true;

            }
            else
            {
                throw new Exception("任务丢失 Id:" + missionId);
            }
            
        }

        /// <summary>
        /// 根据标识解锁任务
        /// </summary>
        /// <param name="missionId"></param>
        /// <exception cref="Exception"></exception>
        public bool UnLockMission(Mission mission) => UnLockMission(mission.MissionId);

        /// <summary>
        /// 根据标识解锁任务
        /// </summary>
        /// <param name="missionId"></param>
        /// <exception cref="Exception"></exception>
        public bool UnLockMission(Mission mission, out string failedTip) => UnLockMission(mission.MissionId, out failedTip);

        /// <summary>
        /// 根据标识锁定任务
        /// </summary>
        /// <param name="missionId"></param>
        /// <exception cref="Exception"></exception>
        public bool LockMission(int missionId,out string failedTip)
        {
            failedTip = string.Empty;
            if (IsPersistence)
            {
                failedTip = $"MissionTree处于持久化写入数据中,访问无效 MissionTree Key:{Key}";
                return false;
            }

            if (runtime_Missions_Controllers.TryGetValue(missionId, out var controller))
            {
                //if (controller.IsChild)
                //    throw new Exception($"指定的任务存在父节点，请通过该任务的MissionController进行对子任务的锁定 Mission Status:{controller.MissionStatus} Mission Id:{controller.Mission.MissionId} Parent Count:{controller.Parents.Count}");

                if (controller.Mission.MissionStatus == MissionStatus.Lock)
                {
                    failedTip = $"Mission:{missionId}已经是锁定状态了";
                    return false;
                }
                controller.ChangeStatus(MissionStatus.Lock);
                return true;

            }
            else
            {
                throw new Exception("任务丢失 Id:" + missionId);
            }

        }

        /// <summary>
        /// 根据标识锁定任务
        /// </summary>
        /// <param name="missionId"></param>
        /// <returns></returns>
        public bool LockMission(int missionId) => LockMission(missionId, out _);

        /// <summary>
        /// 根据标识锁定任务
        /// </summary>
        /// <param name="missionId"></param>
        /// <returns></returns>
        public bool LockMission(Mission mission) => LockMission(mission.MissionId);

        /// <summary>
        /// 根据标识锁定任务
        /// </summary>
        /// <param name="missionId"></param>
        /// <returns></returns>
        public bool LockMission(Mission mission,out string failedTip) => LockMission(mission.MissionId,out failedTip);

        /// <summary>
        /// 根据标识启动任务
        /// </summary>
        /// <param name="missionId"></param>
        /// <exception cref="Exception"></exception>
        public bool StartMission(int missionId,out string failedTip, params object[] param)
        {
            failedTip = string.Empty;
            if (IsPersistence)
            {
                failedTip = $"MissionTree处于持久化写入数据中,访问无效 MissionTree Key:{Key}";
                return false;
            }

            if (runtime_Missions_Controllers.TryGetValue(missionId, out var controller))
            {
                // if (controller.IsChild)
                //     throw new Exception($"任务具有父节点控制,无法手动启动任务，请通过对应的MissionController启动子任务 Mission Status:{controller.MissionStatus} Mission Id:{controller.Mission.MissionId} Parent Count:{controller.Parents.Count}");
                if (controller.MissionStatus != MissionStatus.InActive)
                {
                    failedTip = $"任务不处于InActive状态,请检查是否重复启动或未解锁,如任务已经完成或失败，则需要重置任务后才可以启动! Mission Status:{controller.MissionStatus} Mission Id:{missionId}";
                    return false;
                }
                    //throw new Exception($"任务不处于InActive状态,请检查是否重复启动或未解锁,如任务已经完成或失败，则需要重置任务后才可以启动! Mission Status:{controller.MissionStatus} Mission Id:{missionId}");

                controller.ChangeStatus(MissionStatus.Running,param);
                return true;
               
            }
            else
            {
                throw new Exception("任务丢失 Id:" + missionId);
            }
        }
        /// <summary>
        /// 根据标识启动任务
        /// </summary>
        /// <param name="missionId"></param>
        /// <exception cref="Exception"></exception>
        public bool StartMission(Mission mission,params object[] param) => StartMission(mission.MissionId,param);
        /// <summary>
        /// 根据标识启动任务
        /// </summary>
        /// <param name="missionId"></param>
        /// <exception cref="Exception"></exception>
        public bool StartMission(Mission mission,out string failedTip, params object[] param) => StartMission(mission.MissionId,out failedTip,param);
        /// <summary>
        /// 根据标识启动任务
        /// </summary>
        /// <param name="missionId"></param>
        /// <exception cref="Exception"></exception>
        public bool StartMission(int missionId, params object[] param) => StartMission(missionId, out _,param);

        /// <summary>
        /// 根据标识重置任务
        /// </summary>
        /// <param name="missionId"></param>
        /// <exception cref="Exception"></exception>
        public bool ResetMission(int missionId,out string failedTip)
        {
            failedTip = string.Empty;
            if (IsPersistence)
            {
                failedTip = $"MissionTree处于持久化写入数据中,访问无效 MissionTree Key:{Key}";
                return false;
            }

            if (runtime_Missions_Controllers.TryGetValue(missionId, out var controller))
            {             
                if (controller.MissionStatus == MissionStatus.InActive || controller.MissionStatus == MissionStatus.Lock)
                {
                    failedTip = $"任务是没有被解锁的/已经是待机状态!不会进行重置 Mission Id:{missionId}";
                    return false;
                }
                controller.ChangeStatus(MissionStatus.InActive);
                return true;
            }
            else
            {
                throw new Exception("任务丢失 Id:" + missionId);
            }
        }

        /// <summary>
        /// 根据标识重置任务
        /// </summary>
        /// <param name="missionId"></param>
        /// <exception cref="Exception"></exception>
        public bool ResetMission(Mission mission) => ResetMission(mission.MissionId);

        /// <summary>
        /// 根据标识重置任务
        /// </summary>
        /// <param name="missionId"></param>
        /// <exception cref="Exception"></exception>
        public bool ResetMission(Mission mission, out string failedTip) => StartMission(mission.MissionId, out failedTip);

        /// <summary>
        /// 根据标识重置任务
        /// </summary>
        /// <param name="missionId"></param>
        /// <exception cref="Exception"></exception>
        public bool ResetMission(int missionId) => StartMission(missionId, out _);


        private void BuildAllController(params object[] param)
        {
            MissionTreeSO.ForEach(mission => 
            {
                MissionController controller = MissionController.CreateInstance(mission, this);
                runtime_Missions_Controllers[mission.MissionId] = controller;
            });
            foreach (var item in runtime_Missions_Controllers)
            {
                item.Value.UpdateAllChildMissionController();
            }

            foreach (var controller in runtime_Missions_Controllers.Values)
            {              
                controller.Create(param);
            }
        } 

        private void Update(MonoHelper _) 
        {
            foreach (var item in runtime_Missions_Controllers.Values)
            {
                //if (item.IsChild) continue;
                if (item.MissionStatus == MissionStatus.Running)
                {
                    item.Update();
                }


            }
        }

        private void FixedUpdate(MonoHelper _) 
        {
            foreach (var item in runtime_Missions_Controllers.Values)
            {
                //if (item.IsChild) continue;
                if (item.MissionStatus == MissionStatus.Running)
                {
                    item.FixedUpdate();
                }


            }
        }

        private void LateUpdate(MonoHelper _) 
        {
            foreach (var item in runtime_Missions_Controllers.Values)
            {
                //if (item.IsChild) continue;
                if (item.MissionStatus == MissionStatus.Running)
                {
                    item.LateUpdate();
                }
            }
        }

        void IGlobalSign.Init()
        {
            MonoHelper.Update_RemoveListener(Update);
            MonoHelper.FixedUpdate_RemoveListener(FixedUpdate);
            MonoHelper.LateUpdate_RemoveListener(LateUpdate);
            MonoHelper.Update_AddListener(Update);
            MonoHelper.FixedUpdate_AddListener(FixedUpdate);
            MonoHelper.LateUpdate_AddListener(LateUpdate);
        }

        void IGlobalSign.Release()
        {
            MonoHelper.Update_RemoveListener(Update);
            MonoHelper.FixedUpdate_RemoveListener(FixedUpdate);
            MonoHelper.LateUpdate_RemoveListener(LateUpdate);
            Key = string.Empty;
            MissionTreeSO = null;
            foreach (var item in runtime_Missions_Controllers)
            {
                item.Value.GlobalRelease();
            }
            runtime_Missions_Controllers.Clear();
        }

        class MissionTreeGenerator : IPoolGenerator
        {
            public Type Type => typeof(MissionTree);

            public IGlobalSign Create()
            {
                return new MissionTree();
            }
        }
    }
}
