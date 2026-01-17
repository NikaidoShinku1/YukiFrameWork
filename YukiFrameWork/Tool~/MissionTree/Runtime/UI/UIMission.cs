///=====================================================
/// - FileName:      UIMission.cs
/// - NameSpace:     YukiFrameWork.Missions
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/10/27 0:31:54
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using UnityEngine.UI;
using System.Reflection;
using System.Collections;
using System.Linq;
namespace YukiFrameWork.Missions
{
    public class UIMission : YMonoBehaviour
    {
        [SerializeField,LabelText("任务树标识")]
        private string missionTreeKey;
        [SerializeField,LabelText("任务唯一Id")]
        private int missionId;

        [FoldoutGroup("基本设置")]
        [LabelText("任务名称事件触发")]
        public UnityEvent<string> onUIMissionName;

        [FoldoutGroup("基本设置")]
        [LabelText("任务介绍事件触发")]
        public UnityEvent<string> onUIMissionDes;

        [FoldoutGroup("基本设置")]
        [LabelText("任务图标事件触发")]
        public UnityEvent<Sprite> onUIMissionIcon;

        /// <summary>
        /// 当任务处于未解锁状态时触发
        /// </summary>
        [InfoBox("该事件在任务创建时就会调用一次")]
        public UnityEvent<Mission> onMissionLock;
        /// <summary>
        /// 当任务调用ResetMission方法重置/第一次加载回待机状态时触发
        /// </summary>
        [LabelText("任务处于待机/未执行时触发事件")]       
        public UnityEvent<Mission> onMissionReset;
        /// <summary>
        /// 当任务开始时触发
        /// </summary>
        [LabelText("当任务成功开始时触发事件")]
        public UnityEvent<Mission, object[]> onMissionStarting;
        /// <summary>
        /// 当任务完成时触发
        /// </summary>
        [LabelText("任务完成时触发事件")]
        public UnityEvent<Mission> onMissionCompleted;
        /// <summary>
        /// 当任务失败时触发
        /// </summary>
        [LabelText("当任务失败时触发事件")]
        public UnityEvent<Mission> onMissionFailed;

        public string MissionTreeKey
        {
            get => missionTreeKey;
            set
            {
                if (value.IsNullOrEmpty()) return;
                if (missionTreeKey != value)
                {
                    missionTreeKey = value;
                    Refresh();
                }
            }
        }

        public int MissionId
        {
            get => missionId;
            set
            {
                missionId = value;
                Refresh();
            }
        }

        public MissionTree MissionTree => MissionTree.GetMissionTree(MissionTreeKey);
        public MissionController MissionController { get; private set; }

        private void OnEnable()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (missionTreeKey.IsNullOrEmpty()) return;
            ReleaseControllerEvent();
            MissionTree.TryFindMissionController(MissionId,out var controller);
            MissionController = controller;
            if (MissionController == null) return;
            MissionController = MissionTree.FindMissionController(MissionId);
            MissionController.onReset += MissionReset;
            MissionController.onCompleted += MissionCompleted;
            MissionController.onFailed += MissionFailed;
            MissionController.onStart += MissionStarting;
            MissionController.onLock += MissionLock;
            switch (MissionController.MissionStatus)
            {
                case MissionStatus.Lock:
                    MissionLock(MissionController.Mission);
                    break;
                case MissionStatus.InActive:
                    MissionReset(MissionController.Mission);
                    break;
                case MissionStatus.Running:
                    MissionStarting(MissionController.Mission);
                    break;
                case MissionStatus.Success:
                    MissionCompleted(MissionController.Mission);
                    break;
                case MissionStatus.Failed:
                    MissionFailed(MissionController.Mission);
                    break;
            }

            onUIMissionName?.Invoke(MissionController.Mission.MissionName);
            onUIMissionDes?.Invoke(MissionController.Mission.Description);
            onUIMissionIcon?.Invoke(MissionController.Mission.Icon);
        }

        private void MissionLock(Mission arg) => onMissionLock?.Invoke(arg);
        private void MissionReset(Mission arg) => onMissionReset?.Invoke(arg);
        private void MissionCompleted(Mission arg) => onMissionCompleted?.Invoke(arg);
        private void MissionFailed(Mission arg) => onMissionFailed?.Invoke(arg);
        private void MissionStarting(Mission arg,params object[] param) => onMissionStarting?.Invoke(arg,param);

        public void StartMission()
        {
            if (MissionController == null)           
                throw new NullReferenceException("丢失任务控制器，请检查Id与任务树标识是否填写正确");
            MissionTree.StartMission(MissionId);

        }

        public void ResetMission()
        {
            if (MissionController == null)
                throw new NullReferenceException("丢失任务控制器，请检查Id与任务树标识是否填写正确");
            MissionTree.ResetMission(MissionId);

        }

        public void UnLockMission()
        {
            if (MissionController == null)
                throw new NullReferenceException("丢失任务控制器，请检查Id与任务树标识是否填写正确");

            MissionTree.UnLockMission(MissionId);

        }

        public void LockMission()
        {
            if (MissionController == null)
                throw new NullReferenceException("丢失任务控制器，请检查Id与任务树标识是否填写正确");

            MissionTree.LockMission(MissionId);

        }

        private void OnDestroy()
        {
            ReleaseControllerEvent();
        }

        private void ReleaseControllerEvent()
        {
            if (MissionController != null)
            {
                MissionController.onReset -= MissionReset;
                MissionController.onCompleted -= MissionCompleted;
                MissionController.onFailed -= MissionFailed;
                MissionController.onStart -= MissionStarting;
                MissionController.onLock -= MissionLock;
            }
        }
    }
}
 