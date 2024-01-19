using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork.ABManager
{
    public interface TargetComponentAdapter
    { 
        TargetComponentType TargetComponentType { get; }
        
        Object TargetComponent(ImageLoader loader);

        void SetValue(ImageLoader loader, ImageData imageData);

        void SetColor(ImageLoader loader, Color color);

    }
}


