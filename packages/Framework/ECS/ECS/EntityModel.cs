using System.Collections.Generic;
namespace YukiFrameWork.ECS
{
    public class EntityModel
    {
        private readonly List<Entity> entities = new List<Entity>();
        private readonly List<SystemData> systemDatas = new List<SystemData>();

        public Entity CreateEntity()
        {
            Entity entity = new Entity(entities.Count);
            entities.Add(entity);
            return entity;
        }

        public void AddSystem(SystemData data)
        {
            systemDatas.Add(data);
        }

        public void InitSystem()
        {
            foreach (var system in systemDatas)
            {
                system.Init();
            }
        }

        public void SystemUpdate()
        {            
            foreach (var system in systemDatas)
            {
                system.OnUpdate();
            }
        }

        public void SystemFixedUpdate()
        {
            foreach (var system in systemDatas)
            {
                system.OnFixedUpdate();
            }
        }

        public List<Entity> GetEntities()
        {
            return entities;
        }       

        public List<SystemData> GetSysytemies()
        {
            return systemDatas;
        }

        public void Clear()
        {
            systemDatas.Clear();
            entities.Clear();
        }
    }
}