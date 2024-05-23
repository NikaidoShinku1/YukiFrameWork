ViewController编辑器使用说明

使用示例：
```

    ///首先创建一个架构类
    using YukiFrameWork;
    public class PointGame : Architecture<PointGame>
    {
        ///需要重写的初始化方法
        public override void OnInit()
        {
            
        }        
    }
```
创建完架构类后在ViewController编辑器下开启架构自动化设置如图所示:

![输入图片说明](Texture/controllerEditor.png)

开启后必须设置一个架构(不为None才可以创建脚本),创建后的脚本内容所示:

```

using YukiFrameWork;
using UnityEngine;
using System;

namespace YukiFrameWork.Example
{
    ///对架构的自动化注入，设置类型为架构类型后该控制器即可实现层级逻辑的编写，第二个参数默认是true，表示可以访问到架构，如果设置为False该控制器无法得到架构本体
    [RuntimeInitializeOnArchitecture(typeof(PointGame),true)]
    public class ViewControllerExample : ViewController
    {
        //对于派生自ViewController的字段/包括SingletonMono在内，可使用快速组件赋值的特性示例如下:

        [VFindObjectOfType]
        public Camera mCamera;

        [VGetComponent]
        public Transform mTransform;

        [VAddComponent]
        public Rigidbody rd;

        [VGetOrAddComponent]
        public UnityEngine.Image image;
    }
}
    

```

默认开启,如果选择关闭的话则创建默认的ViewController脚本。