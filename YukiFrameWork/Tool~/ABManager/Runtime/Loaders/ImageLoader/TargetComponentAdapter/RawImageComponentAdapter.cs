using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace YukiFrameWork.ABManager
{
    public class RawImageComponentAdapter : TargetComponentAdapter
    {
        public TargetComponentType TargetComponentType => TargetComponentType.RawImage;

        public void SetColor(ImageLoader loader, Color color)
        {
            if (loader == null) return;

            if (loader.TargetComponent == null)
                loader.TargetComponent = TargetComponent(loader);

            Object component = loader.TargetComponent;
            if (component == null)
            {
                Debug.LogErrorFormat("未在游戏物体:{0} 身上查到组件:RawImage 请确认组件是否存在!", loader.gameObject.name);
                return;
            }

            RawImage image = component as RawImage;
            if (image == null) return;
            image.color = color;
        }

        public void SetValue(ImageLoader loader, ImageData imageData)
        {
            if (loader == null) return;

            if (loader.TargetComponent == null)
                loader.TargetComponent = TargetComponent(loader);

            Object component = loader.TargetComponent;
            if (component == null)
            { 
                Debug.LogErrorFormat("未在游戏物体:{0} 身上查到组件:RawImage 请确认组件是否存在!", loader.gameObject.name);
                return;
            }

            RawImage image = component as RawImage;
            if (image == null) return;
            image.texture = imageData != null ? imageData.texture : null;
            if (loader.auto_set_native_size)
                image.SetNativeSize();
        }

        public Object TargetComponent(ImageLoader loader)
        {
            if (loader == null) return null;
            return loader.GetComponent<RawImage>();
        }
    }
}


