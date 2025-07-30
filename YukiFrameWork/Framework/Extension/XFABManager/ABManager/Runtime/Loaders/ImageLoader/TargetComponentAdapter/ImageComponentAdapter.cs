using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace XFABManager
{
    public class ImageComponentAdapter : TargetComponentAdapter
    {
        public TargetComponentType TargetComponentType => TargetComponentType.Image;

        public Color GetColor(ImageLoader loader)
        {
            if (loader == null) return Color.white;

            if (loader.TargetComponent == null)
                loader.TargetComponent = TargetComponent(loader);

            Object component = loader.TargetComponent;
            if (component == null)
            {
                Debug.LogErrorFormat("未在游戏物体:{0} 身上查到组件:Image 请确认组件是否存在!", loader.gameObject.name);
                return Color.white;
            }

            Image image = component as Image;
            if (image == null) return Color.white;
            return image.color;
        }

        public void SetColor(ImageLoader loader, Color color)
        {
            if (loader == null) return;

            if (loader.TargetComponent == null)
                loader.TargetComponent = TargetComponent(loader);

            Object component = loader.TargetComponent;
            if (component == null)
            {
                Debug.LogErrorFormat("未在游戏物体:{0} 身上查到组件:Image 请确认组件是否存在!", loader.gameObject.name);
                return;
            }

            Image image = component as Image;
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
                Debug.LogErrorFormat("未在游戏物体:{0} 身上查到组件:Image 请确认组件是否存在!",loader.gameObject.name);
                return; 
            }
            
            Image image = component as Image;
            if (image == null) return;

#if XFABMANAGER_LOG_OPEN_TESTING
            Debug.LogFormat("ImageComponentSetValue:{0} value:{1}",image, imageData != null ? imageData.sprite : null);
#endif

            image.sprite = imageData != null ? imageData.sprite : null;
            if (loader.auto_set_native_size)
                image.SetNativeSize();
        }

        public Object TargetComponent(ImageLoader loader)
        {
            if (loader == null) return null; 
            return loader.GetComponent<Image>();
        }
    }
}


