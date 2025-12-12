///=====================================================
/// - FileName:      EquipmentKit.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   高级定制脚本生成
/// - Creation Time: 12/11/2025 6:03:56 PM
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System.Collections;
using YukiFrameWork.Extension;
using YukiFrameWork.Pools;
namespace YukiFrameWork.Equips
{
    public interface IEquipExecutor
    {
        /// <summary>
        /// 能否装备
        /// </summary>
        /// <returns></returns>
        bool IsCanEquip();
    }
    public interface IEquipment : IGlobalSign
    {
        /// <summary>
        /// 装备执行者
        /// </summary>
        IEquipExecutor EquipExecutor { get; set; }
        /// <summary>
        /// 装备关联的数据配置
        /// </summary>
        IEquipmentData EquipmentData { get; set; }
        /// <summary>
        /// 该装备是否可以被装备
        /// </summary>
        /// <returns></returns>
        bool IsCanEquip();
        /// <summary>
        /// 当装备时
        /// </summary>
        /// <param name="param"></param>
        void OnEquip(params object[] param);
        /// <summary>
        /// 当卸下装备时
        /// </summary>
        void OnUnEquip();
    }
    public enum EquipState
    {
        /// <summary>
        /// 装备成功
        /// </summary>
        [LabelText("装备成功")]
        Success,

        /// <summary>
        /// 当前游戏角色不能装备该装备
        /// </summary>
        [LabelText("当前游戏角色不能装备该装备")]
        CannotEquipByExecutor,

        /// <summary>
        /// 当前装备本身无法被装备
        /// </summary>
        CannotEquip,

        /// <summary>
        /// 已经装备
        /// </summary>
        [LabelText("已经装备")]
        AlreadyEquipped
    }
    /// <summary>
    /// 装备系统套件
    /// </summary>
    public static class EquipmentKit
    {
        private readonly static Dictionary<string, IEquipmentData> runtime_equipmentDatas = new Dictionary<string, IEquipmentData>();
        private static IEquipmentLoader equipmentLoader;
        private readonly static Dictionary<IEquipExecutor, List<IEquipment>> runtime_executor_equipments = new Dictionary<IEquipExecutor, List<IEquipment>>();
        private static List<IEquipExecutor> releases = new List<IEquipExecutor>();
        private static void CacheExecutor(IEquipExecutor player)
        {
            if (!runtime_executor_equipments.ContainsKey(player))
                runtime_executor_equipments[player] = new List<IEquipment>();
        }

        static EquipmentKit()
        {
            MonoHelper.LateUpdate_RemoveListener(LateUpdate);
            MonoHelper.LateUpdate_AddListener(LateUpdate);
        }

        private static void LateUpdate(MonoHelper monoHelper)
        {
            foreach (var item in runtime_executor_equipments)
            {
                if (item.Key is UnityEngine.Object obj)
                {
                    if (!obj)
                    {
                        releases.Add(item.Key);
                    }
                }
            }

            if (releases.Count == 0) return;

            foreach (var item in releases)
            {
                runtime_executor_equipments.Remove(item);
            }

            releases.Clear();
        }

        public static void Init(string projectName)
        {
            Init(new ABManagerEquipmentLoader(projectName));
        }

        public static void Init(IEquipmentLoader equipmentLoader)
        {
            EquipmentKit.equipmentLoader = equipmentLoader;
        }

        /// <summary>
        /// 加载装备管理器
        /// </summary>      
        public static void LoadEquipmentConfigDataManager(EquipmentConfigDataManager equipmentConfigDataManager)
        {
            foreach (var configBase in equipmentConfigDataManager.equipmentConfigBases)
            {
                foreach (var item in configBase.Equipments)
                {
                    runtime_equipmentDatas.Add(item.Key, item);
                }
            }
            equipmentLoader?.UnLoad(equipmentConfigDataManager);
        }

        /// <summary>
        /// 加载装备管理器
        /// </summary>
        /// <param name="path"></param>
        public static void LoadEquipmentConfigDataManager(string path)
        {
            LoadEquipmentConfigDataManager(equipmentLoader.Load<EquipmentConfigDataManager>(path));
        }

