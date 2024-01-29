using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork.XFABManager
{
    internal class ImageLoaderTip : LoaderTips
    {
        public override bool IsThrowException => false;
        public override string tips => "XFABManager警告:推荐直接在 Image SpriteRender RawImage 等组件身上挂载 ImageLoader 来实现图片的加载并显示,该组件有引用计数功能,可自动管理资源的加载和释放,推荐使用!";
        public override Type[] types => new Type[] { typeof(Sprite), typeof(Texture), typeof(Texture2D) };
    } 
}

