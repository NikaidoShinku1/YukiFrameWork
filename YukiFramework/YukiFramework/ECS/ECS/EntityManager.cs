using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace YukiFrameWork.ECS
{
    public interface IEntityManager
    {
        void Init();
        List<Entity> GetEntities();
        List<SystemData> GetSysytemies();
        SystemData CreateSystem<T>() where T : SystemData;
        Entity CreateEntity();
        void AddComponent(Entity entity,params IComponentData[] args);
        void Update();
        void FixedUpdate();
        void Clear();
    }
    /// <summary>
    /// 实体管理器
    /// </summary>
    public class EntityManager : IEntityManager
    {
        private EntityModel model = new EntityModel();

        public void Init()
        {
            foreach (var system in GetSysytemies())
            {
                system.Init();
            }
        }

        /// <summary>
        /// 得到管理器管理的所有实体
        /// </summary>
        /// <returns>返回一个实体</returns>
        public List<Entity> GetEntities()
        {
            return model.GetEntities();
        }

        /// <summary>
        /// 获取所有的系统
        /// </summary>
        /// <returns>返回系统列表</returns>
        public List<SystemData> GetSysytemies() => model.GetSysytemies();        

        /// <summary>
        /// 创建系统
        /// </summary>
        /// <typeparam name="T">系统类型</typeparam>
        /// <returns>返回一个系统</returns>
        public SystemData CreateSystem<T>() where T : SystemData
        {
            T data = (T)CreateSystem(typeof(T));
            return data;
        }

        /// <summary>
        /// 创建系统
        /// </summary>
        /// <typeparam name="T">系统类型</typeparam>
        /// <returns>返回一个系统</returns>
        public SystemData CreateSystem(Type type)
        {
            if (type.BaseType != typeof(SystemData)) return null;
            var data = (SystemData)Activator.CreateInstance(type, this);
            model.AddSystem(data);
            return data;
        }

        /// <summary>
        /// 创建一个实体
        /// </summary>
        /// <returns>返回一个实体</returns>
        public Entity CreateEntity()
        {
            Entity entity = model.CreateEntity();           
            return entity;
        }

        /// <summary>
        /// 添加组件
        /// </summary>
        /// <param name="entity">实体</param>
        /// <param name="args">组件集合</param>
        public void AddComponent(Entity entity,params IComponentData[] args)
        {
            if (args != null)
            {
                var entited = model.GetEntities();
                var targetEntity = entited.Find(x => x.ID == entity.ID);
                if (targetEntity != null) 
                {
                    targetEntity.AddComponent(args);
                }
            }
        }

        /// <summary>
        /// 系统update更新
        /// </summary>
        public void Update()
        {
            model.SystemUpdate();
        }

        /// <summary>
        /// 系统fixedUpdate更新
        /// </summary>
        public void FixedUpdate()
        {
            model.SystemFixedUpdate();
        }

        /// <summary>
        /// 释放
        /// </summary>
        public void Clear()
        {
            foreach (var system in GetSysytemies())
            {
                system.Clear();
            }
            model = null;
        }
    }
}