        /// <summary>
        /// 异步加载装备管理器
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IEnumerator LoadMissionConfigManagerAsync(string path)
        {
            bool completed = false;
            equipmentLoader.LoadAsync<EquipmentConfigDataManager>(path, item => {
                LoadEquipmentConfigDataManager(item);
                completed = true;
            });

            yield return CoroutineTool.WaitUntil(() => completed);
        }

        /// <summary>
        /// 添加装备信息
        /// </summary>
        /// <param name="equipmentData"></param>
        public static void AddEquipmentData(IEquipmentData equipmentData)
        {
            runtime_equipmentDatas.Add(equipmentData.Key, equipmentData);
        }

        /// <summary>
        /// 移除装备信息
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool RemoveEquipmentData(string key)
            => runtime_equipmentDatas.Remove(key);

        /// <summary>
        /// 获取装备信息
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IEquipmentData GetEquipmentData(string key)
        {
            return runtime_equipmentDatas[key];
        }

        // public static 

        /// <summary>
        /// 根据标识创建装备
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static IEquipment CreateEquipment(string key)
        {
            if (!runtime_equipmentDatas.TryGetValue(key, out var data))
            {
                throw new Exception("没有添加指定标识的装备信息!Key:" + key);
            }

            if (data.EquipmentType.IsNullOrEmpty())
                throw new Exception($"装备:{key}的指定装备类型为空，无法创建装备");

            Type equipmentType = AssemblyHelper.GetType(data.EquipmentType);

            if (equipmentType == null)
                throw new Exception($"装备:{key}的指定装备类型错误，无法转换创建装备");

            IEquipment equipment = GlobalObjectPools.GlobalAllocation(equipmentType) as IEquipment;

            if (equipment == null)
                throw new Exception("装备丢失!");

            equipment.EquipExecutor = null;
            equipment.EquipmentData = data;

            return equipment;
        }


        /// <summary>
        /// 根据装备信息创建装备
        /// </summary>
        /// <param name="equipmentData"></param>
        /// <returns></returns>
        public static IEquipment CreateEquipment(IEquipmentData equipmentData)
        {
            return CreateEquipment(equipmentData.Key);
        }

        /// <summary>
        /// 创建装备并装备给执行者
        /// </summary>
        /// <param name="player">执行者玩家</param>
        /// <param name="key">标识</param>
        /// <param name="equipState">装备的装备状态</param>
        /// <param name="param">装备参数</param>
        /// <returns></returns>
        public static IEquipment CreateEquipmentAndEquip(this IEquipExecutor player,string key, out EquipState equipState,params object[] param)
        {
            IEquipment equipment = CreateEquipment(key);
            equipState = Equip(player,equipment,param);
            return equipment;
        }

        /// <summary>
        /// 为执行者装备
        /// </summary>
        /// <param name="player"></param>
        /// <param name="equipment"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static EquipState Equip(this IEquipExecutor player, IEquipment equipment, params object[] param)
        {
            if (player == null)
                throw new Exception("执行者丢失!");

            CacheExecutor(player);

            List<IEquipment> equipments = runtime_executor_equipments[player];

            //如果已经有这个装备了则返回
            if (equipments.Contains(equipment))
                return EquipState.AlreadyEquipped;

            if (!player.IsCanEquip())
                return EquipState.CannotEquipByExecutor;

            if (!equipment.IsCanEquip())
                return EquipState.CannotEquip;
            equipments.Add(equipment);
            equipment.EquipExecutor = player;
            equipment.OnEquip(param);
            runtime_executor_equipments[player] = equipments;
            return EquipState.Success;


        }
        /// <summary>
        /// 为执行者卸下装备
        /// </summary>
        /// <param name="player"></param>
        /// <param name="equipment"></param>
        public static void UnEquip(this IEquipExecutor player, IEquipment equipment)
        {
            CacheExecutor(player);
            List<IEquipment> equipments = runtime_executor_equipments[player];
            equipments.Remove(equipment);
            equipment.OnUnEquip();
            equipment.EquipExecutor = null;
            equipment.EquipmentData = null;
            equipment.GlobalRelease();
        }

        public static void UnEquipAll(this IEquipExecutor player)
        {
            CacheExecutor(player);
            List<IEquipment> equipments = runtime_executor_equipments[player];

            foreach (var item in equipments)
            {
                item.OnUnEquip();
                item.EquipExecutor = null;
                item.EquipmentData = null;
                item.GlobalRelease();
            }

            runtime_executor_equipments[player].Clear();

        }
    }

}
