
using System.Collections.Generic;

namespace YukiFrameWork.ECS
{
    /// <summary>
    /// 系统数据
    /// </summary>
    public abstract class SystemData
    {
        private EntityManager Manager;
        public List<Entity> Entities
        {
            get => Manager.GetEntities();
        }
        public abstract void Init();
        public virtual void OnUpdate() { }
        public virtual void OnFixedUpdate() { }
        public virtual void OnDestroy() { }

        public SystemData(EntityManager Manager)
        {
            this.Manager = Manager;
        }

        public virtual void Clear()
        {
            Manager = null;
        }
    }
}
