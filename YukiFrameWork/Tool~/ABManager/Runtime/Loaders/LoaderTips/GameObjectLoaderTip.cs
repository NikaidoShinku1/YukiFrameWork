using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XFABManager
{
    internal class GameObjectLoaderTip : LoaderTips
    {

        public override bool IsThrowException => false;
        public override string tips => "XFABManager警告:推荐使用 GameObjectLoader.Load() 来加载并实例游戏物体, 该方法具有引用计数功能,可以自动管理资源加载和释放,推荐使用!(UIKit加载可以对该警告忽略!)";
        public override Type[] types => new Type[] { typeof(GameObject) };

    }
}


