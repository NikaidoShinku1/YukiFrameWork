using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XFABManager;

namespace XFABManager
{
    public interface TargetComponentAdapter
    { 
        TargetComponentType TargetComponentType { get; }
        
        Object TargetComponent(ImageLoader loader);

        void SetValue(ImageLoader loader, ImageData imageData);

        void SetColor(ImageLoader loader, Color color);

    }
}


