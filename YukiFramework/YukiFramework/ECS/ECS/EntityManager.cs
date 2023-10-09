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
    /// ʵ�������
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
        /// �õ����������������ʵ��
        /// </summary>
        /// <returns>����һ��ʵ��</returns>
        public List<Entity> GetEntities()
        {
            return model.GetEntities();
        }

        /// <summary>
        /// ��ȡ���е�ϵͳ
        /// </summary>
        /// <returns>����ϵͳ�б�</returns>
        public List<SystemData> GetSysytemies() => model.GetSysytemies();        

        /// <summary>
        /// ����ϵͳ
        /// </summary>
        /// <typeparam name="T">ϵͳ����</typeparam>
        /// <returns>����һ��ϵͳ</returns>
        public SystemData CreateSystem<T>() where T : SystemData
        {
            T data = (T)CreateSystem(typeof(T));
            return data;
        }

        /// <summary>
        /// ����ϵͳ
        /// </summary>
        /// <typeparam name="T">ϵͳ����</typeparam>
        /// <returns>����һ��ϵͳ</returns>
        public SystemData CreateSystem(Type type)
        {
            if (type.BaseType != typeof(SystemData)) return null;
            var data = (SystemData)Activator.CreateInstance(type, this);
            model.AddSystem(data);
            return data;
        }

        /// <summary>
        /// ����һ��ʵ��
        /// </summary>
        /// <returns>����һ��ʵ��</returns>
        public Entity CreateEntity()
        {
            Entity entity = model.CreateEntity();           
            return entity;
        }

        /// <summary>
        /// ������
        /// </summary>
        /// <param name="entity">ʵ��</param>
        /// <param name="args">�������</param>
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
        /// ϵͳupdate����
        /// </summary>
        public void Update()
        {
            model.SystemUpdate();
        }

        /// <summary>
        /// ϵͳfixedUpdate����
        /// </summary>
        public void FixedUpdate()
        {
            model.SystemFixedUpdate();
        }

        /// <summary>
        /// �ͷ�
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
