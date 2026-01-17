
#if UNITY_EDITOR
#endif
using YukiFrameWork.Pools;
using YukiFrameWork.Extension;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
///=====================================================
/// - FileName:      Mission.cs
/// - NameSpace:     YukiFrameWork.Missions
/// - Description:   高级定制脚本生成
/// - Creation Time: 1/12/2026 7:25:23 PM
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
namespace YukiFrameWork.Missions
{
    public abstract class MissionController : IController, IGlobalSign, IDisposable
    {
        internal static MissionController CreateInstance(Mission mission, MissionTree missionTree)
        {
            Type type = AssemblyHelper.GetType(mission.MissionControllerType);
            if (type == null)
                throw new Exception("任务控制类型丢失!");

            if (!type.IsSubclassOf(typeof(MissionController)))
                throw new InvalidCastException("类型转换失败，不是继承MissionController的类型! Type:" + type);

            MissionController controller = GlobalObjectPools.GlobalAllocation(type) as MissionController;
            controller.Mission = mission;
            controller.MissionTree = missionTree;
            controller.MissionAwards = mission.MissionAwardParams.ToDictionary(x => x.paramKey, x => x);
            controller.MissionTargets = mission.MissionTargetParams.ToDictionary(x => x.paramKey, x => x);            
            return controller;
        }

        private List<MissionController> missionChildControllers = new List<MissionController>();
        private List<MissionController> missionParentControllers = new List<MissionController>();
        /// <summary>
        /// 任务数据
        /// </summary>
        public Mission Mission { get; private set; }

        /// <summary>
        /// 任务的父节点
        /// </summary>
        public IReadOnlyList<Mission> Parents => Mission.Parents;

        /// <summary>
        /// 任务的子节点
        /// </summary>
        public IReadOnlyList<Mission> Childs => Mission.Childs;

        /// <summary>
        /// 任务树保存的任务so配置
        /// </summary>
        public MissionTreeSO MissionTreeSO => MissionTree.MissionTreeSO;

        /// <summary>
        /// 这个任务所处的的任务树
        /// </summary>
        public MissionTree MissionTree { get; private set; }

        /// <summary>
        /// 这个任务是否是子任务
        /// </summary>
        public bool IsChild => Mission.IsChild;

        /// <summary>
        /// 任务的所有奖励参数
        /// </summary>
        public Dictionary<string, MissionParam> MissionAwards { get; private set; }

        /// <summary>
        /// 任务的所有目标的参数
        /// </summary>
        public Dictionary<string, MissionParam> MissionTargets { get; private set; }

        /// <summary>
        /// 访问所有的子任务
        /// </summary>
        public IReadOnlyList<MissionController> ChildControllers => missionChildControllers;

        /// <summary>
        /// 查找自身父节点的所有控制器
        /// </summary>
        public IReadOnlyList<MissionController> ParentControllers => missionParentControllers;        

        /// <summary>
        /// 任务的状态
        /// </summary>
        public MissionStatus MissionStatus => Mission.MissionStatus;

        /// <summary>
        /// 任务处于锁定状态时触发
        /// </summary>
        public event Action<Mission> onLock;
        /// <summary>
        /// 当任务重置/进入待机触发
        /// </summary>
        public event Action<Mission> onReset;
        /// <summary>
        /// 当任务完成触发
        /// </summary>
        public event Action<Mission> onCompleted;
        /// <summary>
        /// 当任务失败触发
        /// </summary>
        public event Action<Mission> onFailed;
        /// <summary>
        /// 当任务启动触发
        /// </summary>
        public event Action<Mission, object[]> onStart;

        /// <summary>
        /// 判断任务是否是自身的父节点
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public bool ParentContains(MissionController current)
        {
            for (int i = 0; i < Parents.Count; i++)
            {
                if (Parents[i] == current.Mission)
                    return true;
            }

            return false;
        }     
        /// <summary>
        /// 遍历所有的子任务的控制器
        /// </summary>
        /// <param name="each"></param>
        public void ForEachChildrens(Action<MissionController> each)
        {
            ForEachChildrens(each, () => true);
        }

