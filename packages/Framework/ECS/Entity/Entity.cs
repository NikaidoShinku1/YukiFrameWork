
namespace YukiFrameWork.ECS
{
    /// <summary>
    /// 对象实体
    /// </summary>
    public class Entity
    {       
        public EntityData EntityData { get; private set; }
        public int ID { get; }
        public Entity(int ID)
        {           
            this.ID = ID;
            EntityData = new EntityData();
        }

        public virtual T GetComponent<T>()
        {
            return EntityData.GetComponent<T>();
        }

        public virtual T AddComponent<T>(T component,params object[] args)
        {
            return EntityData.AddComponent<T>(component,args);   
        }

        public virtual void RemoveComponent<T>() 
        {
            EntityData.RemoveComponent<T>();
        }

        public virtual void Clear()
        {
            EntityData.Clear();
        }
    }

}
