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
namespace YukiFrameWork.Missions
{
    public class UIMission : YMonoBehaviour
    {
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
        /// 当任务调用ResetMission方法重置/第一次加载回待机状态时触发
        /// </summary>
        [LabelText("任务处于待机/未执行时触发事件")]
        [InfoBox("该事件在任务创建时就会调用一次")]
        public UnityEvent<IMissionData> onMissionIdle;
        /// <summary>
        /// 当任务开始时触发
        /// </summary>
        [LabelText("当任务成功开始时触发事件")]
        public UnityEvent<IMissionData> onMissionStarting;
        /// <summary>
        /// 当任务完成时触发
        /// </summary>
        [LabelText("任务完成时触发事件")]
        public UnityEvent<IMissionData> onMissionCompleted;
        /// <summary>
        /// 当任务失败时触发
        /// </summary>
        [LabelText("当任务失败时触发事件")]
        public UnityEvent<IMissionData> onMissionFailed;
       

        public Mission Mission { get; private set; }

        [ShowInInspector]
        public MissionStatus MissionStatus => Mission == null ? default : Mission.Status;
        
        public void InitMission(Mission mission)
        {          
            if (Mission != null)
            {
                Mission.onMissionIdle.UnRegister(MissionIdle);
                Mission.onMissionCompleted.UnRegister(MissionCompleted);
                Mission.onMissionFailed.UnRegister(MissionFailed);
                Mission.onMissionStarting.UnRegister(MissionStarting);
            }
            this.Mission = mission;           
            Mission.onMissionIdle.RegisterEvent(MissionIdle);
            Mission.onMissionCompleted.RegisterEvent(MissionCompleted);
            Mission.onMissionFailed.RegisterEvent(MissionFailed); 
            Mission.onMissionStarting.RegisterEvent(MissionStarting);
            switch (Mission.Status)
            {
                case MissionStatus.Idle:
                    MissionIdle(mission.MissionData);
                    break;
                case MissionStatus.Running:
                    MissionStarting(mission.MissionData);
                    break;
                case MissionStatus.Completed:
                    MissionCompleted(mission.MissionData);
                    break;
                case MissionStatus.Failed:
                    MissionFailed(mission.MissionData);
                    break;             
            }

            onUIMissionName?.Invoke(mission.MissionData.Name);
            onUIMissionDes?.Invoke(mission.MissionData.Description);
            onUIMissionIcon?.Invoke(mission.MissionData.Icon);
            

            Mission.Group.onUIMissionInit?.Invoke(this);

        }

        private void MissionIdle(IMissionData arg) => onMissionIdle?.Invoke(arg);
        private void MissionCompleted(IMissionData arg) => onMissionCompleted?.Invoke(arg);
        private void MissionFailed(IMissionData arg) => onMissionFailed?.Invoke(arg);
        private void MissionStarting(IMissionData arg) => onMissionStarting?.Invoke(arg);

        public void StartMission() => Mission.StartMission();

        private void OnDestroy()
        {
            if (Mission != null)
            {
                Mission.onMissionIdle.UnRegister(MissionIdle);
                Mission.onMissionCompleted.UnRegister(MissionCompleted);
                Mission.onMissionFailed.UnRegister(MissionFailed);
                Mission.onMissionStarting.UnRegister(MissionStarting);
            }
        }
    }
}
 