        /// <summary>
        /// 遍历所有的子任务的控制器
        /// </summary>
        /// <param name="each"></param>
        /// <param name="condition"></param>
        public void ForEachChildrens(Action<MissionController> each, Func<bool> condition)
        {
            foreach (var item in missionChildControllers)
            {
                if(condition())
                    each?.Invoke(item);
            }
        }

        /// <summary>
        /// 遍历所有的子任务的控制器
        /// </summary>
        /// <param name="each"></param>
        public void ForEachParents(Action<MissionController> each)
        {
            ForEachParents(each, () => true);
        }

        /// <summary>
        /// 遍历所有的子任务的控制器
        /// </summary>
        /// <param name="each"></param>
        /// <param name="condition"></param>
        public void ForEachParents(Action<MissionController> each, Func<bool> condition)
        {
            foreach (var item in missionParentControllers)
            {
                if (condition())
                    each?.Invoke(item);
            }
        }

        /// <summary>
        /// 根据标识查找到指定子任务控制器
        /// </summary>
        /// <param name="missionId"></param>
        /// <returns></returns>
        public MissionController FindChildMissionController(int missionId)
        {
            foreach (var item in missionChildControllers)
            {
                if (item.Mission.MissionId == missionId)
                    return item;
            }

            throw new Exception("指定的控制器不属于自身的子控制 MissionId:" + missionId);
        }

        /// <summary>
        /// 根据标识查找到指定子任务控制器，不会抛出异常
        /// </summary>
        /// <param name="missionId"></param>
        /// <param name="missionController"></param>
        /// <returns></returns>
        public bool TryFindChildMissionController(int missionId, out MissionController missionController)
        {
            missionController = null;
            try
            {
                missionController = FindChildMissionController(missionId);

                return true;
            }
            catch 
            {
                return false;
            }
        }

        /// <summary>
        /// 根据标识查找到指定父任务控制器
        /// </summary>
        /// <param name="missionId"></param>
        /// <returns></returns>
        public MissionController FindParentMissionController(int missionId)
        {
            foreach (var item in missionParentControllers)
            {
                if (item.Mission.MissionId == missionId)
                    return item;
            }

            return null;
        }


        /// <summary>
        /// 根据标识查找到指定父任务控制器，不会抛出异常
        /// </summary>
        /// <param name="missionId"></param>
        /// <param name="missionController"></param>
        /// <returns></returns>
        public bool TryFindParentMissionController(int missionId, out MissionController missionController)
        {
            missionController = null;
            try
            {
                missionController = FindParentMissionController(missionId);

                return true;
            }
            catch
            {
                return false;
            }
        }

        #region internal Method
        internal void ChangeStatus(MissionStatus missionStatus, params object[] param)
        {
            Mission.MissionStatus = missionStatus;
            switch (missionStatus)
            {
                case MissionStatus.Lock:
                    OnLock();
                    onLock?.Invoke(Mission);
                    //当根任务被锁定，则需要让所有的子任务也被锁定
                    //foreach (var item in missionChildControllers)                   
                    //    item.ChangeStatus(MissionStatus.Lock);                  

                    break;
                case MissionStatus.InActive:
                    OnReset();
                    onReset?.Invoke(Mission);
                    break;
                case MissionStatus.Running:
                    OnStart(param);
                    onStart?.Invoke(Mission, param);
                    break;
                case MissionStatus.Failed:
                    OnMissionFailed();
                    onFailed?.Invoke(Mission);
                    break;
                case MissionStatus.Success:
                    OnMissionCompleted();
                    onCompleted?.Invoke(Mission);
                    break;
                default:
                    break;
            }
        }

        internal void UpdateAllChildMissionController()
        {
            foreach (var item in Mission.Childs)
                missionChildControllers.Add(MissionTree.FindMissionController(item.MissionId));

            foreach (var item in Mission.Parents)
                missionParentControllers.Add(MissionTree.FindMissionController(item.MissionId));
        }

