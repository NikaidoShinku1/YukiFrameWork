using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
namespace YukiFrameWork.States
{
    public class ColorConst
    {
        public static Color GridColor { get; } = new Color(0, 0, 0, 0.2f);

        public static Color BackGroundColor { get; } = new Color(0, 0, 0, 0.25f);

        public static Color SelectColor { get; } = new Color(0, 255, 40, 255) / 255;

        public static Color TransitionColor { get; } = new Color(0, 0.78f, 1, 1);

        public static Color ParamBackGround { get; } = new Color(48, 48, 48, 255) / 255.0f;
    }
}
#endif