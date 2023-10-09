
namespace YukiFrameWork.ECS
{
    /// <summary>
    /// 系统数据
    /// </summary>
    public abstract class SystemData
    {
        public EntityManager Manager { get; private set; }
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