        internal void Update()
        {
            OnUpdate();

            if (IsCompleted())
            {
                ChangeStatus(MissionStatus.Success);
            }
            else if (IsFailed())
            {
                ChangeStatus(MissionStatus.Failed);
            }

            //foreach (var item in missionChildControllers)
            //{
            //    if (item.MissionStatus == MissionStatus.Running)
            //        item.Update();            
            //}
        }

        internal void FixedUpdate()
        {
            OnFixedUpdate();

            //foreach (var item in missionChildControllers)
            //{
            //    if (item.MissionStatus == MissionStatus.Running)
            //        item.FixedUpdate();
            //}

        }

        internal void LateUpdate() 
        {
            OnLateUpdate();

            //foreach (var item in missionChildControllers)
            //{
            //    if (item.MissionStatus == MissionStatus.Running)
            //        item.LateUpdate();
            //}

        }

        internal void Create(params object[] param)
            => OnCreate(param);

        internal bool MissionIsUnLock() => IsUnLock();

        #endregion
        /// <summary>
        /// 当任务树被创建后触发
        /// </summary>
        /// <param name="param"></param>
        protected abstract void OnCreate(params object[] param);

        /// <summary>
        /// 当任务被锁定时触发,初始创建任务控制时，调用时机在OnCreate之后
        /// </summary>
        protected virtual void OnLock()
        { }

        /// <summary>
        /// 当任务启动时触发
        /// </summary>
        /// <param name="param"></param>
        protected abstract void OnStart(params object[] param);

        /// <summary>
        /// 当任务完成时触发
        /// </summary>
        protected abstract void OnMissionCompleted();

        /// <summary>
        /// 判断任务完成的条件
        /// </summary>
        /// <returns></returns>
        protected abstract bool IsCompleted();

        /// <summary>
        /// 判断任务失败的条件
        /// </summary>
        /// <returns></returns>
        protected abstract bool IsFailed();

        /// <summary>
        /// 是否能够解锁该任务(默认是True)
        /// </summary>
        /// <returns></returns>
        protected virtual bool IsUnLock() { return true; }

        /// <summary>
        /// 当任务失败时触发
        /// </summary>
        protected abstract void OnMissionFailed();

        /// <summary>
        /// 当任务执行时更新
        /// </summary>
        protected virtual void OnUpdate() { }

        /// <summary>
        /// 当任务执行时晚于更新
        /// </summary>
        protected virtual void OnLateUpdate() { }

        /// <summary>
        /// 当任务执行时间接更新
        /// </summary>
        protected virtual void OnFixedUpdate() { }

        /// <summary>
        /// 当任务进入/重置待机状态触发
        /// </summary>
        protected virtual void OnReset() { }


        #region Architecture
        private object _object = new object();
        private IArchitecture mArchitecture;

        /// <summary>
        /// 可重写的架构属性,不使用特性初始化时需要重写该属性
        /// </summary>
        protected virtual IArchitecture RuntimeArchitecture
        {
            get
            {
                lock (_object)
                {
                    if (mArchitecture == null)
                        Build();
                    return mArchitecture;
                }
            }
        }

        public bool IsMarkIdle { get ; set ; }

        IArchitecture IGetArchitecture.GetArchitecture()
        {
            return RuntimeArchitecture;
        }

        internal void Build()
        {
            if (mArchitecture == null)
            {
                mArchitecture = ArchitectureConstructor.I.Enquene(this);
            }
        }

        #endregion

        void IGlobalSign.Init()
        {
            
        }

        void IGlobalSign.Release()
        {
            Dispose();
            MissionAwards?.Clear();
            MissionTargets?.Clear();
            Mission = null;
            MissionTree = null;
            missionChildControllers.Clear();
            missionParentControllers.Clear();
            onReset = null;
            onStart = null;
            onFailed = null;
            onCompleted = null;
            onLock = null;
        }

        public virtual void Dispose()
        {
            
        }
    }

}
