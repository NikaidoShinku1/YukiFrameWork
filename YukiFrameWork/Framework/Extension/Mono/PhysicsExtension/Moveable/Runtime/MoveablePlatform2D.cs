 
using UnityEngine; 

namespace YukiFrameWork.Platform2D
{ 
    [RequireComponent(typeof(PlatformCatcher2D))]
    public class MoveablePlatform2D : Moveable
    { 
        private PlatformCatcher2D catcher;


        protected override void Awake()
        {
            base.Awake(); 
            catcher = GetComponent<PlatformCatcher2D>();
        }


        protected override void Move(Vector3 target)
        {   
            Vector2 detal = target - transform.position;
            // 移动位置
            transform.position = target;

            // 带着游戏角色一起移动 
            foreach (var item in catcher.Contacts)
            {
                if (item.rigidbody == null) continue;
                item.rigidbody.position += detal; 
            } 
        }


       
    }

}

