using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


namespace XFABManager
{
    public class SpriteRendererComponentAdapter : TargetComponentAdapter
    {
        public TargetComponentType TargetComponentType => TargetComponentType.SpriteRenderer;

        public void SetColor(ImageLoader loader, Color color)
        {
            if (loader == null) return;

            if (loader.TargetComponent == null)
                loader.TargetComponent = TargetComponent(loader);

            Object component = loader.TargetComponent;
            if (component == null)
            {
                Debug.LogErrorFormat("未在游戏物体:{0} 身上查到组件:SpriteRenderer 请确认组件是否存在!", loader.gameObject.name);
                return;
            }

            SpriteRenderer image = component as SpriteRenderer;
            if (image == null) return;
            image.color = color;
        }
        public Color GetColor(ImageLoader loader)
        {
            if (loader == null) return Color.white;

            if (loader.TargetComponent == null)
                loader.TargetComponent = TargetComponent(loader);

            Object component = loader.TargetComponent;
            if (component == null)
            {
                Debug.LogErrorFormat("未在游戏物体:{0} 身上查到组件:SpriteRenderer 请确认组件是否存在!", loader.gameObject.name);
                return Color.white;
            }

            SpriteRenderer image = component as SpriteRenderer;
            if (image == null) return Color.white;
            return image.color;
        }

        public void SetValue(ImageLoader loader, ImageData imageData)
        {
            if (loader == null) return;

            if (loader.TargetComponent == null)
                loader.TargetComponent = TargetComponent(loader);

            Object component = loader.TargetComponent;
            if (component == null)
            { 
                Debug.LogErrorFormat("未在游戏物体:{0} 身上查到组件:SpriteRenderer 请确认组件是否存在!", loader.gameObject.name);
                return;
            }

            SpriteRenderer image = component as SpriteRenderer;
            if (image == null) return;
            image.sprite = imageData != null ? imageData.sprite : null;
        }

        public Object TargetComponent(ImageLoader loader)
        {
            if (loader == null) return null;
            return loader.GetComponent<SpriteRenderer>();
        }
    }

}

