AbstractController使用说明

| Attribute      | 说明 |
| ----------- | ----------- |
| InitController | 仅为AbstractController控制器提供的初始化特性,为控制器标记该特性后在准备完架构后会自动调用里面的OnInit方法|
| order| 注册的层级，默认是0，在严格按照先注册Model再注册System的情况下，该层级的设置，越高则越先注册 |

不继承MonoBehaviour的可自动化控制器。
使用示例：
``` csharp

    ///首先是一个架构类
    using YukiFrameWork;
    public class World : Architecture<World>
    {
        ///需要重写的初始化方法
        public override void OnInit()
        {
            
        }        
    }
```


``` csharp

using YukiFrameWork;
using UnityEngine;
using System;

namespace YukiFrameWork.Example
{
    ///对架构的自动化注入，设置类型为架构类型后该控制器即可实现层级逻辑的编写，第二个参数默认是true，表示可以访问到架构，如果设置为False该控制器无法得到架构本体
    [RuntimeInitializeOnArchitecture(typeof(World),true)]
    [InitController(order = 1000)]//架构准备完成后对该控制器进行初始化
    public class AbstractControllerExample : AbstractController
    {
        public override void OnInit()
        {
            //该控制器特殊的初始化方法，仅AbstractController持有。
        }
    }
}
    

```
